Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL

''' <summary>
''' タンク動静管理オーダー一覧画面クラス
''' </summary>
Public Class GBT00030ORDERLIST
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00030O" '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 34                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_VS_FILECNTDATA As String = "VSFILECNT" 'ファイル数保持用ビューステートデータ
    Private Const CONST_VS_ATTA_UNIQUEID As String = "ATTA_UNIQUEID"
    Private Const CONST_VS_PREV_ATTACHMENTINFO As String = "PREV_ATTACHMENTINFO"
    Private Const CONST_VS_CURR_ATTACHMENTINFO As String = "CURR_ATTACHMENTINFO"

    'アップロードファイルルート
    Private Const CONST_DIRNAME_BL_UPROOT As String = "BL" 'ファイルアップロードルート
    ''' <summary>
    ''' 当リストデータ保存用
    ''' </summary>
    Private SavedDt As DataTable = Nothing
    ''' <summary>
    ''' 添付情報保持データテーブル
    ''' </summary>
    Private dtCurAttachment As DataTable

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
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
                        .SRCDATA = dt
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = ""
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
                Me.dtCurAttachment = CollectDispAttachmentInfo()

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
                If Me.hdnListDBclick.Value <> "" AndAlso Me.hdnListCellclick.Value = "" Then
                    ListRowDbClick()
                    Me.hdnListDBclick.Value = ""
                    Return '単票ページにリダイレクトするため念のため処理は終わらせる
                End If
                '**********************
                ' 一覧表のCellクリック判定
                '**********************
                If Me.hdnListDBclick.Value <> "" AndAlso Me.hdnListCellclick.Value <> "" Then
                    ListCellClick()
                    Me.hdnListDBclick.Value = ""
                    Me.hdnListCellclick.Value = ""
                    'Return '単票ページにリダイレクトするため念のため処理は終わらせる
                End If
                '**********************
                ' 添付ファイル内容表示処理
                '**********************
                If Me.hdnFileDisplay.Value IsNot Nothing AndAlso Me.hdnFileDisplay.Value <> "" Then
                    AttachmentFileNameDblClick()
                    hdnFileDisplay.Value = ""
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
            ViewState(CONST_VS_CURR_ATTACHMENTINFO) = Me.dtCurAttachment
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
                'ORDERNO
                Case vLeftOrderNo.ID
                    Dim dt As DataTable = GetOrderNo()
                    With Me.lbOrderNo
                        .DataSource = dt
                        .DataTextField = "CODE"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtOrderNo.Text)
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

        '自画面MAPIDより親MAP・URLを取得
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
                Case vLeftOrderNo.ID
                    'ORDERNO選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbOrderNo.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbOrderNo.SelectedItem.Value
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
        If Me.txtOrderNo.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtOrderNo.Text = "" OrElse Convert.ToString(dr("ORDERNO")).StartsWith(Me.txtOrderNo.Text))
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
        Me.txtOrderNo.Focus()
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

        AddLangSetting(dicDisplayText, Me.lblOrderNoLabel, "ORDERNo.", "ORDERNo.")

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

        sb.Append("select B.* ")
        sb.Append(" ,PORT.AREANAME ")
        sb.Append(" ,case when B.ROOT = 'I' then ACTIONID else 'E'+ACTIONID end as ACTY ")
        sb.Append("from ( ")
        sb.Append("select ")
        sb.Append("  case when ob.DISCHARGEPORT1 = 'JPSDJ' then ob.LOADPORT1 else ob.DISCHARGEPORT1 end as BASEAREA ")
        sb.Append(", case when ob.DISCHARGEPORT1 = 'JPSDJ' then 'I' else 'E' end as ROOT ")
        sb.Append(", ob.ORDERNO ")
        sb.Append(", isnull(ov.TKALNUM, 0) as TKALNUM ")
        sb.Append(", isnull(ov2.TANKNUM, 0) as TANKNUM ")
        sb.Append(", case when isnull(ap.DAMAGED, 0) = 0 then '' else 'Y' end as DAMAGED ")
        sb.Append(", isnull(ST.ACTIONID,'') as ACTIONID ")
        sb.Append(", ST.ACTUALDATE as ACTUALDATE ")
        sb.Append(", ob.VSL1 + ' ' + ob.VOY1 as VSLVOY ")
        sb.Append(", ob.TRANSIT1VSL1 + ' ' + ob.TRANSIT1VOY1 as TSVSLVOY ")
        sb.Append(", ship.SCHEDELDATE as EATD ")
        sb.Append(", ship.ACTUALDATE as ATD ")
        sb.Append(", arvd.SCHEDELDATE as EATA ")
        sb.Append(", arvd.ACTUALDATE as ATA ")
        sb.Append(", trsh.SCHEDELDATE as TSEATD ")
        sb.Append(", trsh.ACTUALDATE as TSATD ")
        sb.Append(", trav.SCHEDELDATE as TSEATA ")
        sb.Append(", trav.ACTUALDATE as TSATA ")
        sb.Append("from GBT0004_ODR_BASE as ob ")
        sb.Append("inner join GBT0002_BR_BASE as br on br.BRID=ob.BRID and br.DELFLG<>@DELFLG and br.USINGLEASETANK='1' ")
        sb.Append("inner join GBM0004_CUSTOMER as c on c.COMPCODE=@COMPCODE and c.CUSTOMERCODE=ob.SHIPPER and c.STYMD<=@STYMD and c.ENDYMD>=@ENDYMD and c.DELFLG<>@DELFLG ")
        sb.Append("inner join COS0017_FIXVALUE as f on f.CLASS='PROJECT' and f.KEYCODE='HIS' and c.TORICOMP=f.VALUE1 and f.STYMD<=@STYMD and f.ENDYMD>=@ENDYMD and f.DELFLG<>@DELFLG ")
        sb.Append("inner join ( ")
        sb.Append(" select ")
        sb.Append("  ORDERNO ")
        sb.Append(", COUNT(TANKSEQ) as TANKNUM ")
        sb.Append(" from GBT0007_ODR_VALUE2 ")
        sb.Append(" where TRILATERAL = 1 ")
        sb.Append(" and   DELFLG <> @DELFLG ")
        sb.Append(" group by ORDERNO ")
        sb.Append(") as ov2 ON ov2.ORDERNO=ob.ORDERNO ")
        sb.Append("left join ( ")
        sb.Append(" select ")
        sb.Append("  ORDERNO ")
        sb.Append(", COUNT(*) as TKALNUM ")
        sb.Append(" from GBT0005_ODR_VALUE ")
        sb.Append(" where ACTIONID = 'TKAL' ")
        sb.Append(" and   TANKNO<>'' ")
        sb.Append(" and   DELFLG <> @DELFLG ")
        sb.Append(" group by ORDERNO ")
        sb.Append(") as ov ON ov.ORDERNO=ob.ORDERNO ")
        sb.Append("left join ( ")
        sb.Append(" select ")
        sb.Append("  ORDERNO ")
        sb.Append(", SUM(case when TANKCONDITION='2' then 1 else 0 end) as DAMAGED ")
        sb.Append(" from GBT0005_ODR_VALUE ")
        sb.Append(" where TANKNO<>'' ")
        sb.Append(" and   DELFLG <> @DELFLG ")
        sb.Append(" group by ORDERNO ")
        sb.Append(") as ap ON ap.ORDERNO=ob.ORDERNO ")
        sb.Append("left outer join ( ")
        sb.Append("    select distinct ")
        sb.Append("      vt.ORDERNO ")
        sb.Append("    , vt.ACTIONID ")
        sb.Append("	   , vt.ACTUALDATE ")
        sb.Append("    from GBV0001_TANKSTATUS as vt ")
        If Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.ExportInTransit Then
            '回送輸送時は仮引当は除外
            sb.Append("    inner join ( ")
            sb.Append("		select s.TANKNO, min(s.RECENT) as RECENT ")
            sb.Append("		from GBV0001_TANKSTATUS as s ")
            sb.Append("		where not (s.ACTIONID='TKAL' and s.ACTUALDATE=@INITDATE) ")
            sb.Append("		group by s.TANKNO ")
            sb.Append("	) as recent on recent.TANKNO=vt.TANKNO and recent.RECENT=vt.RECENT ")
        Else
            sb.Append("    where vt.RECENT=1 ")
        End If
        sb.Append(") as ST ")
        sb.Append("on  ST.ORDERNO = ob.ORDERNO ")
        sb.Append("left join ( ")
        sb.Append(" select ")
        sb.Append("  ORDERNO,max(SCHEDELDATE) as SCHEDELDATE, max(ACTUALDATE) as ACTUALDATE ")
        sb.Append(" from GBT0005_ODR_VALUE ")
        sb.Append(" where TANKNO <> '' ")
        sb.Append(" and  ACTIONID ='SHIP' ")
        sb.Append(" and  DELFLG <> @DELFLG ")
        sb.Append(" group by ORDERNO, ACTIONID ")
        sb.Append(") as ship ON ship.ORDERNO=ob.ORDERNO ")
        sb.Append("left join ( ")
        sb.Append(" select ")
        sb.Append("  ORDERNO, max(SCHEDELDATE) as SCHEDELDATE, max(ACTUALDATE) as ACTUALDATE ")
        sb.Append(" from GBT0005_ODR_VALUE ")
        sb.Append(" where TANKNO <> '' ")
        sb.Append(" and  ACTIONID ='ARVD' ")
        sb.Append(" and  DELFLG <> @DELFLG ")
        sb.Append(" group by ORDERNO, ACTIONID ")
        sb.Append(") as arvd ON arvd.ORDERNO=ob.ORDERNO ")
        sb.Append("left join ( ")
        sb.Append(" select ")
        sb.Append("  ORDERNO,max(SCHEDELDATE) as SCHEDELDATE, max(ACTUALDATE) as ACTUALDATE ")
        sb.Append(" from GBT0005_ODR_VALUE ")
        sb.Append(" where TANKNO <> '' ")
        sb.Append(" and  ACTIONID ='TRSH' ")
        sb.Append(" and  DELFLG <> @DELFLG ")
        sb.Append(" group by ORDERNO, ACTIONID ")
        sb.Append(") as trsh ON trsh.ORDERNO=ob.ORDERNO ")
        sb.Append("left join ( ")
        sb.Append(" select ")
        sb.Append("  ORDERNO, max(SCHEDELDATE) as SCHEDELDATE, max(ACTUALDATE) as ACTUALDATE ")
        sb.Append(" from GBT0005_ODR_VALUE ")
        sb.Append(" where TANKNO <> '' ")
        sb.Append(" and  ACTIONID ='TRAV' ")
        sb.Append(" and  DELFLG <> @DELFLG ")
        sb.Append(" group by ORDERNO, ACTIONID ")
        sb.Append(") as trav ON trav.ORDERNO=ob.ORDERNO ")
        sb.Append("where 1=1 ")
        sb.Append("and ob.STYMD  <= @STYMD and ob.ENDYMD >= @ENDYMD and ob.DELFLG <> @DELFLG ")
        sb.Append("and (ST.ACTIONID is not null or ov.TKALNUM is null) ")
        sb.Append(") as B ")
        sb.Append("left outer join ( ")
        sb.Append("  select ")
        sb.Append("    pm.PORTCODE ")
        sb.Append("  , pm.AREANAME ")
        sb.Append("  from GBM0002_PORT as pm with(nolock) ")
        sb.Append("  where pm.COMPCODE = '01' ")
        sb.Append("  and   pm.STYMD <= @STYMD ")
        sb.Append("  and   pm.ENDYMD >= @ENDYMD ")
        sb.Append("   and pm.DELFLG <> @DELFLG ")
        sb.Append(") AS PORT on PORT.PORTCODE = B.BASEAREA ")
        sb.Append("order by B.BASEAREA, B.ACTUALDATE desc , B.ORDERNO ")

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

            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        Return SummaryDataTable(retDt)
    End Function

    ''' <summary>
    ''' サマリー一覧編集
    ''' </summary>
    ''' <returns></returns>
    Private Function SummaryDataTable(ByRef dt As DataTable) As DataTable
        Dim leaseTankNum As Integer = 0
        Dim leaseOutNum As Integer = 0
        Dim leaseTotal As Integer = 0

        Dim outputDate As Date = Now
        Dim actyTitle As String = GBT00030LIST.SelectedMode.GetModeName(Me.hdnSelectedMode.Value)

        Dim selAct As List(Of String) = New List(Of String)
        Select Case Me.hdnSelectedMode.Value
            Case GBT00030LIST.SelectedMode.ImportEmptyTank
            Case GBT00030LIST.SelectedMode.ImportBeforeTransport
                Select Case Me.hdnSelectedActy.Value
                    Case "TKAL"
                        selAct.AddRange({"TKAL", "DOUT", "LOAD", "CYIN"})
                    Case "DOUT"
                        selAct.AddRange({"DOUT", "LOAD", "CYIN"})
                    Case "LOAD"
                        selAct.AddRange({"LOAD", "CYIN"})
                    Case "CYIN"
                        selAct.AddRange({"CYIN"})
                End Select
            Case GBT00030LIST.SelectedMode.ImportInTransit
                Select Case Me.hdnSelectedActy.Value
                    Case "SHIP"
                        selAct.AddRange({"SHIP", "TRAV", "TRSH", "ARVD", "DPIN", "DLRY"})
                    Case "TRAV"
                        selAct.AddRange({"TRAV", "TRSH", "ARVD", "DPIN", "DLRY"})
                    Case "TRSH"
                        selAct.AddRange({"TRSH", "ARVD", "DPIN", "DLRY"})
                    Case "ARVD"
                        selAct.AddRange({"ARVD", "DPIN", "DLRY"})
                    Case "DPIN"
                        selAct.AddRange({"DPIN", "DLRY"})
                    Case "DLRY"
                        selAct.AddRange({"DLRY"})
                End Select
            Case GBT00030LIST.SelectedMode.ExportEmptyTank
            Case GBT00030LIST.SelectedMode.ExportBeforeTransport
                Select Case Me.hdnSelectedActy.Value
                    Case "ETKAL"
                        selAct.AddRange({"ETKAL", "EDOUT", "ECYIN"})
                    Case "EDOUT"
                        selAct.AddRange({"EDOUT", "ECYIN"})
                    Case "ECYIN"
                        selAct.AddRange({"ECYIN"})
                End Select
            Case GBT00030LIST.SelectedMode.ExportInTransit
                Select Case Me.hdnSelectedActy.Value
                    Case "ESHIP"
                        selAct.AddRange({"ESHIP", "ETRAV", "ETRSH", "EARVD"})
                    Case "ETRAV"
                        selAct.AddRange({"ETRAV", "ETRSH", "EARVD"})
                    Case "ETRSH"
                        selAct.AddRange({"ETRSH", "EARVD"})
                    Case "EARVD"
                        selAct.AddRange({"EARVD"})
                End Select
            Case GBT00030LIST.SelectedMode.StockTank
                selAct.AddRange({"STOK"})
            Case Else

        End Select

        'サマリデータベーステーブル作成
        Dim retDt = CreateListDataTable()
        Dim lineCnt As Integer = 0
        Dim orderList As List(Of String) = New List(Of String)

        Dim newRow As DataRow = retDt.NewRow
        For Each tRow As DataRow In dt.Rows
            '対象モードのオーダーのみ表示
            If selAct.Contains(tRow("ACTY").ToString) = False Then
                Continue For
            End If
            '未完了オーダーのみ表示
            If selAct.Contains(tRow("ACTY").ToString) = False Then
                Continue For
            End If
            '同オーダーは読み飛ばし
            If orderList.Contains(tRow("ORDERNO").ToString) Then
                Continue For
            End If

            lineCnt += 1
            newRow = retDt.NewRow
            newRow("LINECNT") = lineCnt
            newRow("OPERATION") = ""
            newRow("TIMSTP") = 0
            newRow("SELECT") = "1"
            newRow("HIDDEN") = "0"

            Dim totalTank = 0
            Dim tankNum = 0

            newRow("ORDERNO") = tRow("ORDERNO").ToString
            newRow("AREANAME") = tRow("AREANAME").ToString
            newRow("BASEAREA") = tRow("BASEAREA").ToString

            If Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.ImportBeforeTransport OrElse
                Me.hdnSelectedMode.Value = GBT00030LIST.SelectedMode.ExportBeforeTransport Then
                newRow("TANKNUM") = tRow("TKALNUM").ToString & " / " & tRow("TANKNUM").ToString
            Else
                newRow("TANKNUM") = tRow("TANKNUM").ToString
            End If

            newRow("DAMAGED") = tRow("DAMAGED").ToString

            Dim scheduleDate As String
            Dim actualDate As String
            Dim editDate As String

            scheduleDate = FormatDateContrySettings(tRow("EATD").ToString, "yyyy/MM/dd")
            actualDate = FormatDateContrySettings(tRow("ATD").ToString, "yyyy/MM/dd")
            If scheduleDate <> "1900/01/01" AndAlso actualDate = "1900/01/01" Then
                editDate = "( " & scheduleDate & " )"
            ElseIf actualDate <> "1900/01/01" Then
                editDate = actualDate
            Else
                editDate = ""
            End If
            newRow("ATD") = editDate

            scheduleDate = FormatDateContrySettings(tRow("EATA").ToString, "yyyy/MM/dd")
            actualDate = FormatDateContrySettings(tRow("ATA").ToString, "yyyy/MM/dd")
            If scheduleDate <> "1900/01/01" AndAlso actualDate = "1900/01/01" Then
                editDate = "( " & scheduleDate & " )"
            ElseIf actualDate <> "1900/01/01" Then
                editDate = actualDate
            Else
                editDate = ""
            End If
            newRow("ATA") = editDate

            scheduleDate = FormatDateContrySettings(tRow("TSEATA").ToString, "yyyy/MM/dd")
            actualDate = FormatDateContrySettings(tRow("TSATA").ToString, "yyyy/MM/dd")
            If scheduleDate <> "1900/01/01" AndAlso actualDate = "1900/01/01" Then
                editDate = "( " & scheduleDate & " )"
            ElseIf actualDate <> "1900/01/01" Then
                editDate = actualDate
            Else
                editDate = ""
            End If
            newRow("TSATA") = editDate

            scheduleDate = FormatDateContrySettings(tRow("TSEATD").ToString, "yyyy/MM/dd")
            actualDate = FormatDateContrySettings(tRow("TSATD").ToString, "yyyy/MM/dd")
            If scheduleDate <> "1900/01/01" AndAlso actualDate = "1900/01/01" Then
                editDate = "( " & scheduleDate & " )"
            ElseIf actualDate <> "1900/01/01" Then
                editDate = actualDate
            Else
                editDate = ""
            End If
            newRow("TSATD") = editDate

            newRow("VSLVOY") = tRow("VSLVOY").ToString
            newRow("TSVSLVOY") = tRow("TSVSLVOY").ToString

            newRow("HBL") = ""
            newRow("MBL") = ""
            newRow("ATTACHMENT") = ""

            newRow("ACTYTITLE") = actyTitle
            newRow("OUTPUTDATE") = outputDate

            orderList.Add(tRow("ORDERNO").ToString)

            '添付ファイル数取得
            GetAttachmentCnt(newRow)

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
        Dim colList As New List(Of String) From {"AREANAME", "BASEAREA", "ORDERNO", "TANKNUM",
                                                 "VSLVOY", "ATD", "ATA",
                                                 "TSVSLVOY", "TSATA", "TSATD",
                                                 "HBL", "MBL",
                                                 "DAMAGED", "ATTACHMENT",
                                                 "ACTYTITLE", "OUTPUTDATE"}

        For Each colName As String In colList
            retDt.Columns.Add(colName, GetType(String))
            retDt.Columns(colName).DefaultValue = ""
        Next
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

        ElseIf TypeOf Page.PreviousPage Is GBT00030TANKLIST Then
            Dim prevObj As GBT00030TANKLIST = DirectCast(Page.PreviousPage, GBT00030TANKLIST)
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
        Else
        End If

        '画面表示項目設定
        Dim vari As String = Me.hdnThisMapVariant.Value
        Select Case Me.hdnSelectedMode.Value
            Case GBT00030LIST.SelectedMode.ImportEmptyTank
                'ETYD（MY）
            Case GBT00030LIST.SelectedMode.ImportBeforeTransport
                'MY側　TKAL～CYIN
            Case GBT00030LIST.SelectedMode.ImportInTransit
                '輸送中（輸入）
                vari &= "_SHIP"
            Case GBT00030LIST.SelectedMode.ExportEmptyTank
                'ETYD（JP）
            Case GBT00030LIST.SelectedMode.ExportBeforeTransport
                'JP側　(E)TKAL～(E)CYIN
            Case GBT00030LIST.SelectedMode.ExportInTransit
                '輸送中（回送）
                vari &= "_ESHIP"
            Case GBT00030LIST.SelectedMode.StockTank
                '在庫
            Case Else
        End Select
        Me.hdnThisViewVariant.Value = vari

        'Listタイトル
        Me.lblActyTitle.Text = GBT00030LIST.SelectedMode.GetModeName(Me.hdnSelectedMode.Value, COA0019Session.LANGDISP)
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

            '添付ファイル数取得
            GetAttachmentCnt(dt.Rows(i))
        Next

        '現在表示位置取得
        Dim ListPosition As Integer = 0
        If Me.hdnListPosition.Value = "" Then
            ListPosition = 1
        Else
            If Integer.TryParse(Me.hdnListPosition.Value, ListPosition) = False Then
                ListPosition = 1
            End If
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
        COA0013TableObject.SCROLLTYPE = ""
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
        Dim tankNo As String = ""
        Me.hdnSelectedOrderNo.Value = ""

        '一覧表示データ復元
        Dim dt As DataTable = CreateListDataTable()
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

        '選択行特定
        Dim rowIdString As String = Me.hdnListDBclick.Value
        Dim rowId As Integer = 0
        If Integer.TryParse(rowIdString, rowId) = True Then
            rowId = rowId - 1
        Else
            Return
        End If
        Dim selectedRow As DataRow = dt.Rows(rowId)
        Me.hdnSelectedOrderNo.Value = selectedRow("ORDERNO").ToString()

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
    ''' リストCellクリック時イベント
    ''' </summary>
    Private Sub ListCellClick()
        ListRowDbClick()
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
    ''' オーダー一覧取得
    ''' </summary>
    Private Function GetOrderNo() As DataTable
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
                Return dt
            End If
        Else
            dt = Me.SavedDt
        End If
        'そもそも初期検索結果がない場合は絞り込まず終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return dt
        End If

        Dim retDt As DataTable = New DataTable
        retDt.Columns.Add("CODE")
        For Each row As DataRow In dt.Rows
            Dim newRow = retDt.NewRow()
            newRow("CODE") = row("ORDERNO")
            retDt.Rows.Add(newRow)
        Next

        Return retDt
    End Function



#Region "<<添付ファイル関連 >>"
    ''' <summary>
    ''' 添付ファイルポップアップ-ダウンロードボタン押下時
    ''' </summary>
    Public Sub btnDownloadFiles_Click()
        Dim dtAttachment As DataTable = Me.dtCurAttachment
        Dim aTTauniqueId As String = Convert.ToString(ViewState(CONST_VS_ATTA_UNIQUEID)).Replace("\", "_")
        'ダウンロード対象有無
        If dtAttachment Is Nothing OrElse dtAttachment.Rows.Count = 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.FILENOTEXISTS, Me.lblFooterMessage, pageObject:=Me)
        End If
        Dim dlUrl As String = CommonFunctions.GetAttachmentCompressedFileUrl(dtAttachment, aTTauniqueId)
        If dlUrl <> "" Then
            Me.hdnPrintURL.Value = dlUrl
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
        End If
        '終了メッセージ
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDOWNLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
    End Sub

    ''' <summary>
    ''' 一覧の添付(Attachment)フィールドダブルクリック時
    ''' </summary>
    Public Sub ShowAttachmentArea_Click()
        Me.hdnIsLeftBoxOpen.Value = ""
        Me.hdnLeftboxActiveViewId.Value = ""

        '*********************************
        '添付ファイル情報のリセット
        '*********************************
        ViewState.Remove(CONST_VS_PREV_ATTACHMENTINFO)
        ViewState.Remove(CONST_VS_CURR_ATTACHMENTINFO)
        ViewState.Remove(CONST_VS_ATTA_UNIQUEID)
        '*********************************
        'データを復元し選択行のレコード取得
        '*********************************
        Dim dt As DataTable = CreateListDataTable()
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


        Dim rowIdString As String = Me.hdnListCurrentRownum.Value

        Dim targetDr As DataRow = (From item In dt Where Convert.ToString(item("LINECNT")) = rowIdString).FirstOrDefault
        Dim orderNo As String = Convert.ToString(targetDr("ORDERNO"))
        Dim attrUniqueId As String = String.Format("{0}\{1}", orderNo, "1")

        '*********************************
        '添付ファイルユーザー作業領域のクリア
        '*********************************
        CommonFunctions.CleanUserTempDirectory(CONST_MAPID)
        '*********************************
        '保存済みの添付ファイル一覧の取得、画面設定
        '*********************************
        Dim dtAttachment As DataTable = CommonFunctions.GetInitAttachmentFileList(attrUniqueId, CONST_DIRNAME_BL_UPROOT, CONST_MAPID)
        Me.dtCurAttachment = dtAttachment
        ViewState(CONST_VS_PREV_ATTACHMENTINFO) = dtAttachment
        ViewState(CONST_VS_CURR_ATTACHMENTINFO) = CommonFunctions.DeepCopy(dtAttachment)
        ViewState(CONST_VS_ATTA_UNIQUEID) = attrUniqueId
        'リピーターに一覧を設定
        repAttachment.DataSource = dtAttachment
        repAttachment.DataBind()
        '*********************************
        '添付ファイルポップアップの表示
        '*********************************
        'ヘッダー部分にTANKNOを転送
        Me.lblAttachTankNoTitle.Text = "OrderNo"
        Me.lblAttachTankNo.Text = orderNo
        '表示スタイル設定
        Me.divAttachmentInputAreaWapper.Style.Remove("display")
        Me.divAttachmentInputAreaWapper.Style.Add("display", "block")
    End Sub

    ''' <summary>
    ''' 添付ファイル欄の添付ファイル名ダブルクリック時処理
    ''' </summary>
    Private Sub AttachmentFileNameDblClick()
        Dim fileName As String = Me.hdnFileDisplay.Value
        If fileName = "" Then
            Return
        End If
        Dim dtAttachment As DataTable = Me.dtCurAttachment
        Dim dlUrl As String = CommonFunctions.GetAttachfileDownloadUrl(dtAttachment, fileName)
        Me.hdnPrintURL.Value = dlUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
    End Sub

    ''' <summary>
    ''' 添付ファイルボックスのキャンセルボタン押下時イベント
    ''' </summary>
    Public Sub btnAttachmentUploadCancel_Click()

        'マルチライン入力ボックスの非表示
        Me.divAttachmentInputAreaWapper.Style("display") = "none"

    End Sub

    ''' <summary>
    ''' アップロード済ファイル数を取得
    ''' </summary>
    Private Sub GetAttachmentCnt(dr As DataRow)
        '一旦添付ファイル情報フィールドをクリア
        dr("ATTACHMENT") = ""
        'コピー元のディレクトリ取得
        Dim orderNo As String = Convert.ToString(dr("ORDERNO")).Replace("/", "")

        '対象のファイル有無取得
        Dim upBaseDir As String = COA0019Session.UPLOADFILESDir
        Dim uploadPath As String = IO.Path.Combine(upBaseDir, CONST_DIRNAME_BL_UPROOT, orderNo, "1")
        'フォルダ自体未存在
        If IO.Directory.Exists(uploadPath) = False Then
            Return
        End If
        '対象ディレクトリのファイル情報取得
        Dim filesObj = IO.Directory.GetFiles(uploadPath)
        If filesObj Is Nothing OrElse filesObj.Count = 0 Then
            Return
        End If
        'ここまで来た場合はファイル存在あり
        'dr("ATTACHMENT") = String.Format("{0} File", filesObj.Count)
        dr("HBL") = String.Format("{0} File", filesObj.Count)
    End Sub

    ''' <summary>
    ''' 画面入力情報を取得しデータセットに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function CollectDispAttachmentInfo() As DataTable
        Dim dt As DataTable = DirectCast(ViewState(CONST_VS_CURR_ATTACHMENTINFO), DataTable)
        If dt Is Nothing Then
            Return Nothing
        End If
        '添付ファイルの収集
        Dim dtAttachment As DataTable = CommonFunctions.DeepCopy(dt)
        For Each repItem As RepeaterItem In Me.repAttachment.Items
            Dim fileName As Label = DirectCast(repItem.FindControl("lblFileName"), Label)
            If fileName Is Nothing Then
                Continue For
            End If
            Dim qAttachment = From attachmentItem In dtAttachment Where attachmentItem("FILENAME").Equals(fileName.Text)
            If qAttachment.Any Then
                'qAttachment.FirstOrDefault.Item("ISMODIFIED") = CONST_FLAG_YES
            End If
        Next

        Return dtAttachment
    End Function

#End Region

End Class