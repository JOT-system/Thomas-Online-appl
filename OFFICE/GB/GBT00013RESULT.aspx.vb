Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' B/L 検索結果画面クラス
''' </summary>
Public Class GBT00013RESULT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00013R" '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile

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
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = Me.hdnMapVariant.Value
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0031ProfMap.ERR)})
                    Return
                End If
                ListScrole()
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '右ボックス帳票タブ
                Dim errMsg As String = ""
                errMsg = Me.RightboxInit()
            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                'DO SOMETHING!
                Me.lblFooterMessage.Text = ""
                '**********************
                ' ボタンクリック判定
                '**********************
                'hdnButtonClickに文字列が設定されていたら実行する
                If Me.hdnButtonClick IsNot Nothing AndAlso Me.hdnButtonClick.Value <> "" Then
                    'ボタンID + "_Click"というイベントを実行する。(この規則性ではない場合、個別の分岐をしてください)
                    Dim btnEventName As String = ""
                    If Me.hdnButtonClick.Value.StartsWith("lbl" & Me.WF_LISTAREA.ID & "SHOWTANK") Then
                        btnEventName = "lblListShowTank_Click"
                    ElseIf Me.hdnButtonClick.Value.StartsWith("btn" & Me.WF_LISTAREA.ID & "DISTRIBUTION") Then
                        btnEventName = "btnListDistribution_Click"
                    ElseIf Me.hdnButtonClick.Value.StartsWith("btn" & Me.WF_LISTAREA.ID & "EDIT") Then
                        btnEventName = "btnListEdit_Click"

                    ElseIf Me.hdnButtonClick.Value.StartsWith("btn" & Me.WF_LISTAREA.ID & "ISSUE") Then
                        btnEventName = "btnListIssue_Click"
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
                Case Else
            End Select
        End If

    End Sub
    ''' <summary>
    ''' 保存ボタン押下時処理
    ''' </summary>
    Public Sub btnSave_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl
        Dim messageNo As String = ""
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

        Dim targetDr As List(Of DataRow) = GetModifiedDataTable(dt)
        '保存対象データが0件の場合
        If targetDr Is Nothing OrElse targetDr.Count = 0 Then
            messageNo = C_MESSAGENO.NOENTRYDATA
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage)
            Return
        End If
        'オーダー単位のブレーカー予定費用、オーダー予定費用の差額ディクショナリを作成
        Dim orderKeyList = (From item In targetDr Group By grpOrderNo = Convert.ToString(item("ORDERNO")), grpTransNo = Convert.ToString(item("TRANSNO")) Into grp = Group Select grpOrderNo, grpTransNo).ToList

        '差額保持用のディクショナリ(キー:(オーダーNo,タンクSEQ,第n輸送)　値:差額)
        'Dim dicDifferenceAmount As New Dictionary(Of Tuple(Of String, String, String), Hashtable)
        Dim differenceAmountList As New List(Of DifferenceAmount)
        For Each orderKey In orderKeyList
            '先頭のTankSeq取得
            Dim tankValues = (From item In dt Where item("ORDERNO").Equals(orderKey.grpOrderNo) AndAlso
                                                    item("TRANSNO").Equals(orderKey.grpTransNo) AndAlso
                                                  (Not item("BASEVALUEFLG").Equals("1"))
                              Order By item("TANKSEQ")).FirstOrDefault

            Dim tankSeqList = (From item In dt Where item("ORDERNO").Equals(orderKey.grpOrderNo) AndAlso
                                                                            item("TRANSNO").Equals(orderKey.grpTransNo) AndAlso
                                                                           (Not item("TANKSEQ").Equals(tankValues.Item("TANKSEQ"))) AndAlso
                                                                           (Not item("BASEVALUEFLG").Equals("1"))
                               Order By item("TANKSEQ")
                               Group By grpTankSeq = Convert.ToString(item("TANKSEQ")) Into maxTankNo = Max(Convert.ToString(item("TANKNO")))
                               Select tankSeq = grpTankSeq, tankNo = maxTankNo).ToList

            For Each tankseqHireageUpd In tankSeqList
                '各費用情報より差額合計を保持
                Dim calcResult = (From item In dt Where item("ORDERNO").Equals(orderKey.grpOrderNo) AndAlso
                                                        item("TRANSNO").Equals(orderKey.grpTransNo) AndAlso
                                                        item("TANKSEQ").Equals(tankseqHireageUpd.tankSeq) AndAlso
                                                       (Not item("BASEVALUEFLG").Equals("1"))
                                  Select Decimal.Parse(Convert.ToString(item("AMOUNTBR"))) - Decimal.Parse(Convert.ToString(item("AMOUNTORD")))).Sum()

                'dicValue.Add("TANKSEQ", tankseqHireageUpd)
                'dicValue.Add("TANKNO", tankNo)
                differenceAmountList.Add(New DifferenceAmount With {.OrderNo = orderKey.grpOrderNo,
                                                  .TankSeq = tankseqHireageUpd.tankSeq,
                                                  .TransNo = orderKey.grpTransNo,
                                                  .TankNo = tankseqHireageUpd.tankNo,
                                                  .Amount = calcResult})
                'dicDifferenceAmount.Add(Tuple.Create(orderKey.grpOrderNo, tankseqHireageUpd, orderKey.grpTransNo), dicValue)

            Next
        Next
        ''TOTAL INVOICEの変更情報を収集作成(オーダー番号,(タンクSEQ,TOTAL Invoiceに加算する数値))
        'Dim dicDifferenceTotalInvoice As New Dictionary(Of String, Dictionary(Of String, Decimal))
        'Dim orderTotalInvoiceKeyList = (From item In targetDr Group By grpOrderNo = Convert.ToString(item("ORDERNO")) Into grp = Group Select grpOrderNo).ToList
        'For Each orderNo In orderTotalInvoiceKeyList
        '    Dim difference = (From item In dt Where item("ORDERNO").Equals(orderNo) AndAlso
        '                      (Not item("BASEVALUEFLG").Equals("1"))
        '                      Select New With {.trans1Diff = If(item("TRANSNO").Equals("1"), Decimal.Parse(Convert.ToString(item("AMOUNTBR"))) - Decimal.Parse(Convert.ToString(item("AMOUNTORD"))), 0),
        '                                       .trans2Diff = If(item("TRANSNO").Equals("2"), Decimal.Parse(Convert.ToString(item("AMOUNTBR"))) - Decimal.Parse(Convert.ToString(item("AMOUNTORD"))), 0)}).ToList

        '    Dim trans1Diff = difference.Sum(Function(item) item.trans1Diff)
        '    Dim trans2Diff = difference.Sum(Function(item) item.trans2Diff)

        '    Dim orderList = (From item In dt Where item("ORDERNO").Equals(orderNo) AndAlso
        '                              (Not item("BASEVALUEFLG").Equals("1"))
        '                     Group By tankseq = Convert.ToString(item("TANKSEQ")) Into Group
        '                     Order By tankseq
        '                     Select New With {tankseq,
        '                                         .amountBr = Group.Sum(Function(it) Decimal.Parse(Convert.ToString(it.Item("AMOUNTBR")))),
        '                                         .amountOrd = Group.Sum(Function(it) Decimal.Parse(Convert.ToString(it.Item("AMOUNTORD")))),
        '                                         .amountAdd = If(.amountOrd = 0, .amountBr * -1, 0)
        '                               })
        '    Dim firstRecord = orderList.FirstOrDefault
        '    Dim otherAmount As Decimal = 0
        '    If Not (trans1Diff = 0 AndAlso trans2Diff = 0) Then
        '        otherAmount = trans1Diff + trans2Diff
        '    End If
        '    '先頭レコード
        '    Dim dicTankSeqAmount As New Dictionary(Of String, Decimal)
        '    dicTankSeqAmount.Add(firstRecord.tankseq, otherAmount)
        '    Dim otherRecord = (From orderListItem In orderList Where orderListItem.tankseq <> firstRecord.tankseq)

        '    'その他のタンクSEQ
        '    If otherRecord.Any = True Then
        '        For Each item In otherRecord
        '            dicTankSeqAmount.Add(item.tankseq, item.amountAdd)
        '        Next
        '    End If
        '    dicDifferenceTotalInvoice.Add(orderNo, dicTankSeqAmount)
        'Next

        '更新処理実行
        Dim rightMessage As String = ""
        messageNo = Me.UpdateOrderValue(targetDr, differenceAmountList, rightMessage)
        Dim naeiw = C_NAEIW.NORMAL
        If Not {"", C_MESSAGENO.NORMALDBENTRY, C_MESSAGENO.NORMAL}.Contains(messageNo) Then
            naeiw = C_NAEIW.ABNORMAL
            Me.txtRightErrorMessage.Text = rightMessage
        End If

        '更新後の全レコードを取得   
        Dim afterUpdateDbDt As DataTable = GetOrderListDataTable()
        Dim showTankOrderItem = (From item In dt Where item("SHOWHIDE").Equals("SHOW") Select showOrderNo = Convert.ToString(item("ORDERNO")), showTransNo = Convert.ToString(item("TRANSNO"))).FirstOrDefault
        Dim showTankOrderNo As String = "" 'showTankOrderItem.showOrderNo
        Dim showTankTransNo As String = "" 'showTankOrderItem.showTransNo
        If showTankOrderItem IsNot Nothing Then
            showTankOrderNo = showTankOrderItem.showOrderNo
            showTankTransNo = showTankOrderItem.showTransNo

        End If
        If showTankOrderNo IsNot Nothing OrElse showTankOrderNo <> "" Then
            Dim afterUpdateDrVisibleTankRows = (From item In afterUpdateDbDt Where item("ORDERNO").Equals(showTankOrderNo) AndAlso item("TRANSNO").Equals(showTankTransNo))
            For Each showTanksRow In afterUpdateDrVisibleTankRows
                If showTanksRow("SHOWHIDE").Equals("COST") Then
                    showTanksRow.Item("HIDDEN") = 0
                Else
                    showTanksRow.Item("SHOWHIDE") = "SHOW"
                End If
            Next
        End If
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = afterUpdateDbDt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If

        COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
        COA0021ListTable.TBLDATA = afterUpdateDbDt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If
        CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, naeiw:=naeiw,
                                    messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
        Me.hasModifiedRow.Value = ""

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Me.hdnMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
            'WF_TITLETEXT.Text = COA0011ReturnUrl.NAMES
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        '次画面の変数セット
        HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
        HttpContext.Current.Session("MAPurl") = COA0011ReturnUrl.URL
        '画面遷移実行()
        Server.Transfer(COA0011ReturnUrl.URL)
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
        Dim clickedRow As DataRow = (From item In dt Where item("LINECNT").Equals(Integer.Parse(currentRowNum))).FirstOrDefault
        'ありえないが対象の行番号のデータがない場合
        If clickedRow Is Nothing Then
            Return 'そのまま終了
        End If
        '現在開いているタンクは閉じる
        Dim currentShowDr = (From item In dt Where (Not item("LINECNT").Equals(Integer.Parse(currentRowNum))) AndAlso item("SHOWHIDE").Equals("SHOW")).FirstOrDefault
        If currentShowDr IsNot Nothing Then
            currentShowDr.Item("SHOWHIDE") = "HIDE"
        End If
        Dim hideTankDrList = (From item In dt Where item("SHOWHIDE").Equals("COST") AndAlso item("HIDDEN").Equals(0))
        If hideTankDrList.Any = True Then
            For Each hideTankDr In hideTankDrList
                hideTankDr.Item("HIDDEN") = 1
            Next
        End If
        '選択された行のオーダーに紐づくタンクを表示
        Dim orderNo As String = Convert.ToString(clickedRow.Item("ORDERNO"))
        Dim showHide As String = Convert.ToString(clickedRow.Item("SHOWHIDE"))
        Dim transNo As String = Convert.ToString(clickedRow.Item("TRANSNO"))

        Dim hide As Integer = 0
        Dim showHideAfterProcValue As String = "SHOW"
        If showHide = "SHOW" Then
            hide = 1
            showHideAfterProcValue = "HIDE"
        End If

        Dim tankDrList = (From item In dt Where (Not item("LINECNT").Equals(Integer.Parse(currentRowNum))) AndAlso item("ORDERNO").Equals(orderNo) _
                                                 AndAlso item("TRANSNO").Equals(transNo))

        If tankDrList.Any = True Then
            'タンクレコードを非表示
            For Each tankDr In tankDrList
                tankDr.Item("HIDDEN") = hide
            Next
        End If


        'SHOWHIDE項目の値を入れ替え
        clickedRow.Item("SHOWHIDE") = showHideAfterProcValue
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
    ''' リスト案分ボタン押下時
    ''' </summary>
    Public Sub btnListDistribution_Click()
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
        Dim clickedRow As DataRow = (From item In dt Where item("LINECNT").Equals(Integer.Parse(currentRowNum))).FirstOrDefault
        'ありえないが対象の行番号のデータがない場合
        If clickedRow Is Nothing Then
            Return 'そのまま終了
        End If
        '選択された行のオーダーに紐づくタンクを表示
        Dim orderNo As String = Convert.ToString(clickedRow.Item("ORDERNO"))
        Dim transNo As String = Convert.ToString(clickedRow.Item("TRANSNO"))
        Dim showHide As String = Convert.ToString(clickedRow.Item("SHOWHIDE"))
        Dim tankDrList = (From item In dt Where (Not item("LINECNT").Equals(Integer.Parse(currentRowNum))) AndAlso item("ORDERNO").Equals(orderNo) AndAlso item("TRANSNO").Equals(transNo)).ToList
        If tankDrList Is Nothing OrElse tankDrList.Count = 0 Then
            Return 'ありえないがオーダーに紐付くタンクのレコードがない場合は終了
        End If
        Dim isCopyFromBrCostValue As Boolean = False
        '全ての金額がBRとOREDERにて一致していないか判定
        Dim deffBrOrder = (From item In tankDrList Where Not (item("AMOUNTBR").Equals(item("AMOUNTORD"))))
        If deffBrOrder.Any = True Then
            'BR <> ORDERの金額に1件でも不一致があればBR金額のコピー処理を行うよう変数に設定
            isCopyFromBrCostValue = True
        End If
        '対象オーダーの費目コードをグループ化してリストに保存
        Dim groupList = (From item In tankDrList Group By CostCode = Convert.ToString(item("COSTCODE")), POLPOD = Convert.ToString(item("DTLPOLPOD")) Into grp = Group Select CostCode, POLPOD).ToList

        '費目コードをループ
        For Each groupItem In groupList
            '変更対象のオーダー、費目をループ
            Dim costModDrList = (From item In tankDrList Where item("COSTCODE").Equals(groupItem.CostCode) AndAlso item("DTLPOLPOD").Equals(groupItem.POLPOD) Order By Integer.Parse(Convert.ToString(item("LINECNT"))))
            Dim isFirst As Boolean = True
            For Each costModDr In costModDrList
                If isFirst = True OrElse isCopyFromBrCostValue = True Then
                    costModDr.Item("AMOUNTORD") = costModDr.Item("AMOUNTBR")
                    isFirst = False
                Else
                    costModDr.Item("AMOUNTORD") = "0"
                End If
            Next '更新対象レコードループEnd
        Next '費目Loop End
        'ロード時と変化があるか比較
        Dim modDataList = GetModifiedDataTable(dt)
        Dim findOrder = From item In modDataList Where item("ORDERNO").Equals(orderNo)
        Dim changedText As String = "" '変更文言
        If findOrder.Any = True Then
            changedText = hdnTextUpdated.Value
        End If
        clickedRow.Item("AMOUNTORD") = changedText
        '変更有無フラグをHiddenに設定
        Dim findHasModifiedRow = From item In dt Where item("AMOUNTORD").Equals(hdnTextUpdated.Value)
        If findHasModifiedRow.Any = True Then
            Me.hasModifiedRow.Value = "1"
        Else
            Me.hasModifiedRow.Value = ""
        End If

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If
        'ここまで来たら正常終了
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
    End Sub
    ''' <summary>
    ''' リスト編集ボタン押下時イベント
    ''' </summary>
    ''' <remarks>未実装</remarks>
    Public Sub btnListEdit_Click()
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
        Dim clickedRow As DataRow = (From item In dt Where item("LINECNT").Equals(Integer.Parse(currentRowNum))).FirstOrDefault
        'ありえないが対象の行番号のデータがない場合
        If clickedRow Is Nothing Then
            Return 'そのまま終了
        End If
        '選択レコード情報設定
        Me.hdnSelectedOrderNo.Value = Convert.ToString(clickedRow.Item("ORDERNO"))
        Me.hdnSelectedWhichTrans.Value = Convert.ToString(clickedRow.Item("TRANSNO"))

        Dim mapIdp As String = "GBT00013R"
        Dim varP As String = "GB_BL"

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
    ''' リスト発行ボタン押下時処理
    ''' </summary>
    ''' <remarks>選択したオーダーレコードについて、BLIDを付与する</remarks>
    Public Sub btnListIssue_Click()
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
        Dim clickedRow As DataRow = (From item In dt Where item("LINECNT").Equals(Integer.Parse(currentRowNum))).FirstOrDefault
        'ありえないが対象の行番号のデータがない場合
        If clickedRow Is Nothing Then
            Return 'そのまま終了
        End If
        Dim orderNo As String = Convert.ToString(clickedRow.Item("ORDERNO"))
        Dim transNo As String = Convert.ToString(clickedRow.Item("TRANSNO"))
        Dim stYmd As String = Convert.ToString(clickedRow.Item("STYMD")) '更新処理にて主キー
        Dim initYmd As String = Convert.ToString(clickedRow.Item("INITYMD")) '更新処理にて主キー

        Dim blId As String = Convert.ToString(clickedRow.Item("BLID")).Trim

        '更新直前のレコードを取得   
        Dim latestDbDt As DataTable = GetOrderListDataTable(orderNo)
        If latestDbDt Is Nothing OrElse latestDbDt.Rows.Count = 0 Then
            '他ユーザーにてオーダー削除想定
            CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage)
            Return
        End If
        Dim latestBaseRow = (From item In latestDbDt Where item("BASEVALUEFLG").Equals("1") AndAlso item("TRANSNO").Equals(transNo)).FirstOrDefault
        If latestBaseRow Is Nothing OrElse Not (latestBaseRow.Item("TIMSTP").Equals(clickedRow.Item("TIMSTP"))) Then
            'タイムスタンプ不一致
            CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage)
            Return
        End If
        'SQL作成（直近レコードを引き継ぎ新規挿入し、削除フラグを立てる）
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO GBT0004_ODR_BASE(")
        sqlStat.AppendLine("    ORDERNO")
        sqlStat.AppendLine("   ,STYMD")
        sqlStat.AppendLine("   ,ENDYMD")
        sqlStat.AppendLine("   ,BRID")
        sqlStat.AppendLine("   ,BRTYPE")
        sqlStat.AppendLine("   ,VALIDITYFROM")
        sqlStat.AppendLine("   ,VALIDITYTO")
        sqlStat.AppendLine("   ,TERMTYPE")
        sqlStat.AppendLine("   ,NOOFTANKS")
        sqlStat.AppendLine("   ,SHIPPER")
        sqlStat.AppendLine("   ,CONSIGNEE")
        sqlStat.AppendLine("   ,CARRIER1")
        sqlStat.AppendLine("   ,CARRIER2")
        sqlStat.AppendLine("   ,PRODUCTCODE")
        sqlStat.AppendLine("   ,PRODUCTWEIGHT")
        sqlStat.AppendLine("   ,RECIEPTCOUNTRY1")
        sqlStat.AppendLine("   ,RECIEPTPORT1")
        sqlStat.AppendLine("   ,RECIEPTCOUNTRY2")
        sqlStat.AppendLine("   ,RECIEPTPORT2")
        sqlStat.AppendLine("   ,LOADCOUNTRY1")
        sqlStat.AppendLine("   ,LOADPORT1")
        sqlStat.AppendLine("   ,LOADCOUNTRY2")
        sqlStat.AppendLine("   ,LOADPORT2")
        sqlStat.AppendLine("   ,DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   ,DISCHARGEPORT1")
        sqlStat.AppendLine("   ,DISCHARGECOUNTRY2")
        sqlStat.AppendLine("   ,DISCHARGEPORT2")
        sqlStat.AppendLine("   ,DELIVERYCOUNTRY1")
        sqlStat.AppendLine("   ,DELIVERYPORT1")
        sqlStat.AppendLine("   ,DELIVERYCOUNTRY2")
        sqlStat.AppendLine("   ,DELIVERYPORT2")
        sqlStat.AppendLine("   ,VSL1")
        sqlStat.AppendLine("   ,VOY1")
        sqlStat.AppendLine("   ,ETD1")
        sqlStat.AppendLine("   ,ETA1")
        sqlStat.AppendLine("   ,VSL2")
        sqlStat.AppendLine("   ,VOY2")
        sqlStat.AppendLine("   ,ETD2")
        sqlStat.AppendLine("   ,ETA2")
        sqlStat.AppendLine("   ,INVOICEDBY")
        sqlStat.AppendLine("   ,LOADING")
        sqlStat.AppendLine("   ,STEAMING")
        sqlStat.AppendLine("   ,TIP")
        sqlStat.AppendLine("   ,EXTRA")
        sqlStat.AppendLine("   ,DEMURTO")
        sqlStat.AppendLine("   ,DEMURUSRATE1")
        sqlStat.AppendLine("   ,DEMURUSRATE2")
        sqlStat.AppendLine("   ,SALESPIC")
        sqlStat.AppendLine("   ,AGENTORGANIZER")
        sqlStat.AppendLine("   ,AGENTPOL1")
        sqlStat.AppendLine("   ,AGENTPOL2")
        sqlStat.AppendLine("   ,AGENTPOD1")
        sqlStat.AppendLine("   ,AGENTPOD2")
        sqlStat.AppendLine("   ,BLID1")
        sqlStat.AppendLine("   ,BLAPPDATE1")
        sqlStat.AppendLine("   ,BLID2")
        sqlStat.AppendLine("   ,BLAPPDATE2")
        sqlStat.AppendLine("   ,SHIPPERNAME")
        sqlStat.AppendLine("   ,SHIPPERTEXT")
        sqlStat.AppendLine("   ,CONSIGNEENAME")
        sqlStat.AppendLine("   ,CONSIGNEETEXT")
        sqlStat.AppendLine("   ,IECCODE")
        sqlStat.AppendLine("   ,NOTIFYNAME")
        sqlStat.AppendLine("   ,NOTIFYTEXT")
        sqlStat.AppendLine("   ,NOTIFYCONT")
        sqlStat.AppendLine("   ,NOTIFYCONTNAME")
        sqlStat.AppendLine("   ,NOTIFYCONTTEXT1")
        sqlStat.AppendLine("   ,NOTIFYCONTTEXT2")
        sqlStat.AppendLine("   ,PRECARRIAGETEXT")
        sqlStat.AppendLine("   ,VSL")
        sqlStat.AppendLine("   ,VOY")
        sqlStat.AppendLine("   ,FINDESTINATIONNAME")
        sqlStat.AppendLine("   ,FINDESTINATIONTEXT")
        sqlStat.AppendLine("   ,PRODUCT")
        sqlStat.AppendLine("   ,PRODUCTPORDER")
        sqlStat.AppendLine("   ,PRODUCTTIP")
        sqlStat.AppendLine("   ,PRODUCTFREIGHT")
        sqlStat.AppendLine("   ,FREIGHTANDCHARGES")
        sqlStat.AppendLine("   ,PREPAIDAT")
        sqlStat.AppendLine("   ,GOODSPKGS")
        sqlStat.AppendLine("   ,CONTAINERPKGS")
        sqlStat.AppendLine("   ,BLNUM")
        sqlStat.AppendLine("   ,CONTAINERNO")
        sqlStat.AppendLine("   ,SEALNO")
        sqlStat.AppendLine("   ,NOOFCONTAINER")
        sqlStat.AppendLine("   ,DECLAREDVALUE")
        sqlStat.AppendLine("   ,REVENUETONS")
        sqlStat.AppendLine("   ,RATE")
        sqlStat.AppendLine("   ,PER")
        sqlStat.AppendLine("   ,PREPAID")
        sqlStat.AppendLine("   ,COLLECT")
        sqlStat.AppendLine("   ,EXCHANGERATE")
        sqlStat.AppendLine("   ,PAYABLEAT")
        sqlStat.AppendLine("   ,LOCALCURRENCY")
        sqlStat.AppendLine("   ,CARRIERBLNO")
        sqlStat.AppendLine("   ,BOOKINGNO")
        sqlStat.AppendLine("   ,NOOFPACKAGE")
        sqlStat.AppendLine("   ,BLTYPE")
        sqlStat.AppendLine("   ,NOOFBL")
        sqlStat.AppendLine("   ,PAYMENTPLACE")
        sqlStat.AppendLine("   ,BLISSUEPLACE")
        sqlStat.AppendLine("   ,ANISSUEPLACE")
        sqlStat.AppendLine("   ,MEASUREMENT")
        sqlStat.AppendLine("   ,REMARK")
        sqlStat.AppendLine("   ,DELFLG")
        sqlStat.AppendLine("   ,INITYMD")
        sqlStat.AppendLine("   ,UPDYMD")
        sqlStat.AppendLine("   ,UPDUSER")
        sqlStat.AppendLine("   ,UPDTERMID")
        sqlStat.AppendLine("   ,RECEIVEYMD")
        sqlStat.AppendLine(") SELECT ")
        sqlStat.AppendLine("    ORDERNO")
        sqlStat.AppendLine("   ,STYMD")
        sqlStat.AppendLine("   ,ENDYMD")
        sqlStat.AppendLine("   ,BRID")
        sqlStat.AppendLine("   ,BRTYPE")
        sqlStat.AppendLine("   ,VALIDITYFROM")
        sqlStat.AppendLine("   ,VALIDITYTO")
        sqlStat.AppendLine("   ,TERMTYPE")
        sqlStat.AppendLine("   ,NOOFTANKS")
        sqlStat.AppendLine("   ,SHIPPER")
        sqlStat.AppendLine("   ,CONSIGNEE")
        sqlStat.AppendLine("   ,CARRIER1")
        sqlStat.AppendLine("   ,CARRIER2")
        sqlStat.AppendLine("   ,PRODUCTCODE")
        sqlStat.AppendLine("   ,PRODUCTWEIGHT")
        sqlStat.AppendLine("   ,RECIEPTCOUNTRY1")
        sqlStat.AppendLine("   ,RECIEPTPORT1")
        sqlStat.AppendLine("   ,RECIEPTCOUNTRY2")
        sqlStat.AppendLine("   ,RECIEPTPORT2")
        sqlStat.AppendLine("   ,LOADCOUNTRY1")
        sqlStat.AppendLine("   ,LOADPORT1")
        sqlStat.AppendLine("   ,LOADCOUNTRY2")
        sqlStat.AppendLine("   ,LOADPORT2")
        sqlStat.AppendLine("   ,DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   ,DISCHARGEPORT1")
        sqlStat.AppendLine("   ,DISCHARGECOUNTRY2")
        sqlStat.AppendLine("   ,DISCHARGEPORT2")
        sqlStat.AppendLine("   ,DELIVERYCOUNTRY1")
        sqlStat.AppendLine("   ,DELIVERYPORT1")
        sqlStat.AppendLine("   ,DELIVERYCOUNTRY2")
        sqlStat.AppendLine("   ,DELIVERYPORT2")
        sqlStat.AppendLine("   ,VSL1")
        sqlStat.AppendLine("   ,VOY1")
        sqlStat.AppendLine("   ,ETD1")
        sqlStat.AppendLine("   ,ETA1")
        sqlStat.AppendLine("   ,VSL2")
        sqlStat.AppendLine("   ,VOY2")
        sqlStat.AppendLine("   ,ETD2")
        sqlStat.AppendLine("   ,ETA2")
        sqlStat.AppendLine("   ,INVOICEDBY")
        sqlStat.AppendLine("   ,LOADING")
        sqlStat.AppendLine("   ,STEAMING")
        sqlStat.AppendLine("   ,TIP")
        sqlStat.AppendLine("   ,EXTRA")
        sqlStat.AppendLine("   ,DEMURTO")
        sqlStat.AppendLine("   ,DEMURUSRATE1")
        sqlStat.AppendLine("   ,DEMURUSRATE2")
        sqlStat.AppendLine("   ,SALESPIC")
        sqlStat.AppendLine("   ,AGENTORGANIZER")
        sqlStat.AppendLine("   ,AGENTPOL1")
        sqlStat.AppendLine("   ,AGENTPOL2")
        sqlStat.AppendLine("   ,AGENTPOD1")
        sqlStat.AppendLine("   ,AGENTPOD2")
        If transNo = "1" Then
            sqlStat.AppendLine("   ,@BLID")
            sqlStat.AppendLine("   ,@BLAPPDATE")
            sqlStat.AppendLine("   ,BLID2")
            sqlStat.AppendLine("   ,BLAPPDATE2")
        Else
            sqlStat.AppendLine("   ,BLID1")
            sqlStat.AppendLine("   ,BLAPPDATE1")
            sqlStat.AppendLine("   ,@BLID")
            sqlStat.AppendLine("   ,@BLAPPDATE")
        End If
        sqlStat.AppendLine("   ,SHIPPERNAME")
        sqlStat.AppendLine("   ,SHIPPERTEXT")
        sqlStat.AppendLine("   ,CONSIGNEENAME")
        sqlStat.AppendLine("   ,CONSIGNEETEXT")
        sqlStat.AppendLine("   ,IECCODE")
        sqlStat.AppendLine("   ,NOTIFYNAME")
        sqlStat.AppendLine("   ,NOTIFYTEXT")
        sqlStat.AppendLine("   ,NOTIFYCONT")
        sqlStat.AppendLine("   ,NOTIFYCONTNAME")
        sqlStat.AppendLine("   ,NOTIFYCONTTEXT1")
        sqlStat.AppendLine("   ,NOTIFYCONTTEXT2")
        sqlStat.AppendLine("   ,PRECARRIAGETEXT")
        sqlStat.AppendLine("   ,VSL")
        sqlStat.AppendLine("   ,VOY")
        sqlStat.AppendLine("   ,FINDESTINATIONNAME")
        sqlStat.AppendLine("   ,FINDESTINATIONTEXT")
        sqlStat.AppendLine("   ,PRODUCT")
        sqlStat.AppendLine("   ,PRODUCTPORDER")
        sqlStat.AppendLine("   ,PRODUCTTIP")
        sqlStat.AppendLine("   ,PRODUCTFREIGHT")
        sqlStat.AppendLine("   ,FREIGHTANDCHARGES")
        sqlStat.AppendLine("   ,PREPAIDAT")
        sqlStat.AppendLine("   ,GOODSPKGS")
        sqlStat.AppendLine("   ,CONTAINERPKGS")
        sqlStat.AppendLine("   ,BLNUM")
        sqlStat.AppendLine("   ,CONTAINERNO")
        sqlStat.AppendLine("   ,SEALNO")
        sqlStat.AppendLine("   ,NOOFCONTAINER")
        sqlStat.AppendLine("   ,DECLAREDVALUE")
        sqlStat.AppendLine("   ,REVENUETONS")
        sqlStat.AppendLine("   ,RATE")
        sqlStat.AppendLine("   ,PER")
        sqlStat.AppendLine("   ,PREPAID")
        sqlStat.AppendLine("   ,COLLECT")
        sqlStat.AppendLine("   ,EXCHANGERATE")
        sqlStat.AppendLine("   ,PAYABLEAT")
        sqlStat.AppendLine("   ,LOCALCURRENCY")
        sqlStat.AppendLine("   ,CARRIERBLNO")
        sqlStat.AppendLine("   ,BOOKINGNO")
        sqlStat.AppendLine("   ,NOOFPACKAGE")
        sqlStat.AppendLine("   ,BLTYPE")
        sqlStat.AppendLine("   ,NOOFBL")
        sqlStat.AppendLine("   ,PAYMENTPLACE")
        sqlStat.AppendLine("   ,BLISSUEPLACE")
        sqlStat.AppendLine("   ,ANISSUEPLACE")
        sqlStat.AppendLine("   ,MEASUREMENT")
        sqlStat.AppendLine("   ,REMARK")
        sqlStat.AppendLine("   ,DELFLG")
        sqlStat.AppendLine("   ,@UPDYMD")
        sqlStat.AppendLine("   ,@UPDYMD")
        sqlStat.AppendLine("   ,@UPDUSER")
        sqlStat.AppendLine("   ,@UPDTERMID")
        sqlStat.AppendLine("   ,@RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0004_ODR_BASE")
        sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO")
        sqlStat.AppendLine("   AND STYMD   = @STYMD")
        sqlStat.AppendLine("   AND INITYMD = @INITYMD;")

        sqlStat.AppendLine("UPDATE GBT0004_ODR_BASE")
        sqlStat.AppendLine("   SET DELFLG = '" & CONST_FLAG_YES & "'")
        sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD")
        sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID = @UPDTERMID")
        sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO")
        sqlStat.AppendLine("   AND STYMD   = @STYMD")
        sqlStat.AppendLine("   AND INITYMD = @INITYMD;")

        'SQLの実行
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            If blId = "" Then
                blId = GetBlId(sqlCon)
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                With sqlCmd.Parameters
                    .Add("@BLID", SqlDbType.NVarChar).Value = blId
                    .Add("@BLAPPDATE", SqlDbType.Date).Value = Now

                    .Add("@UPDYMD", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")

                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@STYMD", SqlDbType.Date).Value = stYmd
                    .Add("@INITYMD", SqlDbType.DateTime).Value = initYmd
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.ExecuteNonQuery()
            End Using
        End Using
        '更新後の全レコードを取得   
        Dim afterUpdateDbDt As DataTable = GetOrderListDataTable()
        Dim showTankOrderItem = (From item In dt Where item("SHOWHIDE").Equals("SHOW") Select showOrderNo = Convert.ToString(item("ORDERNO")), showTransNo = Convert.ToString(item("TRANSNO"))).FirstOrDefault
        Dim showTankOrderNo As String = ""
        Dim showTankTransNo As String = ""
        If showTankOrderItem IsNot Nothing Then
            showTankOrderNo = showTankOrderItem.showOrderNo
            showTankTransNo = showTankOrderItem.showTransNo
        End If
        If showTankOrderNo IsNot Nothing OrElse showTankOrderNo <> "" Then
            Dim afterUpdateDrVisibleTankRows = (From item In afterUpdateDbDt Where item("ORDERNO").Equals(showTankOrderNo) AndAlso item("TRANSNO").Equals(showTankTransNo))
            For Each showTanksRow In afterUpdateDrVisibleTankRows
                If showTanksRow("SHOWHIDE").Equals("COST") Then
                    showTanksRow.Item("HIDDEN") = 0
                Else
                    showTanksRow.Item("SHOWHIDE") = "SHOW"
                End If
            Next
        End If
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = afterUpdateDbDt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If
        COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
        COA0021ListTable.TBLDATA = afterUpdateDbDt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL,
                                    messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})

    End Sub
    ''' <summary>
    ''' オーダー一覧より値取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>要UNION オーダー</remarks>
    Private Function GetOrderListDataTable(Optional orderNo As String = "") As DataTable
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        Dim dt As New DataTable
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
        Dim textTraderTblField As String = "NAMES"
        'If COA0019Session.LANGDISP = C_LANG.JA Then
        '    textProductTblField = "NAMES"
        'End If
        Dim textCostTblField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCostTblField = "NAMES"
        End If
        'ソート順取得
        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = Me.hdnMapVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()
        '個別入力条件の設定
        Dim sqlEtdOrderCondition As New StringBuilder
        Dim etdDatefield As String = ""
        Dim etdActy As String = "('SHIP','RPEC','ECHL')"

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
            'オーダー明細

            If Me.hdnETDStYMD.Value <> "" And Me.hdnETDEndYMD.Value <> "" Then

                With sqlEtdOrderCondition
                    .AppendLine(" AND ")
                    .AppendLine("(    (     OBS.ETD1 BETWEEN @ETDST And @ETDEND")
                    .AppendLine("     )")
                    .AppendLine(" Or  (     OBS.ETD2 BETWEEN @ETDST And @ETDEND")
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
            'オーダー明細
            If Me.hdnETDStYMD.Value <> "" And Me.hdnETDEndYMD.Value <> "" Then
                With sqlEtdOrderCondition
                    .AppendLine(" AND ")
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
        End If
        '三国間を考慮し輸送単位ごとにBASEレコードのWith句を生成
        Dim withPreix As String = "With"
        For i = 1 To 2
            'オーダー本体のWidth句(当明細が含まれるブレーカーも対象（削除除く）)
            sqlStat.AppendFormat("{1} W_ORDERBASE{0} As (", i, withPreix).AppendLine()
            sqlStat.AppendLine("SELECT '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(OBS.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,FORMAT(OBS.STYMD,'yyyy/MM/dd') AS STYMD")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendFormat("      ,'{0}' AS TRANSNO ", i) '三国間(1:第一輸送、2:第二輸送)
            sqlStat.AppendLine("      ,OBS.ORDERNO AS ORDERNO")
            sqlStat.AppendFormat("      ,ISNULL(SP.{0},'') AS SHIPPER", textCustomerTblField).AppendLine()
            sqlStat.AppendFormat("      ,ISNULL(CN.{0},'') AS CONSIGNEE", textCustomerTblField).AppendLine()
            sqlStat.AppendLine("      ,ISNULL(POL.AREANAME,'')  AS PORT")
            sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETD{1}", String.Format(etdDatefield, i), i)
            sqlStat.AppendFormat("      ,ISNULL(PD.{0},'')  AS PRODUCT", textProductTblField).AppendLine()
            sqlStat.AppendFormat("      ,ISNULL(CR1.{0},'') AS CARRIER", textTraderTblField).AppendLine()
            sqlStat.AppendFormat("      ,OBS.VSL{0}         AS VSL", i).AppendLine()
            sqlStat.AppendLine("      ,''          AS TANKSEQ")
            sqlStat.AppendLine("      ,''          AS TANKNO")
            sqlStat.AppendLine("      ,''          AS AMOUNTBR")
            sqlStat.AppendLine("      ,''          AS AMOUNTORD")
            sqlStat.AppendFormat("      ,OBS.BLID{0}    AS BLID", i)
            sqlStat.AppendLine("      ,''          AS DISTRIBUTION ") '案分
            sqlStat.AppendLine("      ,''          AS EDIT ") '編集
            sqlStat.AppendLine("      ,''          AS ISSUE ") '発行
            sqlStat.AppendLine("      ,''          AS COSTCODE ")
            sqlStat.AppendLine("      ,''          AS COSTNAME ")
            sqlStat.AppendLine("      ,''          AS DTLPOLPOD ")
            sqlStat.AppendLine("      ,'1'         AS BASEVALUEFLG")  'ブレーカーオーダーフラグ(1:BASE,2:VALUE)
            sqlStat.AppendLine("      ,'HIDE'      AS SHOWHIDE")
            sqlStat.AppendLine("      ,''          AS DATAID")
            sqlStat.AppendLine("      ,convert(nvarchar,OBS.INITYMD,121) AS INITYMD ")
            sqlStat.AppendLine("  FROM GBT0004_ODR_BASE OBS")
            sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP") 'SHIPPER名称用JOIN
            sqlStat.AppendLine("    ON  SP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  SP.COUNTRYCODE  = OBS.LOADCOUNTRY1")
            sqlStat.AppendLine("   AND  SP.CUSTOMERCODE = OBS.SHIPPER")
            sqlStat.AppendLine("   AND  SP.STYMD       <= OBS.ENDYMD")
            sqlStat.AppendLine("   AND  SP.ENDYMD      >= OBS.STYMD")
            sqlStat.AppendLine("   AND  SP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("   AND  SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
            sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'PRODUCT名称用JOIN
            sqlStat.AppendLine("    ON  PD.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  PD.PRODUCTCODE  = OBS.PRODUCTCODE")
            sqlStat.AppendLine("   AND  PD.STYMD       <= OBS.ENDYMD")
            sqlStat.AppendLine("   AND  PD.ENDYMD      >= OBS.STYMD")
            sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("   AND  PD.ENABLED      = @ENABLED")
            sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT POL") 'POL名称用JOIN
            sqlStat.AppendLine("    ON  POL.COMPCODE     = @COMPCODE")
            sqlStat.AppendFormat("   AND  POL.COUNTRYCODE  = OBS.LOADCOUNTRY{0}", i).AppendLine()
            sqlStat.AppendFormat("   AND  POL.PORTCODE     = OBS.LOADPORT{0}", i).AppendLine()
            sqlStat.AppendLine("   AND  POL.STYMD       <= OBS.ENDYMD")
            sqlStat.AppendLine("   AND  POL.ENDYMD      >= OBS.STYMD")
            sqlStat.AppendLine("   AND  POL.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CN") 'CONSIGNEE名称用JOIN
            sqlStat.AppendLine("    ON  CN.COMPCODE     = @COMPCODE")
            sqlStat.AppendFormat("   AND  CN.COUNTRYCODE  = OBS.LOADCOUNTRY{0}", i).AppendLine()
            sqlStat.AppendLine("   AND  CN.CUSTOMERCODE = OBS.CONSIGNEE")
            sqlStat.AppendLine("   AND  CN.STYMD       <= OBS.ENDYMD")
            sqlStat.AppendLine("   AND  CN.ENDYMD      >= OBS.STYMD")
            sqlStat.AppendLine("   AND  CN.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("   AND  CN.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")
            sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER CR1")  'キャリア名取得用JOIN
            sqlStat.AppendLine("    ON  CR1.COMPCODE     = @COMPCODE")
            sqlStat.AppendFormat("   AND  CR1.CARRIERCODE  = OBS.CARRIER{0}", i).AppendLine()
            sqlStat.AppendLine("   AND  CR1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  CR1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  CR1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("   AND  CR1.CLASS        = 'FORWARDER'")
            sqlStat.AppendLine(" WHERE OBS.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND EXISTS (")
            sqlStat.AppendLine("        SELECT 1")
            sqlStat.AppendLine("          FROM GBT0005_ODR_VALUE SVL")
            sqlStat.AppendLine("         WHERE SVL.ORDERNO = OBS.ORDERNO")
            sqlStat.AppendFormat("           AND SVL.DTLPOLPOD like '%{0}'", i)
            sqlStat.AppendLine("           AND SVL.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("              ) ") '第n輸送のレコードが存在すれば対象

            If sqlEtdOrderCondition.Length > 0 Then
                sqlStat.AppendLine(sqlEtdOrderCondition.ToString)
            End If
            If Me.hdnShipper.Value <> "" Then
                sqlStat.AppendLine("   AND OBS.SHIPPER       = @SHIPPER")
            End If
            If Me.hdnConsignee.Value <> "" Then
                sqlStat.AppendLine("   AND OBS.CONSIGNEE     = @CONSIGNEE")

            End If
            If Me.hdnPort.Value <> "" Then
                sqlStat.AppendFormat("   AND OBS.LOADPORT{0}     = @POL", i).AppendLine()
            End If
            If Me.hdnProduct.Value <> "" Then
                sqlStat.AppendLine("   AND OBS.PRODUCTCODE   = @PRODUCTCODE")
            End If
            If Me.hdnCarrier.Value <> "" Then
                sqlStat.AppendFormat("   AND OBS.CARRIER{0}     = @CARRIER", i).AppendLine()
            End If
            If Me.hdnVsl.Value <> "" Then
                sqlStat.AppendFormat("   AND OBS.VSL{0}          = @VSL", i).AppendLine()
            End If
            '一旦保留(BL発行有無)
            If Me.hdnBlIssued.Value = "Y" Then
                sqlStat.AppendLine("   AND OBS.BLID   <> ''")
            End If
            If Me.hdnBlIssued.Value = "N" Then
                sqlStat.AppendLine("   AND OBS.BLID    = ''")
            End If

            If orderNo <> "" Then
                sqlStat.AppendLine("   AND OBS.ORDERNO   = @ORDERNO")
            End If

            sqlStat.AppendLine(")")
            withPreix = ","
        Next
        '共通関数は単一テーブル想定のため全体をサブクエリー化 
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
        sqlStat.AppendLine("FROM (")

        For i = 1 To 2
            If i > 1 Then
                sqlStat.AppendLine("UNION ALL ")
            End If

            sqlStat.AppendLine("SELECT '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(OVL.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,''  AS STYMD") 'VALUEデータについては主キーでないので取得しない
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'1' AS HIDDEN ") 'デフォルトで費用明細は隠す
            sqlStat.AppendFormat("      ,'{0}' AS TRANSNO ", i).AppendLine() '三国間(1:第一輸送、2:第二輸送)
            sqlStat.AppendLine("      ,OVL.ORDERNO      AS ORDERNO")
            sqlStat.AppendLine("      ,''               AS SHIPPER")
            sqlStat.AppendLine("      ,''               AS CONSIGNEE")
            sqlStat.AppendLine("      ,''               AS PORT")
            sqlStat.AppendLine("      ,''               AS ETD")
            sqlStat.AppendLine("      ,''               AS PRODUCT")
            sqlStat.AppendLine("      ,''               AS CARRIER")
            sqlStat.AppendLine("      ,''               AS VSL")
            sqlStat.AppendLine("      ,OVL.TANKSEQ      AS TANKSEQ")
            sqlStat.AppendLine("      ,OVL.TANKNO       AS TANKNO")
            sqlStat.AppendLine("      ,CONVERT(varchar,OVL.AMOUNTBR)     AS AMOUNTBR")
            sqlStat.AppendLine("      ,CONVERT(varchar,OVL.AMOUNTORD)    AS AMOUNTORD")
            sqlStat.AppendLine("      ,''               AS BLID")
            sqlStat.AppendLine("      ,''               AS DISTRIBUTION")
            sqlStat.AppendLine("      ,''               AS EDIT")
            sqlStat.AppendLine("      ,''               AS ISSUE")
            sqlStat.AppendLine("      ,OVL.COSTCODE     AS COSTCODE ")
            sqlStat.AppendFormat("     , ISNULL(CST.{0},'')   AS COSTNAME", textCostTblField).AppendLine()
            sqlStat.AppendLine("      ,OVL.DTLPOLPOD    AS DTLPOLPOD ")
            sqlStat.AppendLine("      ,'2'              AS BASEVALUEFLG")
            sqlStat.AppendLine("      ,'COST'      AS SHOWHIDE")
            sqlStat.AppendLine("      ,CONVERT(varchar(36),OVL.DATAID) AS DATAID")
            sqlStat.AppendLine("      ,'' AS INITYMD")

            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE OVL ")
            sqlStat.AppendFormat(" INNER JOIN W_ORDERBASE{0} BS", i).AppendLine()
            sqlStat.AppendLine("    ON  BS.ORDERNO     = OVL.ORDERNO")
            sqlStat.AppendLine(" INNER JOIN GBM0010_CHARGECODE CST")
            sqlStat.AppendLine("    ON CST.COMPCODE   = @COMPCODE")
            sqlStat.AppendLine("   AND CST.COSTCODE   = OVL.COSTCODE")
            sqlStat.AppendLine("   AND CST.STYMD     <= OVL.ENDYMD")
            sqlStat.AppendLine("   AND CST.ENDYMD    >= OVL.STYMD")
            sqlStat.AppendLine("   AND CST.CLASS9     = '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("   AND CST.DELFLG   <> @DELFLG")
            sqlStat.AppendLine(" WHERE OVL.DELFLG        <> @DELFLG")
            sqlStat.AppendFormat("   AND OVL.DTLPOLPOD like '%{0}'", i).AppendLine()
            'ここにオーダーのユニオン
            sqlStat.AppendLine("UNION ALL ")
            sqlStat.AppendFormat(" SELECT * FROM W_ORDERBASE{0}", i).AppendLine()

        Next
        sqlStat.AppendLine(" ) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@STYMD", SqlDbType.Date).Value = Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Now
                If Me.hdnETDStYMD.Value <> "" Then
                    .Add("@ETDST", SqlDbType.Date).Value = Date.Parse(Me.hdnETDStYMD.Value)
                    .Add("@ETDEND", SqlDbType.Date).Value = Date.Parse(Me.hdnETDEndYMD.Value)
                End If
                If Me.hdnShipper.Value <> "" Then
                    .Add("@SHIPPER", SqlDbType.NVarChar).Value = Me.hdnShipper.Value
                End If
                If Me.hdnConsignee.Value <> "" Then
                    .Add("@CONSIGNEE", SqlDbType.NVarChar).Value = Me.hdnConsignee.Value
                End If
                If Me.hdnPort.Value <> "" Then
                    .Add("@POL", SqlDbType.NVarChar).Value = Me.hdnPort.Value
                End If
                If Me.hdnProduct.Value <> "" Then
                    .Add("@PRODUCTCODE", SqlDbType.NVarChar).Value = Me.hdnProduct.Value
                End If
                If Me.hdnCarrier.Value <> "" Then
                    .Add("@CARRIER", SqlDbType.NVarChar).Value = Me.hdnCarrier.Value
                End If

                If Me.hdnVsl.Value <> "" Then
                    .Add("@VSL", SqlDbType.NVarChar).Value = Me.hdnVsl.Value
                End If

                If orderNo <> "" Then
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                End If
            End With
            'SQLパラメータ(動的変化あり)
            Dim paramValidityfrom As SqlParameter = Nothing
            Dim paramValidityto As SqlParameter = Nothing
            Dim paramShipper As SqlParameter = Nothing
            Dim paramConsignee As SqlParameter = Nothing
            Dim paramPort As SqlParameter = Nothing
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
                    Dim childRows = From item In dt Where item("ORDERNO").Equals(readDr.Item("ORDERNO")) _
                                                  AndAlso item("TRANSNO").Equals(readDr.Item("TRANSNO")) _
                                                  AndAlso item("BASEVALUEFLG").Equals("2")
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
            hdnListPosition.Value = Convert.ToString((dvTBLview.Count - (dvTBLview.Count Mod CONST_SCROLLROWCOUNT)))
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
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        '一覧表用文言
        AddLangSetting(dicDisplayText, Me.hdnTextShow, "表示", "Show")
        AddLangSetting(dicDisplayText, Me.hdnTextHide, "非表示", "Hide")
        AddLangSetting(dicDisplayText, Me.hdnTextDistribution, "実行", "EXECUTE")
        AddLangSetting(dicDisplayText, Me.hdnTextEdit, "編集", "EDIT")
        AddLangSetting(dicDisplayText, Me.hdnTextIssue, "発行", "ISSUE")
        AddLangSetting(dicDisplayText, Me.hdnTextUpdated, "変更あり", "UPDATED")

        AddLangSetting(dicDisplayText, Me.lblConfirmOrderNoName, "Order ID", "Order ID")
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
        Dim fieldList As New List(Of String) From {"SHOWTANK", "ORDERNO", "TRANSNO", "STYMD", "SHIPPER", "CONSIGNEE", "PORT",
                                                   "ETD", "PRODUCT", "CARRIER",
                                                   "VSL", "TANKSEQ", "TANKNO", "AMOUNTBR", "AMOUNTORD",
                                                   "BLID", "DISTRIBUTION", "EDIT", "ISSUE",
                                                   "COSTCODE", "COSTNAME", "DTLPOLPOD", "BASEVALUEFLG",
                                                   "CANNOTEDIT", "SHOWHIDE", "DATAID", "INITYMD", "HASCHILD"}
        For Each fieldName As String In fieldList
            retDt.Columns.Add(fieldName, GetType(String))
            retDt.Columns(fieldName).DefaultValue = ""
        Next
        Return retDt
    End Function
    '''' <summary>
    '''' リスト行ダブルクリック時イベント
    '''' </summary>
    'Private Sub ListRowDbClick()
    '    Dim rowIdString As String = Me.hdnListDBclick.Value
    '    Dim rowId As Integer = 0
    '    If Integer.TryParse(rowIdString, rowId) = True Then
    '        rowId = rowId - 1
    '    Else
    '        Return
    '    End If

    '    Dim dt As DataTable = CreateDataTable()
    '    Dim COA0021ListTable As New COA0021ListTable

    '    COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
    '    COA0021ListTable.TBLDATA = dt
    '    COA0021ListTable.COA0021recoverListTable()
    '    If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
    '        dt = COA0021ListTable.OUTTBL
    '    Else

    '        CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADMM, Me.lblFooterMessage,
    '                                    messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
    '        Return
    '    End If
    '    Dim selectedRow As DataRow = dt.Rows(rowId)
    '    Dim brId As String = Convert.ToString(selectedRow.Item("BRID"))
    '    Dim odId As String = Convert.ToString(selectedRow.Item("ODID"))
    '    Dim brOdFlg As String = Convert.ToString(selectedRow.Item("BRODFLG"))  'ダブルクリックされた行判定("1"ブレーカーレコード、"2"オーダーレコード)
    '    Dim mapIdp As String = "GBT00003R"
    '    Dim varP As String = "GB_OrderNew"
    '    If brOdFlg = "2" Then
    '        mapIdp = "GBT00003R"
    '        varP = "GB_ShowDetail"
    '    End If
    '    Me.hdnSelectedBrId.Value = brId
    '    Me.hdnSelectedOdId.Value = odId

    '    '■■■ 画面遷移先URL取得 ■■■
    '    Dim COA0012DoUrl As New COA0012DoUrl
    '    COA0012DoUrl.MAPIDP = mapIdp
    '    COA0012DoUrl.VARIP = varP
    '    COA0012DoUrl.COA0012GetDoUrl()
    '    If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
    '    Else
    '        CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
    '        Return
    '    End If
    '    Session("MAPmapid") = mapIdp
    '    Session("MAPvariant") = varP
    '    '画面遷移実行
    '    Server.Transfer(COA0012DoUrl.URL)
    'End Sub
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
        COA0013TableObject.MAPID = CONST_MAPID
        COA0013TableObject.VARI = Me.hdnMapVariant.Value
        COA0013TableObject.SRCDATA = listData
        COA0013TableObject.TBLOBJ = Me.WF_LISTAREA
        COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.HIDEOPERATIONOPT = True
        COA0013TableObject.NOCOLUMNWIDTHOPT = 50
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.USERSORTOPT = 0 '開閉行がある為ソートNG
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

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
        Dim dicColumnNameToNo As New Dictionary(Of String, String) From {{"SHOWHIDE", ""}, {"DISTRIBUTION", ""},
                                                                         {"EDIT", ""}, {"ISSUE", ""}, {"BASEVALUEFLG", ""},
                                                                         {"AMOUNTORD", ""}, {"HASCHILD", ""}}
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
                                                                             {"ORDERNO", ""},
                                                                             {"TANKSEQ", ""}}

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

        'ボタンの使用可否
        Dim buttonEnabled As Boolean = True
        If Me.hasModifiedRow.Value = "1" Then
            buttonEnabled = False
        End If
        '******************************
        'レンダリング行のループ
        '******************************
        Dim rowCnt As Integer = rightDataTable.Rows.Count - 1
        Dim dicButtonName As New Dictionary(Of String, String) From {{"EDIT", Me.hdnTextEdit.Value}, {"ISSUE", Me.hdnTextIssue.Value},
                                                                     {"DISTRIBUTION", Me.hdnTextDistribution.Value},
                                                                     {"HIDE", Me.hdnTextShow.Value}, {"SHOW", Me.hdnTextHide.Value}}
        For i = 0 To rowCnt
            Dim tbrRight As TableRow = rightDataTable.Rows(i)

            Dim tbrLeft As TableRow = leftDataTable.Rows(i)
            'Dim hideDelete As String = tbrLeft.Cells(2).Text '1削除負荷、それ以外は削除可能
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

            If showBtn = True AndAlso dicColumnNameToNo("AMOUNTORD") <> "" AndAlso
               tbrRight.Cells(Integer.Parse(dicColumnNameToNo("AMOUNTORD"))).Text <> "" Then
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("AMOUNTORD")))
                    .ViewStateMode = ViewStateMode.Disabled
                    .CssClass = "updatedValue"
                End With
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
            'ボタンの表示非表示制御
            For Each fieldName As String In {"EDIT", "ISSUE", "DISTRIBUTION"}

                If dicColumnNameToNo(fieldName) <> "" Then
                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                        .ViewStateMode = ViewStateMode.Disabled
                        .Controls(0).ViewStateMode = ViewStateMode.Disabled
                        If showBtn = False OrElse (hasChild = False AndAlso fieldName = "DISTRIBUTION") Then
                            .Controls.RemoveAt(0)
                        ElseIf .HasControls = True AndAlso TypeOf .Controls(0) Is HtmlButton Then
                            Dim htmlbutton As HtmlButton = DirectCast(.Controls(0), HtmlButton)
                            Dim htmlInputButton As New HtmlInputButton
                            If htmlbutton.Attributes.Count > 0 Then
                                For Each attrKey As String In htmlbutton.Attributes.Keys
                                    htmlInputButton.Attributes.Add(attrKey, htmlbutton.Attributes(attrKey))
                                Next
                            End If
                            htmlInputButton.ViewStateMode = ViewStateMode.Disabled
                            If {"EDIT", "ISSUE"}.Contains(fieldName) Then
                                htmlInputButton.Disabled = Not buttonEnabled
                            End If
                            htmlInputButton.ViewStateMode = ViewStateMode.Disabled
                            htmlInputButton.Attributes.Add("actType", fieldName)
                            htmlInputButton.Attributes.Add("onclick", "listButtonClick(this);false;")
                            htmlInputButton.ID = htmlbutton.ID
                            htmlInputButton.Style.Add(HtmlTextWriterStyle.Display, "inline-block")
                            htmlInputButton.Value = dicButtonName(fieldName)

                            .Controls.RemoveAt(0)
                            .Controls.Add(htmlInputButton)
                        End If
                    End With
                End If
            Next
            'カラムの値非表示(オーダーNo)
            If dicLeftColumnNameToNo("ORDERNO") <> "" AndAlso showBtn = False Then
                With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("ORDERNO")))
                    '.Attributes.Add("hideValue", "TRUE")
                    .Text = ""
                    .ViewStateMode = ViewStateMode.Disabled
                End With
            End If
        Next 'END ROWCOUNT
    End Sub
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
    ''' 左の出力帳票
    ''' </summary>
    Private Function RightboxInit() As String
        Return C_MESSAGENO.NORMAL 'レポートはまだ未着手
        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim excelMapId As String = "GBT00013"

        'RightBOX情報設定
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget
        Dim COA0022ProfXls As New BASEDLL.COA0022ProfXls
        retVal = C_MESSAGENO.NORMAL

        '初期化
        Me.txtRightErrorMessage.Text = ""

        'レポートID情報
        COA0022ProfXls.MAPID = excelMapId
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
        COA0016VARIget.MAPID = excelMapId
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
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00013SELECT Then
            Me.hdnMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, Me.hdnMapVariant.Value)
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))

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
                                                                        {"txtOffice", Me.hdnOffice}
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
                    End If

                End If
            Next
            '****************************************
            '一覧表作成
            '****************************************
            '一覧表データ取得
            Using dt As DataTable = Me.GetOrderListDataTable()
                'グリッド用データをファイルに退避
                With Nothing
                    Dim COA0021ListTable As New COA0021ListTable
                    COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                    COA0021ListTable.TBLDATA = dt
                    COA0021ListTable.COA0021saveListTable()
                    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                                    messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                        Return
                    End If
                    'ロード直後の情報を保持
                    COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                    COA0021ListTable.TBLDATA = dt
                    COA0021ListTable.COA0021saveListTable()
                    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                                    messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                        Return
                    End If
                End With
            End Using 'DataTable

        ElseIf TypeOf Page.PreviousPage Is GBT00013RESULT Then
            '自身から遷移（削除時のリフレッシュのみの想定、それ以外の用途を追加する場合は注意）
            Dim prevObj As GBT00013RESULT = DirectCast(Page.PreviousPage, GBT00013RESULT)
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
                                                                        {"hdnOffice", Me.hdnOffice}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            '正常メッセージをメッセージエリアに表示（削除正常時のみ自身をリフレッシュするため）
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)

        ElseIf TypeOf Page.PreviousPage Is GBT00014BL Then
            Me.hdnMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, Me.hdnMapVariant.Value)
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))

            '検索画面の場合
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
                                                                        {"hdnOffice", Me.hdnOffice}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            '****************************************
            '一覧表作成
            '****************************************
            '一覧表データ取得
            Using dt As DataTable = Me.GetOrderListDataTable()
                'グリッド用データをファイルに退避
                With Nothing
                    Dim COA0021ListTable As New COA0021ListTable
                    COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                    COA0021ListTable.TBLDATA = dt
                    COA0021ListTable.COA0021saveListTable()
                    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                                    messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                        Return
                    End If
                    'ロード直後の情報を保持
                    COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                    COA0021ListTable.TBLDATA = dt
                    COA0021ListTable.COA0021saveListTable()

                    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                                    messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                        Return
                    End If
                End With
            End Using 'DataTable
        End If
        Me.WF_LISTAREA.CssClass = Me.hdnSearchType.Value
    End Sub
    ''' <summary>
    ''' BLIDをシーケンスより取得
    ''' </summary>
    ''' <returns>BLID</returns>
    ''' <remark>一覧発行ボタン押下時に使用</remark>
    Private Function GetBlId(Optional ByRef sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim orderNo As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT  'BL' ")
            sqlStat.AppendLine("      + left(convert(char,getdate(),12),4)")
            sqlStat.AppendLine("      + '_'")
            sqlStat.AppendLine("      + (SELECT VALUE1")
            sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
            sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
            sqlStat.AppendLine("            AND KEYCODE = @KEYCODE)")
            sqlStat.AppendLine("      + '_'")
            sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR GBQ0007_BLISSUE)),4)")
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
    ''' 変更検知処理
    ''' </summary>
    ''' <returns>変更対象のデータテーブルを生成</returns>
    ''' <remarks>ロード時のデータテーブルと画面上に展開しているデータテーブルを比較し変化があった項目を取得</remarks>
    Private Function GetModifiedDataTable(Optional dt As DataTable = Nothing) As List(Of DataRow)
        Dim COA0021ListTable As New COA0021ListTable
        Dim currentDt As DataTable
        Dim firstTimeDt As DataTable
        Dim retList As New List(Of DataRow)
        If dt IsNot Nothing Then
            currentDt = dt
        Else
            currentDt = CreateDataTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = currentDt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                currentDt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return Nothing
            End If

        End If
        '画面ロード時に退避した編集前のデータテーブル取得
        With Nothing
            firstTimeDt = CreateDataTable()
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
        '現状 予定額しかないが念のためリスト持ちとしておく
        Dim compareFieldList As New List(Of String) From {"AMOUNTORD"}

        '変更後のデータテーブルをループ
        For Each dr As DataRow In currentDt.Rows
            If dr.Item("BASEVALUEFLG").Equals("1") Then
                Continue For
            End If
            Dim dataid As String = Convert.ToString(dr.Item("DATAID"))
            Dim compareRow = (From item In firstTimeDt Where item("DATAID").Equals(dataid)).FirstOrDefault
            If compareRow Is Nothing Then
                Continue For
            End If
            For Each fieldName In compareFieldList
                If Not dr.Item(fieldName).Equals(compareRow.Item(fieldName)) Then
                    '対象行のディープコピーを生成(参照渡しにしない）
                    Dim cloneTgtDr = currentDt.NewRow
                    cloneTgtDr.ItemArray = DirectCast(dr.ItemArray.Clone(), Object())
                    retList.Add(cloneTgtDr)
                End If
            Next
        Next
        Return retList
    End Function
    ''' <summary>
    ''' 案分した費用の更新処理
    ''' </summary>
    ''' <param name="targetDataList">更新対象データRowリスト</param>
    ''' <param name="differenceAmountList">オーダー番号、第n輸送をキーとした差額・タンク番号・タンクシーケンスを格納したディクショナリ</param>
    ''' <param name="rightMessage">[OUT]左ボックス用のメッセージ</param>
    ''' <returns></returns>
    Private Function UpdateOrderValue(targetDataList As List(Of DataRow), differenceAmountList As List(Of DifferenceAmount), ByRef rightMessage As String) As String
        Dim procDate As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        'FixValueより差額保存用の費目、備考文言を取得
        Dim COA0017FixValue As New COA0017FixValue
        Dim balAmount As List(Of String)
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "BALANCEAMOUNT"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            balAmount = COA0017FixValue.VALUEDIC.FirstOrDefault.Value
        Else
            Return COA0017FixValue.ERR
        End If
        'SQL文生成
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
        sqlStat.AppendLine("         ,AMOUNTBR")
        sqlStat.AppendLine("         ,@AMOUNTORD AS AMOUNTORD")
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
        sqlStat.AppendLine("         ,DELFLG")
        sqlStat.AppendLine("         ,@UPDYMD         AS INITYMD")
        sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE DATAID = @DATAID;")

        sqlStat.AppendLine(" UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("    SET DELFLG = '" & CONST_FLAG_YES & "'")
        sqlStat.AppendLine("       ,UPDYMD    = @UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER   = @UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID = @UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE DATAID = @DATAID;")


        Dim modOtherUserOrderNo As New List(Of String) '他ユーザーに変更されたオーダーNoを保持

        '直近のデータを取得
        Dim currentDbDt As DataTable = GetOrderListDataTable()
        '整合性を保つためOrderNo,第n輸送 単位でトランザクションを行う対象の更新からオーダー番号をグループ化
        Dim orderKeyList = (From item In targetDataList Group By grpOrderNo = Convert.ToString(item("ORDERNO")),
                                                                 grpTransNo = Convert.ToString(item("TRANSNO"))
                                                            Into grp = Group
                            Select grpOrderNo, grpTransNo).ToList
        Dim orderNoList = (From item In orderKeyList Group By grpOrderNo = item.grpOrderNo
                           Into grp = Group Select grpOrderNo).ToList

        Dim procOrderNo As New Dictionary(Of String, String)
        'DB接続の生成
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            'オーダーNoのループ(トランザクションはオーダーNo単位(第n輸送単位ではない)
            For Each grpOrderNo In orderNoList
                Dim targetOrderKeyList = (From orderKeyItem In orderKeyList Where orderKeyItem.grpOrderNo = grpOrderNo).ToList
                'トランザクションの開始
                Using tran As SqlTransaction = sqlCon.BeginTransaction,
                  sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    '共通のパラメータ設定
                    With sqlCmd.Parameters
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    For Each orderKey In orderKeyList
                        Dim updateList = (From item In targetDataList Where item("ORDERNO").Equals(orderKey.grpOrderNo) AndAlso
                                                                        item("TRANSNO").Equals(orderKey.grpTransNo))

                        '第n輸送でエラーが他ユーザ更新があった場合は更新しない
                        If modOtherUserOrderNo.Contains(orderKey.grpOrderNo) Then
                            Continue For
                        End If
                        '各レコードで変化があるパラメータにつき変数を切る
                        Dim paramDataId As SqlParameter = sqlCmd.Parameters.Add("@DATAID", SqlDbType.NVarChar)
                        Dim paramAmountOrd As SqlParameter = sqlCmd.Parameters.Add("@AMOUNTORD", SqlDbType.Float)

                        For Each updateItem In updateList
                            '他ユーザー更新チェック
                            Dim diffDr = (From item In currentDbDt
                                          Where item("DATAID").Equals(updateItem.Item("DATAID")) _
                                    AndAlso item("TIMSTP").Equals(updateItem.Item("TIMSTP")))
                            '1件でもOrderNoにつき更新レコードがあればロールバックし次のオーダーNoへ
                            If diffDr.Any = False Then
                                tran.Rollback()
                                If Not modOtherUserOrderNo.Contains(orderKey.grpOrderNo) Then
                                    modOtherUserOrderNo.Add(orderKey.grpOrderNo)
                                End If
                                Exit For
                            End If
                            '動的パラメータに値を設定
                            paramDataId.Value = Convert.ToString(updateItem.Item("DATAID"))
                            paramAmountOrd.Value = updateItem.Item("AMOUNTORD")
                            'SQL実行
                            sqlCmd.ExecuteNonQuery()
                        Next 'EndUpdateItem
                        'オーガナイザレコードを追加する
                        Dim orderNo As String = orderKey.grpOrderNo
                        Dim transNo As String = orderKey.grpTransNo

                        Dim DifferenceAmountValues As List(Of DifferenceAmount) = (From item In differenceAmountList Where item.OrderNo = orderKey.grpOrderNo AndAlso
                                                                                                                              item.TransNo = orderKey.grpTransNo).ToList
                        EntryDifferenceAmount(orderKey.grpOrderNo, orderKey.grpTransNo, DifferenceAmountValues, balAmount, sqlCon, tran, procDate)

                    Next 'end 第n輸送ループ


                    ''TOTAL INVOICEの差分更新
                    'If dicDifferenceTotalInvoice.ContainsKey(grpOrderNo) AndAlso dicDifferenceTotalInvoice(grpOrderNo).Count > 0 Then
                    '    Dim dicTankNoDifference As Dictionary(Of String, Decimal) = dicDifferenceTotalInvoice(grpOrderNo)
                    '    UpdateDifferenceTotalInvoice(grpOrderNo, dicTankNoDifference, balAmount, sqlCon, tran, procDate)
                    'End If

                    If Not modOtherUserOrderNo.Contains(grpOrderNo) Then
                        tran.Commit() '他ユーザー更新でなければコミット
                    End If

                End Using 'End Tran Command

            Next 'end オーダーNoループ
        End Using 'End SQL Connection

        Dim messageNo As String = C_MESSAGENO.NORMALDBENTRY
        '他ユーザー更新データがあった場合はエラーメッセージを設定
        If modOtherUserOrderNo.Count > 0 Then
            messageNo = C_MESSAGENO.RIGHTBIXOUT
            Dim dummyLabel As New Label
            Dim errMessageNo As String = C_MESSAGENO.CANNOTUPDATE
            CommonFunctions.ShowMessage(errMessageNo, dummyLabel,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})
            rightMessage = dummyLabel.Text & ControlChars.CrLf
            For Each orderNo In modOtherUserOrderNo
                rightMessage = rightMessage & String.Format("OrderNo={0}", orderNo) & ControlChars.CrLf
            Next
        End If
        Return messageNo
    End Function
    '''' <summary>
    '''' 差分のTOTAL INVOICEを更新
    '''' </summary>
    'Private Sub UpdateDifferenceTotalInvoice(orderNo As String,
    '                                         dicTankNoDifference As Dictionary(Of String, Decimal),
    '                                         balAmount As List(Of String),
    '                                         ByRef sqlCon As SqlConnection,
    '                                         ByRef tran As SqlTransaction,
    '                                         procDate As String)
    '    Dim sqlStat As New StringBuilder
    '    Dim totalInvoiceCostCode As String = balAmount(2)
    '    '****************************************
    '    '* TOTAL INVOICED の編集
    '    '****************************************
    '    sqlStat.AppendLine("DECLARE @W_DATAID varchar(36);")
    '    sqlStat.AppendLine("DECLARE CUR_TOTALINVOICE CURSOR FOR")
    '    sqlStat.AppendLine(" SELECT CONVERT(varchar(36),DATAID) AS DATAID")
    '    sqlStat.AppendLine("   FROM GBT0005_ODR_VALUE")
    '    sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
    '    sqlStat.AppendLine("   AND COSTCODE  = @TOTALINVOICECOSTCODE")
    '    sqlStat.AppendLine("   AND DTLPOLPOD = @DTLPOLPOD") 'オーガナイザー
    '    sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
    '    sqlStat.AppendLine("   AND DELFLG   <> @DELFLG;")
    '    'sqlStat.AppendLine("   FOR UPDATE;")

    '    sqlStat.AppendLine("  OPEN CUR_TOTALINVOICE;")
    '    sqlStat.AppendLine(" FETCH NEXT FROM CUR_TOTALINVOICE")
    '    sqlStat.AppendLine(" INTO @W_DATAID;")

    '    'sqlStat.AppendLine(" WHILE @@FETCH_STATUS = 0")
    '    'sqlStat.AppendLine(" BEGIN")
    '    'TOTAL INVOICEDレコードのループ
    '    sqlStat.AppendLine("     INSERT INTO GBT0005_ODR_VALUE (")
    '    sqlStat.AppendLine("        ORDERNO")
    '    sqlStat.AppendLine("       ,TANKSEQ")
    '    sqlStat.AppendLine("       ,DTLPOLPOD")
    '    sqlStat.AppendLine("       ,DTLOFFICE")
    '    sqlStat.AppendLine("       ,TANKNO")
    '    sqlStat.AppendLine("       ,COSTCODE")
    '    sqlStat.AppendLine("       ,ACTIONID")
    '    sqlStat.AppendLine("       ,DISPSEQ")
    '    sqlStat.AppendLine("       ,LASTACT")
    '    sqlStat.AppendLine("       ,REQUIREDACT")
    '    sqlStat.AppendLine("       ,ORIGINDESTINATION")
    '    sqlStat.AppendLine("       ,COUNTRYCODE")
    '    sqlStat.AppendLine("       ,CURRENCYCODE")
    '    sqlStat.AppendLine("       ,AMOUNTBR")
    '    sqlStat.AppendLine("       ,AMOUNTORD")
    '    sqlStat.AppendLine("       ,CONTRACTORBR")
    '    sqlStat.AppendLine("       ,CONTRACTORODR")
    '    sqlStat.AppendLine("       ,SCHEDELDATEBR")
    '    sqlStat.AppendLine("       ,SCHEDELDATE")
    '    sqlStat.AppendLine("       ,LOCALBR")
    '    sqlStat.AppendLine("       ,LOCALRATE")
    '    sqlStat.AppendLine("       ,TAXBR")
    '    sqlStat.AppendLine("       ,INVOICEDBY")
    '    sqlStat.AppendLine("       ,REMARK")
    '    sqlStat.AppendLine("       ,BRID")
    '    sqlStat.AppendLine("       ,BRCOST")
    '    sqlStat.AppendLine("       ,AGENTORGANIZER")
    '    sqlStat.AppendLine("       ,DELFLG")
    '    sqlStat.AppendLine("       ,INITYMD")
    '    sqlStat.AppendLine("       ,UPDYMD")
    '    sqlStat.AppendLine("       ,UPDUSER")
    '    sqlStat.AppendLine("       ,UPDTERMID")
    '    sqlStat.AppendLine("       ,RECEIVEYMD")
    '    sqlStat.AppendLine("      )")
    '    sqlStat.AppendLine("     SELECT ")
    '    sqlStat.AppendLine("             OVL.ORDERNO")
    '    sqlStat.AppendLine("            ,OVL.TANKSEQ")
    '    sqlStat.AppendLine("            ,OVL.DTLPOLPOD") 'Organizer
    '    sqlStat.AppendLine("            ,OVL.DTLOFFICE")
    '    sqlStat.AppendLine("            ,OVL.TANKNO")
    '    sqlStat.AppendLine("            ,OVL.COSTCODE")
    '    sqlStat.AppendLine("            ,OVL.ACTIONID")
    '    sqlStat.AppendLine("            ,OVL.DISPSEQ")
    '    sqlStat.AppendLine("            ,OVL.LASTACT")
    '    sqlStat.AppendLine("            ,OVL.REQUIREDACT")
    '    sqlStat.AppendLine("            ,OVL.ORIGINDESTINATION")
    '    sqlStat.AppendLine("            ,OVL.COUNTRYCODE")
    '    sqlStat.AppendLine("            ,OVL.CURRENCYCODE")
    '    sqlStat.AppendLine("            ,OVL.AMOUNTBR")
    '    sqlStat.AppendLine("            ,(OVL.AMOUNTBR + @AMOUNTDIFF) AS AMOUNTORD")
    '    sqlStat.AppendLine("            ,OVL.CONTRACTORBR")
    '    sqlStat.AppendLine("            ,OVL.CONTRACTORODR")
    '    sqlStat.AppendLine("            ,OVL.SCHEDELDATEBR")
    '    sqlStat.AppendLine("            ,OVL.SCHEDELDATE")
    '    sqlStat.AppendLine("            ,OVL.LOCALBR")
    '    sqlStat.AppendLine("            ,OVL.LOCALRATE")
    '    sqlStat.AppendLine("            ,OVL.TAXBR")
    '    sqlStat.AppendLine("            ,OVL.INVOICEDBY")
    '    sqlStat.AppendLine("            ,OVL.REMARK")
    '    sqlStat.AppendLine("            ,OVL.BRID")
    '    sqlStat.AppendLine("            ,OVL.BRCOST") '削除させないため1を立てる
    '    sqlStat.AppendLine("            ,OVL.AGENTORGANIZER")
    '    sqlStat.AppendLine("            ,'" & CONST_FLAG_NO & "'                AS DELFLG")
    '    sqlStat.AppendLine("            ,@UPDYMD            AS INITYMD")
    '    sqlStat.AppendLine("            ,@UPDYMD            AS UPDYMD")
    '    sqlStat.AppendLine("            ,@UPDUSER           AS UPDUSER")
    '    sqlStat.AppendLine("            ,@UPDTERMID         AS UPDTERMID")
    '    sqlStat.AppendLine("            ,@RECEIVEYMD        AS RECEIVEYMD")
    '    sqlStat.AppendLine("      FROM GBT0005_ODR_VALUE OVL")
    '    sqlStat.AppendLine("     WHERE OVL.DATAID = @W_DATAID;")

    '    sqlStat.AppendLine("     UPDATE GBT0005_ODR_VALUE")
    '    sqlStat.AppendLine("        SET DELFLG = '" & CONST_FLAG_YES & "'")
    '    sqlStat.AppendLine("           ,UPDYMD    = @UPDYMD")
    '    sqlStat.AppendLine("           ,UPDUSER   = @UPDUSER")
    '    sqlStat.AppendLine("           ,UPDTERMID = @UPDTERMID")
    '    sqlStat.AppendLine("     WHERE DATAID    = @W_DATAID;")
    '    'sqlStat.AppendLine(" FETCH NEXT FROM CUR_TOTALINVOICE")
    '    'sqlStat.AppendLine(" INTO @W_DATAID;")
    '    'sqlStat.AppendLine(" END")
    '    sqlStat.AppendLine(" CLOSE CUR_TOTALINVOICE;")
    '    sqlStat.AppendLine(" DEALLOCATE CUR_TOTALINVOICE;")

    '    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
    '        With sqlCmd.Parameters
    '            '共通設定パラメータ設定
    '            .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
    '            .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = "Organizer"
    '            .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
    '            .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
    '            .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
    '            .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
    '            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
    '            .Add("@TOTALINVOICECOSTCODE", SqlDbType.NVarChar).Value = totalInvoiceCostCode

    '        End With
    '        Dim paramTankSeq As SqlParameter = sqlCmd.Parameters.Add("@TANKSEQ", SqlDbType.NVarChar)
    '        Dim paramAmountDiff As SqlParameter = sqlCmd.Parameters.Add("@AMOUNTDIFF", SqlDbType.Float)
    '        '追加時のみのパラメータ
    '        For Each dicTankNo As KeyValuePair(Of String, Decimal) In dicTankNoDifference
    '            paramTankSeq.Value = dicTankNo.Key
    '            paramAmountDiff.Value = dicTankNo.Value
    '            'SQL実行
    '            sqlCmd.ExecuteNonQuery()
    '        Next

    '    End Using

    'End Sub
    ''' <summary>
    ''' 案分の差額をオーガナイザレコードに格納する
    ''' </summary>
    ''' <param name="orderNo">オーダー番号</param>
    ''' <param name="transNo">第n輸送</param>
    ''' <param name="differenceAmountValues">上記引数のオーダー番号、第n輸送に合致する、差額・タンク番号・タンクシーケンス</param>
    ''' <param name="balAmount">FixVaueに設定した費用コード及び、Remak文言</param>
    ''' <param name="sqlCon">SQL接続オブジェクト</param>
    ''' <param name="tran">トランザクション</param>
    Private Sub EntryDifferenceAmount(orderNo As String, transNo As String,
                                      differenceAmountValues As List(Of DifferenceAmount),
                                      balAmount As List(Of String),
                                      ByRef sqlCon As SqlConnection,
                                      ByRef tran As SqlTransaction,
                                      procDate As String)
        Dim sqlStat As New StringBuilder

        Dim costCode As String = balAmount(0) 'FIXVALUEに設定した費目を設定
        Dim remarkValue As String = balAmount(1) & transNo 'FIXVALUEに設定した文言+第n輸送を付与し備考フィールドに設定
        For Each differenceAmountValue In differenceAmountValues
            '****************************************
            '* JOT Hirage の追加編集
            '****************************************
            '直近レコードに削除フラグを立てる
            sqlStat.Clear()
            sqlStat.AppendLine(" UPDATE GBT0005_ODR_VALUE")
            sqlStat.AppendLine("    Set DELFLG = '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("       ,UPDYMD    = @UPDYMD")
            sqlStat.AppendLine("       ,UPDUSER   = @UPDUSER")
            sqlStat.AppendLine("       ,UPDTERMID = @UPDTERMID")
            sqlStat.AppendLine("       ,RECEIVEYMD = @RECEIVEYMD")
            sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
            sqlStat.AppendLine("   AND REMARK    = @REMARK")
            sqlStat.AppendLine("   AND COSTCODE  = @COSTCODE")
            sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG;")
            '差額が0を超える場合はオーガナイザーレコードに差額費用を追加
            If differenceAmountValue.Amount > 0 Then
                sqlStat.AppendLine(" INSERT INTO GBT0005_ODR_VALUE(")
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
                sqlStat.AppendLine("       ,AMOUNTBR")
                sqlStat.AppendLine("       ,AMOUNTORD")
                sqlStat.AppendLine("       ,CONTRACTORBR")
                sqlStat.AppendLine("       ,CONTRACTORODR")
                sqlStat.AppendLine("       ,SCHEDELDATEBR")
                sqlStat.AppendLine("       ,SCHEDELDATE")
                sqlStat.AppendLine("       ,LOCALBR")
                sqlStat.AppendLine("       ,LOCALRATE")
                sqlStat.AppendLine("       ,TAXBR")
                sqlStat.AppendLine("       ,INVOICEDBY")
                sqlStat.AppendLine("       ,REMARK")
                sqlStat.AppendLine("       ,BRID")
                sqlStat.AppendLine("       ,BRCOST")
                sqlStat.AppendLine("       ,AGENTORGANIZER")
                sqlStat.AppendLine("       ,DELFLG")
                sqlStat.AppendLine("       ,INITYMD")
                sqlStat.AppendLine("       ,UPDYMD")
                sqlStat.AppendLine("       ,UPDUSER")
                sqlStat.AppendLine("       ,UPDTERMID")
                sqlStat.AppendLine("       ,RECEIVEYMD")
                sqlStat.AppendLine(" )")
                sqlStat.AppendLine("SELECT ")
                sqlStat.AppendLine("        OBS.ORDERNO")
                sqlStat.AppendLine("       ,@TANKSEQ           AS TANKSEQ")
                sqlStat.AppendLine("       ,@DTLPOLPOD         AS DTLPOLPOD") 'Organizer
                sqlStat.AppendLine("       ,OBS.INVOICEDBY     AS DTLOFFICE")
                sqlStat.AppendLine("       ,@TANKNO            AS TANKNO")
                sqlStat.AppendLine("       ,@COSTCODE          AS COSTCODE")
                sqlStat.AppendLine("       ,''                 AS ACTIONID")          '輸送パターンが特定できないのでなし
                sqlStat.AppendLine("       ,''                 AS DISPSEQ")           '輸送パターンが特定できないのでなし
                sqlStat.AppendLine("       ,''                 AS LASTACT")           '輸送パターンが特定できないのでなし
                sqlStat.AppendLine("       ,''                 AS REQUIREDACT")       '輸送パターンが特定できないのでなし
                sqlStat.AppendLine("       ,''                 AS ORIGINDESTINATION") '輸送パターンが特定できないのでなし
                sqlStat.AppendLine("       ,''                 AS COUNTRYCODE")
                'sqlStat.AppendLine("       ,ISNULL(TRD.COUNTRYCODE,'')    AS COUNTRYCODE")       'エージェントオーガナイザより特定
                sqlStat.AppendLine("       ,'" & GBC_CUR_USD & "'              AS CURRENCYCODE")      'エージェントオーガナイザより特定
                sqlStat.AppendLine("       ,@AMOUNT            AS AMOUNTBR")
                sqlStat.AppendLine("       ,@AMOUNT            AS AMOUNTORD")
                sqlStat.AppendLine("       ,''                 AS CONTRACTORBR")
                sqlStat.AppendLine("       ,''                 AS CONTRACTORODR")
                sqlStat.AppendLine("       ,'1900/01/01'       AS SCHEDELDATEBR")
                sqlStat.AppendLine("       ,'1900/01/01'       AS SCHEDELDATE")
                sqlStat.AppendLine("       ,0                  AS LOCALBR")
                sqlStat.AppendLine("       ,0                  AS LOCALRATE")
                sqlStat.AppendLine("       ,0                  AS TAXBR")
                sqlStat.AppendLine("       ,OBS.INVOICEDBY     AS INVOICEDBY")
                sqlStat.AppendLine("       ,@REMARK            AS REMARK")
                sqlStat.AppendLine("       ,OBS.BRID           AS BRID")
                sqlStat.AppendLine("       ,'1'                AS BRCOST") '削除させないため1を立てる
                sqlStat.AppendLine("       ,OBS.AGENTORGANIZER AS AGENTORGANIZER")
                sqlStat.AppendLine("       ,'" & CONST_FLAG_NO & "'                AS DELFLG")
                sqlStat.AppendLine("       ,@UPDYMD            AS INITYMD")
                sqlStat.AppendLine("       ,@UPDYMD            AS UPDYMD")
                sqlStat.AppendLine("       ,@UPDUSER           AS UPDUSER")
                sqlStat.AppendLine("       ,@UPDTERMID         AS UPDTERMID")
                sqlStat.AppendLine("       ,@RECEIVEYMD        AS RECEIVEYMD")
                sqlStat.AppendLine(" FROM GBT0004_ODR_BASE OBS")
                sqlStat.AppendLine(" LEFT JOIN (SELECT DISTINCT ")
                sqlStat.AppendLine("                   TRDS.COUNTRYCODE")
                sqlStat.AppendLine("                  ,TRDS.CARRIERCODE")
                sqlStat.AppendLine("                  ,CNTRY.CURRENCYCODE")
                sqlStat.AppendLine("              FROM GBM0005_TRADER TRDS")
                sqlStat.AppendLine("        INNER JOIN GBM0001_COUNTRY CNTRY")
                sqlStat.AppendLine("                ON CNTRY.COUNTRYCODE = TRDS.COUNTRYCODE")
                sqlStat.AppendLine("               AND CNTRY.COMPCODE = @COMPCODE")
                sqlStat.AppendLine("               AND CNTRY.DELFLG  <> @DELFLG")
                sqlStat.AppendLine("             WHERE TRDS.COMPCODE = @COMPCODE ")
                sqlStat.AppendLine("               AND TRDS.CLASS    = '" & C_TRADER.CLASS.AGENT & "'")
                sqlStat.AppendLine("               AND TRDS.DELFLG  <> @DELFLG")
                sqlStat.AppendLine("           ) TRD")
                sqlStat.AppendLine("        ON TRD.CARRIERCODE = OBS.AGENTORGANIZER")
                sqlStat.AppendLine("WHERE ORDERNO = @ORDERNO")
                sqlStat.AppendLine("  AND DELFLG <> @DELFLG")
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    '削除・追加両方で利用するパラメータ
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@COSTCODE", SqlDbType.NVarChar).Value = costCode
                    .Add("@REMARK", SqlDbType.NVarChar).Value = remarkValue
                    .Add("@TANKSEQ", SqlDbType.NVarChar).Value = differenceAmountValue.TankSeq
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    '追加時のみのパラメータ
                    If differenceAmountValue.Amount > 0 Then
                        .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = "Organizer"
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                        .Add("@TANKNO", SqlDbType.NVarChar).Value = differenceAmountValue.TankNo
                        .Add("@AMOUNT", SqlDbType.Float).Value = differenceAmountValue.Amount
                    End If
                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End With
            End Using

        Next

    End Sub

    Private Class DifferenceAmount
        Public Property OrderNo As String = ""
        Public Property TankSeq As String = ""
        Public Property TransNo As String = ""
        Public Property TankNo As String = ""
        Public Property Amount As Decimal = 0
    End Class
End Class