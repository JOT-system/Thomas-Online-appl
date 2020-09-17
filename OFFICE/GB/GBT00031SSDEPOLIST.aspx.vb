Imports System.Data.SqlClient
Imports BASEDLL

''' <summary>
''' 新港デポ在庫表画面クラス
''' </summary>
Public Class GBT00031SSDEPOLIST
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00031L" '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 25              'マウススクロール時の増分

    Private Const CONST_EXCEL_SHEET_NAME = "新港　在庫表"
    Private Const CONST_EXCEL_ADD_SHEET_NAME = "新港　在庫表 (STOCK分)"
    Private Const CONST_EXCEL_EMPTY_REPORT_ID = "JPSS Depo Stock List Container Only"

    Private Const CONST_VS_FILECNTDATA As String = "VSFILECNT" 'ファイル数保持用ビューステートデータ
    Private Const CONST_VS_ATTA_UNIQUEID As String = "ATTA_UNIQUEID"
    Private Const CONST_VS_PREV_ATTACHMENTINFO As String = "PREV_ATTACHMENTINFO"
    Private Const CONST_VS_CURR_ATTACHMENTINFO As String = "CURR_ATTACHMENTINFO"

    'アップロードファイルルート
    Private Const CONST_DIRNAME_APPEARANCE_UPROOT As String = "APPEARANCE" '外観チェックファイルアップロードルート

    '一覧チェックボックス項目
    Private LISTAREA_ITEM_CHECKBOX As String() = {"CHECK_DPIN", "CHECK_ETYD", "CHECK_DOUT", "CHECK_CYIN"}
    Private errDisp As String = Nothing                     'エラー用表示文言
    Private updateDisp As String = Nothing                  '更新用表示文言

    ''' <summary>
    ''' 処理返却用のメッセージ
    ''' </summary>
    Public Class ProcMessage
        Public Property MessageNo As String = C_MESSAGENO.NORMAL
        Public Property modOtherUsers As List(Of DataRow)
        Public Property dateSeqError As New List(Of DataRow)
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
    ''' 添付情報保持データテーブル
    ''' </summary>
    Private dtCurAttachment As DataTable

    Public Property ProcResult As ProcMessage = Nothing

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

            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

            '表示用文言判定
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errDisp = "ERROR"
                updateDisp = "UPDATE"
            Else
                errDisp = "エラー"
                updateDisp = "更新"
            End If

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
                '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
                Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
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
                '右ボックス帳票IDリストの生成
                '****************************************
                Dim retMessageNo As String = RightboxInit()
                If retMessageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(retMessageNo, Me.lblFooterMessage)
                    Return
                End If
                '****************************************
                '左ボックスリストの生成
                '****************************************
                SetFixvalueListItem("JPSSCHECK", Me.lbCheck)
                SetFixvalueListItem("GENERALFLG", Me.lbYesNo)

                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Using dt As DataTable = Me.GetListDataTable()
                    'グリッド用データをファイルに退避
                    With Nothing
                        Dim COA0021ListTable As New COA0021ListTable With {
                            .FILEdir = hdnXMLsaveFile.Value,
                            .TBLDATA = dt
                        }
                        COA0021ListTable.COA0021saveListTable()
                        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                            Return
                        End If
                        Me.SavedDt = dt

                        '保存時比較用のデータを退避
                        COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                        COA0021ListTable.TBLDATA = dt
                        COA0021ListTable.COA0021saveListTable()
                        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                            Return
                        End If
                    End With

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    Dim listVari As String = Me.hdnThisMapVariant.Value
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = listVari
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "2"
                        '.LEVENT = "ondblclick"
                        '.LFUNC = "ListDbClick"
                        .NOCOLUMNWIDTHOPT = 50
                        .OPERATIONCOLUMNWIDTHOPT = 80
                        .TITLEOPT = True
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
                End Using 'DataTable

                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)

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
                Me.dtCurAttachment = CollectDispAttachmentInfo()

                '**********************
                ' テキストボックス変更判定
                '**********************
                If Me.hdnOnchangeField IsNot Nothing AndAlso Me.hdnOnchangeField.Value <> "" Then
                    Dim btnEventName As String = ""
                    Dim param As Object = Nothing
                    If hdnOnchangeField.Value.StartsWith("txtWF_LISTAREACHECK_") Then

                        '変更イベント受け渡し用のパラメータ
                        Dim paramVal As New Hashtable
                        paramVal.Add("SENDER", hdnOnchangeField.Value) '対象フィールド名
                        paramVal.Add("ROW", Me.hdnListCurrentRownum.Value) '変更した行
                        param = paramVal
                        '実行関数名の生成
                        btnEventName = "txtListCheck_Change"
                        Me.hdnOnchangeField.Value = ""
                        '変更イベントが存在する場合は実行存在しない場合はスキップ
                        Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(btnEventName)
                        If mi IsNot Nothing Then
                            CallByName(Me, btnEventName, CallType.Method, param)
                        End If
                    ElseIf hdnOnchangeField.Value.StartsWith("txtWF_LISTAREA") Then

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
                    Else
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
                ' 一覧表の行ダブルクリック判定
                '**********************
                If Me.hdnListDBclick.Value <> "" Then
                    'イベント未定義
                    ListRowDbClick()
                    Me.hdnListDBclick.Value = ""
                    'Return '単票ページにリダイレクトするため念のため処理は終わらせる
                End If

                '**********************
                ' ファイルアップロード処理
                '**********************
                If Me.hdnListUpload.Value IsNot Nothing AndAlso Me.hdnListUpload.Value <> "" Then
                    If Me.hdnListUpload.Value = "XLS_LOADED" Then
                        UploadExcel()
                    ElseIf Me.hdnListUpload.Value = "PDF_LOADED" Then
                        UploadAttachment()
                    End If

                    Me.hdnListUpload.Value = ""
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
            'Me.Page.Form.Attributes.Add("data-mapvari", Me.hdnThisMapVariant.Value)
            '添付ファイル状態保存
            ViewState(CONST_VS_CURR_ATTACHMENTINFO) = Me.dtCurAttachment

            DisplayListObjEdit() '共通関数により描画された一覧の制御

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

            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
            Return

        End Try
    End Sub

