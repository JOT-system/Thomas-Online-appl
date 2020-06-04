Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL

''' <summary>
''' タンクステータス一覧画面クラス
''' </summary>
Public Class GBT00006RESULTTST
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00006R" '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 34                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 当リストデータ保存用
    ''' </summary>
    Private SavedDt As DataTable = Nothing
    ''' <summary>
    ''' 画面退避用アイテム
    ''' </summary>
    ''' <returns></returns>
    Public Property DisplayItems As GBT00006RESULT.GBT00006RITEMS
    ''' <summary>
    ''' 引当情報
    ''' </summary>
    ''' <returns></returns>
    Public Property OrderInfo As GBT00006RESULT.GBT00006ROrderInfo
    ''' <summary>
    ''' タンク利用申請可否
    ''' </summary>
    ''' <returns>True:申請必要,False:申請不要(デフォルト)</returns>
    ''' <remarks>点検時期近く及び修理中のタンクに付き申請を必須とする</remarks>
    Public Property NeedsTankUseApply As Boolean = False
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
                Me.hdnListEvent.Value = ""
                Me.hdnListFunc.Value = ""
                Me.btnAllocate.Visible = False
                Me.spnAlocTankInfo.Visible = False
                If Me.hdnThisMapVariant.Value <> "GB_TankStatusList" Then
                    Me.btnAllocate.Visible = True
                    'Me.hdnListEvent.Value = "ondblclick" '複数引き当ての為廃止
                    'Me.hdnListFunc.Value = "ListDbClick" '同上
                    Me.spnAlocTankInfo.Visible = True
                End If
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = "Order"
                COA0031ProfMap.COA0031GetDisplayTitle()

                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0031ProfMap.ERR)})
                    Return
                End If
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                '前画面情報の引継ぎ
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                'タンク状態データ取得
                '****************************************
                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, Me.hdnThisMapVariant.Value)
                '一覧表データ取得
                Using dt As DataTable = Me.GetListData()
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
                        Me.SavedDt = dt
                    End With

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = If(Me.hdnThisMapVariant.Value <> "GB_TankStatusList", "GB_Allocate", "Default")
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "2"
                        .LEVENT = If(Me.hdnListEvent.Value = "", Nothing, Me.hdnListEvent.Value)
                        .LFUNC = If(Me.hdnListFunc.Value = "", Nothing, Me.hdnListFunc.Value)
                        .TITLEOPT = True
                        .NOCOLUMNWIDTHOPT = 60
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .USERSORTOPT = 1
                    End With
                    COA0013TableObject.COA0013SetTableObject()
                    '現在の表示LINECNTを保持
                    If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
                        Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                                  Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
                        ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
                        For Each targetCheckBoxId As String In {"ALLOCATECHK"}

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
                    Else
                        ViewState("DISPLAY_LINECNT_LIST") = Nothing
                    End If

                End Using 'DataTable
                Dim actdt As DataTable = GetActy()
                With Me.cblActy2
                    .DataSource = actdt
                    .DataTextField = "LISTBOXNAME"
                    .DataValueField = "CODE"
                    .DataBind()
                    ''一応現在入力しているテキストと一致するものを選択状態
                    'If .Items.Count > 0 Then
                    '    Dim findListItem = .Items.FindByValue(Me.txtActy.Text)
                    '    If findListItem IsNot Nothing Then
                    '        findListItem.Selected = True
                    '    End If
                    'End If
                End With

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
            DisplayListObjEdit() 'リストオブジェクトの編集
            Me.lblAllocateTankSelectedCount.Text = Me.hdnSelectedTankCount.Value
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        Catch ex As Threading.ThreadAbortException
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
        'ビューの存在チェック
        Dim changeViewObj As View = DirectCast(Me.mvLeft.FindControl(Me.hdnLeftboxActiveViewId.Value), View)
        If changeViewObj IsNot Nothing Then
            Me.mvLeft.SetActiveView(changeViewObj)
            Select Case changeViewObj.ID
                '他のビューが存在する場合はViewIdでCaseを追加
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
                            Dim findListItem = .Items.FindByValue(Me.txtActy.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With
                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Me.hdnCalendarValue.Value = txtobj.Text

                        Me.mvLeft.Focus()
                    End If
            End Select
        End If

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '自画面MAPIDより親MAP・URLを取得
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = "GB_TankStatusList" 'Me.hdnThisMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '次画面の変数セット
        HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
        HttpContext.Current.Session("MAPurl") = COA0011ReturnUrl.URL
        '画面遷移実行
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
                Case vLeftActy.ID
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
                        'Dim actyCode As String = Me.lbActy.SelectedItem.Value
                        'Dim dt As DataTable = GetActy(actyCode)
                        'If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                        '    Dim dr As DataRow = dt.Rows(0)
                        '    If targetObject IsNot Nothing Then
                        '        txtObject.Text = Convert.ToString(dr.Item("CODE"))
                        '    End If
                        '    'Me.lblActyText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
                        'End If

                    End If

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
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    Public Sub btnExtract_Click()
        Me.lblFooterMessage.Text = ""
        Dim dt As DataTable = CreateListDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        '一覧表示データ復元 
        If Me.SavedDt Is Nothing Then
            dt = CreateListDataTable()
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

        'フィルタでの絞り込みを利用するか確認
        Dim isFillterOff As Boolean = True
        'If Me.txtTankNo.Text <> "" OrElse Me.txtPort.Text <> "" Then
        If Me.txtTankNo.Text <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtTankNo.Text = "" OrElse Convert.ToString(dr("TANKNO")).StartsWith(Me.txtTankNo.Text))) Then
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
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            Me.SavedDt = dt
        End If
        'カーソル設定
        Me.txtTankNo.Focus()
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
        Dim dt As DataTable = CreateListDataTable()
        If Me.SavedDt Is Nothing Then
            dt = CreateListDataTable()
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

        'ソート
        Dim dvTBLview As DataView
        dvTBLview = New DataView(dt)
        dvTBLview.RowFilter = "HIDDEN= '0'"

        'ポジションを設定するのみ
        If dvTBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            hdnListPosition.Value = (dvTBLview.Count - (dvTBLview.Count Mod CONST_SCROLLROWCOUNT)).ToString
        Else
            hdnListPosition.Value = (dvTBLview.Count - (dvTBLview.Count Mod CONST_SCROLLROWCOUNT) + 1).ToString
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
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excel出力", "Excel Download")


        AddLangSetting(dicDisplayText, Me.btnBack, "戻る", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        AddLangSetting(dicDisplayText, Me.lblTankNoLabel, "TANKNo.", "TANKNo.")
        AddLangSetting(dicDisplayText, Me.lblPort, "港", "PORT")
        AddLangSetting(dicDisplayText, Me.btnAllocate, "引当", "Allocate")
        AddLangSetting(dicDisplayText, Me.hdnConfirmTitle, "申請が必要です。申請しますか？", "Needs apply.Are you sure you want to apply for?")
        AddLangSetting(dicDisplayText, Me.hdnApplyMessage, "以下のタンクは利用申請が必要です。申請しますか？<br />{0}", "The following tank is required use application.Are you sure you want to apply for?<br />{0}")

        AddLangSetting(dicDisplayText, Me.lblAllocateTankCount, "引当数", "Allocate Count")
        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub
    ''' <summary>
    ''' タンク動静関連の各種テーブルより情報を取得
    ''' TODOデータベースより取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>本当にランダムダミーデータです日付の整合も無視</remarks>
    Private Function GetListData() As DataTable
        Dim dt As DataTable = CreateListDataTable()
        Dim dtDbResult As DataTable = Nothing
        'タンク引当の場合はETYD,ETYCのみ取得するようパラメータを設定
        Dim isAllocateOnly As Integer = 0
        Dim allocateOrderNo As String = ""
        If Me.hdnThisMapVariant.Value = "GB_TankSelect" Then
            isAllocateOnly = 1
            Dim orderInfo As GBT00006ROrderInfo = DirectCast(ViewState("ORDERINFO"), GBT00006ROrderInfo)
            allocateOrderNo = orderInfo.OrderNo
        End If
        Dim dicTankInfo As GBT00006ROrderInfo = Nothing
        Dim tankNoList As List(Of String) = Nothing
        If ViewState("ORDERINFO") IsNot Nothing Then
            dicTankInfo = DirectCast(ViewState("ORDERINFO"), GBT00006ROrderInfo)
            '引当済のタンク番号一覧を取得
            Dim q = (From item In dicTankInfo.TankInfoList Where item.Value.TankNo <> "" Select item.Value.TankNo)
            If q.Any Then
                tankNoList = q.ToList
            End If
        End If
        '一旦国コードのパラメータは付与していない
        Dim GBA00012TankInfo As New GBA00012TankInfo With {.ISALLOCATEONLY = isAllocateOnly, .ALLOCATEORDERNO = allocateOrderNo, .TANKNOLIST = tankNoList}
        GBA00012TankInfo.GBA00012getTankStatusTable()
        If Not {C_MESSAGENO.NORMAL, C_MESSAGENO.NOENTRYDATA}.Contains(GBA00012TankInfo.ERR) Then
            CommonFunctions.ShowMessage(GBA00012TankInfo.ERR, Me.lblFooterMessage)
            Return dt
        End If
        dtDbResult = GBA00012TankInfo.TANKSTATUS_TABLE
        'テーブル抽出結果に含まれるテーブル名一覧
        Dim sqlFieldList As List(Of String) = (From col In dtDbResult.Columns.Cast(Of DataColumn) Select col.ColumnName).ToList
        '表示対象データテーブルとテーブル抽出結果に含まれるフィールド名が一致する一覧
        Dim copyFieldList As List(Of String) = (From col In dt.Columns.Cast(Of DataColumn) Where sqlFieldList.Contains(col.ColumnName) Select col.ColumnName).ToList

        Dim lineCnt As Integer = 0
        '点検日付格納
        Dim targetDate As Date = Now
        If Date.TryParse(Me.hdnOrderMaxEtd.Value, targetDate) = True Then
            targetDate = Date.Parse(Me.hdnOrderMaxEtd.Value)
        Else
            targetDate = Date.Now
        End If
        Dim dicTestTypeMinusMonth As New Dictionary(Of String, Integer) From {{"2.5", -1}, {"5", -3}}

        For Each drTankNo As DataRow In dtDbResult.Rows

            Dim dr As DataRow = dt.NewRow
            'Dim acty As String = actyList.OrderBy(Function(item) Guid.NewGuid()).FirstOrDefault
            Dim tankNo As String = Convert.ToString(drTankNo.Item("TANKNO"))
            lineCnt = lineCnt + 1
            dr.Item("LINECNT") = lineCnt
            dr.Item("SELECT") = "1"
            dr.Item("HIDDEN") = "0"

            For Each copyField In copyFieldList
                dr.Item(copyField) = Convert.ToString(drTankNo.Item(copyField))
            Next
            '引当済、またはマスターより戻って来た際のチェック済のチェックボックスを保持

            If (Me.DisplayItems Is Nothing OrElse Me.DisplayItems.Gbt00006RCheckedTankNo Is Nothing) _
                AndAlso tankNoList IsNot Nothing AndAlso tankNoList.Contains(tankNo) Then
                dr.Item("ALLOCATECHK") = "on"

            ElseIf Me.DisplayItems IsNot Nothing AndAlso
                   Me.DisplayItems.Gbt00006RCheckedTankNo IsNot Nothing AndAlso
                   Me.DisplayItems.Gbt00006RCheckedTankNo.Count > 0 AndAlso
                   Me.DisplayItems.Gbt00006RCheckedTankNo.Contains(tankNo) Then
                dr.Item("ALLOCATECHK") = "on"

            End If

            'リペアステータス判定
            If Convert.ToString(dr("CANPROVISION")).Trim <> CONST_FLAG_NO Then
                dr("NEEDSAPPLY") = "1"
            End If

            '定期点検日付チェック
            Dim nextDate As Date

            If Date.TryParse(Convert.ToString(dr("T_NEXTDATE")), nextDate) = True AndAlso
               dicTestTypeMinusMonth.ContainsKey(Convert.ToString(dr("T_NEXTTYPE")).Trim) Then

                nextDate = Date.Parse(Convert.ToString(dr("T_NEXTDATE")))
                Dim appendMonth As Integer = dicTestTypeMinusMonth(Convert.ToString(dr("T_NEXTTYPE")).Trim)
                Dim checkDate As Date = nextDate.AddMonths(appendMonth)

                If checkDate <= targetDate Then
                    dr("NEEDSAPPLY") = "1"
                End If

            End If
            'EMPTY OR FULL
            If Convert.ToString(dr.Item("DISCHDATE")) < Convert.ToString(dr.Item("LADENDATE")) Then
                dr.Item("EF") = "F"
            Else
                dr.Item("EF") = "E"
            End If
            'チェック可否フラグ(CANALLOCATE) '1'チェック可能、それ以外チェック不可
            Dim acty As String = Convert.ToString(dr.Item("ACTY"))
            Dim tkalStat As String = Convert.ToString(dr.Item("TKAL_STATUS"))
            Dim orderNo As String = Convert.ToString(dr.Item("ORDERNOOUT"))
            If {"ETYD", "ETYC", ""}.Contains(acty) _
              OrElse ({"TKAL", "TAEC", "TAED"}.Contains(acty) AndAlso tkalStat.Trim <> C_APP_STATUS.APPLYING AndAlso
                      orderNo = allocateOrderNo) Then
                dr.Item("CANALLOCATE") = "1"
            End If

            dr.Item("TIMSTP") = ""
            dr.Item("OPERATION") = ""
            'dr.Item("CANPROVISION") = "1" '引き当て可能かDOTOこれはSQL無いしロジックで情報をかならず付与

            dt.Rows.Add(dr)
        Next

        Return dt
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
    ''' 一覧表用のデータテーブルを作成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateListDataTable() As DataTable
        Dim retDt As New DataTable
        '固定部分は追加しておく
        retDt.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        retDt.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        retDt.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        retDt.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        retDt.Columns.Add("HIDDEN", GetType(Integer))
        'Dim colList As New List(Of String) From {"TANKNO", "ACTY", "ORDERSCHEDULE", "ORDERFINISH", "DEPOOUTSCHEDULE", "DEPOOUTFINISH",
        '                                         "FILLINGSCHEDULE", "FILLINGFINISH", "CYCUTSCHEDULE", "CYCUTFINISH",
        '                                         "ETDSCHEDULE", "ETDFINISH", "ETASCHEDULE", "ETAFINISH",
        '                                         "DISCHARGESCHEDULE", "DISCHARGEFINISH", "DEPOINSCHEDULE", "DEPOINFINISH",
        '                                         "CLEANSCHEDULE", "CLEANFINISH", "ORDERINFO", "REPAIRSTATUS",
        '                                         "COMMENT", "CANPROVISION"}
        Dim colList As New List(Of String) From {"TANKNO", "ACTY", "TYPE", "FDA", "FROMAREA", "TOAREA",
                                                 "ETAARR", "DISCHDATE", "DEMMYN", "DEMMSTART",
                                                 "DEPOTINDATE", "LOCATION", "CLEANDATE", "JOBNO",
                                                 "ALLOCATIONDATE", "DEPOTOUT", "LADENDATE", "DEPOINFINISH",
                                                 "EF", "ETDDATE", "ETADATE", "DESTINATION",
                                                 "CANPROVISION", "REPAIRDATE",
                                                 "A2_5YTEST", "A5YTEST",
                                                 "ORDERNOIN", "ORDERNOOUT",
                                                 "T_NEXTTYPE", "T_NEXTDATE", '次回検査種別,次回検査実施日
                                                 "PD_HIST1", "PD_HIST2", "PD_HIST3", '前回・前々回・３つ前積み荷
                                                 "TAREWEIGHT", "CAPACITY", 'キャパシティ,自重
                                                 "EDIT", '編集ボタン用
                                                 "ALLOCATECHK",  '引当チェックボックス用
                                                 "NEEDSAPPLY", '申請必須(1:必須、それ以外:不要)
                                                 "TKAL_APPLYID", "TKAL_STATUS", "TKAL_APPLYTEXT", "TKAL_APPROVEDTEXT", '申請関連情報
                                                 "CANALLOCATE", '引当チェック可否('1':チェック可,それ以外:チェック不可)
                                                 "POL_POLCOUNTRY", "POL_POLPORT", '帳票用
                                                 "POL_PODCOUNTRY", "POL_PODPORT", '帳票用
                                                 "NEXTA2_5YTEST", "NEXTA5YTEST"}

        For Each colName As String In colList
            retDt.Columns.Add(colName, GetType(String))
            retDt.Columns(colName).DefaultValue = ""
        Next
        Return retDt
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
            dt = CreateListDataTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
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

        'この段階でありえないがデータテーブルがない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If
        '引当ではない場合は保持する入力情報が無いためそのまま終了
        If Me.hdnThisMapVariant.Value = "GB_TankStatusList" Then
            Me.SavedDt = dt
            Return C_MESSAGENO.NORMAL
        End If

        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objTxtPrifix As String = "txt" & Me.WF_LISTAREA.ID
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String)
        '引当チェックボックス
        fieldIdList.Add("ALLOCATECHK", objChkPrifix)

        'Dim formToPost = New NameValueCollection(Request.Form)
        For Each i In displayLineCnt
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    '                    formToPost.Remove(dispObjId)
                End If
                Dim dr As DataRow = dt.Rows(i - 1)
                If Convert.ToString(dr.Item("CANALLOCATE")) <> "1" AndAlso fieldId.Key = "ALLOCATECHK" Then
                    Continue For
                Else
                    dr.Item(fieldId.Key) = displayValue
                End If
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
    ''' 前画面より各種情報を引き継ぎ
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00004ORDER Then
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
                                                                        {"hdnBrId", Me.hdnBrId},   'ここは本来不要
                                                                        {"hdnFillingDate", Me.hdnFillingDate},
                                                                        {"hdnEtd1", Me.hdnEtd1},
                                                                        {"hdnEta1", Me.hdnEta1},
                                                                        {"hdnEtd2", Me.hdnEtd2},
                                                                        {"hdnEta2", Me.hdnEta2},
                                                                        {"hdnOrgXMLsaveFile", Me.hdnOrderOrgXMLsaveFile},
                                                                        {"hdnXMLsaveFile", Me.hdnOrderXMLsaveFile},
                                                                        {"hdnIsNewData", Me.hdnIsNewData},
                                                                        {"hdnCopy", Me.hdnCopy},
                                                                        {"hdnSelectedOrderId", Me.hdnSelectedOrderId},
                                                                        {"hdnSelectedTankSeq", Me.hdnSelectedTankSeq},
                                                                        {"hdnSelectedDataId", Me.hdnSelectedDataId},
                                                                        {"hdnListPosition", Me.hdnOrderDispListPosition},
                                                                        {"hdnListMapVariant", Me.hdnListMapVariant},
                                                                        {"hdnDateTermStYMD", Me.hdnDateTermStYMD},
                                                                        {"hdnDateTermEndYMD", Me.hdnDateTermEndYMD},
                                                                        {"hdnApproval", Me.hdnApproval},
                                                                        {"hdnReportVariant", Me.hdnReportVariant},
                                                                        {"hdnListId", Me.hdnListId},
                                                                        {"txtActy", Me.hdnActy}}


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
            If prevObj.Request.Form("hdnListSortValueGBT00004WF_LISTAREA") IsNot Nothing Then
                Me.hdnListSortValueGBT00004WF_LISTAREA.Value = prevObj.Request.Form("hdnListSortValueGBT00004WF_LISTAREA")
            End If
            Me.hdnOrderMaxEtd.Value = prevObj.Gbt00006ROrderInfo.ETD
            Me.lblAllocateTankMaxCount.Text = Convert.ToString(prevObj.Gbt00006ROrderInfo.TankInfoList.Count)
            Dim selectedCount As Integer = (From item In prevObj.Gbt00006ROrderInfo.TankInfoList Where item.Value.TankNo <> "").Count
            Me.hdnSelectedTankCount.Value = Convert.ToString(selectedCount)
            ViewState("ORDERINFO") = prevObj.Gbt00006ROrderInfo
        ElseIf TypeOf Page.PreviousPage Is GBM00006TANK Then
            Dim prevObj As GBM00006TANK = DirectCast(Page.PreviousPage, GBM00006TANK)
            SetGbt00006items(prevObj.Gbt00006items)
        Else
            Me.hdnOrderMaxEtd.Value = Now.ToString("yyyy/MM/dd")
        End If
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
        Dim dt As DataTable = CreateListDataTable()

        If Me.SavedDt Is Nothing Then
            dt = CreateListDataTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                Return
            End If
        Else
            dt = Me.SavedDt
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
        COA0013TableObject.VARI = If(Me.hdnThisMapVariant.Value <> "GB_TankStatusList", "GB_Allocate", "Default")
        COA0013TableObject.SRCDATA = listData
        COA0013TableObject.TBLOBJ = Me.WF_LISTAREA
        COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.LEVENT = If(Me.hdnListEvent.Value = "", Nothing, Me.hdnListEvent.Value)
        COA0013TableObject.LFUNC = If(Me.hdnListFunc.Value = "", Nothing, Me.hdnListFunc.Value)
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.NOCOLUMNWIDTHOPT = 60
        COA0013TableObject.OPERATIONCOLUMNWIDTHOPT = -1
        COA0013TableObject.USERSORTOPT = 1
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""
        '現在の表示LINECNTを保持
        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            For Each targetCheckBoxId As String In {"ALLOCATECHK"}

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
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If


    End Sub
    ''' <summary>
    ''' リスト行ダブルクリック時イベント
    ''' </summary>
    Private Sub ListRowDbClick()
        Dim tankNo As String = ""
        Me.hdnSelectedTankId.Value = ""

        '一覧表示データ復元
        Dim dt As DataTable = CreateListDataTable()
        If Me.SavedDt Is Nothing Then
            dt = CreateListDataTable()
            Dim COA0021ListTable As New COA0021ListTable
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

        Dim lineCnt As String = Me.hdnListDBclick.Value
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim targetRow As DataRow = dt.Rows(Convert.ToInt32(lineCnt) - 1)
        Me.hdnSelectedTankId.Value = Convert.ToString(targetRow.Item("TANKNO"))
        Dim needsApply As Boolean = False
        If Convert.ToString(targetRow.Item("CANPROVISION")).Trim <> CONST_FLAG_NO Then
            needsApply = True
        End If
        Dim nextDate As Date
        Dim dicTestTypeMinusMonth As New Dictionary(Of String, Integer) From {{"2.5", -1}, {"5", -3}}
        If needsApply = False AndAlso
           Date.TryParse(Convert.ToString(targetRow.Item("T_NEXTDATE")), nextDate) = True AndAlso
           dicTestTypeMinusMonth.ContainsKey(Convert.ToString(targetRow.Item("T_NEXTTYPE")).Trim) Then
            nextDate = Date.Parse(Convert.ToString(targetRow.Item("T_NEXTDATE")))
            Dim appendMonth As Integer = dicTestTypeMinusMonth(Convert.ToString(targetRow.Item("T_NEXTTYPE")).Trim)
            Dim checkDate As Date = nextDate.AddMonths(appendMonth)
            If checkDate <= Date.Now Then
                needsApply = True
            End If
        End If
        Me.NeedsTankUseApply = needsApply

        btnBack_Click()
    End Sub
    ''' <summary>
    ''' 一覧表のEDITボタン押下時処理
    ''' </summary>
    Public Sub btnShowTankMaster_Click()
        Dim thisDisplayItems As GBT00006RESULT.GBT00006RITEMS = GetGbt00006items()
        Me.DisplayItems = thisDisplayItems
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '■■■ 画面遷移先URL取得 ■■■
        COA0012DoUrl.MAPIDP = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
        COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        Dim url As String = COA0012DoUrl.URL
        '画面遷移実行
        Server.Transfer(url)
    End Sub
    ''' <summary>
    ''' 引当ボタン押下時処理
    ''' </summary>
    Public Sub btnAllocate_Click()
        Me.lblFooterMessage.Text = ""
        If ViewState("ORDERINFO") Is Nothing Then
            'ありえないがオーダー情報が保持されていない場合は終了
            Return
        End If
        'オーダー画面より引き継いだオーダー、タンクSEQ、タンクNoを取得
        Dim orderInfo As GBT00006RESULT.GBT00006ROrderInfo = DirectCast(ViewState("ORDERINFO"), GBT00006RESULT.GBT00006ROrderInfo)
        '当画面よりチェックした一覧を取得
        Dim selectedTankInfo As DataTable = Nothing
        Dim q As IEnumerable(Of DataRow) = (From item In Me.SavedDt
                                            Where Convert.ToString(item("ALLOCATECHK")) = "on").GroupBy(Function(p) p("TANKNO")) _
                                           .Select(Function(group) group.First())
        Dim selectedTankList As New List(Of String)
        If q.Any Then
            selectedTankInfo = q.CopyToDataTable
            selectedTankList = (From item In selectedTankInfo Select Convert.ToString(item("TANKNO"))).ToList
        Else
            selectedTankInfo = Me.SavedDt.Clone
        End If
        '引当チェック済レコードが引当対象のタンクSEQを超過しているかチェック
        Dim tankInfoList = orderInfo.TankInfoList
        If selectedTankInfo.Rows.Count > tankInfoList.Count Then
            CommonFunctions.ShowMessage(C_MESSAGENO.TOOMANYALOCATETANKS, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {Convert.ToString(tankInfoList.Count),
                                                                                 Convert.ToString(selectedTankInfo.Rows.Count)})
            Return
        End If

        '****************************************
        'チェックが外されたタンクをブランクに変更
        '****************************************
        Dim unCheckTankSeqQ = (From tankInfo In tankInfoList
                               Where Not selectedTankList.Contains(tankInfo.Value.TankNoPrevMod)
                               Select tankInfo.Key)
        If unCheckTankSeqQ.Any = True Then
            For Each tankSeq As String In unCheckTankSeqQ
                tankInfoList(tankSeq).TankNo = ""
                tankInfoList(tankSeq).NeedsApply = False
                tankInfoList(tankSeq).LastStep = ""
            Next
        End If

        For Each dr As DataRow In selectedTankInfo.Rows
            Dim tankNo As String = Convert.ToString(dr.Item("TANKNO"))
            Dim tankSeq As String = ""
            '変更なしのタンクはスキップ
            Dim allocatedtankQue = (From tankInfo In tankInfoList
                                    Where tankInfo.Value.TankNo = tankNo)
            If allocatedtankQue.Any Then
                Continue For
            End If
            '未割り当てのタンクSEQ取得
            tankSeq = (From tankInfo In tankInfoList
                       Where tankInfo.Value.TankNo = ""
                       Select tankInfo.Key).FirstOrDefault
            tankInfoList(tankSeq).TankNo = tankNo
            If Convert.ToString(dr.Item("NEEDSAPPLY")) = "1" Then
                tankInfoList(tankSeq).NeedsApply = True
            End If
        Next

        '引当処理すべきレコードが無い場合
        If (From item In tankInfoList Where item.Value.TankNo <> item.Value.TankNoPrevMod).Any = False Then
            '未選択の場合メッセージを表示し処理終了
            CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Dim needsApplyTankNo As New List(Of String)
        '遷移時と変化があり申請必要なTANKNOを絞り込み
        Dim needsApplyTankNoq = (From item In tankInfoList
                                 Where item.Value.TankNo <> item.Value.TankNoPrevMod _
                               AndAlso item.Value.NeedsApply = True _
                               AndAlso item.Value.TankNo <> ""
                                 Select item.Value.TankNo)
        If needsApplyTankNoq.Any Then
            needsApplyTankNo = needsApplyTankNoq.ToList
        End If
        '申請必須の場合
        If needsApplyTankNo.Count > 0 Then
            'ダイアログの設定をする
            Dim tankList As String = "<ul style='margin-left:20px;'>"
            For Each tankNo In needsApplyTankNo
                tankList = tankList & "<li>" & tankNo & "</li>"
            Next
            tankList = tankList & "</ul>"
            Me.lblConfirmMessage.Text = String.Format(Me.hdnApplyMessage.Value, tankList)
            divConfirmBoxWrapper.Style.Item(HtmlTextWriterStyle.Display) = "block"
            ViewState("UPDTANKINFO") = tankInfoList
            Return
        End If
        'タンク登録
        Dim procDate As Date = Now
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            For Each tankInfo In tankInfoList.Values

                'タンクNoに変化のないデータはスキップ
                If tankInfo.TankNo = tankInfo.TankNoPrevMod Then
                    Continue For
                End If
                'タンクシーケンス単位にトランザクションを張る
                Using tran As SqlTransaction = sqlCon.BeginTransaction
                    UpdateOrderValue(orderInfo.OrderNo, tankInfo, sqlCon, tran, procDate)
                    UpdateOrderValue2(orderInfo.OrderNo, tankInfo, sqlCon, tran, procDate)
                    tran.Commit()
                End Using
            Next
        End Using

        orderInfo.IsAllocated = True
        Me.OrderInfo = orderInfo
        btnBack_Click()
    End Sub
    ''' <summary>
    ''' 確認ダイアログOKクリック時
    ''' </summary>
    Public Sub btnConfirmOk_Click()
        If ViewState("UPDTANKINFO") Is Nothing Then
            Return
        End If
        If ViewState("ORDERINFO") Is Nothing Then
            'ありえないがオーダー情報が保持されていない場合は終了
            Return
        End If
        'オーダー画面より引き継いだオーダー、タンクSEQ、タンクNoを取得
        Dim orderInfo As GBT00006RESULT.GBT00006ROrderInfo = DirectCast(ViewState("ORDERINFO"), GBT00006RESULT.GBT00006ROrderInfo)

        '引当済情報の取得
        Dim tankInfoList As Dictionary(Of String, GBT00006RESULT.GBT00006RTankInfo) = DirectCast(ViewState("UPDTANKINFO"), Dictionary(Of String, GBT00006RESULT.GBT00006RTankInfo))
        Dim needsApplyTankInfoQue = (From item In tankInfoList
                                     Where item.Value.TankNo <> item.Value.TankNoPrevMod _
                                   AndAlso item.Value.NeedsApply = True _
                                   AndAlso item.Value.TankNo <> ""
                                     Select item.Value)
        'ここまで来て要申請のデータが未存在はありえないが念のため
        If needsApplyTankInfoQue.Any = False Then
            Return
        End If
        '申請登録
        Dim needsApplyTankInfo = needsApplyTankInfoQue.ToList
        Dim messageNo As String = EntryApply(needsApplyTankInfo)
        If messageNo <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)
        End If

        For Each needsApplyTank In needsApplyTankInfo
            With tankInfoList(needsApplyTank.TankSeq)
                .ApplyId = needsApplyTank.ApplyId
                .LastStep = needsApplyTank.LastStep
            End With
        Next
        'タンク登録
        Dim procDate As Date = Now
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            For Each tankInfo In tankInfoList.Values

                'タンクNoに変化のないデータはスキップ
                If tankInfo.TankNo = tankInfo.TankNoPrevMod Then
                    Continue For
                End If
                'タンクシーケンス単位にトランザクションを張る
                Using tran As SqlTransaction = sqlCon.BeginTransaction
                    UpdateOrderValue(orderInfo.OrderNo, tankInfo, sqlCon, tran, procDate)
                    UpdateOrderValue2(orderInfo.OrderNo, tankInfo, sqlCon, tran, procDate)
                    tran.Commit()
                End Using
            Next
        End Using
        ViewState("UPDTANKINFO") = Nothing
        orderInfo.IsAllocated = True
        Me.OrderInfo = orderInfo
        btnBack_Click()
    End Sub
    ''' <summary>
    ''' Excelダウンロードボタン押下時
    ''' </summary>
    Public Sub btnExcelDownload_Click()
        Dim dt As DataTable = CreateListDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        '一覧表示データ復元 
        If Me.SavedDt Is Nothing Then
            dt = CreateListDataTable()
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
    ''' 画面表示のテーブルを制御する
    ''' </summary>
    Private Sub DisplayListObjEdit()
        '引当不可にするレコードを色付けする判定
        Dim dicTestTypeMinusMonth As New Dictionary(Of String, Integer) From {{"2.5", -1}, {"5", -3}}

        Dim targetPanel As Panel = Me.WF_LISTAREA

        Dim rightDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DR"), Panel)
        If rightDataDiv.HasControls = False _
           OrElse Not (TypeOf rightDataDiv.Controls(0) Is Table) _
           OrElse DirectCast(rightDataDiv.Controls(0), Table).Rows.Count = 0 Then
            Return
        End If
        Dim rightHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HR"), Panel)
        Dim rightHeaderTable As Table = DirectCast(rightHeaderDiv.Controls(0), Table)
        Dim dicColumnNameToNo As New Dictionary(Of String, String) From {{"CANPROVISION", ""},
                                                                         {"T_NEXTTYPE", ""},
                                                                         {"T_NEXTDATE", ""},
                                                                         {"CANALLOCATE", ""}}
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
        Dim dicLeftColumnNameToNo As New Dictionary(Of String, String) From {{"TANKNO", ""}, {"EDIT", ""}, {"ACTY", ""}, {"ALLOCATECHK", ""}}

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
        '点検日付格納
        Dim targetDate As Date = Now
        If Date.TryParse(Me.hdnOrderMaxEtd.Value, targetDate) = True Then
            targetDate = Date.Parse(Me.hdnOrderMaxEtd.Value)
        Else
            targetDate = Date.Now
        End If
        Dim repairAttr As String = "data-repair"
        Dim inspectionSoonAttr As String = "data-inspectionsoon"
        Dim rowCnt As Integer = rightDataTable.Rows.Count - 1
        For i = 0 To rowCnt
            Dim tbrRight As TableRow = rightDataTable.Rows(i)
            Dim tbrLeft As TableRow = leftDataTable.Rows(i)

            Dim lineCnt As String = tbrLeft.Cells(0).Text

            '各行の編集ボタンを加工
            If dicLeftColumnNameToNo("EDIT") <> "" AndAlso
               dicLeftColumnNameToNo("TANKNO") <> "" Then
                Dim tankNo As String = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO"))).Text
                With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("EDIT")))
                    If .HasControls = True AndAlso TypeOf .Controls(0) Is HtmlButton Then
                        Dim tmpBtn As HtmlButton = DirectCast(.Controls(0), HtmlButton)
                        Dim tmpInpBtn As New HtmlInputButton("button") With {.ViewStateMode = ViewStateMode.Disabled,
                                                                             .ID = tmpBtn.ID, .Name = tmpBtn.ID,
                                                                             .Value = "EDIT"}
                        tmpInpBtn.Attributes.Add("onclick", String.Format("showTankMaster('{0}'); return false;", tankNo))
                        .Controls.Clear()
                        .Controls.Add(tmpInpBtn)

                    End If
                End With
            End If
            'リペアステータス判定
            If dicColumnNameToNo("CANPROVISION") <> "" AndAlso
               dicLeftColumnNameToNo("TANKNO") <> "" Then
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("CANPROVISION")))
                    If .Text.Trim <> CONST_FLAG_NO Then
                        'リペアステータスが0の場合修理中の為、行に属性を追加
                        tbrRight.Attributes.Add(repairAttr, "1")
                        tbrLeft.Attributes.Add(repairAttr, "1")
                        With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO")))
                            .Attributes.Add(repairAttr, "1")
                        End With
                    End If
                End With
            End If
            '定期点検日付チェック
            Dim nextDate As Date
            If dicColumnNameToNo("T_NEXTTYPE") <> "" AndAlso
               dicColumnNameToNo("T_NEXTDATE") <> "" AndAlso
               dicLeftColumnNameToNo("TANKNO") <> "" AndAlso
               Date.TryParse(tbrRight.Cells(Integer.Parse(dicColumnNameToNo("T_NEXTDATE"))).Text, nextDate) = True AndAlso
               dicTestTypeMinusMonth.ContainsKey(tbrRight.Cells(Integer.Parse(dicColumnNameToNo("T_NEXTTYPE"))).Text.Trim) Then

                nextDate = Date.Parse(tbrRight.Cells(Integer.Parse(dicColumnNameToNo("T_NEXTDATE"))).Text)
                Dim appendMonth As Integer = dicTestTypeMinusMonth(tbrRight.Cells(Integer.Parse(dicColumnNameToNo("T_NEXTTYPE"))).Text.Trim)
                Dim checkDate As Date = nextDate.AddMonths(appendMonth)


                If checkDate <= targetDate Then
                    tbrRight.Attributes.Add(inspectionSoonAttr, "1")
                    tbrLeft.Attributes.Add(inspectionSoonAttr, "1")
                    With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO")))
                        .Attributes.Add(inspectionSoonAttr, "1")
                    End With
                End If
            End If
            'チェックボックスの使用可否制御
            If dicColumnNameToNo("CANALLOCATE") <> "" AndAlso
               dicLeftColumnNameToNo("ALLOCATECHK") <> "" AndAlso
               tbrRight.Cells(Integer.Parse(dicColumnNameToNo("CANALLOCATE"))).Text <> "1" Then
                With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("ALLOCATECHK")))
                    Dim chkAllocateObj As CheckBox = DirectCast(.Controls(0), CheckBox)
                    chkAllocateObj.Enabled = False
                End With
            ElseIf dicColumnNameToNo("CANALLOCATE") <> "" AndAlso
                   dicLeftColumnNameToNo("ALLOCATECHK") <> "" Then
                With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("ALLOCATECHK")))
                    Dim chkAllocateObj As CheckBox = DirectCast(.Controls(0), CheckBox)
                    'chkAllocateObj.Attributes.Add("onclick", "this.blur();this.focus();")
                    chkAllocateObj.Attributes.Add("onchange", "allocateCount('" & chkAllocateObj.ClientID & "');")

                End With
            End If
            ''デマレージにてJOTのみOFFICEの変更を許可する(TODO：JOTユーザー判定)
            'If Me.hdnListMapVariant.Value = "GB_Demurrage" AndAlso
            '   dicColumnNameToNo("DTLOFFICE") <> "" AndAlso
            '   dicColumnNameToNo("ORGOFFICE") <> "" Then
            '    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("DTLOFFICE")))
            '        .Attributes.Add("ondblclick", String.Format("swapOffice('{0}');", lineCnt))
            '        If .Text <> tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ORGOFFICE"))).Text Then
            '            .CssClass = "swappedOffice"
            '        End If
            '    End With
            'End If
            ''SOA時にGB_Demurrageの項目を使用不可に変更
            'If Me.hdnListMapVariant.Value = "GB_SOA" AndAlso
            '   ((dicColumnNameToNo("COSTCODE") <> "" AndAlso
            '   tbrRight.Cells(Integer.Parse(dicColumnNameToNo("COSTCODE"))).Text = CONST_COSTCODE_DEMURRAGE) OrElse
            '    (dicColumnNameToNo("ISBILLINGCLOSED") <> "" AndAlso
            '    tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ISBILLINGCLOSED"))).Text = "1")) Then
            '    For Each fieldName As String In {"AMOUNTFIX", "CONTRACTORFIX", "ACTUALDATE", "AMOUNTPAY", "SOAAPPDATE", "LOCALPAY"}
            '        If dicColumnNameToNo(fieldName) <> "" Then
            '            With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
            '                If Not .Text.Contains("readonly=") Then
            '                    .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
            '                    .Style.Add("pointer-events", "none")
            '                End If
            '            End With
            '        End If
            '    Next
            '    'SOAでは申請すらさせない
            '    If dicColumnNameToNo("APPLY") <> "" AndAlso dicColumnNameToNo("APPLYTEXT") <> "" Then
            '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("APPLY")))
            '            .Controls.Clear()
            '            .Text = ""
            '        End With

            '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("APPLYTEXT")))
            '            .Controls.Clear()
            '            .Text = ""
            '        End With
            '    End If
            'End If
            ''SOA時にGB_Demurrageの項目を使用不可に変更
            'If Me.hdnListMapVariant.Value = "GB_SOA" AndAlso
            '   (dicColumnNameToNo("ISAUTOCLOSE") <> "" OrElse dicColumnNameToNo("ISAUTOCLOSELONG") <> "") Then
            '    If dicColumnNameToNo("ISAUTOCLOSE") <> "" Then
            '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ISAUTOCLOSE")))
            '            If .Text = "1" Then
            '                tbrRight.Attributes.Add("data-isautoclose", "1")
            '                tbrLeft.Attributes.Add("data-isautoclose", "1")
            '            End If
            '        End With
            '    End If
            '    If dicColumnNameToNo("ISAUTOCLOSELONG") <> "" Then
            '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ISAUTOCLOSELONG")))
            '            If .Text = "1" Then
            '                tbrRight.Attributes.Add("data-isautoclose", "2")
            '                tbrLeft.Attributes.Add("data-isautoclose", "2")
            '            End If
            '        End With
            '    End If

            'End If
            ''ブレーカー項目入力項目を使用不可に
            'For Each fieldName As String In {"AMOUNTBR", "CONTRACTORBR", "SCHEDELDATEBR"}
            '    If dicColumnNameToNo(fieldName) <> "" Then
            '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
            '            If .Text.StartsWith(String.Format("<input id=""txtWF_LISTAREA{0}", fieldName)) Then
            '                .Text = .Text.Replace(">", " disabled=""disabled"" class=""aspNetDisabled"" />")
            '            End If
            '        End With
            '    End If
            'Next

            ''タンク関連処理
            ''ノンブレ・SOA以外の場合は引当・引きはがしのタグを挿入する
            'If (dicColumnNameToNo("TANKNO") <> "" OrElse dicLeftColumnNameToNo("TANKNO") <> "") AndAlso
            '   dicLeftColumnNameToNo("ORDERNO") <> "" AndAlso
            '   dicLeftColumnNameToNo("TANKSEQ") <> "" AndAlso
            '   Not {"GB_NonBreaker", "GB_SOA", "GB_CostUp", "GB_TankActivity"}.Contains(Me.hdnListMapVariant.Value) Then
            '    Dim orderNo As String = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("ORDERNO"))).Text
            '    Dim tankSeq As String = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKSEQ"))).Text

            '    Dim dataId As String = ""
            '    If dicColumnNameToNo("DATAID") <> "" Then
            '        dataId = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("DATAID"))).Text
            '    End If
            '    If dicColumnNameToNo("TANKNO") <> "" Then
            '        '右にTANKNOがある場合
            '        Dim tankNo As String = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("TANKNO"))).Text

            '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("TANKNO")))
            '            Dim canDelete As Boolean = False
            '            If tankNo = "" Then
            '                .CssClass = "noTank"
            '            Else
            '                .CssClass = "hasTank"
            '                canDelete = True
            '            End If

            '            .Text = String.Format("<span ondblclick=""browseTankList('{1}', '{2}', '{3}');"">{0}</span>", tankNo, orderNo, tankSeq, dataId)
            '            If canDelete = True Then
            '                .Text = .Text & String.Format("<span class=""deleteTank"" onclick=""deleteTankNo('{0}', '{1}', '{2}');""></span>", orderNo, tankSeq, dataId)
            '            End If
            '        End With
            '    ElseIf dicLeftColumnNameToNo("TANKNO") <> "" Then
            '        '左にTANKNOがある場合
            '        Dim tankNo As String = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO"))).Text

            '        With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO")))
            '            Dim canDelete As Boolean = False
            '            If tankNo = "" Then
            '                .CssClass = "noTank"
            '            Else
            '                .CssClass = "hasTank"
            '                canDelete = True
            '            End If

            '            .Text = String.Format("<span ondblclick=""browseTankList('{1}', '{2}', '{3}');"">{0}</span>", tankNo, orderNo, tankSeq, dataId)
            '            If canDelete = True Then
            '                .Text = .Text & String.Format("<span class=""deleteTank"" onclick=""deleteTankNo('{0}', '{1}', '{2}');""></span>", orderNo, tankSeq, dataId)
            '            End If
            '        End With
            '    End If
            'End If
            ''課税フラグの表示制御
            'If dicColumnNameToNo("TAXATION") <> "" AndAlso dicColumnNameToNo("COUNTRYCODE") <> "" Then
            '    Dim rowCountryCode As String = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("COUNTRYCODE"))).Text
            '    If rowCountryCode <> "JP" Then
            '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("TAXATION")))
            '            If .HasControls = True AndAlso TypeOf .Controls(0) Is CheckBox Then
            '                Dim chkObj As CheckBox = DirectCast(.Controls(0), CheckBox)
            '                chkObj.Style.Add("display", "none")
            '            End If

            '        End With
            '    End If
            'End If
            ''削除ボタンの表示非表示制御
            'If dicLeftColumnNameToNo("ACTION") <> "" Then
            '    With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("ACTION")))
            '        If .HasControls = True AndAlso TypeOf .Controls(0) Is HtmlButton AndAlso
            '            hideDelete = "1" Then

            '            .Controls.RemoveAt(0)
            '        ElseIf .HasControls = True AndAlso TypeOf .Controls(0) Is HtmlButton Then
            '            Dim htmlbutton As HtmlButton = DirectCast(.Controls(0), HtmlButton)
            '            Dim htmlInputButton As New HtmlInputButton
            '            If htmlbutton.Attributes.Count > 0 Then
            '                For Each attrKey As String In htmlbutton.Attributes.Keys
            '                    htmlInputButton.Attributes.Add(attrKey, htmlbutton.Attributes(attrKey))
            '                Next
            '            End If
            '            htmlInputButton.ID = htmlbutton.ID
            '            htmlInputButton.Style.Add(HtmlTextWriterStyle.Display, "inline-block")
            '            htmlInputButton.Value = Me.hdnListDeleteName.Value
            '            .Controls.RemoveAt(0)
            '            .Controls.Add(htmlInputButton)
            '        End If

            '    End With
            'End If
        Next 'END ROWCOUNT
    End Sub
    ''' <summary>
    ''' 申請登録処理
    ''' </summary>
    ''' <param name="applyTankInfo"></param>
    ''' <returns></returns>
    Private Function EntryApply(ByRef applyTankInfo As List(Of GBT00006RESULT.GBT00006RTankInfo)) As String
        Dim eventCode As String = C_TKAEVENT.APPLY

        '申請ID取得オブジェクトの生成
        Dim GBA00011ApplyID As New GBA00011ApplyID With {
                .COMPCODE = GBC_COMPCODE_D, 'COA0019Session.APSRVCamp,
                .SYSCODE = C_SYSCODE_GB,
                .KEYCODE = COA0019Session.APSRVname,
                .DIVISION = "T",
                .SEQOBJID = C_SQLSEQ.TKAAPPLY,
                .SEQLEN = 6
                }
        '申請処理共通オブジェクトの生成
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval With {
            .I_COMPCODE = COA0019Session.APSRVCamp,
            .I_MAPID = CONST_MAPID,
            .I_EVENTCODE = eventCode
        }

        '申請対象レコードのループ
        Dim applyId As String = ""
        Dim subCode As String = ""
        Dim lastStep As String = ""
        Dim skipApplyData As New List(Of DataRow) '他者更新により読み飛ばしたデータ
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            For Each item In applyTankInfo
                '申請IDの取得
                GBA00011ApplyID.GBA00011getApplyID()
                If GBA00011ApplyID.ERR = C_MESSAGENO.NORMAL Then
                    item.ApplyId = GBA00011ApplyID.APPLYID
                Else
                    Return GBA00011ApplyID.ERR
                End If

                If item.ApplyId = "" Then
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
                '申請登録
                subCode = "" 'Convert.ToString(dr.Item("AGENTORGANIZER"))
                COA0032Apploval.I_APPLYID = item.ApplyId
                COA0032Apploval.I_SUBCODE = subCode
                COA0032Apploval.COA0032setApply()

                If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
                    item.LastStep = COA0032Apploval.O_LASTSTEP
                Else
                    Return COA0032Apploval.O_ERR
                End If

            Next 'End For Each dr In applyTankInfo
        End Using
        Return C_MESSAGENO.NORMAL
    End Function
    Private Function UpdateOrderValue(orderNo As String,
                                      tankInfo As GBT00006RESULT.GBT00006RTankInfo,
                                      ByRef sqlCon As SqlConnection,
                                      Optional ByRef sqlTran As SqlTransaction = Nothing,
                                      Optional procDate As Date = #1900/1/1#) As String

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
        sqlStat.AppendLine("         ,@UPDYMD         AS INITYMD")
        sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND TANKNO   <> @TANKNO")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

        sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("   SET TANKNO         = @TANKNO ")
        sqlStat.AppendLine("      ,SCHEDELDATE    = CASE WHEN ACTIONID = 'TKAL' THEN @SCHEDELDATE ELSE SCHEDELDATE END")
        sqlStat.AppendLine("      ,ACTUALDATE     = CASE WHEN ACTIONID = 'TKAL' THEN @ACTUALDATE  ELSE ACTUALDATE END")
        sqlStat.AppendLine("      ,UPDYMD         = @UPDYMD")
        sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
        sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND TANKNO   <> @TANKNO")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
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
        'sqlStat.AppendLine("         ,@TANKNO        AS TANKNO")
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
        'sqlStat.AppendLine("         ,CASE WHEN ACTIONID IN ('TKAL','TAED','TAEC') THEN @SCHEDELDATE ELSE SCHEDELDATE END")
        'sqlStat.AppendLine("         ,CASE WHEN ACTIONID IN ('TKAL','TAED','TAEC') THEN @ACTUALDATE  ELSE ACTUALDATE END")
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
        'sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        'sqlStat.AppendLine("   AND TANKNO   <> @TANKNO")
        'sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        'sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

        'sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        'sqlStat.AppendLine("   SET UPDYMD         = @UPDYMD")
        'sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
        'sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
        'sqlStat.AppendLine("      ,DELFLG         = @DELFLG")
        'sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
        'sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        'sqlStat.AppendLine("   AND TANKNO   <> @TANKNO")
        'sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        'sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
#End Region

        Dim updDateString As String = ""
        If tankInfo.TankNo <> "" Then
            updDateString = Date.Now.ToString("yyyy/MM/dd")
        End If

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@TANKNO", SqlDbType.NVarChar).Value = tankInfo.TankNo

                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate


                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = tankInfo.TankSeq
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                If tankInfo.TankNo = "" Then
                    '引き剥がしの場合は日付クリア
                    .Add("@SCHEDELDATE", SqlDbType.NVarChar).Value = ""
                    .Add("@ACTUALDATE", SqlDbType.NVarChar).Value = ""
                ElseIf tankInfo.NeedsApply Then
                    '申請必須タンクの場合は予定のみ
                    .Add("@SCHEDELDATE", SqlDbType.NVarChar).Value = updDateString
                    .Add("@ACTUALDATE", SqlDbType.NVarChar).Value = ""
                Else
                    '申請必須タンクの場合は予定のみ
                    .Add("@SCHEDELDATE", SqlDbType.NVarChar).Value = updDateString
                    .Add("@ACTUALDATE", SqlDbType.NVarChar).Value = updDateString
                End If
            End With
            sqlCmd.ExecuteNonQuery()
        End Using
        Return C_MESSAGENO.NORMALDBENTRY

    End Function
    Private Function UpdateOrderValue2(orderNo As String,
                                      tankInfo As GBT00006RESULT.GBT00006RTankInfo,
                                      ByRef sqlCon As SqlConnection,
                                      Optional ByRef sqlTran As SqlTransaction = Nothing,
                                      Optional procDate As Date = #1900/1/1#) As String

        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        Dim sqlStat As New StringBuilder
        'SQL文作成

        sqlStat.AppendLine("INSERT INTO GBT0007_ODR_VALUE2 (")
        sqlStat.AppendLine("      ORDERNO")
        sqlStat.AppendLine("     ,STYMD")
        sqlStat.AppendLine("     ,ENDYMD")
        sqlStat.AppendLine("     ,TANKSEQ")
        sqlStat.AppendLine("     ,TRILATERAL")
        sqlStat.AppendLine("     ,TANKTYPE")
        sqlStat.AppendLine("     ,GROSSWEIGHT")
        sqlStat.AppendLine("     ,NETWEIGHT")
        sqlStat.AppendLine("     ,SEALNO1")
        sqlStat.AppendLine("     ,SEALNO2")
        sqlStat.AppendLine("     ,SEALNO3")
        sqlStat.AppendLine("     ,SEALNO4")
        sqlStat.AppendLine("     ,EMPTYORFULL")
        sqlStat.AppendLine("     ,NOOFPACKAGE")
        sqlStat.AppendLine("     ,EXSHIPRATE")
        sqlStat.AppendLine("     ,INSHIPRATE")
        sqlStat.AppendLine("     ,APPLYID")
        sqlStat.AppendLine("     ,APPLYTEXT")
        sqlStat.AppendLine("     ,LASTSTEP")
        sqlStat.AppendLine("     ,DELFLG")
        sqlStat.AppendLine("     ,INITYMD")
        sqlStat.AppendLine("     ,UPDYMD")
        sqlStat.AppendLine("     ,UPDUSER")
        sqlStat.AppendLine("     ,UPDTERMID")
        sqlStat.AppendLine("     ,RECEIVEYMD")
        sqlStat.AppendLine(" ) SELECT ")
        sqlStat.AppendLine("      ORDERNO")
        sqlStat.AppendLine("     ,STYMD")
        sqlStat.AppendLine("     ,ENDYMD")
        sqlStat.AppendLine("     ,TANKSEQ")
        sqlStat.AppendLine("     ,TRILATERAL")
        sqlStat.AppendLine("     ,TANKTYPE")
        sqlStat.AppendLine("     ,GROSSWEIGHT")
        sqlStat.AppendLine("     ,NETWEIGHT")
        sqlStat.AppendLine("     ,SEALNO1")
        sqlStat.AppendLine("     ,SEALNO2")
        sqlStat.AppendLine("     ,SEALNO3")
        sqlStat.AppendLine("     ,SEALNO4")
        sqlStat.AppendLine("     ,EMPTYORFULL")
        sqlStat.AppendLine("     ,NOOFPACKAGE")
        sqlStat.AppendLine("     ,EXSHIPRATE")
        sqlStat.AppendLine("     ,INSHIPRATE")
        sqlStat.AppendLine("     ,@APPLYID")
        sqlStat.AppendLine("     ,APPLYTEXT")
        sqlStat.AppendLine("     ,@LASTSETP")
        sqlStat.AppendLine("     ,'2'             AS DELFLG")
        sqlStat.AppendLine("     ,@UPDYMD         AS INITYMD")
        sqlStat.AppendLine("     ,@UPDYMD         AS UPDYMD")
        sqlStat.AppendLine("     ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("     ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("     ,@RECEIVEYMD     AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0007_ODR_VALUE2")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        sqlStat.AppendLine("   AND DELFLG    = @DELFLG")

        sqlStat.AppendLine("UPDATE GBT0007_ODR_VALUE2")
        sqlStat.AppendLine("   SET UPDYMD         = @UPDYMD")
        sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
        sqlStat.AppendLine("      ,DELFLG         = '" & CONST_FLAG_YES & "'")
        sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        sqlStat.AppendLine("   AND DELFLG    = @DELFLG")

        sqlStat.AppendLine("UPDATE GBT0007_ODR_VALUE2")
        sqlStat.AppendLine("   SET DELFLG         = @DELFLG")
        sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        sqlStat.AppendLine("   AND DELFLG    = '2'")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters

                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = tankInfo.TankSeq
                .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(tankInfo.ApplyId)
                .Add("@LASTSETP", SqlDbType.NVarChar).Value = Convert.ToString(tankInfo.LastStep)
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
            End With
            sqlCmd.ExecuteNonQuery()
        End Using
        Return C_MESSAGENO.NORMALDBENTRY

    End Function
    ''' <summary>
    ''' 当画面の保持必要情報を保持し退避用クラスを生成
    ''' </summary>
    ''' <returns></returns>
    Public Function GetGbt00006items() As GBT00006RESULT.GBT00006RITEMS
        Dim item As New GBT00006RESULT.GBT00006RITEMS
        item.SearchType = Me.hdnSearchType.Value
        item.ETDStYMD = Me.hdnETDStYMD.Value
        item.ETDEndYMD = Me.hdnETDEndYMD.Value
        item.ETAStYMD = Me.hdnETAStYMD.Value
        item.ETAEndYMD = Me.hdnETAEndYMD.Value
        item.Shipper = Me.hdnShipper.Value
        item.Consignee = Me.hdnConsignee.Value
        item.PortOfLoading = Me.hdnPortOfLoading.Value
        item.PortOfDischarge = Me.hdnPortOfDischarge.Value
        item.Office = Me.hdnOffice.Value
        item.BrId = Me.hdnBrId.Value
        item.FillingDate = Me.hdnFillingDate.Value
        item.Etd1 = Me.hdnFillingDate.Value
        item.Eta1 = Me.hdnEta1.Value
        item.Etd2 = Me.hdnEtd2.Value
        item.Eta2 = Me.hdnEta2.Value
        item.OrderOrgXMLsaveFile = Me.hdnOrderOrgXMLsaveFile.Value
        item.OrderXMLsaveFile = Me.hdnOrderXMLsaveFile.Value
        item.IsNewData = Me.hdnIsNewData.Value
        item.Copy = Me.hdnCopy.Value
        item.SelectedOrderId = Me.hdnSelectedOrderId.Value
        item.SelectedTankSeq = Me.hdnSelectedTankSeq.Value
        item.SelectedDataId = Me.hdnSelectedDataId.Value
        item.OrderDispListPosition = Me.hdnOrderDispListPosition.Value
        item.ListMapVariant = Me.hdnListMapVariant.Value
        item.DateTermStYMD = Me.hdnDateTermStYMD.Value
        item.DateTermEndYMD = Me.hdnDateTermEndYMD.Value
        item.Approval = Me.hdnApproval.Value
        item.ReportVariant = Me.hdnReportVariant.Value
        item.ListId = Me.hdnListId.Value
        item.Acty = Me.hdnActy.Value
        item.OrderMaxEtd = Me.hdnOrderMaxEtd.Value
        item.Gbt00006RXMLsaveFile = Me.hdnXMLsaveFile.Value
        item.Gbt00006MapVariant = Me.hdnThisMapVariant.Value
        item.Gbt00006SelectedTankNo = Me.hdnSelectedTankId.Value
        item.Gbt00004OrderListSort = Me.hdnListSortValueGBT00004WF_LISTAREA.Value
        item.Gbt00006RCheckedTankNo = Nothing
        item.AllocateCountSelected = Me.hdnSelectedTankCount.Value
        item.AllocateCountMax = Me.lblAllocateTankMaxCount.Text
        Dim targetCheckBoxId As String = "ALLOCATECHK"
        Dim targetCheckedTankNo = (From dr As DataRow In Me.SavedDt
                                   Where Convert.ToString(dr.Item(targetCheckBoxId)) <> ""
                                   Select Convert.ToString(dr.Item("TANKNO")))
        If targetCheckedTankNo.Any Then
            item.Gbt00006RCheckedTankNo = targetCheckedTankNo.ToList
        End If
        item.Gbt00006ROrderInfo = DirectCast(ViewState("ORDERINFO"), GBT00006RESULT.GBT00006ROrderInfo)
        Return item
    End Function
    ''' <summary>
    ''' 退避情報を画面に戻す
    ''' </summary>
    ''' <param name="item"></param>
    Private Sub SetGbt00006items(item As GBT00006RESULT.GBT00006RITEMS)
        Me.DisplayItems = item
        Me.hdnSearchType.Value = item.SearchType
        Me.hdnETDStYMD.Value = item.ETDStYMD
        Me.hdnETDEndYMD.Value = item.ETDEndYMD
        Me.hdnETAStYMD.Value = item.ETAStYMD
        Me.hdnETAEndYMD.Value = item.ETAEndYMD
        Me.hdnShipper.Value = item.Shipper
        Me.hdnConsignee.Value = item.Consignee
        Me.hdnPortOfLoading.Value = item.PortOfLoading
        Me.hdnPortOfDischarge.Value = item.PortOfDischarge
        Me.hdnOffice.Value = item.Office
        Me.hdnBrId.Value = item.BrId
        Me.hdnFillingDate.Value = item.FillingDate
        Me.hdnFillingDate.Value = item.Etd1
        Me.hdnEta1.Value = item.Eta1
        Me.hdnEtd2.Value = item.Etd2
        Me.hdnEta2.Value = item.Eta2
        Me.hdnOrderOrgXMLsaveFile.Value = item.OrderOrgXMLsaveFile
        Me.hdnOrderXMLsaveFile.Value = item.OrderXMLsaveFile
        Me.hdnIsNewData.Value = item.IsNewData
        Me.hdnCopy.Value = item.Copy
        Me.hdnSelectedOrderId.Value = item.SelectedOrderId
        Me.hdnSelectedTankSeq.Value = item.SelectedTankSeq
        Me.hdnSelectedDataId.Value = item.SelectedDataId
        Me.hdnOrderDispListPosition.Value = item.OrderDispListPosition
        Me.hdnListMapVariant.Value = item.ListMapVariant
        Me.hdnDateTermStYMD.Value = item.DateTermStYMD
        Me.hdnDateTermEndYMD.Value = item.DateTermEndYMD
        Me.hdnApproval.Value = item.Approval
        Me.hdnReportVariant.Value = item.ReportVariant
        Me.hdnListId.Value = item.ListId
        Me.hdnActy.Value = item.Acty
        Me.hdnOrderMaxEtd.Value = item.OrderMaxEtd
        Me.hdnXMLsaveFile.Value = item.Gbt00006RXMLsaveFile
        Me.hdnThisMapVariant.Value = item.Gbt00006MapVariant
        Me.hdnSelectedTankCount.Value = item.AllocateCountSelected
        Me.lblAllocateTankMaxCount.Text = item.AllocateCountMax
        Me.hdnListSortValueGBT00004WF_LISTAREA.Value = item.Gbt00004OrderListSort
        ViewState("ORDERINFO") = item.Gbt00006ROrderInfo
    End Sub

    ''' <summary>
    ''' GBT000006画面情報退避用クラス
    ''' </summary>
    <Serializable>
    Public Class GBT00006RITEMS
        Public Property SearchType As String
        Public Property ETDStYMD As String
        Public Property ETDEndYMD As String
        Public Property ETAStYMD As String
        Public Property ETAEndYMD As String
        Public Property Shipper As String
        Public Property Consignee As String
        Public Property PortOfLoading As String
        Public Property PortOfDischarge As String
        Public Property Office As String
        Public Property BrId As String
        Public Property FillingDate As String
        Public Property Etd1 As String
        Public Property Eta1 As String
        Public Property Etd2 As String
        Public Property Eta2 As String
        Public Property OrderOrgXMLsaveFile As String
        Public Property OrderXMLsaveFile As String
        Public Property IsNewData As String
        Public Property Copy As String
        Public Property SelectedOrderId As String
        Public Property SelectedTankSeq As String
        Public Property SelectedDataId As String
        Public Property OrderDispListPosition As String
        Public Property ListMapVariant As String
        Public Property DateTermStYMD As String
        Public Property DateTermEndYMD As String
        Public Property Approval As String
        Public Property ReportVariant As String
        Public Property ListId As String
        Public Property Acty As String
        Public Property OrderMaxEtd As String
        Public Property Gbt00006RXMLsaveFile As String
        Public Property Gbt00006MapVariant As String
        Public Property Gbt00006SelectedTankNo As String

        Public Property Gbt00004OrderListSort As String

        Public Property Gbt00006ROrderInfo As GBT00006ROrderInfo
        Public Property Gbt00006RCheckedTankNo As List(Of String) = Nothing

        Public Property AllocateCountSelected As String
        Public Property AllocateCountMax As String

    End Class

    ''' <summary>
    ''' タンク情報クラス(オーダータンク引当情報)
    ''' </summary>
    <Serializable>
    Public Class GBT00006ROrderInfo
        ''' <summary>
        ''' オーダーNo
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderNo As String
        ''' <summary>
        ''' 引当済か(GBT00006画面にて引き当て処理済の場合はTrue,
        ''' それ以外はFalse(キャンセルでオーダー画面に戻ったパターン))
        ''' </summary>
        ''' <returns></returns>
        Public Property IsAllocated As Boolean = False
        ''' <summary>
        ''' 引当、引き剥がし時オーダー画面に戻る際に引き渡すメッセージ番号
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>上記 IsAllocatedがTrueの時のみ使用する想定</remarks>
        Public Property AllocateMessageNo As String = ""

        ''' <summary>
        ''' ETD
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ETD As String
        '''' <summary>
        '''' ETA
        '''' </summary>
        '''' <returns></returns>
        'Public Property ETA As String
        ''' <summary>
        ''' タンクSEQごとの情報
        ''' </summary>
        ''' <returns></returns>
        Public Property TankInfoList As Dictionary(Of String, GBT00006RTankInfo)

    End Class
    ''' <summary>
    ''' タンク単位情報(オーダータンク引当情報)
    ''' </summary>
    <Serializable>
    Public Class GBT00006RTankInfo
        ''' <summary>
        ''' TANKSEQ
        ''' </summary>
        ''' <returns></returns>
        Public Property TankSeq As String
        ''' <summary>
        ''' タンク番号
        ''' </summary>
        ''' <returns></returns>
        Public Property TankNo As String
        ''' <summary>
        ''' タンク番号（変更前）
        ''' 同じタンク番号をon/offに一度の操作でされた場合、
        ''' 同じシーケンスに割り振る為保持
        ''' </summary>
        ''' <returns></returns>
        Public Property TankNoPrevMod As String
        ''' <summary>
        ''' 申請必須(True:申請必須,False:申請不要(デフォルト))
        ''' </summary>
        ''' <returns></returns>
        Public Property NeedsApply As Boolean = False
        ''' <summary>
        ''' 申請ID
        ''' </summary>
        ''' <returns></returns>
        Public Property ApplyId As String
        ''' <summary>
        ''' 最終承認ステップ
        ''' </summary>
        ''' <returns></returns>
        Public Property LastStep As String
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="tankSeq"></param>
        ''' <param name="tankNo"></param>
        Public Sub New(tankSeq As String, tankNo As String)
            Me.TankSeq = tankSeq
            Me.TankNo = tankNo
            Me.TankNoPrevMod = tankNo
            Me.ApplyId = ""
            Me.LastStep = ""
            Me.NeedsApply = False
        End Sub
    End Class
End Class