Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' ノンブレーカー一覧画面（親のみ（費用を含まない））
''' </summary>
Public Class GBT00007RESULT
    Inherits System.Web.UI.Page
    Private Const CONST_MAPID As String = "GBT00007" '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    ''' <summary>
    ''' チェックボックスの動きがある為、ポストバック時に復元・保存したDataTable
    ''' </summary>
    Private SavedDt As DataTable
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' メッセージ取得(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0004LableMessage As COA0004LableMessage
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            COA0003LogFile = New COA0003LogFile              'ログ出力
            COA0004LableMessage = New COA0004LableMessage    'メッセージ取得
            '****************************************
            '初回ロード時・ポストバック両方で必要な処理
            '****************************************
            ''DO SOMETHING!
            ''URL直打ちorセッション変数死活
            Dim userId As String = Convert.ToString(Session("Userid"))
            If userId = "" Then
                Server.Transfer(C_LOGIN_URL)
                Return
            End If
            'オンラインサービス判定
            Dim COA0005TermInfo As New BASEDLL.COA0005TermInfo With {.TERMid = Convert.ToString(HttpContext.Current.Session("APSRVname"))}
            COA0005TermInfo.COA0005GetTermInfo()
            If COA0005TermInfo.ERR = C_MESSAGENO.NORMAL Then
            Else
                'BASEDLL処理異常 画面にメッセージを表示
                COA0004LableMessage.MESSAGENO = COA0005TermInfo.ERR
                COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
                COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
                COA0004LableMessage.COA0004getMessage()
                Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
                Return 'ここでReturnするかしないかは画面で判断してください。後続処理が走らないため画面がそのままそのままになります。
            End If

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                'セッション変数のMAPVariant退避
                Me.hdnMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
                '****************************************
                'ヘッダー日付設定
                '****************************************
                Dim headerDateFormat As String = C_HEADER_DATE_FORMAT.JA
                If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                    headerDateFormat = C_HEADER_DATE_FORMAT.EN
                End If
                Me.lblTitleDate.Text = Date.Now.ToString(headerDateFormat)
                '****************************************
                'ID設定
                '****************************************
                Me.lblTitleId.Text = CONST_MAPID
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
                    COA0004LableMessage.MESSAGENO = C_MESSAGENO.SYSTEMADM
                    COA0004LableMessage.PARA01 = String.Format("CODE:{0}", COA0031ProfMap.ERR)
                    COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
                    COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
                    COA0004LableMessage.COA0004getMessage()
                    Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
                    Return
                End If
                '****************************************
                'ヘッダー会社設定
                '****************************************
                With Nothing 'スコープを限定
                    Dim COA0007getCompanyInfo As New COA0007CompanyInfo
                    COA0007getCompanyInfo.COMPCODE = HttpContext.Current.Session("APSRVCamp").ToString
                    COA0007getCompanyInfo.STYMD = Date.Now
                    COA0007getCompanyInfo.ENDYMD = Date.Now
                    COA0007getCompanyInfo.COA0007getCompanyInfo()
                    If COA0007getCompanyInfo.ERR = C_MESSAGENO.NORMAL Then
                        If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                            lblTitleOffice.Text = COA0007getCompanyInfo.NAMES_EN
                        Else
                            lblTitleOffice.Text = COA0007getCompanyInfo.NAMES
                        End If
                    Else
                        COA0004LableMessage.MESSAGENO = COA0007getCompanyInfo.ERR
                        COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
                        COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
                        COA0004LableMessage.COA0004getMessage()
                        Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
                        Return
                    End If
                End With
                ''****************************************
                ''表示条件ラジオボタンの設定
                ''****************************************
                'SetListViewTypeListItem()
                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Using dt As DataTable = Me.GetNonBreakerListDataTable()
                    'グリッド用データをファイルに退避
                    With Nothing
                        Dim COA0021ListTable As New COA0021ListTable
                        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
                        COA0021ListTable.TBLDATA = dt
                        COA0021ListTable.COA0021saveListTable()
                        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                            COA0004LableMessage.MESSAGENO = COA0021ListTable.ERR
                            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
                            COA0004LableMessage.PARA01 = String.Format("CODE:{0}", COA0021ListTable.ERR)
                            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
                            COA0004LableMessage.COA0004getMessage()
                            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
                            Return
                        End If
                    End With

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    Using WW_TBLview As DataView = New DataView(dt)
                        WW_TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & (1 + CONST_DSPROWCOUNT)
                        Dim listData As DataTable = WW_TBLview.ToTable
                        Dim COA0013TableObject As New COA0013TableObject With {
                                .MAPID = CONST_MAPID,
                                .VARI = Me.hdnMapVariant.Value,
                                .SRCDATA = listData,
                                .TBLOBJ = WF_LISTAREA,
                                .SCROLLTYPE = "2",
                                .LEVENT = "ondblclick",
                                .LFUNC = "ListDbClick",
                                .TITLEOPT = True
                        }
                        COA0013TableObject.COA0013SetTableObject()

                        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
                            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
                            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
                        Else
                            ViewState("DISPLAY_LINECNT_LIST") = Nothing
                        End If

                    End Using 'DataView


                End Using 'DataTable
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
                '画面の入力情報を保持
                Dim messageNo As String = FileSaveDisplayInput()
                If messageNo <> C_MESSAGENO.NORMAL Then
                    COA0004LableMessage.MESSAGENO = messageNo
                    COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
                    COA0004LableMessage.PARA01 = String.Format("CODE:{0}", messageNo)
                    COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
                    COA0004LableMessage.COA0004getMessage()
                    Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
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
                ' 申請理由入力ボックス表示
                '**********************
                If Me.hdnRemarkboxField.Value <> "" Then
                    DisplayApplyReason(True)
                    Me.divRemarkInputBoxWrapper.Style("display") = "block"
                End If
                '**********************
                ' Help表示
                '**********************
                If Me.hdnHelpChange.Value IsNot Nothing AndAlso Me.hdnHelpChange.Value <> "" Then
                    DivShowHelp_DoubleClick()
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
            COA0004LableMessage.MESSAGENO = messageNo 'ここは適宜変えてください
            COA0004LableMessage.PARA01 = String.Format("CODE:{0}", messageNo)
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            If COA0004LableMessage.ERR = C_MESSAGENO.NORMAL Then
                Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            End If

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
    ''' 申請ボタン押下時処理
    ''' </summary>
    Public Sub btnApply_Click()
        Me.lblFooterMessage.Text = "動作しません。まだテーブル・フィールド検討段階です。"
        Return
    End Sub
    ''' <summary>
    ''' ノンブレーカー作成ボタン押下時
    ''' </summary>
    Public Sub btnCreateNonBr_Click()
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '画面遷移先URL取得
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = Me.hdnMapVariant.Value
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            COA0004LableMessage.MESSAGENO = COA0012DoUrl.ERR
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            Return
        End If

        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0004LableMessage As New BASEDLL.COA0004LableMessage    'メッセージ取得
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Me.hdnMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
            'WF_TITLETEXT.Text = COA0011ReturnUrl.NAMES
        Else

            COA0004LableMessage.MESSAGENO = COA0011ReturnUrl.ERR
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            Exit Sub
        End If
        '次画面の変数セット
        HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
        HttpContext.Current.Session("MAPurl") = COA0011ReturnUrl.URL
        '画面遷移実行()
        Server.Transfer(COA0011ReturnUrl.URL)
    End Sub
    '''' <summary>
    '''' 絞り込みボタン押下時処理
    '''' </summary>
    'Public Sub btnExtract_Click()
    '    Dim dt As DataTable = CreateDataTable()
    '    Dim COA0021ListTable As New BASEDLL.COA0021ListTable
    '    Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
    '    '一覧表示データ復元 
    '    COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
    '    COA0021ListTable.TBLDATA = dt
    '    COA0021ListTable.COA0021recoverListTable()
    '    If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
    '        dt = COA0021ListTable.OUTTBL
    '    Else
    '        COA0004LableMessage.MESSAGENO = COA0021ListTable.ERR
    '        COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
    '        COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
    '        COA0004LableMessage.COA0004getMessage()
    '        Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
    '        Return
    '    End If
    '    'そもそも初期検索結果がない場合は絞り込まず終了
    '    If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
    '        Return
    '    End If

    '    'フィルタでの絞り込みを利用するか確認
    '    Dim isFillterOff As Boolean = True
    '    If Me.txtShipper.Text.Trim <> "" OrElse Me.txtConsignee.Text.Trim <> "" OrElse Me.rblListViewType.SelectedValue <> "ALL" Then
    '        isFillterOff = False
    '    End If

    '    For Each dr As DataRow In dt.Rows
    '        dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
    '        'フィルタ使用時の場合
    '        If isFillterOff = False Then
    '            '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
    '            If Not ((Me.txtShipper.Text.Trim = "" OrElse Convert.ToString(dr("SHIPPER")).Contains(Me.txtShipper.Text.Trim)) _
    '              AndAlso (Me.txtConsignee.Text.Trim = "" OrElse Convert.ToString(dr("CONSIGNEE")).Contains(Me.txtConsignee.Text.Trim)) _
    '              AndAlso (Me.rblListViewType.SelectedValue = "ALL" OrElse Me.rblListViewType.SelectedValue = "BRONLY" AndAlso Convert.ToString(dr("BRODFLG")) = "1")) Then
    '                dr.Item("HIDDEN") = 1
    '            End If
    '        End If
    '    Next
    '    '画面先頭を表示
    '    hdnListPosition.Value = "1"

    '    '一覧表示データ保存
    '    COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
    '    COA0021ListTable.TBLDATA = dt
    '    COA0021ListTable.COA0021saveListTable()
    '    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
    '        COA0004LableMessage.MESSAGENO = COA0021ListTable.ERR
    '        COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
    '        COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
    '        COA0004LableMessage.COA0004getMessage()
    '        Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
    '    Else
    '        'メッセージ表示
    '        COA0004LableMessage.MESSAGENO = "00007"
    '        COA0004LableMessage.NAEIW = C_NAEIW.NORMAL
    '        COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
    '        COA0004LableMessage.COA0004getMessage()
    '        Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
    '    End If

    '    'カーソル設定
    '    Me.txtShipper.Focus()

    'End Sub
    ''' <summary>
    ''' Excelダウンロードボタン押下時処理
    ''' </summary>
    Public Sub btnExcelDownload_Click()
        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        'Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        '一覧表示データ復元 
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            COA0004LableMessage.MESSAGENO = COA0021ListTable.ERR
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            Return
        End If
        'そもそも初期検索結果がない場合は絞り込まず終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If
        '帳票出力
        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
            Dim reportId As String = Me.lbRightList.SelectedItem.Value
            Dim reportMapId As String = CONST_MAPID
            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
            COA0027ReportTable.COA0027ReportTable()

            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
            Else
                COA0004LableMessage.MESSAGENO = COA0027ReportTable.ERR
                COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
                COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
                COA0004LableMessage.COA0004getMessage()
                Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
                Return
            End If

            '別画面でExcelを表示
            hdnPrintURL.Value = COA0027ReportTable.URL
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

        End With
    End Sub
    ''' <summary>
    ''' オーダー基本情報よりノンブレーカー情報を取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>TODO：BRTYPEでの条件付与およびテーブルに該当カラム追加</remarks>
    Private Function GetNonBreakerListDataTable() As DataTable
        Dim mapId As String = CONST_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0004LableMessage As New BASEDLL.COA0004LableMessage    'メッセージ取得
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得
        'ソート順取得
        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = Me.hdnMapVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim retDt As DataTable = CreateDataTable()
        Dim sqlStat As New StringBuilder
        Dim sqlNoOfCost As New StringBuilder
        Dim sqlDateCondition As New StringBuilder
        sqlNoOfCost.AppendLine("SELECT COUNT(VL.DATAID) AS NOOFCOST")
        sqlNoOfCost.AppendLine("  FROM GBT0005_ODR_VALUE VL")
        sqlNoOfCost.AppendLine(" WHERE VL.ORDERNO = BS.ORDERNO")
        sqlNoOfCost.AppendLine("   AND VL.DELFLG <> @DELFLG")
        Dim dateField As String = "SCHEDELDATE"
        If Me.hdnSearchType.Value = "02FIX" Then
            dateField = "ACTUAKDATE"
        End If

        sqlDateCondition.AppendLine("SELECT 1")
        sqlDateCondition.AppendLine("  FROM GBT0005_ODR_VALUE VL")
        sqlDateCondition.AppendLine(" WHERE VL.ORDERNO = BS.ORDERNO")
        sqlDateCondition.AppendLine("   AND VL.DELFLG <> @DELFLG")
        If Me.hdnDateTermStYMD.Value <> "" And Me.hdnDateTermEndYMD.Value = "" Then
            sqlDateCondition.AppendFormat("   AND VL.{0} BETWEEN @DATETERMST AND @DATETERMEND", dateField).AppendLine()
        End If

        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
        sqlStat.AppendLine("FROM (")
        'JOINした場合ソート機能が活きないため、取得データをサブクエリー化
        sqlStat.AppendLine("SELECT '' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(BS.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1'               AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0'               AS HIDDEN ")
        sqlStat.AppendLine("      ,''                AS ACTION ")
        sqlStat.AppendLine("      ,BS.ORDERNO        AS NONBRID")
        sqlStat.AppendLine("      ,BS.AGENTORGANIZER AS OFFICE")
        sqlStat.AppendFormat("      ,({0})           AS NOOFCOST", sqlNoOfCost.ToString).AppendLine()
        sqlStat.AppendLine("      ,'1' AS CANDELETEORDER") '一旦一律削除可能
        sqlStat.AppendLine("  FROM  GBT0004_ODR_BASE BS")
        sqlStat.AppendLine(" WHERE BS.BRID = ''") '一旦ブレーカIDがブランクのノンブレとしておく
        sqlStat.AppendLine("   AND BS.ORDERNO LIKE 'NB%'") '一旦オーダーIDの先頭ば"BR"をノンブレとしておく
        sqlStat.AppendLine("   AND BS.DELFLG <> @DELFLG")

        sqlStat.AppendFormat("   AND EXISTS({0})", sqlDateCondition.ToString).AppendLine()

        sqlStat.AppendLine(") TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        'DB接続
        Using sqlCon As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon"))),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = "1"
                .Add("@ENABLED", SqlDbType.NVarChar, 1).Value = "1"
                If Me.hdnDateTermStYMD.Value <> "" Then
                    .Add("@DATETERMST", SqlDbType.Date).Value = Date.Parse(Me.hdnDateTermStYMD.Value)
                    .Add("@DATETERMEND", SqlDbType.Date).Value = Date.Parse(Me.hdnDateTermEndYMD.Value)
                End If

            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                retDt.Load(sqlDr)
            End Using
            'Using sqlDa As New SqlDataAdapter(sqlCmd)
            '    sqlDa.Fill(retDt)
            'End Using
        End Using

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
    ''' 備考入力ボックスのOKボタン押下時イベント
    ''' </summary>
    Public Sub btnRemarkInputOk_Click()
        DisplayApplyReason(False)

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
        hdnListPosition.Value = 1.ToString

    End Sub
    ''' <summary>
    ''' 最終頁ボタン押下時
    ''' </summary>
    Public Sub btnLAST_Click()
        COA0004LableMessage = New BASEDLL.COA0004LableMessage       'メッセージ取得
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
            COA0004LableMessage.MESSAGENO = COA0021ListTable.ERR
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            Return
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
    ''' 一覧表★ボタン押下時処理
    ''' </summary>
    Public Sub btnListAction_Click()
        Dim currentRownum As String = Me.hdnListCurrentRownum.Value
        Dim rowId As Integer = 0
        If Integer.TryParse(currentRownum, rowId) = True Then
            rowId = rowId - 1
        Else
            Return
        End If

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
            COA0004LableMessage.MESSAGENO = COA0021ListTable.ERR
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            Return
        End If
        'この段階でありえないが初期検索結果がない場合は終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If
        '引き渡す情報を当画面のHidden項目に格納
        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim brId As String = Convert.ToString(selectedRow.Item("BRID"))
        Dim odId As String = Convert.ToString(selectedRow.Item("ODID"))

        Me.hdnSelectedOdId.Value = odId
        'JavaScriptにて別タブ表示を実行するフラグを立てる
    End Sub
    ''' <summary>
    ''' 一覧表DELETEボタン押下時処理
    ''' </summary>
    Public Sub btnListDelete_Click()
        Dim currentRownum As String = Me.hdnListCurrentRownum.Value
        Dim rowId As Integer = 0
        If Integer.TryParse(currentRownum, rowId) = True Then
            rowId = rowId - 1
        Else
            Return
        End If

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
            COA0004LableMessage.MESSAGENO = COA0021ListTable.ERR
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            Return
        End If
        'この段階でありえないが初期検索結果がない場合は終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If
        '引き渡す情報を当画面のHidden項目に格納
        Dim selectedRow As DataRow = dt.Rows(rowId)
        'SQL接続生成
        Using sqlCon As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon")))
            sqlCon.Open()
            '論理削除可能かチェック
            If CheckCanDelete(selectedRow, sqlCon) = False Then
                COA0004LableMessage.MESSAGENO = C_MESSAGENO.CANNOTUPDATE
                COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
                COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
                COA0004LableMessage.COA0004getMessage()
                Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
                Return
            End If
            '各オーダーテーブルの論理削除
            Dim orderNo As String = Convert.ToString(selectedRow.Item("ODID"))
            DeleteOrderBaseValue(orderNo, sqlCon)
        End Using
        'ここまで来たら論理削除正常終了のため自身を再読み込みし一覧を再取得
        Server.Transfer(Request.Url.LocalPath)
    End Sub
    ''' <summary>
    ''' 「?」ボタンダブルクリック時イベント
    ''' </summary>
    Protected Sub DivShowHelp_DoubleClick()
        Try

            Session("Class") = "WF_HELPDisplay"
            '■■■ 画面遷移実行 ■■■
            'TODO COA0019Sessionに差し替えるかも
            HttpContext.Current.Session("HELPid") = CONST_MAPID
            Me.hdnCanHelpOpen.Value = "1"
        Catch ex As Exception
            Dim messageNo As String = "89001" 'ここは適宜変えてください
            Dim NORMAL As String = ""
            COA0004LableMessage.MESSAGENO = "00003" 'ここは適宜変えてください
            COA0004LableMessage.PARA01 = String.Format("CODE:{0}", messageNo)
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            If COA0004LableMessage.ERR = C_MESSAGENO.NORMAL Then
                Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = messageNo
            COA0003LogFile.COA0003WriteLog()

            Return
        End Try
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
        'AddLangSetting(dicDisplayText, Me.btnExtract, "絞り込み", "Extract")
        AddLangSetting(dicDisplayText, Me.btnApply, "申請", "APPLY")
        AddLangSetting(dicDisplayText, Me.btnCreateNonBr, "ノンブレーカー作成", "Create")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        'AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "EXCEL DOWNLOAD")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Selection")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        'AddLangSetting(dicDisplayText, Me.lblShipperLabel, "荷主", "SHIPPER")
        'AddLangSetting(dicDisplayText, Me.lblConsigneeLabel, "荷受人", "CONSIGNEE")

        AddLangSetting(dicDisplayText, Me.hdnConfirmTitle, "削除しますよろしいですか？", "Are you sure you want to delete?")
        AddLangSetting(dicDisplayText, Me.lblConfirmOrderNoName, "NON BR ID", "NON BR ID")
        '上記で設定したオブジェクトの文言を変更
        For Each displayTextItem In dicDisplayText
            '足りないかもしれないので適宜追加
            Dim bufItem As Control = displayTextItem.Key
            If TypeOf bufItem Is Label Then
                'ラベルの場合
                Dim bufLabel As Label = DirectCast(bufItem, Label)
                bufLabel.Text = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is Button Then
                'ボタンの場合
                Dim bufButton As Button = DirectCast(bufItem, Button)
                bufButton.Text = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HiddenField Then
                '隠しフィールドの場合
                Dim bufHdf As HiddenField = DirectCast(bufItem, HiddenField)
                bufHdf.Value = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is RadioButton Then
                'ラジオボタン文言
                Dim bufRadio As RadioButton = DirectCast(bufItem, RadioButton)
                bufRadio.Text = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HtmlInputButton Then
                'Input[Type=button]
                Dim bufhtmlInputButton As HtmlInputButton = DirectCast(bufItem, HtmlInputButton)
                bufhtmlInputButton.Value = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HtmlInputHidden Then
                'Input[Type=Hidden]
                Dim bufhtmlInputHidden As HtmlInputHidden = DirectCast(bufItem, HtmlInputHidden)
                bufhtmlInputHidden.Value = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HtmlTableCell Then
                'テーブルセル<td>は<td></td>のすべての文字を設定
                Dim bufhtmlTableCell As HtmlTableCell = DirectCast(bufItem, HtmlTableCell)
                bufhtmlTableCell.InnerHtml = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HtmlControl Then
                'ここは今のところ不明オブジェクトなので何もしない
                Dim bufhtmlCont = DirectCast(bufItem, HtmlControl)
            End If
        Next
        '****************************************
        ' グリッドヘッダーの表示文言設定(GrivViewだけは個別制御が必要)
        '****************************************
        '適宜追加を
    End Sub

    ''' <summary>
    ''' LangSetting関数で利用する文言設定ディクショナリ作成関数
    ''' </summary>
    ''' <param name="dicDisplayText">対象ディクショナリオブジェクト</param>
    ''' <param name="obj">オブジェクト</param>
    ''' <param name="jaText">日本語文言</param>
    ''' <param name="enText">英語文言</param>
    Private Sub AddLangSetting(ByRef dicDisplayText As Dictionary(Of Control, Dictionary(Of String, String)),
                               ByVal obj As Control, ByVal jaText As String, enText As String)
        dicDisplayText.Add(obj,
                           New Dictionary(Of String, String) _
                           From {{C_LANG.JA, jaText}, {C_LANG.EN, enText}})
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
        retDt.Columns.Add("ACTION", GetType(String))
        retDt.Columns.Add("NONBRID", GetType(String))
        retDt.Columns.Add("OFFICE", GetType(String))

        retDt.Columns.Add("STATUS", GetType(String))
        retDt.Columns.Add("APPLY", GetType(String))
        retDt.Columns.Add("APPLYTEXT", GetType(String))

        retDt.Columns.Add("NOOFCOST", GetType(String))

        retDt.Columns.Add("CANDELETEORDER", GetType(String))

        For Each col As DataColumn In retDt.Columns
            If col.DataType Is GetType(String) AndAlso col.DefaultValue Is DBNull.Value Then
                col.DefaultValue = ""
            End If
        Next

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
            COA0004LableMessage.MESSAGENO = C_MESSAGENO.SYSTEMADM
            COA0004LableMessage.PARA01 = "CODE:" & COA0021ListTable.ERR & ""
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            Return
        End If
        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim odId As String = Convert.ToString(selectedRow.Item("NONBRID"))
        Dim mapIdp As String = CONST_MAPID
        Dim varP As String = Me.hdnMapVariant.Value

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = mapIdp
        COA0012DoUrl.VARIP = varP
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            COA0004LableMessage.MESSAGENO = COA0012DoUrl.ERR
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
            Exit Sub
        End If
        Session("MAPmapid") = mapIdp
        Session("MAPvariant") = varP
        Me.hdnSelectedOdId.Value = odId
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' 一覧 マウスホイール時処理 (一覧スクロール)
    ''' </summary>
    Protected Sub ListScrole()
        'If hdnMouseWheel.Value = "" Then
        '    Return
        'End If
        COA0004LableMessage = New COA0004LableMessage                   'メッセージ取得
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
            COA0004LableMessage.MESSAGENO = COA0021ListTable.ERR
            COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
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
        Using TBLview As DataView = New DataView(dt)
            TBLview.Sort = "LINECNT"
            TBLview.RowFilter = "HIDDEN= '0' and SELECT >= " & (ListPosition).ToString & " and SELECT <= " & (ListPosition + CONST_DSPROWCOUNT).ToString
            Dim listData As DataTable = TBLview.ToTable

            '一覧作成
            Dim COA0013TableObject As New BASEDLL.COA0013TableObject With {
                    .MAPID = CONST_MAPID,
                    .VARI = Me.hdnMapVariant.Value,
                    .SRCDATA = listData,
                    .TBLOBJ = Me.WF_LISTAREA,
                    .SCROLLTYPE = "2",
                    .LEVENT = "ondblclick",
                    .LFUNC = "ListDbClick",
                    .TITLEOPT = True
                }

            COA0013TableObject.COA0013SetTableObject()

            If TBLview.Count = 0 Then
                hdnListPosition.Value = "1"
            Else
                hdnListPosition.Value = Convert.ToString(TBLview.Item(0)("SELECT"))
            End If

            '1.現在表示しているLINECNTのリストをビューステートに保持
            '2.APPLYチェックがついているチェックボックスオブジェクトをチェック状態にする
            If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
                Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                          Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
                ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
                Dim targetCheckBoxLineCnt = (From dr As DataRow In listData
                                             Where Convert.ToString(dr.Item("APPLY")) <> ""
                                             Select Convert.ToInt32(dr.Item("LINECNT")))
                For Each lineCnt As Integer In targetCheckBoxLineCnt
                    Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & "APPLY" & lineCnt.ToString
                    Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                    If tmpObj IsNot Nothing Then
                        Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        chkObj.Checked = True
                    End If
                Next
            Else
                ViewState("DISPLAY_LINECNT_LIST") = Nothing
            End If

            hdnMouseWheel.Value = ""

        End Using

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
                .Add("@DELFLG", SqlDbType.NVarChar).Value = "1"
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
    ''' オーダーの基本情報、および明細情報をオーダーNoより論理削除する
    ''' <param name="orderNo">論理削除対象のオーダーNo</param>
    ''' <param name="sqlCon">SQLServer接続</param>
    ''' </summary>
    Private Sub DeleteOrderBaseValue(orderNo As String, sqlCon As SqlConnection)
        '2テーブルの論理削除時間をそろえるため実施前に変数に格納
        Dim deleteTime As Date = Date.Now
        'SQL生成
        Dim sqlStatBase As New StringBuilder
        sqlStatBase.AppendLine("UPDATE GBT0004_ODR_BASE ")
        sqlStatBase.AppendLine("   SET DELFLG    = @DELFLG")
        sqlStatBase.AppendLine("      ,UPDYMD    = @UPDYMD")
        sqlStatBase.AppendLine("      ,UPDUSER   = @UPDUSER")
        sqlStatBase.AppendLine("      ,UPDTERMID = @UPDTERMID")
        sqlStatBase.AppendLine("  WHERE ORDERNO  = @ORDERNO")
        sqlStatBase.AppendLine("    AND DELFLG  <> @DELFLG")
        Dim sqlStatValue As New StringBuilder
        sqlStatValue.AppendLine("UPDATE GBT0005_ODR_VALUE ")
        sqlStatValue.AppendLine("   SET DELFLG    = @DELFLG")
        sqlStatValue.AppendLine("      ,UPDYMD    = @UPDYMD")
        sqlStatValue.AppendLine("      ,UPDUSER   = @UPDUSER")
        sqlStatValue.AppendLine("      ,UPDTERMID = @UPDTERMID")
        sqlStatValue.AppendLine("  WHERE ORDERNO  = @ORDERNO")
        sqlStatValue.AppendLine("    AND DELFLG  <> @DELFLG")
        'SQLコマンド実行
        Using sqlCmd As New SqlCommand() With {.Connection = sqlCon}
            'パラメータ設定
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = "1"
                .Add("@UPDYMD", SqlDbType.DateTime).Value = deleteTime
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
            End With
            '2テーブル更新のためトランザクション利用
            Using tran = sqlCon.BeginTransaction
                sqlCmd.Transaction = tran
                '基本情報の論理削除
                sqlCmd.CommandText = sqlStatBase.ToString
                sqlCmd.ExecuteNonQuery()
                '明細情報の論理削除
                sqlCmd.CommandText = sqlStatValue.ToString
                sqlCmd.ExecuteNonQuery()
                tran.Commit()
            End Using
        End Using

    End Sub

    ''' <summary>
    ''' 左の出力帳票
    ''' </summary>
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
        COA0016VARIget.COMPCODE = "Default"
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
        If TypeOf Page.PreviousPage Is GBT00007SELECT Then
            '検索画面の場合
            Dim prevObj As GBT00007SELECT = DirectCast(Page.PreviousPage, GBT00007SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtDateTermStYMD", Me.hdnDateTermStYMD},
                                                                        {"hdnDateTermEndYMD", Me.hdnDateTermEndYMD},
                                                                        {"txtOffice", Me.hdnOffice},
                                                                        {"rblSearchType", Me.hdnSearchType}}

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
        ElseIf TypeOf Page.PreviousPage Is GBT00004ORDER Then
            'オーダー入力画面からの遷移
            Dim prevObj As GBT00004ORDER = DirectCast(Page.PreviousPage, GBT00004ORDER)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnDateTermStYMD", Me.hdnDateTermStYMD},
                                                                        {"hdnDateTermEndYMD", Me.hdnDateTermEndYMD},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSearchType", Me.hdnSearchType}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next
        ElseIf TypeOf Page.PreviousPage Is GBT00007RESULT Then
            '自身から遷移（削除時のリフレッシュのみの想定、それ以外の用途を追加する場合は注意）
            Dim prevObj As GBT00007RESULT = DirectCast(Page.PreviousPage, GBT00007RESULT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnDateTermStYMD", Me.hdnDateTermStYMD},
                                                                        {"hdnDateTermEndYMD", Me.hdnDateTermEndYMD},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSearchType", Me.hdnSearchType}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            '正常メッセージをメッセージエリアに表示（削除正常時のみ自身をリフレッシュするため）
            COA0004LableMessage.MESSAGENO = C_MESSAGENO.NORMALDBENTRY
            COA0004LableMessage.NAEIW = C_NAEIW.NORMAL
            COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
            COA0004LableMessage.COA0004getMessage()
            Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
        End If
        Me.WF_LISTAREA.CssClass = Me.hdnSearchType.Value
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
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateDataTable()
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


        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objTxtPrifix As String = "txt" & Me.WF_LISTAREA.ID
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String) From {{"APPLY", objChkPrifix}}

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
                dr.Item(fieldId.Key) = displayValue
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
    ''' 申請理由表示処理
    ''' </summary>
    ''' <param name="isOpen"></param>
    Private Sub DisplayApplyReason(isOpen As Boolean)
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        '一覧表示データ復元 
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateDataTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                COA0004LableMessage.MESSAGENO = COA0021ListTable.ERR
                COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
                COA0004LableMessage.MESSAGEBOX = Me.lblFooterMessage
                COA0004LableMessage.COA0004getMessage()
                Me.lblFooterMessage = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
                Return
            End If
        Else
            dt = Me.SavedDt
        End If

        Dim uniqueIndex As String = Me.hdnCurrentUnieuqIndex.Value
        Dim targetRow = (From dr In dt
                         Where Convert.ToString(dr.Item("LINECNT")) = uniqueIndex)

        If targetRow IsNot Nothing AndAlso targetRow.Count > 0 Then
            If isOpen = True Then
                Me.txtRemarkInput.Text = Convert.ToString(targetRow(0).Item("APPLYTEXT"))
                Me.txtRemarkInput.Focus()
            Else
                targetRow(0).Item("APPLYTEXT") = Me.txtRemarkInput.Text
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
    '''' <summary>
    '''' 固定値マスタよりラジオボタン選択肢を取得
    '''' </summary>
    'Private Sub SetListViewTypeListItem()

    '    Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
    '    'リストクリア
    '    Me.rblListViewType.Items.Clear()
    '    Dim tmpListBoxObj As New ListBox
    '    'ユーザＩＤListBox設定
    '    COA0017FixValue.COMPCODE = "Default"
    '    COA0017FixValue.CLAS = "ORDERLISTVIEWTYPE"
    '    If COA0019Session.LANGDISP = C_LANG.JA Then
    '        COA0017FixValue.LISTBOX1 = tmpListBoxObj
    '    Else
    '        COA0017FixValue.LISTBOX2 = tmpListBoxObj
    '    End If
    '    COA0017FixValue.COA0017getListFixValue()
    '    If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
    '        If COA0019Session.LANGDISP = C_LANG.JA Then
    '            tmpListBoxObj = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
    '        Else
    '            tmpListBoxObj = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
    '        End If
    '    Else
    '        Return
    '    End If

    '    For Each item As ListItem In tmpListBoxObj.Items
    '        Me.rblListViewType.Items.Add(item)
    '    Next
    '    Dim COA0016VARIget As New BASEDLL.COA0016VARIget With {
    '            .MAPID = CONST_MAPID,
    '            .COMPCODE = "",
    '            .VARI = Convert.ToString(HttpContext.Current.Session("MAPvariant")),
    '            .FIELD = "ORDERLISTVIEWTYPE"
    '        }
    '    COA0016VARIget.COA0016VARIget()
    '    If Me.rblListViewType.Items.FindByValue(COA0016VARIget.VALUE) IsNot Nothing Then
    '        Me.rblListViewType.SelectedValue = COA0016VARIget.VALUE
    '    End If
    'End Sub
End Class