#Region "<< 画面制御 >>"

    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is COM00002MENU Then
            'メニュー画面の場合
        ElseIf TypeOf Page.PreviousPage Is GBT00031SSDEPOLIST Then
            '自分自身のリロード（SAVE時に発生想定）
            Dim prevPage As GBT00031SSDEPOLIST = DirectCast(Page.PreviousPage, GBT00031SSDEPOLIST)

            '検索条件
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnMsgId", Me.hdnMsgId}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next
        End If

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
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "Excel Download")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        '****************************************
        ' 添付ファイルヘッダー部
        '****************************************
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderText, "添付書類を登録する場合は、ここにドロップすること", "To register attached documents, drop it here")
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderFileName, "ファイル名", "FileName")
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderDelete, "削 除", "Delete")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)

    End Sub
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
        Dim dicColumnNameToNo As New Dictionary(Of String, String) From {{"TANKNO", ""}, {"IMPORDERNO", ""}, {"EXPORDERNO", ""},
                                                                         {"ARVD", ""}, {"DPIN", ""}, {"ETYD", ""},
                                                                         {"TKAL", ""}, {"DOUT", ""}, {"CYIN", ""},
                                                                         {"CHECK_DPIN", ""}, {"CHECK_ETYD", ""},
                                                                         {"CHECK_DOUT", ""}, {"CHECK_CYIN", ""},
                                                                         {"UPDATE_DPIN", ""}, {"UPDATE_ETYD", ""},
                                                                         {"UPDATE_DOUT", ""}, {"UPDATE_CYIN", ""},
                                                                         {"ATTACHMENT", ""}, {"ISSTOCK", ""}}
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
        Dim dicLeftColumnNameToNo As New Dictionary(Of String, String) From {{"TANKNO", ""}}

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
        Dim disableRow As Boolean = False
        Dim rowCnt As Integer = rightDataTable.Rows.Count - 1
        Dim displayRow As DataRow = Nothing
        For i = 0 To rowCnt
            disableRow = False
            Dim tbrRight As TableRow = rightDataTable.Rows(i)

            Dim tbrLeft As TableRow = leftDataTable.Rows(i)
            Dim lineCnt As String = tbrLeft.Cells(0).Text
            displayRow = Nothing
            If dicDisplayRows.ContainsKey(CInt(lineCnt)) Then
                displayRow = dicDisplayRows(CInt(lineCnt))
            End If

            '復路未TKALのアイテムは非表示
            If displayRow.Item("EXPORDERNO").ToString = "" Then
                '入力項目を使用不可に
                For Each fieldName As String In {"DOUT", "CYIN", "CHECK_DOUT", "CHECK_CYIN"}
                    If disableRow = False AndAlso dicColumnNameToNo(fieldName) <> "" Then
                        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                            If .Text.StartsWith(String.Format("<input id=""txtWF_LISTAREA{0}", fieldName)) Then
                                .Text = .Text.Replace(">", " disabled=""disabled"" class=""aspNetDisabled"" />")
                            End If
                        End With
                    End If
                Next

            End If

        Next 'END ROWCOUNT
    End Sub

    ''' <summary>
    ''' 一覧 マウスホイール時処理 (一覧スクロール)
    ''' </summary>
    Protected Sub ListScrole()

        '表示データ件数取得
        Dim DataCnt As Integer = 0                  '(絞り込み後)有効Data数

        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル
            dt = CreateListDataTable()
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
        ElseIf Integer.TryParse(Me.hdnListPosition.Value, ListPosition) = False Then
            ListPosition = 1
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
        With COA0013TableObject
            .MAPID = CONST_MAPID
            .VARI = Me.hdnThisMapVariant.Value
            .SRCDATA = listData
            .TBLOBJ = Me.WF_LISTAREA
            .SCROLLTYPE = "2"
            '.LEVENT = "ondblclick"
            '.LFUNC = "ListDbClick"
            .NOCOLUMNWIDTHOPT = 50
            .OPERATIONCOLUMNWIDTHOPT = 80
            .TITLEOPT = True
            .USERSORTOPT = 1
        End With
        COA0013TableObject.COA0013SetTableObject()

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.チェックがついているチェックボックス"DISPLAY_LINECNT_LIST"オブジェクトをチェック状態にする
        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If

        hdnMouseWheel.Value = ""

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
            dt = CreateListDataTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
                Me.PrevDt = dt.Clone
                For Each cdr As DataRow In dt.Rows
                    Me.PrevDt.ImportRow(cdr)
                Next
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

        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objTxtPrifix As String = "txt" & Me.WF_LISTAREA.ID
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String)
        fieldIdList.Add("DPIN", objTxtPrifix)
        fieldIdList.Add("ETYD", objTxtPrifix)
        fieldIdList.Add("DOUT", objTxtPrifix)
        fieldIdList.Add("CYIN", objTxtPrifix)
        fieldIdList.Add("CHECK_DPIN", objTxtPrifix)
        fieldIdList.Add("CHECK_ETYD", objTxtPrifix)
        fieldIdList.Add("CHECK_DOUT", objTxtPrifix)
        fieldIdList.Add("CHECK_CYIN", objTxtPrifix)

        For Each i In displayLineCnt
            Dim dr As DataRow = dt.Rows(i - 1)

            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                End If

                Dim val As String = ""
                If {"DPIN", "ETYD", "DOUT", "CYIN"}.Contains(fieldId.Key) Then

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
#End Region

#Region "<< イベント(btn) >>"
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()

        Dim notSavedData = GetModifiedDataTable()
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
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '画面戻先URL取得
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = hdnThisMapVariant.Value
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

        '画面遷移実行
        Server.Transfer(url)
    End Sub

    ''' <summary>
    ''' Excelダウンロードボタン押下時処理
    ''' </summary>
    Public Sub btnExcelDownload_Click()
        Dim dt As DataTable
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        If Me.SavedDt Is Nothing Then
            dt = CreateListDataTable()
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
        '現在表示しているもののみ
        Dim dispDispRow = (From item In dt Where Convert.ToString(item("HIDDEN")) = "0")
        If dispDispRow.Any = False Then
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

            Dim outputRow = (From item In dispDispRow Where Convert.ToString(item("ISSTOCK")) <> "Y")
            If outputRow.Any = True Then
                outputDt = outputRow.CopyToDataTable
                For i As Integer = 0 To outputDt.Rows.Count - 1
                    '行（ラインカウント）を再設定する
                    outputDt.Rows(i)("LINECNT") = i + 1
                    outputDt.Rows(i)("REPORTDATE") = ""
                Next
            Else
                outputDt = dt.Clone
            End If


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


            'STOK
            Dim stockRow = (From item In dispDispRow Where Convert.ToString(item("ISSTOCK")) = "Y")
            If stockRow.Any = True Then
                outputDt = stockRow.CopyToDataTable
                For i As Integer = 0 To outputDt.Rows.Count - 1
                    '行（ラインカウント）を再設定する
                    outputDt.Rows(i)("LINECNT") = i + 1
                    outputDt.Rows(i)("REPORTDATE") = ""
                Next
            Else
                outputDt = dt.Clone
            End If

            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = outputDt                              'PARAM04:データ参照tabledata
            COA0027ReportTable.ADDFILE = COA0027ReportTable.FILEpath

            COA0027ReportTable.ADDSHEET = CONST_EXCEL_ADD_SHEET_NAME
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
    ''' 保存ボタン押下時イベント
    ''' </summary>
    Public Sub btnSave_Click()
        Dim dt As DataTable = Nothing
        Dim messageNo As String

        If Me.SavedDt Is Nothing Then
            dt = CreateListDataTable()
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
        ChangeInvalidChar(dt, New List(Of String) From {"DPIN", "ETYD", "DOUT", "CYIN", "CHECK_DPIN", "CHECK_ETYD", "CHECK_DOUT", "CHECK_CYIN"})

        Dim targetData = GetModifiedDataTable()
        '登録対象データが0件の場合は処理終了
        If targetData Is Nothing OrElse targetData.Count = 0 Then
            messageNo = C_MESSAGENO.NOENTRYDATA
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '入力チェック
        Dim errMessage As String = ""
        Dim fieldList As New List(Of String) From {"DPIN", "ETYD", "DOUT", "CYIN", "CHECK_DPIN", "CHECK_ETYD", "CHECK_DOUT", "CHECK_CYIN"}
        Dim keyFields As New List(Of String) From {"LINECNT"}

        Dim checkDt As DataTable = Me.CreateListDataTable
        checkDt.Merge(targetData.CopyToDataTable)

        '単項目チェック
        If checkDt.Rows.Count > 0 Then
            messageNo = CheckSingle(CONST_MAPID, checkDt, fieldList, errMessage, keyFields:=keyFields)
            If messageNo <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)

                '右ボックスにエラーメッセージ表示
                Me.txtRightErrorMessage.Text = errMessage
                Return
            End If

            'LISTチェック
            For Each row As DataRow In checkDt.Rows
                For Each col In LISTAREA_ITEM_CHECKBOX
                    messageNo = ChedckList(row(col).ToString, Me.lbCheck, col, errMessage)
                    If messageNo <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)

                        '右ボックスにエラーメッセージ表示
                        Me.txtRightErrorMessage.Text = errMessage
                        Return
                    End If

                Next
            Next

            'ERRORが存在する場合は更新不可
            For Each row As DataRow In checkDt.Rows
                If row.Item("OPERATION").ToString = errDisp Then
                    messageNo = C_MESSAGENO.INPUTERROR
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)
                    Return
                End If
            Next

        End If

        'OrderValueへのDataTable変換
        Dim updData = GetUpdateData(targetData)

        Me.ProcResult = EntryOrderValue(updData)
        If Me.ProcResult.MessageNo <> C_MESSAGENO.NORMALDBENTRY Then
            Dim naeiw As String = C_NAEIW.ABNORMAL
            CommonFunctions.ShowMessage(Me.ProcResult.MessageNo, Me.lblFooterMessage, naeiw:=naeiw, pageObject:=Me)
            '右ボックス表示する結果の場合はメッセージを生成
            If Me.ProcResult.MessageNo = C_MESSAGENO.RIGHTBIXOUT Then
                Dim message As New StringBuilder
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

            ''添付ファイルを正式フォルダに転送
            'For Each a As DataRow In targetData
            '    Dim orderNo = a.Item("IMPORDERNO").ToString
            '    CommonFunctions.SaveAttachmentFilesList(dtCurAttachment, orderNo, CONST_DIRNAME_APPEARANCE_UPROOT)
            'Next

            HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
            Server.Transfer(Request.Url.LocalPath)
        End If

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
#End Region

