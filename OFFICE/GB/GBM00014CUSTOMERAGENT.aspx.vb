Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Net
Imports BASEDLL

''' <summary>
''' 積載品マスタ画面クラス
''' </summary>
Public Class GBM00014CUSTOMERAGENT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBM00014"    '自身のMAPID
    Private Const CONST_BASEDATATABLE = "GBM00014TBL"
    Private Const CONST_JNRDATATABLE = "GBM00014JNRTBL"
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_TBLMASTER = "GBM0014_CUSTOMERAGENT"

    Private returnCode As String = String.Empty         'サブ用リターンコード

    Private GBM00014row As DataRow                      '行のロウデータ
    Private GBM00014CustAgentrow As DataRow             '行のロウデータ

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile

    ''' <summary>
    ''' 一覧格納用テーブル
    ''' </summary>
    Private BASEtbl As DataTable
    ''' <summary>
    ''' ジャーナル用テーブル
    ''' </summary>
    Private JNRtbl As DataTable
    ''' <summary>
    ''' 行のロウデータ
    ''' </summary>
    Private WORKrow As DataRow

    ''' <summary>
    ''' ページロード時処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            COA0003LogFile = New COA0003LogFile                             'ログ出力
            Dim COA0007getCompanyInfo As New BASEDLL.COA0007CompanyInfo     '会社情報取得
            Dim COA0013TableObject As New BASEDLL.COA0013TableObject
            Dim COA0021ListTable As New BASEDLL.COA0021ListTable
            Dim COA0031ProfMap As New BASEDLL.COA0031ProfMap

            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

            '画面モード（更新・参照）設定
            If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) = "2" Then
                hdnMAPpermitCode.Value = "TRUE"
            Else
                hdnMAPpermitCode.Value = "FALSE"
            End If

            'リターンコード設定
            returnCode = C_MESSAGENO.NORMAL

            '作業用データベース設定
            BASEtbl = New DataTable(CONST_BASEDATATABLE)
            JNRtbl = New DataTable(CONST_JNRDATATABLE)

            '****************************************
            'メッセージ初期化
            '****************************************
            Me.lblFooterMessage.Text = ""
            Me.lblFooterMessage.ForeColor = Color.Black
            Me.lblFooterMessage.Font.Bold = False

            '代理店名初期設定
            lblAgentName.Text = Convert.ToString(HttpContext.Current.Session(CONST_MAPID & "_AGENTNAME"))

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                Me.hdnViewVariant.Value = Convert.ToString(HttpContext.Current.Session(CONST_MAPID & "_VIEWID"))
                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFile.Value = COA0019Session.XMLDir & "\" & Date.Now.ToString("yyyyMMdd") & "-" & COA0019Session.USERID & "-" & CONST_MAPID & "-" & Me.hdnThisMapVariant.Value & "-" & Date.Now.ToString("HHmmss") & ".txt"
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'タイトル設定
                '****************************************
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = Me.hdnThisMapVariant.Value
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                End If

                '****************************************
                '選択情報　設定処理
                '****************************************
                '右Boxへの値設定
                RightboxInit()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
                    Return
                End If

                '一覧表示項目取得
                GetListData()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
                    Return
                End If

                'Detail初期設定
                RepeaterInit()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
                    Return
                End If

                '一覧表示データ保存
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = BASEtbl
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                    Return
                End If

            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
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
                '**********************
                ' detailboxラジオボタン処理
                '**********************
                If hdnRepUse.Value IsNot Nothing AndAlso hdnRepUse.Value <> "" Then
                    rdoRpRepUse_Click()
                    hdnRepUse.Value = ""
                    hdnRepPosition.Value = ""
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
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定

            '****************************************
            'Close処理
            '****************************************
            BASEtbl.Dispose()
            BASEtbl = Nothing
            JNRtbl.Dispose()
            JNRtbl = Nothing

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
    ''' Rightbox初期化
    ''' </summary>
    Private Sub RightboxInit()
        'RightBOX情報設定
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget
        Dim COA0022ProfXls As New BASEDLL.COA0022ProfXls
        returnCode = C_MESSAGENO.NORMAL

        '初期化
        Me.txtRightErrorMessage.Text = ""

        'レポートID情報
        COA0022ProfXls.MAPID = CONST_MAPID
        COA0022ProfXls.COA0022getReportId()
        If COA0022ProfXls.ERR = C_MESSAGENO.NORMAL Then
            Try
                For i As Integer = 0 To DirectCast(COA0022ProfXls.REPORTOBJ, System.Web.UI.WebControls.ListBox).Items.Count - 1
                    lbRightList.Items.Add(New ListItem(DirectCast(COA0022ProfXls.REPORTOBJ, System.Web.UI.WebControls.ListBox).Items(i).Text, DirectCast(COA0022ProfXls.REPORTOBJ, System.Web.UI.WebControls.ListBox).Items(i).Value))
                Next
            Catch ex As Exception
            End Try
        Else
            returnCode = COA0022ProfXls.ERR
            Return
        End If

        'レポートID変数検索
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "REPORTID"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR <> C_MESSAGENO.NORMAL Then
            returnCode = COA0016VARIget.ERR
            Return
        End If

        'ListBox選択
        lbRightList.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To lbRightList.Items.Count - 1
            If lbRightList.Items(i).Value = COA0016VARIget.VALUE Then
                lbRightList.SelectedIndex = i
            End If
        Next

    End Sub
    ''' <summary>
    ''' 一覧データ取得
    ''' </summary>
    Protected Sub GetListData()
        COA0003LogFile = New COA0003LogFile                         'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort  'テーブルソート文字列取得
        returnCode = C_MESSAGENO.NORMAL

        Dim lastCustCode As String = ""

        'DataBase接続文字
        Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
        Dim SQLStr As String = Nothing
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing

        '画面表示用データ取得
        Try
            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = Me.hdnViewVariant.Value
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            'テーブル検索結果をテーブル退避
            BASEtblColumnsAdd(BASEtbl)

            'DataBase接続文字
            SQLcon = New SqlConnection(COA0019Session.DBcon)
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 "     SELECT isnull(rtrim(D.CUSTOMERCODE),'')      as CUSTOMERCODE ,   " _
               & "            isnull(rtrim(A.COMPCODE),'')          as COMPCODE ,       " _
               & "            isnull(rtrim(B.CODE),'')              as ORGCODE ,        " _
               & "            isnull(rtrim(D.NAMES),'')             as NAMES ,          " _
               & "            isnull(rtrim(D.NAMESEN),'')           as NAMESEN ,        " _
               & "            isnull(rtrim(C.AGENTCODE),'')         as AGENTCODE ,      " _
               & "            isnull(rtrim(C.BILLCODE),'')          as BILLCODE ,       " _
               & "            isnull(rtrim(C.TYPE01),'')            as TYPE01 ,         " _
               & "            isnull(rtrim(C.TYPE02),'')            as TYPE02 ,         " _
               & "            isnull(rtrim(C.TYPE03),'')            as TYPE03 ,         " _
               & "            isnull(rtrim(C.TYPE04),'')            as TYPE04 ,         " _
               & "            isnull(rtrim(C.TYPE05),'')            as TYPE05 ,         " _
               & "            isnull(rtrim(C.DELFLG),'0')           as DELFLG ,         " _
               & "            TIMSTP = cast(C.UPDTIMSTP  as bigint)                     " _
               & " FROM       GBM0004_CUSTOMER D                                        " _
               & " INNER JOIN COS0011_AUTHOR A                                          " _
               & "         ON A.USERID   = @P1                                          " _
               & "        and A.COMPCODE = @P2                                          " _
               & "        and (A.OBJECT   = 'ORG' OR 1=1)                                        " _
               & "        and A.STYMD   <= @P3                                          " _
               & "        and A.ENDYMD  >= @P3                                          " _
               & "        and A.DELFLG  <> '1'                                          " _
               & " INNER JOIN COS0010_ROLE B                                            " _
               & "         ON B.COMPCODE = A.COMPCODE                                   " _
               & "        and B.OBJECT   = A.OBJECT                                     " _
               & "        and B.ROLE     = A.ROLE                                       " _
               & "        and B.CODE     = @P4                                          " _
               & "        and B.STYMD   <= @P3                                          " _
               & "        and B.ENDYMD  >= @P3                                          " _
               & "        and B.DELFLG  <> '1'                                          " _
               & "  LEFT JOIN GBM0014_CUSTOMERAGENT C                                   " _
               & "         ON C.COMPCODE = B.COMPCODE                                   " _
               & "        and C.CUSTOMERCODE = D.CUSTOMERCODE                           " _
               & "        and C.AGENTCODE    = B.CODE                                   " _
               & "        and C.DELFLG  <> '1'                                          " _
               & "      WHERE D.STYMD   <= @P3                                          " _
               & "        and D.ENDYMD  >= @P5                                          " _
               & "        and D.DELFLG  <> '1'                                          " _
               & "  GROUP BY  D.CUSTOMERCODE, A.COMPCODE, B.CODE, D.NAMES, D.NAMESEN, C.AGENTCODE, C.BILLCODE, " _
               & "            C.TYPE01, C.TYPE02, C.TYPE03, C.TYPE04, C.TYPE05, C.DELFLG, C.UPDTIMSTP " _
               & "  ORDER BY  A.COMPCODE ASC, C.DELFLG DESC, D.CUSTOMERCODE ASC          "

            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 20)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            PARA1.Value = COA0019Session.USERID
            PARA2.Value = HttpContext.Current.Session(CONST_MAPID & "_COMPANY")
            PARA3.Value = Date.Now
            PARA4.Value = HttpContext.Current.Session(CONST_MAPID & "_AGENT")
            PARA5.Value = Date.Now.AddMonths(-1).Year.ToString("0000") & "/" & Date.Now.AddMonths(-1).Month.ToString("00") & "/01"
            SQLdr = SQLcmd.ExecuteReader()

            'BASEtbl値設定
            Dim dataCnt As Integer = -1
            While SQLdr.Read

                'テーブル初期化
                GBM00014row = BASEtbl.NewRow()

                'データ設定

                '固定項目
                GBM00014row("OPERATION") = ""
                If IsDBNull(SQLdr("TIMSTP")) Then
                    GBM00014row("TIMSTP") = "0"
                Else
                    GBM00014row("TIMSTP") = SQLdr("TIMSTP")
                End If

                GBM00014row("SELECT") = 1   '1:表示
                GBM00014row("HIDDEN") = 0   '0:表示

                '画面毎の設定項目
                GBM00014row("CUSTOMERCODE") = SQLdr("CUSTOMERCODE")
                GBM00014row("COMPCODE") = SQLdr("COMPCODE")
                GBM00014row("ORGCODE") = SQLdr("ORGCODE")
                GBM00014row("AGENTCODE") = SQLdr("AGENTCODE")
                GBM00014row("NAMES") = SQLdr("NAMES")
                GBM00014row("NAMESEN") = SQLdr("NAMESEN")
                GBM00014row("BILLCODE") = SQLdr("BILLCODE")
                GBM00014row("TYPE01") = SQLdr("TYPE01")
                GBM00014row("TYPE02") = SQLdr("TYPE02")
                GBM00014row("TYPE03") = SQLdr("TYPE03")
                GBM00014row("TYPE04") = SQLdr("TYPE04")
                GBM00014row("TYPE05") = SQLdr("TYPE05")
                GBM00014row("DELFLG") = SQLdr("DELFLG")

                '顧客コードがブレイク
                If Convert.ToString(GBM00014row("CUSTOMERCODE")) = lastCustCode Then
                    GBM00014row("SELECT") = 0
                Else
                    GBM00014row("SELECT") = 1
                    GBM00014row("HIDDEN") = 0   '0:表示
                    '前回キー保存
                    lastCustCode = Convert.ToString(GBM00014row("CUSTOMERCODE"))

                End If

                '抽出対象外の場合、名称取得、レコード追加しない
                If Convert.ToString(GBM00014row("SELECT")) = "1" Then

                    dataCnt = dataCnt + 1
                    GBM00014row("LINECNT") = dataCnt.ToString

                    BASEtbl.Rows.Add(GBM00014row)
                End If

            End While

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
            Return

        Finally
            'CLOSE
            If Not SQLdr Is Nothing Then
                SQLdr.Close()
            End If
            If Not SQLcmd Is Nothing Then
                SQLcmd.Dispose()
                SQLcmd = Nothing
            End If
            If Not SQLcon Is Nothing Then
                SQLcon.Close()
                SQLcon.Dispose()
                SQLcon = Nothing
            End If

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
                'Case Me.vLeftCal.ID
                '    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                '    If targetObject IsNot Nothing Then
                '        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                '        Me.hdnCalendarValue.Value = txtobj.Text

                '        Me.mvLeft.Focus()
                '    End If

            End Select
        End If

    End Sub
    ''' <summary>
    ''' リピーター初期処理
    ''' </summary>
    Private Sub RepeaterInit()

        Dim textStr As String = ""

        'Detail変数設定
        Dim TBLview As DataView = New DataView(BASEtbl)
        rpDetail.DataSource = TBLview
        rpDetail.DataBind()  'Bind処理記述を行っていないので空行だけ作成される。

        For i As Integer = 0 To rpDetail.Items.Count - 1

            Dim EnabledFLG As Boolean = Nothing

            '組織コード
            DirectCast(rpDetail.Items(i).FindControl("hdnOrgCode"), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(BASEtbl.Rows(i)("ORGCODE"))
            '代理店コード
            DirectCast(rpDetail.Items(i).FindControl("hdnAgentCode"), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(BASEtbl.Rows(i)("AGENTCODE"))
            '削除フラグ
            DirectCast(rpDetail.Items(i).FindControl("hdnDelFlg"), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(BASEtbl.Rows(i)("DELFLG"))
            '顧客コード
            DirectCast(rpDetail.Items(i).FindControl("lblRpCustCode"), System.Web.UI.WebControls.Label).Text = Convert.ToString(BASEtbl.Rows(i)("CUSTOMERCODE"))
            '顧客名称
            If (COA0019Session.LANGDISP = C_LANG.JA) Then
                DirectCast(rpDetail.Items(i).FindControl("lblRpCustName"), System.Web.UI.WebControls.Label).Text = Convert.ToString(BASEtbl.Rows(i)("NAMES"))
            Else
                DirectCast(rpDetail.Items(i).FindControl("lblRpCustName"), System.Web.UI.WebControls.Label).Text = Convert.ToString(BASEtbl.Rows(i)("NAMESEN"))
            End If

            '使用有無
            If (COA0019Session.LANGDISP = C_LANG.JA) Then
                DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOn"), System.Web.UI.WebControls.RadioButton).Text = " 使用 "
                DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOff"), System.Web.UI.WebControls.RadioButton).Text = " 未使用 "
            Else
                DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOn"), System.Web.UI.WebControls.RadioButton).Text = " Used "
                DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOff"), System.Web.UI.WebControls.RadioButton).Text = " Unused "
            End If
            DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOn"), System.Web.UI.WebControls.RadioButton).Attributes.Add("Onclick", "Rep_ButtonChange('rdoRpUseOn','" & i & "')")
            DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOff"), System.Web.UI.WebControls.RadioButton).Attributes.Add("Onclick", "Rep_ButtonChange('rdoRpUseOff','" & i & "')")
            If String.IsNullOrEmpty(Convert.ToString(BASEtbl.Rows(i)("AGENTCODE"))) OrElse Convert.ToString(BASEtbl.Rows(i)("DELFLG")) = "1" Then
                '運用部署が存在しない場合、または、削除フラグがONの場合、「未使用」に設定
                DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOn"), System.Web.UI.WebControls.RadioButton).Checked = False
                DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOff"), System.Web.UI.WebControls.RadioButton).Checked = True
                EnabledFLG = False

            Else
                '運用部署が存在する場合、「使用」に設定
                DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOn"), System.Web.UI.WebControls.RadioButton).Checked = True
                DirectCast(rpDetail.Items(i).FindControl("rdoRpUseOff"), System.Web.UI.WebControls.RadioButton).Checked = False
                EnabledFLG = True

            End If

            '請求先
            DirectCast(rpDetail.Items(i).FindControl("txtRpBillCode"), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(BASEtbl.Rows(i)("BILLCODE"))
            DirectCast(rpDetail.Items(i).FindControl("txtRpBillCode"), System.Web.UI.WebControls.TextBox).Enabled = EnabledFLG

            '取引タイプ01
            DirectCast(rpDetail.Items(i).FindControl("txtRpType01"), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(BASEtbl.Rows(i)("TYPE01"))
            DirectCast(rpDetail.Items(i).FindControl("txtRpType01"), System.Web.UI.WebControls.TextBox).Enabled = EnabledFLG

            '取引タイプ02
            DirectCast(rpDetail.Items(i).FindControl("txtRpType02"), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(BASEtbl.Rows(i)("TYPE02"))
            DirectCast(rpDetail.Items(i).FindControl("txtRpType02"), System.Web.UI.WebControls.TextBox).Enabled = EnabledFLG

            '取引タイプ03
            DirectCast(rpDetail.Items(i).FindControl("txtRpType03"), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(BASEtbl.Rows(i)("TYPE03"))
            DirectCast(rpDetail.Items(i).FindControl("txtRpType03"), System.Web.UI.WebControls.TextBox).Enabled = EnabledFLG

            '取引タイプ04
            DirectCast(rpDetail.Items(i).FindControl("txtRpType04"), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(BASEtbl.Rows(i)("TYPE04"))
            DirectCast(rpDetail.Items(i).FindControl("txtRpType04"), System.Web.UI.WebControls.TextBox).Enabled = EnabledFLG

            '取引タイプ05
            DirectCast(rpDetail.Items(i).FindControl("txtRpType05"), System.Web.UI.WebControls.TextBox).Text = Convert.ToString(BASEtbl.Rows(i)("TYPE05"))
            DirectCast(rpDetail.Items(i).FindControl("txtRpType05"), System.Web.UI.WebControls.TextBox).Enabled = EnabledFLG

        Next

        rpDetail.Visible = True

    End Sub
    ''' <summary>
    ''' Repeater ラジオボタン 処理
    ''' </summary>
    Private Sub rdoRpRepUse_Click()
        COA0003LogFile = New COA0003LogFile                         'ログ出力

        'Repeater ラジオボタンによる入力保護/解除
        Try
            'INDEX設定
            Dim Position As Integer = CInt(hdnRepPosition.Value)

            '○Repeater明細の操作行を判定する
            If hdnRepUse.Value = "rdoRpUseOn" Then
                '内部値の変更
                DirectCast(rpDetail.Items(Position).FindControl("hdnAgentCode"), System.Web.UI.WebControls.TextBox).Text =
                    DirectCast(rpDetail.Items(Position).FindControl("hdnOrgCode"), System.Web.UI.WebControls.TextBox).Text
                DirectCast(rpDetail.Items(Position).FindControl("hdnDelFlg"), System.Web.UI.WebControls.TextBox).Text = "0"

                '各項目を活性に変更
                DirectCast(rpDetail.Items(Position).FindControl("txtRpBillCode"), System.Web.UI.WebControls.TextBox).Enabled = True
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType01"), System.Web.UI.WebControls.TextBox).Enabled = True
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType02"), System.Web.UI.WebControls.TextBox).Enabled = True
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType03"), System.Web.UI.WebControls.TextBox).Enabled = True
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType04"), System.Web.UI.WebControls.TextBox).Enabled = True
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType05"), System.Web.UI.WebControls.TextBox).Enabled = True

                'フォーカス設定
                DirectCast(rpDetail.Items(Position).FindControl("rdoRpUseOn"), System.Web.UI.WebControls.RadioButton).Focus()
            Else
                '内部値の変更
                DirectCast(rpDetail.Items(Position).FindControl("hdnAgentCode"), System.Web.UI.WebControls.TextBox).Text = ""
                DirectCast(rpDetail.Items(Position).FindControl("hdnDelFlg"), System.Web.UI.WebControls.TextBox).Text = "1"

                '各項目を非活性に変更
                DirectCast(rpDetail.Items(Position).FindControl("txtRpBillCode"), System.Web.UI.WebControls.TextBox).Enabled = False
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType01"), System.Web.UI.WebControls.TextBox).Enabled = False
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType02"), System.Web.UI.WebControls.TextBox).Enabled = False
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType03"), System.Web.UI.WebControls.TextBox).Enabled = False
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType04"), System.Web.UI.WebControls.TextBox).Enabled = False
                DirectCast(rpDetail.Items(Position).FindControl("txtRpType05"), System.Web.UI.WebControls.TextBox).Enabled = False

                'フォーカス設定
                DirectCast(rpDetail.Items(Position).FindControl("rdoRpUseOff"), System.Web.UI.WebControls.RadioButton).Focus()
            End If


        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
            Return
        End Try

    End Sub
    ''' <summary>
    ''' DB更新ボタン押下時
    ''' </summary>
    Public Sub btnDbUpdate_Click()
        COA0003LogFile = New COA0003LogFile                         'ログ出力
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        Dim copyDataTable As New DataTable
        Dim dummyMsgBox As Label = Nothing
        Dim UpdFlg As Boolean = False
        Dim ErrFlg As Boolean = False

        'DataBase接続文字
        Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
        Dim SQLStr As String = Nothing
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing
        Dim SQLStr2 As String = Nothing
        Dim SQLcmd2 As New SqlCommand()
        Dim SQLdr2 As SqlDataReader = Nothing

        'Gridview追加処理
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
            BASEtbl.AcceptChanges()
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        'ジャーナル出力用テーブル準備
        JNRtbl_ColumnsAdd(JNRtbl)

        'DetailBoxをBASEtblへ退避
        DetailBoxToBASEtbl()

        txtRightErrorMessage.Text = ""

        'BASEtbl内容チェック
        BASEtblCheck()
        If returnCode <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
            Return
        End If

        Try

            SQLcon.Open() 'DataBase接続(Open)

            'DB更新前チェック
            '  ※同一Key全てのレコードが更新されていない事をチェックする

            For Each GBM0014row As DataRow In BASEtbl.Rows
                Select Case GBM0014row.RowState
                    'データに変更が存在するレコードのみ、以下処理を行う
                    Case DataRowState.Modified

                        ErrFlg = False

                        Try

                            '同一Keyレコードを抽出
                            SQLStr =
                               " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP " _
                             & " FROM GBM0014_CUSTOMERAGENT " _
                             & " WHERE CUSTOMERCODE     = @P01 " _
                             & "   and COMPCODE         = @P02 " _
                             & "   and AGENTCODE        = @P03 " _
                             & "   and DELFLG           <> '1' ; "

                            SQLcmd = New SqlCommand(SQLStr, SQLcon)
                            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.Char, 20)
                            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Char, 20)
                            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Char, 20)

                            PARA01.Value = GBM0014row("CUSTOMERCODE")
                            PARA02.Value = GBM0014row("COMPCODE")
                            PARA03.Value = GBM0014row("AGENTCODE")

                            SQLdr = SQLcmd.ExecuteReader()

                            While SQLdr.Read
                                If RTrim(Convert.ToString(GBM0014row("TIMSTP"))) <> Convert.ToString(SQLdr("TIMSTP")) Then

                                    'エラーレポート編集
                                    Dim errMessageStr As String = ""

                                    'メッセージ取得
                                    CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, dummyMsgBox)

                                    errMessageStr = "・" & dummyMsgBox.Text
                                    errMessageStr = errMessageStr & ControlChars.NewLine
                                    errMessageStr = errMessageStr & Me.ErrItemSet(GBM0014row)
                                    If txtRightErrorMessage.Text <> "" Then
                                        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                                    End If
                                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & errMessageStr

                                    ErrFlg = True

                                End If

                            End While

                        Finally
                            'CLOSE
                            If Not SQLdr Is Nothing Then
                                SQLdr.Close()
                            End If
                            If Not SQLcmd Is Nothing Then
                                SQLcmd.Dispose()
                                SQLcmd = Nothing
                            End If

                        End Try

                        'エラーが存在しない場合
                        If Not ErrFlg Then

                            Try

                                '更新フラグをON
                                UpdFlg = True

                                'ＤＢ更新
                                '　※エラーは処理されない

                                '更新SQL文･･･マスタへ更新
                                Dim dateNow As DateTime = Date.Now

                                SQLStr =
                                   " DECLARE @hensuu as bigint ;                                        " _
                                 & " set @hensuu = 0 ;                                                  " _
                                 & " DECLARE hensuu CURSOR FOR                                          " _
                                 & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                       " _
                                 & "     FROM GBM0014_CUSTOMERAGENT                                     " _
                                 & "     WHERE    CUSTOMERCODE  = @P01                                  " _
                                 & "       and    COMPCODE      = @P02                                  " _
                                 & "       and    AGENTCODE     = @P03 ;                                " _
                                 & "                                                                    " _
                                 & " OPEN hensuu ;                                                      " _
                                 & " FETCH NEXT FROM hensuu INTO @hensuu ;                              " _
                                 & " IF ( @@FETCH_STATUS = 0 )                                          " _
                                 & "    UPDATE GBM0014_CUSTOMERAGENT                                    " _
                                 & "       SET    BILLCODE      = @P04 ,                                " _
                                 & "              TYPE01        = @P05 ,                                " _
                                 & "              TYPE02        = @P06 ,                                " _
                                 & "              TYPE03        = @P07 ,                                " _
                                 & "              TYPE04        = @P08 ,                                " _
                                 & "              TYPE05        = @P09 ,                                " _
                                 & "              DELFLG        = @P10 ,                                " _
                                 & "              UPDYMD        = @P12 ,                                " _
                                 & "              UPDUSER       = @P13 ,                                " _
                                 & "              UPDTERMID     = @P14 ,                                " _
                                 & "              RECEIVEYMD    = @P15                                  " _
                                 & "     WHERE    CUSTOMERCODE  = @P01                                  " _
                                 & "       and    COMPCODE      = @P02                                  " _
                                 & "       and    AGENTCODE     = @P03 ;                                " _
                                 & " IF ( @@FETCH_STATUS <> 0 )                                         " _
                                 & "    INSERT INTO GBM0014_CUSTOMERAGENT                               " _
                                 & "             (CUSTOMERCODE ,                                        " _
                                 & "              COMPCODE ,                                            " _
                                 & "              AGENTCODE ,                                           " _
                                 & "              BILLCODE ,                                            " _
                                 & "              TYPE01 ,                                              " _
                                 & "              TYPE02 ,                                              " _
                                 & "              TYPE03 ,                                              " _
                                 & "              TYPE04 ,                                              " _
                                 & "              TYPE05 ,                                              " _
                                 & "              DELFLG ,                                              " _
                                 & "              INITYMD ,                                             " _
                                 & "              UPDYMD ,                                              " _
                                 & "              UPDUSER ,                                             " _
                                 & "              UPDTERMID ,                                           " _
                                 & "              RECEIVEYMD)                                           " _
                                 & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,    " _
                                 & "              @P11,@P12,@P13,@P14,@P15                          );  " _
                                 & " CLOSE hensuu ;                                                     " _
                                 & " DEALLOCATE hensuu ;                                                "

                                SQLcmd = New SqlCommand(SQLStr, SQLcon)
                                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.Char, 20)
                                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Char, 20)
                                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Char, 20)
                                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Char, 20)
                                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Char, 20)
                                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Char, 20)
                                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Char, 20)
                                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Char, 20)
                                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.Char, 20)
                                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.Char, 1)
                                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
                                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)
                                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.Char, 20)
                                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.Char, 30)
                                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.DateTime)

                                PARA01.Value = GBM0014row("CUSTOMERCODE")
                                PARA02.Value = GBM0014row("COMPCODE")
                                PARA03.Value = GBM0014row("ORGCODE")
                                PARA04.Value = GBM0014row("BILLCODE")
                                PARA05.Value = GBM0014row("TYPE01")
                                PARA06.Value = GBM0014row("TYPE02")
                                PARA07.Value = GBM0014row("TYPE03")
                                PARA08.Value = GBM0014row("TYPE04")
                                PARA09.Value = GBM0014row("TYPE05")
                                PARA10.Value = GBM0014row("DELFLG")
                                PARA11.Value = dateNow
                                PARA12.Value = dateNow
                                PARA13.Value = COA0019Session.USERID
                                PARA14.Value = HttpContext.Current.Session("APSRVname")
                                PARA15.Value = CONST_DEFAULT_RECEIVEYMD

                                SQLcmd.ExecuteNonQuery()

                                '更新ジャーナル追加
                                COA0030Journal.TABLENM = CONST_TBLMASTER
                                COA0030Journal.ACTION = "UPDATE_INSERT"

                                GBM00014CustAgentrow = JNRtbl.NewRow
                                GBM00014CustAgentrow("CUSTOMERCODE") = GBM0014row("CUSTOMERCODE")
                                GBM00014CustAgentrow("COMPCODE") = GBM0014row("COMPCODE")
                                GBM00014CustAgentrow("AGENTCODE") = GBM0014row("AGENTCODE")
                                GBM00014CustAgentrow("BILLCODE") = GBM0014row("BILLCODE")
                                GBM00014CustAgentrow("TYPE01") = GBM0014row("TYPE01")
                                GBM00014CustAgentrow("TYPE02") = GBM0014row("TYPE02")
                                GBM00014CustAgentrow("TYPE03") = GBM0014row("TYPE03")
                                GBM00014CustAgentrow("TYPE04") = GBM0014row("TYPE04")
                                GBM00014CustAgentrow("TYPE05") = GBM0014row("TYPE05")
                                GBM00014CustAgentrow("DELFLG") = GBM0014row("DELFLG")

                                COA0030Journal.ROW = GBM00014CustAgentrow
                                COA0030Journal.COA0030SaveJournal()
                                If COA0030Journal.ERR = C_MESSAGENO.NORMAL Then
                                Else
                                    CommonFunctions.ShowMessage(COA0030Journal.ERR, Me.lblFooterMessage)
                                    Return
                                End If

                                '更新結果(TIMSTP)再取得 …　連続処理を可能にする。
                                SQLStr2 =
                                           " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP " _
                                         & " FROM GBM0014_CUSTOMERAGENT " _
                                         & " WHERE CUSTOMERCODE  = @P01 " _
                                         & "   And COMPCODE      = @P02 " _
                                         & "   And AGENTCODE     = @P03 ;"

                                SQLcmd2 = New SqlCommand(SQLStr2, SQLcon)
                                Dim PARA1 As SqlParameter = SQLcmd2.Parameters.Add("@P01", System.Data.SqlDbType.Char, 20)
                                Dim PARA2 As SqlParameter = SQLcmd2.Parameters.Add("@P02", System.Data.SqlDbType.Char, 20)
                                Dim PARA3 As SqlParameter = SQLcmd2.Parameters.Add("@P03", System.Data.SqlDbType.Char, 20)

                                PARA1.Value = GBM0014row("CUSTOMERCODE")
                                PARA2.Value = GBM0014row("COMPCODE")
                                PARA3.Value = GBM0014row("AGENTCODE")

                                SQLdr2 = SQLcmd2.ExecuteReader()

                                While SQLdr2.Read
                                    GBM0014row("TIMSTP") = SQLdr2("TIMSTP")
                                End While

                            Finally
                                'CLOSE
                                If Not SQLdr Is Nothing Then
                                    SQLdr.Close()
                                End If
                                If Not SQLcmd Is Nothing Then
                                    SQLcmd.Dispose()
                                    SQLcmd = Nothing
                                End If
                                If Not SQLdr2 Is Nothing Then
                                    SQLdr2.Close()
                                End If
                                If Not SQLcmd2 Is Nothing Then
                                    SQLcmd2.Dispose()
                                    SQLcmd2 = Nothing
                                End If

                            End Try
                        End If

                    Case Else

                End Select
            Next

        Catch ex As Exception

            Dim O_ERR As String = C_MESSAGENO.EXCEPTION
            CommonFunctions.ShowMessage(O_ERR, Me.lblFooterMessage)

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = O_ERR
            COA0003LogFile.COA0003WriteLog()                             'ログ出力

            Return

        Finally
            'CLOSE
            If Not SQLcon Is Nothing Then
                SQLcon.Close()
                SQLcon.Dispose()
                SQLcon = Nothing
            End If

        End Try


        If UpdFlg Then
            'BASEtbl最新化
            GetListData()

            'GridViewデータをテーブルに保存
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = BASEtbl
            COA0021ListTable.COA0021saveListTable()
            If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return
            End If

        End If

        'Detail初期設定
        RepeaterInit()

        'メッセージ表示
        If txtRightErrorMessage.Text = "" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
        End If

    End Sub
    ''' <summary>
    ''' 終了ボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '自画面MAPIDより親MAP・URLを取得
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage)
            Return
        End If

        '画面遷移実行
        Server.Transfer(COA0011ReturnUrl.URL)

    End Sub
    ''' <summary>
    ''' BASEtbl更新
    ''' </summary>
    Private Sub DetailBoxToBASEtbl()
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get

        'DetailよりBASEtbl編集
        Dim sText As String = Nothing

        For i As Integer = 0 To rpDetail.Items.Count - 1

            '代理店コード
            sText = DirectCast(rpDetail.Items(i).FindControl("hdnAgentCode"), System.Web.UI.WebControls.TextBox).Text
            If sText <> BASEtbl.Rows(i)("AGENTCODE").ToString Then
                COA0008InvalidChar.CHARin = sText
                COA0008InvalidChar.COA0008RemoveInvalidChar()
                BASEtbl.Rows(i)("AGENTCODE") = COA0008InvalidChar.CHARout
                BASEtbl.Rows(i)("DELFLG") = DirectCast(rpDetail.Items(i).FindControl("hdnDelFlg"), System.Web.UI.WebControls.TextBox).Text
            End If

            '請求先
            sText = DirectCast(rpDetail.Items(i).FindControl("txtRpBillCode"), System.Web.UI.WebControls.TextBox).Text
            If sText <> BASEtbl.Rows(i)("BILLCODE").ToString Then
                COA0008InvalidChar.CHARin = sText
                COA0008InvalidChar.COA0008RemoveInvalidChar()
                BASEtbl.Rows(i)("BILLCODE") = COA0008InvalidChar.CHARout
            End If

            '取引タイプ01
            sText = DirectCast(rpDetail.Items(i).FindControl("txtRpType01"), System.Web.UI.WebControls.TextBox).Text
            If sText <> BASEtbl.Rows(i)("TYPE01").ToString Then
                COA0008InvalidChar.CHARin = sText
                COA0008InvalidChar.COA0008RemoveInvalidChar()
                BASEtbl.Rows(i)("TYPE01") = COA0008InvalidChar.CHARout
            End If

            '取引タイプ02
            sText = DirectCast(rpDetail.Items(i).FindControl("txtRpType02"), System.Web.UI.WebControls.TextBox).Text
            If sText <> BASEtbl.Rows(i)("TYPE02").ToString Then
                COA0008InvalidChar.CHARin = sText
                COA0008InvalidChar.COA0008RemoveInvalidChar()
                BASEtbl.Rows(i)("TYPE02") = COA0008InvalidChar.CHARout
            End If

            '取引タイプ03
            sText = DirectCast(rpDetail.Items(i).FindControl("txtRpType03"), System.Web.UI.WebControls.TextBox).Text
            If sText <> BASEtbl.Rows(i)("TYPE03").ToString Then
                COA0008InvalidChar.CHARin = sText
                COA0008InvalidChar.COA0008RemoveInvalidChar()
                BASEtbl.Rows(i)("TYPE03") = COA0008InvalidChar.CHARout
            End If

            '取引タイプ04
            sText = DirectCast(rpDetail.Items(i).FindControl("txtRpType04"), System.Web.UI.WebControls.TextBox).Text
            If sText <> BASEtbl.Rows(i)("TYPE04").ToString Then
                COA0008InvalidChar.CHARin = sText
                COA0008InvalidChar.COA0008RemoveInvalidChar()
                BASEtbl.Rows(i)("TYPE04") = COA0008InvalidChar.CHARout
            End If

            '取引タイプ05
            sText = DirectCast(rpDetail.Items(i).FindControl("txtRpType05"), System.Web.UI.WebControls.TextBox).Text
            If sText <> BASEtbl.Rows(i)("TYPE05").ToString Then
                COA0008InvalidChar.CHARin = sText
                COA0008InvalidChar.COA0008RemoveInvalidChar()
                BASEtbl.Rows(i)("TYPE05") = COA0008InvalidChar.CHARout
            End If

        Next

    End Sub
    ''' <summary>
    ''' 入力データチェック
    ''' </summary>
    Protected Sub BASEtblCheck()
        COA0003LogFile = New COA0003LogFile                         'ログ出力
        Dim COA0024Author As New BASEDLL.COA0024Author
        Dim dateNow As Date = Date.Now
        Dim errMessage As String = Nothing
        Dim errItemStr As String = Nothing
        Dim errFlg As Boolean = False

        returnCode = C_MESSAGENO.NORMAL

        For Each GB0014row As DataRow In BASEtbl.Rows
            Dim lowCnt As Integer = CInt(GB0014row("LINECNT"))
            'ボックスの背景色を初期化
            DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpBillCode"), System.Web.UI.WebControls.TextBox).Style.Remove("background-color")
            DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType01"), System.Web.UI.WebControls.TextBox).Style.Remove("background-color")
            DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType02"), System.Web.UI.WebControls.TextBox).Style.Remove("background-color")
            DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType03"), System.Web.UI.WebControls.TextBox).Style.Remove("background-color")
            DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType04"), System.Web.UI.WebControls.TextBox).Style.Remove("background-color")
            DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType05"), System.Web.UI.WebControls.TextBox).Style.Remove("background-color")


            Select Case GB0014row.RowState
                'データに変更が存在するレコードのみ、以下処理を行う
                Case DataRowState.Modified

                    '■■■ 単項目チェック(キー情報) ■■■

                    '権限チェック（更新権限）
                    If Convert.ToString(GB0014row("ORGCODE")) <> "" Then

                        '組織コード
                        COA0024Author.USERID = COA0019Session.USERID
                        COA0024Author.OBJCODE = "ORG"
                        COA0024Author.CODE = Convert.ToString(GB0014row("ORGCODE"))
                        COA0024Author.STYMD = dateNow
                        COA0024Author.ENDYMD = dateNow
                        COA0024Author.COA0024GetAuthor()
                        If COA0024Author.ERR <> C_MESSAGENO.NORMAL OrElse COA0024Author.PERMITCODE <> "2" Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.NOAUTHERROR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
                            errMessage = Me.lblFooterMessage.Text

                            errItemStr = Me.ErrItemSet(GB0014row)
                            If txtRightErrorMessage.Text <> "" Then
                                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                            End If
                            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & errMessage & ControlChars.NewLine & errItemStr
                            errFlg = True

                        End If
                    End If

                    '単項目チェック(明細情報)

                    '請求先
                    If CheckSingle(GB0014row, "BILLCODE", Me.lblBill.Text) <> C_MESSAGENO.NORMAL Then
                        'ボックスの背景色を変更&名称クリア
                        DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpBillCode"), System.Web.UI.WebControls.TextBox).Style.Add("background-color", "darksalmon")
                        DirectCast(rpDetail.Items(lowCnt).FindControl("lblRpBillName"), System.Web.UI.WebControls.Label).Text = ""
                        errFlg = True
                    End If

                    'タイプ１
                    If CheckSingle(GB0014row, "TYPE01", Me.lblType1.Text) <> C_MESSAGENO.NORMAL Then
                        'ボックスの背景色を変更&名称クリア
                        DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType01"), System.Web.UI.WebControls.TextBox).Style.Add("background-color", "darksalmon")
                        DirectCast(rpDetail.Items(lowCnt).FindControl("lblRpType01"), System.Web.UI.WebControls.Label).Text = ""
                        errFlg = True
                    End If

                    'タイプ２
                    If CheckSingle(GB0014row, "TYPE02", Me.lblType2.Text) <> C_MESSAGENO.NORMAL Then
                        'ボックスの背景色を変更&名称クリア
                        DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType02"), System.Web.UI.WebControls.TextBox).Style.Add("background-color", "darksalmon")
                        DirectCast(rpDetail.Items(lowCnt).FindControl("lblRpType02"), System.Web.UI.WebControls.Label).Text = ""
                        errFlg = True
                    End If

                    'タイプ３
                    If CheckSingle(GB0014row, "TYPE03", Me.lblType3.Text) <> C_MESSAGENO.NORMAL Then
                        'ボックスの背景色を変更&名称クリア
                        DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType03"), System.Web.UI.WebControls.TextBox).Style.Add("background-color", "darksalmon")
                        DirectCast(rpDetail.Items(lowCnt).FindControl("lblRpType03"), System.Web.UI.WebControls.Label).Text = ""
                        errFlg = True
                    End If

                    'タイプ４
                    If CheckSingle(GB0014row, "TYPE04", Me.lblType4.Text) <> C_MESSAGENO.NORMAL Then
                        'ボックスの背景色を変更&名称クリア
                        DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType04"), System.Web.UI.WebControls.TextBox).Style.Add("background-color", "darksalmon")
                        DirectCast(rpDetail.Items(lowCnt).FindControl("lblRpType04"), System.Web.UI.WebControls.Label).Text = ""
                        errFlg = True
                    End If

                    'タイプ５
                    If CheckSingle(GB0014row, "TYPE05", Me.lblType5.Text) <> C_MESSAGENO.NORMAL Then
                        'ボックスの背景色を変更&名称クリア
                        DirectCast(rpDetail.Items(lowCnt).FindControl("txtRpType05"), System.Web.UI.WebControls.TextBox).Style.Add("background-color", "darksalmon")
                        DirectCast(rpDetail.Items(lowCnt).FindControl("lblRpType05"), System.Web.UI.WebControls.Label).Text = ""
                        errFlg = True
                    End If

                Case Else

                    Continue For

            End Select
        Next

        If errFlg Then
            returnCode = C_MESSAGENO.RIGHTBIXOUT
        Else
            returnCode = C_MESSAGENO.NORMAL
        End If

    End Sub
    ''' <summary>
    ''' エラーキー情報出力
    ''' </summary>
    ''' <param name="argRow"></param>
    ''' <returns></returns>
    Private Function ErrItemSet(ByVal argRow As DataRow) As String
        Dim rtc As String = String.Empty

        If (COA0019Session.LANGDISP <> C_LANG.JA) Then
            rtc &= ControlChars.NewLine & "  --> Customer Code   =" & Convert.ToString(argRow("CUSTOMERCODE")) & " "
        Else
            rtc &= ControlChars.NewLine & "  --> 顧客コード      =" & Convert.ToString(argRow("CUSTOMERCODE")) & " "
        End If

        Return rtc

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
                'ビューごとの処理はケースを追加で実現
                'Case Me.vLeftCal.ID 'アクティブなビューがカレンダー
                '    'カレンダー選択時
                '    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                '    If targetObject IsNot Nothing Then
                '        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                '        txtobj.Text = Me.hdnCalendarValue.Value
                '        txtobj.Focus()
                '    End If
                Case Else
                    '何もしない
            End Select
        End If
        '画面左サイドボックス非表示は、画面JavaScriptで実行
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
        '画面左サイドボックス非表示は、画面JavaScriptで実行
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

        'ラベル等やグリッドを除くの文言設定(適宜追加) リピーターの表ヘッダーもこの方式で可能ですので
        '作成者に聞いてください。
        AddLangSetting(dicDisplayText, Me.lblAgent, "代理店:", "Agent:")

        AddLangSetting(dicDisplayText, Me.lblCustomer, "顧客", "Customer")
        AddLangSetting(dicDisplayText, Me.lblUse, "使用有無", "Used")
        AddLangSetting(dicDisplayText, Me.lblBill, "請求先", "Billing")
        AddLangSetting(dicDisplayText, Me.lblType1, "タイプ１", "Type1")
        AddLangSetting(dicDisplayText, Me.lblType2, "タイプ２", "Type2")
        AddLangSetting(dicDisplayText, Me.lblType3, "タイプ３", "Type3")
        AddLangSetting(dicDisplayText, Me.lblType4, "タイプ４", "Type4")
        AddLangSetting(dicDisplayText, Me.lblType5, "タイプ５", "Type5")

        AddLangSetting(dicDisplayText, Me.btnDbUpdate, "DB更新", "Update")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)

    End Sub

    ''' <summary>
    ''' 内部テーブルカラム設定
    ''' </summary>
    ''' <param name="table">内部テーブル</param>
    Protected Sub BASEtblColumnsAdd(table As DataTable)

        'DB項目クリア
        If table.Columns.Count = 0 Then
        Else
            table.Columns.Clear()
        End If
        table.Clear()

        '共通項目
        table.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        table.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        table.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        table.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        table.Columns.Add("HIDDEN", GetType(Integer))             'DBの固定フィールド

        '画面固有項目
        table.Columns.Add("ORGCODE", GetType(String))
        table.Columns.Add("CUSTOMERCODE", GetType(String))
        table.Columns.Add("COMPCODE", GetType(String))
        table.Columns.Add("AGENTCODE", GetType(String))
        table.Columns.Add("NAMES", GetType(String))
        table.Columns.Add("NAMESEN", GetType(String))
        table.Columns.Add("BILLCODE", GetType(String))
        table.Columns.Add("TYPE01", GetType(String))
        table.Columns.Add("TYPE02", GetType(String))
        table.Columns.Add("TYPE03", GetType(String))
        table.Columns.Add("TYPE04", GetType(String))
        table.Columns.Add("TYPE05", GetType(String))
        table.Columns.Add("DELFLG", GetType(String))

    End Sub
    ''' <summary>
    ''' 内部テーブルRow初期値設定
    ''' </summary>
    ''' <param name="argTbl">内部テーブル</param>
    Protected Sub DefaultToTBL(argTbl As DataTable)

        Dim workRow As DataRow                                      'デフォルト用のロウデータ

        workRow = argTbl.NewRow
        workRow("LINECNT") = 0                                      'DBの固定フィールド
        workRow("OPERATION") = ""                                   'DBの固定フィールド
        workRow("TIMSTP") = "0"                                     'DBの固定フィールド
        workRow("SELECT") = "0"                                     'DBの固定フィールド
        workRow("HIDDEN") = "0"                                     'DBの固定フィールド

        workRow("ORGCODE") = ""
        workRow("CUSTOMERCODE") = ""
        workRow("COMPCODE") = ""
        workRow("AGENTCODE") = ""
        workRow("NAMES") = ""
        workRow("NAMESEN") = ""
        workRow("BILLCODE") = ""
        workRow("TYPE01") = ""
        workRow("TYPE02") = ""
        workRow("TYPE03") = ""
        workRow("TYPE04") = ""
        workRow("TYPE05") = ""
        workRow("DELFLG") = ""

        argTbl.Rows.Add(workRow)

    End Sub
    ''' <summary>
    ''' ジャーナルテーブルRow初期値設定
    ''' </summary>
    ''' <param name="table"></param>
    Protected Sub JNRtbl_ColumnsAdd(table As DataTable)

        'DB項目クリア
        If table.Columns.Count = 0 Then
        Else
            table.Columns.Clear()
        End If
        table.Clear()

        '画面固有項目
        table.Columns.Add("CUSTOMERCODE", GetType(String))
        table.Columns.Add("COMPCODE", GetType(String))
        table.Columns.Add("AGENTCODE", GetType(String))
        table.Columns.Add("BILLCODE", GetType(String))
        table.Columns.Add("TYPE01", GetType(String))
        table.Columns.Add("TYPE02", GetType(String))
        table.Columns.Add("TYPE03", GetType(String))
        table.Columns.Add("TYPE04", GetType(String))
        table.Columns.Add("TYPE05", GetType(String))
        table.Columns.Add("DELFLG", GetType(String))

    End Sub
    ''' <summary>
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="argRow"></param>
    Protected Function CheckSingle(ByVal argRow As DataRow, ByVal itmKey As String, ByVal itmKeyName As String) As String
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar        '例外文字排除 String Get
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck          '項目チェック

        Dim errMessage As String = Nothing
        Dim errItemStr As String = Nothing
        Dim retCode As String = C_MESSAGENO.NORMAL

        '単項目チェック
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.FIELD = itmKey
        COA0026FieldCheck.VALUE = Convert.ToString(argRow(itmKey))
        COA0026FieldCheck.COA0026FieldCheck()
        If COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL Then

            retCode = COA0026FieldCheck.ERR
            CommonFunctions.ShowMessage(retCode, Me.lblFooterMessage)
            errMessage = Me.lblFooterMessage.Text

            errItemStr = Me.ErrItemSet(argRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If

            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                               & "・" & errMessage & "(" & itmKeyName & ")" & ControlChars.NewLine & errItemStr

        End If

        Return retCode

    End Function
End Class