Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL

''' <summary>
''' タンク動静管理タンク一覧画面クラス
''' </summary>
Public Class GBT00030TANKLIST
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00030T" '自身のMAPID
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
    Public Property DisplayItems As GBT00030DITEMS
    ''' <summary>
    ''' 引当情報
    ''' </summary>
    ''' <returns></returns>
    Public Property OrderInfo As GBT00030LIST.GBT00030OrderInfo
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
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                '前画面情報の引継ぎ
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '状態データ取得
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
                        .VARI = hdnThisViewVariant.Value
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

                End Using 'DataTable
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
            'Me.Page.Form.Attributes.Add("data-mapvari", Me.hdnThisMapVariant.Value)
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        Catch ex As Threading.ThreadAbortException
            'キャンセルやServerTransferにて後続の処理が打ち切られた場合のエラーは発生させない
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
                'TANKNO
                Case vLeftTankNo.ID
                    Dim dt As DataTable = GetTankNo()
                    With Me.lbTankNo
                        .DataSource = dt
                        .DataTextField = "CODE"
                        .DataValueField = "CODE"
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
            End Select
        End If

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        Dim vari As String = Me.hdnThisMapVariant.Value
        'ETYD時はタンク一覧に遷移（それ以外はオーダー一覧）
        If Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.ImportEmptyTank OrElse
            Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.ExportEmptyTank OrElse
            Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.StockTank Then
            vari &= "_ETYD"
        End If

        '自画面MAPIDより親MAP・URLを取得
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = vari
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '次画面の変数セット
        If Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.ImportEmptyTank OrElse
            Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.ExportEmptyTank OrElse
            Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.StockTank Then
            HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return.Replace("_ETYD", "")
        Else
            HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
        End If
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
                Case vLeftTankNo.ID
                    'TANKNO選択時
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
        If Me.txtTankNo.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtTankNo.Text = "" OrElse Convert.ToString(dr("TANKNO")).StartsWith(Me.txtTankNo.Text))
                   ) Then
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
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excel出力", "Excel Download")

        AddLangSetting(dicDisplayText, Me.btnBack, "戻る", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        AddLangSetting(dicDisplayText, Me.lblTankNoLabel, "TANKNo.", "TANKNo.")

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
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        Dim retDt As New DataTable
        Dim sb As New StringBuilder(2048)

        sb.Append("select ")
        sb.Append("    st.ORDERNO ")
        sb.Append("  , st.TANKNO ")
        sb.Append("  , st.RECENT ")
        sb.Append("  , st.ACTIONID ")
        sb.Append("  , st.ACTUALDATE ")
        sb.Append("  , case when ob.DISCHARGEPORT1 = 'JPSDJ' then 'I' else 'E' end as ROOT ")
        sb.Append("  , tk.NEXTINSPECTTYPE ")
        sb.Append("  , tk.NEXTINSPECTDATE ")
        sb.Append("  , '' as LASTIMPORTORDERNO ")
        sb.Append("  , isnull(ov.ISIMPORT,'0') as ISIMPORT ")
        sb.Append("from GBV0001_TANKSTATUS as st ")
        '-- 対象リースタンク
        sb.Append("inner join GBV0002_LEASETANK as lt on lt.TANKNO=st.TANKNO and lt.SHIPPER= @SHIPPER and lt.PRODUCTCODE=@PRODUCTCODE ")
        '-- 輸送/回送判定
        sb.Append("inner join GBT0004_ODR_BASE as ob on ob.ORDERNO= st.ORDERNO and ob.DELFLG <> @DELFLG ")
        '-- 前回輸送判定
        sb.Append("left join ( ")
        sb.Append(" select ")
        sb.Append("  ORDERNO , '1' as ISIMPORT")
        sb.Append(" from GBT0005_ODR_VALUE ")
        sb.Append(" where ACTIONID = 'LOAD' ")
        sb.Append(" and   TANKNO<>'' ")
        sb.Append(" and   DELFLG <> @DELFLG ")
        sb.Append(" group by ORDERNO ")
        sb.Append(") as ov ON ov.ORDERNO=st.ORDERNO ")
        '-- タンクマスタ
        sb.Append("left outer join ( ")
        sb.Append("  select ")
        sb.Append("    TANKNO ")
        sb.Append("  , NEXTINSPECTTYPE ")
        sb.Append("  , NEXTINSPECTDATE ")
        sb.Append("  from GBM0006_TANK with(nolock) ")
        sb.Append("  where COMPCODE = @COMPCODE ")
        sb.Append("  and   STYMD <= @STYMD ")
        sb.Append("  and   ENDYMD >= @ENDYMD ")
        sb.Append("  and   DELFLG <> @DELFLG ")
        sb.Append(") AS tk on tk.TANKNO = st.TANKNO ")
        sb.Append("order by st.TANKNO, st.RECENT ")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sb.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@INITDATE", SqlDbType.Date).Value = "1900/01/01"
                .Add("@STYMD", SqlDbType.Date).Value = Now()
                .Add("@ENDYMD", SqlDbType.Date).Value = Now()
                .Add("@SHIPPER", SqlDbType.NVarChar, 20).Value = "JPC01082"
                .Add("@PRODUCTCODE", SqlDbType.NVarChar, 20).Value = "000662"
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        'Summart/Edit'
        Return SummaryDataTable(retDt)

    End Function

    ''' <summary>
    ''' サマリー一覧編集
    ''' </summary>
    ''' <returns></returns>
    Private Function SummaryDataTable(ByRef dt As DataTable) As DataTable
        Dim unAllocate As String() = {"ETYC", "ETYD", "LESD"}
        Dim actyTitle As String = ""
        Dim outputDate As Date = Now

        Dim selAct As String() = {""}
        Select Case Me.hdnSelectedMode.Value
            Case GBT00030LIST.SelectedMode.ImportEmptyTank
                'ETYD（MY）
                actyTitle = "輸入コンテナ　ETYD"
            Case GBT00030LIST.SelectedMode.ImportBeforeTransport
                'MY側　TKAL～CYIN
                actyTitle = "輸入コンテナ　輸送手配"
            Case GBT00030LIST.SelectedMode.ImportInTransit
                '輸送中（輸入）
                actyTitle = "輸入コンテナ　海上輸送中"
            Case GBT00030LIST.SelectedMode.ExportEmptyTank
                'ETYD（JP）
                actyTitle = "回送コンテナ　ETYD"
            Case GBT00030LIST.SelectedMode.ExportBeforeTransport
                'JP側　(E)TKAL～(E)CYIN
                actyTitle = "回送コンテナ　回送手配"
            Case GBT00030LIST.SelectedMode.ExportInTransit
                actyTitle = "回送コンテナ　海上輸送中"
                '輸送中（回送）
            Case GBT00030LIST.SelectedMode.StockTank
                '仙台予備在庫
                actyTitle = "仙台予備在庫"
            Case Else
        End Select

        '一覧表用データテーブル作成
        Dim retDt = CreateListDataTable()
        Dim lineCnt As Integer = 0

        'タンク一覧作成（タンクステータス履歴）
        Dim tmpDt = dt.AsEnumerable
        If Me.hdnSelectedOrderNo.Value <> "" Then
            '対象ORDERNO限定
            tmpDt = tmpDt.Where(Function(a) a.Item("ORDERNO").ToString = Me.hdnSelectedOrderNo.Value)
        End If

        'タンク毎に処理
        Dim tankDt = tmpDt.GroupBy(Function(a) a.Item("TANKNO").ToString)
        For Each tank In tankDt
            Dim orderNo As String = tank.First.Item("ORDERNO").ToString
            Dim tankNo As String = tank.First.Item("TANKNO").ToString
            Dim recent As String = tank.First.Item("RECENT").ToString
            Dim lastAct As String = tank.First.Item("ACTIONID").ToString
            Dim actDate As String = tank.First.Item("ACTUALDATE").ToString
            Dim inspecType As String = tank.First.Item("NEXTINSPECTTYPE").ToString
            Dim inspecDate As String = tank.First.Item("NEXTINSPECTDATE").ToString
            Dim root As String = tank.First.Item("ROOT").ToString
            Dim isImport As String = tank.First.Item("ISIMPORT").ToString

            '前回輸送オーダー検索有無
            Dim lastOrderSerch As Boolean = False

            If Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.ImportEmptyTank Then
                If unAllocate.Contains(lastAct) AndAlso root = "E" Then
                    lastOrderSerch = True
                Else
                    Continue For
                End If
            ElseIf Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.ExportEmptyTank Then
                If unAllocate.Contains(lastAct) AndAlso root = "I" Then
                    lastOrderSerch = True
                Else
                    Continue For
                End If
            ElseIf Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.StockTank Then
                If lastAct = "STOK" Then
                Else
                    Continue For
                End If
            End If

            lineCnt += 1
            Dim newRow = retDt.NewRow
            newRow("LINECNT") = lineCnt
            newRow("OPERATION") = ""
            newRow("TIMSTP") = 0
            newRow("SELECT") = "1"
            newRow("HIDDEN") = "0"

            newRow("ORDERNO") = orderNo
            newRow("TANKNO") = tankNo
            newRow("NEXTINSPECTTYPE") = inspecType
            newRow("NEXTINSPECTDATE") = FormatDateContrySettings(inspecDate, "yyyy/MM/dd")
            newRow("LASTIMPORTORDERNO") = ""

            'タンク動静履歴
            For Each actCol As DataRow In tank
                Dim act As String = actCol.Item("ACTIONID").ToString

                Select Case act
                    Case "ETYD",
                         "ETYC",
                         "LESD"
                        If lastOrderSerch = False Then
                            Exit For
                        End If
                    Case "TKAL",
                         "DOUT",
                         "CYIN"
                        If lastOrderSerch = False Then
                            newRow(act) = FormatDateContrySettings(actCol("ACTUALDATE").ToString, "yyyy/MM/dd")
                        End If
                    Case "LOAD"
                        If lastOrderSerch = True AndAlso actCol.Item("ORDERNO").ToString <> orderNo Then
                            '前回輸送オーダー番号※回送ではないのでLOADで判定
                            newRow("LASTIMPORTORDERNO") = actCol.Item("ORDERNO").ToString
                            Exit For
                        End If
                        newRow(act) = FormatDateContrySettings(actCol("ACTUALDATE").ToString, "yyyy/MM/dd")
                    Case "STOK"
                        Exit For
                    Case Else
                End Select
            Next

            newRow("ACTYTITLE") = actyTitle
            newRow("OUTPUTDATE") = outputDate

            retDt.Rows.Add(newRow)
        Next

        Return retDt
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
        Dim colList As New List(Of String) From {"ORDERNO", "TANKNO",
                                                 "TKAL", "DOUT", "LOAD", "CYIN",
                                                 "NEXTINSPECTTYPE", "NEXTINSPECTDATE",
                                                 "LASTIMPORTORDERNO",
                                                 "ACTYTITLE", "OUTPUTDATE"}

        For Each colName As String In colList
            retDt.Columns.Add(colName, GetType(String))
            retDt.Columns(colName).DefaultValue = ""
        Next
        Return retDt
    End Function

    ''' <summary>
    ''' タンク番号一覧を取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetTankNo() As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT DISTINCT TANKNO AS CODE")
        sqlStat.AppendLine("  FROM GBM0006_TANK")
        sqlStat.AppendLine(" WHERE COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   And TANKNO      <> ''")
        sqlStat.AppendLine("ORDER BY TANKNO ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar)
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
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
    ''' 右ボックス設定
    ''' </summary>
    Public Function RightboxInit() As String
        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim excelMapId As String = CONST_MAPID

        'RightBOX情報設定
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget
        Dim COA0022ProfXls As New BASEDLL.COA0022ProfXls
        retVal = C_MESSAGENO.NORMAL
        '初期化
        'Me.txtRightErrorMessage.Text = ""

        'レポートID情報
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
            retVal = COA0022ProfXls.ERR
            Return retVal
        End If

        'レポートID変数検索
        COA0016VARIget.MAPID = CONST_MAPID
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
    ''' 前画面より各種情報を引き継ぎ
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00030LIST Then
            Dim prevObj As GBT00030LIST = DirectCast(Page.PreviousPage, GBT00030LIST)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSelectedPort", Me.hdnSelectedPort},
                                                                        {"hdnSelectedMode", Me.hdnSelectedMode},
                                                                        {"hdnSelectedActy", Me.hdnSelectedActy}}

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
        ElseIf TypeOf Page.PreviousPage Is GBT00030ORDERLIST Then
            Dim prevObj As GBT00030ORDERLIST = DirectCast(Page.PreviousPage, GBT00030ORDERLIST)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSelectedPort", Me.hdnSelectedPort},
                                                                        {"hdnSelectedMode", Me.hdnSelectedMode},
                                                                        {"hdnSelectedActy", Me.hdnSelectedActy},
                                                                        {"hdnSelectedOrderNo", Me.hdnSelectedOrderNo}}

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
            'ViewState("ORDERINFO") = prevObj.OrderInfo
        End If

        '画面表示項目設定
        Dim vari As String = Me.hdnThisMapVariant.Value
        Select Case Me.hdnSelectedMode.Value
            Case GBT00030LIST.SelectedMode.ImportEmptyTank
                'ETYD（MY）
                vari &= "_ETYD"
            Case GBT00030LIST.SelectedMode.ImportBeforeTransport
                'MY側　TKAL～CYIN
                vari &= "_IMP"
            Case GBT00030LIST.SelectedMode.ImportInTransit
                '輸送中（輸入）
            Case GBT00030LIST.SelectedMode.ExportEmptyTank
                'ETYD（JP）
                vari &= "_ETYD"
            Case GBT00030LIST.SelectedMode.ExportBeforeTransport
                'JP側　(E)TKAL～(E)CYIN
                vari &= "_EXP"
            Case GBT00030LIST.SelectedMode.ExportInTransit
                '輸送中（回送）
            Case GBT00030LIST.SelectedMode.StockTank
                '仙台予備在庫
            Case Else
        End Select
        Me.hdnThisViewVariant.Value = vari
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
        COA0013TableObject.VARI = Me.hdnThisViewVariant.Value
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

    End Sub
    ''' <summary>
    ''' リスト行ダブルクリック時イベント
    ''' </summary>
    Private Sub ListRowDbClick()
        Return
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

        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        '帳票出力
        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
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
    ''' 当画面の保持必要情報を保持し退避用クラスを生成
    ''' </summary>
    ''' <returns></returns>
    Public Function GetGbt00006items() As GBT00030DITEMS
        Dim item As New GBT00030DITEMS
        item.OrderInfo = DirectCast(ViewState("ORDERINFO"), GBT00030LIST.GBT00030OrderInfo)
        Return item
    End Function
    ''' <summary>
    ''' 退避情報を画面に戻す
    ''' </summary>
    ''' <param name="item"></param>
    Private Sub SetGbt00006items(item As GBT00030DITEMS)
        Me.DisplayItems = item
        ViewState("ORDERINFO") = item.OrderInfo
    End Sub

    ''' <summary>
    ''' 画面情報退避用クラス
    ''' </summary>
    <Serializable>
    Public Class GBT00030DITEMS
        Public Property OrderInfo As GBT00030LIST.GBT00030OrderInfo

    End Class
End Class