#Region "<< LeftBox >>"

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
                    If Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREADPIN") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREAETYD") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREADOUT") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREACYIN") Then
                        Dim rowitem = GetDatatableDate(Me.hdnTextDbClickField.Value, Me.hdnListCurrentRownum.Value)
                        Dim selectedDate As String = ""
                        If rowitem.Value IsNot Nothing Then
                            selectedDate = Convert.ToString(rowitem.Value(rowitem.Key))
                        End If
                        Dim tmpDate As Date
                        If Date.TryParse(selectedDate, tmpDate) = False Then
                            selectedDate = ""
                        End If
                        Me.hdnCalendarValue.Value = selectedDate
                        Me.mvLeft.Focus()
                    Else
                        targetObject = FindControl(Me.hdnTextDbClickField.Value)
                        If targetObject IsNot Nothing Then

                            Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                            Dim wkDate As Date = Nothing
                            If Date.TryParseExact(txtobj.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, wkDate) Then
                                Me.hdnCalendarValue.Value = wkDate.ToString("yyyy/MM/dd")
                            Else
                                Me.hdnCalendarValue.Value = txtobj.Text
                            End If

                            Me.mvLeft.Focus()
                        End If
                    End If
                'チェックビュー表示切替
                Case Me.vLeftCheck.ID
                    '書き換えるテキストフィールドを特定
                    Dim targetDateField = Me.hdnTextDbClickField.Value
                    If targetDateField.StartsWith("txtWF_LISTAREACHECK_DPIN") Then
                        targetDateField = "CHECK_DPIN"
                    ElseIf targetDateField.StartsWith("txtWF_LISTAREACHECK_ETYD") Then
                        targetDateField = "CHECK_ETYD"
                    ElseIf targetDateField.StartsWith("txtWF_LISTAREACHECK_DOUT") Then
                        targetDateField = "CHECK_DOUT"
                    ElseIf targetDateField.StartsWith("txtWF_LISTAREACHECK_CYIN") Then
                        targetDateField = "CHECK_CYIN"
                    End If

                    Dim targetRows = From dr As DataRow In Me.SavedDt
                                     Where Convert.ToString(dr.Item("LINECNT")) = Me.hdnListCurrentRownum.Value
                    'ありえないが編集業が存在しない場合
                    If targetRows Is Nothing Then
                        Return
                    End If
                    '一応現在入力しているテキストと一致するものを選択状態
                    With Me.lbCheck
                        .Focus()
                        .SelectedIndex = -1
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByText(targetRows.First.Item(targetDateField).ToString)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With

                Case Else

                    Dim dicListId As New Dictionary(Of String, ListBox) _
                        From {{Me.vLeftYesNo.ID, Me.lbYesNo}}

                    If dicListId.ContainsKey(changeViewObj.ID) = False Then
                        Return
                    End If
                    Dim targetListObj = dicListId(changeViewObj.ID)
                    targetListObj.SelectedIndex = -1
                    targetListObj.Focus()

                    If Me.hdnTextDbClickField.Value.StartsWith("repAttachment_txtDeleteFlg_") Then
                        Dim drIndex As Integer = CInt(Me.hdnTextDbClickField.Value.Replace("repAttachment_txtDeleteFlg_", ""))
                        Dim dtAttachment As DataTable = Me.dtCurAttachment
                        Dim drTargetAttachmentRow = dtAttachment.Rows(drIndex)
                        Dim findLbValue As ListItem = targetListObj.Items.FindByValue(Convert.ToString(drTargetAttachmentRow("DELFLG")))
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
            End Select
        End If
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
                    If Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREADPIN") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREAETYD") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREADOUT") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREACYIN") Then
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
                    End If
                Case Me.vLeftCheck.ID 'アクティブなビューがCheck
                    'Me.hdnActiveElementAfterOnChange.Value = Me.hdnTextDbClickField.Value
                    Dim messageNo As String = UpdateDatatable(Me.lbCheck.SelectedItem.Text, Me.hdnTextDbClickField.Value, Me.hdnListCurrentRownum.Value)
                    If messageNo <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage)
                    End If

                Case Else
                    If Me.hdnTextDbClickField.Value.StartsWith("repAttachment_txtDeleteFlg_") Then
                        Dim drIndex As Integer = CInt(Me.hdnTextDbClickField.Value.Replace("repAttachment_txtDeleteFlg_", ""))
                        Dim dtAttachment As DataTable = Me.dtCurAttachment
                        Dim drTargetAttachmentRow = dtAttachment.Rows(drIndex)

                        If Me.lbYesNo.SelectedItem IsNot Nothing Then
                            drTargetAttachmentRow("DELFLG") = Me.lbYesNo.SelectedValue
                        Else
                            drTargetAttachmentRow("DELFLG") = ""
                        End If
                        Me.repAttachment.DataSource = dtAttachment
                        Me.repAttachment.DataBind()
                        Exit Select
                    End If
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
    ''' Fixvalueを元にリストボックスを作成
    ''' </summary>
    ''' <param name="className"></param>
    ''' <param name="targetList"></param>
    ''' <remarks></remarks>
    Private Sub SetFixvalueListItem(className As String, targetList As ListBox)
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Dim jpList As New ListBox
        Dim engList As New ListBox
        targetList.Items.Clear()
        'Term選択肢
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = className
        COA0017FixValue.LISTBOX1 = jpList
        COA0017FixValue.LISTBOX2 = engList
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                targetList.Items.AddRange(jpList.Items.Cast(Of ListItem).ToArray)
            Else
                targetList.Items.AddRange(engList.Items.Cast(Of ListItem).ToArray)
            End If
        Else
            Throw New Exception("Fix value getError")
        End If
    End Sub

#End Region

#Region "<< RightBox >>"
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

#End Region

    ''' <summary>
    ''' DBより一覧用データ取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetListDataTable() As DataTable
        Dim mapId As String = CONST_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力

        Dim retDt As New DataTable
        Dim sb As New StringBuilder(2048)

        sb.Append("select ")
        sb.Append("    RANK() OVER(PARTITION BY ov.TANKNO ORDER BY (CASE WHEN ov.ACTUALDATE = '1900/01/01' THEN '9999/12/12' else ov.ACTUALDATE END) desc, convert(char(10),ov.INITYMD,111) desc, convert(int,ov.DISPSEQ) desc) as RECENT ")
        sb.Append("   , case when ob.DISCHARGEPORT1='JPSDJ' then '1' else '0' end as ISIMPORT ")
        sb.Append("  , ov.DATAID ")
        sb.Append("  , ov.ORDERNO ")
        sb.Append("  , ov.TANKNO ")
        sb.Append("  , ov.ACTIONID ")
        sb.Append("  , ov.SCHEDELDATE ")
        sb.Append("  , ov.ACTUALDATE ")
        sb.Append("  , case when ov.TANKCONDITION = '' then '0' else ov.TANKCONDITION end as TANKCONDITION ")
        sb.Append("  ,isnull(convert(nvarchar, ov.UPDYMD , 120),'') as UPDYMD ")
        sb.Append("  ,isnull(rtrim(ov.UPDUSER),'')                  as UPDUSER ")
        sb.Append("  ,isnull(rtrim(ov.UPDTERMID),'')                as UPDTERMID ")
        sb.Append("from GBT0005_ODR_VALUE as ov with(nolock) ")
        sb.Append("inner join GBT0004_ODR_BASE as ob on ob.ORDERNO=ov.ORDERNO and ob.DELFLG<>@DELFLG ")
        sb.Append("inner join ( ")
        '-- 対象タンク
        sb.Append("	select ")
        sb.Append("	    s.TANKNO ")
        sb.Append("	from GBV0001_TANKSTATUS as s ")
        '-- リースタンク
        sb.Append("	inner join GBV0002_LEASETANK as lt on lt.TANKNO=s.TANKNO ")
        '-- HIS判定
        sb.Append("	inner join ( ")
        sb.Append("		 select ")
        sb.Append("		    ORDERNO ")
        sb.Append("		   , case when b.DISCHARGEPORT1='JPSDJ' then '1' else '0' end as ISIMPORT ")
        sb.Append("		 from GBT0004_ODR_BASE as b ")
        sb.Append("		 inner join GBM0004_CUSTOMER as c on c.COMPCODE=@COMPCODE and c.CUSTOMERCODE=b.SHIPPER and c.STYMD<=b.STYMD and c.ENDYMD>=b.ENDYMD and c.DELFLG<>@DELFLG ")
        sb.Append("		 inner join COS0017_FIXVALUE as f on f.CLASS='PROJECT' and f.KEYCODE='HIS' and f.STYMD<=b.STYMD and f.ENDYMD>=b.ENDYMD and f.DELFLG<>@DELFLG ")
        sb.Append("		 and   b.DELFLG <> @DELFLG ")
        sb.Append("		 and   c.TORICOMP=f.VALUE1 ")
        sb.Append("		) as his ON his.ORDERNO=s.ORDERNO ")
        sb.Append("	where  s.RECENT=1 ")
        sb.Append("	and ((his.ISIMPORT='1' and s.ACTIONID in ('ARVD', 'STOK', 'DPIN', 'DLRY', 'ETYD')) or (his.ISIMPORT='0' and s.ACTIONID in ('TKAL', 'DOUT'))) ")
        sb.Append("	) as st on st.TANKNO=ov.TANKNO ")
        sb.Append("where ov.DELFLG <> @DELFLG ")
        sb.Append("and   ov.ACTIONID <> '' ")
        '--       sb.Append("and  ((ov.ACTUALDATE <> '1900/01/01' ) or ( (ov.ACTIONID = 'TKAL' or ov.ACTIONID = 'TAED' or ov.ACTIONID = 'TAEC') and  ov.ACTUALDATE =  '1900/01/01')) ")

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

        '一覧表編集
        retDt = SummaryDataTable(retDt)

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
        retDt.Columns.Add("Select", GetType(Integer))             'DBの固定フィールド
        retDt.Columns.Add("HIDDEN", GetType(Integer))

        Dim colList As New List(Of String) From {"TANKNO", "TANKNO_H", "TANKNO_N",
                                                "OUTPUTDATE", "REPORTDATE",
                                                "IMPORDERNO", "EXPORDERNO",
                                                "ACTIONID",
                                                "ISSTOCK",
                                                "ARVD",
                                                "DPIN", "DLRY", "ETYD",
                                                "TKAL",
                                                "DOUT", "CYIN",
                                                "CHECK_DPIN", "CHECK_DLRY", "CHECK_ETYD",
                                                "CHECK_DOUT", "CHECK_CYIN",
                                                "DATAID_DPIN", "DATAID_DLRY", "DATAID_ETYD",
                                                "DATAID_DOUT", "DATAID_CYIN",
                                                "UPDATE_DPIN", "UPDATE_DLRY", "UPDATE_ETYD",
                                                "UPDATE_DOUT", "UPDATE_CYIN",
                                                "UPDYMD_DPIN", "UPDYMD_DLRY", "UPDYMD_ETYD",
                                                "UPDYMD_DOUT", "UPDYMD_CYIN",
                                                "ATTACHMENT"
        }

        For Each colName As String In colList
            retDt.Columns.Add(colName, GetType(String))
            retDt.Columns(colName).DefaultValue = ""
        Next
        Return retDt
    End Function

    ''' <summary>
    ''' サマリー一覧編集
    ''' </summary>
    ''' <returns></returns>
    Private Function SummaryDataTable(ByRef dt As DataTable) As DataTable
        Dim impSSDepo As String() = {"ARVD", "DPIN", "DLRY", "ETYD", "STOK"}
        Dim expSSDepo As String() = {"TKAL", "DOUT", "CYIN"}

        '一覧表用データテーブル作成
        Dim retDt = CreateListDataTable()
        Dim lineCnt As Integer = 0

        Dim outputDate As String = Today().ToShortDateString
        'タンク一覧作成（タンクステータス履歴）
        Dim tmpDt = dt.AsEnumerable
        'タンク毎に処理
        Dim tankDt = tmpDt.GroupBy(Function(a) a.Item("TANKNO").ToString)
        For Each tank In tankDt
            Dim tankNo As String = tank.First.Item("TANKNO").ToString

            lineCnt += 1
            Dim newRow = retDt.NewRow
            newRow("LINECNT") = lineCnt
            newRow("OPERATION") = ""
            newRow("TIMSTP") = 0
            newRow("Select") = "1"
            newRow("HIDDEN") = "0"

            newRow("OUTPUTDATE") = outputDate
            newRow("REPORTDATE") = outputDate

            newRow("TANKNO") = tankNo
            newRow("TANKNO_H") = Left(tankNo, 4)
            newRow("TANKNO_N") = Mid(tankNo, 5)

            Dim lastact As String = ""
            'タンク動静履歴
            For Each actCol As DataRow In tank
                Dim act As String = actCol.Item("ACTIONID").ToString
                Dim actualDate As String = FormatDateContrySettings(actCol("ACTUALDATE").ToString, "yyyy/MM/dd")
                Dim scheduleDate As String = FormatDateContrySettings(actCol("SCHEDELDATE").ToString, "yyyy/MM/dd")
                Dim orderNo As String = actCol.Item("ORDERNO").ToString
                Dim isImport As String = actCol.Item("ISIMPORT").ToString

                If isImport = "1" AndAlso impSSDepo.Contains(act) Then
                    newRow("IMPORDERNO") = orderNo
                ElseIf isImport = "0" AndAlso expSSDepo.Contains(act) Then
                    newRow("EXPORDERNO") = orderNo
                Else
                    Continue For
                End If


                If retDt.Columns.Contains(act) Then
                    If actualDate = "1900/01/01" AndAlso scheduleDate <> "1900/01/01" Then
                        '予定日
                        newRow(act) = scheduleDate
                    ElseIf actualDate <> "1900/01/01" Then
                        '実施日
                        newRow(act) = actualDate
                        If String.IsNullOrEmpty(lastact) Then
                            lastact = act
                        End If
                    Else
                        newRow(act) = ""
                    End If

                    Dim checkValue As String = actCol("TANKCONDITION").ToString
                    Dim listItem = Me.lbCheck.Items.FindByValue(checkValue)
                    If Not IsNothing(listItem) Then
                        checkValue = listItem.Text
                    End If

                    If retDt.Columns.Contains("DATAID_" & act) Then
                        newRow("DATAID_" & act) = actCol("DATAID").ToString
                    End If
                    If retDt.Columns.Contains("CHECK_" & act) Then
                        newRow("CHECK_" & act) = checkValue
                    End If
                    If retDt.Columns.Contains("UPDYMD_" & act) Then
                        newRow("UPDYMD_" & act) = actCol("UPDYMD").ToString
                    End If
                    If retDt.Columns.Contains("UPDATE_" & act) Then
                        newRow("UPDATE_" & act) = "0"
                    End If
                End If
                '輸入着港まで
                If isImport = "1" AndAlso act = "ARVD" Then
                    Exit For
                End If
                'STOCK指定
                '※ STOCK解除後（後続工程の日付入力関係なく）もSTOCKとして分類する
                If act = "STOK" AndAlso actualDate <> "1900/01/01" Then
                    newRow.Item("ISSTOCK") = "Y"
                End If
            Next

            '輸入オーダー存在しない場合はSkip
            If newRow("IMPORDERNO").ToString = "" Then
                Exit For
            End If

            newRow("ACTIONID") = lastact
            retDt.Rows.Add(newRow)

            '添付ファイル数取得
            GetAttachmentCnt(newRow)
        Next

        Return retDt
    End Function


    ''' <summary>
    ''' リスト行ダブルクリック時イベント
    ''' </summary>
    Private Sub ListRowDbClick()
        'イベントなし

    End Sub

    ''' <summary>
    ''' 一覧表チェック項目変更時イベント
    ''' </summary>
    ''' <param name="param">キー:SENDER 値：変更したテキストボックスID</param>
    '''                     キー:ROW       値：対象の行
    Public Sub txtListCheck_Change(param As Hashtable)
        Dim val As String = ""
        Dim targetObjId As String = Convert.ToString(param("SENDER"))
        Dim rowNum As String = Convert.ToString(param("ROW"))
        If Request.Form.AllKeys.Contains(targetObjId) = True Then
            val = Request.Form.Item(targetObjId)
            val = val.Trim

            UpdateDatatable(val, targetObjId, rowNum)
        End If
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
            dt = CreateListDataTable()
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
        Dim targetDateField As String = ""
        If txtBoxId.StartsWith("txtWF_LISTAREADPIN") Then
            targetDateField = "DPIN"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREAETYD") Then
            targetDateField = "ETYD"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREADOUT") Then
            targetDateField = "DOUT"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREACYIN") Then
            targetDateField = "CYIN"
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
    ''' 対応する項目に関連するデータテーブルを更新
    ''' </summary>
    ''' <param name="dtValue"></param>
    ''' <param name="txtBoxId"></param>
    ''' <param name="rowNum"></param>
    ''' <returns></returns>
    Private Function UpdateDatatable(dtValue As String, txtBoxId As String, rowNum As String, Optional targetDt As DataTable = Nothing) As String
        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = Nothing

        '一覧表示データ復元
        If targetDt IsNot Nothing Then
            dt = targetDt
        ElseIf Me.SavedDt Is Nothing Then
            dt = CreateListDataTable()
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

        '変更対象の行を取得
        Dim targetRows = From dr As DataRow In Me.PrevDt
                         Where Convert.ToString(dr.Item("LINECNT")) = rowNum
        Dim afterInputRows = From dr As DataRow In dt
                             Where Convert.ToString(dr.Item("LINECNT")) = rowNum

        'ありえないが編集行が存在しない場合
        If targetRows Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If
        '自身の対象行を取得
        Dim targetRow As DataRow = targetRows(0)
        Dim afterInputRow As DataRow = afterInputRows(0)

        '書き換えるテキストフィールドを特定
        Dim targetDateField As String = ""
        If txtBoxId.StartsWith("txtWF_LISTAREACHECK_DPIN") Then
            targetDateField = "CHECK_DPIN"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREACHECK_ETYD") Then
            targetDateField = "CHECK_ETYD"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREACHECK_DOUT") Then
            targetDateField = "CHECK_DOUT"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREACHECK_CYIN") Then
            targetDateField = "CHECK_CYIN"
        Else
            Return C_MESSAGENO.NORMAL
        End If

        '自身の行の設定値を取得
        Dim prevDtValue As String = Convert.ToString(targetRow.Item(targetDateField))
        '変更発生時
        If dtValue <> prevDtValue Then
            Dim retMessageNo As String = ""
            Dim retMessage As String = ""

            retMessageNo = ChedckList(dtValue, Me.lbCheck, targetDateField, retMessage)
            If retMessageNo <> C_MESSAGENO.NORMAL Then
                afterInputRow.Item("UPDATE_" & targetDateField.Replace("CHECK_", "")) = "9"
            Else
                '自身の行の値を編集
                afterInputRow.Item(targetDateField) = dtValue
                afterInputRow.Item("UPDATE_" & targetDateField.Replace("CHECK_", "")) = "1"
            End If
        End If

        Dim retValue = CheckRowStatus(afterInputRow)
        If retValue <> C_MESSAGENO.NORMAL Then
            Return C_MESSAGENO.RIGHTBIXOUT
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
            dt = CreateListDataTable()
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
        If txtBoxId.StartsWith("txtWF_LISTAREADPIN") Then
            targetDateField = "DPIN"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREAETYD") Then
            targetDateField = "ETYD"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREADOUT") Then
            targetDateField = "DOUT"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREACYIN") Then
            targetDateField = "CYIN"
        Else
            Return C_MESSAGENO.NORMAL
        End If
        '日付変更対象の行を取得
        Dim targetRows = From dr As DataRow In Me.PrevDt
                         Where Convert.ToString(dr.Item("LINECNT")) = rowNum
        Dim afterInputRows = From dr As DataRow In dt
                             Where Convert.ToString(dr.Item("LINECNT")) = rowNum

        'ありえないが編集行が存在しない場合
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
        '自身の行の報告日付を取得
        Dim reportDtValue As String = Convert.ToString(targetRow.Item("REPORTDATE"))
        '変更発生時
        '又は日付が予定→実績とみなす（報告日より以前）場合
        If dtValue <> prevDtValue OrElse dtValue < reportDtValue Then
            '自身の行の日付を編集
            afterInputRow.Item(targetDateField) = dtValue
            afterInputRow.Item("UPDATE_" & targetDateField.Replace("CHECK_", "")) = "1"

            Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck              '項目チェック
            Dim retMessageNo As String = C_MESSAGENO.NORMAL
            Dim hasError As Boolean = False
            Dim dummyLabelObj As New Label '画面描画しないダミーのラベルオブジェクト
            Dim retMessage As New StringBuilder

            'チェックごとに変わらないパラメータ設定
            COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
            COA0026FieldCheck.MAPID = CONST_MAPID
            '日付単項目チェック
            COA0026FieldCheck.FIELD = targetDateField
            COA0026FieldCheck.VALUE = dtValue
            COA0026FieldCheck.COA0026FieldCheck()
            If COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL Then
                afterInputRow.Item("UPDATE_" & targetDateField.Replace("CHECK_", "")) = "9"

                retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                retMessage.AppendFormat("・{0} ： {1}", targetDateField, dummyLabelObj.Text).AppendLine()

            End If 'END  COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL

            '関連日付チェック
            Dim updateList = New List(Of String)
            Dim checkDateSpanObjectsTransit As New List(Of String) From {
                afterInputRow.Item("ARVD").ToString,
                afterInputRow.Item("DPIN").ToString,
                afterInputRow.Item("ETYD").ToString,
                afterInputRow.Item("TKAL").ToString,
                afterInputRow.Item("DOUT").ToString,
                afterInputRow.Item("CYIN").ToString}
            retMessageNo = CheckDateSpan(checkDateSpanObjectsTransit)
            If retMessageNo = C_MESSAGENO.VALIDITYINPUT Then
                afterInputRow.Item("UPDATE_" & targetDateField.Replace("CHECK_", "")) = "9"
                retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                retMessage.AppendFormat("・{0} ： {1}", targetDateField, dummyLabelObj.Text).AppendLine()
            End If

            CheckRowStatus(afterInputRow)

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
    ''' 日付間隔チェック
    ''' </summary>
    ''' <param name="dateObj"></param>
    ''' <returns>メッセージNo</returns>
    Private Function CheckDateSpan(dateObj As List(Of String)) As String

        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim hasValue As Boolean = False
        Dim prevFieldtDate As Date
        Dim currentDate As Date
        For Each txtObj As String In dateObj
            '空白の場合はスキップ
            If txtObj.Trim = "" Then
                Continue For
            End If
            Dim dateString = FormatDateYMD(txtObj, GBA00003UserSetting.DATEFORMAT)
            If hasValue = False Then
                Date.TryParse(dateString, prevFieldtDate)
                hasValue = True
                Continue For
            End If
            Date.TryParse(dateString, currentDate)

            If currentDate < prevFieldtDate Then
                retMessageNo = C_MESSAGENO.VALIDITYINPUT
                Return retMessageNo
            Else
                prevFieldtDate = currentDate
            End If
        Next
        Return retMessageNo
    End Function

    ''' <summary>
    ''' 一覧表レコードチェック
    ''' </summary>
    ''' <returns>メッセージNo</returns>
    Private Function CheckRowStatus(targetRow As DataRow) As String
        Dim retVal As String = C_MESSAGENO.NORMAL

        Dim colList = New List(Of String)
        For Each col As String In {"UPDATE_DPIN", "UPDATE_ETYD", "UPDATE_DOUT", "UPDATE_CYIN"}
            colList.Add(targetRow(col).ToString)
        Next
        If colList.Contains("9") Then
            targetRow.Item("OPERATION") = errDisp
            retVal = C_MESSAGENO.RIGHTBIXOUT
        ElseIf colList.Contains("1") Then
            targetRow.Item("OPERATION") = updateDisp
        Else
            targetRow.Item("OPERATION") = ""
        End If

        Return retVal
    End Function

#Region "<< 在庫表アップロード関連 >>"
    ''' <summary>
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    Protected Sub UploadExcel()
        Dim COA0029XlsTable As New BASEDLL.COA0029XlsTable
        Dim reportId As String = Me.lbRightList.SelectedItem.Value
        Dim reportMapId As String = CONST_MAPID


        '初期処理
        Me.txtRightErrorMessage.Text = ""
        Me.lblFooterMessage.Text = ""
        'Me.lblFooterMessage.ForeColor = Color.Black
        'Me.lblFooterMessage.Font.Bold = False


        ''初期処理
        'errList = New List(Of String)
        'errListAll = New List(Of String)
        Dim returnCode As String = C_MESSAGENO.NORMAL

        ''UPLOAD_XLSデータ取得
        COA0029XlsTable.MAPID = reportMapId
        COA0029XlsTable.SHEETNAME = CONST_EXCEL_SHEET_NAME
        COA0029XlsTable.COA0029XlsToTable()
        If COA0029XlsTable.ERR = C_MESSAGENO.NORMAL Then
        Else
            '取得したExcelデータのレポートIDが現在の表示機能と一致しているか確認
            If Not Me.lbRightList.SelectedItem.Value.Equals(COA0029XlsTable.REPORTID) Then
                'TODOエラーメッセージ＋Return
            End If
            returnCode = COA0029XlsTable.ERR
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Dim excelDt As DataTable = COA0029XlsTable.TBLDATA.Copy

        ''UPLOAD_XLSデータ取得
        COA0029XlsTable.MAPID = reportMapId
        COA0029XlsTable.SHEETNAME = CONST_EXCEL_ADD_SHEET_NAME
        COA0029XlsTable.COA0029XlsToTable()
        If COA0029XlsTable.ERR = C_MESSAGENO.NORMAL Then
        Else
            '取得したExcelデータのレポートIDが現在の表示機能と一致しているか確認
            If Not Me.lbRightList.SelectedItem.Value.Equals(COA0029XlsTable.REPORTID) Then
                'TODOエラーメッセージ＋Return
            End If
            returnCode = COA0029XlsTable.ERR
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '通常分と在庫分マージ
        excelDt.Merge(COA0029XlsTable.TBLDATA)
        If excelDt.Rows.Count = 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Dim errMsg As String = ""
        returnCode = UpdateDataTableFromExcelFile(excelDt, errMsg)
        Dim naeiw As String = C_NAEIW.ABNORMAL
        If returnCode = C_MESSAGENO.NORMAL Then
            naeiw = C_NAEIW.NORMAL
        Else
            Me.txtRightErrorMessage.Text = errMsg
        End If
        CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=naeiw, pageObject:=Me)
    End Sub


    ''' <summary>
    ''' アップロードされたExcelデータテーブルをもとに内部データテーブルを更新する
    ''' </summary>
    ''' <param name="uploadedExcelDt">Excelで取得したデータテーブ</param>
    ''' <param name="errMsg">[OUT]右ボックス用メッセージ</param>
    ''' <returns>メッセージNo</returns>
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
            writeDt = CreateListDataTable()
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

        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim retMessage As New StringBuilder


        'ACTYが空のデータを上に持っていき処理を行う
        Dim uploadedExcelDtSorted = ""
        Dim writeFieldNameList As New List(Of String) '更新対象フィールド一覧
        writeFieldNameList.AddRange({"DPIN", "ETYD", "DOUT", "CYIN", "CHECK_DPIN", "CHECK_ETYD", "CHECK_DOUT", "CHECK_CYIN"})
        Dim sortedUploadExcelDt As DataTable = (From uploadedExcelDr In uploadedExcelDt).CopyToDataTable
        For Each dr As DataRow In sortedUploadExcelDt.Rows
            Dim tankNo = Convert.ToString(dr.Item("TANKNO_H")) & Convert.ToString(dr.Item("TANKNO_N"))

            'ExcelのTANKNOとローカルTANKNOをマッチングさせ書き込む行を特定
            Dim writeDr As DataRow = (From wdr In writeDt
                                      Where wdr.Item("TANKNO").Equals(tankNo)).FirstOrDefault

            '書き込み先が存在しない場合はつぎへスキップ
            If writeDr Is Nothing Then
                Continue For
            End If

            '報告日・出力日チェック
            Dim outputDateString = dr.Item("OUTPUTDATE").ToString
            Dim reportDateString = dr.Item("REPORTDATE").ToString
            outputDateString = outputDateString.Trim
            outputDateString = FormatDateYMD(outputDateString, GBA00003UserSetting.DATEFORMAT)
            reportDateString = reportDateString.Trim
            reportDateString = FormatDateYMD(reportDateString, GBA00003UserSetting.DATEFORMAT)
            If outputDateString = "" Then
                retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                retMessage.AppendFormat("・{0}：{1}", "出力日", "未入力です！").AppendLine()
                retMessage.AppendFormat("--> {0} = {1}", "出力日", outputDateString).AppendLine()
                errMsg = retMessage.ToString
                Return retMessageNo
            ElseIf reportDateString = "" Then
                retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                retMessage.AppendFormat("・{0}：{1}", "報告日", "未入力です！").AppendLine()
                retMessage.AppendFormat("--> {0} = {1}", "報告日", reportDateString).AppendLine()
                errMsg = retMessage.ToString
                Return retMessageNo
            ElseIf reportDateString < outputDateString Then
                retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                retMessage.AppendFormat("・{0}：{1}", "報告日", "出力日より過去の入力です！").AppendLine()
                retMessage.AppendFormat("--> {0} = {1}", "報告日", reportDateString).AppendLine()
                errMsg = retMessage.ToString
                Return retMessageNo
            End If

            writeDr.Item("REPORTDATE") = reportDateString

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
                If {"DPIN", "ETYD", "DOUT", "CYIN"}.Contains(fieldName) Then
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
                    If dateString <> "" Then
                        '日付入力したACTYをもとに他の日付を連鎖して更新
                        Dim rowNum As String = Convert.ToString(writeDr.Item("LINECNT"))
                        Dim txtBoxName As String = String.Format("txt{0}{1}Dummy", Me.WF_LISTAREA.ID, fieldName)
                        UpdateDatatableDate(dateString, txtBoxName, rowNum, writeDt)
                    End If

                    Continue For '日付処理は終了のため後続処理へ
                End If '日付項目処理

                'チェック項目の場合
                If {"CHECK_DPIN", "CHECK_ETYD", "CHECK_DOUT", "CHECK_CYIN"}.Contains(fieldName) Then
                    Dim rowNum As String = Convert.ToString(writeDr.Item("LINECNT"))
                    Dim txtBoxName As String = String.Format("txt{0}{1}Dummy", Me.WF_LISTAREA.ID, fieldName)
                    UpdateDatatable(dr.Item(fieldName).ToString, txtBoxName, rowNum, writeDt)

                    Continue For 'チェック項目処理は終了のため後続処理へ
                End If 'チェック項目項目処理

                ''Excelに入力した値をコピー
                'writeDr.Item(fieldName) = dr.Item(fieldName)
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
#End Region

