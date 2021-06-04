Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' 経理連携出力画面クラス
''' </summary>
Public Class GBT00023RESULT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00023R"   '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 12              'マウススクロール時の増分
    Private returnCode As String = String.Empty
    ''' <summary>
    ''' 現在表示している一覧表ファイル（国）
    ''' </summary>
    Private Const CONST_CURRENTXML_COUNTRY As String = "COUNTRY"
    ''' <summary>
    ''' 現在表示している一覧表ファイル（JOT）
    ''' </summary>
    Private Const CONST_CURRENTXML_JOT As String = "JOT"
    ''' <summary>
    ''' 帳票に追加する詳細シートのレポートID
    ''' </summary>
    Private Const CONST_DETAIL_REPORT_ID As String = "AccDetail"
    ''' <summary>
    ''' 帳票に追加する詳細シート名
    ''' </summary>
    Private Const CONST_DETAIL_REPORT_SHEETNAME As String = "Detail"
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    Private SavedDt As DataTable = Nothing
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
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFileCountry.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}Country.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, Me.hdnThisMapVariant.Value)
                Me.hdnXMLsaveFileJot.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}Jot.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, Me.hdnThisMapVariant.Value)
                '初期は国表示とする
                Me.hdnCurrentViewFile.Value = CONST_CURRENTXML_COUNTRY
                Me.hdnXMLsaveFile.Value = Me.hdnXMLsaveFileCountry.Value
                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()

                'レポート設定
                Dim retMessageNo As String = RightboxInit()
                If retMessageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(retMessageNo, Me.lblFooterMessage)
                    Return
                End If
                '****************************************
                '表示非表示制御
                '****************************************
                'DisplayControl()
                '****************************************
                'ヘッダー部初期値設定
                '****************************************
                InitValueSet()
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
                '一覧表作成
                '****************************************
                '一覧表データ取得（国初期表示分）
                Using dt As DataTable = Me.GetListDataTableCountry()

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
                    Me.SavedDt = dt
                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = hdnThisMapVariant.Value
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "2"
                        '.LEVENT = "ondblclick"
                        '.LFUNC = "ListDbClick"
                        .TITLEOPT = True
                        .NOCOLUMNWIDTHOPT = 50
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .USERSORTOPT = 1
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
                    'Dim checkedValue As Boolean
                End Using 'DataTable
                '一覧表データ取得（JOT初期表示分）
                Using dt As DataTable = Me.GetListDataTableJot()
                    'グリッド用データをファイルに退避
                    With Nothing
                        Dim COA0021ListTable As New COA0021ListTable
                        COA0021ListTable.FILEdir = Me.hdnXMLsaveFileJot.Value
                        COA0021ListTable.TBLDATA = dt
                        COA0021ListTable.COA0021saveListTable()
                        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                            Return
                        End If
                    End With

                End Using
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
                ''**********************
                '' 一覧表の行ダブルクリック判定
                ''**********************
                'If Me.hdnListDBclick.Value <> "" Then
                '    ListRowDbClick()
                '    Me.hdnListDBclick.Value = ""
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

            DisplayListObjEdit() '共通関数により描画された一覧の制御
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
                '部門
                Case Me.vLeftDepartment.ID
                    SetDepartmentListItem(Me.txtDepartment.Text)
                'ReportMonth
                Case Me.vLeftReportMonth.ID
                    SetReportMonthListItem(Me.txtReportMonth.Text)
                '経理円貨外貨区分
                Case Me.vLeftAccCurrencySegment.ID
                    Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                    Dim selectedRow As DataRow = (From item In Me.SavedDt Where Convert.ToString(item("LINECNT")) = lineCnt).FirstOrDefault
                    Dim selectedCode As String = Convert.ToString(selectedRow("ACCCURRENCYSEGMENT"))
                    SetAccCurrencySegmentListItem(selectedCode)
                '両建区分
                Case Me.vLeftBothClass.ID
                    Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                    Dim selectedRow As DataRow = (From item In Me.SavedDt Where Convert.ToString(item("LINECNT")) = lineCnt).FirstOrDefault
                    Dim selectedCode As String = Convert.ToString(selectedRow("BOTHCLASS"))
                    SetBothClassListItem(selectedCode)
                '取引先コード
                Case Me.vLeftToriCode.ID
                    Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                    Dim selectedRow As DataRow = (From item In Me.SavedDt Where Convert.ToString(item("LINECNT")) = lineCnt).FirstOrDefault
                    Dim targetFieldName As String = ""
                    Dim toriKbn As String = ""
                    If Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREAINCTORICODE") Then
                        targetFieldName = "INCTORICODE"
                        toriKbn = "I"
                    Else
                        targetFieldName = "EXPTORICODE"
                        toriKbn = "E"
                    End If
                    Dim selectedCode As String = Convert.ToString(selectedRow(targetFieldName))
                    Dim toriComp As String = Convert.ToString(selectedRow("TORICOMP"))
                    SetToriCodeListItem(toriComp, toriKbn, selectedCode)
                '入金・出金期日
                Case Me.vLeftPayDay.ID
                    Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                    Dim selectedRow As DataRow = (From item In Me.SavedDt Where Convert.ToString(item("LINECNT")) = lineCnt).FirstOrDefault
                    Dim targetFieldName As String = ""
                    If Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREADEPOSITDAY") Then
                        targetFieldName = "DEPOSITDAY"
                    Else
                        targetFieldName = "OVERDRAWDAY"
                    End If
                    Dim selectedCode As String = Convert.ToString(selectedRow(targetFieldName))
                    SetPayDayListItem(selectedCode)
                '休日フラグ
                Case Me.vLeftHolidayFlg.ID
                    Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                    Dim selectedRow As DataRow = (From item In Me.SavedDt Where Convert.ToString(item("LINECNT")) = lineCnt).FirstOrDefault
                    Dim selectedCode As String = Convert.ToString(selectedRow("HOLIDAYFLG"))
                    SetHolidayFlgListItem(selectedCode)
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
                Case Me.vLeftDepartment.ID 'アクティブなビューが承認
                    '部門選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbDepartment.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbDepartment.SelectedItem.Value
                            Me.lblDepartmentText.Text = Me.lbDepartment.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblDepartmentText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftReportMonth.ID
                    '精算月選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbReportMonth.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbReportMonth.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            txtobj.Focus()
                        End If
                        '年月に応じ対象の業者を再取得一覧表データ取得（JOT初期表示分）
                        Using dt As DataTable = Me.GetListDataTableJot()
                            'グリッド用データをファイルに退避
                            With Nothing
                                Dim COA0021ListTable As New COA0021ListTable
                                COA0021ListTable.FILEdir = Me.hdnXMLsaveFileJot.Value
                                COA0021ListTable.TBLDATA = dt
                                COA0021ListTable.COA0021saveListTable()
                                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                                    Return
                                End If
                            End With

                        End Using

                    End If
                Case Me.vLeftAccCurrencySegment.ID
                    '通貨セグメント
                    If Me.lbAccCurrencySegment.SelectedItem IsNot Nothing Then
                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        Dim targetTextField As String = Me.hdnTextDbClickField.Value
                        UpdateDataTable(Me.lbAccCurrencySegment.SelectedValue, lineCnt, "ACCCURRENCYSEGMENT")
                    End If
                Case Me.vLeftBothClass.ID
                    '両建区分
                    If Me.lbBothClass.SelectedItem IsNot Nothing Then
                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        Dim targetTextField As String = Me.hdnTextDbClickField.Value
                        UpdateDataTable(Me.lbBothClass.SelectedValue, lineCnt, "BOTHCLASS")
                    End If
                Case Me.vLeftToriCode.ID
                    '取引先コード
                    If Me.lbToriCode.SelectedItem IsNot Nothing Then
                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        Dim targetTextField As String = Me.hdnTextDbClickField.Value
                        Dim targetFieldName As String = ""
                        If Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREAINCTORICODE") Then
                            targetFieldName = "INCTORICODE"
                        Else
                            targetFieldName = "EXPTORICODE"
                        End If
                        UpdateDataTable(Me.lbToriCode.SelectedValue, lineCnt, targetFieldName)
                    End If
                Case Me.vLeftPayDay.ID
                    '期日
                    If Me.lbPayDay.SelectedItem IsNot Nothing Then
                        Dim targetFieldName As String = ""
                        If Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREADEPOSITDAY") Then
                            targetFieldName = "DEPOSITDAY"
                        Else
                            targetFieldName = "OVERDRAWDAY"
                        End If

                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        Dim targetTextField As String = Me.hdnTextDbClickField.Value
                        UpdateDataTable(Me.lbPayDay.SelectedValue, lineCnt, targetFieldName)
                    End If
                Case Me.vLeftHolidayFlg.ID
                    '休日フラグ
                    If Me.lbHolidayFlg.SelectedItem IsNot Nothing Then
                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        Dim targetTextField As String = Me.hdnTextDbClickField.Value
                        UpdateDataTable(Me.lbHolidayFlg.SelectedValue, lineCnt, "HOLIDAYFLG")
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
        Dim dt As DataTable = New DataTable

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
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.lblDepartment, "部門", "Department")
        AddLangSetting(dicDisplayText, Me.lblReportMonth, "出力月", "Report Month")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)

    End Sub
    ''' <summary>
    ''' 一覧表のデータテーブルを取得する関数(一覧表示（国））
    ''' </summary>
    ''' <returns></returns>
    Private Function GetListDataTableCountry() As DataTable
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        'ソート順取得
        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = Me.hdnThisMapVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder
        Dim retDt As New DataTable
        Dim fixDispTextField As String = "VALUE2"
        Dim cntyDispTextField As String = "NAMES"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            fixDispTextField = "VALUE1"
            cntyDispTextField = "NAMESJP"
        End If



        '承認情報取得
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       '' AS OPERATION")
        sqlStat.AppendLine("      ,'' AS TIMSTP")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,CT.COUNTRYCODE AS COUNTRYCODE ")
        sqlStat.AppendFormat("      ,ISNULL(CT.{0},'') AS COUNTRYNAME", cntyDispTextField).AppendLine()
        sqlStat.AppendLine("      ,'' AS [PRINT] ")
        sqlStat.AppendLine("      ,CT.ACCCURRENCYSEGMENT AS ACCCURRENCYSEGMENT ")
        sqlStat.AppendFormat("      ,ISNULL(F_CURSEG.{0},'') AS ACCCURRENCYSEGMENTNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,CT.BOTHCLASS AS BOTHCLASS ")
        sqlStat.AppendFormat("      ,ISNULL(F_BCLS.{0},'') AS BOTHCLASSNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,CT.TORICOMP AS TORICOMP ")
        sqlStat.AppendLine("      ,CT.INCTORICODE       AS INCTORICODE ")
        'sqlStat.AppendLine("      ,ISNULL(INCTORI.NAMES,'') AS INCTORICODENAME ")
        sqlStat.AppendLine("      ,ISNULL(INCTORI.NAMES1,'') AS INCTORICODENAME ")
        sqlStat.AppendLine("      ,CT.EXPTORICODE       AS EXPTORICODE ")
        'sqlStat.AppendLine("      ,ISNULL(EXPTORI.NAMES,'') AS EXPTORICODENAME ")
        sqlStat.AppendLine("      ,ISNULL(EXPTORI.NAMES1,'') AS EXPTORICODENAME ")
        sqlStat.AppendLine("      ,CT.DEPOSITDAY AS DEPOSITDAY")
        sqlStat.AppendFormat("      ,ISNULL(F_DD.{0},'') AS DEPOSITDAYNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,CONVERT(nvarchar,CT.DEPOSITADDMM) AS DEPOSITADDMM")
        sqlStat.AppendLine("      ,CT.OVERDRAWDAY AS OVERDRAWDAY")
        sqlStat.AppendFormat("      ,ISNULL(F_OD.{0},'') AS OVERDRAWDAYNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,CONVERT(nvarchar,CT.OVERDRAWADDMM) AS OVERDRAWADDMM")
        sqlStat.AppendLine("      ,CT.HOLIDAYFLG AS HOLIDAYFLG")
        sqlStat.AppendFormat("      ,ISNULL(F_HD.{0},'') AS HOLIDAYFLGNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,CT.ISTOPSORT")
        sqlStat.AppendLine("      ,'0' AS ISREFCUSTOMERMST") 'JOT分のレコードとフィールドを合わせるために存在
        sqlStat.AppendLine("  FROM (          SELECT 0 AS ISTOPSORT,COMPCODE,COUNTRYCODE,NAMESJP,NAMES,ACCCURRENCYSEGMENT,BOTHCLASS,TORICOMP,INCTORICODE,EXPTORICODE,DEPOSITDAY,DEPOSITADDMM,OVERDRAWDAY,OVERDRAWADDMM,HOLIDAYFLG,STYMD,ENDYMD,DELFLG FROM GBM0001_COUNTRY")
        sqlStat.AppendLine("        UNION ALL SELECT 1 AS ISTOPSORT, @COMPCODE,'" & GBC_JOT_SOA_COUNTRY & "' AS COUNTRYCODE,'" & GBC_JOT_SOA_COUNTRY & "','" & GBC_JOT_SOA_COUNTRY & "','','','','','','','','','','',@STYMD AS STYMD,@ENDYMD AS ENDYMD,'" & CONST_FLAG_NO & "' AS DELFLG ) CT")
        'CurrencySecment名JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_CURSEG")
        sqlStat.AppendLine("        ON F_CURSEG.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_CURSEG.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_CURSEG.CLASS    = 'ACCCURRENCYSEGMENT'")
        sqlStat.AppendLine("       AND F_CURSEG.KEYCODE  = CT.ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("       AND F_CURSEG.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_CURSEG.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_CURSEG.DELFLG  <> @DELFLG ")
        '両建区分名 JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_BCLS")
        sqlStat.AppendLine("        ON F_BCLS.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_BCLS.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_BCLS.CLASS    = 'BOTHCLASS'")
        sqlStat.AppendLine("       AND F_BCLS.KEYCODE  = CT.BOTHCLASS")
        sqlStat.AppendLine("       AND F_BCLS.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_BCLS.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_BCLS.DELFLG  <> @DELFLG ")
        '取引先コード（収入）名称取得用
        sqlStat.AppendLine(" LEFT JOIN GBM0025_TORI INCTORI")
        sqlStat.AppendLine("        ON INCTORI.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("       AND INCTORI.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND INCTORI.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND INCTORI.DELFLG  <> @DELFLG ")
        sqlStat.AppendLine("       AND INCTORI.TORICODE = CT.INCTORICODE")
        sqlStat.AppendLine("       AND INCTORI.TORIKBN  = 'I'")
        '取引先コード（費用）名称取得用
        sqlStat.AppendLine(" LEFT JOIN GBM0025_TORI EXPTORI")
        sqlStat.AppendLine("        ON EXPTORI.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("       AND EXPTORI.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND EXPTORI.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND EXPTORI.DELFLG  <> @DELFLG ")
        sqlStat.AppendLine("       AND EXPTORI.TORICODE = CT.EXPTORICODE")
        sqlStat.AppendLine("       AND EXPTORI.TORIKBN  = 'E'")
        'DEPOSITDAY名 JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_DD")
        sqlStat.AppendLine("        ON F_DD.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_DD.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_DD.CLASS    = 'PAYDAY'")
        sqlStat.AppendLine("       AND F_DD.KEYCODE  = CT.DEPOSITDAY")
        sqlStat.AppendLine("       AND F_DD.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_DD.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_DD.DELFLG  <> @DELFLG ")
        'OVERDRAWDAY名 JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_OD")
        sqlStat.AppendLine("        ON F_OD.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_OD.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_OD.CLASS    = 'PAYDAY'")
        sqlStat.AppendLine("       AND F_OD.KEYCODE  = CT.OVERDRAWDAY")
        sqlStat.AppendLine("       AND F_OD.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_OD.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_OD.DELFLG  <> @DELFLG ")
        'HOLIDAYFLG名 JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_HD")
        sqlStat.AppendLine("        ON F_HD.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_HD.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_HD.CLASS    = 'HOLIDAYFLG'")
        sqlStat.AppendLine("       AND F_HD.KEYCODE  = CT.HOLIDAYFLG")
        sqlStat.AppendLine("       AND F_HD.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_HD.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_HD.DELFLG  <> @DELFLG ")
        '全体条件
        sqlStat.AppendLine(" WHERE CT.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("   And CT.STYMD  <= @STYMD")
        sqlStat.AppendLine("   And CT.ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("   And CT.DELFLG <> @DELFLG ")

        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@COMPCODE_D", SqlDbType.NVarChar).Value = GBC_COMPCODE_D
                .Add("@SYSCODE", SqlDbType.NVarChar).Value = C_SYSCODE_GB
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        Return retDt
    End Function
    ''' <summary>
    ''' 一覧表のデータテーブルを取得する関数(一覧表示（JOT））
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>SQLは仮中の仮</remarks>
    Private Function GetListDataTableJot() As DataTable
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        'ソート順取得
        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = Me.hdnThisMapVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder
        Dim retDt As New DataTable
        Dim fixDispTextField As String = "VALUE2"
        Dim cntyDispTextField As String = "NAMES"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            fixDispTextField = "VALUE1"
            cntyDispTextField = "NAMESJP"
        End If
        '承認情報取得
        sqlStat.AppendLine("With W_JOTAGENT As (") 'START 
        sqlStat.AppendLine("   Select TR.CARRIERCODE")
        sqlStat.AppendLine("     FROM GBM0005_TRADER TR")
        sqlStat.AppendLine("    WHERE TR.STYMD  <= @STYMD")
        sqlStat.AppendLine("      And TR.ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("      And TR.DELFLG <> @DELFLG")
        sqlStat.AppendLine("      And EXISTS (Select 1")
        sqlStat.AppendLine("                    FROM COS0017_FIXVALUE FXV")
        sqlStat.AppendLine("                   WHERE FXV.COMPCODE   = 'Default'")
        sqlStat.AppendLine("                     AND FXV.SYSCODE    = 'GB'")
        sqlStat.AppendLine("                     AND FXV.CLASS      = 'JOTCOUNTRYORG'")
        sqlStat.AppendLine("                     AND FXV.KEYCODE     = TR.MORG")
        sqlStat.AppendLine("                     AND FXV.STYMD     <= @STYMD")
        sqlStat.AppendLine("                     AND FXV.ENDYMD    >= @ENDYMD")
        sqlStat.AppendLine("                     AND FXV.DELFLG    <> @DELFLG")
        sqlStat.AppendLine("                 )")
        sqlStat.AppendLine(")")
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       '' AS OPERATION")
        sqlStat.AppendLine("      ,'' AS TIMSTP")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,CT.CONTRACTORFIX    AS COUNTRYCODE ")
        sqlStat.AppendFormat("      ,ISNULL(CT.{0},'') AS COUNTRYNAME", cntyDispTextField).AppendLine()
        sqlStat.AppendLine("      ,'' AS [PRINT] ")
        sqlStat.AppendLine("      ,ACCCURRENCYSEGMENT AS ACCCURRENCYSEGMENT ")
        sqlStat.AppendFormat("      ,ISNULL(F_CURSEG.{0},'') AS ACCCURRENCYSEGMENTNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,BOTHCLASS AS BOTHCLASS ")
        sqlStat.AppendFormat("      ,ISNULL(F_BCLS.{0},'') AS BOTHCLASSNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,CT.TORICOMP AS TORICOMP ")
        sqlStat.AppendLine("      ,CT.INCTORICODE       AS INCTORICODE ")
        'sqlStat.AppendLine("      ,ISNULL(INCTORI.NAMES,'') AS INCTORICODENAME ")
        sqlStat.AppendLine("      ,ISNULL(INCTORI.NAMES1,'') AS INCTORICODENAME ")
        sqlStat.AppendLine("      ,CT.EXPTORICODE       AS EXPTORICODE ")
        'sqlStat.AppendLine("      ,ISNULL(EXPTORI.NAMES,'') AS EXPTORICODENAME ")
        sqlStat.AppendLine("      ,ISNULL(EXPTORI.NAMES1,'') AS EXPTORICODENAME ")
        sqlStat.AppendLine("      ,CT.DEPOSITDAY AS DEPOSITDAY")
        sqlStat.AppendFormat("      ,ISNULL(F_DD.{0},'') AS DEPOSITDAYNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,CONVERT(nvarchar,CT.DEPOSITADDMM) AS DEPOSITADDMM")
        sqlStat.AppendLine("      ,CT.OVERDRAWDAY AS OVERDRAWDAY")
        sqlStat.AppendFormat("      ,ISNULL(F_OD.{0},'') AS OVERDRAWDAYNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,CONVERT(nvarchar,CT.OVERDRAWADDMM) AS OVERDRAWADDMM")
        sqlStat.AppendLine("      ,CT.HOLIDAYFLG AS HOLIDAYFLG ")
        sqlStat.AppendFormat("      ,ISNULL(F_HD.{0},'') AS HOLIDAYFLGNAME ", fixDispTextField).AppendLine()
        sqlStat.AppendLine("      ,ISTOPSORT AS ISTOPSORT")
        sqlStat.AppendLine("      ,CT.ISREFCUSTOMERMST AS ISREFCUSTOMERMST")
        sqlStat.AppendLine("  FROM (          SELECT distinct 0 AS ISTOPSORT")
        sqlStat.AppendLine("                         ,VL.CONTRACTORFIX ")
        sqlStat.AppendLine("                         ,COALESCE(CUS.NAMES   ,TR.NAMESJP,DP.NAMESJP,'') AS NAMESJP")
        sqlStat.AppendLine("                         ,COALESCE(CUS.NAMESEN ,TR.NAMES,DP.NAMES,'')     AS NAMES")
        sqlStat.AppendLine("                         ,COALESCE(CUS.ACCCURRENCYSEGMENT ,TR.ACCCURRENCYSEGMENT,DP.ACCCURRENCYSEGMENT,'')  AS ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("                         ,COALESCE(CUS.BOTHCLASS ,TR.BOTHCLASS,DP.BOTHCLASS,'')              AS BOTHCLASS")
        sqlStat.AppendLine("                         ,COALESCE(CUS.TORICOMP ,TR.TORICOMP,DP.TORICOMP,'')                 AS TORICOMP")
        sqlStat.AppendLine("                         ,COALESCE(CUS.INCTORICODE ,TR.INCTORICODE,DP.INCTORICODE,'')        AS INCTORICODE")
        sqlStat.AppendLine("                         ,COALESCE(CUS.EXPTORICODE ,TR.EXPTORICODE,DP.EXPTORICODE,'')        AS EXPTORICODE")
        sqlStat.AppendLine("                         ,COALESCE(CUS.DEPOSITDAY ,TR.DEPOSITDAY,DP.DEPOSITDAY,'')           AS DEPOSITDAY")
        sqlStat.AppendLine("                         ,COALESCE(CUS.DEPOSITADDMM ,TR.DEPOSITADDMM,DP.DEPOSITADDMM,'')     AS DEPOSITADDMM")
        sqlStat.AppendLine("                         ,COALESCE(CUS.OVERDRAWDAY ,TR.OVERDRAWDAY,DP.OVERDRAWDAY,'')        AS OVERDRAWDAY")
        sqlStat.AppendLine("                         ,COALESCE(CUS.OVERDRAWADDMM ,TR.OVERDRAWADDMM,DP.OVERDRAWADDMM,'')  AS OVERDRAWADDMM")
        sqlStat.AppendLine("                         ,COALESCE(CUS.HOLIDAYFLG ,TR.HOLIDAYFLG,DP.HOLIDAYFLG,'')           AS HOLIDAYFLG")
        sqlStat.AppendLine("                         ,CASE WHEN CUS.CUSTOMERCODE IS NULL THEN '0' ELSE '1' END           AS ISREFCUSTOMERMST")
        sqlStat.AppendLine("                    FROM GBT0008_JOTSOA_VALUE VL")
        sqlStat.AppendLine("                    LEFT JOIN (SELECT CSTS.COSTCODE") '費用判定（顧客マスタと紐づけるか業者、デポと紐づけるか）
        sqlStat.AppendLine("                                 FROM GBM0010_CHARGECODE CSTS")
        sqlStat.AppendLine("                                WHERE CSTS.STYMD  <= @STYMD")
        sqlStat.AppendLine("                                  AND CSTS.ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("                                  AND CSTS.DELFLG <> @DELFLG")
        sqlStat.AppendLine("                                  AND CSTS.CLASS2 <> ''")
        sqlStat.AppendLine("                              GROUP BY CSTS.COSTCODE")
        sqlStat.AppendLine("                              ) CST")
        sqlStat.AppendLine("                           ON CST.COSTCODE = VL.COSTCODE")
        sqlStat.AppendLine("                    LEFT JOIN GBM0004_CUSTOMER CUS")
        sqlStat.AppendLine("                           ON CUS.CUSTOMERCODE = VL.CONTRACTORFIX")
        sqlStat.AppendLine("                          AND CUS.STYMD  <= @STYMD")
        sqlStat.AppendLine("                          AND CUS.ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("                          AND CUS.DELFLG <> @DELFLG")
        sqlStat.AppendLine("                          AND CST.COSTCODE IS NOT NULL") '顧客マスタと紐づけるべきもの
        sqlStat.AppendLine("                    LEFT JOIN GBM0005_TRADER TR")
        sqlStat.AppendLine("                           ON TR.CARRIERCODE = VL.CONTRACTORFIX")
        sqlStat.AppendLine("                          AND TR.STYMD  <= @STYMD")
        sqlStat.AppendLine("                          AND TR.ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("                          AND TR.DELFLG <> @DELFLG")
        sqlStat.AppendLine("                          AND CST.COSTCODE IS NULL") '顧客マスタと紐づけるないもの
        sqlStat.AppendLine("                    LEFT JOIN GBM0003_DEPOT DP")
        sqlStat.AppendLine("                           ON DP.DEPOTCODE = VL.CONTRACTORFIX")
        sqlStat.AppendLine("                          AND DP.STYMD  <= @STYMD")
        sqlStat.AppendLine("                          AND DP.ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("                          AND DP.DELFLG <> @DELFLG")
        sqlStat.AppendLine("                          AND CST.COSTCODE IS NULL") '顧客マスタと紐づけるないもの
        sqlStat.AppendLine("                   WHERE VL.STYMD  <= @STYMD") '仮置きこの抽出条件は必ず見直す！たまればたまるほど重くなる
        sqlStat.AppendLine("                     AND VL.ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("                     AND VL.REPORTMONTH  = @REPORTMONTH")
        sqlStat.AppendLine("                     AND VL.CLOSINGMONTH = @REPORTMONTH")
        sqlStat.AppendLine("                     AND VL.DELFLG <> @DELFLG")
        sqlStat.AppendLine("                     AND VL.INVOICEDBY  IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) ")
        sqlStat.AppendLine("                     AND COALESCE(CUS.CUSTOMERCODE,TR.CARRIERCODE,DP.DEPOTCODE,'') <> '' ")
        sqlStat.AppendLine("        UNION ALL SELECT 1 AS ISTOPSORT,'" & GBC_JOT_SOA_COUNTRY & "' ,'国一覧','Country List','','','','','','','','','','','') CT")
        '文言用JOIN
        'CurrencySecment名JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_CURSEG")
        sqlStat.AppendLine("        ON F_CURSEG.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_CURSEG.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_CURSEG.CLASS    = 'ACCCURRENCYSEGMENT'")
        sqlStat.AppendLine("       AND F_CURSEG.KEYCODE  = CT.ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("       AND F_CURSEG.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_CURSEG.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_CURSEG.DELFLG  <> @DELFLG ")
        '両建区分名 JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_BCLS")
        sqlStat.AppendLine("        ON F_BCLS.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_BCLS.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_BCLS.CLASS    = 'BOTHCLASS'")
        sqlStat.AppendLine("       AND F_BCLS.KEYCODE  = CT.BOTHCLASS")
        sqlStat.AppendLine("       AND F_BCLS.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_BCLS.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_BCLS.DELFLG  <> @DELFLG ")
        '取引先名(収入） JOIN
        sqlStat.AppendLine(" LEFT JOIN GBM0025_TORI INCTORI")
        sqlStat.AppendLine("       ON INCTORI.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("      AND INCTORI.TORICODE = CT.INCTORICODE")
        sqlStat.AppendLine("      AND INCTORI.STYMD   <= @STYMD")
        sqlStat.AppendLine("      AND INCTORI.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("      AND INCTORI.DELFLG  <> @DELFLG ")
        sqlStat.AppendLine("      AND INCTORI.TORIKBN  = 'I' ")
        '取引先名(費用） JOIN
        sqlStat.AppendLine(" LEFT JOIN GBM0025_TORI EXPTORI")
        sqlStat.AppendLine("       ON EXPTORI.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("      AND EXPTORI.TORICODE = CT.EXPTORICODE")
        sqlStat.AppendLine("      AND EXPTORI.STYMD   <= @STYMD")
        sqlStat.AppendLine("      AND EXPTORI.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("      AND EXPTORI.DELFLG  <> @DELFLG ")
        sqlStat.AppendLine("      AND EXPTORI.TORIKBN  = 'E' ")
        'DEPOSITDAY名 JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_DD")
        sqlStat.AppendLine("        ON F_DD.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_DD.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_DD.CLASS    = 'PAYDAY'")
        sqlStat.AppendLine("       AND F_DD.KEYCODE  = CT.DEPOSITDAY")
        sqlStat.AppendLine("       AND F_DD.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_DD.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_DD.DELFLG  <> @DELFLG ")
        'OVERDRAWDAY名 JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_OD")
        sqlStat.AppendLine("        ON F_OD.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_OD.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_OD.CLASS    = 'PAYDAY'")
        sqlStat.AppendLine("       AND F_OD.KEYCODE  = CT.OVERDRAWDAY")
        sqlStat.AppendLine("       AND F_OD.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_OD.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_OD.DELFLG  <> @DELFLG ")
        'HOLIDAYFLG名 JOIN
        sqlStat.AppendLine(" LEFT JOIN COS0017_FIXVALUE F_HD")
        sqlStat.AppendLine("        ON F_HD.COMPCODE = @COMPCODE_D")
        sqlStat.AppendLine("       AND F_HD.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("       AND F_HD.CLASS    = 'HOLIDAYFLG'")
        sqlStat.AppendLine("       AND F_HD.KEYCODE  = CT.HOLIDAYFLG")
        sqlStat.AppendLine("       AND F_HD.STYMD   <= @STYMD")
        sqlStat.AppendLine("       AND F_HD.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("       AND F_HD.DELFLG  <> @DELFLG ")

        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        '
        Dim reportMonth As String = ""

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@COMPCODE_D", SqlDbType.NVarChar).Value = GBC_COMPCODE_D
                .Add("@SYSCODE", SqlDbType.NVarChar).Value = C_SYSCODE_GB
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                Dim strRepMonth As String = "1900/01"
                If Me.lbReportMonth.Items.Count > 0 Then
                    Dim findResult = Me.lbReportMonth.Items.FindByText(Me.txtReportMonth.Text)
                    If findResult IsNot Nothing Then
                        strRepMonth = findResult.Value
                    End If
                End If

                .Add("@REPORTMONTH", System.Data.SqlDbType.NVarChar).Value = strRepMonth
            End With

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
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル

        '表示データ件数取得
        Dim DataCnt As Integer = 0                  '(絞り込み後)有効Data数

        '一覧表示データ復元
        Dim dt As DataTable = New DataTable

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
        If ListPosition <= 0 Then
            ListPosition = 1
        End If

        'ソート
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, ListPosition, hdnListPosition)
        Dim qhasTopRow = From item In listData Where item("ISTOPSORT").Equals(1)
        Dim copyItem As Object() = Nothing
        Dim removeIdx As Integer = -1
        If qhasTopRow.Any Then
            Dim drTop As DataRow = qhasTopRow(0)
            Dim idx = listData.Rows.IndexOf(drTop)
            If idx <> 0 Then
                removeIdx = idx
            End If
            copyItem = drTop.ItemArray
        Else
            'If listData.Rows.Count > 1 Then
            '    removeIdx = listData.Rows.Count
            'End If
            Dim qTopRow = From item In dt Where item("ISTOPSORT").Equals(1)
            If qTopRow.Any Then
                copyItem = qTopRow(0).ItemArray
            End If
        End If
        If removeIdx <> -1 Then
            listData.Rows.RemoveAt(removeIdx)
            Dim nRow = listData.NewRow()
            nRow.ItemArray = copyItem
            listData.Rows.InsertAt(nRow, 0)
        End If

        '一覧作成
        COA0013TableObject.MAPID = CONST_MAPID
        COA0013TableObject.VARI = Me.hdnThisMapVariant.Value
        COA0013TableObject.SRCDATA = listData
        COA0013TableObject.TBLOBJ = Me.WF_LISTAREA
        COA0013TableObject.SCROLLTYPE = "2"
        'COA0013TableObject.LEVENT = "ondblclick"
        'COA0013TableObject.LFUNC = "ListDbClick"
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.NOCOLUMNWIDTHOPT = 50
        COA0013TableObject.OPERATIONCOLUMNWIDTHOPT = -1
        COA0013TableObject.USERSORTOPT = 1
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""
        '1.現在表示しているLINECNTのリストをビューステートに保持
        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If

    End Sub
    ''' <summary>
    ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>もはや不要（デッドロジックの為、しばらくしたら削除）</remarks>
    Private Function CreateDataTable() As DataTable
        Dim retDt As New DataTable
        '共通項目
        retDt.Columns.Add("LINECNT", GetType(Integer))              'DBの固定フィールド
        retDt.Columns.Add("OPERATION", GetType(String))             'DBの固定フィールド
        retDt.Columns.Add("TIMSTP", GetType(String))                'DBの固定フィールド
        retDt.Columns.Add("SELECT", GetType(Integer))               'DBの固定フィールド
        retDt.Columns.Add("HIDDEN", GetType(Integer))

        '個別項目
        retDt.Columns.Add("DATAID", GetType(String))                'データID
        retDt.Columns.Add("ORDERNO", GetType(String))               '受注番号
        retDt.Columns.Add("STYMD", GetType(String))                 '有効開始日
        retDt.Columns.Add("ENDYMD", GetType(String))                '有効終了日
        retDt.Columns.Add("TANKSEQ", GetType(String))               '作業番号(タンクSEQ)
        retDt.Columns.Add("DTLPOLPOD", GetType(String))             '発地着地区分
        retDt.Columns.Add("DTLOFFICE", GetType(String))             '代理店
        retDt.Columns.Add("TANKNO", GetType(String))                'タンク番号
        retDt.Columns.Add("COSTCODE", GetType(String))              '費用コード
        retDt.Columns.Add("ACTIONID", GetType(String))              'アクションコード
        retDt.Columns.Add("DISPSEQ", GetType(String))               '表示順番
        retDt.Columns.Add("LASTACT", GetType(String))               '輸送完了作業
        retDt.Columns.Add("REQUIREDACT", GetType(String))           '必須作業
        retDt.Columns.Add("ORIGINDESTINATION", GetType(String))     '起点終点
        retDt.Columns.Add("COUNTRYCODE", GetType(String))           '国コード
        retDt.Columns.Add("CURRENCYCODE", GetType(String))          '通貨換算コード
        retDt.Columns.Add("TAXATION", GetType(String))              '課税フラグ
        retDt.Columns.Add("AMOUNTBR", GetType(String))              '金額(BR)
        retDt.Columns.Add("AMOUNTORD", GetType(String))             '金額(ORD)
        retDt.Columns.Add("AMOUNTFIX", GetType(String))             '金額(FIX)
        retDt.Columns.Add("CONTRACTORBR", GetType(String))          '業者コード(BR)
        retDt.Columns.Add("CONTRACTORODR", GetType(String))         '業者コード(ORD)
        retDt.Columns.Add("CONTRACTORFIX", GetType(String))         '業者コード(FIX)
        retDt.Columns.Add("SCHEDELDATEBR", GetType(String))         '作業日(BR) 
        retDt.Columns.Add("SCHEDELDATE", GetType(String))           '作業日(ORD)
        retDt.Columns.Add("ACTUALDATE", GetType(String))            '作業日(FIX)
        retDt.Columns.Add("LOCALBR", GetType(String))               '現地金額(BR)
        retDt.Columns.Add("LOCALRATE", GetType(String))             '現地通貨換算レート
        retDt.Columns.Add("TAXBR", GetType(String))                 '税(BR)
        retDt.Columns.Add("AMOUNTPAY", GetType(String))             '金額(PAY)
        retDt.Columns.Add("LOCALPAY", GetType(String))              '現地金額(PAY)
        retDt.Columns.Add("TAXPAY", GetType(String))                '税(PAY)
        retDt.Columns.Add("INVOICEDBY", GetType(String))            '船荷証券発行コード
        retDt.Columns.Add("APPLYID", GetType(String))               '費用変更申請ID
        retDt.Columns.Add("APPLYTEXT", GetType(String))             '申請コメント
        retDt.Columns.Add("LASTSTEP", GetType(String))              '最終承認STEP
        retDt.Columns.Add("SOAAPPDATE", GetType(String))            'SOA締日付
        retDt.Columns.Add("REMARK", GetType(String))                '所見
        retDt.Columns.Add("BLID", GetType(String))                  'BL番号
        retDt.Columns.Add("BLAPPDATE", GetType(String))             'BL承認日
        retDt.Columns.Add("BRID", GetType(String))                  'ブレーカーID
        retDt.Columns.Add("BRCOST", GetType(String))                'ブレーカー起因費用
        retDt.Columns.Add("DATEFIELD", GetType(String))             '予定日付参照
        retDt.Columns.Add("DATEINTERVAL", GetType(String))          '予定日付加減算日数
        retDt.Columns.Add("BRADDEDCOST", GetType(String))           'ブレーカーコスト追加フラグ
        retDt.Columns.Add("AGENTORGANIZER", GetType(String))        'オーガナイザーエージェント
        retDt.Columns.Add("DELFLG", GetType(String))                '削除フラグ
        retDt.Columns.Add("REPORTMONTH", GetType(String))            '出力月
        retDt.Columns.Add("SOACODE", GetType(String))                'SOAコード
        retDt.Columns.Add("COUNTRYNAME", GetType(String))           '国名
        retDt.Columns.Add("APPROVALOBJECT", GetType(String))        '承認対象(通常、代行、SKIP)
        retDt.Columns.Add("APPROVALORREJECT", GetType(String))      '承認or否認
        retDt.Columns.Add("CHECK", GetType(String))                 'チェック
        retDt.Columns.Add("STEP", GetType(String))                  'ステップ
        retDt.Columns.Add("STATUS", GetType(String))                'ステータス
        retDt.Columns.Add("CURSTEP", GetType(String))               '承認ステップ
        retDt.Columns.Add("APPROVALTYPE", GetType(String))          '承認区分
        retDt.Columns.Add("APPROVERID", GetType(String))
        retDt.Columns.Add("APPLYOFFICE", GetType(String))
        retDt.Columns.Add("OFFICENAME", GetType(String))
        retDt.Columns.Add("APPLYUSER", GetType(String))
        retDt.Columns.Add("EVENTCODE", GetType(String))
        retDt.Columns.Add("APPLYDATE", GetType(String))
        retDt.Columns.Add("SUBCODE", GetType(String))
        retDt.Columns.Add("CLOSEDATE", GetType(String))

        retDt.Columns.Add("PRINT", GetType(String))

        retDt.Columns.Add("ACCCURRENCYSEGMENT", GetType(String))
        retDt.Columns.Add("BOTHCLASS", GetType(String))
        retDt.Columns.Add("TORICODE", GetType(String))
        retDt.Columns.Add("PAYDAY", GetType(String))
        retDt.Columns.Add("HOLIDAYFLG", GetType(String))
        retDt.Columns.Add("ISTOPSORT", GetType(String))

        retDt.Columns.Add("PRINTMONTH", GetType(String)) 'PROFVIEWから排除したら消す

        Return retDt
    End Function


    ''' <summary>
    ''' オーダー情報取得処理(経理連携出力用)
    ''' </summary>
    ''' <param name="ReportMonth"></param>
    ''' <param name="Department"></param>
    ''' <param name="targetRowItems"></param>
    ''' <returns></returns>
    Private Function CollectDisplayReportInfoAC(ByVal ReportMonth As String, ByVal Department As String, ByVal targetRowItems As List(Of DataRow), isJotPrint As Boolean) As DataTable
        'Private Function CollectDisplayReportInfoAC(ByVal reportMonth As String, ByVal countryCode As String, ByVal department As String) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = New DataTable

        Try
            '************************************
            'SQL生成
            '************************************
            'ユーザーの言語に応じ日本語⇔英語フィールド設定
            Dim textTblField As String = "NAMESJP"
            If COA0019Session.LANGDISP <> C_LANG.JA Then
                textTblField = "NAMES"
            End If

            If isJotPrint = True Then

                DelAcValue(ReportMonth, Department, Nothing, isJotPrint)

                Dim dtAddData As New DataTable
                dtAddData = Me.AddItemList(ReportMonth)

                If dtAddData IsNot Nothing OrElse dtAddData.Rows.Count > 0 Then
                    targetRowItems.AddRange(From itm In dtAddData)
                End If

                For Each targetRowItem In targetRowItems

                    InsAcValueActual(ReportMonth, Department, targetRowItem, isJotPrint)

                    InsAcValueTentative(ReportMonth, Department, targetRowItem, isJotPrint)

                    InsAcValueDailyRate(ReportMonth, Department, targetRowItem, isJotPrint)

                Next

                InsAcValueTentativeCan(ReportMonth, Department, Nothing, isJotPrint)

                InsAcValueDailyRateCan(ReportMonth, Department, Nothing, isJotPrint)

            Else
                For Each targetRowItem In targetRowItems

                    DelAcValue(ReportMonth, Department, targetRowItem, isJotPrint)

                    InsAcValueActual(ReportMonth, Department, targetRowItem, isJotPrint)

                    InsAcValueTentative(ReportMonth, Department, targetRowItem, isJotPrint)
                    InsAcValueDailyRate(ReportMonth, Department, targetRowItem, isJotPrint)

                    InsAcValueTentativeCan(ReportMonth, Department, targetRowItem, isJotPrint)
                    InsAcValueDailyRateCan(ReportMonth, Department, targetRowItem, isJotPrint)


                Next

            End If

            Dim sqlStat As New StringBuilder()
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("   BOTHCLASS")
            sqlStat.AppendLine("  ,DATACRITERIA")
            sqlStat.AppendLine("  ,JOURNALENTRY")
            sqlStat.AppendLine("  ,INPUTSCREENNO")
            sqlStat.AppendLine("  ,DOCUMENTDATE")
            sqlStat.AppendLine("  ,SETTLEMONTHCLS")
            sqlStat.AppendLine("  ,PROOFNO")
            sqlStat.AppendLine("  ,SLIPNUMBER")
            sqlStat.AppendLine("  ,SLIPNO")
            sqlStat.AppendLine("  ,DETAILLINENO")
            sqlStat.AppendLine("  ,DEBSUBJECT")
            sqlStat.AppendLine("  ,DEBSECTION")
            sqlStat.AppendLine("  ,DEBBANK")
            sqlStat.AppendLine("  ,DEBPARTNER")
            sqlStat.AppendLine("  ,DEBGENPURPOSE")
            sqlStat.AppendLine("  ,DEBSEGMENT1")
            sqlStat.AppendLine("  ,DEBSEGMENT2")
            sqlStat.AppendLine("  ,DEBSEGMENT3")
            sqlStat.AppendLine("  ,DEBNO1")
            sqlStat.AppendLine("  ,DEBNO2")
            sqlStat.AppendLine("  ,DEBCONTAXCLS")
            sqlStat.AppendLine("  ,DEBCONTAXCODE")
            'sqlStat.AppendLine("  ,case when substring(DEBSUBJECT,1,1) = '1' then '40' ")
            'sqlStat.AppendLine("        when substring(DEBSUBJECT,1,1) = '2' then '40' ")
            'sqlStat.AppendLine("        else DEBCONTAXCODE ")
            'sqlStat.AppendLine("   end as DEBCONTAXCODE")
            sqlStat.AppendLine("  ,DEBCONTAXRTCLS")
            'sqlStat.AppendLine("  ,case when substring(DEBSUBJECT,1,1) = '1' then '0' ")
            'sqlStat.AppendLine("        when substring(DEBSUBJECT,1,1) = '2' then '0' ")
            'sqlStat.AppendLine("        else DEBCONTAXRTCLS ")
            'sqlStat.AppendLine("   end as DEBCONTAXRTCLS")
            sqlStat.AppendLine("  ,DEBSIMINPCLS")
            'sqlStat.AppendLine("  ,case when substring(DEBSUBJECT,1,1) = '1' then '0' ")
            'sqlStat.AppendLine("        when substring(DEBSUBJECT,1,1) = '2' then '0' ")
            'sqlStat.AppendLine("        else DEBSIMINPCLS ")
            'sqlStat.AppendLine("   end as DEBSIMINPCLS")
            sqlStat.AppendLine("  ,sum(DEBAMOUNT) as DEBAMOUNT")
            'sqlStat.AppendLine("  ,case when substring(DEBSUBJECT,1,1) = '1' then sum(DEBAMOUNT) + round(sum(DEBCONSTAXAMOUNT),0) ")
            'sqlStat.AppendLine("        when substring(DEBSUBJECT,1,1) = '2' then sum(DEBAMOUNT) + round(sum(DEBCONSTAXAMOUNT),0) ")
            'sqlStat.AppendLine("        else sum(DEBAMOUNT) ")
            'sqlStat.AppendLine("   end as DEBAMOUNT")
            sqlStat.AppendLine("  ,round(sum(DEBCONSTAXAMOUNT),0) as DEBCONSTAXAMOUNT")
            'sqlStat.AppendLine("  ,case when substring(DEBSUBJECT,1,1) = '1' then 0.0 ")
            'sqlStat.AppendLine("        when substring(DEBSUBJECT,1,1) = '2' then 0.0 ")
            'sqlStat.AppendLine("        else round(sum(DEBCONSTAXAMOUNT),0) ")
            'sqlStat.AppendLine("   end as DEBCONSTAXAMOUNT")
            sqlStat.AppendLine("  ,round(sum(DEBFORCURAMOUNT),2) as DEBFORCURAMOUNT")
            'sqlStat.AppendLine("  ,case when substring(DEBSUBJECT,1,1) = '1' and DEBFORCURRATE <> 0.0 then round(sum(DEBFORCURAMOUNT) + (sum(DEBCONSTAXAMOUNT) / DEBFORCURRATE),2)")
            'sqlStat.AppendLine("        when substring(DEBSUBJECT,1,1) = '2' and DEBFORCURRATE <> 0.0 then  round(sum(DEBFORCURAMOUNT) + (sum(DEBCONSTAXAMOUNT) / DEBFORCURRATE),2)")
            'sqlStat.AppendLine("        else round(sum(DEBFORCURAMOUNT),2) ")
            'sqlStat.AppendLine("   end as DEBFORCURAMOUNT")
            sqlStat.AppendLine("  ,DEBFORCURRATE")
            sqlStat.AppendLine("  ,DEBFORCURTRDCLS")
            sqlStat.AppendLine("  ,CRESUBJECT")
            sqlStat.AppendLine("  ,CRESECTION")
            sqlStat.AppendLine("  ,CREBANK")
            sqlStat.AppendLine("  ,CREPARTNER")
            sqlStat.AppendLine("  ,CREGENPURPOSE")
            sqlStat.AppendLine("  ,CRESEGMENT1")
            sqlStat.AppendLine("  ,CRESEGMENT2")
            sqlStat.AppendLine("  ,CRESEGMENT3")
            sqlStat.AppendLine("  ,CRENO1")
            sqlStat.AppendLine("  ,CRENO2")
            sqlStat.AppendLine("  ,CRECONTAXCLS")
            sqlStat.AppendLine("  ,CRECONTAXCODE")
            'sqlStat.AppendLine("  ,case when substring(CRESUBJECT,1,1) = '1' then '40' ")
            'sqlStat.AppendLine("        when substring(CRESUBJECT,1,1) = '2' then '40' ")
            'sqlStat.AppendLine("        else CRECONTAXCODE ")
            'sqlStat.AppendLine("   end as CRECONTAXCODE")
            sqlStat.AppendLine("  ,CRECONTAXRTCLS")
            'sqlStat.AppendLine("  ,case when substring(CRESUBJECT,1,1) = '1' then '0' ")
            'sqlStat.AppendLine("        when substring(CRESUBJECT,1,1) = '2' then '0' ")
            'sqlStat.AppendLine("        else CRECONTAXRTCLS ")
            'sqlStat.AppendLine("   end as CRECONTAXRTCLS")
            sqlStat.AppendLine("  ,CRESIMINPCLS")
            'sqlStat.AppendLine("  ,case when substring(CRESUBJECT,1,1) = '1' then '0' ")
            'sqlStat.AppendLine("        when substring(CRESUBJECT,1,1) = '2' then '0' ")
            'sqlStat.AppendLine("        else CRESIMINPCLS ")
            'sqlStat.AppendLine("   end as CRESIMINPCLS")
            sqlStat.AppendLine("  ,sum(CREAMOUNT) as CREAMOUNT")
            'sqlStat.AppendLine("  ,case when substring(CRESUBJECT,1,1) = '1' then sum(CREAMOUNT) + round(sum(CRECONSTAXAMOUNT),0) ")
            'sqlStat.AppendLine("        when substring(CRESUBJECT,1,1) = '2' then sum(CREAMOUNT) + round(sum(CRECONSTAXAMOUNT),0) ")
            'sqlStat.AppendLine("        else sum(CREAMOUNT) ")
            'sqlStat.AppendLine("   end as CREAMOUNT")
            sqlStat.AppendLine("  ,round(sum(CRECONSTAXAMOUNT),0) as CRECONSTAXAMOUNT")
            'sqlStat.AppendLine("  ,case when substring(CRESUBJECT,1,1) = '1' then 0.0 ")
            'sqlStat.AppendLine("        when substring(CRESUBJECT,1,1) = '2' then 0.0 ")
            'sqlStat.AppendLine("        else round(sum(CRECONSTAXAMOUNT),0) ")
            'sqlStat.AppendLine("   end as CRECONSTAXAMOUNT")
            sqlStat.AppendLine("  ,round(sum(CREFORCURAMOUNT),2) as CREFORCURAMOUNT")
            'sqlStat.AppendLine("  ,case when substring(CRESUBJECT,1,1) = '1' and CREFORCURRATE <> 0.0 then  round(sum(CREFORCURAMOUNT) + (sum(CRECONSTAXAMOUNT) / CREFORCURRATE),2)")
            'sqlStat.AppendLine("        when substring(CRESUBJECT,1,1) = '2' and CREFORCURRATE <> 0.0 then  round(sum(CREFORCURAMOUNT) + (sum(CRECONSTAXAMOUNT) / CREFORCURRATE),2)")
            'sqlStat.AppendLine("        else round(sum(CREFORCURAMOUNT),2) ")
            'sqlStat.AppendLine("   end as CREFORCURAMOUNT")
            sqlStat.AppendLine("  ,CREFORCURRATE")
            sqlStat.AppendLine("  ,CREFORCURTRDCLS")
            sqlStat.AppendLine("  ,DEADLINE")
            'sqlStat.AppendLine("  ,SUMMARY")
            sqlStat.AppendLine("  ,case when left(DEBPARTNER,3) = '991' then")
            sqlStat.AppendLine("      WORKC1 + ' : ' + SUMMARY ")
            sqlStat.AppendLine("  else ")
            sqlStat.AppendLine("      case when PROOFNO = 'G9' then")
            'sqlStat.AppendLine("          WORKC1 + ' $' + trim(convert(char,convert(decimal(16,2),sum(WORKF1)))) ")
            sqlStat.AppendLine("          WORKC1 + ' $' + trim(convert(char,convert(decimal(16,2),FSUM.SUMWORKF))) ")
            sqlStat.AppendLine("      else")
            sqlStat.AppendLine("          WORKC1 + ' \' + trim(convert(char,convert(decimal(16,0),FSUM.SUMWORKY)))")
            sqlStat.AppendLine("      end")
            sqlStat.AppendLine("  end as 'SUMMARY'")
            sqlStat.AppendLine("  ,SUMMARYCODE")
            sqlStat.AppendLine("  ,CREATEDDATE")
            'sqlStat.AppendLine("  ,CREATEDTIME")
            sqlStat.AppendLine("  ,Replace(Convert(varchar, getdate(), 108), ':', '') as 'CREATEDTIME'")
            sqlStat.AppendLine("  ,AUTHOR")
            sqlStat.AppendLine("  ,WORKC2") '出力ソート用
            sqlStat.AppendLine("FROM GBT0014_AC_VALUE ")
            sqlStat.AppendLine("INNER JOIN ( ")
            'sqlStat.AppendLine("  SELECT  DEBPARTNER as 'SUMPARTNER', DEBCONTAXCLS as 'SUMCONTAXCLS', sum(WORKF1) as 'SUMWORKF', sum(DEBAMOUNT) + sum(DEBCONSTAXAMOUNT) as 'SUMWORKY' ")
            sqlStat.AppendLine("  SELECT  DEBPARTNER as 'SUMPARTNER', PROOFNO as 'SUMPROOFNO', DEBCONTAXCLS as 'SUMCONTAXCLS', sum(WORKF1) as 'SUMWORKF', sum(DEBAMOUNT) + sum(DEBCONSTAXAMOUNT) as 'SUMWORKY' ")
            sqlStat.AppendLine("  FROM GBT0014_AC_VALUE ")
            sqlStat.AppendLine("  WHERE ")
            sqlStat.AppendLine("      CLOSINGMONTH = @CLOSINGMONTH ")
            sqlStat.AppendLine("    AND CLOSINGGROUP = @CLOSINGGROUP ")
            sqlStat.AppendLine("    AND DELFLG       <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY  ")
            'sqlStat.AppendLine("    DEBPARTNER,DEBCONTAXCLS ")
            sqlStat.AppendLine("    DEBPARTNER,PROOFNO,DEBCONTAXCLS ")
            sqlStat.AppendLine(") FSUM ")
            sqlStat.AppendLine("  ON FSUM.SUMPARTNER = DEBPARTNER ")
            sqlStat.AppendLine("  AND FSUM.SUMPROOFNO = PROOFNO ")
            sqlStat.AppendLine("  AND FSUM.SUMCONTAXCLS = DEBCONTAXCLS ")
            sqlStat.AppendLine("WHERE ")
            sqlStat.AppendLine("      CLOSINGMONTH = @CLOSINGMONTH ")
            sqlStat.AppendLine("  AND CLOSINGGROUP = @CLOSINGGROUP ")
            sqlStat.AppendLine("  AND DELFLG       <> @DELFLG ")
            sqlStat.AppendLine("GROUP BY  ")
            sqlStat.AppendLine("         DATACRITERIA,JOURNALENTRY,INPUTSCREENNO,DOCUMENTDATE, ")
            sqlStat.AppendLine("         SETTLEMONTHCLS,PROOFNO,SLIPNUMBER,SLIPNO,DETAILLINENO, ")
            sqlStat.AppendLine("         DEBSUBJECT,DEBSECTION,DEBBANK,DEBPARTNER,DEBGENPURPOSE, ")
            sqlStat.AppendLine("         DEBSEGMENT1,DEBSEGMENT2,DEBSEGMENT3,DEBNO1,DEBNO2, ")
            sqlStat.AppendLine("         DEBCONTAXCLS,DEBCONTAXCODE,DEBCONTAXRTCLS,DEBSIMINPCLS, ")
            sqlStat.AppendLine("         DEBFORCURRATE,DEBFORCURTRDCLS,CRESUBJECT,CRESECTION,CREBANK, ")
            sqlStat.AppendLine("         CREPARTNER,CREGENPURPOSE,CRESEGMENT1,CRESEGMENT2,CRESEGMENT3, ")
            sqlStat.AppendLine("         CRENO1,CRENO2,CRECONTAXCLS,CRECONTAXCODE,CRECONTAXRTCLS, ")
            sqlStat.AppendLine("         CRESIMINPCLS,CREFORCURRATE,CREFORCURTRDCLS,DEADLINE,SUMMARY, ")
            'sqlStat.AppendLine("         SUMMARYCODE,CREATEDDATE,CREATEDTIME,AUTHOR,BOTHCLASS ")
            'sqlStat.AppendLine("         SUMMARYCODE,CREATEDDATE,AUTHOR,BOTHCLASS ")
            'sqlStat.AppendLine("         SUMMARYCODE,CREATEDDATE,AUTHOR,BOTHCLASS,WORKC1 ")
            sqlStat.AppendLine("         SUMMARYCODE,CREATEDDATE,AUTHOR,BOTHCLASS,WORKC1,FSUM.SUMWORKF,FSUM.SUMWORKY, WORKC2 ")
            sqlStat.AppendLine("HAVING sum(DEBAMOUNT) <> 0.0 or sum(DEBAMOUNT) <> 0.0 ")
            sqlStat.AppendLine("ORDER BY DEBPARTNER, BOTHCLASS, PROOFNO, DEBCONTAXCLS, DEBSUBJECT, CRESUBJECT ")

            Dim dtDbResult As New DataTable
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
                sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン
                'Dim soaAppDateFrom As Date
                'Dim soaAppDateTo As Date
                'If Date.Now.Day() > 25 Then
                '    soaAppDateFrom = DateSerial(Now.Year, Now.Month, 26)
                '    soaAppDateTo = DateSerial(Now.Year, Now.Month + 1, 25)
                'Else
                '    soaAppDateFrom = DateSerial(Now.Year, Now.Month - 1, 26)
                '    soaAppDateTo = DateSerial(Now.Year, Now.Month, 25)
                'End If
                'SQLパラメータ設定
                With sqlCmd.Parameters

                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = ReportMonth
                    If isJotPrint = True Then
                        .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = CONST_CURRENTXML_JOT
                    Else
                        .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = targetRowItems(0)("COUNTRYCODE")

                    End If
                End With
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        'Throw New Exception("Get AccountingCooperationList Error")
                    End If
                    retDt = CreateOrderInfoTableAC()

                    For Each dr As DataRow In dt.Rows
                        Dim writeDr As DataRow
                        writeDr = retDt.NewRow
                        For Each col As DataColumn In dt.Columns
                            writeDr.Item(col.ColumnName) = Convert.ToString(dr.Item(col.ColumnName))
                        Next
                        retDt.Rows.Add(writeDr)
                    Next

                End Using
            End Using
            Return retDt

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
            Return Nothing
        End Try

        Return retDt
    End Function
    ''' <summary>
    ''' オーダー情報を格納する空のデータテーブルを作成する
    ''' </summary>
    Private Function CreateOrderInfoTableAC() As DataTable
        Dim retDt As New DataTable
        '固定部分は追加しておく
        Dim colList As New List(Of String) From {
                                                    "BOTHCLASS", "DATACRITERIA", "JOURNALENTRY", "INPUTSCREENNO", "DOCUMENTDATE", "SETTLEMONTHCLS",
                                                    "PROOFNO", "SLIPNUMBER", "SLIPNO", "DETAILLINENO", "DEBSUBJECT", "DEBSECTION",
                                                    "DEBBANK", "DEBPARTNER", "DEBGENPURPOSE", "DEBSEGMENT1", "DEBSEGMENT2", "DEBSEGMENT3",
                                                    "DEBNO1", "DEBNO2", "DEBCONTAXCLS", "DEBCONTAXCODE", "DEBCONTAXRTCLS", "DEBSIMINPCLS",
                                                    "DEBAMOUNT", "DEBCONSTAXAMOUNT", "DEBFORCURAMOUNT", "DEBFORCURRATE", "DEBFORCURTRDCLS",
                                                    "CRESUBJECT", "CRESECTION", "CREBANK", "CREPARTNER", "CREGENPURPOSE", "CRESEGMENT1",
                                                    "CRESEGMENT2", "CRESEGMENT3", "CRENO1", "CRENO2", "CRECONTAXCLS", "CRECONTAXCODE",
                                                    "CRECONTAXRTCLS", "CRESIMINPCLS", "CREAMOUNT", "CRECONSTAXAMOUNT", "CREFORCURAMOUNT",
                                                    "CREFORCURRATE", "CREFORCURTRDCLS", "DEADLINE", "SUMMARY", "SUMMARYCODE", "CREATEDDATE",
                                                    "CREATEDTIME", "AUTHOR", "WORKC2"}
        For Each colName As String In colList
            retDt.Columns.Add(colName, GetType(String))
            retDt.Columns(colName).DefaultValue = ""
        Next

        '検討中
        retDt.Columns.Add("DUMMY", GetType(String))
        retDt.Columns.Add("DUMMY2", GetType(String))

        Dim dr As DataRow = retDt.NewRow
        dr.Item("DUMMY") = ""
        retDt.Rows.Add(dr)
        Return retDt
    End Function

    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()

        'メニュー以外から遷移
        If Page.PreviousPage Is Nothing Then
            'メニュー以外から遷移
            Me.hdnThisMapVariant.Value = "GB_AccountingCooperation"
        End If

    End Sub

    ''' <summary>
    ''' 画面グリッドのデータを取得しファイルに保存する。
    ''' </summary>
    Private Function FileSaveDisplayInput() As String
        '一覧表示データ復元
        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = New DataTable

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            Return C_MESSAGENO.SYSTEMADM
        End If
        Me.SavedDt = dt
        'そもそも画面表示データがない状態の場合はそのまま終了
        If ViewState("DISPLAY_LINECNT_LIST") Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If
        Dim displayLineCnt = DirectCast(ViewState("DISPLAY_LINECNT_LIST"), List(Of Integer))

        'この段階でありえないがデータテーブルがない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If
        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objTxtPrifix As String = "txt" & Me.WF_LISTAREA.ID
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID

        Dim fieldIdList = New Dictionary(Of String, String) From {{"ACCCURRENCYSEGMENT", objTxtPrifix},
                                                                  {"BOTHCLASS", objTxtPrifix},
                                                                  {"INCTORICODE", objTxtPrifix},
                                                                  {"EXPTORICODE", objTxtPrifix},
                                                                  {"DEPOSITDAY", objTxtPrifix},
                                                                  {"DEPOSITADDMM", objTxtPrifix},
                                                                  {"OVERDRAWDAY", objTxtPrifix},
                                                                  {"OVERDRAWADDMM", objTxtPrifix},
                                                                  {"HOLIDAYFLG", objTxtPrifix}}
        For Each i In displayLineCnt
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                End If
                Dim dr As DataRow = dt.Rows(i - 1)
                If Convert.ToString(dr.Item(fieldId.Key)) <> displayValue.Trim Then
                    UpdateDataTable(displayValue.Trim, i.ToString, fieldId.Key)
                End If
            Next fieldId
        Next i 'End displayLineCnt
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0021ListTable.ERR
        End If
        Me.SavedDt = dt
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' データテーブル更新処理
    ''' </summary>
    ''' <param name="selectedCode">選択したコード</param>
    ''' <param name="lineCnt"></param>
    ''' <param name="targetField"></param>
    Private Sub UpdateDataTable(selectedCode As String, lineCnt As String, targetField As String)
        Dim codeVal As String = selectedCode
        Dim textVal As String = ""
        Dim selectedItem As ListItem = Nothing
        Dim listObj As ListBox = Nothing
        Dim removeColon As Boolean = False

        Dim selectedRow As DataRow = (From item In Me.SavedDt Where Convert.ToString(item("LINECNT")) = lineCnt).FirstOrDefault
        If selectedRow Is Nothing Then
            Return
        End If

        Select Case targetField
            Case "ACCCURRENCYSEGMENT"
                SetAccCurrencySegmentListItem()
                listObj = Me.lbAccCurrencySegment
            Case "BOTHCLASS"
                SetBothClassListItem()
                listObj = Me.lbBothClass
            Case "INCTORICODE", "EXPTORICODE"
                Dim toriComp As String = Convert.ToString(selectedRow("TORICOMP"))
                Dim toriKbn As String = "E"
                If targetField.StartsWith("INC") Then
                    toriKbn = "I"
                End If
                SetToriCodeListItem(toriComp, toriKbn)
                listObj = Me.lbToriCode
                removeColon = True
            Case "DEPOSITDAY", "OVERDRAWDAY"
                SetPayDayListItem()
                listObj = Me.lbPayDay
            Case "HOLIDAYFLG"
                SetHolidayFlgListItem()
                listObj = Me.lbHolidayFlg
        End Select

        If listObj IsNot Nothing Then
            selectedItem = listObj.Items.FindByValue(selectedCode)
        End If

        If selectedItem IsNot Nothing Then
            codeVal = selectedItem.Value
            textVal = selectedItem.Text
        End If
        'コロン除去対象の場合
        If removeColon AndAlso textVal.Contains(":") Then
            Dim splitedTextVal = Split(textVal, ":", 2)
            textVal = splitedTextVal(1)
        End If

        Dim targetDispField = targetField & "NAME"

        selectedRow.Item(targetField) = codeVal

        If Me.SavedDt.Columns.Contains(targetDispField) Then
            selectedRow.Item(targetDispField) = textVal
        End If
        Dim COA0021ListTable As New COA0021ListTable
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = Me.SavedDt
        COA0021ListTable.COA0021saveListTable()

    End Sub
    ''' <summary>
    ''' 一覧PDF出力ボタン押下時
    ''' </summary>
    Public Sub btnListPrint_Click()

        returnCode = C_MESSAGENO.NORMAL
        'クリックされた行レコードを取得
        Dim rowIdString As String = Me.hdnListCurrentRownum.Value
        Dim rowId As Integer = 0
        If Integer.TryParse(rowIdString, rowId) = True Then
            rowId = rowId - 1
        Else
            Return
        End If

        Dim dt As DataTable = New DataTable
        Dim COA0021ListTable As New COA0021ListTable

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
        '印刷ボタンが押された行のデータ取得
        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim targetRowItems As New List(Of DataRow)
        Dim isJotPrint As Boolean = False
        Dim closingGroup As String = ""
        If Convert.ToString(selectedRow("ISTOPSORT")) = "1" Then
            'JOTのPRINT選択時
            'JOT分のデータを復元し対象レコードとする
            Dim dtJot As DataTable = New DataTable
            COA0021ListTable = New COA0021ListTable

            COA0021ListTable.FILEdir = Me.hdnXMLsaveFileJot.Value
            COA0021ListTable.TBLDATA = dtJot
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dtJot = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
                Return
            End If
            If dtJot IsNot Nothing Then
                Dim qFilter = From item In dtJot Where Convert.ToString(item("ISTOPSORT")) <> "1"
                If qFilter.Any Then
                    targetRowItems.AddRange(qFilter.ToArray)
                End If
            End If
            closingGroup = GBC_JOT_SOA_COUNTRY
            isJotPrint = True
        Else
            '単国PRINT選択時
            '選択した行の入力値を取得
            targetRowItems.Add(selectedRow)
            closingGroup = Convert.ToString(selectedRow("COUNTRYCODE"))
        End If
        If targetRowItems Is Nothing OrElse targetRowItems.Count = 0 Then
            'CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage, pageObject:=Me)
            'Return
        End If
        'チェック処理
        Dim errLineCnt As String = "1"
        checkProc(targetRowItems, isJotPrint, errLineCnt)
        If returnCode <> C_MESSAGENO.NORMAL Then
            If isJotPrint Then
                'JOT表に不整合がある為一覧を変更
                'ポジションを設定するのみ
                hdnListPosition.Value = "1"
                'JOTタブに移動
                Me.hdnCurrentViewFile.Value = CONST_CURRENTXML_JOT
                Me.hdnXMLsaveFile.Value = Me.hdnXMLsaveFileJot.Value
            End If
            Return
        End If
        '各国の年月フォーマットの値をyyyy/MMに戻す
        Dim closingMonth As String = CDate(FormatDateYMD(Me.txtReportMonth.Text, GBA00003UserSetting.DATEYMFORMAT)).ToString("yyyy/MM")

        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        '帳票出力
        Dim tmpFile As String = ""
        Dim outUrl As String = ""

        Dim outputDt As DataTable = New DataTable


        '''' 出力用データ取得
        'dt = CollectDisplayReportInfoAC(Convert.ToString(selectedRow.Item("PRINTMONTH")), Convert.ToString(selectedRow.Item("COUNTRYCODE")), Me.txtDepartment.Text)
        dt = CollectDisplayReportInfoAC(closingMonth, txtDepartment.Text, targetRowItems, isJotPrint)
        If dt Is Nothing Then
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me, messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.SYSTEMADM)})
            Return
        End If
        If dt.Rows.Count = 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If


        Dim targetDt = (From dr In dt
                        Where Convert.ToString(dr.Item("DATACRITERIA")) <> "")

        If targetDt.Any = False Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Dim voucher As Integer = 1
        Dim ditail As Integer = 1
        Dim toriCode As String = Left(Convert.ToString(targetDt(0).Item("DEBPARTNER")), 9)
        Dim ProofNo As String = Convert.ToString(targetDt(0).Item("PROOFNO"))
        Dim revenue As String = Convert.ToString(targetDt(0).Item("DEBCONTAXCLS"))

        For Each dr As DataRow In targetDt

            'Dim aas = (From rec In targetDt Where rec("") = "" And    Select CInt(rec(""))).Sum

            If Convert.ToString(dr.Item("BOTHCLASS")) = "B" Then
                '両建
                If Not (toriCode.Equals(Left(dr.Item("DEBPARTNER").ToString, 9)) _
                    AndAlso ProofNo.Equals(dr.Item("PROOFNO")) _
                    AndAlso revenue.Equals(dr.Item("DEBCONTAXCLS"))) Then

                    voucher += 1
                    ditail = 1
                    toriCode = Left(Convert.ToString(dr.Item("DEBPARTNER")), 9)
                    ProofNo = Convert.ToString(dr.Item("PROOFNO"))
                    revenue = Convert.ToString(dr.Item("DEBCONTAXCLS"))
                End If
            Else
                '相殺
                If Not (toriCode.Equals(Left(dr.Item("DEBPARTNER").ToString, 9)) _
                    AndAlso ProofNo.Equals(dr.Item("PROOFNO"))) Then

                    voucher += 1
                    ditail = 1
                    toriCode = Left(Convert.ToString(dr.Item("DEBPARTNER")), 9)
                    ProofNo = Convert.ToString(dr.Item("PROOFNO"))
                    revenue = Convert.ToString(dr.Item("DEBCONTAXCLS"))

                End If
            End If

            '伝票番号
            dr.Item("SLIPNUMBER") = voucher.ToString("00000000")
            '伝票NO
            dr.Item("SLIPNO") = voucher.ToString
            '明細行番号
            dr.Item("DETAILLINENO") = ditail.ToString("000")

            Dim drSwapped = SwapRow(dr, 10, 19)
            dr.ItemArray = drSwapped.ItemArray

            ditail += 1

        Next
        '20190628ADD START ↓詳細シート分のデータ取得

        Dim dtDetailSheetData As DataTable = GetReportDetailData(closingGroup, closingMonth)
        '20190628ADD END   ↑詳細シート分のデータ取得
        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
            Dim targetDt2 = From item In targetDt Order By item("WORKC2"), item("DEBCONTAXCLS"), item("JOURNALENTRY"), item("SLIPNUMBER"), item("DETAILLINENO")
            COA0027ReportTable.MAPID = CONST_MAPID                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = targetDt2.CopyToDataTable              'PARAM04:データ参照tabledata
            COA0027ReportTable.COA0027ReportTable()

            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If

            tmpFile = COA0027ReportTable.FILEpath
            '詳細シートにデータ書き込み
            COA0027ReportTable.ADDFILE = tmpFile
            COA0027ReportTable.TBLDATA = dtDetailSheetData
            COA0027ReportTable.REPORTID = CONST_DETAIL_REPORT_ID
            COA0027ReportTable.ADDSHEET = CONST_DETAIL_REPORT_SHEETNAME
            COA0027ReportTable.COA0027ReportTable()
            outUrl = COA0027ReportTable.URL
            tmpFile = COA0027ReportTable.FILEpath
        End With

        '別画面でExcelを表示
        hdnPrintURL.Value = outUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

    End Sub
    ''' <summary>
    ''' 一覧表の一覧ファイル変更ボタン押下時
    ''' </summary>
    Public Sub btnListChange_Click()
        With Request.Form.GetType.BaseType.BaseType.GetField("_readOnly", Reflection.BindingFlags.NonPublic _
                                                                 Or Reflection.BindingFlags.Instance)
            .SetValue(Request.Form, False)
        End With
        Request.Form("hdnListSortValueGBT00023RWF_LISTAREA") = ""
        hdnListPosition.Value = "1"
        If Me.hdnCurrentViewFile.Value.Equals(CONST_CURRENTXML_JOT) Then
            Me.hdnCurrentViewFile.Value = CONST_CURRENTXML_COUNTRY
            Me.hdnXMLsaveFile.Value = Me.hdnXMLsaveFileCountry.Value
        Else
            Me.hdnCurrentViewFile.Value = CONST_CURRENTXML_JOT
            Me.hdnXMLsaveFile.Value = Me.hdnXMLsaveFileJot.Value
        End If
    End Sub
    ''' <summary>
    ''' 右ボックス出力帳票選択肢設定
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
        Me.txtRightErrorMessage.Text = ""

        'レポートID情報
        COA0022ProfXls.MAPID = excelMapId
        COA0022ProfXls.COA0022getReportId()
        Me.lbRightList.Items.Clear() '一旦選択肢をクリア
        If COA0022ProfXls.ERR = C_MESSAGENO.NORMAL Then
            Try
                Dim listBoxObj As ListBox = DirectCast(COA0022ProfXls.REPORTOBJ, ListBox)
                For Each listItem As ListItem In listBoxObj.Items
                    If listItem.Value.Equals(CONST_DETAIL_REPORT_ID) Then
                        Continue For
                    End If
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
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
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
    ''' 画面の初期値設定
    ''' </summary>
    Private Function InitValueSet() As String
        Dim retVal As String = ""
        'ProfVariインスタンス生成
        Dim COA0016VARIget As New COA0016VARIget With {
                    .MAPID = CONST_MAPID,
                    .COMPCODE = GBC_COMPCODE_D,
                    .VARI = Me.hdnThisMapVariant.Value
            }
        '部門
        COA0016VARIget.FIELD = "SECTION"
        COA0016VARIget.COA0016VARIget()
        Dim setVal As String = "ALL"
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            setVal = COA0016VARIget.VALUE
        End If
        Me.txtDepartment.Text = setVal
        txtDepartment_Change()
        'REPORTMONTH
        COA0016VARIget.FIELD = "REPORTMONTH"
        COA0016VARIget.COA0016VARIget()
        setVal = Now.ToString(GBA00003UserSetting.DATEYMFORMAT)
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL AndAlso COA0016VARIget.VALUE <> "" Then
            setVal = Date.Parse(COA0016VARIget.VALUE).ToString(GBA00003UserSetting.DATEYMFORMAT)
        End If
        Me.txtReportMonth.Text = setVal
        '有効な精算月の選択肢の先頭（直近最大月）
        SetReportMonthListItem(Me.txtReportMonth.Text)
        If Me.lbReportMonth.Items IsNot Nothing AndAlso Me.lbReportMonth.Items.Count > 0 Then
            Me.txtReportMonth.Text = Me.lbReportMonth.Items(0).Text
        End If

        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 表示非表示制御
    ''' </summary>
    ''' <remarks>初回ロード時（非ポストバック時）に実行する想定</remarks>
    Private Sub DisplayControl()

    End Sub

    ''' <summary>
    ''' 期日設定
    ''' </summary>
    Private Function GetPayDay(targetMonth As String, holidayFlg As String, payday As String) As String
        Dim retVal As String = ""   '戻り値用のString
        Dim retDt As New DataTable
        Dim targetday As String = ""
        Dim dt As Date

        If targetMonth = "" Then
            Return retVal
        End If

        If Not Date.TryParse(targetMonth & "/01", dt) Then
            Return retVal
        End If

        If payday = "LAST" Then
            dt = dt.AddMonths(1).AddDays(-1)
            targetday = dt.ToString("yyyy/MM/dd")
        Else
            targetday = targetMonth & "/" & payday
        End If

        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("WITH DateTable (MyDate, Part) ")
        sqlStat.AppendLine("  AS( ")
        sqlStat.AppendLine("      SELECT	(DATEADD(dd, 1, EOMONTH (@TargetDate , -2))),datepart(weekday,DATEADD(dd, 1, EOMONTH (@TargetDate , -2))) ")
        sqlStat.AppendLine("      UNION ALL")
        sqlStat.AppendLine("      SELECT	DATEADD(dd, 1, MyDate),datepart(weekday,DATEADD(dd, 1, MyDate)) ")
        sqlStat.AppendLine("      FROM   DateTable ")
        sqlStat.AppendLine("      WHERE  MyDate <EOMONTH(@TargetDate, 1) ")
        sqlStat.AppendLine("    ), WorkCalender as ( ")
        sqlStat.AppendLine("      SELECT   d.MyDate,d.Part,isnull(h.HOLYDAY_NAME,'') as holidayname, ")
        sqlStat.AppendLine("      case when d.Part = '1' or d.Part = '7' or isnull(h.HOLYDAY_NAME,'') <> '' then '1' ")
        sqlStat.AppendLine("      else '0' end as holydayflg ")
        sqlStat.AppendLine("      FROM     DateTable d ")
        sqlStat.AppendLine("      left join GBM0026_HOLYDAYS h ")
        sqlStat.AppendLine("      on h.HOLYDAY_DATE = d.MyDate ")
        sqlStat.AppendLine("    ) ")

        '休日フラグ
        If holidayFlg = "B" Then
            '前
            sqlStat.AppendLine("  select format(max(MyDate),'yyyy/MM/dd') from WorkCalender w ")
            sqlStat.AppendLine("  where w.MyDate <= @TargetDate ")

        ElseIf holidayFlg = "A" Then
            '後
            sqlStat.AppendLine("  select format(min(MyDate),'yyyy/MM/dd') from WorkCalender w ")
            sqlStat.AppendLine("  where w.MyDate >= @TargetDate ")

        Else
            'そのまま
            Return targetday

        End If
        sqlStat.AppendLine("  and   w.holydayflg <> '1' ")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramTargetDate As SqlParameter = sqlCmd.Parameters.Add("@TargetDate", SqlDbType.NVarChar)
            'SQLパラメータ値セット
            paramTargetDate.Value = targetday
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)

                If retDt.Rows.Count > 0 Then
                    retVal = retDt.Rows(0).Item(0).ToString()
                End If

            End Using
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 年月のリストを取得する
    ''' </summary>
    ''' <returns></returns>
    Private Function GetClosingDay() As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル

        'SQL文作成
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT distinct CD.COUNTRYCODE,CD.REPORTMONTH AS REPORTMONTH")
        sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY CD")
        sqlStat.AppendLine("  INNER JOIN COT0002_APPROVALHIST AP")
        sqlStat.AppendLine("          ON CD.APPLYID  = AP.APPLYID ")
        sqlStat.AppendLine("         AND CD.LASTSTEP = AP.STEP")
        sqlStat.AppendLine("         AND AP.STATUS IN ('" & C_APP_STATUS.APPROVED & "','" & C_APP_STATUS.COMPLETE & "')")
        sqlStat.AppendLine(" WHERE CD.REPORTMONTH <> ''")
        sqlStat.AppendLine("   AND CD.DELFLG = @DELFLG")
        sqlStat.AppendLine("ORDER BY CD.REPORTMONTH DESC ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
            End With
            sqlCon.Open() '接続オープン
            'SQLパラメータ値セット
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 印刷時に締め日情報を再取得
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <returns></returns>
    Private Function GetPrintClosingDate(ByVal countryCode As String, ByVal reportDate As String) As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル

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
        sqlStat.AppendLine("   AND CL.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("   AND CL.REPORTMONTH = @REPORTMONTH")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定

            Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar)
            Dim paramReportDate As SqlParameter = sqlCmd.Parameters.Add("@REPORTMONTH", SqlDbType.NVarChar)
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)
            Dim paramLangDisp As SqlParameter = sqlCmd.Parameters.Add("@LANGDISP", SqlDbType.NVarChar)
            Dim paramStYMD As SqlParameter = sqlCmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
            Dim paramEndYMD As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
            Dim paramTargetYM As SqlParameter = sqlCmd.Parameters.Add("@TARGETYM", SqlDbType.Date)
            paramCountryCode.Value = countryCode
            paramReportDate.Value = FormatDateContrySettings(FormatDateYMD(reportDate, GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramDelFlg.Value = CONST_FLAG_YES
            paramLangDisp.Value = COA0019Session.LANGDISP
            paramStYMD.Value = Date.Now
            paramEndYMD.Value = Date.Now
            paramTargetYM.Value = Date.Parse(reportDate)

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 部門リストアイテムを設定
    ''' </summary>
    Private Sub SetDepartmentListItem(selectedValue As String)

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get

        'リストクリア
        Me.lbDepartment.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DEPARTMENTLIST"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbDepartment
        Else
            COA0017FixValue.LISTBOX2 = Me.lbDepartment
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbDepartment = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbDepartment = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If
        Else
            Return
        End If

        '一覧先頭にALLを追加
        Me.lbDepartment.Items.Insert(0, New ListItem("ALL", "ALL"))

    End Sub

    ''' <summary>
    ''' 部門名設定
    ''' </summary>
    Public Sub txtDepartment_Change()
        Try
            Me.lblDepartmentText.Text = ""
            If Me.txtDepartment.Text.Trim = "" Then
                Return
            End If
            SetDepartmentListItem(Me.txtDepartment.Text)
            If Me.lbDepartment.Items.Count > 0 Then
                Dim findListItem = Me.lbDepartment.Items.FindByValue(Me.txtDepartment.Text)
                If findListItem IsNot Nothing Then
                    Me.lblDepartmentText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbDepartment.Items.FindByValue(Me.txtDepartment.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblDepartmentText.Text = findListItemUpper.Text
                        Me.txtDepartment.Text = findListItemUpper.Value
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
    ''' 精算月リストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetReportMonthListItem(Optional selectedValue As String = "")
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbReportMonth.Items.Clear()

        'リスト設定
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT MAX(CD.REPORTMONTH) AS REPORTMONTH")
        sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY CD")
        sqlStat.AppendLine("  INNER JOIN COT0002_APPROVALHIST AP")
        sqlStat.AppendLine("          ON CD.APPLYID  = AP.APPLYID ")
        sqlStat.AppendLine("         AND CD.LASTSTEP = AP.STEP")
        sqlStat.AppendLine("         AND AP.STATUS IN ('" & C_APP_STATUS.APPROVED & "','" & C_APP_STATUS.COMPLETE & "')")
        sqlStat.AppendLine(" WHERE CD.REPORTMONTH <> ''")
        Dim maxMonth As String
        Using SQLcon As New SqlConnection(COA0019Session.DBcon),
              SQLcmd As New SqlCommand(sqlStat.ToString, SQLcon)
            SQLcon.Open()
            maxMonth = Convert.ToString(SQLcmd.ExecuteScalar())
        End Using
        'どの国も締めていない場合、年月が正しく取得できない場合
        Dim dtVal As Date
        If maxMonth = "" OrElse Date.TryParse(maxMonth & "/01", dtVal) = False Then
            returnCode = C_MESSAGENO.NORMAL
            Return
        End If
        Dim listItems As New List(Of ListItem)
        '全国の締め月の最大から過去12月分のリストを生成
        For i As Integer = 0 To 11
            'yyyy/MM形式フォーマット(こちらをPG内で利用）
            Dim valMonth As String = dtVal.AddMonths(i * -1).ToString("yyyy/MM")
            'ユーザー国に応じたフォーマット（手入力のマッチングはこちらで）
            Dim dispMonth As String = dtVal.AddMonths(i * -1).ToString(GBA00003UserSetting.DATEYMFORMAT)
            listItems.Add(New ListItem(dispMonth, valMonth))
        Next
        lbReportMonth.Items.AddRange(listItems.ToArray)
        '一応現在入力しているテキストと一致するものを選択状態
        If Me.lbReportMonth.Items.Count > 0 Then
            Dim findListItem = Me.lbReportMonth.Items.FindByText(selectedValue)
            If findListItem IsNot Nothing Then
                findListItem.Selected = True
            End If
        End If
        '正常
        returnCode = C_MESSAGENO.NORMAL
    End Sub
    ''' <summary>
    ''' 締め月変更
    ''' </summary>
    Public Sub txtReportMonth_Change()
        Try
            Me.lbReportMonth.Items.Clear()
            If Me.txtReportMonth.Text.Trim <> "" Then
                SetReportMonthListItem(Me.txtReportMonth.Text)
            End If

            If Me.lbReportMonth.Items.Count > 0 Then
                Dim findListItem = Me.lbReportMonth.Items.FindByValue(Me.txtReportMonth.Text)
                If findListItem Is Nothing Then
                    Dim findListItemUpper = Me.lbReportMonth.Items.FindByValue(Me.txtReportMonth.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.txtReportMonth.Text = findListItemUpper.Value
                    End If
                End If
            End If
            '年月に応じ対象の業者を再取得一覧表データ取得（JOT初期表示分）
            Using dt As DataTable = Me.GetListDataTableJot()
                'グリッド用データをファイルに退避
                With Nothing
                    Dim COA0021ListTable As New COA0021ListTable
                    COA0021ListTable.FILEdir = Me.hdnXMLsaveFileJot.Value
                    COA0021ListTable.TBLDATA = dt
                    COA0021ListTable.COA0021saveListTable()
                    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                        Return
                    End If
                End With

            End Using

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub

    ''' <summary>
    ''' 経理円貨外貨区分フラグリストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetAccCurrencySegmentListItem(Optional selectedValue As String = "")
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbAccCurrencySegment.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "ACCCURRENCYSEGMENT"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbAccCurrencySegment
        Else
            COA0017FixValue.LISTBOX2 = Me.lbAccCurrencySegment
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbAccCurrencySegment = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbAccCurrencySegment = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbAccCurrencySegment.Items.Count > 0 Then
                Dim findListItem = Me.lbAccCurrencySegment.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If
    End Sub
    ''' <summary>
    ''' 両建区分フラグリストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetBothClassListItem(Optional selectedValue As String = "")
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbBothClass.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "BOTHCLASS"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbBothClass
        Else
            COA0017FixValue.LISTBOX2 = Me.lbBothClass
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbBothClass = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbBothClass = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbBothClass.Items.Count > 0 Then
                Dim findListItem = Me.lbBothClass.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If
    End Sub
    ''' <summary>
    ''' 取引先コードリストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetToriCodeListItem(toriCompCode As String, toriKbn As String, Optional selectedValue As String = "", Optional currentView As String = "")
        Dim COA0017FixValue As New COA0017FixValue
        If currentView = "" Then
            currentView = Me.hdnCurrentViewFile.Value
        End If

        Dim codeField = "TORICODE"

        'リストクリア
        Me.lbToriCode.Items.Clear()

        'リスト設定
        Dim sqlStat As New StringBuilder
        sqlStat.AppendFormat("SELECT {0} AS [CODE]", codeField).AppendLine()
        'sqlStat.AppendLine("      ,NAMES AS [NAME]")
        sqlStat.AppendLine("      ,NAMES1 AS [NAME]")
        sqlStat.AppendLine("  FROM GBM0025_TORI")
        sqlStat.AppendLine(" WHERE DELFLG <> @DELFLG ")
        sqlStat.AppendLine("   AND LEFT(TORICODE,5) = @TORICOMP")
        sqlStat.AppendLine("   AND TORIKBN          = @TORIKBN")
        sqlStat.AppendLine("   AND STYMD           <= getdate()")
        sqlStat.AppendLine("   AND ENDYMD          >= getdate()")
        'sqlStat.AppendFormat(" GROUP BY {0},REMARK", codeField).AppendLine()



        Using SQLcon As New SqlConnection(COA0019Session.DBcon),
              SQLcmd As New SqlCommand(sqlStat.ToString, SQLcon)
            SQLcon.Open()

            With SQLcmd.Parameters
                .Add("@TORICOMP", SqlDbType.NVarChar).Value = toriCompCode
                .Add("@TORIKBN", SqlDbType.NVarChar).Value = toriKbn
                .Add("@DELFLG", SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
            End With
            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                While SQLdr.Read
                    'DBからアイテムを設定
                    Me.lbToriCode.Items.Add(New ListItem(Convert.ToString(SQLdr("CODE")) & ":" & Convert.ToString(SQLdr("NAME")), Convert.ToString(SQLdr("CODE"))))
                End While
            End Using

        End Using

        '一応現在入力しているテキストと一致するものを選択状態
        If Me.lbToriCode.Items.Count > 0 Then
            Dim findListItem = Me.lbToriCode.Items.FindByValue(selectedValue)
            If findListItem IsNot Nothing Then
                findListItem.Selected = True
            End If
        End If
        '正常
        returnCode = C_MESSAGENO.NORMAL
    End Sub
    ''' <summary>
    ''' 期日リストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetPayDayListItem(Optional selectedValue As String = "")
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbPayDay.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "PAYDAY"
        COA0017FixValue.LISTBOX1 = Me.lbPayDay

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbPayDay = DirectCast(COA0017FixValue.LISTBOX1, ListBox)

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbPayDay.Items.Count > 0 Then
                Dim findListItem = Me.lbPayDay.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If
    End Sub
    ''' <summary>
    ''' 休日フラグアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetHolidayFlgListItem(Optional selectedValue As String = "")
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbHolidayFlg.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "HOLIDAYFLG"
        COA0017FixValue.LISTBOX1 = Me.lbHolidayFlg

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbHolidayFlg = DirectCast(COA0017FixValue.LISTBOX1, ListBox)

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbHolidayFlg.Items.Count > 0 Then
                Dim findListItem = Me.lbHolidayFlg.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If
    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="selectedRows">チェック対象行</param>
    ''' <param name="isJotPrint">JOT印刷か</param>
    Public Sub checkProc(selectedRows As List(Of DataRow), isJotPrint As Boolean, ByRef errLineCnt As String)
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get

        '入力文字置き換え
        '画面PassWord内の使用禁止文字排除

        '部門
        COA0008InvalidChar.CHARin = txtDepartment.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtDepartment.Text = COA0008InvalidChar.CHARout
        End If

        'Dept 単項目チェック
        CheckSingle("SECTION", txtDepartment.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtDepartment.Focus()
            Return
        End If
        'Dept List存在チェック
        CheckList(txtDepartment.Text, lbDepartment, lblDepartment.Text, False)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtDepartment.Focus()
            Return
        End If
        '年月未入力はそもそも印刷ボタンは押せないが念のため
        CheckSingle("REPORTMONTH", txtReportMonth.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtDepartment.Focus()
            Return
        End If
        'Dept List存在チェック
        CheckList(txtReportMonth.Text, lbReportMonth, lblReportMonth.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtReportMonth.Focus()
            Return
        End If
        '一覧表の対象行に付きDATAFIELDマスタより入力チェック
        Dim chkFields As New List(Of String) From {"ACCCURRENCYSEGMENT", "BOTHCLASS",
                                                   "INCTORICODE", "EXPTORICODE", "DEPOSITDAY", "DEPOSITADDMM", "OVERDRAWDAY", "OVERDRAWADDMM", "HOLIDAYFLG"}
        Dim targetListBoxes As New Dictionary(Of String, ListBox) From {{"ACCCURRENCYSEGMENT", lbAccCurrencySegment},
                                                                        {"BOTHCLASS", lbBothClass},
                                                                        {"INCTORICODE", lbToriCode},
                                                                        {"EXPTORICODE", lbToriCode},
                                                                        {"DEPOSITDAY", lbPayDay},
                                                                        {"OVERDRAWDAY", lbPayDay},
                                                                        {"HOLIDAYFLG", lbHolidayFlg}
                                                                        }
        'DROPDOWNの取得
        SetAccCurrencySegmentListItem()
        SetBothClassListItem()
        SetPayDayListItem()
        SetHolidayFlgListItem()
        Dim objectIdBase As String = "txt" & Me.WF_LISTAREA.ID & "{0}{1}"
        Dim objectId As String = ""
        '一覧表の入力項目チェック
        Dim needsToriSingleCheck As Boolean = True
        For Each selectedRow In selectedRows
            For Each chkField In chkFields
                needsToriSingleCheck = True
                objectId = String.Format(objectIdBase, chkField, selectedRow("LINECNT"))
                'If isJotPrint Then
                '    objectId = ""
                'End If
                If isJotPrint AndAlso (chkField.Equals("INCTORICODE") AndAlso selectedRow("ISREFCUSTOMERMST").Equals("0") OrElse
                                       chkField.Equals("EXPTORICODE") AndAlso selectedRow("ISREFCUSTOMERMST").Equals("1")) Then
                    '顧客マスタ参照の取引先コード費用の必須はなし
                    '顧客マスタ非参照のの取引先コード収入必須はなし
                    needsToriSingleCheck = False
                End If

                Dim val As String = Convert.ToString(selectedRow(chkField))
                '単項目チェック
                CheckSingle(chkField, val)
                If returnCode <> C_MESSAGENO.NORMAL And needsToriSingleCheck Then
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
                    errLineCnt = Convert.ToString(selectedRow("LINECNT"))
                    Me.hdnActiveElementAfterOnChange.Value = objectId
                    Return
                End If
                'List存在チェック
                If targetListBoxes.ContainsKey(chkField) Then
                    If chkField.EndsWith("TORICODE") Then
                        Dim toriComp As String = Convert.ToString(selectedRow("TORICOMP"))
                        Dim toriKbn As String = "E"
                        If chkField.StartsWith("INC") Then
                            toriKbn = "I"
                        End If
                        SetToriCodeListItem(toriComp, toriKbn)
                    End If
                    CheckList(val, targetListBoxes(chkField), chkField, False)
                    If returnCode <> C_MESSAGENO.NORMAL Then
                        errLineCnt = Convert.ToString(selectedRow("LINECNT"))
                        Me.hdnActiveElementAfterOnChange.Value = objectId
                        Return
                    End If
                End If
            Next chkField 'End フィールド名
        Next selectedRow 'End 一覧対象行
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
    ''' LIST登録チェック
    ''' </summary>
    ''' <param name="inText"></param>
    ''' <param name="inList"></param>
    Protected Function CheckList(ByVal inText As String, ByVal inList As ListBox, fieldName As String, Optional checkByText As Boolean = True) As Boolean

        Dim flag As Boolean = False

        If inText <> "" Then
            Dim findItem As ListItem = Nothing

            If checkByText Then
                findItem = inList.Items.FindByText(inText)
            Else
                findItem = inList.Items.FindByValue(inText)
            End If

            If findItem Is Nothing Then
                flag = True
                returnCode = C_MESSAGENO.UNSELECTABLEERR
                '〇単項目チェック
                Dim COA0026FieldCheck As New COA0026FieldCheck
                COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
                COA0026FieldCheck.MAPID = CONST_MAPID
                COA0026FieldCheck.COA0026getFieldList()
                Dim dicField = COA0026FieldCheck.FIELDDIC
                Dim dispField As String = fieldName
                If dicField.ContainsKey(fieldName) Then
                    dispField = dicField(fieldName)
                End If
                Dim addMsg As New List(Of String) From {dispField}
                CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me, messageParams:=addMsg)
            End If
        End If

        Return flag
    End Function
    ''' <summary>
    ''' 画面表示のテーブルを制御する
    ''' </summary>
    Private Sub DisplayListObjEdit()
        Dim targetPanel As Panel = Me.WF_LISTAREA
        targetPanel.Attributes.Add("data-listitemtype", Me.hdnCurrentViewFile.Value)
        Dim rightDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DR"), Panel)
        If rightDataDiv.HasControls = False _
           OrElse Not (TypeOf rightDataDiv.Controls(0) Is Table) _
           OrElse DirectCast(rightDataDiv.Controls(0), Table).Rows.Count = 0 Then
            Return
        End If

        '一覧表示データ復元 
        Dim dt As DataTable = New DataTable
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
        '各国の締め月一覧を取得
        Dim dtCloseMonth = GetClosingDay()
        '画面指定の締め月
        Dim selectedReportMonth As String = ""
        Dim selectedReportMonthItme = lbReportMonth.Items.FindByText(Me.txtReportMonth.Text)
        If selectedReportMonthItme IsNot Nothing Then
            selectedReportMonth = selectedReportMonthItme.Value
        End If
        Dim rightHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HR"), Panel)
        Dim rightHeaderTable As Table = DirectCast(rightHeaderDiv.Controls(0), Table)
        Dim dicColumnNameToNo As New Dictionary(Of String, String) From {{"ISTOPSORT", ""}, {"PRINT", ""}}
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
        Dim dicLeftColumnNameToNo As New Dictionary(Of String, String) From {{"PRINT", ""}, {"COUNTRYNAME", ""}}

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
        For i = 0 To rowCnt
            Dim tbrRight As TableRow = rightDataTable.Rows(i)
            Dim tbrLeft As TableRow = leftDataTable.Rows(i)
            Dim lineCnt As String = tbrLeft.Cells(0).Text
            Dim isTopSortRow As Boolean = False
            'JOT⇔COUNTRY一覧遷移用行の装飾
            If dicColumnNameToNo("ISTOPSORT") <> "" Then
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ISTOPSORT")))
                    If .Text = "1" Then
                        tbrRight.Attributes.Add("data-istop", "1")
                        tbrLeft.Attributes.Add("data-istop", "1")
                        isTopSortRow = True
                    End If
                End With
            End If
            '左列のボタンイベント紐づけ
            Dim printButton As HtmlButton = Nothing
            If dicLeftColumnNameToNo("PRINT") <> "" OrElse dicColumnNameToNo("PRINT") <> "" Then
                Dim targetCell As TableCell = If(dicLeftColumnNameToNo("PRINT") <> "", tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("PRINT"))),
                                                                                       tbrRight.Cells(Integer.Parse(dicColumnNameToNo("PRINT"))))

                If targetCell.HasControls AndAlso TypeOf targetCell.Controls(0) Is HtmlButton Then
                    printButton = DirectCast(targetCell.Controls(0), HtmlButton)
                    Dim funcName As String = "listPrintButtonClick(this);return false;"
                    printButton.Attributes.Add("onclick", funcName)
                    targetCell.Attributes.Add("cellfiedlname", "PRINT")

                End If

            End If
            '名称の横にボタンを追加
            If dicLeftColumnNameToNo("COUNTRYNAME") <> "" AndAlso isTopSortRow Then
                With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("COUNTRYNAME")))
                    Dim divColumn As New HtmlGenericControl("div") With {.ID = "div" & targetPanel.ID & "COUNTRYNAME" & lineCnt}

                    Dim spnCn As New Label With {.ID = "lbl" & targetPanel.ID & "COUNTRYNAME" & lineCnt}
                    spnCn.Text = .Text
                    divColumn.Controls.Add(spnCn)
                    Dim btnDetail As New HtmlButton With {.ID = "btn" & targetPanel.ID & "DETAIL" & lineCnt}
                    Dim funcName = "listListFileChangeButtonClick(this);return false;"
                    btnDetail.Attributes.Add("onclick", funcName)

                    divColumn.Controls.Add(btnDetail)
                    .Controls.Add(divColumn)
                End With
            End If
            'JOT⇔COUNTRY一覧遷移用行の装飾
            If dicColumnNameToNo("ISTOPSORT") <> "" Then
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ISTOPSORT")))
                    If .Text = "1" Then
                        tbrRight.Attributes.Add("data-istop", "1")
                        tbrLeft.Attributes.Add("data-istop", "1")
                    End If
                End With
            End If
            '国一覧の場合は画面上部の締め月のデータが各国に存在するか判定
            If Me.hdnCurrentViewFile.Value = CONST_CURRENTXML_COUNTRY AndAlso printButton IsNot Nothing Then
                Dim qtargetRow = From item In dt Where Convert.ToString(item("LINECNT")) = lineCnt
                Dim btnIsShow As Boolean = False
                If qtargetRow.Any Then
                    Dim countryCode As String = Convert.ToString(qtargetRow(0).Item("COUNTRYCODE"))
                    Dim qHasCloseData = From item In dtCloseMonth Where item("REPORTMONTH").Equals(selectedReportMonth) AndAlso item("COUNTRYCODE").Equals(countryCode)
                    If qHasCloseData.Any Then
                        btnIsShow = True
                    End If
                End If
                printButton.Disabled = Not btnIsShow
            End If

        Next

    End Sub
    Public Sub DelAcValue(ByVal ReportMonth As String, ByVal Department As String, ByVal targetRowItem As DataRow, isJotPrint As Boolean)

        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("UPDATE GBT0014_AC_VALUE")
        sqlStat.AppendLine("    SET  DELFLG     = @DELFLG")
        sqlStat.AppendLine("       , UPDYMD     = @ENTYMD")
        sqlStat.AppendLine("       , UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("       , UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("       , RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine("  WHERE DELFLG      = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("    AND CLOSINGMONTH = @CLOSINGMONTH")
        sqlStat.AppendLine("    AND CLOSINGGROUP = @CLOSINGGROUP")

        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
     sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = ReportMonth
                If isJotPrint = True Then
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = CONST_CURRENTXML_JOT
                Else
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                End If

            End With
            sqlCmd.ExecuteNonQuery()
        End Using

    End Sub

    Public Sub InsAcValueActual(ByVal ReportMonth As String, ByVal Department As String, ByVal targetRowItem As DataRow, isJotPrint As Boolean)

        Dim sqlStat As New StringBuilder
        Dim workReportMonthInc As String
        Dim workReportMonthExp As String
        Dim DeadLineInc As String
        Dim DeadLineExp As String

        '期日設定
        workReportMonthInc = Date.Parse(txtReportMonth.Text & "/01").AddMonths(CInt(targetRowItem("DEPOSITADDMM")) + 1).ToString("yyyy/MM")
        workReportMonthExp = Date.Parse(txtReportMonth.Text & "/01").AddMonths(CInt(targetRowItem("OVERDRAWADDMM")) + 1).ToString("yyyy/MM")
        '収入
        DeadLineInc = GetPayDay(workReportMonthInc, Convert.ToString(targetRowItem("HOLIDAYFLG")), Convert.ToString(targetRowItem("DEPOSITDAY")))
        '費用
        DeadLineExp = GetPayDay(workReportMonthExp, Convert.ToString(targetRowItem("HOLIDAYFLG")), Convert.ToString(targetRowItem("OVERDRAWDAY")))

        sqlStat.AppendLine("DECLARE @PAYABLE float;")
        sqlStat.AppendLine("DECLARE @RECEIVABLE float;")
        If isJotPrint = True Then
            If targetRowItem("ISREFCUSTOMERMST").Equals("1") Then
                sqlStat.AppendLine("select @PAYABLE = isnull(sum(aw.UAG_USD),0) from GBT0015_AC_WORK aw where aw.CLOSINGMONTH = @REPORTMONTH and   aw.CLOSINGGROUP = @CLOSINGGROUP and aw.CONTRACTORFIX = @CONTRACTOR and aw.REPORTMONTH = @REPORTMONTH and aw.DELFLG <> @DELFLG and aw.COSTTYPE = '1';")
                sqlStat.AppendLine("select @RECEIVABLE = 0;")
            Else
                sqlStat.AppendLine("select @PAYABLE = 0;")
                sqlStat.AppendLine("select @RECEIVABLE = isnull(sum(aw.UAG_USD),0) from GBT0015_AC_WORK aw where aw.CLOSINGMONTH = @REPORTMONTH and   aw.CLOSINGGROUP = @CLOSINGGROUP and aw.CONTRACTORFIX = @CONTRACTOR  and aw.REPORTMONTH = @REPORTMONTH and aw.DELFLG <> @DELFLG and aw.COSTTYPE = '2';")
            End If
        Else
            sqlStat.AppendLine("select @PAYABLE = PAYABLE from GBT0006_CLOSINGDAY cd where cd.COUNTRYCODE = @CLOSINGGROUP and REPORTMONTH = @REPORTMONTH and DELFLG <> @DELFLG;")
            sqlStat.AppendLine("select @RECEIVABLE = RECEIVABLE from GBT0006_CLOSINGDAY cd where cd.COUNTRYCODE = @CLOSINGGROUP and REPORTMONTH = @REPORTMONTH and DELFLG <> @DELFLG;")
        End If
        sqlStat.AppendLine("DECLARE @DEBSEGMENT2 nvarchar(10);")
        sqlStat.AppendLine("select @DEBSEGMENT2 = DEBITSEGMENT from GBM0001_COUNTRY mc where mc.COUNTRYCODE = @COUNTRYCODE and mc.STYMD <= @ENTYMD and mc.ENDYMD >=@ENTYMD and DELFLG <> @DELFLG;")
        'sqlStat.AppendLine("DECLARE @TAXRATE float;")
        'sqlStat.AppendLine("select @TAXRATE = TAXRATE from GBM0001_COUNTRY mc where mc.COUNTRYCODE = 'JP' and mc.STYMD <= @ENTYMD and mc.ENDYMD >=@ENTYMD and DELFLG <> @DELFLG;")
        sqlStat.AppendLine("DECLARE @OFFSETFLG char(1);")
        sqlStat.AppendLine("select @OFFSETFLG = case when @BOTHCLASS = 'O' and @PAYABLE >= @RECEIVABLE then 'C'")
        sqlStat.AppendLine("                         when @BOTHCLASS = 'O' then 'I'")
        sqlStat.AppendLine("                         else 'B' end;")

        sqlStat.AppendLine("INSERT INTO GBT0014_AC_VALUE")
        sqlStat.AppendLine(" (")
        sqlStat.AppendLine("   CLOSINGMONTH")
        sqlStat.AppendLine("  ,CLOSINGGROUP")
        sqlStat.AppendLine("  ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("  ,BOTHCLASS")
        sqlStat.AppendLine("  ,ISREFCUSTOMERMST")
        sqlStat.AppendLine("  ,DATACRITERIA")
        sqlStat.AppendLine("  ,JOURNALENTRY")
        sqlStat.AppendLine("  ,INPUTSCREENNO")
        sqlStat.AppendLine("  ,DOCUMENTDATE")
        sqlStat.AppendLine("  ,SETTLEMONTHCLS")
        sqlStat.AppendLine("  ,PROOFNO")
        sqlStat.AppendLine("  ,SLIPNUMBER")
        sqlStat.AppendLine("  ,SLIPNO")
        sqlStat.AppendLine("  ,DETAILLINENO")
        sqlStat.AppendLine("  ,DEBSUBJECT")
        sqlStat.AppendLine("  ,DEBSECTION")
        sqlStat.AppendLine("  ,DEBBANK")
        sqlStat.AppendLine("  ,DEBPARTNER")
        sqlStat.AppendLine("  ,DEBGENPURPOSE")
        sqlStat.AppendLine("  ,DEBSEGMENT1")
        sqlStat.AppendLine("  ,DEBSEGMENT2")
        sqlStat.AppendLine("  ,DEBSEGMENT3")
        sqlStat.AppendLine("  ,DEBNO1")
        sqlStat.AppendLine("  ,DEBNO2")
        sqlStat.AppendLine("  ,DEBCONTAXCLS")
        sqlStat.AppendLine("  ,DEBCONTAXCODE")
        sqlStat.AppendLine("  ,DEBCONTAXRTCLS")
        sqlStat.AppendLine("  ,DEBSIMINPCLS")
        sqlStat.AppendLine("  ,DEBAMOUNT")
        sqlStat.AppendLine("  ,DEBCONSTAXAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURRATE")
        sqlStat.AppendLine("  ,DEBFORCURTRDCLS")
        sqlStat.AppendLine("  ,CRESUBJECT")
        sqlStat.AppendLine("  ,CRESECTION")
        sqlStat.AppendLine("  ,CREBANK")
        sqlStat.AppendLine("  ,CREPARTNER")
        sqlStat.AppendLine("  ,CREGENPURPOSE")
        sqlStat.AppendLine("  ,CRESEGMENT1")
        sqlStat.AppendLine("  ,CRESEGMENT2")
        sqlStat.AppendLine("  ,CRESEGMENT3")
        sqlStat.AppendLine("  ,CRENO1")
        sqlStat.AppendLine("  ,CRENO2")
        sqlStat.AppendLine("  ,CRECONTAXCLS")
        sqlStat.AppendLine("  ,CRECONTAXCODE")
        sqlStat.AppendLine("  ,CRECONTAXRTCLS")
        sqlStat.AppendLine("  ,CRESIMINPCLS")
        sqlStat.AppendLine("  ,CREAMOUNT")
        sqlStat.AppendLine("  ,CRECONSTAXAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURRATE")
        sqlStat.AppendLine("  ,CREFORCURTRDCLS")
        sqlStat.AppendLine("  ,DEADLINE")
        sqlStat.AppendLine("  ,SUMMARY")
        sqlStat.AppendLine("  ,SUMMARYCODE")
        sqlStat.AppendLine("  ,CREATEDDATE")
        sqlStat.AppendLine("  ,CREATEDTIME")
        sqlStat.AppendLine("  ,AUTHOR")
        sqlStat.AppendLine("  ,WORKC1")
        sqlStat.AppendLine("  ,WORKC2")
        sqlStat.AppendLine("  ,WORKC3")
        sqlStat.AppendLine("  ,WORKF1")
        sqlStat.AppendLine("  ,WORKF2")
        sqlStat.AppendLine("  ,WORKF3")
        sqlStat.AppendLine("  ,DELFLG")
        sqlStat.AppendLine("  ,INITYMD")
        sqlStat.AppendLine("  ,UPDYMD")
        sqlStat.AppendLine("  ,UPDUSER")
        sqlStat.AppendLine("  ,UPDTERMID")
        sqlStat.AppendLine("  ,RECEIVEYMD")

        sqlStat.AppendLine(" )")

        sqlStat.AppendLine(" SELECT ")
        sqlStat.AppendLine("    @REPORTMONTH as 'CLOSINGMONTH', ")
        sqlStat.AppendLine("    @CLOSINGGROUP as 'CLOSINGGROUP', ")
        sqlStat.AppendLine("    @ACCCURRENCYSEGMENT as ACCCURRENCYSEGMENT, ")
        sqlStat.AppendLine("    @BOTHCLASS as BOTHCLASS, ")
        sqlStat.AppendLine("    @ISREFCUSTOMERMST as ISREFCUSTOMERMST, ")
        sqlStat.AppendLine("    '0' as 'DATACRITERIA', ")
        sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'F' then '1010' else '1002' end as 'JOURNALENTRY',")
        sqlStat.AppendLine("    '11' as 'INPUTSCREENNO',")
        sqlStat.AppendLine("    convert(char(10),EOMONTH(convert(datetime,@REPORTMONTH+'/01')),111) as 'DOCUMENTDATE',")
        sqlStat.AppendLine("    '0' as 'SETTLEMONTHCLS',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'Y' and am.COSTTYPE = '1' then 'A9' ")
        sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'Y' and am.COSTTYPE = '2' then 'C9' ")
        sqlStat.AppendLine("         else 'G9'")
        sqlStat.AppendLine("    end as 'PROOFNO',")
        sqlStat.AppendLine("    '--' as 'SLIPNUMBER',")
        sqlStat.AppendLine("    '--' as 'SLIPNO',")
        sqlStat.AppendLine("    '--' as 'DETAILLINENO',")
        sqlStat.AppendLine("    -- 借方")
        ' 07.26改S
        'sqlStat.AppendLine("    case ")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' and @OFFSETFLG = 'I' and am.COSTTYPE = '1' then am.OFFDBACCOUNT")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' and @OFFSETFLG = 'C' and am.COSTTYPE = '2' then am.OFFDBACCOUNT")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then am.DBACCOUNT")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' and @OFFSETFLG = 'I' and am.COSTTYPE = '1' then am.OFFDBACCOUNTFORIGN")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' and @OFFSETFLG = 'C' and am.COSTTYPE = '2' then am.OFFDBACCOUNTFORIGN")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' then am.DBACCOUNTFORIGN")
        'sqlStat.AppendLine("    end 'DEBSUBJECT',")
        sqlStat.AppendLine("    am.DEBSUBJECT as 'DEBSUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("    @DEPARTMENT as 'DEBSECTION',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then '0001' ")
        sqlStat.AppendLine("            when @OFFSETFLG = 'I' and am.COSTTYPE = '1' then tc.BANKCODE")
        sqlStat.AppendLine("            when @OFFSETFLG = 'C' and am.COSTTYPE = '2' then tc.BANKCODE")
        sqlStat.AppendLine("            else td.BANKCODE")
        sqlStat.AppendLine("    end as 'DEBBANK',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("            when @OFFSETFLG = 'I' and am.COSTTYPE = '1' then @EXPTORICODE")
        sqlStat.AppendLine("            when @OFFSETFLG = 'C' and am.COSTTYPE = '2' then @INCTORICODE")
        sqlStat.AppendLine("            else am.DEBPARTNER")
        sqlStat.AppendLine("    end as 'DEBPARTNER',")
        sqlStat.AppendLine("    am.DEBGENPURPOSE as 'DEBGENPURPOSE',")
        sqlStat.AppendLine("    am.DEBSEGMENT1 as 'DEBSEGMENT1',")
        ' ↓間違い
        'sqlStat.AppendLine("    case ")
        'sqlStat.AppendLine("            when @OFFSETFLG = 'I' and am.COSTTYPE = '1' then am.CRESEGMENT1")
        'sqlStat.AppendLine("            when @OFFSETFLG = 'C' and am.COSTTYPE = '2' then am.CRESEGMENT1")
        'sqlStat.AppendLine("            else am.DEBSEGMENT1")
        'sqlStat.AppendLine("    end as 'DEBSEGMENT1',")
        sqlStat.AppendLine("    @DEBSEGMENT2 as 'DEBSEGMENT2',")
        sqlStat.AppendLine("    @DEBSEGMENT3 as 'DEBSEGMENT3',")
        sqlStat.AppendLine("    '' as 'DEBNO1',")
        sqlStat.AppendLine("    '' as 'DEBNO2',")
        sqlStat.AppendLine("    am.DEBCONTAXCLS as 'DEBCONTAXCLS',")
        ' 07.26改S
        'sqlStat.AppendLine("    am.DEBCONTAXCODE as 'DEBCONTAXCODE',")
        sqlStat.AppendLine("    case when substring(am.DEBSUBJECT,1,1) = '1' then '40' ")
        sqlStat.AppendLine("        when substring(am.DEBSUBJECT,1,1) = '2' then '40' ")
        sqlStat.AppendLine("        else am.DEBCONTAXCODE ")
        sqlStat.AppendLine("    end as DEBCONTAXCODE,")
        'sqlStat.AppendLine("    am.DEBCONTAXRTCLS as 'DEBCONTAXRTCLS',")
        sqlStat.AppendLine("    case when substring(am.DEBSUBJECT,1,1) = '1' then '0' ")
        sqlStat.AppendLine("        when substring(am.DEBSUBJECT,1,1) = '2' then '0' ")
        sqlStat.AppendLine("        else am.DEBCONTAXRTCLS ")
        sqlStat.AppendLine("    end as DEBCONTAXRTCLS,")
        'sqlStat.AppendLine("    am.DEBSIMINPCLS as 'DEBSIMINPCLS',")
        sqlStat.AppendLine("    case when substring(am.DEBSUBJECT,1,1) = '1' then '0' ")
        sqlStat.AppendLine("        when substring(am.DEBSUBJECT,1,1) = '2' then '0' ")
        sqlStat.AppendLine("        else am.DEBSIMINPCLS ")
        sqlStat.AppendLine("    end as DEBSIMINPCLS,")
        'sqlStat.AppendLine("    am.DEBAMOUNT as 'DEBAMOUNT',")
        sqlStat.AppendLine("    case when substring(am.DEBSUBJECT,1,1) = '1' then am.DEBAMOUNT + round(am.DEBCONSTAXAMOUNT,0) ")
        sqlStat.AppendLine("        when substring(am.DEBSUBJECT,1,1) = '2' then am.DEBAMOUNT + round(am.DEBCONSTAXAMOUNT,0) ")
        sqlStat.AppendLine("        else am.DEBAMOUNT ")
        sqlStat.AppendLine("    end as DEBAMOUNT,")
        'sqlStat.AppendLine("    am.DEBCONSTAXAMOUNT as 'DEBCONSTAXAMOUNT',")
        sqlStat.AppendLine("    case when substring(am.DEBSUBJECT,1,1) = '1' then 0.0 ")
        sqlStat.AppendLine("        when substring(am.DEBSUBJECT,1,1) = '2' then 0.0 ")
        sqlStat.AppendLine("        else round(am.DEBCONSTAXAMOUNT,0) ")
        sqlStat.AppendLine("    end as DEBCONSTAXAMOUNT,")
        'sqlStat.AppendLine("    am.DEBFORCURAMOUNT as 'DEBFORCURAMOUNT',")
        sqlStat.AppendLine("    case when substring(am.DEBSUBJECT,1,1) = '1' and am.DEBFORCURRATE <> 0.0 then round(am.DEBFORCURAMOUNT + (am.DEBCONSTAXAMOUNT / am.DEBFORCURRATE),2)")
        sqlStat.AppendLine("        when substring(am.DEBSUBJECT,1,1) = '2' and am.DEBFORCURRATE <> 0.0 then round(am.DEBFORCURAMOUNT + (am.DEBCONSTAXAMOUNT / am.DEBFORCURRATE),2)")
        sqlStat.AppendLine("        else round(am.DEBFORCURAMOUNT,2) ")
        sqlStat.AppendLine("    end as DEBFORCURAMOUNT,")
        ' 07.26改E
        sqlStat.AppendLine("    am.DEBFORCURRATE as 'DEBFORCURRATE',")
        sqlStat.AppendLine("    0 as 'DEBFORCURTRDCLS',")
        sqlStat.AppendLine("    --貸方")
        ' 07.26改S
        'sqlStat.AppendLine("    case ")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'Y' and @OFFSETFLG = 'I' and am.COSTTYPE = '1' then am.OFFCRACCOUNT")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'Y' and @OFFSETFLG = 'C' and am.COSTTYPE = '2' then am.OFFCRACCOUNT")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'Y' then am.CRACCOUNT")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'F' and @OFFSETFLG = 'I' and am.COSTTYPE = '1' then am.OFFCRACCOUNTFORIGN")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'F' and @OFFSETFLG = 'C' and am.COSTTYPE = '2' then am.OFFCRACCOUNTFORIGN")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'F' then am.CRACCOUNTFORIGN")
        'sqlStat.AppendLine("    end 'CRESUBJECT',")
        sqlStat.AppendLine("    am.CRESUBJECT as 'CRESUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("    @DEPARTMENT as 'CRESECTION',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then '0001'")
        sqlStat.AppendLine("            when @OFFSETFLG = 'I' and am.COSTTYPE = '1' then td.BANKCODE")
        sqlStat.AppendLine("            when @OFFSETFLG = 'C' and am.COSTTYPE = '2' then td.BANKCODE")
        sqlStat.AppendLine("            else tc.BANKCODE")
        sqlStat.AppendLine("    end as 'CREBANK',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("            when @OFFSETFLG = 'I' and am.COSTTYPE = '1' then @EXPTORICODE")
        sqlStat.AppendLine("            when @OFFSETFLG = 'C' and am.COSTTYPE = '2' then @INCTORICODE")
        sqlStat.AppendLine("            else am.CREPARTNER")
        sqlStat.AppendLine("    end as 'CREPARTNER',")
        sqlStat.AppendLine("    am.CREGENPURPOSE as 'CREGENPURPOSE',")
        sqlStat.AppendLine("    am.CRESEGMENT1 as 'CRESEGMENT1',")
        ' ↓　間違い
        'sqlStat.AppendLine("    case ")
        'sqlStat.AppendLine("            when @OFFSETFLG = 'I' and am.COSTTYPE = '1' then am.DEBSEGMENT1")
        'sqlStat.AppendLine("            when @OFFSETFLG = 'C' and am.COSTTYPE = '2' then am.DEBSEGMENT1")
        'sqlStat.AppendLine("            else am.CRESEGMENT1")
        'sqlStat.AppendLine("    end as 'CRESEGMENT1',")
        sqlStat.AppendLine("    @DEBSEGMENT2 as 'CRESEGMENT2',")
        sqlStat.AppendLine("    @DEBSEGMENT3 as 'CRESEGMENT3',")
        sqlStat.AppendLine("    '' as 'CRENO1',")
        sqlStat.AppendLine("    '' as 'CRENO2',")
        sqlStat.AppendLine("    am.CRECONTAXCLS as 'CRECONTAXCLS',")
        ' 07.26改S
        'sqlStat.AppendLine("    am.CRECONTAXCODE as 'CRECONTAXCODE',")
        sqlStat.AppendLine("    case when substring(am.CRESUBJECT,1,1) = '1' then '40' ")
        sqlStat.AppendLine("        when substring(am.CRESUBJECT,1,1) = '2' then '40' ")
        sqlStat.AppendLine("        else am.CRECONTAXCODE ")
        sqlStat.AppendLine("    end as CRECONTAXCODE,")
        'sqlStat.AppendLine("    am.CRECONTAXRTCLS as 'CRECONTAXRTCLS',")
        sqlStat.AppendLine("    case when substring(am.CRESUBJECT,1,1) = '1' then '0' ")
        sqlStat.AppendLine("        when substring(am.CRESUBJECT,1,1) = '2' then '0' ")
        sqlStat.AppendLine("        else am.CRECONTAXRTCLS ")
        sqlStat.AppendLine("    end as CRECONTAXRTCLS,")
        'sqlStat.AppendLine("    am.CRESIMINPCLS as 'CRESIMINPCLS',")
        sqlStat.AppendLine("    case when substring(am.CRESUBJECT,1,1) = '1' then '0' ")
        sqlStat.AppendLine("        when substring(am.CRESUBJECT,1,1) = '2' then '0' ")
        sqlStat.AppendLine("        else am.CRESIMINPCLS ")
        sqlStat.AppendLine("    end as CRESIMINPCLS,")
        'sqlStat.AppendLine("    am.CREAMOUNT as 'CREAMOUNT',")
        sqlStat.AppendLine("    case when substring(am.CRESUBJECT,1,1) = '1' then am.CREAMOUNT + round(am.CRECONSTAXAMOUNT,0) ")
        sqlStat.AppendLine("        when substring(am.CRESUBJECT,1,1) = '2' then am.CREAMOUNT + round(am.CRECONSTAXAMOUNT,0) ")
        sqlStat.AppendLine("        else am.CREAMOUNT ")
        sqlStat.AppendLine("    end as CREAMOUNT,")
        'sqlStat.AppendLine("    am.CRECONSTAXAMOUNT as 'CRECONSTAXAMOUNT',")
        sqlStat.AppendLine("    case when substring(am.CRESUBJECT,1,1) = '1' then 0.0 ")
        sqlStat.AppendLine("        when substring(am.CRESUBJECT,1,1) = '2' then 0.0 ")
        sqlStat.AppendLine("        else round(am.CRECONSTAXAMOUNT,0) ")
        sqlStat.AppendLine("    end as CRECONSTAXAMOUNT,")
        'sqlStat.AppendLine("    am.CREFORCURAMOUNT as 'CREFORCURAMOUNT',")
        sqlStat.AppendLine("    case when substring(am.CRESUBJECT,1,1) = '1' and am.CREFORCURRATE <> 0.0 then  round(am.CREFORCURAMOUNT + (am.CRECONSTAXAMOUNT / am.CREFORCURRATE),2)")
        sqlStat.AppendLine("        when substring(am.CRESUBJECT,1,1) = '2' and am.CREFORCURRATE <> 0.0 then  round(am.CREFORCURAMOUNT + (am.CRECONSTAXAMOUNT / am.CREFORCURRATE),2)")
        sqlStat.AppendLine("        else round(am.CREFORCURAMOUNT,2) ")
        sqlStat.AppendLine("    end as CREFORCURAMOUNT,")
        ' 07.26改E
        sqlStat.AppendLine("    am.DEBFORCURRATE as 'CREFORCURRATE',")
        sqlStat.AppendLine("    0 as 'CREFORCURTRDCLS',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("            when @OFFSETFLG = 'I' and am.COSTTYPE = '1' then @EXPDEADLINE")
        sqlStat.AppendLine("            when am.COSTTYPE = '1' then @INCDEADLINE")
        sqlStat.AppendLine("            when @OFFSETFLG = 'C' and am.COSTTYPE = '2' then @INCDEADLINE")
        sqlStat.AppendLine("            when am.COSTTYPE = '2' then @EXPDEADLINE")
        sqlStat.AppendLine("    end as 'DEADLINE',")
        'sqlStat.AppendLine("    '' as 'DEADLINE',")
        sqlStat.AppendLine("    '' as 'SUMMARY',")
        'sqlStat.AppendLine("    case ")
        'sqlStat.AppendLine("            when @OFFSETFLG = 'I' and am.COSTTYPE = '1' then '000001'")
        'sqlStat.AppendLine("            when am.COSTTYPE = '1' then '000000'")
        'sqlStat.AppendLine("            when @OFFSETFLG = 'C' and am.COSTTYPE = '2' then '000000'")
        'sqlStat.AppendLine("            when am.COSTTYPE = '2' then '000001'")
        'sqlStat.AppendLine("    end as 'SUMMARYCODE',")
        sqlStat.AppendLine("    am.SUMMARYCODE as 'SUMMARYCODE',")
        sqlStat.AppendLine("    CONVERT(varchar, @ENTYMD, 112) as 'CREATEDDATE',")
        sqlStat.AppendLine("    REPLACE(CONVERT(varchar, @ENTYMD, 108), ':', '') as 'CREATEDTIME',")
        sqlStat.AppendLine("    @UPDUSER as 'AUTHOR',")
        'sqlStat.AppendLine("    '' as 'WORKC1',")
        If isJotPrint = True Then
            'sqlStat.AppendLine("    td.NAMES as WORKC1,")
            sqlStat.AppendLine("    td.NAMES1 as WORKC1,")
        Else
            sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'F' then isnull(cty.NAMESJP,'') else '' end as WORKC1,")
        End If
        sqlStat.AppendLine("    '1' as 'WORKC2',") '※出力ソート用
        sqlStat.AppendLine("    '' as 'WORKC3',")
        'sqlStat.AppendLine("    0.0 as 'WORKF1',")
        'sqlStat.AppendLine("    am.WORKF1 as 'WORKF1',")
        sqlStat.AppendLine("    case when am.CREFORCURRATE <> 0.0 then  round(am.CREFORCURAMOUNT + (am.CRECONSTAXAMOUNT / am.CREFORCURRATE),2)")
        sqlStat.AppendLine("         when am.CREFORCURRATE <> 0.0 then  round(am.CREFORCURAMOUNT + (am.CRECONSTAXAMOUNT / am.CREFORCURRATE),2)")
        sqlStat.AppendLine("        else round(am.CREFORCURAMOUNT,2) ")
        sqlStat.AppendLine("    end as WORKF1,")
        sqlStat.AppendLine("    0.0 as 'WORKF2',")
        sqlStat.AppendLine("    0.0 as 'WORKF3',")
        sqlStat.AppendLine("    '" & CONST_FLAG_NO & "' as 'DELFLG',")
        sqlStat.AppendLine("    @ENTYMD as 'INITYMD',")
        sqlStat.AppendLine("    @ENTYMD as 'UPDYMD',")
        sqlStat.AppendLine("    @UPDUSER as 'UPDUSER',")
        sqlStat.AppendLine("    @UPDTERMID as 'UPDTERMID',")
        sqlStat.AppendLine("    @RECEIVEYMD as 'RECEIVEYMD'")
        sqlStat.AppendLine("from")
        sqlStat.AppendLine("(")
        sqlStat.AppendLine("    select")
        sqlStat.AppendLine("        aw.COSTTYPE,")
        sqlStat.AppendLine("        ---- 借方")
        ' 07.26改S
        'sqlStat.AppendLine("        aw.DBACCOUNT,")
        'sqlStat.AppendLine("        aw.DBACCOUNTFORIGN,")
        'sqlStat.AppendLine("        aw.OFFDBACCOUNT,")
        'sqlStat.AppendLine("        aw.OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("        case ")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' and @OFFSETFLG = 'I' and aw.COSTTYPE = '1' then aw.OFFDBACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' and @OFFSETFLG = 'C' and aw.COSTTYPE = '2' then aw.OFFDBACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then aw.DBACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' and @OFFSETFLG = 'I' and aw.COSTTYPE = '1' then aw.OFFDBACCOUNTFORIGN")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' and @OFFSETFLG = 'C' and aw.COSTTYPE = '2' then aw.OFFDBACCOUNTFORIGN")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' then aw.DBACCOUNTFORIGN")
        sqlStat.AppendLine("        end 'DEBSUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("        case when aw.COSTTYPE = '1' then @INCTORICODE else @EXPTORICODE end as 'DEBPARTNER',")
        sqlStat.AppendLine("        aw.DEBGENPURPOSE,")
        sqlStat.AppendLine("        aw.DBSEGMENT1 as 'DEBSEGMENT1',")
        sqlStat.AppendLine("        aw.COSTTYPE as 'DEBCONTAXCLS',")
        sqlStat.AppendLine("        case when aw.TAXATION = '0' then '40' else '20' end as 'DEBCONTAXCODE',")
        sqlStat.AppendLine("        case when aw.TAXATION = '0' then '0' else fv.VALUE1 end as 'DEBCONTAXRTCLS',")
        sqlStat.AppendLine("        case when aw.TAXATION = '0' then '0' else '1' end as 'DEBSIMINPCLS',")
        If targetRowItem("ACCCURRENCYSEGMENT").ToString = "F" Then
            'sqlStat.AppendLine("        round(sum(aw.UAG_JPY),0) as 'DEBAMOUNT',")
            sqlStat.AppendLine("        round(sum(aw.UAG_USD) * aw.REPORTRATEJPY,0) as 'DEBAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY * (isnull(la.TAXRATE,@TAXRATE) / 100.0) end),0) as 'DEBCONSTAXAMOUNT',")
            'sqlStat.AppendLine("        round(round(sum(case when TAXATION = '0' then 0 else aw.UAG_USD end) * aw.REPORTRATEJPY,0) * (isnull(la.TAXRATE,@TAXRATE) / 100.0) ,0) as 'DEBCONSTAXAMOUNT',")
            sqlStat.AppendLine("        round(round(sum(case when TAXATION = '0' then 0 else aw.UAG_USD end) * aw.REPORTRATEJPY,0) * (isnull(la.TAXRATE,ct.TAXRATE) / 100.0) ,0) as 'DEBCONSTAXAMOUNT',")
        Else
            sqlStat.AppendLine("        round(sum(aw.UAG_JPY_SHIP),0) as 'DEBAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY_SHIP * (isnull(la.TAXRATE,@TAXRATE) / 100.0) end),0) as 'DEBCONSTAXAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY_SHIP end) * (isnull(la.TAXRATE,@TAXRATE) / 100.0) ,0) as 'DEBCONSTAXAMOUNT',")
            sqlStat.AppendLine("        round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY_SHIP end) * (isnull(la.TAXRATE,ct.TAXRATE) / 100.0) ,0) as 'DEBCONSTAXAMOUNT',")
        End If
        sqlStat.AppendLine("        sum(case when @ACCCURRENCYSEGMENT = 'F' then aw.UAG_USD else 0 end) as 'DEBFORCURAMOUNT',")
        sqlStat.AppendLine("        case when @ACCCURRENCYSEGMENT = 'F' then aw.REPORTRATEJPY else 0 end as 'DEBFORCURRATE',")
        sqlStat.AppendLine("        -- 貸方")
        ' 07.26改S
        'sqlStat.AppendLine("        aw.CRACCOUNT,")
        'sqlStat.AppendLine("        aw.CRACCOUNTFORIGN,")
        'sqlStat.AppendLine("        aw.OFFCRACCOUNT,")
        'sqlStat.AppendLine("        aw.OFFCRACCOUNTFORIGN,")
        sqlStat.AppendLine("        case ")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' and @OFFSETFLG = 'I' and aw.COSTTYPE = '1' then aw.OFFCRACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' and @OFFSETFLG = 'C' and aw.COSTTYPE = '2' then aw.OFFCRACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then aw.CRACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' and @OFFSETFLG = 'I' and aw.COSTTYPE = '1' then aw.OFFCRACCOUNTFORIGN")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' and @OFFSETFLG = 'C' and aw.COSTTYPE = '2' then aw.OFFCRACCOUNTFORIGN")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' then aw.CRACCOUNTFORIGN")
        sqlStat.AppendLine("        end 'CRESUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("        case when aw.COSTTYPE = '1' then @INCTORICODE else @EXPTORICODE end as 'CREPARTNER',")
        sqlStat.AppendLine("        aw.CREGENPURPOSE,")
        sqlStat.AppendLine("        aw.CRSEGMENT1 as 'CRESEGMENT1',")
        sqlStat.AppendLine("        aw.COSTTYPE as 'CRECONTAXCLS',")
        sqlStat.AppendLine("        case when aw.TAXATION = '0' then '40' else '20' end as 'CRECONTAXCODE',")
        sqlStat.AppendLine("        case when aw.TAXATION = '0' then '0' else fv.VALUE1 end as 'CRECONTAXRTCLS',")
        sqlStat.AppendLine("        case when aw.TAXATION = '0' then '0' else '1' end as 'CRESIMINPCLS',")
        If targetRowItem("ACCCURRENCYSEGMENT").ToString = "F" Then
            'sqlStat.AppendLine("        round(sum(aw.UAG_JPY),0) as 'CREAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when aw.TAXATION = '0' then 0 else aw.UAG_JPY * (isnull(la.TAXRATE,@TAXRATE) / 100.0) end),0) as 'CRECONSTAXAMOUNT',")
            sqlStat.AppendLine("        round(sum(aw.UAG_USD) * aw.REPORTRATEJPY,0) as 'CREAMOUNT',")
            'sqlStat.AppendLine("        round(round(sum(case when aw.TAXATION = '0' then 0 else aw.UAG_USD end ) *aw.REPORTRATEJPY,0) * (isnull(la.TAXRATE,@TAXRATE) / 100.0) ,0) as 'CRECONSTAXAMOUNT',")
            sqlStat.AppendLine("        round(round(sum(case when aw.TAXATION = '0' then 0 else aw.UAG_USD end ) *aw.REPORTRATEJPY,0) * (isnull(la.TAXRATE,ct.TAXRATE) / 100.0) ,0) as 'CRECONSTAXAMOUNT',")
        Else
            sqlStat.AppendLine("        round(sum(aw.UAG_JPY_SHIP),0) as 'CREAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when aw.TAXATION = '0' then 0 else aw.UAG_JPY_SHIP * (isnull(la.TAXRATE,@TAXRATE) / 100.0) end),0) as 'CRECONSTAXAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when aw.TAXATION = '0' then 0 else aw.UAG_JPY_SHIP end) * (isnull(la.TAXRATE,@TAXRATE) / 100.0),0) as 'CRECONSTAXAMOUNT',")
            sqlStat.AppendLine("        round(sum(case when aw.TAXATION = '0' then 0 else aw.UAG_JPY_SHIP end) * (isnull(la.TAXRATE,ct.TAXRATE) / 100.0),0) as 'CRECONSTAXAMOUNT',")
        End If
        sqlStat.AppendLine("        sum(case when @ACCCURRENCYSEGMENT = 'F' then aw.UAG_USD else 0 end) as 'CREFORCURAMOUNT',")
        sqlStat.AppendLine("        case when @ACCCURRENCYSEGMENT = 'F' then aw.REPORTRATEJPY else 0 end as 'CREFORCURRATE',")
        'sqlStat.AppendLine("        case when aw.COSTTYPE = '1' then '000000' else '000001' end as 'SUMMARYCODE'")
        'sqlStat.AppendLine("        case when aw.COSTTYPE = '2' or @OFFSETFLG = 'I' then '000001' else '' end as 'SUMMARYCODE'")
        sqlStat.AppendLine("        case when (aw.COSTTYPE = '2' and @OFFSETFLG = 'B') or @OFFSETFLG = 'I' then '000001' else '' end as 'SUMMARYCODE',")
        sqlStat.AppendLine("        sum(case when @ACCCURRENCYSEGMENT = 'F' then aw.UAG_USD else UAG_USD_SHIP end) as 'WORKF1'")
        sqlStat.AppendLine("    from GBT0015_AC_WORK aw")
        sqlStat.AppendLine("    left outer join  GBT0011_LBR_AGREEMENT la")
        sqlStat.AppendLine("      on  la.RELATEDORDERNO = aw.ORDERNO")
        sqlStat.AppendLine("      and la.DELFLG <> @DELFLG")
        sqlStat.AppendLine("      and aw.COSTCODE in ('S0103-01','S0103-02','S0103-03')")
        sqlStat.AppendLine("    left outer join  GBM0001_COUNTRY ct")
        sqlStat.AppendLine("      on  ct.COUNTRYCODE = 'JP'")
        sqlStat.AppendLine("      and ct.STYMD <= aw.ACTUALDATE")
        sqlStat.AppendLine("      and ct.ENDYMD >= aw.ACTUALDATE")
        sqlStat.AppendLine("      and ct.DELFLG <> @DELFLG")
        sqlStat.AppendLine("    inner join  COS0017_FIXVALUE fv")
        sqlStat.AppendLine("      on  fv.SYSCODE = 'GB'")
        sqlStat.AppendLine("      and fv.CLASS = 'SALESTAX'")
        sqlStat.AppendLine("      and fv.KEYCODE = trim(convert(char,isnull(la.TAXRATE,ct.TAXRATE)))")
        sqlStat.AppendLine("      and fv.DELFLG <> @DELFLG")
        sqlStat.AppendLine("      and fv.STYMD <= @ENTYMD")
        sqlStat.AppendLine("      and fv.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("    where aw.DELFLG <> @DELFLG")
        sqlStat.AppendLine("    and aw.CLOSINGGROUP = @CLOSINGGROUP")
        If isJotPrint = True Then
            sqlStat.AppendLine("    and aw.CONTRACTORFIX = @CONTRACTOR")
            If targetRowItem("ISREFCUSTOMERMST").Equals("1") Then
                sqlStat.AppendLine("    and aw.COSTTYPE = '1'")
            Else
                sqlStat.AppendLine("    and aw.COSTTYPE = '2'")
            End If
        End If
        sqlStat.AppendLine("    and aw.REPORTMONTH = @REPORTMONTH")
        sqlStat.AppendLine("    and aw.CLOSINGMONTH = @REPORTMONTH")
        sqlStat.AppendLine("    and aw.SOAAPPDATE <> '1900/01/01'")
        sqlStat.AppendLine("    group by aw.DBACCOUNT,")
        sqlStat.AppendLine("        aw.DBACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.OFFDBACCOUNT,")
        sqlStat.AppendLine("        aw.OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.CRACCOUNT,")
        sqlStat.AppendLine("        aw.CRACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.OFFCRACCOUNT,")
        sqlStat.AppendLine("        aw.OFFCRACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.COSTTYPE,")
        sqlStat.AppendLine("        aw.DBSEGMENT1,")
        sqlStat.AppendLine("        aw.CRSEGMENT1,")
        sqlStat.AppendLine("        aw.TAXATION,")
        sqlStat.AppendLine("        aw.REPORTRATEJPY,")
        sqlStat.AppendLine("        aw.DEBGENPURPOSE,")
        sqlStat.AppendLine("        aw.CREGENPURPOSE,")
        sqlStat.AppendLine("        fv.VALUE1,")
        sqlStat.AppendLine("        la.TAXRATE,")
        sqlStat.AppendLine("        ct.TAXRATE")
        sqlStat.AppendLine(") am")
        sqlStat.AppendLine("inner join GBM0025_TORI td")
        sqlStat.AppendLine("    on td.TORICODE = am.DEBPARTNER")
        sqlStat.AppendLine("    and td.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and td.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("    and td.DELFLG <> @DELFLG")
        sqlStat.AppendLine("inner join GBM0025_TORI tc")
        sqlStat.AppendLine("    on tc.TORICODE = am.CREPARTNER")
        sqlStat.AppendLine("    and tc.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and tc.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("    and tc.DELFLG <> @DELFLG")
        sqlStat.AppendLine("left join GBM0001_COUNTRY cty")
        sqlStat.AppendLine("    on cty.DELFLG <> @DELFLG")
        sqlStat.AppendLine("    and cty.COUNTRYCODE = @CLOSINGGROUP")
        sqlStat.AppendLine("    and cty.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and cty.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("where ( am.DEBAMOUNT <> 0.0 or am.CREAMOUNT <> 0.0)")
        sqlStat.AppendLine("order by am.DEBPARTNER, am.DEBCONTAXCLS ")

        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
            sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = targetRowItem("ACCCURRENCYSEGMENT")
                .Add("@BOTHCLASS", SqlDbType.NVarChar).Value = targetRowItem("BOTHCLASS")
                .Add("@INCTORICODE", SqlDbType.NVarChar).Value = targetRowItem("INCTORICODE")
                .Add("@EXPTORICODE", SqlDbType.NVarChar).Value = targetRowItem("EXPTORICODE")
                .Add("@INCDEADLINE", SqlDbType.NVarChar).Value = DeadLineInc
                .Add("@EXPDEADLINE", SqlDbType.NVarChar).Value = DeadLineExp

                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = ReportMonth
                .Add("@DEPARTMENT", SqlDbType.NVarChar).Value = Department
                .Add("@DEBSEGMENT3", SqlDbType.NVarChar).Value = "30"

                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = ReportMonth
                If isJotPrint = True Then
                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = "JP"
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = CONST_CURRENTXML_JOT
                    .Add("@CONTRACTOR", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                Else
                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                End If
                .Add("@ISREFCUSTOMERMST", SqlDbType.NVarChar).Value = targetRowItem("ISREFCUSTOMERMST")

            End With
            sqlCmd.ExecuteNonQuery()
        End Using

    End Sub

    Public Sub InsAcValueTentative(ByVal ReportMonth As String, ByVal Department As String, ByVal targetRowItem As DataRow, isJotPrint As Boolean)

        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("DECLARE @DEBSEGMENT2 nvarchar(10);")
        sqlStat.AppendLine("select @DEBSEGMENT2 = DEBITSEGMENT from GBM0001_COUNTRY mc where mc.COUNTRYCODE = @COUNTRYCODE and mc.STYMD <= @ENTYMD and mc.ENDYMD >=@ENTYMD  and DELFLG <> @DELFLG;")
        'sqlStat.AppendLine("DECLARE @TAXRATE float;")
        'sqlStat.AppendLine("select @TAXRATE = TAXRATE from GBM0001_COUNTRY mc where mc.COUNTRYCODE = 'JP' and mc.STYMD <= @ENTYMD and mc.ENDYMD >=@ENTYMD and DELFLG <> @DELFLG;")

        ' 未着・未洗浄ＴＨＯＭＡＳ
        sqlStat.AppendLine("DECLARE @TORICODE nvarchar(10);")
        sqlStat.AppendLine("select @TORICODE = VALUE1 from COS0017_FIXVALUE fv where fv.SYSCODE = 'GB' and fv.CLASS = 'AC_SYSCODE' and fv.KEYCODE = 'THOMAS' and fv.STYMD <= @ENTYMD and fv.ENDYMD >= @ENTYMD and fv.DELFLG <> 'Y';")
        sqlStat.AppendLine("DECLARE @SUBJECTS_RECEIVABLE nvarchar(10);")
        sqlStat.AppendFormat("select @SUBJECTS_RECEIVABLE = VALUE1 from COS0017_FIXVALUE fv where fv.SYSCODE = 'GB' and fv.CLASS = 'AC_T_SUBJECTS' and fv.KEYCODE = 'RECEIVABLE_{0}' and fv.STYMD <= @ENTYMD and fv.ENDYMD >= @ENTYMD and fv.DELFLG <> 'Y';", targetRowItem("ACCCURRENCYSEGMENT"))
        sqlStat.AppendLine("DECLARE @SUBJECTS_UNPAID nvarchar(10);")
        sqlStat.AppendFormat("select @SUBJECTS_UNPAID = VALUE1 from COS0017_FIXVALUE fv where fv.SYSCODE = 'GB' and fv.CLASS = 'AC_T_SUBJECTS' and fv.KEYCODE = 'UNPAID_{0}' and fv.STYMD <= @ENTYMD and fv.ENDYMD >= @ENTYMD and fv.DELFLG <> 'Y';", targetRowItem("ACCCURRENCYSEGMENT"))
        sqlStat.AppendLine("DECLARE @BASESHIP nvarchar(10);")
        sqlStat.AppendLine("select @BASESHIP = convert(char,BILLINGYMD,111) from GBT0006_CLOSINGDAY cd where cd.COUNTRYCODE = @CLOSINGGROUP and REPORTMONTH = @REPORTMONTH and DELFLG <> 'Y';")

        sqlStat.AppendLine("INSERT INTO GBT0014_AC_VALUE")
        sqlStat.AppendLine(" (")
        sqlStat.AppendLine("   CLOSINGMONTH")
        sqlStat.AppendLine("  ,CLOSINGGROUP")
        sqlStat.AppendLine("  ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("  ,BOTHCLASS")
        sqlStat.AppendLine("  ,ISREFCUSTOMERMST")
        sqlStat.AppendLine("  ,DATACRITERIA")
        sqlStat.AppendLine("  ,JOURNALENTRY")
        sqlStat.AppendLine("  ,INPUTSCREENNO")
        sqlStat.AppendLine("  ,DOCUMENTDATE")
        sqlStat.AppendLine("  ,SETTLEMONTHCLS")
        sqlStat.AppendLine("  ,PROOFNO")
        sqlStat.AppendLine("  ,SLIPNUMBER")
        sqlStat.AppendLine("  ,SLIPNO")
        sqlStat.AppendLine("  ,DETAILLINENO")
        sqlStat.AppendLine("  ,DEBSUBJECT")
        sqlStat.AppendLine("  ,DEBSECTION")
        sqlStat.AppendLine("  ,DEBBANK")
        sqlStat.AppendLine("  ,DEBPARTNER")
        sqlStat.AppendLine("  ,DEBGENPURPOSE")
        sqlStat.AppendLine("  ,DEBSEGMENT1")
        sqlStat.AppendLine("  ,DEBSEGMENT2")
        sqlStat.AppendLine("  ,DEBSEGMENT3")
        sqlStat.AppendLine("  ,DEBNO1")
        sqlStat.AppendLine("  ,DEBNO2")
        sqlStat.AppendLine("  ,DEBCONTAXCLS")
        sqlStat.AppendLine("  ,DEBCONTAXCODE")
        sqlStat.AppendLine("  ,DEBCONTAXRTCLS")
        sqlStat.AppendLine("  ,DEBSIMINPCLS")
        sqlStat.AppendLine("  ,DEBAMOUNT")
        sqlStat.AppendLine("  ,DEBCONSTAXAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURRATE")
        sqlStat.AppendLine("  ,DEBFORCURTRDCLS")
        sqlStat.AppendLine("  ,CRESUBJECT")
        sqlStat.AppendLine("  ,CRESECTION")
        sqlStat.AppendLine("  ,CREBANK")
        sqlStat.AppendLine("  ,CREPARTNER")
        sqlStat.AppendLine("  ,CREGENPURPOSE")
        sqlStat.AppendLine("  ,CRESEGMENT1")
        sqlStat.AppendLine("  ,CRESEGMENT2")
        sqlStat.AppendLine("  ,CRESEGMENT3")
        sqlStat.AppendLine("  ,CRENO1")
        sqlStat.AppendLine("  ,CRENO2")
        sqlStat.AppendLine("  ,CRECONTAXCLS")
        sqlStat.AppendLine("  ,CRECONTAXCODE")
        sqlStat.AppendLine("  ,CRECONTAXRTCLS")
        sqlStat.AppendLine("  ,CRESIMINPCLS")
        sqlStat.AppendLine("  ,CREAMOUNT")
        sqlStat.AppendLine("  ,CRECONSTAXAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURRATE")
        sqlStat.AppendLine("  ,CREFORCURTRDCLS")
        sqlStat.AppendLine("  ,DEADLINE")
        sqlStat.AppendLine("  ,SUMMARY")
        sqlStat.AppendLine("  ,SUMMARYCODE")
        sqlStat.AppendLine("  ,CREATEDDATE")
        sqlStat.AppendLine("  ,CREATEDTIME")
        sqlStat.AppendLine("  ,AUTHOR")
        sqlStat.AppendLine("  ,WORKC1")
        sqlStat.AppendLine("  ,WORKC2")
        sqlStat.AppendLine("  ,WORKC3")
        sqlStat.AppendLine("  ,WORKF1")
        sqlStat.AppendLine("  ,WORKF2")
        sqlStat.AppendLine("  ,WORKF3")
        sqlStat.AppendLine("  ,DELFLG")
        sqlStat.AppendLine("  ,INITYMD")
        sqlStat.AppendLine("  ,UPDYMD")
        sqlStat.AppendLine("  ,UPDUSER")
        sqlStat.AppendLine("  ,UPDTERMID")
        sqlStat.AppendLine("  ,RECEIVEYMD")

        sqlStat.AppendLine(" )")

        sqlStat.AppendLine(" SELECT ")
        sqlStat.AppendLine("    @REPORTMONTH as 'CLOSINGMONTH', ")
        sqlStat.AppendLine("    @CLOSINGGROUP as 'CLOSINGGROUP', ")
        sqlStat.AppendLine("    @ACCCURRENCYSEGMENT as ACCCURRENCYSEGMENT, ")
        sqlStat.AppendLine("    @BOTHCLASS as BOTHCLASS, ")
        sqlStat.AppendLine("    @ISREFCUSTOMERMST as ISREFCUSTOMERMST, ")
        sqlStat.AppendLine("    '0' as 'DATACRITERIA', ")
        sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'F' then '1010' else '1002' end as 'JOURNALENTRY',")
        sqlStat.AppendLine("    '11' as 'INPUTSCREENNO',")
        sqlStat.AppendLine("    convert(char(10),EOMONTH(convert(datetime,@REPORTMONTH+'/01')),111) as 'DOCUMENTDATE',")
        sqlStat.AppendLine("    '0' as 'SETTLEMONTHCLS',")
        sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'F' then 'G9' else 'F9' end as 'PROOFNO',")
        sqlStat.AppendLine("    '--' as 'SLIPNUMBER',")
        sqlStat.AppendLine("    '--' as 'SLIPNO',")
        sqlStat.AppendLine("    '--' as 'DETAILLINENO',")
        sqlStat.AppendLine("    -- 借方")
        ' 07.26改S
        'sqlStat.AppendLine("    case ")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then am.DBACCOUNT")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' then am.DBACCOUNTFORIGN")
        'sqlStat.AppendLine("    end 'DEBSUBJECT',")
        'sqlStat.AppendLine("    am.DEBSUBJECT as 'DEBSUBJECT',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("            when substring(am.DEBSUBJECT,1,1) = '1' then @SUBJECTS_RECEIVABLE")
        sqlStat.AppendLine("            when substring(am.DEBSUBJECT,1,1) = '2' then @SUBJECTS_UNPAID")
        sqlStat.AppendLine("            else am.DEBSUBJECT")
        sqlStat.AppendLine("    end 'DEBSUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("    @DEPARTMENT as 'DEBSECTION',")
        sqlStat.AppendLine("    td.BANKCODE as 'DEBBANK',")
        'sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'Y' then '0001' else td.BANKCODE end as 'DEBBANK',")
        sqlStat.AppendLine("    am.DEBPARTNER as 'DEBPARTNER',")
        sqlStat.AppendLine("    am.DEBGENPURPOSE as 'DEBGENPURPOSE',")
        sqlStat.AppendLine("    am.DEBSEGMENT1 as 'DEBSEGMENT1',")
        sqlStat.AppendLine("    @DEBSEGMENT2 as 'DEBSEGMENT2',")
        sqlStat.AppendLine("    @DEBSEGMENT3 as 'DEBSEGMENT3',")
        sqlStat.AppendLine("    '99999999' as 'DEBNO1',")
        sqlStat.AppendLine("    '9999' as 'DEBNO2',")
        sqlStat.AppendLine("    am.DEBCONTAXCLS as 'DEBCONTAXCLS',")
        sqlStat.AppendLine("    am.DEBCONTAXCODE as 'DEBCONTAXCODE',")
        sqlStat.AppendLine("    am.DEBCONTAXRTCLS as 'DEBCONTAXRTCLS',")
        sqlStat.AppendLine("    '0' as 'DEBSIMINPCLS',")
        sqlStat.AppendLine("    am.DEBAMOUNT as 'DEBAMOUNT',")
        sqlStat.AppendLine("    am.DEBCONSTAXAMOUNT as 'DEBCONSTAXAMOUNT',")
        sqlStat.AppendLine("    am.DEBFORCURAMOUNT as 'DEBFORCURAMOUNT',")
        sqlStat.AppendLine("    am.DEBFORCURRATE as 'DEBFORCURRATE',")
        sqlStat.AppendLine("    0 as 'DEBFORCURTRDCLS',")
        sqlStat.AppendLine("    --貸方")
        ' 07.26改S
        'sqlStat.AppendLine("    case ")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'Y' then am.CRACCOUNT")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'F' then am.CRACCOUNTFORIGN")
        'sqlStat.AppendLine("    end 'CRESUBJECT',")
        'sqlStat.AppendLine("    am.CRESUBJECT as 'CRESUBJECT',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("            when substring(am.CRESUBJECT,1,1) = '1' then @SUBJECTS_RECEIVABLE")
        sqlStat.AppendLine("            when substring(am.CRESUBJECT,1,1) = '2' then @SUBJECTS_UNPAID")
        sqlStat.AppendLine("            else am.CRESUBJECT")
        sqlStat.AppendLine("    end 'CRESUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("    @DEPARTMENT as 'CRESECTION',")
        sqlStat.AppendLine("    tc.BANKCODE as 'CREBANK',")
        'sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'Y' then '0001' else tc.BANKCODE end as 'CREBANK',")
        sqlStat.AppendLine("    am.CREPARTNER as 'CREPARTNER',")
        sqlStat.AppendLine("    am.CREGENPURPOSE as 'CREGENPURPOSE',")
        sqlStat.AppendLine("    am.CRESEGMENT1 as 'CRESEGMENT1',")
        sqlStat.AppendLine("    @DEBSEGMENT2 as 'CRESEGMENT2',")
        sqlStat.AppendLine("    @DEBSEGMENT3 as 'CRESEGMENT3',")
        sqlStat.AppendLine("    '99999999' as 'CRENO1',")
        sqlStat.AppendLine("    '9999' as 'CRENO2',")
        sqlStat.AppendLine("    am.CRECONTAXCLS as 'CRECONTAXCLS',")
        sqlStat.AppendLine("    am.CRECONTAXCODE as 'CRECONTAXCODE',")
        sqlStat.AppendLine("    am.CRECONTAXRTCLS as 'CRECONTAXRTCLS',")
        sqlStat.AppendLine("    '0' as 'CRESIMINPCLS',")
        sqlStat.AppendLine("    am.CREAMOUNT as 'CREAMOUNT',")
        sqlStat.AppendLine("    am.CRECONSTAXAMOUNT as 'CRECONSTAXAMOUNT',")
        sqlStat.AppendLine("    am.CREFORCURAMOUNT as 'CREFORCURAMOUNT',")
        sqlStat.AppendLine("    am.DEBFORCURRATE as 'CREFORCURRATE',")
        sqlStat.AppendLine("    0 as 'CREFORCURTRDCLS',")
        sqlStat.AppendLine("    '2099/12/31' as 'DEADLINE',")
        sqlStat.AppendLine("    '未着・未洗浄ＴＨＯＭＡＳ' as 'SUMMARY',")
        sqlStat.AppendLine("    am.SUMMARYCODE as 'SUMMARYCODE',")
        sqlStat.AppendLine("    CONVERT(varchar, @ENTYMD, 112) as 'CREATEDDATE',")
        sqlStat.AppendLine("    REPLACE(CONVERT(varchar, @ENTYMD, 108), ':', '') as 'CREATEDTIME',")
        sqlStat.AppendLine("    @UPDUSER as 'AUTHOR',")
        'sqlStat.AppendLine("    '' as 'WORKC1',")
        If isJotPrint = True Then
            sqlStat.AppendLine("    'JOT' as 'WORKC1',")
        Else
            sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'F' then isnull(cty.NAMESJP,'') else '' end as WORKC1,")
        End If
        sqlStat.AppendLine("    '2' as 'WORKC2',") '※出力ソート用
        sqlStat.AppendLine("    '' as 'WORKC3',")
        sqlStat.AppendLine("    0.0 as 'WORKF1',")
        sqlStat.AppendLine("    0.0 as 'WORKF2',")
        sqlStat.AppendLine("    0.0 as 'WORKF3',")
        sqlStat.AppendLine("    '" & CONST_FLAG_NO & "' as 'DELFLG',")
        sqlStat.AppendLine("    @ENTYMD as 'INITYMD',")
        sqlStat.AppendLine("    @ENTYMD as 'UPDYMD',")
        sqlStat.AppendLine("    @UPDUSER as 'UPDUSER',")
        sqlStat.AppendLine("    @UPDTERMID as 'UPDTERMID',")
        sqlStat.AppendLine("    @RECEIVEYMD as 'RECEIVEYMD'")
        sqlStat.AppendLine("from")
        sqlStat.AppendLine("(")
        sqlStat.AppendLine("    select")
        sqlStat.AppendLine("        aw.COSTTYPE,")
        sqlStat.AppendLine("        ---- 借方")
        ' 07.26改S
        'sqlStat.AppendLine("        aw.DBACCOUNT,")
        'sqlStat.AppendLine("        aw.DBACCOUNTFORIGN,")
        'sqlStat.AppendLine("        aw.OFFDBACCOUNT,")
        'sqlStat.AppendLine("        aw.OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("        case ")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then aw.DBACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' then aw.DBACCOUNTFORIGN")
        sqlStat.AppendLine("            else ''")
        sqlStat.AppendLine("        end 'DEBSUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("        @TORICODE as 'DEBPARTNER',")
        sqlStat.AppendLine("        aw.DEBGENPURPOSE,")
        sqlStat.AppendLine("        aw.DBSEGMENT1 as 'DEBSEGMENT1',")
        sqlStat.AppendLine("        aw.COSTTYPE as 'DEBCONTAXCLS',")
        'sqlStat.AppendLine("        case when aw.TAXATION = '0' then '40' else '20' end as 'DEBCONTAXCODE',")
        'sqlStat.AppendLine("        case when aw.TAXATION = '0' then '0' else fv.VALUE1 end as 'DEBCONTAXRTCLS',")
        sqlStat.AppendLine("        '40' as 'DEBCONTAXCODE',")
        sqlStat.AppendLine("        '0' as 'DEBCONTAXRTCLS',")
        If targetRowItem("ACCCURRENCYSEGMENT").ToString = "F" Then
            'sqlStat.AppendLine("        round(sum(aw.UAG_JPY),0) as 'DEBAMOUNT',")
            sqlStat.AppendLine("        round(sum(aw.UAG_USD) * aw.REPORTRATEJPY,0)  as 'DEBAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY * (isnull(la.TAXRATE,@TAXRATE) / 100.0) end),0) as 'DEBCONSTAXAMOUNT',")
            sqlStat.AppendLine("        0 as 'DEBCONSTAXAMOUNT',")
        Else
            sqlStat.AppendLine("        round(sum(aw.UAG_JPY_SHIP),0) as 'DEBAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY_SHIP * (isnull(la.TAXRATE,@TAXRATE) / 100.0) end),0) as 'DEBCONSTAXAMOUNT',")
            sqlStat.AppendLine("        0 as 'DEBCONSTAXAMOUNT',")
        End If
        sqlStat.AppendLine("        sum(case when @ACCCURRENCYSEGMENT = 'F' then aw.UAG_USD else 0 end) as 'DEBFORCURAMOUNT',")
        sqlStat.AppendLine("        case when @ACCCURRENCYSEGMENT = 'F' then aw.REPORTRATEJPY else 0 end as 'DEBFORCURRATE',")
        sqlStat.AppendLine("        -- 貸方")
        ' 07.26改S
        'sqlStat.AppendLine("        aw.CRACCOUNT,")
        'sqlStat.AppendLine("        aw.CRACCOUNTFORIGN,")
        'sqlStat.AppendLine("        aw.OFFCRACCOUNT,")
        'sqlStat.AppendLine("        aw.OFFCRACCOUNTFORIGN,")
        sqlStat.AppendLine("        case ")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then aw.CRACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' then aw.CRACCOUNTFORIGN")
        sqlStat.AppendLine("            else ''")
        sqlStat.AppendLine("        end 'CRESUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("        @TORICODE as 'CREPARTNER',")
        sqlStat.AppendLine("        aw.CREGENPURPOSE,")
        sqlStat.AppendLine("        aw.CRSEGMENT1 as 'CRESEGMENT1',")
        sqlStat.AppendLine("        aw.COSTTYPE as 'CRECONTAXCLS',")
        'sqlStat.AppendLine("        case when aw.TAXATION = '0' then '40' else '20' end as 'CRECONTAXCODE',")
        'sqlStat.AppendLine("        case when aw.TAXATION = '0' then '0' else fv.VALUE1 end as 'CRECONTAXRTCLS',")
        sqlStat.AppendLine("        '40' as 'CRECONTAXCODE',")
        sqlStat.AppendLine("        '0' as 'CRECONTAXRTCLS',")
        If targetRowItem("ACCCURRENCYSEGMENT").ToString = "F" Then
            'sqlStat.AppendLine("        round(sum(aw.UAG_JPY),0) as 'CREAMOUNT',")
            sqlStat.AppendLine("        round(sum(aw.UAG_USD) * aw.REPORTRATEJPY,0) as 'CREAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when aw.TAXATION = '0' then 0 else aw.UAG_JPY * (isnull(la.TAXRATE,@TAXRATE) / 100.0) end),0) as 'CRECONSTAXAMOUNT',")
            sqlStat.AppendLine("        0 as 'CRECONSTAXAMOUNT',")
        Else
            sqlStat.AppendLine("        round(sum(aw.UAG_JPY_SHIP),0) as 'CREAMOUNT',")
            'sqlStat.AppendLine("        round(sum(case when aw.TAXATION = '0' then 0 else aw.UAG_JPY_SHIP * (isnull(la.TAXRATE,@TAXRATE) / 100.0) end),0) as 'CRECONSTAXAMOUNT',")
            sqlStat.AppendLine("        0 as 'CRECONSTAXAMOUNT',")
        End If
        sqlStat.AppendLine("        sum(case when @ACCCURRENCYSEGMENT = 'F' then aw.UAG_USD else 0 end) as 'CREFORCURAMOUNT',")
        sqlStat.AppendLine("        case when @ACCCURRENCYSEGMENT = 'F' then aw.REPORTRATEJPY else 0 end as 'CREFORCURRATE',")
        'sqlStat.AppendLine("        case when aw.COSTTYPE = '1' then '000000' else '000001' end as 'SUMMARYCODE'")
        sqlStat.AppendLine("        '' as 'SUMMARYCODE'")
        sqlStat.AppendLine("    from GBT0015_AC_WORK aw")
        'sqlStat.AppendLine("    left outer join  GBT0011_LBR_AGREEMENT la")
        'sqlStat.AppendLine("      on  la.RELATEDORDERNO = aw.ORDERNO")
        'sqlStat.AppendLine("      and la.DELFLG <> @DELFLG")
        'sqlStat.AppendLine("      and aw.COSTCODE in ('S0103-01','S0103-02','S0103-03')")
        'sqlStat.AppendLine("    inner join  COS0017_FIXVALUE fv")
        'sqlStat.AppendLine("      on  fv.SYSCODE = 'GB'")
        'sqlStat.AppendLine("      and fv.CLASS = 'SALESTAX'")
        'sqlStat.AppendLine("      and fv.KEYCODE = trim(convert(char,isnull(la.TAXRATE,@TAXRATE)))")
        'sqlStat.AppendLine("      and fv.DELFLG <> @DELFLG")
        'sqlStat.AppendLine("      and fv.STYMD <= @ENTYMD")
        'sqlStat.AppendLine("      and fv.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("    where aw.DELFLG <> 'Y'")
        sqlStat.AppendLine("    and aw.CLOSINGGROUP = @CLOSINGGROUP")
        If isJotPrint = True Then
            sqlStat.AppendLine("    and aw.CONTRACTORFIX = @CONTRACTOR")
            If targetRowItem("ISREFCUSTOMERMST").Equals("1") Then
                sqlStat.AppendLine("    and aw.COSTTYPE = '1'")
            Else
                sqlStat.AppendLine("    and aw.COSTTYPE = '2'")
            End If
        End If
        'sqlStat.AppendLine("    and aw.REPORTMONTH > @REPORTMONTH")
        sqlStat.AppendLine("    and ( aw.REPORTMONTH > @REPORTMONTH or ( aw.REPORTMONTH = @REPORTMONTH and aw.SOAAPPDATE = '1900/01/01')) ")
        sqlStat.AppendLine("    and aw.CLOSINGMONTH = @REPORTMONTH")
        ' デマレージはSHIPに依存しない
        'sqlStat.AppendLine("    and (aw.SHIPDATE < @BASESHIP And aw.SHIPDATE <> '1900/01/01')")
        sqlStat.AppendLine("    and (( aw.COSTCODE <> 'S0102-01' and aw.SHIPDATE < @BASESHIP And aw.SHIPDATE <> '1900/01/01') ")
        sqlStat.AppendLine("          or (aw.COSTCODE = 'S0102-01' and aw.REPORTMONTHORG <= @REPORTMONTH))")
        sqlStat.AppendLine("    group by aw.DBACCOUNT,")
        sqlStat.AppendLine("        aw.DBACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.OFFDBACCOUNT,")
        sqlStat.AppendLine("        aw.OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.CRACCOUNT,")
        sqlStat.AppendLine("        aw.CRACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.OFFCRACCOUNT,")
        sqlStat.AppendLine("        aw.OFFCRACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.COSTTYPE,")
        sqlStat.AppendLine("        aw.DBSEGMENT1,")
        sqlStat.AppendLine("        aw.CRSEGMENT1,")
        sqlStat.AppendLine("        aw.TAXATION,")
        sqlStat.AppendLine("        aw.REPORTRATEJPY,")
        sqlStat.AppendLine("        aw.DEBGENPURPOSE,")
        sqlStat.AppendLine("        aw.CREGENPURPOSE")
        'sqlStat.AppendLine("        fv.VALUE1,")
        'sqlStat.AppendLine("        la.TAXRATE")
        sqlStat.AppendLine(") am")
        sqlStat.AppendLine("inner join GBM0025_TORI td")
        sqlStat.AppendLine("    on td.TORICODE = am.DEBPARTNER")
        sqlStat.AppendLine("    and td.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and td.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("    and td.DELFLG <> @DELFLG")
        sqlStat.AppendLine("inner join GBM0025_TORI tc")
        sqlStat.AppendLine("    on tc.TORICODE = am.CREPARTNER")
        sqlStat.AppendLine("    and tc.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and tc.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("    and tc.DELFLG <> @DELFLG")
        sqlStat.AppendLine("left join GBM0001_COUNTRY cty")
        sqlStat.AppendLine("    on cty.DELFLG <> @DELFLG")
        sqlStat.AppendLine("    and cty.COUNTRYCODE = @CLOSINGGROUP")
        sqlStat.AppendLine("    and cty.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and cty.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("where ( am.DEBAMOUNT <> 0.0 or am.CREAMOUNT <> 0.0)")

        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
            sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters

                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = ReportMonth
                .Add("@DEPARTMENT", SqlDbType.NVarChar).Value = Department
                .Add("@DEBSEGMENT3", SqlDbType.NVarChar).Value = "30"

                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = ReportMonth
                .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = targetRowItem("ACCCURRENCYSEGMENT")
                .Add("@BOTHCLASS", SqlDbType.NVarChar).Value = "B"
                If isJotPrint = True Then
                    '.Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = "Y"
                    '.Add("@BOTHCLASS", SqlDbType.NVarChar).Value = "B"
                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = "JP"
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = CONST_CURRENTXML_JOT
                    .Add("@CONTRACTOR", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                Else
                    '.Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = targetRowItem("ACCCURRENCYSEGMENT")
                    '.Add("@BOTHCLASS", SqlDbType.NVarChar).Value = targetRowItem("BOTHCLASS")
                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                End If
                .Add("@ISREFCUSTOMERMST", SqlDbType.NVarChar).Value = targetRowItem("ISREFCUSTOMERMST")

            End With
            sqlCmd.ExecuteNonQuery()
        End Using

    End Sub

    Public Sub InsAcValueDailyRate(ByVal ReportMonth As String, ByVal Department As String, ByVal targetRowItem As DataRow, isJotPrint As Boolean)

        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("DECLARE @DEBSEGMENT2 nvarchar(10);")
        sqlStat.AppendLine("select @DEBSEGMENT2 = DEBITSEGMENT from GBM0001_COUNTRY mc where mc.COUNTRYCODE = @COUNTRYCODE and mc.STYMD <= @ENTYMD and mc.ENDYMD >=@ENTYMD  and DELFLG <> 'Y';")
        'sqlStat.AppendLine("DECLARE @TAXRATE float;")
        'sqlStat.AppendLine("select @TAXRATE = TAXRATE from GBM0001_COUNTRY mc where mc.COUNTRYCODE = 'JP' and mc.STYMD <= @ENTYMD and mc.ENDYMD >=@ENTYMD and DELFLG <> @DELFLG;")

        ' 未着・未洗浄按分
        sqlStat.AppendLine("DECLARE @TORICODE nvarchar(10);")
        sqlStat.AppendLine("select @TORICODE = VALUE1 from COS0017_FIXVALUE fv where fv.SYSCODE = 'GB' and fv.CLASS = 'AC_SYSCODE' and fv.KEYCODE = 'DIVIDE' and fv.STYMD <= @ENTYMD and fv.ENDYMD >= @ENTYMD and fv.DELFLG <> 'Y';")
        sqlStat.AppendLine("DECLARE @SUBJECTS_RECEIVABLE nvarchar(10);")
        sqlStat.AppendFormat("select @SUBJECTS_RECEIVABLE = VALUE1 from COS0017_FIXVALUE fv where fv.SYSCODE = 'GB' and fv.CLASS = 'AC_T_SUBJECTS' and fv.KEYCODE = 'RECEIVABLE_{0}' and fv.STYMD <= @ENTYMD and fv.ENDYMD >= @ENTYMD and fv.DELFLG <> 'Y';", targetRowItem("ACCCURRENCYSEGMENT"))
        sqlStat.AppendLine("DECLARE @SUBJECTS_UNPAID nvarchar(10);")
        sqlStat.AppendFormat("select @SUBJECTS_UNPAID = VALUE1 from COS0017_FIXVALUE fv where fv.SYSCODE = 'GB' and fv.CLASS = 'AC_T_SUBJECTS' and fv.KEYCODE = 'UNPAID_{0}' and fv.STYMD <= @ENTYMD and fv.ENDYMD >= @ENTYMD and fv.DELFLG <> 'Y';", targetRowItem("ACCCURRENCYSEGMENT"))
        sqlStat.AppendLine("DECLARE @BASESHIP nvarchar(10);")
        sqlStat.AppendLine("select @BASESHIP = convert(char,BILLINGYMD,111) from GBT0006_CLOSINGDAY cd where cd.COUNTRYCODE = @CLOSINGGROUP and REPORTMONTH = @REPORTMONTH and DELFLG <> 'Y';")

        sqlStat.AppendLine("INSERT INTO GBT0014_AC_VALUE")
        sqlStat.AppendLine(" (")
        sqlStat.AppendLine("   CLOSINGMONTH")
        sqlStat.AppendLine("  ,CLOSINGGROUP")
        sqlStat.AppendLine("  ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("  ,BOTHCLASS")
        sqlStat.AppendLine("  ,ISREFCUSTOMERMST")
        sqlStat.AppendLine("  ,DATACRITERIA")
        sqlStat.AppendLine("  ,JOURNALENTRY")
        sqlStat.AppendLine("  ,INPUTSCREENNO")
        sqlStat.AppendLine("  ,DOCUMENTDATE")
        sqlStat.AppendLine("  ,SETTLEMONTHCLS")
        sqlStat.AppendLine("  ,PROOFNO")
        sqlStat.AppendLine("  ,SLIPNUMBER")
        sqlStat.AppendLine("  ,SLIPNO")
        sqlStat.AppendLine("  ,DETAILLINENO")
        sqlStat.AppendLine("  ,DEBSUBJECT")
        sqlStat.AppendLine("  ,DEBSECTION")
        sqlStat.AppendLine("  ,DEBBANK")
        sqlStat.AppendLine("  ,DEBPARTNER")
        sqlStat.AppendLine("  ,DEBGENPURPOSE")
        sqlStat.AppendLine("  ,DEBSEGMENT1")
        sqlStat.AppendLine("  ,DEBSEGMENT2")
        sqlStat.AppendLine("  ,DEBSEGMENT3")
        sqlStat.AppendLine("  ,DEBNO1")
        sqlStat.AppendLine("  ,DEBNO2")
        sqlStat.AppendLine("  ,DEBCONTAXCLS")
        sqlStat.AppendLine("  ,DEBCONTAXCODE")
        sqlStat.AppendLine("  ,DEBCONTAXRTCLS")
        sqlStat.AppendLine("  ,DEBSIMINPCLS")
        sqlStat.AppendLine("  ,DEBAMOUNT")
        sqlStat.AppendLine("  ,DEBCONSTAXAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURRATE")
        sqlStat.AppendLine("  ,DEBFORCURTRDCLS")
        sqlStat.AppendLine("  ,CRESUBJECT")
        sqlStat.AppendLine("  ,CRESECTION")
        sqlStat.AppendLine("  ,CREBANK")
        sqlStat.AppendLine("  ,CREPARTNER")
        sqlStat.AppendLine("  ,CREGENPURPOSE")
        sqlStat.AppendLine("  ,CRESEGMENT1")
        sqlStat.AppendLine("  ,CRESEGMENT2")
        sqlStat.AppendLine("  ,CRESEGMENT3")
        sqlStat.AppendLine("  ,CRENO1")
        sqlStat.AppendLine("  ,CRENO2")
        sqlStat.AppendLine("  ,CRECONTAXCLS")
        sqlStat.AppendLine("  ,CRECONTAXCODE")
        sqlStat.AppendLine("  ,CRECONTAXRTCLS")
        sqlStat.AppendLine("  ,CRESIMINPCLS")
        sqlStat.AppendLine("  ,CREAMOUNT")
        sqlStat.AppendLine("  ,CRECONSTAXAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURRATE")
        sqlStat.AppendLine("  ,CREFORCURTRDCLS")
        sqlStat.AppendLine("  ,DEADLINE")
        sqlStat.AppendLine("  ,SUMMARY")
        sqlStat.AppendLine("  ,SUMMARYCODE")
        sqlStat.AppendLine("  ,CREATEDDATE")
        sqlStat.AppendLine("  ,CREATEDTIME")
        sqlStat.AppendLine("  ,AUTHOR")
        sqlStat.AppendLine("  ,WORKC1")
        sqlStat.AppendLine("  ,WORKC2")
        sqlStat.AppendLine("  ,WORKC3")
        sqlStat.AppendLine("  ,WORKF1")
        sqlStat.AppendLine("  ,WORKF2")
        sqlStat.AppendLine("  ,WORKF3")
        sqlStat.AppendLine("  ,DELFLG")
        sqlStat.AppendLine("  ,INITYMD")
        sqlStat.AppendLine("  ,UPDYMD")
        sqlStat.AppendLine("  ,UPDUSER")
        sqlStat.AppendLine("  ,UPDTERMID")
        sqlStat.AppendLine("  ,RECEIVEYMD")

        sqlStat.AppendLine(" )")

        sqlStat.AppendLine(" SELECT ")
        sqlStat.AppendLine("    @REPORTMONTH as 'CLOSINGMONTH', ")
        sqlStat.AppendLine("    @CLOSINGGROUP as 'CLOSINGGROUP', ")
        sqlStat.AppendLine("    @ACCCURRENCYSEGMENT as ACCCURRENCYSEGMENT, ")
        sqlStat.AppendLine("    @BOTHCLASS as BOTHCLASS, ")
        sqlStat.AppendLine("    @ISREFCUSTOMERMST as ISREFCUSTOMERMST, ")
        sqlStat.AppendLine("    '0' as 'DATACRITERIA', ")
        sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'F' then '1010' else '1002' end as 'JOURNALENTRY',")
        sqlStat.AppendLine("    '11' as 'INPUTSCREENNO',")
        sqlStat.AppendLine("    convert(char(10),EOMONTH(convert(datetime,@REPORTMONTH+'/01')),111) as 'DOCUMENTDATE',")
        sqlStat.AppendLine("    '0' as 'SETTLEMONTHCLS',")
        sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'F' then 'G9' else 'F9' end as 'PROOFNO',")
        sqlStat.AppendLine("    '--' as 'SLIPNUMBER',")
        sqlStat.AppendLine("    '--' as 'SLIPNO',")
        sqlStat.AppendLine("    '--' as 'DETAILLINENO',")

        '日割り按分用は借方、貸方に逆勘定科目で設定
        sqlStat.AppendLine("    -- 借方")
        ' 07.26改S
        'sqlStat.AppendLine("    case ")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'Y' then am.CRACCOUNT")
        'sqlStat.AppendLine("         when @ACCCURRENCYSEGMENT = 'F' then am.CRACCOUNTFORIGN")
        'sqlStat.AppendLine("    end 'CRESUBJECT',")
        'sqlStat.AppendLine("    am.CRESUBJECT as 'CRESUBJECT',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("            when substring(am.CRESUBJECT,1,1) = '1' then @SUBJECTS_RECEIVABLE")
        sqlStat.AppendLine("            when substring(am.CRESUBJECT,1,1) = '2' then @SUBJECTS_UNPAID")
        sqlStat.AppendLine("            else am.CRESUBJECT")
        sqlStat.AppendLine("    end 'CRESUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("    @DEPARTMENT as 'CRESECTION',")
        sqlStat.AppendLine("    tc.BANKCODE as 'CREBANK',")
        'sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'Y' then '0001' else tc.BANKCODE end as 'CREBANK',")
        sqlStat.AppendLine("    am.CREPARTNER as 'CREPARTNER',")
        sqlStat.AppendLine("    am.CREGENPURPOSE as 'CREGENPURPOSE',")
        sqlStat.AppendLine("    am.CRESEGMENT1 as 'CRESEGMENT1',")
        sqlStat.AppendLine("    @DEBSEGMENT2 as 'CRESEGMENT2',")
        sqlStat.AppendLine("    @DEBSEGMENT3 as 'CRESEGMENT3',")
        sqlStat.AppendLine("    '99999999' as 'CRENO1',")
        sqlStat.AppendLine("    '9999' as 'CRENO2',")
        sqlStat.AppendLine("    am.CRECONTAXCLS as 'CRECONTAXCLS',")
        sqlStat.AppendLine("    am.CRECONTAXCODE as 'CRECONTAXCODE',")
        sqlStat.AppendLine("    am.CRECONTAXRTCLS as 'CRECONTAXRTCLS',")
        sqlStat.AppendLine("    '0' as 'CRESIMINPCLS',")
        sqlStat.AppendLine("    am.CREAMOUNT as 'CREAMOUNT',")
        sqlStat.AppendLine("    am.CRECONSTAXAMOUNT as 'CRECONSTAXAMOUNT',")
        sqlStat.AppendLine("    am.CREFORCURAMOUNT as 'CREFORCURAMOUNT',")
        sqlStat.AppendLine("    am.DEBFORCURRATE as 'CREFORCURRATE',")
        sqlStat.AppendLine("    0 as 'CREFORCURTRDCLS',")

        sqlStat.AppendLine("    --貸方")
        ' 07.26改S
        'sqlStat.AppendLine("    case ")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then am.DBACCOUNT")
        'sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' then am.DBACCOUNTFORIGN")
        'sqlStat.AppendLine("    end 'DEBSUBJECT',")
        'sqlStat.AppendLine("    am.DEBSUBJECT as 'DEBSUBJECT',")
        sqlStat.AppendLine("    case ")
        sqlStat.AppendLine("            when substring(am.DEBSUBJECT,1,1) = '1' then @SUBJECTS_RECEIVABLE")
        sqlStat.AppendLine("            when substring(am.DEBSUBJECT,1,1) = '2' then @SUBJECTS_UNPAID")
        sqlStat.AppendLine("            else am.DEBSUBJECT")
        sqlStat.AppendLine("    end 'DEBSUBJECT',")
        ' 07.26改E
        sqlStat.AppendLine("    @DEPARTMENT as 'DEBSECTION',")
        sqlStat.AppendLine("    td.BANKCODE as 'DEBBANK',")
        'sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'Y' then '0001' else td.BANKCODE end as 'DEBBANK',")
        sqlStat.AppendLine("    am.DEBPARTNER as 'DEBPARTNER',")
        sqlStat.AppendLine("    am.DEBGENPURPOSE as 'DEBGENPURPOSE',")
        sqlStat.AppendLine("    am.DEBSEGMENT1 as 'DEBSEGMENT1',")
        sqlStat.AppendLine("    @DEBSEGMENT2 as 'DEBSEGMENT2',")
        sqlStat.AppendLine("    @DEBSEGMENT3 as 'DEBSEGMENT3',")
        sqlStat.AppendLine("    '99999999' as 'DEBNO1',")
        sqlStat.AppendLine("    '9999' as 'DEBNO2',")
        sqlStat.AppendLine("    am.DEBCONTAXCLS as 'DEBCONTAXCLS',")
        sqlStat.AppendLine("    am.DEBCONTAXCODE as 'DEBCONTAXCODE',")
        sqlStat.AppendLine("    am.DEBCONTAXRTCLS as 'DEBCONTAXRTCLS',")
        sqlStat.AppendLine("    '0' as 'DEBSIMINPCLS',")
        sqlStat.AppendLine("    am.DEBAMOUNT as 'DEBAMOUNT',")
        sqlStat.AppendLine("    am.DEBCONSTAXAMOUNT as 'DEBCONSTAXAMOUNT',")
        sqlStat.AppendLine("    am.DEBFORCURAMOUNT as 'DEBFORCURAMOUNT',")
        sqlStat.AppendLine("    am.DEBFORCURRATE as 'DEBFORCURRATE',")
        sqlStat.AppendLine("    0 as 'DEBFORCURTRDCLS',")
        sqlStat.AppendLine("    '2099/12/31' as 'DEADLINE',")
        sqlStat.AppendLine("    '未着・未洗浄按分' as 'SUMMARY',")
        sqlStat.AppendLine("    am.SUMMARYCODE as 'SUMMARYCODE',")
        sqlStat.AppendLine("    CONVERT(varchar, @ENTYMD, 112) as 'CREATEDDATE',")
        sqlStat.AppendLine("    REPLACE(CONVERT(varchar, @ENTYMD, 108), ':', '') as 'CREATEDTIME',")
        sqlStat.AppendLine("    @UPDUSER as 'AUTHOR',")
        'sqlStat.AppendLine("    '' as 'WORKC1',")
        If isJotPrint = True Then
            sqlStat.AppendLine("    'JOT' as 'WORKC1',")
        Else
            sqlStat.AppendLine("    case when @ACCCURRENCYSEGMENT = 'F' then isnull(cty.NAMESJP,'') else '' end as WORKC1,")
        End If
        sqlStat.AppendLine("    '3' as 'WORKC2',") '※出力ソート用
        sqlStat.AppendLine("    '' as 'WORKC3',")
        sqlStat.AppendLine("    0.0 as 'WORKF1',")
        sqlStat.AppendLine("    0.0 as 'WORKF2',")
        sqlStat.AppendLine("    0.0 as 'WORKF3',")
        sqlStat.AppendLine("    '" & CONST_FLAG_NO & "' as 'DELFLG',")
        sqlStat.AppendLine("    @ENTYMD as 'INITYMD',")
        sqlStat.AppendLine("    @ENTYMD as 'UPDYMD',")
        sqlStat.AppendLine("    @UPDUSER as 'UPDUSER',")
        sqlStat.AppendLine("    @UPDTERMID as 'UPDTERMID',")
        sqlStat.AppendLine("    @RECEIVEYMD as 'RECEIVEYMD'")
        sqlStat.AppendLine("from")
        sqlStat.AppendLine("(")
        sqlStat.AppendLine("    select")
        sqlStat.AppendLine("        aw.COSTTYPE,")
        sqlStat.AppendLine("        ---- 借方")
        ' 07.26改S
        'sqlStat.AppendLine("        aw.DBACCOUNT,")
        'sqlStat.AppendLine("        aw.DBACCOUNTFORIGN,")
        'sqlStat.AppendLine("        aw.OFFDBACCOUNT,")
        'sqlStat.AppendLine("        aw.OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("        case ")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then aw.CRACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' then aw.CRACCOUNTFORIGN")
        sqlStat.AppendLine("            else ''")
        sqlStat.AppendLine("        end 'CRESUBJECT',")
        ' 07.26改S
        sqlStat.AppendLine("        @TORICODE as 'DEBPARTNER',")
        sqlStat.AppendLine("        aw.DEBGENPURPOSE,")
        sqlStat.AppendLine("        aw.DBSEGMENT1 as 'DEBSEGMENT1',")
        sqlStat.AppendLine("        aw.COSTTYPE as 'DEBCONTAXCLS',")
        'sqlStat.AppendLine("        case when aw.TAXATION = '0' then '40' else '20' end as 'DEBCONTAXCODE',")
        'sqlStat.AppendLine("        case when aw.TAXATION = '0' then '0' else fv.VALUE1 end as 'DEBCONTAXRTCLS',")
        sqlStat.AppendLine("        '40' as 'DEBCONTAXCODE',")
        sqlStat.AppendLine("        '0' as 'DEBCONTAXRTCLS',")
        If targetRowItem("ACCCURRENCYSEGMENT").ToString = "F" Then
            'sqlStat.AppendLine("        round(round(sum(aw.UAG_JPY),0) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS,0) as 'DEBAMOUNT',")
            sqlStat.AppendLine("        round(round(sum(case when @ACCCURRENCYSEGMENT = 'F' then aw.UAG_USD else 0 end) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS, 2) * aw.REPORTRATEJPY,0) as 'DEBAMOUNT',")
            'sqlStat.AppendLine("        round(round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY end),0) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS * (isnull(la.TAXRATE,@TAXRATE) / 100.0),0) as 'DEBCONSTAXAMOUNT',")
            sqlStat.AppendLine("        0 as 'DEBCONSTAXAMOUNT',")
        Else
            sqlStat.AppendLine("        round(round(sum(aw.UAG_JPY_SHIP),0) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS,0) as 'DEBAMOUNT',")
            'sqlStat.AppendLine("        round(round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY_SHIP end),0) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS * (isnull(la.TAXRATE,@TAXRATE) / 100.0),0) as 'DEBCONSTAXAMOUNT',")
            sqlStat.AppendLine("        0 as 'DEBCONSTAXAMOUNT',")
        End If
        sqlStat.AppendLine("        round(sum(case when @ACCCURRENCYSEGMENT = 'F' then aw.UAG_USD else 0 end) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS, 2) as 'DEBFORCURAMOUNT',")
        sqlStat.AppendLine("        case when @ACCCURRENCYSEGMENT = 'F' then aw.REPORTRATEJPY else 0 end as 'DEBFORCURRATE',")
        sqlStat.AppendLine("        -- 貸方")
        ' 07.26改S
        'sqlStat.AppendLine("        aw.CRACCOUNT,")
        'sqlStat.AppendLine("        aw.CRACCOUNTFORIGN,")
        'sqlStat.AppendLine("        aw.OFFCRACCOUNT,")
        'sqlStat.AppendLine("        aw.OFFCRACCOUNTFORIGN,")
        sqlStat.AppendLine("        case ")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'Y' then aw.DBACCOUNT")
        sqlStat.AppendLine("            when @ACCCURRENCYSEGMENT = 'F' then aw.DBACCOUNTFORIGN")
        sqlStat.AppendLine("            else ''")
        sqlStat.AppendLine("        end 'DEBSUBJECT',")
        ' 07.26改S
        sqlStat.AppendLine("        @TORICODE as 'CREPARTNER',")
        sqlStat.AppendLine("        aw.CREGENPURPOSE,")
        sqlStat.AppendLine("        aw.CRSEGMENT1 as 'CRESEGMENT1',")
        sqlStat.AppendLine("        aw.COSTTYPE as 'CRECONTAXCLS',")
        'sqlStat.AppendLine("        case when aw.TAXATION = '0' then '40' else '20' end as 'CRECONTAXCODE',")
        'sqlStat.AppendLine("        case when aw.TAXATION = '0' then '0' else fv.VALUE1 end as 'CRECONTAXRTCLS',")
        sqlStat.AppendLine("        '40' as 'CRECONTAXCODE',")
        sqlStat.AppendLine("        '0' as 'CRECONTAXRTCLS',")
        If targetRowItem("ACCCURRENCYSEGMENT").ToString = "F" Then
            'sqlStat.AppendLine("        round(round(sum(aw.UAG_JPY),0) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS,0) as 'CREAMOUNT',")
            sqlStat.AppendLine("        round( round(sum(case when @ACCCURRENCYSEGMENT = 'F' then aw.UAG_USD else 0 end) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS, 2) * aw.REPORTRATEJPY ,0) as 'CREAMOUNT',")
            'sqlStat.AppendLine("        round(round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY end),0) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS * (isnull(la.TAXRATE,@TAXRATE) / 100.0), 0) as 'CRECONSTAXAMOUNT',")
            sqlStat.AppendLine("        0 as 'CRECONSTAXAMOUNT',")
        Else
            sqlStat.AppendLine("        round(round(sum(aw.UAG_JPY_SHIP),0) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS,0) as 'CREAMOUNT',")
            'sqlStat.AppendLine("        round(round(sum(case when TAXATION = '0' then 0 else aw.UAG_JPY_SHIP end),0) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS * (isnull(la.TAXRATE,@TAXRATE) / 100.0), 0) as 'CRECONSTAXAMOUNT',")
            sqlStat.AppendLine("        0 as 'CRECONSTAXAMOUNT',")
        End If
        sqlStat.AppendLine("        round(sum(case when @ACCCURRENCYSEGMENT = 'F' then aw.UAG_USD else 0 end) * (aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) / aw.ROUTEDAYS, 2) as 'CREFORCURAMOUNT',")
        sqlStat.AppendLine("        case when @ACCCURRENCYSEGMENT = 'F' then aw.REPORTRATEJPY else 0 end as 'CREFORCURRATE',")
        'sqlStat.AppendLine("        case when aw.COSTTYPE = '1' then '000000' else '000001' end as 'SUMMARYCODE'")
        sqlStat.AppendLine("        '' as 'SUMMARYCODE'")
        sqlStat.AppendLine("    from GBT0015_AC_WORK aw")
        'sqlStat.AppendLine("    left outer join  GBT0011_LBR_AGREEMENT la")
        'sqlStat.AppendLine("      on  la.RELATEDORDERNO = aw.ORDERNO")
        'sqlStat.AppendLine("      and la.DELFLG <> @DELFLG")
        'sqlStat.AppendLine("      and aw.COSTCODE in ('S0103-01','S0103-02','S0103-03')")
        'sqlStat.AppendLine("    inner join  COS0017_FIXVALUE fv")
        'sqlStat.AppendLine("      on  fv.SYSCODE = 'GB'")
        'sqlStat.AppendLine("      and fv.CLASS = 'SALESTAX'")
        'sqlStat.AppendLine("      and fv.KEYCODE = trim(convert(char,isnull(la.TAXRATE,@TAXRATE)))")
        'sqlStat.AppendLine("      and fv.DELFLG <> @DELFLG")
        'sqlStat.AppendLine("      and fv.STYMD <= @ENTYMD")
        'sqlStat.AppendLine("      and fv.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("    where aw.DELFLG <> 'Y'")
        sqlStat.AppendLine("    and aw.CLOSINGGROUP = @CLOSINGGROUP")
        If isJotPrint = True Then
            sqlStat.AppendLine("    and aw.CONTRACTORFIX = @CONTRACTOR")
            If targetRowItem("ISREFCUSTOMERMST").Equals("1") Then
                sqlStat.AppendLine("    and aw.COSTTYPE = '1'")
            Else
                sqlStat.AppendLine("    and aw.COSTTYPE = '2'")
            End If
        End If
        sqlStat.AppendLine("    and aw.CLOSINGMONTH = @REPORTMONTH")
        ' デマレージはSHIPに依存しない
        'sqlStat.AppendLine("    and (aw.SHIPDATE < @BASESHIP And aw.SHIPDATE <> '1900/01/01')")
        sqlStat.AppendLine("    and (( aw.COSTCODE <> 'S0102-01' and aw.SHIPDATE < @BASESHIP And aw.SHIPDATE <> '1900/01/01') ")
        sqlStat.AppendLine("          or (aw.COSTCODE = 'S0102-01' and aw.REPORTMONTHORG <= @REPORTMONTH))")
        sqlStat.AppendLine("    and ((aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@REPORTMONTH + '/01'))+1)) > 0 )")
        sqlStat.AppendLine("    group by aw.DBACCOUNT,")
        sqlStat.AppendLine("        aw.DBACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.OFFDBACCOUNT,")
        sqlStat.AppendLine("        aw.OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.CRACCOUNT,")
        sqlStat.AppendLine("        aw.CRACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.OFFCRACCOUNT,")
        sqlStat.AppendLine("        aw.OFFCRACCOUNTFORIGN,")
        sqlStat.AppendLine("        aw.COSTTYPE,")
        sqlStat.AppendLine("        aw.DBSEGMENT1,")
        sqlStat.AppendLine("        aw.CRSEGMENT1,")
        sqlStat.AppendLine("        aw.TAXATION,")
        sqlStat.AppendLine("        aw.REPORTRATEJPY,")
        sqlStat.AppendLine("        aw.DEBGENPURPOSE,")
        sqlStat.AppendLine("        aw.CREGENPURPOSE,")
        sqlStat.AppendLine("        aw.ROUTEDAYS,")
        sqlStat.AppendLine("        aw.DOUTDATE")
        'sqlStat.AppendLine("        fv.VALUE1")
        sqlStat.AppendLine(") am")
        sqlStat.AppendLine("inner join GBM0025_TORI td")
        sqlStat.AppendLine("    on td.TORICODE = am.DEBPARTNER")
        sqlStat.AppendLine("    and td.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and td.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("    and td.DELFLG <> @DELFLG")
        sqlStat.AppendLine("inner join GBM0025_TORI tc")
        sqlStat.AppendLine("    on tc.TORICODE = am.CREPARTNER")
        sqlStat.AppendLine("    and tc.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and tc.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("    and tc.DELFLG <> @DELFLG")
        sqlStat.AppendLine("left join GBM0001_COUNTRY cty")
        sqlStat.AppendLine("    on cty.DELFLG <> @DELFLG")
        sqlStat.AppendLine("    and cty.COUNTRYCODE = @CLOSINGGROUP")
        sqlStat.AppendLine("    and cty.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and cty.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("where ( am.DEBAMOUNT <> 0.0 or am.CREAMOUNT <> 0.0)")


        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
            sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters

                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = ReportMonth
                .Add("@DEPARTMENT", SqlDbType.NVarChar).Value = Department
                .Add("@DEBSEGMENT3", SqlDbType.NVarChar).Value = "30"

                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = ReportMonth
                .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = targetRowItem("ACCCURRENCYSEGMENT")
                .Add("@BOTHCLASS", SqlDbType.NVarChar).Value = "B"
                If isJotPrint = True Then
                    '.Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = "Y"
                    '.Add("@BOTHCLASS", SqlDbType.NVarChar).Value = "B"
                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = "JP"
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = CONST_CURRENTXML_JOT
                    .Add("@CONTRACTOR", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                Else
                    '.Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = targetRowItem("ACCCURRENCYSEGMENT")
                    '.Add("@BOTHCLASS", SqlDbType.NVarChar).Value = targetRowItem("BOTHCLASS")
                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                End If
                .Add("@ISREFCUSTOMERMST", SqlDbType.NVarChar).Value = targetRowItem("ISREFCUSTOMERMST")

            End With
            sqlCmd.ExecuteNonQuery()
        End Using

    End Sub

    Public Sub InsAcValueTentativeCan(ByVal ReportMonth As String, ByVal Department As String, ByVal targetRowItem As DataRow, isJotPrint As Boolean)

        Const CONST_SUMMARY As String = "取消　未着・未洗浄ＴＨＯＭＡＳ"
        Dim sqlStat As New StringBuilder

        ' 取消　未着・未洗浄ＴＨＯＭＡＳ
        sqlStat.AppendLine("DECLARE @TORICODE nvarchar(10);")
        sqlStat.AppendLine("select @TORICODE = VALUE1 from COS0017_FIXVALUE fv where fv.SYSCODE = 'GB' and fv.CLASS = 'AC_SYSCODE' and fv.KEYCODE = 'THOMAS' and fv.STYMD <= @ENTYMD and fv.ENDYMD >= @ENTYMD and fv.DELFLG <> 'Y';")

        sqlStat.AppendLine("INSERT INTO GBT0014_AC_VALUE")
        sqlStat.AppendLine(" (")
        sqlStat.AppendLine("   CLOSINGMONTH")
        sqlStat.AppendLine("  ,CLOSINGGROUP")
        sqlStat.AppendLine("  ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("  ,BOTHCLASS")
        sqlStat.AppendLine("  ,ISREFCUSTOMERMST")
        sqlStat.AppendLine("  ,DATACRITERIA")
        sqlStat.AppendLine("  ,JOURNALENTRY")
        sqlStat.AppendLine("  ,INPUTSCREENNO")
        sqlStat.AppendLine("  ,DOCUMENTDATE")
        sqlStat.AppendLine("  ,SETTLEMONTHCLS")
        sqlStat.AppendLine("  ,PROOFNO")
        sqlStat.AppendLine("  ,SLIPNUMBER")
        sqlStat.AppendLine("  ,SLIPNO")
        sqlStat.AppendLine("  ,DETAILLINENO")
        sqlStat.AppendLine("  ,DEBSUBJECT")
        sqlStat.AppendLine("  ,DEBSECTION")
        sqlStat.AppendLine("  ,DEBBANK")
        sqlStat.AppendLine("  ,DEBPARTNER")
        sqlStat.AppendLine("  ,DEBGENPURPOSE")
        sqlStat.AppendLine("  ,DEBSEGMENT1")
        sqlStat.AppendLine("  ,DEBSEGMENT2")
        sqlStat.AppendLine("  ,DEBSEGMENT3")
        sqlStat.AppendLine("  ,DEBNO1")
        sqlStat.AppendLine("  ,DEBNO2")
        sqlStat.AppendLine("  ,DEBCONTAXCLS")
        sqlStat.AppendLine("  ,DEBCONTAXCODE")
        sqlStat.AppendLine("  ,DEBCONTAXRTCLS")
        sqlStat.AppendLine("  ,DEBSIMINPCLS")
        sqlStat.AppendLine("  ,DEBAMOUNT")
        sqlStat.AppendLine("  ,DEBCONSTAXAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURRATE")
        sqlStat.AppendLine("  ,DEBFORCURTRDCLS")
        sqlStat.AppendLine("  ,CRESUBJECT")
        sqlStat.AppendLine("  ,CRESECTION")
        sqlStat.AppendLine("  ,CREBANK")
        sqlStat.AppendLine("  ,CREPARTNER")
        sqlStat.AppendLine("  ,CREGENPURPOSE")
        sqlStat.AppendLine("  ,CRESEGMENT1")
        sqlStat.AppendLine("  ,CRESEGMENT2")
        sqlStat.AppendLine("  ,CRESEGMENT3")
        sqlStat.AppendLine("  ,CRENO1")
        sqlStat.AppendLine("  ,CRENO2")
        sqlStat.AppendLine("  ,CRECONTAXCLS")
        sqlStat.AppendLine("  ,CRECONTAXCODE")
        sqlStat.AppendLine("  ,CRECONTAXRTCLS")
        sqlStat.AppendLine("  ,CRESIMINPCLS")
        sqlStat.AppendLine("  ,CREAMOUNT")
        sqlStat.AppendLine("  ,CRECONSTAXAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURRATE")
        sqlStat.AppendLine("  ,CREFORCURTRDCLS")
        sqlStat.AppendLine("  ,DEADLINE")
        sqlStat.AppendLine("  ,SUMMARY")
        sqlStat.AppendLine("  ,SUMMARYCODE")
        sqlStat.AppendLine("  ,CREATEDDATE")
        sqlStat.AppendLine("  ,CREATEDTIME")
        sqlStat.AppendLine("  ,AUTHOR")
        sqlStat.AppendLine("  ,WORKC1")
        sqlStat.AppendLine("  ,WORKC2")
        sqlStat.AppendLine("  ,WORKC3")
        sqlStat.AppendLine("  ,WORKF1")
        sqlStat.AppendLine("  ,WORKF2")
        sqlStat.AppendLine("  ,WORKF3")
        sqlStat.AppendLine("  ,DELFLG")
        sqlStat.AppendLine("  ,INITYMD")
        sqlStat.AppendLine("  ,UPDYMD")
        sqlStat.AppendLine("  ,UPDUSER")
        sqlStat.AppendLine("  ,UPDTERMID")
        sqlStat.AppendLine("  ,RECEIVEYMD")

        sqlStat.AppendLine(" )")

        sqlStat.AppendLine(" SELECT ")
        sqlStat.AppendLine("   @CLOSINGMONTH")
        sqlStat.AppendLine("  ,av.CLOSINGGROUP")
        sqlStat.AppendLine("  ,av.ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("  ,av.BOTHCLASS")
        sqlStat.AppendLine("  ,av.ISREFCUSTOMERMST")
        sqlStat.AppendLine("  ,av.DATACRITERIA")
        sqlStat.AppendLine("  ,av.JOURNALENTRY")
        sqlStat.AppendLine("  ,av.INPUTSCREENNO")
        sqlStat.AppendLine("  ,convert(char(10),EOMONTH(convert(datetime,@REPORTMONTH+'/01')),111) as 'DOCUMENTDATE'")
        sqlStat.AppendLine("  ,av.SETTLEMONTHCLS")
        sqlStat.AppendLine("  ,av.PROOFNO")
        sqlStat.AppendLine("  ,av.SLIPNUMBER")
        sqlStat.AppendLine("  ,av.SLIPNO")
        sqlStat.AppendLine("  ,av.DETAILLINENO")
        ' 取り消しデータは、貸方と借方を入れ替える
        sqlStat.AppendLine("  ,av.CRESUBJECT")
        sqlStat.AppendLine("  ,av.CRESECTION")
        sqlStat.AppendLine("  ,av.CREBANK")
        sqlStat.AppendLine("  ,av.CREPARTNER")
        sqlStat.AppendLine("  ,av.CREGENPURPOSE")
        sqlStat.AppendLine("  ,av.CRESEGMENT1")
        sqlStat.AppendLine("  ,av.CRESEGMENT2")
        sqlStat.AppendLine("  ,av.CRESEGMENT3")
        'sqlStat.AppendLine("  ,av.CRENO1")
        'sqlStat.AppendLine("  ,av.CRENO2")
        sqlStat.AppendLine("  ,''")
        sqlStat.AppendLine("  ,''")
        sqlStat.AppendLine("  ,av.CRECONTAXCLS")
        sqlStat.AppendLine("  ,av.CRECONTAXCODE")
        sqlStat.AppendLine("  ,av.CRECONTAXRTCLS")
        sqlStat.AppendLine("  ,av.CRESIMINPCLS")
        sqlStat.AppendLine("  ,av.CREAMOUNT")
        sqlStat.AppendLine("  ,av.CRECONSTAXAMOUNT")
        sqlStat.AppendLine("  ,av.CREFORCURAMOUNT")
        sqlStat.AppendLine("  ,av.CREFORCURRATE")
        sqlStat.AppendLine("  ,av.CREFORCURTRDCLS")

        sqlStat.AppendLine("  ,av.DEBSUBJECT")
        sqlStat.AppendLine("  ,av.DEBSECTION")
        sqlStat.AppendLine("  ,av.DEBBANK")
        sqlStat.AppendLine("  ,av.DEBPARTNER")
        sqlStat.AppendLine("  ,av.DEBGENPURPOSE")
        sqlStat.AppendLine("  ,av.DEBSEGMENT1")
        sqlStat.AppendLine("  ,av.DEBSEGMENT2")
        sqlStat.AppendLine("  ,av.DEBSEGMENT3")
        'sqlStat.AppendLine("  ,av.DEBNO1")
        'sqlStat.AppendLine("  ,av.DEBNO2")
        sqlStat.AppendLine("  ,''")
        sqlStat.AppendLine("  ,''")
        sqlStat.AppendLine("  ,av.DEBCONTAXCLS")
        sqlStat.AppendLine("  ,av.DEBCONTAXCODE")
        sqlStat.AppendLine("  ,av.DEBCONTAXRTCLS")
        sqlStat.AppendLine("  ,av.DEBSIMINPCLS")
        sqlStat.AppendLine("  ,av.DEBAMOUNT")
        sqlStat.AppendLine("  ,av.DEBCONSTAXAMOUNT")
        sqlStat.AppendLine("  ,av.DEBFORCURAMOUNT")
        sqlStat.AppendLine("  ,av.DEBFORCURRATE")
        sqlStat.AppendLine("  ,av.DEBFORCURTRDCLS")
        sqlStat.AppendLine("  ,av.DEADLINE")
        sqlStat.AppendLine("  ,'" & CONST_SUMMARY & "' as SUMMARY")
        sqlStat.AppendLine("  ,av.SUMMARYCODE")
        sqlStat.AppendLine("  ,CONVERT(varchar, @ENTYMD, 112) as 'CREATEDDATE'")
        sqlStat.AppendLine("  ,REPLACE(CONVERT(varchar, @ENTYMD, 108), ':', '') as 'CREATEDTIME'")
        sqlStat.AppendLine("  ,@UPDUSER as 'AUTHOR'")
        'sqlStat.AppendLine("  ,'' as 'WORKC1'")
        If isJotPrint = True Then
            sqlStat.AppendLine("  ,'' as 'WORKC1'")
        Else
            sqlStat.AppendLine("  ,case when av.ACCCURRENCYSEGMENT = 'F' then isnull(cty.NAMESJP,'') else '' end as WORKC1")
        End If
        sqlStat.AppendLine("  ,'2' as 'WORKC2'") '※出力ソート用
        sqlStat.AppendLine("  ,'' as 'WORKC3'")
        sqlStat.AppendLine("  ,0.0 as 'WORKF1'")
        sqlStat.AppendLine("  ,0.0 as 'WORKF2'")
        sqlStat.AppendLine("  ,0.0 as 'WORKF3'")
        sqlStat.AppendLine("  ,'" & CONST_FLAG_NO & "' as 'DELFLG'")
        sqlStat.AppendLine("  ,@ENTYMD as 'INITYMD'")
        sqlStat.AppendLine("  ,@ENTYMD as 'UPDYMD'")
        sqlStat.AppendLine("  ,@UPDUSER as 'UPDUSER'")
        sqlStat.AppendLine("  ,@UPDTERMID as 'UPDTERMID'")
        sqlStat.AppendLine("  ,@RECEIVEYMD as 'RECEIVEYMD'")
        sqlStat.AppendLine("from GBT0014_AC_VALUE av")
        sqlStat.AppendLine("left join GBM0001_COUNTRY cty")
        sqlStat.AppendLine("    on cty.DELFLG <> @DELFLG")
        sqlStat.AppendLine("    and cty.COUNTRYCODE = @CLOSINGGROUP")
        sqlStat.AppendLine("    and cty.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and cty.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("where av.CLOSINGMONTH = @BFCLOSINGMONTH")
        sqlStat.AppendLine("and   av.CLOSINGGROUP = @CLOSINGGROUP")
        sqlStat.AppendLine("and   av.DEBPARTNER = @TORICODE")
        sqlStat.AppendLine("and   av.SUMMARY <> '" & CONST_SUMMARY & "'")
        sqlStat.AppendLine("and   av.DELFLG <> @DELFLG")

        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
            sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters

                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = ReportMonth
                .Add("@BFCLOSINGMONTH", SqlDbType.NVarChar).Value = CDate(ReportMonth & "/01").AddMonths(-1).ToString("yyyy/MM")
                .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = ReportMonth
                If isJotPrint = True Then
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = CONST_CURRENTXML_JOT
                Else
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                End If

            End With
            sqlCmd.ExecuteNonQuery()
        End Using

    End Sub

    Public Sub InsAcValueDailyRateCan(ByVal ReportMonth As String, ByVal Department As String, ByVal targetRowItem As DataRow, isJotPrint As Boolean)

        Const CONST_SUMMARY As String = "取消　未着・未洗浄按分"
        Dim sqlStat As New StringBuilder

        ' 取り消し　未着・未洗浄按分
        sqlStat.AppendLine("DECLARE @TORICODE nvarchar(10);")
        sqlStat.AppendLine("select @TORICODE = VALUE1 from COS0017_FIXVALUE fv where fv.SYSCODE = 'GB' and fv.CLASS = 'AC_SYSCODE' and fv.KEYCODE = 'DIVIDE' and fv.STYMD <= @ENTYMD and fv.ENDYMD >= @ENTYMD and fv.DELFLG <> 'Y';")

        sqlStat.AppendLine("INSERT INTO GBT0014_AC_VALUE")
        sqlStat.AppendLine(" (")
        sqlStat.AppendLine("   CLOSINGMONTH")
        sqlStat.AppendLine("  ,CLOSINGGROUP")
        sqlStat.AppendLine("  ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("  ,BOTHCLASS")
        sqlStat.AppendLine("  ,ISREFCUSTOMERMST")
        sqlStat.AppendLine("  ,DATACRITERIA")
        sqlStat.AppendLine("  ,JOURNALENTRY")
        sqlStat.AppendLine("  ,INPUTSCREENNO")
        sqlStat.AppendLine("  ,DOCUMENTDATE")
        sqlStat.AppendLine("  ,SETTLEMONTHCLS")
        sqlStat.AppendLine("  ,PROOFNO")
        sqlStat.AppendLine("  ,SLIPNUMBER")
        sqlStat.AppendLine("  ,SLIPNO")
        sqlStat.AppendLine("  ,DETAILLINENO")
        sqlStat.AppendLine("  ,DEBSUBJECT")
        sqlStat.AppendLine("  ,DEBSECTION")
        sqlStat.AppendLine("  ,DEBBANK")
        sqlStat.AppendLine("  ,DEBPARTNER")
        sqlStat.AppendLine("  ,DEBGENPURPOSE")
        sqlStat.AppendLine("  ,DEBSEGMENT1")
        sqlStat.AppendLine("  ,DEBSEGMENT2")
        sqlStat.AppendLine("  ,DEBSEGMENT3")
        sqlStat.AppendLine("  ,DEBNO1")
        sqlStat.AppendLine("  ,DEBNO2")
        sqlStat.AppendLine("  ,DEBCONTAXCLS")
        sqlStat.AppendLine("  ,DEBCONTAXCODE")
        sqlStat.AppendLine("  ,DEBCONTAXRTCLS")
        sqlStat.AppendLine("  ,DEBSIMINPCLS")
        sqlStat.AppendLine("  ,DEBAMOUNT")
        sqlStat.AppendLine("  ,DEBCONSTAXAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURAMOUNT")
        sqlStat.AppendLine("  ,DEBFORCURRATE")
        sqlStat.AppendLine("  ,DEBFORCURTRDCLS")
        sqlStat.AppendLine("  ,CRESUBJECT")
        sqlStat.AppendLine("  ,CRESECTION")
        sqlStat.AppendLine("  ,CREBANK")
        sqlStat.AppendLine("  ,CREPARTNER")
        sqlStat.AppendLine("  ,CREGENPURPOSE")
        sqlStat.AppendLine("  ,CRESEGMENT1")
        sqlStat.AppendLine("  ,CRESEGMENT2")
        sqlStat.AppendLine("  ,CRESEGMENT3")
        sqlStat.AppendLine("  ,CRENO1")
        sqlStat.AppendLine("  ,CRENO2")
        sqlStat.AppendLine("  ,CRECONTAXCLS")
        sqlStat.AppendLine("  ,CRECONTAXCODE")
        sqlStat.AppendLine("  ,CRECONTAXRTCLS")
        sqlStat.AppendLine("  ,CRESIMINPCLS")
        sqlStat.AppendLine("  ,CREAMOUNT")
        sqlStat.AppendLine("  ,CRECONSTAXAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURAMOUNT")
        sqlStat.AppendLine("  ,CREFORCURRATE")
        sqlStat.AppendLine("  ,CREFORCURTRDCLS")
        sqlStat.AppendLine("  ,DEADLINE")
        sqlStat.AppendLine("  ,SUMMARY")
        sqlStat.AppendLine("  ,SUMMARYCODE")
        sqlStat.AppendLine("  ,CREATEDDATE")
        sqlStat.AppendLine("  ,CREATEDTIME")
        sqlStat.AppendLine("  ,AUTHOR")
        sqlStat.AppendLine("  ,WORKC1")
        sqlStat.AppendLine("  ,WORKC2")
        sqlStat.AppendLine("  ,WORKC3")
        sqlStat.AppendLine("  ,WORKF1")
        sqlStat.AppendLine("  ,WORKF2")
        sqlStat.AppendLine("  ,WORKF3")
        sqlStat.AppendLine("  ,DELFLG")
        sqlStat.AppendLine("  ,INITYMD")
        sqlStat.AppendLine("  ,UPDYMD")
        sqlStat.AppendLine("  ,UPDUSER")
        sqlStat.AppendLine("  ,UPDTERMID")
        sqlStat.AppendLine("  ,RECEIVEYMD")

        sqlStat.AppendLine(" )")

        sqlStat.AppendLine(" SELECT ")
        sqlStat.AppendLine("   @CLOSINGMONTH")
        sqlStat.AppendLine("  ,av.CLOSINGGROUP")
        sqlStat.AppendLine("  ,av.ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("  ,av.BOTHCLASS")
        sqlStat.AppendLine("  ,av.ISREFCUSTOMERMST")
        sqlStat.AppendLine("  ,av.DATACRITERIA")
        sqlStat.AppendLine("  ,av.JOURNALENTRY")
        sqlStat.AppendLine("  ,av.INPUTSCREENNO")
        sqlStat.AppendLine("  ,convert(char(10),EOMONTH(convert(datetime,@REPORTMONTH+'/01')),111) as 'DOCUMENTDATE'")
        sqlStat.AppendLine("  ,av.SETTLEMONTHCLS")
        sqlStat.AppendLine("  ,av.PROOFNO")
        sqlStat.AppendLine("  ,av.SLIPNUMBER")
        sqlStat.AppendLine("  ,av.SLIPNO")
        sqlStat.AppendLine("  ,av.DETAILLINENO")
        ' 取り消しデータは、貸方と借方を入れ替える
        sqlStat.AppendLine("  ,av.CRESUBJECT")
        sqlStat.AppendLine("  ,av.CRESECTION")
        sqlStat.AppendLine("  ,av.CREBANK")
        sqlStat.AppendLine("  ,av.CREPARTNER")
        sqlStat.AppendLine("  ,av.CREGENPURPOSE")
        sqlStat.AppendLine("  ,av.CRESEGMENT1")
        sqlStat.AppendLine("  ,av.CRESEGMENT2")
        sqlStat.AppendLine("  ,av.CRESEGMENT3")
        'sqlStat.AppendLine("  ,av.CRENO1")
        'sqlStat.AppendLine("  ,av.CRENO2")
        sqlStat.AppendLine("  ,''")
        sqlStat.AppendLine("  ,''")
        sqlStat.AppendLine("  ,av.CRECONTAXCLS")
        sqlStat.AppendLine("  ,av.CRECONTAXCODE")
        sqlStat.AppendLine("  ,av.CRECONTAXRTCLS")
        sqlStat.AppendLine("  ,av.CRESIMINPCLS")
        sqlStat.AppendLine("  ,av.CREAMOUNT")
        sqlStat.AppendLine("  ,av.CRECONSTAXAMOUNT")
        sqlStat.AppendLine("  ,av.CREFORCURAMOUNT")
        sqlStat.AppendLine("  ,av.CREFORCURRATE")
        sqlStat.AppendLine("  ,av.CREFORCURTRDCLS")

        sqlStat.AppendLine("  ,av.DEBSUBJECT")
        sqlStat.AppendLine("  ,av.DEBSECTION")
        sqlStat.AppendLine("  ,av.DEBBANK")
        sqlStat.AppendLine("  ,av.DEBPARTNER")
        sqlStat.AppendLine("  ,av.DEBGENPURPOSE")
        sqlStat.AppendLine("  ,av.DEBSEGMENT1")
        sqlStat.AppendLine("  ,av.DEBSEGMENT2")
        sqlStat.AppendLine("  ,av.DEBSEGMENT3")
        'sqlStat.AppendLine("  ,av.DEBNO1")
        'sqlStat.AppendLine("  ,av.DEBNO2")
        sqlStat.AppendLine("  ,''")
        sqlStat.AppendLine("  ,''")
        sqlStat.AppendLine("  ,av.DEBCONTAXCLS")
        sqlStat.AppendLine("  ,av.DEBCONTAXCODE")
        sqlStat.AppendLine("  ,av.DEBCONTAXRTCLS")
        sqlStat.AppendLine("  ,av.DEBSIMINPCLS")
        sqlStat.AppendLine("  ,av.DEBAMOUNT")
        sqlStat.AppendLine("  ,av.DEBCONSTAXAMOUNT")
        sqlStat.AppendLine("  ,av.DEBFORCURAMOUNT")
        sqlStat.AppendLine("  ,av.DEBFORCURRATE")
        sqlStat.AppendLine("  ,av.DEBFORCURTRDCLS")
        sqlStat.AppendLine("  ,av.DEADLINE")
        sqlStat.AppendLine("  ,'" & CONST_SUMMARY & "' as SUMMARY")
        sqlStat.AppendLine("  ,av.SUMMARYCODE")
        sqlStat.AppendLine("  ,CONVERT(varchar, @ENTYMD, 112) as 'CREATEDDATE'")
        sqlStat.AppendLine("  ,REPLACE(CONVERT(varchar, @ENTYMD, 108), ':', '') as 'CREATEDTIME'")
        sqlStat.AppendLine("  ,@UPDUSER as 'AUTHOR'")
        'sqlStat.AppendLine("  ,'' as 'WORKC1'")
        If isJotPrint = True Then
            sqlStat.AppendLine("  ,'' as 'WORKC1'")
        Else
            sqlStat.AppendLine("  ,case when av.ACCCURRENCYSEGMENT = 'F' then isnull(cty.NAMESJP,'') else '' end as WORKC1")
        End If
        sqlStat.AppendLine("  ,'3' as 'WORKC2'") '※出力ソート用
        sqlStat.AppendLine("  ,'' as 'WORKC3'")
        sqlStat.AppendLine("  ,0.0 as 'WORKF1'")
        sqlStat.AppendLine("  ,0.0 as 'WORKF2'")
        sqlStat.AppendLine("  ,0.0 as 'WORKF3'")
        sqlStat.AppendLine("  ,'" & CONST_FLAG_NO & "' as 'DELFLG'")
        sqlStat.AppendLine("  ,@ENTYMD as 'INITYMD'")
        sqlStat.AppendLine("  ,@ENTYMD as 'UPDYMD'")
        sqlStat.AppendLine("  ,@UPDUSER as 'UPDUSER'")
        sqlStat.AppendLine("  ,@UPDTERMID as 'UPDTERMID'")
        sqlStat.AppendLine("  ,@RECEIVEYMD as 'RECEIVEYMD'")
        sqlStat.AppendLine("from GBT0014_AC_VALUE av")
        sqlStat.AppendLine("left join GBM0001_COUNTRY cty")
        sqlStat.AppendLine("    on cty.DELFLG <> @DELFLG")
        sqlStat.AppendLine("    and cty.COUNTRYCODE = @CLOSINGGROUP")
        sqlStat.AppendLine("    and cty.STYMD <= @ENTYMD")
        sqlStat.AppendLine("    and cty.ENDYMD >= @ENTYMD")
        sqlStat.AppendLine("where av.CLOSINGMONTH = @BFCLOSINGMONTH")
        sqlStat.AppendLine("and   av.CLOSINGGROUP = @CLOSINGGROUP")
        sqlStat.AppendLine("and   av.DEBPARTNER = @TORICODE")
        sqlStat.AppendLine("and   av.SUMMARY <> '" & CONST_SUMMARY & "'")
        sqlStat.AppendLine("and   av.DELFLG <> @DELFLG")


        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
            sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters

                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = ReportMonth
                .Add("@BFCLOSINGMONTH", SqlDbType.NVarChar).Value = CDate(ReportMonth & "/01").AddMonths(-1).ToString("yyyy/MM")
                .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = ReportMonth
                If isJotPrint = True Then
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = CONST_CURRENTXML_JOT
                Else
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = targetRowItem("COUNTRYCODE")
                End If

            End With
            sqlCmd.ExecuteNonQuery()
        End Using

    End Sub

    Private Function AddItemList(ByVal ReportMonth As String) As DataTable

        Dim retDt As DataTable = New DataTable

        Try
            '************************************
            'SQL生成
            '************************************
            Dim sqlStat As New StringBuilder()
            sqlStat.AppendLine("WITH ")
            sqlStat.AppendLine("    WITH_BASE as (")
            sqlStat.AppendLine("      SELECT CONTRACTORFIX, COSTTYPE ")
            sqlStat.AppendLine("      FROM GBT0015_AC_WORK ")
            sqlStat.AppendLine("      WHERE DELFLG <> @DELFLG ")
            sqlStat.AppendLine("        AND CLOSINGMONTH = @CLOSINGMONTH ")
            sqlStat.AppendLine("        AND CLOSINGGROUP = @CLOSINGGROUP ")
            sqlStat.AppendLine("        AND REPORTMONTH <> @CLOSINGMONTH ")
            sqlStat.AppendLine("        AND NOT EXISTS ( SELECT aw2.CONTRACTORFIX  ")
            sqlStat.AppendLine("                         FROM GBT0015_AC_WORK aw2 ")
            sqlStat.AppendLine("                         WHERE aw2.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("                         AND aw2.CLOSINGMONTH = @CLOSINGMONTH ")
            sqlStat.AppendLine("                         AND aw2.CLOSINGGROUP = @CLOSINGGROUP ")
            sqlStat.AppendLine("                         AND aw2.REPORTMONTH = @CLOSINGMONTH ")
            sqlStat.AppendLine("                         AND aw2.CONTRACTORFIX = GBT0015_AC_WORK.CONTRACTORFIX ) ")
            sqlStat.AppendLine("      GROUP BY CONTRACTORFIX, COSTTYPE ")
            sqlStat.AppendLine("    ) ")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("   aw.CONTRACTORFIX as COUNTRYCODE,")
            sqlStat.AppendLine("   '1' as ISREFCUSTOMERMST,")
            sqlStat.AppendLine("   cm.BOTHCLASS,")
            sqlStat.AppendLine("   cm.ACCCURRENCYSEGMENT,")
            sqlStat.AppendLine("   cm.BOTHCLASS,")
            sqlStat.AppendLine("   cm.INCTORICODE,")
            sqlStat.AppendLine("   cm.EXPTORICODE,")
            sqlStat.AppendLine("   cm.DEPOSITDAY,")
            sqlStat.AppendLine("   cm.DEPOSITADDMM,")
            sqlStat.AppendLine("   cm.OVERDRAWDAY,")
            sqlStat.AppendLine("   cm.OVERDRAWADDMM,")
            sqlStat.AppendLine("   cm.HOLIDAYFLG")
            sqlStat.AppendLine("FROM WITH_BASE aw")
            'sqlStat.AppendLine("FROM GBT0015_AC_WORK aw")
            sqlStat.AppendLine("inner join GBM0004_CUSTOMER cm")
            sqlStat.AppendLine("  on  cm.CUSTOMERCODE = aw.CONTRACTORFIX ")
            sqlStat.AppendLine("  and cm.STYMD  <= @ENTYMD ")
            sqlStat.AppendLine("  and cm.ENDYMD >= @ENTYMD ")
            sqlStat.AppendLine("  and cm.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("WHERE aw.COSTTYPE = '1' ")
            'sqlStat.AppendLine("  AND aw.DELFLG <> @DELFLG ")
            'sqlStat.AppendLine("  AND aw.CLOSINGMONTH = @CLOSINGMONTH ")
            'sqlStat.AppendLine("  AND aw.CLOSINGGROUP = @CLOSINGGROUP ")
            'sqlStat.AppendLine("  AND aw.REPORTMONTH <> @CLOSINGMONTH ")
            sqlStat.AppendLine("UNION ")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("   aw.CONTRACTORFIX as COUNTRYCODE,")
            sqlStat.AppendLine("   '0' as ISREFCUSTOMERMST,")
            sqlStat.AppendLine("   tm.BOTHCLASS,")
            sqlStat.AppendLine("   tm.ACCCURRENCYSEGMENT,")
            sqlStat.AppendLine("   tm.BOTHCLASS,")
            sqlStat.AppendLine("   tm.INCTORICODE,")
            sqlStat.AppendLine("   tm.EXPTORICODE,")
            sqlStat.AppendLine("   tm.DEPOSITDAY,")
            sqlStat.AppendLine("   tm.DEPOSITADDMM,")
            sqlStat.AppendLine("   tm.OVERDRAWDAY,")
            sqlStat.AppendLine("   tm.OVERDRAWADDMM,")
            sqlStat.AppendLine("   tm.HOLIDAYFLG")
            sqlStat.AppendLine("FROM WITH_BASE aw")
            'sqlStat.AppendLine("FROM GBT0015_AC_WORK aw")
            sqlStat.AppendLine("inner join GBM0005_TRADER tm")
            sqlStat.AppendLine("  on  tm.CARRIERCODE = aw.CONTRACTORFIX ")
            sqlStat.AppendLine("  and tm.STYMD  <= @ENTYMD ")
            sqlStat.AppendLine("  and tm.ENDYMD >= @ENTYMD ")
            sqlStat.AppendLine("  and tm.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("WHERE aw.COSTTYPE = '2' ")
            'sqlStat.AppendLine("  AND aw.DELFLG <> @DELFLG ")
            'sqlStat.AppendLine("  AND aw.CLOSINGMONTH = @CLOSINGMONTH ")
            'sqlStat.AppendLine("  AND aw.CLOSINGGROUP = @CLOSINGGROUP ")
            'sqlStat.AppendLine("  AND aw.REPORTMONTH <> @CLOSINGMONTH ")
            sqlStat.AppendLine("UNION ")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("   aw.CONTRACTORFIX as COUNTRYCODE,")
            sqlStat.AppendLine("   '0' as ISREFCUSTOMERMST,")
            sqlStat.AppendLine("   dm.BOTHCLASS,")
            sqlStat.AppendLine("   dm.ACCCURRENCYSEGMENT,")
            sqlStat.AppendLine("   dm.BOTHCLASS,")
            sqlStat.AppendLine("   dm.INCTORICODE,")
            sqlStat.AppendLine("   dm.EXPTORICODE,")
            sqlStat.AppendLine("   dm.DEPOSITDAY,")
            sqlStat.AppendLine("   dm.DEPOSITADDMM,")
            sqlStat.AppendLine("   dm.OVERDRAWDAY,")
            sqlStat.AppendLine("   dm.OVERDRAWADDMM,")
            sqlStat.AppendLine("   dm.HOLIDAYFLG")
            sqlStat.AppendLine("FROM WITH_BASE aw")
            'sqlStat.AppendLine("FROM GBT0015_AC_WORK aw")
            sqlStat.AppendLine("inner join GBM0003_DEPOT dm")
            sqlStat.AppendLine("  on  dm.DEPOTCODE = aw.CONTRACTORFIX ")
            sqlStat.AppendLine("  and dm.STYMD  <= @ENTYMD ")
            sqlStat.AppendLine("  and dm.ENDYMD >= @ENTYMD ")
            sqlStat.AppendLine("  and dm.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("WHERE aw.COSTTYPE = '2' ")
            'sqlStat.AppendLine("  AND aw.DELFLG <> @DELFLG ")
            'sqlStat.AppendLine("  AND aw.CLOSINGMONTH = @CLOSINGMONTH ")
            'sqlStat.AppendLine("  AND aw.CLOSINGGROUP = @CLOSINGGROUP ")
            'sqlStat.AppendLine("  AND aw.REPORTMONTH <> @CLOSINGMONTH ")

            Dim dtDbResult As New DataTable
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
                sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン
                'SQLパラメータ設定
                With sqlCmd.Parameters

                    .Add("@ENTYMD", SqlDbType.DateTime).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = ReportMonth
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = CONST_CURRENTXML_JOT
                End With
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(retDt)

                End Using
            End Using
            Return retDt

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
            Return Nothing
        End Try

        Return retDt
    End Function

    Function SwapRow(rowItem As DataRow, startColumnIdx As Integer, count As Integer) As DataRow
        If CDbl(rowItem("DEBAMOUNT")) >= 0 Then
            Return rowItem
        End If
        Dim retRow = rowItem.Table.NewRow
        retRow.ItemArray = rowItem.ItemArray '一旦全フィールド値コピー
        Dim swapTargetStartIdx = startColumnIdx + count
        For colIdx = 0 To count - 1
            retRow(startColumnIdx + colIdx) = rowItem(swapTargetStartIdx + colIdx)
            retRow(swapTargetStartIdx + colIdx) = rowItem(startColumnIdx + colIdx)
        Next
        For Each fieldName In {"DEBAMOUNT", "DEBCONSTAXAMOUNT", "DEBFORCURAMOUNT",
                               "CREAMOUNT", "CRECONSTAXAMOUNT", "CREFORCURAMOUNT"}
            retRow(fieldName) = CDbl(retRow(fieldName)) * -1
        Next

        Return retRow
    End Function
    ''' <summary>
    ''' 帳票詳細データ取得関数
    ''' </summary>
    ''' <param name="group">〆グループ(国or'JOT')</param>
    ''' <param name="month">〆月</param>
    ''' <returns></returns>
    ''' <remarks>SQL、引数などは三宅仮作成なので適宜変更を</remarks>
    Private Function GetReportDetailData(group As String, month As String) As DataTable
        Dim retDt As New DataTable
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ")
        '**GBT0015_AC_WORKフィールド
        sqlStat.AppendLine("        CONVERT(varchar(36),aw.DATAID) AS DATAID")
        sqlStat.AppendLine("       ,aw.CLOSINGMONTH")
        sqlStat.AppendLine("       ,aw.CLOSINGGROUP")
        sqlStat.AppendLine("       ,aw.INVOICEDBY")
        sqlStat.AppendLine("       ,aw.CONTRACTORFIX")
        'sqlStat.AppendLine("       ,isnull(tr.NAMES,'') as 'TORI'")
        sqlStat.AppendLine("       ,isnull(tr.NAMES1,'') as 'TORI'")
        sqlStat.AppendLine("       ,aw.ORDERNO")
        sqlStat.AppendLine("       ,aw.TANKNO")
        sqlStat.AppendLine("       ,aw.COSTCODE")
        sqlStat.AppendLine("       ,aw.COSTTYPE")
        sqlStat.AppendLine("       ,aw.CRACCOUNT")
        sqlStat.AppendLine("       ,aw.DBACCOUNT")
        sqlStat.AppendLine("       ,aw.CRACCOUNTFORIGN")
        sqlStat.AppendLine("       ,aw.DBACCOUNTFORIGN")
        sqlStat.AppendLine("       ,aw.OFFCRACCOUNT")
        sqlStat.AppendLine("       ,aw.OFFDBACCOUNT")
        sqlStat.AppendLine("       ,aw.OFFCRACCOUNTFORIGN")
        sqlStat.AppendLine("       ,aw.OFFDBACCOUNTFORIGN")
        sqlStat.AppendLine("       ,aw.CRSEGMENT1")
        sqlStat.AppendLine("       ,aw.DBSEGMENT1")
        sqlStat.AppendLine("       ,aw.CRGENERALPURPOSE")
        sqlStat.AppendLine("       ,aw.CREGENPURPOSE")
        sqlStat.AppendLine("       ,aw.DBGENERALPURPOSE")
        sqlStat.AppendLine("       ,aw.DEBGENPURPOSE")
        sqlStat.AppendLine("       ,aw.COUNTRYCODE")
        sqlStat.AppendLine("       ,aw.CURRENCYCODE")
        sqlStat.AppendLine("       ,aw.TAXATION")
        sqlStat.AppendLine("       ,aw.AMOUNTFIX")
        sqlStat.AppendLine("       ,aw.LOCALBR")
        sqlStat.AppendLine("       ,aw.LOCALRATE")
        sqlStat.AppendLine("       ,aw.AMOUNTPAYODR")
        sqlStat.AppendLine("       ,aw.LOCALPAYODR")
        sqlStat.AppendLine("       ,aw.TAXBR")
        sqlStat.AppendLine("       ,aw.LOCALRATESOA")
        sqlStat.AppendLine("       ,aw.AMOUNTPAY")
        sqlStat.AppendLine("       ,aw.LOCALPAY")
        sqlStat.AppendLine("       ,aw.TAXPAY")
        sqlStat.AppendLine("       ,aw.UAG_USD")
        sqlStat.AppendLine("       ,aw.UAG_LOCAL")
        sqlStat.AppendLine("       ,aw.UAG_JPY")
        sqlStat.AppendLine("       ,aw.UAG_USD_SHIP")
        sqlStat.AppendLine("       ,aw.UAG_JPY_SHIP")
        sqlStat.AppendLine("       ,aw.USD_USD")
        sqlStat.AppendLine("       ,aw.USD_LOCAL")
        sqlStat.AppendLine("       ,aw.LOCAL_USD")
        sqlStat.AppendLine("       ,aw.LOCAL_LOCAL")
        sqlStat.AppendLine("       ,aw.ACTUALDATE")
        sqlStat.AppendLine("       ,aw.SOAAPPDATE")
        sqlStat.AppendLine("       ,aw.REMARK")
        sqlStat.AppendLine("       ,aw.BRID")
        sqlStat.AppendLine("       ,aw.APPLYID")
        sqlStat.AppendLine("       ,aw.SOACODE")
        sqlStat.AppendLine("       ,aw.SOASHORTCODE")
        sqlStat.AppendLine("       ,aw.REPORTMONTH")
        sqlStat.AppendLine("       ,aw.REPORTMONTHORG")
        sqlStat.AppendLine("       ,aw.REPORTRATEJPY")
        sqlStat.AppendLine("       ,aw.SHIPDATE")
        sqlStat.AppendLine("       ,aw.DOUTDATE")
        sqlStat.AppendLine("       ,aw.LOADING")
        sqlStat.AppendLine("       ,aw.STEAMING")
        sqlStat.AppendLine("       ,aw.TIP")
        sqlStat.AppendLine("       ,aw.EXTRA")
        sqlStat.AppendLine("       ,aw.ROUTEDAYS")
        sqlStat.AppendLine("       ,CONVERT(varchar(36),aw.DATAIDODR)  AS DATAIDODR")
        sqlStat.AppendLine("       ,aw.ACCRECRATE")
        sqlStat.AppendLine("       ,aw.ACCRECYEN")
        sqlStat.AppendLine("       ,aw.ACCRECFOREIGN")
        sqlStat.AppendLine("       ,aw.EXSHIPRATE1")
        sqlStat.AppendLine("       ,aw.INSHIPRATE1")
        sqlStat.AppendLine("       ,aw.EXSHIPRATE2")
        sqlStat.AppendLine("       ,aw.INSHIPRATE2")
        sqlStat.AppendLine("       ,aw.TANKSEQ")
        sqlStat.AppendLine("       ,aw.DTLPOLPOD")
        sqlStat.AppendLine("       ,aw.DTLOFFICE")
        '**GBT0006_CLOSINGDAY関連
        'sqlStat.AppendLine("       ,cd.COUNTRYCODE") 'GBT0015_AC_WORK かぶるので割愛
        'sqlStat.AppendLine("       ,cd.STYMD")
        'sqlStat.AppendLine("       ,cd.ENDYMD")
        sqlStat.AppendLine("       ,cd.BILLINGYMD")
        'sqlStat.AppendLine("       ,cd.REPORTMONTH") 'GBT0015_AC_WORK かぶるので割愛
        'sqlStat.AppendLine("       ,cd.APPLYID")     '申請関連は不要と想定割愛
        'sqlStat.AppendLine("       ,cd.APPLYTEXT")   '申請関連は不要と想定割愛
        'sqlStat.AppendLine("       ,cd.LASTSTEP")    '申請関連は不要と想定割愛
        'sqlStat.AppendLine("       ,cd.APPLYUSER")   '申請関連は不要と想定割愛
        'sqlStat.AppendLine("       ,cd.APPLYOFFICE") '申請関連は不要と想定割愛
        sqlStat.AppendLine("       ,cd.PAYABLE")
        sqlStat.AppendLine("       ,cd.RECEIVABLE")
        sqlStat.AppendLine("       ,cd.NETSETTLEMENTDUE")

        sqlStat.AppendLine("  FROM  GBT0015_AC_WORK aw")
        sqlStat.AppendLine("  INNER JOIN GBT0006_CLOSINGDAY cd")
        sqlStat.AppendLine("     ON cd.COUNTRYCODE  = aw.CLOSINGGROUP")
        sqlStat.AppendLine("    AND cd.REPORTMONTH  = aw.CLOSINGMONTH")
        sqlStat.AppendLine("    AND cd.DELFLG      <> @DELFLG")
        If group = "JOT" Then

            '--- JOT取引先
            sqlStat.AppendLine("  LEFT OUTER JOIN GBM0004_CUSTOMER cm")
            sqlStat.AppendLine("     ON cm.CUSTOMERCODE  = aw.CONTRACTORFIX")
            sqlStat.AppendLine("    AND cm.STYMD       <= @ENTYMD")
            sqlStat.AppendLine("    AND cm.ENDYMD      >= @ENTYMD")
            sqlStat.AppendLine("    AND cm.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT OUTER JOIN GBM0005_TRADER tm")
            sqlStat.AppendLine("     ON tm.CARRIERCODE  = aw.CONTRACTORFIX")
            sqlStat.AppendLine("    AND tm.STYMD       <= @ENTYMD")
            sqlStat.AppendLine("    AND tm.ENDYMD      >= @ENTYMD")
            sqlStat.AppendLine("    AND tm.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT OUTER JOIN GBM0003_DEPOT dm")
            sqlStat.AppendLine("     ON dm.DEPOTCODE  = aw.CONTRACTORFIX")
            sqlStat.AppendLine("    AND dm.STYMD       <= @ENTYMD")
            sqlStat.AppendLine("    AND dm.ENDYMD      >= @ENTYMD")
            sqlStat.AppendLine("    AND dm.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT OUTER JOIN GBM0001_COUNTRY ct")
            sqlStat.AppendLine("     ON ct.COUNTRYCODE  = tm.COUNTRYCODE")
            sqlStat.AppendLine("    AND ct.STYMD       <= @ENTYMD")
            sqlStat.AppendLine("    AND ct.ENDYMD      >= @ENTYMD")
            sqlStat.AppendLine("    AND tm.CLASS = 'AGENT'")
            sqlStat.AppendLine("    AND ct.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("  LEFT OUTER JOIN GBM0025_TORI tr")
            sqlStat.AppendLine("     ON ")
            sqlStat.AppendLine("      ( (tr.TORICODE = cm.INCTORICODE AND tr.TORIKBN = 'I' AND aw.COSTTYPE = '1' )")
            sqlStat.AppendLine("        OR ")
            sqlStat.AppendLine("       (tr.TORICODE = isnull(tm.EXPTORICODE,isnull(dm.EXPTORICODE,isnull(ct.EXPTORICODE,''))) AND tr.TORIKBN = 'E' AND aw.COSTTYPE = '2' )")
            sqlStat.AppendLine("      )")
            sqlStat.AppendLine("    AND tr.STYMD      <= @ENTYMD")
            sqlStat.AppendLine("    AND tr.ENDYMD     >= @ENTYMD")
            sqlStat.AppendLine("    AND tr.DELFLG     <> @DELFLG")

        Else
            '--- 代理店取引先
            sqlStat.AppendLine("  LEFT OUTER JOIN GBM0001_COUNTRY ct")
            sqlStat.AppendLine("     ON ct.COUNTRYCODE  = aw.CLOSINGGROUP")
            sqlStat.AppendLine("    AND ct.STYMD       <= @ENTYMD")
            sqlStat.AppendLine("    AND ct.ENDYMD      >= @ENTYMD")
            sqlStat.AppendLine("    AND ct.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT OUTER JOIN GBM0025_TORI tr")
            sqlStat.AppendLine("     ON tr.TORICODE   = ct.EXPTORICODE")
            sqlStat.AppendLine("    AND tr.TORIKBN    = 'E'")
            sqlStat.AppendLine("    AND tr.STYMD     <= @ENTYMD")
            sqlStat.AppendLine("    AND tr.ENDYMD    >= @ENTYMD")
            sqlStat.AppendLine("    AND tr.DELFLG    <> @DELFLG")
        End If

        sqlStat.AppendLine(" WHERE aw.CLOSINGMONTH  = @CLOSINGMONTH")
        sqlStat.AppendLine("   AND aw.CLOSINGGROUP  = @CLOSINGGROUP")
        'sqlStat.AppendLine("   AND aw.REPORTMONTH   = @CLOSINGMONTH")
        sqlStat.AppendLine("   AND aw.DELFLG       <> @DELFLG")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
            sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = group
                .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = month
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = Date.Now
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
End Class