#Region "<< OV更新処理 >>"

    ''' <summary>
    ''' 変更検知処理
    ''' </summary>
    ''' <returns>変更対象のデータテーブルを生成</returns>
    ''' <remarks>当処理の戻り値データテーブルが更新・追加・論理削除対象のデータとなる</remarks>
    Private Function GetModifiedDataTable() As List(Of DataRow)
        Dim COA0021ListTable As New COA0021ListTable

        Dim currentDt As DataTable
        Dim firstTimeDt As DataTable
        '**************************************************
        'データテーブル復元
        '**************************************************
        '画面編集しているデータテーブル取得
        If Me.SavedDt Is Nothing Then
            currentDt = CreateListDataTable()
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
            firstTimeDt = CreateListDataTable()
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
        Dim updateTargetList = currentDt.AsEnumerable
        Dim compareFieldList As New List(Of String) From {"DPIN", "ETYD", "DOUT", "CYIN",
                                                          "CHECK_DPIN", "CHECK_ETYD", "CHECK_DOUT", "CHECK_CYIN",
                                                          "UPDATE_DPIN", "UPDATE_ETYD", "UPDATE_DOUT", "UPDATE_CYIN"}

        Dim updRowList As New List(Of DataRow)
        Dim updRow As DataRow
        For Each tgtDr In updateTargetList
            Dim tankNo As String = Convert.ToString(tgtDr.Item("TANKNO"))
            Dim compareDr = (From fstDr In firstTimeDt Where Convert.ToString(fstDr.Item("TANKNO")) = tankNo).FirstOrDefault
            Dim hasUnmatch As Boolean = False
            For Each fieldName As String In compareFieldList
                If compareDr Is Nothing OrElse
                Not tgtDr(fieldName).Equals(compareDr(fieldName)) Then
                    tgtDr.Item("UPDATE_" & fieldName.Replace("CHECK_", "").Replace("UPDATE_", "")) = "1"
                    hasUnmatch = True
                End If
            Next
            If hasUnmatch = True Then
                updRow = currentDt.NewRow
                updRow.ItemArray = tgtDr.ItemArray
                updRowList.Add(updRow)
            End If
        Next

        Return updRowList
    End Function

    ''' <summary>
    ''' 更新用のデータテーブルを作成
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
            Dim colList As New List(Of String) From {"ACTION", "ORDERNO", "TANKNO", "ACTIONID",
                                                     "SCHEDELDATE", "ACTUALDATE", "TANKCONDITION",
                                                     "UPDYMD", "UPDUSER", "UPDTERMID"
            }
            For Each colName As String In colList
                .Add(colName, GetType(String)).DefaultValue = ""
            Next
        End With
        Return retDt
    End Function

    ''' <summary>
    ''' 更新用Data取得
    ''' </summary>
    ''' <returns>変更対象のデータテーブルを生成</returns>
    ''' <remarks>当処理の戻り値データテーブルが更新・追加・論理削除対象のデータとなる</remarks>
    Private Function GetUpdateData(tgtDt As List(Of DataRow)) As List(Of DataRow)

        Dim updDt As DataTable = CreateOrderListTable()
        updDt.Columns.Add("MODIFIED", GetType(ModifyType))
        Dim updRowList As New List(Of DataRow)

        Dim updFieldList As String() = {"UPDATE_DPIN", "UPDATE_ETYD",
                                        "UPDATE_DOUT", "UPDATE_CYIN"}
        For Each tgtDr In tgtDt
            Dim tankNo As String = Convert.ToString(tgtDr.Item("TANKNO"))
            Dim updDate As String = tgtDr("REPORTDATE").ToString
            If updDate = "" Then
                updDate = Today().ToShortDateString
            End If

            For Each fieldName As String In updFieldList
                If tgtDr(fieldName).Equals("1") Then
                    Dim updRow = updDt.NewRow
                    updRow.Item("LINECNT") = tgtDr("LINECNT")

                    Dim fieldId = fieldName.Replace("UPDATE_", "")
                    Dim fieldDate = tgtDr(fieldId).ToString

                    updRow.Item("DATAID") = tgtDr("DATAID_" & fieldId)
                    updRow.Item("ACTIONID") = fieldId

                    If {"DPIN", "ETYD"}.Contains(fieldId) Then
                        updRow.Item("ORDERNO") = tgtDr("IMPORDERNO")
                    Else
                        updRow.Item("ORDERNO") = tgtDr("EXPORDERNO")
                    End If
                    updRow.Item("TANKNO") = tgtDr("TANKNO")
                    If fieldDate <> "1900/01/01" Then
                        '未来日なら予定日更新
                        If fieldDate > updDate Then
                            updRow.Item("SCHEDELDATE") = tgtDr(fieldId)
                            updRow.Item("TANKCONDITION") = ""
                        Else
                            updRow.Item("ACTUALDATE") = tgtDr(fieldId)
                            updRow.Item("TANKCONDITION") = tgtDr("CHECK_" & fieldId)
                        End If
                    Else
                        updRow.Item("SCHEDELDATE") = "1900/01/01"
                        updRow.Item("ACTUALDATE") = "1900/01/01"
                        updRow.Item("TANKCONDITION") = ""
                    End If
                    updRow.Item("UPDYMD") = tgtDr("UPDYMD_" & fieldId)
                    updRowList.Add(updRow)

                    'ETYD時はDLRYも更新
                    If {"ETYD"}.Contains(fieldId) Then
                        Dim dlry = updDt.NewRow()
                        dlry.ItemArray = updRow.ItemArray

                        fieldId = "DLRY"
                        updRow.Item("DATAID") = tgtDr("DATAID_" & fieldId)
                        updRow.Item("UPDYMD") = tgtDr("UPDYMD_" & fieldId)
                        updRowList.Add(dlry)
                    End If
                End If
            Next
        Next

        Return updRowList
    End Function
    ''' <summary>
    ''' オーダー明細にデータを登録
    ''' </summary>
    ''' <param name="targetData"></param>
    ''' <returns></returns>
    ''' <remarks>それぞれ追加・更新・タンク更新・削除の処理へ飛ばす</remarks>
    Private Function EntryOrderValue(targetData As List(Of DataRow)) As ProcMessage
        Dim retMessage As New ProcMessage
        Dim modOtherUser As New List(Of DataRow) '他ユーザー更新により登録不可のレコードを保持

        Dim procDate As Date = Date.Now '更新日時保持用(1度の更新処理での時刻は合わせるため)
        Dim messageNo As String = C_MESSAGENO.RIGHTBIXOUT
        'ログファイル書き込み共通機能の変動しないプロパティを設定
        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
        COA0003LogFile.MESSAGENO = messageNo

        'DB接続の生成
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            For Each item In targetData
                '他ユーザー更新チェック
                If CheckUpdateOtherUsers(item, sqlCon) = False Then
                    modOtherUser.Add(item)
                    Continue For
                End If
                Try
                    UpdateOrderValue(item, sqlCon, procDate:=procDate)
                Catch ex As Exception
                    COA0003LogFile.TEXT = String.Format("オーダー費用明細 更新時エラー:DATAID({0}" & ControlChars.CrLf & "{1}",
                                                        item("DATAID"),
                                                        ex.ToString())
                    COA0003LogFile.TEXT = ex.ToString()
                    COA0003LogFile.COA0003WriteLog()
                End Try

            Next
        End Using
        '処理結果に応じ左ボックス用のメッセージを表示
        If modOtherUser.Count = 0 Then
            '全て正常の場合
            retMessage.MessageNo = C_MESSAGENO.NORMALDBENTRY

        Else
            retMessage.MessageNo = C_MESSAGENO.RIGHTBIXOUT
            retMessage.modOtherUsers = modOtherUser
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

        'TODO更新チェックは他のフィールドになる想定なので要変更
        sqlStat.AppendLine("SELECT TIMSTP = cast(VL.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
        'sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
        'sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
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
                    If Convert.ToString(targetDr.Item("UPDYMD")).TrimEnd = Convert.ToString(sqlDr("UPDYMD")).TrimEnd Then
                        Return True
                    End If
                End While
            End Using
        End Using
        'ここまで来てReturnしていない場合は比較結果不一致のため他ユーザー更新
        Return False
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
            sqlStat.AppendLine("     ,CURRENCYSEGMENT")
            sqlStat.AppendLine("     ,ACCCRERATE")
            sqlStat.AppendLine("     ,ACCCREYEN")
            sqlStat.AppendLine("     ,ACCCREFOREIGN")
            sqlStat.AppendLine("     ,ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("     ,FORCECLOSED")
            sqlStat.AppendLine("     ,AMOUNTFIXBFC")
            sqlStat.AppendLine("     ,ACCCREYENBFC")
            sqlStat.AppendLine("     ,ACCCREFOREIGNBFC")
            sqlStat.AppendLine("     ,TANKCONDITION")
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
            sqlStat.AppendLine("         ,CURRENCYSEGMENT")
            sqlStat.AppendLine("         ,ACCCRERATE")
            sqlStat.AppendLine("         ,ACCCREYEN")
            sqlStat.AppendLine("         ,ACCCREFOREIGN")
            sqlStat.AppendLine("         ,ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("         ,FORCECLOSED")
            sqlStat.AppendLine("         ,AMOUNTFIXBFC")
            sqlStat.AppendLine("         ,ACCCREYENBFC")
            sqlStat.AppendLine("         ,ACCCREFOREIGNBFC")
            sqlStat.AppendLine("         ,TANKCONDITION")
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
            sqlStat.AppendLine("    SET SCHEDELDATE   = @SCHEDELDATE")
            sqlStat.AppendLine("       ,ACTUALDATE    = @ACTUALDATE")
            sqlStat.AppendLine("       ,TANKCONDITION = @TANKCONDITION")
            sqlStat.AppendLine("       ,DELFLG        = '" & CONST_FLAG_NO & "'")
            sqlStat.AppendLine("       ,UPDYMD        = @UPDYMD")
            sqlStat.AppendLine("       ,UPDUSER       = @UPDUSER")
            sqlStat.AppendLine("       ,UPDTERMID     = @UPDTERMID")
            sqlStat.AppendLine("       ,RECEIVEYMD    = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE DATAID = @DATAID;")

        End If
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@SCHEDELDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("SCHEDELDATE")))
                .Add("@ACTUALDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ACTUALDATE")))
                .Add("@TANKCONDITION", SqlDbType.NVarChar).Value = dr.Item("TANKCONDITION")
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
#End Region

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
        Dim orderNo As String = Convert.ToString(targetDr("IMPORDERNO"))
        Dim tankNo As String = Convert.ToString(targetDr("TANKNO"))
        Dim attrUniqueId As String = String.Format("{0}\{1}", orderNo, tankNo.Replace("/", ""))

        '*********************************
        '添付ファイルユーザー作業領域のクリア
        '*********************************
        CommonFunctions.CleanUserTempDirectory(CONST_MAPID)
        '*********************************
        '保存済みの添付ファイル一覧の取得、画面設定
        '*********************************
        Dim dtAttachment As DataTable = CommonFunctions.GetInitAttachmentFileList(attrUniqueId, CONST_DIRNAME_APPEARANCE_UPROOT, CONST_MAPID)
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
        Me.lblAttachTankNoTitle.Text = "TankNo.(OrderNo)"
        Me.lblAttachTankNo.Text = tankNo & "(" & orderNo & ")"
        '表示スタイル設定
        Me.divAttachmentInputAreaWapper.Style.Remove("display")
        Me.divAttachmentInputAreaWapper.Style.Add("display", "block")
    End Sub
    ''' <summary>
    ''' 添付ファイルアップロード処理
    ''' </summary>
    Private Sub UploadAttachment()
        Dim attrUniqueId As String = Convert.ToString(ViewState(CONST_VS_ATTA_UNIQUEID))
        Dim dtAttachment As DataTable = Me.dtCurAttachment
        Dim chkMsgNo = CommonFunctions.CheckUploadAttachmentFile(dtAttachment)
        If chkMsgNo <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(chkMsgNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        dtAttachment = CommonFunctions.UploadAttachmentFile(dtAttachment, attrUniqueId, CONST_MAPID)
        repAttachment.DataSource = dtAttachment
        repAttachment.DataBind()
        Me.dtCurAttachment = dtAttachment
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
    ''' 添付ファイルボックスのOKボタン押下時イベント
    ''' </summary>
    Public Sub btnAttachmentUploadOk_Click()
        '添付ファイルに動きがあったかチェック
        If HasModifiedAttachmentFile() Then
            Dim attaUniqueIdx As String = Convert.ToString(ViewState(CONST_VS_ATTA_UNIQUEID))
            '動きがある場合添付ファイルを正式フォルダに転送
            CommonFunctions.SaveAttachmentFilesList(Me.dtCurAttachment, attaUniqueIdx, CONST_DIRNAME_APPEARANCE_UPROOT)
        End If

        'マルチライン入力ボックスの非表示
        Me.divAttachmentInputAreaWapper.Style("display") = "none"

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
        Dim orderNo As String = Convert.ToString(dr("IMPORDERNO")).Replace("/", "")
        Dim tankNo As String = Convert.ToString(dr("TANKNO"))

        '対象のファイル有無取得
        Dim upBaseDir As String = COA0019Session.UPLOADFILESDir
        Dim uploadPath As String = IO.Path.Combine(upBaseDir, CONST_DIRNAME_APPEARANCE_UPROOT, orderNo, tankNo)
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
        dr("ATTACHMENT") = String.Format("{0} File", filesObj.Count)
    End Sub

    ''' <summary>
    ''' 添付ファイルの変更有無チェック
    ''' </summary>
    ''' <returns>True:変更あり,False:変更なし</returns>
    Private Function HasModifiedAttachmentFile() As Boolean
        '添付ファイルの個数判定
        Dim prevAttachDt As DataTable = DirectCast(ViewState(CONST_VS_PREV_ATTACHMENTINFO), DataTable)
        Dim dispAttachDt = Me.dtCurAttachment

        With Nothing
            Dim dispAttachFileCnt As Integer = 0
            Dim prevAttachFileCnt As Integer = 0
            If dispAttachDt IsNot Nothing Then
                dispAttachFileCnt = dispAttachDt.Rows.Count
            End If
            If prevAttachDt IsNot Nothing Then
                prevAttachFileCnt = prevAttachDt.Rows.Count
            End If
            If prevAttachFileCnt <> dispAttachFileCnt Then
                '添付ファイルの数値が合わない場合は変更あり
                Return True
            End If
        End With
        'フィールド変更チェック
        Dim chkAttachFields As New List(Of String) From {"FILENAME", "DELFLG", "ISMODIFIED"}
        Dim maxRowIdx As Integer = dispAttachDt.Rows.Count - 1
        For rowIdx = 0 To maxRowIdx Step 1
            Dim dispDr As DataRow = dispAttachDt.Rows(rowIdx)
            Dim prevDr As DataRow = prevAttachDt.Rows(rowIdx)
            For Each fieldName In chkAttachFields
                If Not dispDr(fieldName).Equals(prevDr(fieldName)) Then
                    '対象フィールドの値に変更があった場合
                    Return True
                End If
            Next fieldName 'フィールドループ
        Next 'データテーブル行ループ
        'ここまでくれば変更なし
        Return False
    End Function

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
            Dim deleteFlg As TextBox = DirectCast(repItem.FindControl("txtDeleteFlg"), TextBox)
            If fileName Is Nothing OrElse deleteFlg Is Nothing Then
                Continue For
            End If
            Dim qAttachment = From attachmentItem In dtAttachment Where attachmentItem("FILENAME").Equals(fileName.Text)
            If qAttachment.Any Then
                qAttachment.FirstOrDefault.Item("DELFLG") = deleteFlg.Text
                'qAttachment.FirstOrDefault.Item("ISMODIFIED") = CONST_FLAG_YES
            End If
        Next

        Return dtAttachment
    End Function


#End Region

#Region "<< その他書式編集 >>"
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
        Dim checkMapId As String = mapId
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

        Next 'END For Each dr As DataRow In dt.Rows
        errMessage = retMessage.ToString
        Return retMessageNo
    End Function

    ''' <summary>
    ''' LIST登録チェック
    ''' </summary>
    ''' <param name="inText"></param>
    ''' <param name="inList"></param>
    Protected Function ChedckList(ByVal inText As String, ByVal inList As ListBox, ByVal textNm As String, ByRef errMessage As String) As String
        Dim flag As Boolean = False
        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim dummyLabelObj As New Label '画面描画しないダミーのラベルオブジェクト
        Dim retMessage As New StringBuilder

        If inText <> "" Then

            For i As Integer = 0 To inList.Items.Count - 1
                If inList.Items(i).Text = inText Then
                    flag = True
                    Exit For
                End If
            Next

            If (flag = False) Then
                retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                CommonFunctions.ShowMessage(C_MESSAGENO.INVALIDINPUT, dummyLabelObj)
                retMessage.AppendFormat("・{0}：{1}", textNm, dummyLabelObj.Text).AppendLine()
            End If
        End If

        errMessage = retMessage.ToString
        Return retMessageNo
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
#End Region

End Class