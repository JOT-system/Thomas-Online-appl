Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' 承認処理関連クラス
''' </summary>
Public Class ApprovalProc
    ''' <summary>
    ''' 承認
    ''' </summary>
    ''' <returns></returns>
    Public Property Proc As ApprovalMasterClass
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="masterName">処理するマスタ名</param>
    Public Sub New(masterName As String)
        '当クラス名＋サブクラス名の完全修飾文字を生成
        Dim className As String = Me.[GetType]().AssemblyQualifiedName
        className = className.Replace(Me.[GetType]().FullName, Me.[GetType]().FullName & "+" & masterName)
        '生成したクラス名をもとに生成する型を生成
        Dim masterType As Type = Type.GetType(className)
        '生成した型を元にインスタンスを作成(New)
        Me.Proc = DirectCast(Activator.CreateInstance(masterType), ApprovalMasterClass)

    End Sub
    ''' <summary>
    ''' デストラクタ
    ''' </summary>
    Protected Overrides Sub Finalize()
        '一応プロパティの処理は破棄
        Proc = Nothing
        MyBase.Finalize()
    End Sub
    '********************************************
    '処理の基底クラス
    '********************************************
    ''' <summary>
    ''' 各マスタ処理の根底クラス
    ''' </summary>
    ''' <remarks>このガワにより呼び出し先にメソッドの候補（インテリセンス）が見える</remarks>
    Public MustInherit Class ApprovalMasterClass
        Public Sub New()

        End Sub

        '外部インターフェースで使用するクラス（ここで処理のないガワを作る）
        ''' <summary>
        ''' 空のデータテーブル作成処理
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function CreateDataTable() As DataTable
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function GetData(stYMD As String, endYMD As String) As DataTable
        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public MustOverride Sub MstDbUpdate(dtRow As DataRow)
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public MustOverride Sub ApplyMstDbUpdate(dtRow As DataRow)
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public MustOverride Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)


    End Class
    '********************************************
    'マスタ処理ごとのサブクラス
    '********************************************
    ''' <summary>
    ''' 積載品マスタ関連処理
    ''' </summary>
    Private Class GBM00008
        Inherits ApprovalMasterClass '基底クラスを継承
        '↓呼出し元で実行必要なメソッドはパブリックコープ+Overloadsにしておいてください
        '　（サブクラス内のみで済むのはPrivateでOK"
        Private Const CONST_MAPID As String = "GBM00008"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyProduct"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable

            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))              '固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))             '固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))                '固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))               '固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))               '固定フィールド
            '個別項目
            dt.Columns.Add("COMPCODE", GetType(String))              '会社コード
            dt.Columns.Add("PRODUCTCODE", GetType(String))           '積載品コード
            dt.Columns.Add("STYMD", GetType(String))                 '有効開始日
            dt.Columns.Add("ENDYMD", GetType(String))                '有効終了日
            dt.Columns.Add("PRODUCTNAME", GetType(String))           '製品名
            dt.Columns.Add("CHEMICALNAME", GetType(String))          '化学名
            dt.Columns.Add("IMDGCODE", GetType(String))              'ＩＭＤＧコード
            dt.Columns.Add("UNNO", GetType(String))                  '国連番号
            dt.Columns.Add("HAZARDCLASS", GetType(String))           '等級
            dt.Columns.Add("PACKINGGROUP", GetType(String))          '容器等級
            dt.Columns.Add("FIRESERVICEACT", GetType(String))        '消防法
            dt.Columns.Add("PANDDCONTROLACT", GetType(String))       '毒劇法
            dt.Columns.Add("CASNO", GetType(String))                 'CAS No.
            dt.Columns.Add("GRAVITY", GetType(String))               '比重
            dt.Columns.Add("FLASHPOINT", GetType(String))            '引火点
            dt.Columns.Add("TANKGRADE", GetType(String))             'タンクグレード
            dt.Columns.Add("PRPVISIONS", GetType(String))            '追加規定
            dt.Columns.Add("ENABLED", GetType(String))               '有効フラグ
            dt.Columns.Add("MANUFACTURE", GetType(String))           '製造者
            dt.Columns.Add("REMARK", GetType(String))                '備考
            dt.Columns.Add("DELFLG", GetType(String))                '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))        '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))      '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                 'チェック
            dt.Columns.Add("APPLYID", GetType(String))               '申請ID
            dt.Columns.Add("STEP", GetType(String))                  'ステップ
            dt.Columns.Add("STATUS", GetType(String))                'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))               '承認ステップ
            dt.Columns.Add("STEPSTATE", GetType(String))             'ステップ状況
            dt.Columns.Add("APPROVALTYPE", GetType(String))          '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))            '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))              'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable

            Dim dt As New DataTable
            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(PA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,PA.COMPCODE")
            sqlStat.AppendLine("      ,PA.PRODUCTCODE")
            sqlStat.AppendLine("      ,convert(nvarchar, PA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, PA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,PA.PRODUCTNAME")
            sqlStat.AppendLine("      ,PA.CHEMICALNAME")
            sqlStat.AppendLine("      ,PA.IMDGCODE")
            sqlStat.AppendLine("      ,PA.UNNO")
            sqlStat.AppendLine("      ,PA.HAZARDCLASS")
            sqlStat.AppendLine("      ,PA.PACKINGGROUP")
            sqlStat.AppendLine("      ,PA.FIRESERVICEACT")
            sqlStat.AppendLine("      ,PA.PANDDCONTROLACT")
            sqlStat.AppendLine("      ,PA.CASNO")
            sqlStat.AppendLine("      ,PA.GRAVITY")
            sqlStat.AppendLine("      ,PA.FLASHPOINT")
            sqlStat.AppendLine("      ,PA.TANKGRADE")
            sqlStat.AppendLine("      ,PA.PRPVISIONS")
            sqlStat.AppendLine("      ,PA.ENABLED")
            sqlStat.AppendLine("      ,PA.MANUFACTURE")
            sqlStat.AppendLine("      ,PA.REMARK")
            sqlStat.AppendLine("      ,PA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0011_PRODUCTAPPLY PA") '積載品マスタ(申請)
            sqlStat.AppendLine("    ON  PA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  PA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  PA.ENDYMD      >= @ENDYMD")
            'sqlStat.AppendLine("   AND  PA.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")

            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function
        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 積載品マスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0008_PRODUCT ")
                sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE ")
                sqlStat.AppendLine("   AND PRODUCTCODE = @PRODUCTCODE ")
                sqlStat.AppendLine("   AND STYMD = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0008_PRODUCT ")
                sqlStat.AppendLine("  SET ENDYMD            = @ENDYMD , ")
                sqlStat.AppendLine("      PRODUCTNAME       = @PRODUCTNAME , ")
                sqlStat.AppendLine("      CHEMICALNAME      = @CHEMICALNAME , ")
                sqlStat.AppendLine("      IMDGCODE          = @IMDGCODE , ")
                sqlStat.AppendLine("      UNNO              = @UNNO , ")
                sqlStat.AppendLine("      HAZARDCLASS       = @HAZARDCLASS , ")
                sqlStat.AppendLine("      PACKINGGROUP      = @PACKINGGROUP , ")
                sqlStat.AppendLine("      FIRESERVICEACT    = @FIRESERVICEACT , ")
                sqlStat.AppendLine("      PANDDCONTROLACT   = @PANDDCONTROLACT , ")
                sqlStat.AppendLine("      CASNO             = @CASNO , ")
                sqlStat.AppendLine("      GRAVITY           = @GRAVITY , ")
                sqlStat.AppendLine("      FLASHPOINT        = @FLASHPOINT , ")
                sqlStat.AppendLine("      TANKGRADE         = @TANKGRADE , ")
                sqlStat.AppendLine("      PRPVISIONS        = @PRPVISIONS , ")
                sqlStat.AppendLine("      ENABLED           = @ENABLED , ")
                sqlStat.AppendLine("      MANUFACTURE       = @MANUFACTURE , ")
                sqlStat.AppendLine("      REMARK            = @REMARK , ")
                sqlStat.AppendLine("      DELFLG            = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD            = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER           = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID         = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD        = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE         = @COMPCODE ")
                sqlStat.AppendLine("   AND PRODUCTCODE      = @PRODUCTCODE ")
                sqlStat.AppendLine("   AND STYMD = @STYMD ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0008_PRODUCT ( ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      PRODUCTCODE , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      PRODUCTNAME , ")
                sqlStat.AppendLine("      CHEMICALNAME , ")
                sqlStat.AppendLine("      IMDGCODE , ")
                sqlStat.AppendLine("      UNNO , ")
                sqlStat.AppendLine("      HAZARDCLASS , ")
                sqlStat.AppendLine("      PACKINGGROUP , ")
                sqlStat.AppendLine("      FIRESERVICEACT , ")
                sqlStat.AppendLine("      PANDDCONTROLACT , ")
                sqlStat.AppendLine("      CASNO , ")
                sqlStat.AppendLine("      GRAVITY , ")
                sqlStat.AppendLine("      FLASHPOINT , ")
                sqlStat.AppendLine("      TANKGRADE , ")
                sqlStat.AppendLine("      PRPVISIONS , ")
                sqlStat.AppendLine("      ENABLED , ")
                sqlStat.AppendLine("      MANUFACTURE , ")
                sqlStat.AppendLine("      REMARK , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine(" @COMPCODE,@PRODUCTCODE,@STYMD,@ENDYMD,@PRODUCTNAME,@CHEMICALNAME,@IMDGCODE,@UNNO,")
                sqlStat.AppendLine(" @HAZARDCLASS,@PACKINGGROUP,@FIRESERVICEACT,@PANDDCONTROLACT,@CASNO,@GRAVITY,@FLASHPOINT,")
                sqlStat.AppendLine(" @TANKGRADE,@PRPVISIONS,@ENABLED,@MANUFACTURE,@REMARK,@DELFLG,")
                sqlStat.AppendLine(" @INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@PRODUCTCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PRODUCTCODE"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.Date).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@PRODUCTNAME", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PRODUCTNAME"))
                        .Add("@CHEMICALNAME", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CHEMICALNAME"))
                        .Add("@IMDGCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("IMDGCODE"))
                        .Add("@UNNO", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("UNNO"))
                        .Add("@HAZARDCLASS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("HAZARDCLASS"))
                        .Add("@PACKINGGROUP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PACKINGGROUP"))
                        .Add("@FIRESERVICEACT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FIRESERVICEACT"))
                        .Add("@PANDDCONTROLACT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PANDDCONTROLACT"))
                        .Add("@CASNO", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CASNO"))
                        .Add("@GRAVITY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("GRAVITY"))
                        .Add("@FLASHPOINT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FLASHPOINT"))
                        .Add("@TANKGRADE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TANKGRADE"))
                        .Add("@PRPVISIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PRPVISIONS"))
                        .Add("@ENABLED", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENABLED"))
                        .Add("@MANUFACTURE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MANUFACTURE"))
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0008_PRODUCT"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

            Dim befAppDir As String = Nothing
            Dim officialDir As String = Nothing
            '承認前ディレクトリ
            befAppDir = COA0019Session.BEFOREAPPROVALDir & "\MSDS\" & Trim(Convert.ToString(dtRow.Item("PRODUCTCODE")))
            '正式ディレクトリ
            officialDir = COA0019Session.UPLOADFILESDir & "\MSDS\" & Trim(Convert.ToString(dtRow.Item("PRODUCTCODE")))

            'フォルダが存在する場合承認前から正式フォルダに移動
            If System.IO.Directory.Exists(befAppDir) Then

                'ディレクトリが存在しない場合、作成する
                If System.IO.Directory.Exists(officialDir) = False Then
                    System.IO.Directory.CreateDirectory(officialDir)
                Else
                    '格納フォルダクリア処理
                    For Each tempFile As String In System.IO.Directory.GetFiles(officialDir, "*", System.IO.SearchOption.AllDirectories)
                        'サブフォルダは対象外
                        Try
                            System.IO.File.Delete(tempFile)
                        Catch ex As Exception
                        End Try
                    Next
                End If

                '承認前フォルダのファイルをPDF正式格納フォルダへコピー
                For Each tempFile As String In System.IO.Directory.GetFiles(befAppDir, "*", System.IO.SearchOption.AllDirectories)
                    'ディレクトリ付ファイル名より、ファイル名編集
                    Dim fileName As String = tempFile
                    Do
                        If InStr(fileName, "\") > 0 Then
                            fileName = Mid(fileName, InStr(fileName, "\") + 1, 1024)
                        End If

                    Loop Until InStr(fileName, "\") <= 0

                    'Update_Hフォルダ内PDF→PDF正式格納フォルダへ上書コピー
                    System.IO.File.Copy(tempFile, officialDir & "\" & fileName, True)

                Next

                '集配信用フォルダ格納処理
                Dim COA00034SendDirectory As New COA00034SendDirectory
                Dim pgmDir As String = "\MSDS\" & Trim(Convert.ToString(dtRow.Item("PRODUCTCODE")))
                COA00034SendDirectory.SendDirectoryCopy(pgmDir, officialDir, "2")

            End If

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim befAppDir As String = Nothing
            Dim sendDir As String = Nothing
            Dim uplDir As String = ""
            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 積載品マスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0011_PRODUCTAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE      = @COMPCODE")
                sqlStat.AppendLine("   AND PRODUCTCODE   = @PRODUCTCODE")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("COMPCODE"))
                        .Add("@PRODUCTCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("PRODUCTCODE"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

            Dim repStr As String = COA0019Session.SYSTEMROOTDir
            uplDir = COA0019Session.BEFOREAPPROVALDir.Replace(repStr, "")

            '承認前ディレクトリ
            befAppDir = COA0019Session.BEFOREAPPROVALDir & "\MSDS\" & Trim(Convert.ToString(dtRow.Item("PRODUCTCODE")))
            '集配信用フォルダ
            sendDir = COA0019Session.SENDDir & "\SENDSTOR\" & Convert.ToString(HttpContext.Current.Session("APSRVname"))
            sendDir = sendDir & uplDir
            sendDir = sendDir & "\MSDS\" & Trim(Convert.ToString(dtRow.Item("PRODUCTCODE")))

            'フォルダが存在する場合、ファイル削除
            If System.IO.Directory.Exists(befAppDir) Then
                'PDF格納フォルダクリア処理
                For Each tempFile As String In System.IO.Directory.GetFiles(befAppDir, "*", System.IO.SearchOption.AllDirectories)
                    'サブフォルダは対象外
                    Try
                        System.IO.File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next
            End If

            'フォルダが存在する場合、ファイル削除
            If System.IO.Directory.Exists(sendDir) Then
                '配信用フォルダクリア処理
                For Each tempFile As String In System.IO.Directory.GetFiles(sendDir, "*", System.IO.SearchOption.AllDirectories)
                    'サブフォルダは対象外
                    Try
                        System.IO.File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next
            End If

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            li.Add(Convert.ToString(dtRow.Item("APPLYID")))

            'li.Add(Convert.ToString(dtRow.Item("COMPCODE")))
            'li.Add(Convert.ToString(dtRow.Item("PRODUCTCODE")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li
        End Function
    End Class
    ''' <summary>
    ''' 国連番号マスタ関連処理
    ''' </summary>
    Private Class GBM00007
        Inherits ApprovalMasterClass '基底クラスを継承
        Private Const CONST_MAPID As String = "GBM00007"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyUnNo"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド
            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ID
            dt.Columns.Add("UNNO", GetType(String))                 '国連番号
            dt.Columns.Add("HAZARDCLASS", GetType(String))          '等級
            dt.Columns.Add("PACKINGGROUP", GetType(String))         '容器等級
            dt.Columns.Add("STYMD", GetType(String))                '有効開始日
            dt.Columns.Add("ENDYMD", GetType(String))               '有効終了日
            dt.Columns.Add("PRODUCTNAME", GetType(String))          '製品名（日）
            dt.Columns.Add("PRODUCTNAME_EN", GetType(String))       '製品名（英）
            dt.Columns.Add("NAME", GetType(String))                 '日本語名
            dt.Columns.Add("NAME_EN", GetType(String))              '英語名
            dt.Columns.Add("COMPATIBILITYGROUP", GetType(String))   '隔離区分
            dt.Columns.Add("SUBSIDIARYRISK", GetType(String))       '副次危険性等級
            dt.Columns.Add("LIMITEDQUANTITIES", GetType(String))    '少量危険物の許容容量又は許容質量
            dt.Columns.Add("EXCEPTETQUANTITIES", GetType(String))   '微量危険物の許容容量又は許容質量
            dt.Columns.Add("PKINSTRUCTIONS", GetType(String))       '容器及び包装－小型容器又は高圧容器－容器
            dt.Columns.Add("PKPROVISIONS", GetType(String))         '容器及び包装－小型容器又は高圧容器－追加規定
            dt.Columns.Add("LPKINSTRUCTIONS", GetType(String))      '容器及び包装－大型容器－容器
            dt.Columns.Add("LPKPROVISIONS", GetType(String))        '容器及び包装－大型容器－追加規定
            dt.Columns.Add("IBCINSTRUCTIONS", GetType(String))      '容器及び包装－IBC容器－容器
            dt.Columns.Add("IBCPROVISIONS", GetType(String))        '容器及び包装－IBC容器－追加規定
            dt.Columns.Add("TANKINSTRUCTIONS", GetType(String))     '容器及び包装－ポータブルタンク－タンク
            dt.Columns.Add("TANKPROVISIONS", GetType(String))       '容器及び包装－ポータブルタンク－追加規定
            dt.Columns.Add("FLEXIBLE", GetType(String))             '容器及び包装－フレキシブルバルクコンテナ
            dt.Columns.Add("SPPROVISIONS", GetType(String))         '容器及び包装－特別規定
            dt.Columns.Add("LOADINGMETHOD", GetType(String))        '積載方法
            dt.Columns.Add("SEGREGATION", GetType(String))          '隔離
            dt.Columns.Add("REMARK", GetType(String))               '備考
            dt.Columns.Add("ENABLED", GetType(String))              '有効フラグ
            dt.Columns.Add("DELFLG", GetType(String))               '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("STEPSTATE", GetType(String))            'ステップ状況
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(UA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,UA.UNNO")
            sqlStat.AppendLine("      ,UA.HAZARDCLASS")
            sqlStat.AppendLine("      ,UA.PACKINGGROUP")
            sqlStat.AppendLine("      ,convert(nvarchar, UA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, UA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,UA.PRODUCTNAME")
            sqlStat.AppendLine("      ,UA.PRODUCTNAME_EN")
            sqlStat.AppendLine("      ,UA.NAME")
            sqlStat.AppendLine("      ,UA.NAME_EN")
            sqlStat.AppendLine("      ,UA.COMPATIBILITYGROUP")
            sqlStat.AppendLine("      ,UA.SUBSIDIARYRISK")
            sqlStat.AppendLine("      ,UA.LIMITEDQUANTITIES")
            sqlStat.AppendLine("      ,UA.EXCEPTETQUANTITIES")
            sqlStat.AppendLine("      ,UA.PKINSTRUCTIONS")
            sqlStat.AppendLine("      ,UA.PKPROVISIONS")
            sqlStat.AppendLine("      ,UA.LPKINSTRUCTIONS")
            sqlStat.AppendLine("      ,UA.LPKPROVISIONS")
            sqlStat.AppendLine("      ,UA.IBCINSTRUCTIONS")
            sqlStat.AppendLine("      ,UA.IBCPROVISIONS")
            sqlStat.AppendLine("      ,UA.TANKINSTRUCTIONS")
            sqlStat.AppendLine("      ,UA.TANKPROVISIONS")
            sqlStat.AppendLine("      ,UA.FLEXIBLE")
            sqlStat.AppendLine("      ,UA.SPPROVISIONS")
            sqlStat.AppendLine("      ,UA.LOADINGMETHOD")
            sqlStat.AppendLine("      ,UA.SEGREGATION")
            sqlStat.AppendLine("      ,UA.REMARK")
            sqlStat.AppendLine("      ,UA.ENABLED")
            sqlStat.AppendLine("      ,UA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0012_UNNOAPPLY UA") '国連番号マスタ(申請)
            sqlStat.AppendLine("    ON  UA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  UA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  UA.ENDYMD      >= @ENDYMD")
            'sqlStat.AppendLine("   AND  UA.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function

        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 国連番号マスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0007_UNNO ")
                sqlStat.AppendLine(" WHERE UNNO = @UNNO ")
                sqlStat.AppendLine("   AND HAZARDCLASS = @HAZARDCLASS ")
                sqlStat.AppendLine("   AND PACKINGGROUP = @PACKINGGROUP ")
                sqlStat.AppendLine("   AND STYMD = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0007_UNNO ")
                sqlStat.AppendLine("  SET ENDYMD = @ENDYMD , ")
                sqlStat.AppendLine("      NAME = @NAME , ")
                sqlStat.AppendLine("      NAME_EN = @NAME_EN , ")
                sqlStat.AppendLine("      PRODUCTNAME = @PRODUCTNAME , ")
                sqlStat.AppendLine("      PRODUCTNAME_EN = @PRODUCTNAME_EN , ")
                sqlStat.AppendLine("      COMPATIBILITYGROUP = @COMPATIBILITYGROUP , ")
                sqlStat.AppendLine("      SUBSIDIARYRISK     = @SUBSIDIARYRISK , ")
                sqlStat.AppendLine("      LIMITEDQUANTITIES  = @LIMITEDQUANTITIES , ")
                sqlStat.AppendLine("      EXCEPTETQUANTITIES = @EXCEPTETQUANTITIES , ")
                sqlStat.AppendLine("      PKINSTRUCTIONS     = @PKINSTRUCTIONS , ")
                sqlStat.AppendLine("      PKPROVISIONS       = @PKPROVISIONS , ")
                sqlStat.AppendLine("      LPKINSTRUCTIONS    = @LPKINSTRUCTIONS , ")
                sqlStat.AppendLine("      LPKPROVISIONS      = @LPKPROVISIONS , ")
                sqlStat.AppendLine("      IBCINSTRUCTIONS    = @IBCINSTRUCTIONS , ")
                sqlStat.AppendLine("      IBCPROVISIONS      = @IBCPROVISIONS , ")
                sqlStat.AppendLine("      TANKINSTRUCTIONS   = @TANKINSTRUCTIONS , ")
                sqlStat.AppendLine("      TANKPROVISIONS     = @TANKPROVISIONS , ")
                sqlStat.AppendLine("      FLEXIBLE           = @FLEXIBLE , ")
                sqlStat.AppendLine("      SPPROVISIONS       = @SPPROVISIONS , ")
                sqlStat.AppendLine("      LOADINGMETHOD      = @LOADINGMETHOD , ")
                sqlStat.AppendLine("      SEGREGATION        = @SEGREGATION , ")
                sqlStat.AppendLine("      REMARK             = @REMARK , ")
                sqlStat.AppendLine("      ENABLED            = @ENABLED , ")
                sqlStat.AppendLine("      DELFLG             = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD             = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER            = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID          = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD         = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE UNNO              = @UNNO ")
                sqlStat.AppendLine("   AND HAZARDCLASS       = @HAZARDCLASS ")
                sqlStat.AppendLine("   AND PACKINGGROUP      = @PACKINGGROUP ")
                sqlStat.AppendLine("   AND STYMD             = @STYMD ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0007_UNNO ( ")
                sqlStat.AppendLine("      UNNO , ")
                sqlStat.AppendLine("      HAZARDCLASS , ")
                sqlStat.AppendLine("      PACKINGGROUP , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      PRODUCTNAME , ")
                sqlStat.AppendLine("      PRODUCTNAME_EN , ")
                sqlStat.AppendLine("      NAME , ")
                sqlStat.AppendLine("      NAME_EN , ")
                sqlStat.AppendLine("      COMPATIBILITYGROUP , ")
                sqlStat.AppendLine("      SUBSIDIARYRISK , ")
                sqlStat.AppendLine("      LIMITEDQUANTITIES , ")
                sqlStat.AppendLine("      EXCEPTETQUANTITIES , ")
                sqlStat.AppendLine("      PKINSTRUCTIONS , ")
                sqlStat.AppendLine("      PKPROVISIONS , ")
                sqlStat.AppendLine("      LPKINSTRUCTIONS , ")
                sqlStat.AppendLine("      LPKPROVISIONS , ")
                sqlStat.AppendLine("      IBCINSTRUCTIONS , ")
                sqlStat.AppendLine("      IBCPROVISIONS , ")
                sqlStat.AppendLine("      TANKINSTRUCTIONS , ")
                sqlStat.AppendLine("      TANKPROVISIONS , ")
                sqlStat.AppendLine("      FLEXIBLE , ")
                sqlStat.AppendLine("      SPPROVISIONS , ")
                sqlStat.AppendLine("      LOADINGMETHOD , ")
                sqlStat.AppendLine("      SEGREGATION , ")
                sqlStat.AppendLine("      REMARK , ")
                sqlStat.AppendLine("      ENABLED , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine(" @UNNO,@HAZARDCLASS,@PACKINGGROUP,@STYMD,@ENDYMD,@PRODUCTNAME,@PRODUCTNAME_EN,@NAME,@NAME_EN,@COMPATIBILITYGROUP,@SUBSIDIARYRISK,@LIMITEDQUANTITIES,")
                sqlStat.AppendLine(" @EXCEPTETQUANTITIES,@PKINSTRUCTIONS,@PKPROVISIONS,@LPKINSTRUCTIONS,@LPKPROVISIONS,@IBCINSTRUCTIONS,@IBCPROVISIONS,@TANKINSTRUCTIONS,@TANKPROVISIONS,@FLEXIBLE,")
                sqlStat.AppendLine(" @SPPROVISIONS,@LOADINGMETHOD,@SEGREGATION,@REMARK,@ENABLED,@DELFLG,@INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@UNNO", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("UNNO"))
                        .Add("@HAZARDCLASS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("HAZARDCLASS"))
                        .Add("@PACKINGGROUP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PACKINGGROUP"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.Date).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@PRODUCTNAME", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PRODUCTNAME"))
                        .Add("@PRODUCTNAME_EN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PRODUCTNAME_EN"))
                        .Add("@NAME", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAME"))
                        .Add("@NAME_EN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAME_EN"))
                        .Add("@COMPATIBILITYGROUP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPATIBILITYGROUP"))
                        .Add("@SUBSIDIARYRISK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("SUBSIDIARYRISK"))
                        .Add("@LIMITEDQUANTITIES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LIMITEDQUANTITIES"))
                        .Add("@EXCEPTETQUANTITIES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("EXCEPTETQUANTITIES"))
                        .Add("@PKINSTRUCTIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PKINSTRUCTIONS"))
                        .Add("@PKPROVISIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PKPROVISIONS"))
                        .Add("@LPKINSTRUCTIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LPKINSTRUCTIONS"))
                        .Add("@LPKPROVISIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LPKPROVISIONS"))
                        .Add("@IBCINSTRUCTIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("IBCINSTRUCTIONS"))
                        .Add("@IBCPROVISIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("IBCPROVISIONS"))
                        .Add("@TANKINSTRUCTIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TANKINSTRUCTIONS"))
                        .Add("@TANKPROVISIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TANKPROVISIONS"))
                        .Add("@FLEXIBLE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FLEXIBLE"))
                        .Add("@SPPROVISIONS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("SPPROVISIONS"))
                        .Add("@LOADINGMETHOD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LOADINGMETHOD"))
                        .Add("@SEGREGATION", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("SEGREGATION"))
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK"))
                        .Add("@ENABLED", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENABLED"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0007_UNNO"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 国連番号マスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0012_UNNOAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE UNNO          = @UNNO")
                sqlStat.AppendLine("   AND HAZARDCLASS   = @HAZARDCLASS")
                sqlStat.AppendLine("   AND PACKINGGROUP  = @PACKINGGROUP")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@UNNO", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("UNNO"))
                        .Add("@HAZARDCLASS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("HAZARDCLASS"))
                        .Add("@PACKINGGROUP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("PACKINGGROUP"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            li.Add(Convert.ToString(dtRow.Item("APPLYID")))

            'li.Add(Convert.ToString("Default"))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))
            'li.Add(Convert.ToString(dtRow.Item("UNNO")))
            'li.Add(Convert.ToString(dtRow.Item("HAZARDCLASS")))
            'li.Add(Convert.ToString(dtRow.Item("PACKINGGROUP")))

            Return li

        End Function
    End Class
    ''' <summary>
    ''' ユーザマスタ関連処理
    ''' </summary>
    Private Class COM00005

        Inherits ApprovalMasterClass '基底クラスを継承
        '↓呼出し元で実行必要なメソッドはパブリックコープ+Overloadsにしておいてください
        '　（サブクラス内のみで済むのはPrivateでOK"
        Private Const CONST_MAPID As String = "COM00005"   '自身のMAPID
        'Private Const CONST_EVENTCODE As String = "MasterApplyUser"
        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            ApplyMstDbUpdate(dtRow)

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' ユーザマスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM COS0005_USER ")
                sqlStat.AppendLine(" WHERE USERID = @USERID ")
                sqlStat.AppendLine("   AND STYMD  = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE COS0005_USER ")
                sqlStat.AppendLine("  SET ENDYMD        = @ENDYMD , ")
                sqlStat.AppendLine("      COMPCODE      = @COMPCODE , ")
                sqlStat.AppendLine("      ORG           = @ORG , ")
                sqlStat.AppendLine("      PROFID        = @PROFID , ")
                sqlStat.AppendLine("      STAFFCODE     = @STAFFCODE , ")
                sqlStat.AppendLine("      STAFFNAMES    = @STAFFNAMES , ")
                sqlStat.AppendLine("      STAFFNAMEL    = @STAFFNAMEL , ")
                sqlStat.AppendLine("      STAFFNAMES_EN = @STAFFNAMES_EN , ")
                sqlStat.AppendLine("      STAFFNAMEL_EN = @STAFFNAMEL_EN , ")
                sqlStat.AppendLine("      TEL           = @TEL , ")
                sqlStat.AppendLine("      FAX           = @FAX , ")
                sqlStat.AppendLine("      MOBILE        = @MOBILE , ")
                sqlStat.AppendLine("      EMAIL         = @EMAIL , ")
                sqlStat.AppendLine("      DEFAULTSRV    = @DEFAULTSRV , ")
                sqlStat.AppendLine("      LOGINFLG      = @LOGINFLG , ")
                sqlStat.AppendLine("      MAPID         = @MAPID , ")
                sqlStat.AppendLine("      VARIANT       = @VARIANT , ")
                sqlStat.AppendLine("      LANGDISP      = @LANGDISP , ")
                sqlStat.AppendLine("      DELFLG        = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD        = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER       = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID     = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE USERID       = @USERID ")
                sqlStat.AppendLine("   AND STYMD        = @STYMD ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO COS0005_USER ( ")
                sqlStat.AppendLine("      USERID , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      ORG , ")
                sqlStat.AppendLine("      PROFID , ")
                sqlStat.AppendLine("      STAFFCODE , ")
                sqlStat.AppendLine("      STAFFNAMES , ")
                sqlStat.AppendLine("      STAFFNAMEL , ")
                sqlStat.AppendLine("      STAFFNAMES_EN , ")
                sqlStat.AppendLine("      STAFFNAMEL_EN , ")
                sqlStat.AppendLine("      TEL , ")
                sqlStat.AppendLine("      FAX , ")
                sqlStat.AppendLine("      MOBILE , ")
                sqlStat.AppendLine("      EMAIL , ")
                sqlStat.AppendLine("      DEFAULTSRV , ")
                sqlStat.AppendLine("      LOGINFLG , ")
                sqlStat.AppendLine("      MAPID , ")
                sqlStat.AppendLine("      VARIANT , ")
                sqlStat.AppendLine("      LANGDISP , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      INITUSER , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine(" @USERID,@STYMD,@ENDYMD,@COMPCODE,@ORG,@PROFID,@STAFFCODE,@STAFFNAMES,@STAFFNAMEL,@STAFFNAMES_EN,")
                sqlStat.AppendLine(" @STAFFNAMEL_EN,@TEL,@FAX,@MOBILE,@EMAIL,@DEFAULTSRV,@LOGINFLG,@MAPID,@VARIANT,@LANGDISP,")
                sqlStat.AppendLine(" @DELFLG,@INITYMD,@INITUSER,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@USERID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("USERID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.Date).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@ORG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ORG"))
                        .Add("@PROFID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PROFID"))
                        .Add("@STAFFCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STAFFCODE"))
                        .Add("@STAFFNAMES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STAFFNAMES"))
                        .Add("@STAFFNAMEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STAFFNAMEL"))
                        .Add("@STAFFNAMES_EN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STAFFNAMES_EN"))
                        .Add("@STAFFNAMEL_EN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STAFFNAMEL_EN"))
                        .Add("@TEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TEL"))
                        .Add("@FAX", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FAX"))
                        .Add("@MOBILE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MOBILE"))
                        .Add("@EMAIL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("EMAIL"))
                        .Add("@DEFAULTSRV", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEFAULTSRV"))
                        .Add("@LOGINFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LOGINFLG"))
                        .Add("@MAPID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MAPID"))
                        .Add("@VARIANT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("VARIANT"))
                        .Add("@LANGDISP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LANGDISP"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@INITUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '******************************
                ' パスワードマスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM COS0006_USERPASS ")
                sqlStat.AppendLine(" WHERE USERID = @USERID ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE COS0006_USERPASS ")
                sqlStat.AppendLine("  SET PASSWORD      = @PASSWORD , ")
                sqlStat.AppendLine("      MISSCNT       = @MISSCNT , ")
                sqlStat.AppendLine("      PASSENDYMD    = @PASSENDYMD , ")
                sqlStat.AppendLine("      DELFLG        = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD        = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER       = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID     = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE USERID       = @USERID ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO COS0006_USERPASS ( ")
                sqlStat.AppendLine("      USERID , ")
                sqlStat.AppendLine("      PASSWORD , ")
                sqlStat.AppendLine("      MISSCNT , ")
                sqlStat.AppendLine("      PASSENDYMD , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine(" @USERID,@PASSWORD,@MISSCNT,@PASSENDYMD,@DELFLG,@INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@USERID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("USERID"))
                        .Add("@PASSWORD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PASSWORD"))
                        .Add("@MISSCNT", SqlDbType.Int).Value = Convert.ToInt32(dtRow("MISSCNT"))
                        .Add("@PASSENDYMD", SqlDbType.Date).Value = Convert.ToString(dtRow("PASSENDYMD"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '権限（機能）
                If Convert.ToString(dtRow("ROLEMAP")) <> "" Then

                    UpdateAuthority("MAP", dtRow, sqlCon, nowDate)

                End If

                '権限（組織）
                If Convert.ToString(dtRow("ROLEORG")) <> "" Then

                    UpdateAuthority("ORG", dtRow, sqlCon, nowDate)

                End If

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0007_UNNO"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

        End Sub
        ''' <summary>
        ''' 権限マスタ更新
        ''' </summary>
        ''' <param name="Obj"></param>
        Private Sub UpdateAuthority(ByVal Obj As String, ByVal dtRow As DataRow, ByVal SQLcon As SqlConnection, nowDate As DateTime)

            Dim SQLStr As String = Nothing
            Dim SQLcmd As New SqlCommand()
            Dim SQLdr As SqlDataReader = Nothing

            Try

                '権限マスタ更新
                SQLStr =
                    " DECLARE @hensuu as bigint ;                                                        " _
                    & " set @hensuu = 0 ;                                                                " _
                    & " DECLARE hensuu CURSOR FOR                                                        " _
                    & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                                     " _
                    & "     FROM COS0011_AUTHOR                                                          " _
                    & "     WHERE   USERID          = @P1                                                " _
                    & "       and   COMPCODE        = @P2                                                " _
                    & "       and   OBJECT          = @P3                                                " _
                    & "       and   ROLE            = @P4                                                " _
                    & "       and   STYMD           = @P6 ;                                              " _
                    & " OPEN hensuu ;                                                                    " _
                    & "                                                                                  " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu ;                                            " _
                    & "                                                                                  " _
                    & " IF ( @@FETCH_STATUS = 0 )                                                        " _
                    & "     UPDATE COS0011_AUTHOR                                                        " _
                    & "       SET   ENDYMD          = @P7 ,                                              " _
                    & "             ROLENAMES       = @P8 ,                                              " _
                    & "             ROLENAMEL       = @P9 ,                                              " _
                    & "             DELFLG          = @P10 ,                                             " _
                    & "             UPDYMD          = @P12 ,                                             " _
                    & "             UPDUSER         = @P13 ,                                             " _
                    & "             UPDTERMID       = @P14 ,                                             " _
                    & "             RECEIVEYMD      = @P15                                               " _
                    & "     WHERE   USERID          = @P1                                                " _
                    & "       and   COMPCODE        = @P2                                                " _
                    & "       and   OBJECT          = @P3                                                " _
                    & "       and   ROLE            = @P4                                                " _
                    & "       and   STYMD           = @P6 ;                                              " _
                    & "                                                                                  " _
                    & " IF ( @@FETCH_STATUS <> 0 )                                                       " _
                    & "     INSERT INTO COS0011_AUTHOR                                                   " _
                    & "            (USERID ,                                                             " _
                    & "             COMPCODE ,                                                           " _
                    & "             OBJECT ,                                                             " _
                    & "             ROLE,                                                                " _
                    & "             SEQ,                                                                 " _
                    & "             STYMD ,                                                              " _
                    & "             ENDYMD ,                                                             " _
                    & "             ROLENAMES ,                                                          " _
                    & "             ROLENAMEL ,                                                          " _
                    & "             DELFLG ,                                                             " _
                    & "             INITYMD ,                                                            " _
                    & "             UPDYMD ,                                                             " _
                    & "             UPDUSER ,                                                            " _
                    & "             UPDTERMID ,                                                          " _
                    & "             RECEIVEYMD)                                                          " _
                    & "     VALUES (@P1,@P2,@P3,@P4,@P5,@P6,@P7,@P8,@P9,@P10,@P11,@P12,@P13,@P14,@P15) ;  " _
                    & "                                                                                  " _
                    & " CLOSE hensuu ;                                                                   " _
                    & " DEALLOCATE hensuu ;                                                              "

                SQLcmd = New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Int)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P9", System.Data.SqlDbType.NVarChar)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar)
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.DateTime)

                PARA01.Value = Convert.ToString(dtRow("USERID"))
                PARA02.Value = "Default" 'TODO:とりあえずDefault固定
                'PARA02.Value = Convert.ToString(dtRow("COMPCODE"))
                PARA03.Value = Obj
                If Obj = "MAP" Then
                    PARA04.Value = Convert.ToString(dtRow("ROLEMAP"))
                ElseIf Obj = "ORG" Then
                    PARA04.Value = Convert.ToString(dtRow("ROLEORG"))
                End If
                PARA05.Value = 1
                PARA06.Value = Convert.ToString(dtRow("STYMD"))
                PARA07.Value = Convert.ToString(dtRow("ENDYMD"))
                PARA08.Value = Convert.ToString(dtRow("STAFFNAMES"))
                PARA09.Value = Convert.ToString(dtRow("STAFFNAMEL"))
                PARA10.Value = Convert.ToString(dtRow("DELFLG"))
                PARA11.Value = nowDate
                PARA12.Value = nowDate
                PARA13.Value = COA0019Session.USERID
                PARA14.Value = HttpContext.Current.Session("APSRVname")
                PARA15.Value = CONST_DEFAULT_RECEIVEYMD

                SQLcmd.ExecuteNonQuery()

            Catch ex As Exception
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
            End Try

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' ユーザマスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE COS0020_USERAPPLY")
                sqlStat.AppendLine("   Set DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine(" WHERE USERID        = @USERID")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@USERID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("USERID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド

            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ＩＤ
            dt.Columns.Add("USERID", GetType(String))               'ユーザＩＤ
            dt.Columns.Add("STYMD", GetType(String))                '開始年月日
            dt.Columns.Add("ENDYMD", GetType(String))               '終了年月日
            dt.Columns.Add("COMPCODE", GetType(String))             '所属会社
            dt.Columns.Add("ORG", GetType(String))                  '所属組織
            dt.Columns.Add("PROFID", GetType(String))               'プロファイルＩＤ
            dt.Columns.Add("STAFFCODE", GetType(String))            '社員コード
            dt.Columns.Add("STAFFNAMES", GetType(String))           '社員名（短）
            dt.Columns.Add("STAFFNAMEL", GetType(String))           '社員名（長）
            dt.Columns.Add("STAFFNAMES_EN", GetType(String))        '社員名（短）英名
            dt.Columns.Add("STAFFNAMEL_EN", GetType(String))        '社員名（長）英名
            dt.Columns.Add("TEL", GetType(String))                  '電話番号
            dt.Columns.Add("FAX", GetType(String))                  'FAX番号
            dt.Columns.Add("MOBILE", GetType(String))               '携帯番号
            dt.Columns.Add("EMAIL", GetType(String))                'E-mailｱﾄﾞﾚｽ
            dt.Columns.Add("DEFAULTSRV", GetType(String))           'デフォルトサーバ
            dt.Columns.Add("LOGINFLG", GetType(String))             'ログインチェックフラグ
            dt.Columns.Add("MAPID", GetType(String))                '画面ＩＤ
            dt.Columns.Add("VARIANT", GetType(String))              '変数
            dt.Columns.Add("LANGDISP", GetType(String))             '表示言語
            dt.Columns.Add("PASSWORD", GetType(String))             'パスワード
            dt.Columns.Add("MISSCNT", GetType(String))              '誤り回数
            dt.Columns.Add("PASSENDYMD", GetType(String))           'パスワード有効期限
            dt.Columns.Add("ROLEMAP", GetType(String))              '権限（機能）
            dt.Columns.Add("ROLEORG", GetType(String))              '権限（組織）
            dt.Columns.Add("DELFLG", GetType(String))               '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("STEPSTATE", GetType(String))            'ステップ状況
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <param name="stYMD"></param>
        ''' <param name="endYMD"></param>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") AS LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(UA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,UA.USERID")
            sqlStat.AppendLine("      ,convert(nvarchar, UA.STYMD , 111) AS STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, UA.ENDYMD , 111) AS ENDYMD")
            sqlStat.AppendLine("      ,UA.COMPCODE")
            sqlStat.AppendLine("      ,UA.ORG")
            sqlStat.AppendLine("      ,UA.PROFID")
            sqlStat.AppendLine("      ,UA.STAFFCODE")
            sqlStat.AppendLine("      ,UA.STAFFNAMES")
            sqlStat.AppendLine("      ,UA.STAFFNAMEL")
            sqlStat.AppendLine("      ,UA.STAFFNAMES_EN")
            sqlStat.AppendLine("      ,UA.STAFFNAMEL_EN")
            sqlStat.AppendLine("      ,UA.TEL")
            sqlStat.AppendLine("      ,UA.FAX")
            sqlStat.AppendLine("      ,UA.MOBILE")
            sqlStat.AppendLine("      ,UA.EMAIL")
            sqlStat.AppendLine("      ,UA.DEFAULTSRV")
            sqlStat.AppendLine("      ,UA.LOGINFLG")
            sqlStat.AppendLine("      ,UA.MAPID")
            sqlStat.AppendLine("      ,UA.VARIANT")
            sqlStat.AppendLine("      ,UA.LANGDISP")
            sqlStat.AppendLine("      ,UA.PASSWORD")
            sqlStat.AppendLine("      ,UA.MISSCNT")
            sqlStat.AppendLine("      ,convert(nvarchar, UA.PASSENDYMD , 111) AS PASSENDYMD")
            sqlStat.AppendLine("      ,UA.ROLEMAP")
            sqlStat.AppendLine("      ,UA.ROLEORG")
            sqlStat.AppendLine("      ,UA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END AS CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN COS0020_USERAPPLY UA") '国連番号マスタ(申請)
            sqlStat.AppendLine("    ON  UA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  UA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  UA.ENDYMD      >= @ENDYMD")
            'sqlStat.AppendLine("   AND  UA.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE     = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = C_USEMSTEVENT.APPLY
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            li.Add(Convert.ToString(dtRow.Item("APPLYID")))

            'li.Add(Convert.ToString(dtRow.Item("USERID")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li
        End Function
    End Class

    ''' <summary>
    ''' 国マスタマスタ関連処理
    ''' </summary>
    Private Class GBM00001
        Inherits ApprovalMasterClass '基底クラスを継承
        Private Const CONST_MAPID As String = "GBM00001"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyCountry"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド
            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ＩＤ
            dt.Columns.Add("COMPCODE", GetType(String))             '会社コード
            dt.Columns.Add("ORGCODE", GetType(String))              '組織コード
            dt.Columns.Add("STYMD", GetType(String))                '開始年月日
            dt.Columns.Add("ENDYMD", GetType(String))               '終了年月日
            dt.Columns.Add("COUNTRYCODE", GetType(String))          '国コード
            dt.Columns.Add("NAMES", GetType(String))                '国名称（短）
            dt.Columns.Add("NAMEL", GetType(String))                '国名称（長）
            dt.Columns.Add("NAMESJP", GetType(String))              '国名称（短）JP
            dt.Columns.Add("NAMELJP", GetType(String))              '国名称（長）JP
            dt.Columns.Add("CURRENCYCODE", GetType(String))         '通貨換算コード
            dt.Columns.Add("TAXRATE", GetType(String))              '税率
            dt.Columns.Add("DATEFORMAT", GetType(String))           '日付フォーマット
            dt.Columns.Add("DECIMALPLACES", GetType(String))        '小数桁
            dt.Columns.Add("RATEDECIMALPLACES", GetType(String))    '為替レート小数桁
            dt.Columns.Add("ROUNDFLG", GetType(String))             '端数制御フラグ
            dt.Columns.Add("DEBITSEGMENT", GetType(String))         '借方セグメント
            dt.Columns.Add("ACCCURRENCYSEGMENT", GetType(String))   '経理円貨外貨区分
            dt.Columns.Add("BOTHCLASS", GetType(String))            '両建区分
            dt.Columns.Add("TORICOMP", GetType(String))             '取引先会社コード
            dt.Columns.Add("INCTORICODE", GetType(String))          '取引先コード（収入）
            dt.Columns.Add("EXPTORICODE", GetType(String))          '取引先コード（費用）
            dt.Columns.Add("DEPOSITDAY", GetType(String))           '入金期日
            dt.Columns.Add("DEPOSITADDMM", GetType(String))         '入金期日(加算月）
            dt.Columns.Add("OVERDRAWDAY", GetType(String))          '出金期日
            dt.Columns.Add("OVERDRAWADDMM", GetType(String))        '出金期日(加算月）
            dt.Columns.Add("HOLIDAYFLG", GetType(String))           '休日フラグ
            dt.Columns.Add("OFFICENAME", GetType(String))           'オフィス名
            dt.Columns.Add("MAIL_COUNTRY", GetType(String))         'メールアドレス
            dt.Columns.Add("REMARKS", GetType(String))              '備考
            dt.Columns.Add("DELFLG", GetType(String))               '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(CA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,CA.COMPCODE")
            sqlStat.AppendLine("      ,CA.ORGCODE")
            sqlStat.AppendLine("      ,convert(nvarchar, CA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, CA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,CA.COUNTRYCODE")
            sqlStat.AppendLine("      ,CA.NAMES")
            sqlStat.AppendLine("      ,CA.NAMEL")
            sqlStat.AppendLine("      ,CA.NAMESJP")
            sqlStat.AppendLine("      ,CA.NAMELJP")
            sqlStat.AppendLine("      ,CA.CURRENCYCODE")
            sqlStat.AppendLine("      ,CA.TAXRATE")
            sqlStat.AppendLine("      ,CA.DATEFORMAT")
            sqlStat.AppendLine("      ,CA.DECIMALPLACES")
            sqlStat.AppendLine("      ,CA.RATEDECIMALPLACES")
            sqlStat.AppendLine("      ,CA.ROUNDFLG")
            sqlStat.AppendLine("      ,CA.DEBITSEGMENT")
            sqlStat.AppendLine("      ,CA.ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("      ,CA.BOTHCLASS")
            sqlStat.AppendLine("      ,CA.TORICOMP")
            sqlStat.AppendLine("      ,CA.INCTORICODE")
            sqlStat.AppendLine("      ,CA.EXPTORICODE")
            sqlStat.AppendLine("      ,CA.DEPOSITDAY")
            sqlStat.AppendLine("      ,CA.DEPOSITADDMM")
            sqlStat.AppendLine("      ,CA.OVERDRAWDAY")
            sqlStat.AppendLine("      ,CA.OVERDRAWADDMM")
            sqlStat.AppendLine("      ,CA.HOLIDAYFLG")
            sqlStat.AppendLine("      ,CA.OFFICENAME")
            sqlStat.AppendLine("      ,CA.MAIL_COUNTRY")
            sqlStat.AppendLine("      ,CA.REMARKS")
            sqlStat.AppendLine("      ,CA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0015_COUNTRYAPPLY CA") '国マスタ(申請)
            sqlStat.AppendLine("    ON  CA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  CA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  CA.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function

        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 国マスタマスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0001_COUNTRY ")
                sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE ")
                sqlStat.AppendLine("   AND ORGCODE = @ORGCODE ")
                sqlStat.AppendLine("   AND STYMD = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0001_COUNTRY ")
                sqlStat.AppendLine("  SET ENDYMD = @ENDYMD , ")
                sqlStat.AppendLine("      COUNTRYCODE = @COUNTRYCODE , ")
                sqlStat.AppendLine("      NAMES = @NAMES , ")
                sqlStat.AppendLine("      NAMEL = @NAMEL , ")
                sqlStat.AppendLine("      NAMESJP = @NAMESJP , ")
                sqlStat.AppendLine("      NAMELJP = @NAMELJP , ")
                sqlStat.AppendLine("      CURRENCYCODE = @CURRENCYCODE , ")
                sqlStat.AppendLine("      TAXRATE = @TAXRATE , ")
                sqlStat.AppendLine("      DATEFORMAT = @DATEFORMAT , ")
                sqlStat.AppendLine("      DECIMALPLACES = @DECIMALPLACES , ")
                sqlStat.AppendLine("      RATEDECIMALPLACES = @RATEDECIMALPLACES , ")
                sqlStat.AppendLine("      ROUNDFLG = @ROUNDFLG , ")
                sqlStat.AppendLine("      DEBITSEGMENT = @DEBITSEGMENT , ")
                sqlStat.AppendLine("      ACCCURRENCYSEGMENT = @ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      BOTHCLASS = @BOTHCLASS , ")
                sqlStat.AppendLine("      TORICOMP = @TORICOMP , ")
                sqlStat.AppendLine("      INCTORICODE = @INCTORICODE , ")
                sqlStat.AppendLine("      EXPTORICODE = @EXPTORICODE , ")
                sqlStat.AppendLine("      DEPOSITDAY = @DEPOSITDAY , ")
                sqlStat.AppendLine("      DEPOSITADDMM = @DEPOSITADDMM , ")
                sqlStat.AppendLine("      OVERDRAWDAY = @OVERDRAWDAY , ")
                sqlStat.AppendLine("      OVERDRAWADDMM = @OVERDRAWADDMM , ")
                sqlStat.AppendLine("      HOLIDAYFLG = @HOLIDAYFLG , ")
                sqlStat.AppendLine("      OFFICENAME = @OFFICENAME , ")
                sqlStat.AppendLine("      MAIL_COUNTRY = @MAIL_COUNTRY , ")
                sqlStat.AppendLine("      REMARKS = @REMARKS , ")
                sqlStat.AppendLine("      DELFLG = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD             = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER            = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID          = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD         = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE       = @COMPCODE ")
                sqlStat.AppendLine("   AND ORGCODE       = @ORGCODE ")
                sqlStat.AppendLine("   AND STYMD       = @STYMD ")
                sqlStat.AppendLine("   ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0001_COUNTRY ( ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      ORGCODE , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      COUNTRYCODE , ")
                sqlStat.AppendLine("      NAMES , ")
                sqlStat.AppendLine("      NAMEL , ")
                sqlStat.AppendLine("      NAMESJP , ")
                sqlStat.AppendLine("      NAMELJP , ")
                sqlStat.AppendLine("      CURRENCYCODE , ")
                sqlStat.AppendLine("      TAXRATE , ")
                sqlStat.AppendLine("      DATEFORMAT , ")
                sqlStat.AppendLine("      DECIMALPLACES , ")
                sqlStat.AppendLine("      RATEDECIMALPLACES , ")
                sqlStat.AppendLine("      ROUNDFLG , ")
                sqlStat.AppendLine("      DEBITSEGMENT , ")
                sqlStat.AppendLine("      ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      BOTHCLASS , ")
                sqlStat.AppendLine("      TORICOMP , ")
                sqlStat.AppendLine("      INCTORICODE , ")
                sqlStat.AppendLine("      EXPTORICODE , ")
                sqlStat.AppendLine("      DEPOSITDAY , ")
                sqlStat.AppendLine("      DEPOSITADDMM , ")
                sqlStat.AppendLine("      OVERDRAWDAY , ")
                sqlStat.AppendLine("      OVERDRAWADDMM , ")
                sqlStat.AppendLine("      HOLIDAYFLG , ")
                sqlStat.AppendLine("      OFFICENAME , ")
                sqlStat.AppendLine("      MAIL_COUNTRY , ")
                sqlStat.AppendLine("      REMARKS , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine("      @COMPCODE , ")
                sqlStat.AppendLine("      @ORGCODE , ")
                sqlStat.AppendLine("      @STYMD , ")
                sqlStat.AppendLine("      @ENDYMD , ")
                sqlStat.AppendLine("      @COUNTRYCODE , ")
                sqlStat.AppendLine("      @NAMES , ")
                sqlStat.AppendLine("      @NAMEL , ")
                sqlStat.AppendLine("      @NAMESJP , ")
                sqlStat.AppendLine("      @NAMELJP , ")
                sqlStat.AppendLine("      @CURRENCYCODE , ")
                sqlStat.AppendLine("      @TAXRATE , ")
                sqlStat.AppendLine("      @DATEFORMAT , ")
                sqlStat.AppendLine("      @DECIMALPLACES , ")
                sqlStat.AppendLine("      @RATEDECIMALPLACES , ")
                sqlStat.AppendLine("      @ROUNDFLG , ")
                sqlStat.AppendLine("      @DEBITSEGMENT , ")
                sqlStat.AppendLine("      @ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      @BOTHCLASS , ")
                sqlStat.AppendLine("      @TORICOMP , ")
                sqlStat.AppendLine("      @INCTORICODE , ")
                sqlStat.AppendLine("      @EXPTORICODE , ")
                sqlStat.AppendLine("      @DEPOSITDAY , ")
                sqlStat.AppendLine("      @DEPOSITADDMM , ")
                sqlStat.AppendLine("      @OVERDRAWDAY , ")
                sqlStat.AppendLine("      @OVERDRAWADDMM , ")
                sqlStat.AppendLine("      @HOLIDAYFLG , ")
                sqlStat.AppendLine("      @OFFICENAME , ")
                sqlStat.AppendLine("      @MAIL_COUNTRY , ")
                sqlStat.AppendLine("      @REMARKS , ")
                sqlStat.AppendLine("      @DELFLG , ")
                sqlStat.AppendLine(" @INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@ORGCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ORGCODE"))
                        .Add("@STYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COUNTRYCODE"))
                        .Add("@NAMES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMES"))
                        .Add("@NAMEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMEL"))
                        .Add("@NAMESJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMESJP"))
                        .Add("@NAMELJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMELJP"))
                        .Add("@CURRENCYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CURRENCYCODE"))
                        .Add("@TAXRATE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TAXRATE"))
                        .Add("@DATEFORMAT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DATEFORMAT"))
                        .Add("@DECIMALPLACES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DECIMALPLACES"))
                        .Add("@RATEDECIMALPLACES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("RATEDECIMALPLACES"))
                        .Add("@ROUNDFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ROUNDFLG"))
                        .Add("@DEBITSEGMENT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEBITSEGMENT"))
                        .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACCCURRENCYSEGMENT"))
                        .Add("@BOTHCLASS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BOTHCLASS"))
                        .Add("@TORICOMP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TORICOMP"))
                        .Add("@INCTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("INCTORICODE"))
                        .Add("@EXPTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("EXPTORICODE"))
                        .Add("@DEPOSITDAY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEPOSITDAY"))
                        .Add("@DEPOSITADDMM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEPOSITADDMM"))
                        .Add("@OVERDRAWDAY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OVERDRAWDAY"))
                        .Add("@OVERDRAWADDMM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OVERDRAWADDMM"))
                        .Add("@HOLIDAYFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("HOLIDAYFLG"))
                        .Add("@OFFICENAME", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OFFICENAME"))
                        .Add("@MAIL_COUNTRY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MAIL_COUNTRY"))
                        .Add("@REMARKS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARKS"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0001_COUNTRY"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 国マスタマスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0015_COUNTRYAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,UPDTERMID     = @UPDTERMID ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE APPLYID       = @APPLYID")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("APPLYID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            li.Add(Convert.ToString(dtRow.Item("APPLYID")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li

        End Function
    End Class

    ''' <summary>
    ''' 港マスタ関連処理
    ''' </summary>
    Private Class GBM00002
        Inherits ApprovalMasterClass '基底クラスを継承
        Private Const CONST_MAPID As String = "GBM00002"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyPort"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド
            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ID
            dt.Columns.Add("COMPCODE", GetType(String))             '会社コード
            dt.Columns.Add("ORGCODE", GetType(String))              '組織コード
            dt.Columns.Add("STYMD", GetType(String))                '有効開始日
            dt.Columns.Add("ENDYMD", GetType(String))               '有効終了日
            dt.Columns.Add("COUNTRYCODE", GetType(String))          '国コード
            dt.Columns.Add("PORTCODE", GetType(String))             '港コード
            dt.Columns.Add("AREACODE", GetType(String))             'エリアコード
            dt.Columns.Add("AREANAME", GetType(String))             'エリア名
            dt.Columns.Add("GROUPCODE", GetType(String))            'グループコード
            dt.Columns.Add("GROUPNAME", GetType(String))            'グループ名
            dt.Columns.Add("REMARK", GetType(String))               '備考
            dt.Columns.Add("DELFLG", GetType(String))               '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("STEPSTATE", GetType(String))            'ステップ状況
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(PA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,PA.COMPCODE")
            sqlStat.AppendLine("      ,PA.ORGCODE")
            sqlStat.AppendLine("      ,convert(nvarchar, PA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, PA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,PA.COUNTRYCODE")
            sqlStat.AppendLine("      ,PA.PORTCODE")
            sqlStat.AppendLine("      ,PA.AREACODE")
            sqlStat.AppendLine("      ,PA.AREANAME")
            sqlStat.AppendLine("      ,PA.GROUPCODE")
            sqlStat.AppendLine("      ,PA.GROUPNAME")
            sqlStat.AppendLine("      ,PA.REMARK")
            sqlStat.AppendLine("      ,PA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0016_PORTAPPLY PA") '港マスタ(申請)
            sqlStat.AppendLine("    ON  PA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  PA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  PA.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function

        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 港マスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0002_PORT ")
                sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE ")
                sqlStat.AppendLine("   AND ORGCODE     = @ORGCODE ")
                sqlStat.AppendLine("   AND STYMD       = @STYMD ")
                sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE ")
                sqlStat.AppendLine("   AND PORTCODE    = @PORTCODE ")
                sqlStat.AppendLine("   AND AREACODE    = @AREACODE ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0002_PORT ")
                sqlStat.AppendLine("  SET ENDYMD       = @ENDYMD , ")
                sqlStat.AppendLine("      AREANAME     = @AREANAME , ")
                sqlStat.AppendLine("      GROUPCODE    = @GROUPCODE , ")
                sqlStat.AppendLine("      GROUPNAME    = @GROUPNAME , ")
                sqlStat.AppendLine("      REMARK       = @REMARK , ")
                sqlStat.AppendLine("      DELFLG       = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD       = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER      = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID    = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD   = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE ")
                sqlStat.AppendLine("   AND ORGCODE     = @ORGCODE ")
                sqlStat.AppendLine("   AND STYMD       = @STYMD ")
                sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE ")
                sqlStat.AppendLine("   AND PORTCODE    = @PORTCODE ")
                sqlStat.AppendLine("   AND AREACODE    = @AREACODE ")
                sqlStat.AppendLine("   ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0002_PORT ( ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      ORGCODE , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      COUNTRYCODE , ")
                sqlStat.AppendLine("      PORTCODE , ")
                sqlStat.AppendLine("      AREACODE , ")
                sqlStat.AppendLine("      AREANAME , ")
                sqlStat.AppendLine("      GROUPCODE , ")
                sqlStat.AppendLine("      GROUPNAME , ")
                sqlStat.AppendLine("      REMARK , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine("      @COMPCODE , ")
                sqlStat.AppendLine("      @ORGCODE , ")
                sqlStat.AppendLine("      @STYMD , ")
                sqlStat.AppendLine("      @ENDYMD , ")
                sqlStat.AppendLine("      @COUNTRYCODE , ")
                sqlStat.AppendLine("      @PORTCODE , ")
                sqlStat.AppendLine("      @AREACODE , ")
                sqlStat.AppendLine("      @AREANAME , ")
                sqlStat.AppendLine("      @GROUPCODE , ")
                sqlStat.AppendLine("      @GROUPNAME , ")
                sqlStat.AppendLine("      @REMARK , ")
                sqlStat.AppendLine("      @DELFLG , ")
                sqlStat.AppendLine(" @INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@ORGCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ORGCODE"))
                        .Add("@STYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COUNTRYCODE"))
                        .Add("@PORTCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PORTCODE"))
                        .Add("@AREACODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("AREACODE"))
                        .Add("@AREANAME", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("AREANAME"))
                        .Add("@GROUPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("GROUPCODE"))
                        .Add("@GROUPNAME", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("GROUPNAME"))
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0002_PORT"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 港マスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0016_PORTAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,UPDTERMID     = @UPDTERMID ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE APPLYID       = @APPLYID")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("APPLYID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            li.Add(Convert.ToString(dtRow.Item("APPLYID")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li

        End Function
    End Class

    ''' <summary>
    ''' 費用項目マスタ関連処理
    ''' </summary>
    Private Class GBM00010
        Inherits ApprovalMasterClass '基底クラスを継承
        Private Const CONST_MAPID As String = "GBM00010"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyCharge"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド
            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ID
            dt.Columns.Add("COMPCODE", GetType(String))             '会社コード
            dt.Columns.Add("COSTCODE", GetType(String))             '費用コード
            dt.Columns.Add("LDKBN", GetType(String))                '発着区分
            dt.Columns.Add("STYMD", GetType(String))                '有効開始日
            dt.Columns.Add("ENDYMD", GetType(String))               '有効終了日
            dt.Columns.Add("CLASS1", GetType(String))               '分類１
            dt.Columns.Add("CLASS2", GetType(String))               '分類２(売上内訳)
            dt.Columns.Add("CLASS3", GetType(String))               '分類３(費用内訳)
            dt.Columns.Add("CLASS4", GetType(String))               '分類４(発生区分)
            dt.Columns.Add("CLASS5", GetType(String))               '分類５(手配要否)
            dt.Columns.Add("CLASS6", GetType(String))               '分類６(税区分)
            dt.Columns.Add("CLASS7", GetType(String))               '分類７(発生ACTY)
            dt.Columns.Add("CLASS8", GetType(String))               '分類８(US$入力)
            dt.Columns.Add("CLASS9", GetType(String))               '分類９(per B/L)
            dt.Columns.Add("CLASS10", GetType(String))              '分類１０(デマレッジ終端費用コード)
            dt.Columns.Add("SALESBR", GetType(String))              'セールスBR
            dt.Columns.Add("OPERATIONBR", GetType(String))          '移動BR
            dt.Columns.Add("REPAIRBR", GetType(String))             '修理BR
            dt.Columns.Add("SALES", GetType(String))                '受注
            dt.Columns.Add("BL", GetType(String))                   'BL
            dt.Columns.Add("TANKOPE", GetType(String))              '手配
            dt.Columns.Add("NONBR", GetType(String))                'その他経費
            dt.Columns.Add("SOA", GetType(String))                  '精算
            dt.Columns.Add("NAMES", GetType(String))                '費用名称（短）
            dt.Columns.Add("NAMEL", GetType(String))                '費用名称（長）
            dt.Columns.Add("NAMESJP", GetType(String))              '費用名称（短）JP
            dt.Columns.Add("NAMELJP", GetType(String))              '費用名称（長）JP
            dt.Columns.Add("SOACODE", GetType(String))              'ＳＯＡコード
            dt.Columns.Add("DATA", GetType(String))                 'データ
            dt.Columns.Add("JOTCODE", GetType(String))              'ＪＯＴコード
            dt.Columns.Add("ACCODE", GetType(String))               'ＡＣコード
            dt.Columns.Add("CRACCOUNT", GetType(String))            '会計勘定科目(貸方)
            dt.Columns.Add("DBACCOUNT", GetType(String))            '会計勘定科目(借方)
            dt.Columns.Add("CRACCOUNTFORIGN", GetType(String))      '会計勘定科目(貸方)外貨
            dt.Columns.Add("DBACCOUNTFORIGN", GetType(String))      '会計勘定科目(借方)外貨
            dt.Columns.Add("OFFCRACCOUNT", GetType(String))         '会計勘定科目(貸方)相殺用
            dt.Columns.Add("OFFDBACCOUNT", GetType(String))         '会計勘定科目(借方)相殺用
            dt.Columns.Add("OFFCRACCOUNTFORIGN", GetType(String))   '会計勘定科目(貸方)外貨相殺用
            dt.Columns.Add("OFFDBACCOUNTFORIGN", GetType(String))   '会計勘定科目(借方)外貨相殺用
            dt.Columns.Add("ACCAMPCODE", GetType(String))           '会計・会社コード
            dt.Columns.Add("ACTORICODE", GetType(String))           '会計・取引先コード
            dt.Columns.Add("ACTORICODES", GetType(String))          '会計・取引先支店コード
            dt.Columns.Add("CRGENERALPURPOSE", GetType(String))     '汎用補助１採用区分(貸方)
            dt.Columns.Add("DBGENERALPURPOSE", GetType(String))     '汎用補助１採用区分(借方)
            dt.Columns.Add("CRSEGMENT1", GetType(String))           'セグメント１(貸方)
            dt.Columns.Add("DBSEGMENT1", GetType(String))           'セグメント１(借方)
            dt.Columns.Add("REMARK", GetType(String))               '備考
            dt.Columns.Add("DELFLG", GetType(String))               '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("STEPSTATE", GetType(String))            'ステップ状況
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(CA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,CA.COMPCODE")
            sqlStat.AppendLine("      ,CA.COSTCODE")
            sqlStat.AppendLine("      ,CA.LDKBN")
            sqlStat.AppendLine("      ,convert(nvarchar, CA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, CA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,CA.CLASS1")
            sqlStat.AppendLine("      ,CA.CLASS2")
            sqlStat.AppendLine("      ,CA.CLASS3")
            sqlStat.AppendLine("      ,CA.CLASS4")
            sqlStat.AppendLine("      ,CA.CLASS5")
            sqlStat.AppendLine("      ,CA.CLASS6")
            sqlStat.AppendLine("      ,CA.CLASS7")
            sqlStat.AppendLine("      ,CA.CLASS8")
            sqlStat.AppendLine("      ,CA.CLASS9")
            sqlStat.AppendLine("      ,CA.CLASS10")
            sqlStat.AppendLine("      ,CA.SALESBR")
            sqlStat.AppendLine("      ,CA.OPERATIONBR")
            sqlStat.AppendLine("      ,CA.REPAIRBR")
            sqlStat.AppendLine("      ,CA.SALES")
            sqlStat.AppendLine("      ,CA.BL")
            sqlStat.AppendLine("      ,CA.TANKOPE")
            sqlStat.AppendLine("      ,CA.NONBR")
            sqlStat.AppendLine("      ,CA.SOA")
            sqlStat.AppendLine("      ,CA.NAMES")
            sqlStat.AppendLine("      ,CA.NAMEL")
            sqlStat.AppendLine("      ,CA.NAMESJP")
            sqlStat.AppendLine("      ,CA.NAMELJP")
            sqlStat.AppendLine("      ,CA.SOACODE")
            sqlStat.AppendLine("      ,CA.DATA")
            sqlStat.AppendLine("      ,CA.JOTCODE")
            sqlStat.AppendLine("      ,CA.ACCODE")
            sqlStat.AppendLine("      ,CA.CRACCOUNT")
            sqlStat.AppendLine("      ,CA.DBACCOUNT")
            sqlStat.AppendLine("      ,CA.CRACCOUNTFORIGN")
            sqlStat.AppendLine("      ,CA.DBACCOUNTFORIGN")
            sqlStat.AppendLine("      ,CA.OFFCRACCOUNT")
            sqlStat.AppendLine("      ,CA.OFFDBACCOUNT")
            sqlStat.AppendLine("      ,CA.OFFCRACCOUNTFORIGN")
            sqlStat.AppendLine("      ,CA.OFFDBACCOUNTFORIGN")
            sqlStat.AppendLine("      ,CA.ACCAMPCODE")
            sqlStat.AppendLine("      ,CA.ACTORICODE")
            sqlStat.AppendLine("      ,CA.ACTORICODES")
            sqlStat.AppendLine("      ,CA.CRGENERALPURPOSE")
            sqlStat.AppendLine("      ,CA.DBGENERALPURPOSE")
            sqlStat.AppendLine("      ,CA.CRSEGMENT1")
            sqlStat.AppendLine("      ,CA.DBSEGMENT1")
            sqlStat.AppendLine("      ,CA.REMARK")
            sqlStat.AppendLine("      ,CA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0018_CHARGECODEAPPLY CA") '費用項目マスタ(申請)
            sqlStat.AppendLine("    ON  CA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  CA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  CA.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function

        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 費用項目マスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0010_CHARGECODE ")
                sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE ")
                sqlStat.AppendLine("   AND COSTCODE = @COSTCODE ")
                sqlStat.AppendLine("   AND LDKBN    = @LDKBN ")
                sqlStat.AppendLine("   AND STYMD    = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0010_CHARGECODE ")
                sqlStat.AppendLine("  SET ENDYMD = @ENDYMD , ")
                sqlStat.AppendLine("      CLASS1 = @CLASS1 , ")
                sqlStat.AppendLine("      CLASS2 = @CLASS2 , ")
                sqlStat.AppendLine("      CLASS3 = @CLASS3 , ")
                sqlStat.AppendLine("      CLASS4 = @CLASS4 , ")
                sqlStat.AppendLine("      CLASS5 = @CLASS5 , ")
                sqlStat.AppendLine("      CLASS6 = @CLASS6 , ")
                sqlStat.AppendLine("      CLASS7 = @CLASS7 , ")
                sqlStat.AppendLine("      CLASS8 = @CLASS8 , ")
                sqlStat.AppendLine("      CLASS9 = @CLASS9 , ")
                sqlStat.AppendLine("      CLASS10 = @CLASS10 , ")
                sqlStat.AppendLine("      SALESBR = @SALESBR , ")
                sqlStat.AppendLine("      OPERATIONBR = @OPERATIONBR , ")
                sqlStat.AppendLine("      REPAIRBR = @REPAIRBR , ")
                sqlStat.AppendLine("      SALES = @SALES , ")
                sqlStat.AppendLine("      BL = @BL , ")
                sqlStat.AppendLine("      TANKOPE = @TANKOPE , ")
                sqlStat.AppendLine("      NONBR = @NONBR , ")
                sqlStat.AppendLine("      SOA = @SOA , ")
                sqlStat.AppendLine("      NAMES = @NAMES , ")
                sqlStat.AppendLine("      NAMEL = @NAMEL , ")
                sqlStat.AppendLine("      NAMESJP = @NAMESJP , ")
                sqlStat.AppendLine("      NAMELJP = @NAMELJP , ")
                sqlStat.AppendLine("      SOACODE = @SOACODE , ")
                sqlStat.AppendLine("      DATA    = @DATA , ")
                sqlStat.AppendLine("      JOTCODE = @JOTCODE , ")
                sqlStat.AppendLine("      ACCODE  = @ACCODE , ")
                sqlStat.AppendLine("      CRACCOUNT = @CRACCOUNT , ")
                sqlStat.AppendLine("      DBACCOUNT = @DBACCOUNT , ")
                sqlStat.AppendLine("      CRACCOUNTFORIGN = @CRACCOUNTFORIGN , ")
                sqlStat.AppendLine("      DBACCOUNTFORIGN = @DBACCOUNTFORIGN , ")
                sqlStat.AppendLine("      OFFCRACCOUNT = @OFFCRACCOUNT , ")
                sqlStat.AppendLine("      OFFDBACCOUNT = @OFFDBACCOUNT , ")
                sqlStat.AppendLine("      OFFCRACCOUNTFORIGN = @OFFCRACCOUNTFORIGN , ")
                sqlStat.AppendLine("      OFFDBACCOUNTFORIGN = @OFFDBACCOUNTFORIGN , ")
                sqlStat.AppendLine("      ACCAMPCODE = @ACCAMPCODE , ")
                sqlStat.AppendLine("      ACTORICODE = @ACTORICODE , ")
                sqlStat.AppendLine("      ACTORICODES = @ACTORICODES , ")
                sqlStat.AppendLine("      CRGENERALPURPOSE = @CRGENERALPURPOSE , ")
                sqlStat.AppendLine("      DBGENERALPURPOSE = @DBGENERALPURPOSE , ")
                sqlStat.AppendLine("      CRSEGMENT1 = @CRSEGMENT1 , ")
                sqlStat.AppendLine("      DBSEGMENT1 = @DBSEGMENT1 , ")
                sqlStat.AppendLine("      REMARK = @REMARK , ")
                sqlStat.AppendLine("      DELFLG = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD             = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER            = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID          = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD         = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE              = @COMPCODE ")
                sqlStat.AppendLine("   AND COSTCODE       = @COSTCODE ")
                sqlStat.AppendLine("   AND STYMD       = @STYMD ")
                sqlStat.AppendLine("   ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0010_CHARGECODE ( ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      COSTCODE , ")
                sqlStat.AppendLine("      LDKBN , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      CLASS1 , ")
                sqlStat.AppendLine("      CLASS2 , ")
                sqlStat.AppendLine("      CLASS3 , ")
                sqlStat.AppendLine("      CLASS4 , ")
                sqlStat.AppendLine("      CLASS5 , ")
                sqlStat.AppendLine("      CLASS6 , ")
                sqlStat.AppendLine("      CLASS7 , ")
                sqlStat.AppendLine("      CLASS8 , ")
                sqlStat.AppendLine("      CLASS9 , ")
                sqlStat.AppendLine("      CLASS10 , ")
                sqlStat.AppendLine("      SALESBR , ")
                sqlStat.AppendLine("      OPERATIONBR , ")
                sqlStat.AppendLine("      REPAIRBR , ")
                sqlStat.AppendLine("      SALES , ")
                sqlStat.AppendLine("      BL , ")
                sqlStat.AppendLine("      TANKOPE , ")
                sqlStat.AppendLine("      NONBR , ")
                sqlStat.AppendLine("      SOA , ")
                sqlStat.AppendLine("      NAMES , ")
                sqlStat.AppendLine("      NAMEL , ")
                sqlStat.AppendLine("      NAMESJP , ")
                sqlStat.AppendLine("      NAMELJP , ")
                sqlStat.AppendLine("      SOACODE , ")
                sqlStat.AppendLine("      DATA , ")
                sqlStat.AppendLine("      JOTCODE , ")
                sqlStat.AppendLine("      ACCODE , ")
                sqlStat.AppendLine("      CRACCOUNT , ")
                sqlStat.AppendLine("      DBACCOUNT , ")
                sqlStat.AppendLine("      CRACCOUNTFORIGN , ")
                sqlStat.AppendLine("      DBACCOUNTFORIGN , ")
                sqlStat.AppendLine("      OFFCRACCOUNT , ")
                sqlStat.AppendLine("      OFFDBACCOUNT , ")
                sqlStat.AppendLine("      OFFCRACCOUNTFORIGN , ")
                sqlStat.AppendLine("      OFFDBACCOUNTFORIGN , ")
                sqlStat.AppendLine("      ACCAMPCODE , ")
                sqlStat.AppendLine("      ACTORICODE , ")
                sqlStat.AppendLine("      ACTORICODES , ")
                sqlStat.AppendLine("      CRGENERALPURPOSE , ")
                sqlStat.AppendLine("      DBGENERALPURPOSE , ")
                sqlStat.AppendLine("      CRSEGMENT1 , ")
                sqlStat.AppendLine("      DBSEGMENT1 , ")
                sqlStat.AppendLine("      REMARK , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine("      @COMPCODE , ")
                sqlStat.AppendLine("      @COSTCODE , ")
                sqlStat.AppendLine("      @LDKBN , ")
                sqlStat.AppendLine("      @STYMD , ")
                sqlStat.AppendLine("      @ENDYMD , ")
                sqlStat.AppendLine("      @CLASS1 , ")
                sqlStat.AppendLine("      @CLASS2 , ")
                sqlStat.AppendLine("      @CLASS3 , ")
                sqlStat.AppendLine("      @CLASS4 , ")
                sqlStat.AppendLine("      @CLASS5 , ")
                sqlStat.AppendLine("      @CLASS6 , ")
                sqlStat.AppendLine("      @CLASS7 , ")
                sqlStat.AppendLine("      @CLASS8 , ")
                sqlStat.AppendLine("      @CLASS9 , ")
                sqlStat.AppendLine("      @CLASS10 , ")
                sqlStat.AppendLine("      @SALESBR , ")
                sqlStat.AppendLine("      @OPERATIONBR , ")
                sqlStat.AppendLine("      @REPAIRBR , ")
                sqlStat.AppendLine("      @SALES , ")
                sqlStat.AppendLine("      @BL , ")
                sqlStat.AppendLine("      @TANKOPE , ")
                sqlStat.AppendLine("      @NONBR , ")
                sqlStat.AppendLine("      @SOA , ")
                sqlStat.AppendLine("      @NAMES , ")
                sqlStat.AppendLine("      @NAMEL , ")
                sqlStat.AppendLine("      @NAMESJP , ")
                sqlStat.AppendLine("      @NAMELJP , ")
                sqlStat.AppendLine("      @SOACODE , ")
                sqlStat.AppendLine("      @DATA , ")
                sqlStat.AppendLine("      @JOTCODE , ")
                sqlStat.AppendLine("      @ACCODE , ")
                sqlStat.AppendLine("      @CRACCOUNT , ")
                sqlStat.AppendLine("      @DBACCOUNT , ")
                sqlStat.AppendLine("      @CRACCOUNTFORIGN , ")
                sqlStat.AppendLine("      @DBACCOUNTFORIGN , ")
                sqlStat.AppendLine("      @OFFCRACCOUNT , ")
                sqlStat.AppendLine("      @OFFDBACCOUNT , ")
                sqlStat.AppendLine("      @OFFCRACCOUNTFORIGN , ")
                sqlStat.AppendLine("      @OFFDBACCOUNTFORIGN , ")
                sqlStat.AppendLine("      @ACCAMPCODE , ")
                sqlStat.AppendLine("      @ACTORICODE , ")
                sqlStat.AppendLine("      @ACTORICODES , ")
                sqlStat.AppendLine("      @CRGENERALPURPOSE , ")
                sqlStat.AppendLine("      @DBGENERALPURPOSE , ")
                sqlStat.AppendLine("      @CRSEGMENT1 , ")
                sqlStat.AppendLine("      @DBSEGMENT1 , ")
                sqlStat.AppendLine("      @REMARK , ")
                sqlStat.AppendLine("      @DELFLG , ")
                sqlStat.AppendLine(" @INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@COSTCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COSTCODE"))
                        .Add("@LDKBN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LDKBN"))
                        .Add("@STYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@CLASS1", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS1"))
                        .Add("@CLASS2", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS2"))
                        .Add("@CLASS3", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS3"))
                        .Add("@CLASS4", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS4"))
                        .Add("@CLASS5", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS5"))
                        .Add("@CLASS6", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS6"))
                        .Add("@CLASS7", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS7"))
                        .Add("@CLASS8", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS8"))
                        .Add("@CLASS9", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS9"))
                        .Add("@CLASS10", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS10"))
                        .Add("@SALESBR", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("SALESBR"))
                        .Add("@OPERATIONBR", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OPERATIONBR"))
                        .Add("@REPAIRBR", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REPAIRBR"))
                        .Add("@SALES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("SALES"))
                        .Add("@BL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BL"))
                        .Add("@TANKOPE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TANKOPE"))
                        .Add("@NONBR", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NONBR"))
                        .Add("@SOA", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("SOA"))
                        .Add("@NAMES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMES"))
                        .Add("@NAMEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMEL"))
                        .Add("@NAMESJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMESJP"))
                        .Add("@NAMELJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMELJP"))
                        .Add("@SOACODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("SOACODE"))
                        .Add("@DATA", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DATA"))
                        .Add("@JOTCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("JOTCODE"))
                        .Add("@ACCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACCODE"))
                        .Add("@CRACCOUNT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CRACCOUNT"))
                        .Add("@DBACCOUNT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DBACCOUNT"))
                        .Add("@CRACCOUNTFORIGN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CRACCOUNTFORIGN"))
                        .Add("@DBACCOUNTFORIGN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DBACCOUNTFORIGN"))
                        .Add("@OFFCRACCOUNT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OFFCRACCOUNT"))
                        .Add("@OFFDBACCOUNT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OFFDBACCOUNT"))
                        .Add("@OFFCRACCOUNTFORIGN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OFFCRACCOUNTFORIGN"))
                        .Add("@OFFDBACCOUNTFORIGN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OFFDBACCOUNTFORIGN"))
                        .Add("@ACCAMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACCAMPCODE"))
                        .Add("@ACTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACTORICODE"))
                        .Add("@ACTORICODES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACTORICODES"))
                        .Add("@CRGENERALPURPOSE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CRGENERALPURPOSE"))
                        .Add("@DBGENERALPURPOSE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DBGENERALPURPOSE"))
                        .Add("@CRSEGMENT1", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CRSEGMENT1"))
                        .Add("@DBSEGMENT1", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DBSEGMENT1"))
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

            End Using

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 費用項目マスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0018_CHARGECODEAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,UPDTERMID     = @UPDTERMID ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE APPLYID       = @APPLYID")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("APPLYID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            li.Add(Convert.ToString(dtRow.Item("APPLYID")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li

        End Function
    End Class
    ''' <summary>
    ''' デポマスタ関連処理
    ''' </summary>
    Private Class GBM00003
        Inherits ApprovalMasterClass '基底クラスを継承
        Private Const CONST_MAPID As String = "GBM00003"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyDepot"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド
            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ID
            dt.Columns.Add("COMPCODE", GetType(String))             '会社コード
            dt.Columns.Add("ORGCODE", GetType(String))              '組織コード
            dt.Columns.Add("STYMD", GetType(String))                '有効開始日
            dt.Columns.Add("ENDYMD", GetType(String))               '有効終了日
            dt.Columns.Add("DEPOTCODE", GetType(String))            'デポコード
            dt.Columns.Add("NAMES", GetType(String))                'デポ名称（短）
            dt.Columns.Add("NAMEL", GetType(String))                'デポ名称（長）
            dt.Columns.Add("NAMESJP", GetType(String))              'デポ名称（短）JP
            dt.Columns.Add("NAMELJP", GetType(String))              'デポ名称（長）JP
            dt.Columns.Add("LOCATION", GetType(String))             'ロケーション
            dt.Columns.Add("POSTNUM1", GetType(String))             '郵便番号（上）
            dt.Columns.Add("POSTNUM2", GetType(String))             '郵便番号（下）
            dt.Columns.Add("ADDR", GetType(String))                 '業者住所
            dt.Columns.Add("ADDRJP", GetType(String))               '業者住所JP
            dt.Columns.Add("TEL", GetType(String))                  '電話番号
            dt.Columns.Add("FAX", GetType(String))                  'ＦＡＸ番号
            dt.Columns.Add("CONTACTORG", GetType(String))           '担当部署
            dt.Columns.Add("CONTACTPERSON", GetType(String))        '担当者
            dt.Columns.Add("CONTACTMAIL", GetType(String))          '担当メールアドレス
            dt.Columns.Add("FREETORAL", GetType(String))            'フリーデイ（トータル）
            dt.Columns.Add("FREEBEFORE", GetType(String))           'フリーデイ（洗浄前）
            dt.Columns.Add("FREEAFTER", GetType(String))            'フリーデイ（洗浄後）
            dt.Columns.Add("CURRENCYCODE", GetType(String))         '通貨コード
            dt.Columns.Add("EMPTYCLEAN", GetType(String))           '留置料／日（洗浄後）
            dt.Columns.Add("EMPTYDIRTY", GetType(String))           '留置料／日（洗浄前）
            dt.Columns.Add("LADEN", GetType(String))                '留置料／日（荷積）
            dt.Columns.Add("BILLINGMETHODS", GetType(String))       '請求方法
            dt.Columns.Add("ACCCURRENCYSEGMENT", GetType(String))   '経理円貨外貨区分
            dt.Columns.Add("BOTHCLASS", GetType(String))            '両建区分
            dt.Columns.Add("TORICOMP", GetType(String))             '取引先会社コード
            dt.Columns.Add("INCTORICODE", GetType(String))          '取引先コード（収入）
            dt.Columns.Add("EXPTORICODE", GetType(String))          '取引先コード（費用）
            dt.Columns.Add("DEPOSITDAY", GetType(String))           '入金期日
            dt.Columns.Add("DEPOSITADDMM", GetType(String))         '入金期日(加算月）
            dt.Columns.Add("OVERDRAWDAY", GetType(String))          '出金期日
            dt.Columns.Add("OVERDRAWADDMM", GetType(String))        '出金期日(加算月）
            dt.Columns.Add("HOLIDAYFLG", GetType(String))           '休日フラグ
            dt.Columns.Add("REMARK", GetType(String))               '備考
            dt.Columns.Add("DELFLG", GetType(String))               '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(DA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,DA.COMPCODE")
            sqlStat.AppendLine("      ,DA.ORGCODE")
            sqlStat.AppendLine("      ,convert(nvarchar, DA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, DA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,DA.DEPOTCODE")
            sqlStat.AppendLine("      ,DA.NAMES")
            sqlStat.AppendLine("      ,DA.NAMEL")
            sqlStat.AppendLine("      ,DA.NAMESJP")
            sqlStat.AppendLine("      ,DA.NAMELJP")
            sqlStat.AppendLine("      ,DA.LOCATION")
            sqlStat.AppendLine("      ,DA.POSTNUM1")
            sqlStat.AppendLine("      ,DA.POSTNUM2")
            sqlStat.AppendLine("      ,DA.ADDR")
            sqlStat.AppendLine("      ,DA.ADDRJP")
            sqlStat.AppendLine("      ,DA.TEL")
            sqlStat.AppendLine("      ,DA.FAX")
            sqlStat.AppendLine("      ,DA.CONTACTORG")
            sqlStat.AppendLine("      ,DA.CONTACTPERSON")
            sqlStat.AppendLine("      ,DA.CONTACTMAIL")
            sqlStat.AppendLine("      ,DA.FREETORAL")
            sqlStat.AppendLine("      ,DA.FREEBEFORE")
            sqlStat.AppendLine("      ,DA.FREEAFTER")
            sqlStat.AppendLine("      ,DA.CURRENCYCODE")
            sqlStat.AppendLine("      ,DA.EMPTYCLEAN")
            sqlStat.AppendLine("      ,DA.EMPTYDIRTY")
            sqlStat.AppendLine("      ,DA.LADEN")
            sqlStat.AppendLine("      ,DA.BILLINGMETHODS")
            sqlStat.AppendLine("      ,DA.ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("      ,DA.BOTHCLASS")
            sqlStat.AppendLine("      ,DA.TORICOMP")
            sqlStat.AppendLine("      ,DA.INCTORICODE")
            sqlStat.AppendLine("      ,DA.EXPTORICODE")
            sqlStat.AppendLine("      ,DA.DEPOSITDAY")
            sqlStat.AppendLine("      ,DA.DEPOSITADDMM")
            sqlStat.AppendLine("      ,DA.OVERDRAWDAY")
            sqlStat.AppendLine("      ,DA.OVERDRAWADDMM")
            sqlStat.AppendLine("      ,DA.HOLIDAYFLG")
            sqlStat.AppendLine("      ,DA.REMARK")
            sqlStat.AppendLine("      ,DA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0017_DEPOTAPPLY DA") 'デポマスタ(申請)
            sqlStat.AppendLine("    ON  DA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  DA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  DA.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function

        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' デポマスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0003_DEPOT ")
                sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE ")
                sqlStat.AppendLine("   AND ORGCODE = @ORGCODE ")
                sqlStat.AppendLine("   AND STYMD = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0003_DEPOT ")
                sqlStat.AppendLine("  SET ENDYMD = @ENDYMD , ")
                sqlStat.AppendLine("      DEPOTCODE = @DEPOTCODE , ")
                sqlStat.AppendLine("      NAMES = @NAMES , ")
                sqlStat.AppendLine("      NAMEL = @NAMEL , ")
                sqlStat.AppendLine("      NAMESJP = @NAMESJP , ")
                sqlStat.AppendLine("      NAMELJP = @NAMELJP , ")
                sqlStat.AppendLine("      LOCATION = @LOCATION , ")
                sqlStat.AppendLine("      POSTNUM1 = @POSTNUM1 , ")
                sqlStat.AppendLine("      POSTNUM2 = @POSTNUM2 , ")
                sqlStat.AppendLine("      ADDR = @ADDR , ")
                sqlStat.AppendLine("      ADDRJP = @ADDRJP , ")
                sqlStat.AppendLine("      TEL = @TEL , ")
                sqlStat.AppendLine("      FAX = @FAX , ")
                sqlStat.AppendLine("      CONTACTORG = @CONTACTORG , ")
                sqlStat.AppendLine("      CONTACTPERSON = @CONTACTPERSON , ")
                sqlStat.AppendLine("      CONTACTMAIL = @CONTACTMAIL , ")
                sqlStat.AppendLine("      FREETORAL   = @FREETORAL , ")
                sqlStat.AppendLine("      FREEBEFORE  = @FREEBEFORE , ")
                sqlStat.AppendLine("      FREEAFTER   = @FREEAFTER , ")
                sqlStat.AppendLine("      CURRENCYCODE = @CURRENCYCODE , ")
                sqlStat.AppendLine("      EMPTYCLEAN   = @EMPTYCLEAN , ")
                sqlStat.AppendLine("      EMPTYDIRTY   = @EMPTYDIRTY , ")
                sqlStat.AppendLine("      LADEN        = @LADEN , ")
                sqlStat.AppendLine("      BILLINGMETHODS = @BILLINGMETHODS , ")
                sqlStat.AppendLine("      ACCCURRENCYSEGMENT = @ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      BOTHCLASS = @BOTHCLASS , ")
                sqlStat.AppendLine("      TORICOMP = @TORICOMP , ")
                sqlStat.AppendLine("      INCTORICODE = @INCTORICODE , ")
                sqlStat.AppendLine("      EXPTORICODE = @EXPTORICODE , ")
                sqlStat.AppendLine("      DEPOSITDAY = @DEPOSITDAY , ")
                sqlStat.AppendLine("      DEPOSITADDMM = @DEPOSITADDMM , ")
                sqlStat.AppendLine("      OVERDRAWDAY = @OVERDRAWDAY , ")
                sqlStat.AppendLine("      OVERDRAWADDMM = @OVERDRAWADDMM , ")
                sqlStat.AppendLine("      HOLIDAYFLG = @HOLIDAYFLG , ")
                sqlStat.AppendLine("      REMARK = @REMARK , ")
                sqlStat.AppendLine("      DELFLG = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD             = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER            = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID          = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD         = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE              = @COMPCODE ")
                sqlStat.AppendLine("   AND ORGCODE       = @ORGCODE ")
                sqlStat.AppendLine("   AND STYMD       = @STYMD ")
                sqlStat.AppendLine("   ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0003_DEPOT ( ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      ORGCODE , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      DEPOTCODE , ")
                sqlStat.AppendLine("      NAMES , ")
                sqlStat.AppendLine("      NAMEL , ")
                sqlStat.AppendLine("      NAMESJP , ")
                sqlStat.AppendLine("      NAMELJP , ")
                sqlStat.AppendLine("      LOCATION , ")
                sqlStat.AppendLine("      POSTNUM1 , ")
                sqlStat.AppendLine("      POSTNUM2 , ")
                sqlStat.AppendLine("      ADDR , ")
                sqlStat.AppendLine("      ADDRJP , ")
                sqlStat.AppendLine("      TEL , ")
                sqlStat.AppendLine("      FAX , ")
                sqlStat.AppendLine("      CONTACTORG , ")
                sqlStat.AppendLine("      CONTACTPERSON , ")
                sqlStat.AppendLine("      CONTACTMAIL , ")
                sqlStat.AppendLine("      FREETORAL , ")
                sqlStat.AppendLine("      FREEBEFORE , ")
                sqlStat.AppendLine("      FREEAFTER , ")
                sqlStat.AppendLine("      CURRENCYCODE , ")
                sqlStat.AppendLine("      EMPTYCLEAN , ")
                sqlStat.AppendLine("      EMPTYDIRTY , ")
                sqlStat.AppendLine("      LADEN , ")
                sqlStat.AppendLine("      BILLINGMETHODS , ")
                sqlStat.AppendLine("      ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      BOTHCLASS , ")
                sqlStat.AppendLine("      TORICOMP , ")
                sqlStat.AppendLine("      INCTORICODE , ")
                sqlStat.AppendLine("      EXPTORICODE , ")
                sqlStat.AppendLine("      DEPOSITDAY , ")
                sqlStat.AppendLine("      DEPOSITADDMM , ")
                sqlStat.AppendLine("      OVERDRAWDAY , ")
                sqlStat.AppendLine("      OVERDRAWADDMM , ")
                sqlStat.AppendLine("      HOLIDAYFLG , ")
                sqlStat.AppendLine("      REMARK , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine("      @COMPCODE , ")
                sqlStat.AppendLine("      @ORGCODE , ")
                sqlStat.AppendLine("      @STYMD , ")
                sqlStat.AppendLine("      @ENDYMD , ")
                sqlStat.AppendLine("      @DEPOTCODE , ")
                sqlStat.AppendLine("      @NAMES , ")
                sqlStat.AppendLine("      @NAMEL , ")
                sqlStat.AppendLine("      @NAMESJP , ")
                sqlStat.AppendLine("      @NAMELJP , ")
                sqlStat.AppendLine("      @LOCATION , ")
                sqlStat.AppendLine("      @POSTNUM1 , ")
                sqlStat.AppendLine("      @POSTNUM2 , ")
                sqlStat.AppendLine("      @ADDR , ")
                sqlStat.AppendLine("      @ADDRJP , ")
                sqlStat.AppendLine("      @TEL , ")
                sqlStat.AppendLine("      @FAX , ")
                sqlStat.AppendLine("      @CONTACTORG , ")
                sqlStat.AppendLine("      @CONTACTPERSON , ")
                sqlStat.AppendLine("      @CONTACTMAIL , ")
                sqlStat.AppendLine("      @FREETORAL , ")
                sqlStat.AppendLine("      @FREEBEFORE , ")
                sqlStat.AppendLine("      @FREEAFTER , ")
                sqlStat.AppendLine("      @CURRENCYCODE , ")
                sqlStat.AppendLine("      @EMPTYCLEAN , ")
                sqlStat.AppendLine("      @EMPTYDIRTY , ")
                sqlStat.AppendLine("      @LADEN , ")
                sqlStat.AppendLine("      @BILLINGMETHODS , ")
                sqlStat.AppendLine("      @ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      @BOTHCLASS , ")
                sqlStat.AppendLine("      @TORICOMP , ")
                sqlStat.AppendLine("      @INCTORICODE , ")
                sqlStat.AppendLine("      @EXPTORICODE , ")
                sqlStat.AppendLine("      @DEPOSITDAY , ")
                sqlStat.AppendLine("      @DEPOSITADDMM , ")
                sqlStat.AppendLine("      @OVERDRAWDAY , ")
                sqlStat.AppendLine("      @OVERDRAWADDMM , ")
                sqlStat.AppendLine("      @HOLIDAYFLG , ")
                sqlStat.AppendLine("      @REMARK , ")
                sqlStat.AppendLine("      @DELFLG , ")
                sqlStat.AppendLine(" @INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@ORGCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ORGCODE"))
                        .Add("@STYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@DEPOTCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEPOTCODE"))
                        .Add("@NAMES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMES"))
                        .Add("@NAMEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMEL"))
                        .Add("@NAMESJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMESJP"))
                        .Add("@NAMELJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMELJP"))
                        .Add("@LOCATION", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LOCATION"))
                        .Add("@POSTNUM1", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("POSTNUM1"))
                        .Add("@POSTNUM2", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("POSTNUM2"))
                        .Add("@ADDR", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ADDR"))
                        .Add("@ADDRJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ADDRJP"))
                        .Add("@TEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TEL"))
                        .Add("@FAX", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FAX"))
                        .Add("@CONTACTORG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONTACTORG"))
                        .Add("@CONTACTPERSON", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONTACTPERSON"))
                        .Add("@CONTACTMAIL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONTACTMAIL"))
                        .Add("@FREETORAL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FREETORAL"))
                        .Add("@FREEBEFORE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FREEBEFORE"))
                        .Add("@FREEAFTER", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FREEAFTER"))
                        .Add("@CURRENCYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CURRENCYCODE"))
                        .Add("@EMPTYCLEAN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("EMPTYCLEAN"))
                        .Add("@EMPTYDIRTY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("EMPTYDIRTY"))
                        .Add("@LADEN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LADEN"))
                        .Add("@BILLINGMETHODS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BILLINGMETHODS"))
                        .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACCCURRENCYSEGMENT"))
                        .Add("@BOTHCLASS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BOTHCLASS"))
                        .Add("@TORICOMP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TORICOMP"))
                        .Add("@INCTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("INCTORICODE"))
                        .Add("@EXPTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("EXPTORICODE"))
                        .Add("@DEPOSITDAY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEPOSITDAY"))
                        .Add("@DEPOSITADDMM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEPOSITADDMM"))
                        .Add("@OVERDRAWDAY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OVERDRAWDAY"))
                        .Add("@OVERDRAWADDMM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OVERDRAWADDMM"))
                        .Add("@HOLIDAYFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("HOLIDAYFLG"))
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0003_DEPOT"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' デポマスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0017_DEPOTAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,UPDTERMID     = @UPDTERMID ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE APPLYID       = @APPLYID")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("APPLYID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            li.Add(Convert.ToString(dtRow.Item("APPLYID")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li

        End Function
    End Class

    ''' <summary>
    ''' 業者マスタ関連処理
    ''' </summary>
    Private Class GBM00005
        Inherits ApprovalMasterClass '基底クラスを継承
        Private Const CONST_MAPID As String = "GBM00005"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyTrader"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド
            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ID
            dt.Columns.Add("COMPCODE", GetType(String))             '会社コード
            dt.Columns.Add("COUNTRYCODE", GetType(String))          '国コード
            dt.Columns.Add("CARRIERCODE", GetType(String))          '業者コード
            dt.Columns.Add("STYMD", GetType(String))                '開始年月日
            dt.Columns.Add("ENDYMD", GetType(String))               '終了年月日
            dt.Columns.Add("CLASS", GetType(String))                '分類
            dt.Columns.Add("NAMES", GetType(String))                '業者名称（短）
            dt.Columns.Add("NAMEL", GetType(String))                '業者名称（長）
            dt.Columns.Add("NAMESJP", GetType(String))              '業者名称（短）JP
            dt.Columns.Add("NAMELJP", GetType(String))              '業者名称（長）JP
            dt.Columns.Add("CARRIERBLNAME", GetType(String))        '業者-BL名称
            dt.Columns.Add("POSTNUM1", GetType(String))             '郵便番号（上）
            dt.Columns.Add("POSTNUM2", GetType(String))             '郵便番号（下）
            dt.Columns.Add("ADDR", GetType(String))                 '業者住所
            dt.Columns.Add("ADDRJP", GetType(String))               '業者住所JP
            dt.Columns.Add("TEL", GetType(String))                  '電話番号
            dt.Columns.Add("FAX", GetType(String))                  'ＦＡＸ番号
            dt.Columns.Add("CONTACTORG", GetType(String))           '担当部署
            dt.Columns.Add("CONTACTPERSON", GetType(String))        '担当者
            dt.Columns.Add("CONTACTMAIL", GetType(String))          '担当メールアドレス
            dt.Columns.Add("MAIL_ORGANIZER", GetType(String))       'オーガナイザーメールアドレス
            dt.Columns.Add("MAIL_POL", GetType(String))             'ＰＯＬメールアドレス
            dt.Columns.Add("MAIL_POD", GetType(String))             'ＰＯＤメールアドレス
            dt.Columns.Add("MORG", GetType(String))                 '管理部署
            dt.Columns.Add("REMARK", GetType(String))               '備考
            dt.Columns.Add("DELFLG", GetType(String))               '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("STEPSTATE", GetType(String))            'ステップ状況
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(TA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,TA.COMPCODE")
            sqlStat.AppendLine("      ,TA.COUNTRYCODE")
            sqlStat.AppendLine("      ,TA.CARRIERCODE")
            sqlStat.AppendLine("      ,convert(nvarchar, TA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, TA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,TA.CLASS")
            sqlStat.AppendLine("      ,TA.NAMES")
            sqlStat.AppendLine("      ,TA.NAMEL")
            sqlStat.AppendLine("      ,TA.NAMESJP")
            sqlStat.AppendLine("      ,TA.NAMELJP")
            sqlStat.AppendLine("      ,TA.CARRIERBLNAME")
            sqlStat.AppendLine("      ,TA.POSTNUM1")
            sqlStat.AppendLine("      ,TA.POSTNUM2")
            sqlStat.AppendLine("      ,TA.ADDR")
            sqlStat.AppendLine("      ,TA.ADDRJP")
            sqlStat.AppendLine("      ,TA.TEL")
            sqlStat.AppendLine("      ,TA.FAX")
            sqlStat.AppendLine("      ,TA.CONTACTORG")
            sqlStat.AppendLine("      ,TA.CONTACTPERSON")
            sqlStat.AppendLine("      ,TA.CONTACTMAIL")
            sqlStat.AppendLine("      ,TA.MAIL_ORGANIZER")
            sqlStat.AppendLine("      ,TA.MAIL_POL")
            sqlStat.AppendLine("      ,TA.MAIL_POD")
            sqlStat.AppendLine("      ,TA.MORG")
            sqlStat.AppendLine("      ,TA.ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("      ,TA.BOTHCLASS")
            sqlStat.AppendLine("      ,TA.TORICOMP")
            sqlStat.AppendLine("      ,TA.INCTORICODE")
            sqlStat.AppendLine("      ,TA.EXPTORICODE")
            sqlStat.AppendLine("      ,TA.DEPOSITDAY")
            sqlStat.AppendLine("      ,TA.DEPOSITADDMM")
            sqlStat.AppendLine("      ,TA.OVERDRAWDAY")
            sqlStat.AppendLine("      ,TA.OVERDRAWADDMM")
            sqlStat.AppendLine("      ,TA.HOLIDAYFLG")
            sqlStat.AppendLine("      ,TA.REMARK")
            sqlStat.AppendLine("      ,TA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0019_TRADERAPPLY TA") '業者マスタ(申請)
            sqlStat.AppendLine("    ON  TA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  TA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  TA.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function

        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 業者マスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0005_TRADER ")
                sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE ")
                sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE ")
                sqlStat.AppendLine("   AND CARRIERCODE = @CARRIERCODE ")
                sqlStat.AppendLine("   AND STYMD = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0005_TRADER ")
                sqlStat.AppendLine("  SET ENDYMD = @ENDYMD , ")
                sqlStat.AppendLine("      CLASS = @CLASS , ")
                sqlStat.AppendLine("      NAMES = @NAMES , ")
                sqlStat.AppendLine("      NAMEL = @NAMEL , ")
                sqlStat.AppendLine("      NAMESJP = @NAMESJP , ")
                sqlStat.AppendLine("      NAMELJP = @NAMELJP , ")
                sqlStat.AppendLine("      CARRIERBLNAME = @CARRIERBLNAME , ")
                sqlStat.AppendLine("      POSTNUM1 = @POSTNUM1 , ")
                sqlStat.AppendLine("      POSTNUM2 = @POSTNUM2 , ")
                sqlStat.AppendLine("      ADDR = @ADDR , ")
                sqlStat.AppendLine("      ADDRJP = @ADDRJP , ")
                sqlStat.AppendLine("      TEL = @TEL , ")
                sqlStat.AppendLine("      FAX = @FAX , ")
                sqlStat.AppendLine("      CONTACTORG = @CONTACTORG , ")
                sqlStat.AppendLine("      CONTACTPERSON = @CONTACTPERSON , ")
                sqlStat.AppendLine("      CONTACTMAIL = @CONTACTMAIL , ")
                sqlStat.AppendLine("      MAIL_ORGANIZER = @MAIL_ORGANIZER , ")
                sqlStat.AppendLine("      MAIL_POL = @MAIL_POL , ")
                sqlStat.AppendLine("      MAIL_POD = @MAIL_POD , ")
                sqlStat.AppendLine("      MORG = @MORG , ")
                sqlStat.AppendLine("      ACCCURRENCYSEGMENT = @ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      BOTHCLASS = @BOTHCLASS , ")
                sqlStat.AppendLine("      TORICOMP = @TORICOMP , ")
                sqlStat.AppendLine("      INCTORICODE = @INCTORICODE , ")
                sqlStat.AppendLine("      EXPTORICODE = @EXPTORICODE , ")
                sqlStat.AppendLine("      DEPOSITDAY = @DEPOSITDAY , ")
                sqlStat.AppendLine("      DEPOSITADDMM = @DEPOSITADDMM , ")
                sqlStat.AppendLine("      OVERDRAWDAY = @OVERDRAWDAY , ")
                sqlStat.AppendLine("      OVERDRAWADDMM = @OVERDRAWADDMM , ")
                sqlStat.AppendLine("      HOLIDAYFLG = @HOLIDAYFLG , ")
                sqlStat.AppendLine("      REMARK = @REMARK , ")
                sqlStat.AppendLine("      DELFLG = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD             = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER            = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID          = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD         = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE          = @COMPCODE ")
                sqlStat.AppendLine("   AND COUNTRYCODE       = @COUNTRYCODE ")
                sqlStat.AppendLine("   AND CARRIERCODE       = @CARRIERCODE ")
                sqlStat.AppendLine("   AND STYMD       = @STYMD ")
                sqlStat.AppendLine("   ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0005_TRADER ( ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      COUNTRYCODE , ")
                sqlStat.AppendLine("      CARRIERCODE , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      CLASS , ")
                sqlStat.AppendLine("      NAMES , ")
                sqlStat.AppendLine("      NAMEL , ")
                sqlStat.AppendLine("      NAMESJP , ")
                sqlStat.AppendLine("      NAMELJP , ")
                sqlStat.AppendLine("      CARRIERBLNAME , ")
                sqlStat.AppendLine("      POSTNUM1 , ")
                sqlStat.AppendLine("      POSTNUM2 , ")
                sqlStat.AppendLine("      ADDR , ")
                sqlStat.AppendLine("      ADDRJP , ")
                sqlStat.AppendLine("      TEL , ")
                sqlStat.AppendLine("      FAX , ")
                sqlStat.AppendLine("      CONTACTORG , ")
                sqlStat.AppendLine("      CONTACTPERSON , ")
                sqlStat.AppendLine("      CONTACTMAIL , ")
                sqlStat.AppendLine("      MAIL_ORGANIZER , ")
                sqlStat.AppendLine("      MAIL_POL , ")
                sqlStat.AppendLine("      MAIL_POD , ")
                sqlStat.AppendLine("      MORG , ")
                sqlStat.AppendLine("      ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      BOTHCLASS , ")
                sqlStat.AppendLine("      TORICOMP , ")
                sqlStat.AppendLine("      INCTORICODE , ")
                sqlStat.AppendLine("      EXPTORICODE , ")
                sqlStat.AppendLine("      DEPOSITDAY , ")
                sqlStat.AppendLine("      DEPOSITADDMM , ")
                sqlStat.AppendLine("      OVERDRAWDAY , ")
                sqlStat.AppendLine("      OVERDRAWADDMM , ")
                sqlStat.AppendLine("      HOLIDAYFLG , ")
                sqlStat.AppendLine("      REMARK , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine("      @COMPCODE , ")
                sqlStat.AppendLine("      @COUNTRYCODE , ")
                sqlStat.AppendLine("      @CARRIERCODE , ")
                sqlStat.AppendLine("      @STYMD , ")
                sqlStat.AppendLine("      @ENDYMD , ")
                sqlStat.AppendLine("      @CLASS , ")
                sqlStat.AppendLine("      @NAMES , ")
                sqlStat.AppendLine("      @NAMEL , ")
                sqlStat.AppendLine("      @NAMESJP , ")
                sqlStat.AppendLine("      @NAMELJP , ")
                sqlStat.AppendLine("      @CARRIERBLNAME , ")
                sqlStat.AppendLine("      @POSTNUM1 , ")
                sqlStat.AppendLine("      @POSTNUM2 , ")
                sqlStat.AppendLine("      @ADDR , ")
                sqlStat.AppendLine("      @ADDRJP , ")
                sqlStat.AppendLine("      @TEL , ")
                sqlStat.AppendLine("      @FAX , ")
                sqlStat.AppendLine("      @CONTACTORG , ")
                sqlStat.AppendLine("      @CONTACTPERSON , ")
                sqlStat.AppendLine("      @CONTACTMAIL , ")
                sqlStat.AppendLine("      @MAIL_ORGANIZER , ")
                sqlStat.AppendLine("      @MAIL_POL , ")
                sqlStat.AppendLine("      @MAIL_POD , ")
                sqlStat.AppendLine("      @MORG , ")
                sqlStat.AppendLine("      @ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      @BOTHCLASS , ")
                sqlStat.AppendLine("      @TORICOMP , ")
                sqlStat.AppendLine("      @INCTORICODE , ")
                sqlStat.AppendLine("      @EXPTORICODE , ")
                sqlStat.AppendLine("      @DEPOSITDAY , ")
                sqlStat.AppendLine("      @DEPOSITADDMM , ")
                sqlStat.AppendLine("      @OVERDRAWDAY , ")
                sqlStat.AppendLine("      @OVERDRAWADDMM , ")
                sqlStat.AppendLine("      @HOLIDAYFLG , ")
                sqlStat.AppendLine("      @REMARK , ")
                sqlStat.AppendLine("      @DELFLG , ")
                sqlStat.AppendLine(" @INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COUNTRYCODE"))
                        .Add("@CARRIERCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CARRIERCODE"))
                        .Add("@STYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@CLASS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CLASS"))
                        .Add("@NAMES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMES"))
                        .Add("@NAMEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMEL"))
                        .Add("@NAMESJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMESJP"))
                        .Add("@NAMELJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMELJP"))
                        .Add("@CARRIERBLNAME", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CARRIERBLNAME"))
                        .Add("@POSTNUM1", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("POSTNUM1"))
                        .Add("@POSTNUM2", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("POSTNUM2"))
                        .Add("@ADDR", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ADDR"))
                        .Add("@ADDRJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ADDRJP"))
                        .Add("@TEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TEL"))
                        .Add("@FAX", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FAX"))
                        .Add("@CONTACTORG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONTACTORG"))
                        .Add("@CONTACTPERSON", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONTACTPERSON"))
                        .Add("@CONTACTMAIL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONTACTMAIL"))
                        .Add("@MAIL_ORGANIZER", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MAIL_ORGANIZER"))
                        .Add("@MAIL_POL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MAIL_POL"))
                        .Add("@MAIL_POD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MAIL_POD"))
                        .Add("@MORG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MORG"))
                        .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACCCURRENCYSEGMENT"))
                        .Add("@BOTHCLASS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BOTHCLASS"))
                        .Add("@TORICOMP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TORICOMP"))
                        .Add("@INCTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("INCTORICODE"))
                        .Add("@EXPTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("EXPTORICODE"))
                        .Add("@DEPOSITDAY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEPOSITDAY"))
                        .Add("@DEPOSITADDMM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEPOSITADDMM"))
                        .Add("@OVERDRAWDAY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OVERDRAWDAY"))
                        .Add("@OVERDRAWADDMM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OVERDRAWADDMM"))
                        .Add("@HOLIDAYFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("HOLIDAYFLG"))
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0005_TRADER"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 業者マスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0019_TRADERAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,UPDTERMID     = @UPDTERMID ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE APPLYID       = @APPLYID")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("APPLYID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            li.Add(Convert.ToString(dtRow.Item("APPLYID")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li

        End Function
    End Class

    ''' <summary>
    ''' 為替レートマスタ関連処理
    ''' </summary>
    Private Class GBM00020
        Inherits ApprovalMasterClass '基底クラスを継承
        Private Const CONST_MAPID As String = "GBM00020"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyExRate"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド
            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ID
            dt.Columns.Add("COMPCODE", GetType(String))             '会社コード
            dt.Columns.Add("COUNTRYCODE", GetType(String))          '国コード
            dt.Columns.Add("CURRENCYCODE", GetType(String))         '通貨コード
            dt.Columns.Add("TARGETYM", GetType(String))             '対象年月
            dt.Columns.Add("STYMD", GetType(String))                '有効開始日
            dt.Columns.Add("ENDYMD", GetType(String))               '有効終了日
            dt.Columns.Add("EXRATE", GetType(String))               '為替レート
            dt.Columns.Add("REMARK", GetType(String))               '備考
            dt.Columns.Add("DELFLG", GetType(String))               '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("STEPSTATE", GetType(String))            'ステップ状況
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(EA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,EA.COMPCODE")
            sqlStat.AppendLine("      ,EA.COUNTRYCODE")
            sqlStat.AppendLine("      ,EA.CURRENCYCODE")
            sqlStat.AppendLine("      ,convert(nvarchar, EA.TARGETYM , 111) as TARGETYM")
            sqlStat.AppendLine("      ,convert(nvarchar, EA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, EA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,EA.EXRATE")
            sqlStat.AppendLine("      ,EA.REMARK")
            sqlStat.AppendLine("      ,EA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0021_EXRATEAPPLY EA") '為替レートマスタ(申請)
            sqlStat.AppendLine("    ON  EA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  EA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  EA.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function

        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 為替レートマスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0020_EXRATE ")
                sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE ")
                sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE ")
                sqlStat.AppendLine("   AND CURRENCYCODE = @CURRENCYCODE ")
                sqlStat.AppendLine("   AND TARGETYM = @TARGETYM ")
                sqlStat.AppendLine("   AND STYMD = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0020_EXRATE ")
                sqlStat.AppendLine("  SET ENDYMD = @ENDYMD , ")
                sqlStat.AppendLine("      EXRATE = @EXRATE , ")
                sqlStat.AppendLine("      REMARK = @REMARK , ")
                sqlStat.AppendLine("      DELFLG = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD             = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER            = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID          = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD         = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE          = @COMPCODE ")
                sqlStat.AppendLine("   AND COUNTRYCODE       = @COUNTRYCODE ")
                sqlStat.AppendLine("   AND CURRENCYCODE      = @CURRENCYCODE ")
                sqlStat.AppendLine("   AND TARGETYM          = @TARGETYM ")
                sqlStat.AppendLine("   AND STYMD             = @STYMD ")
                sqlStat.AppendLine("   ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0020_EXRATE ( ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      COUNTRYCODE , ")
                sqlStat.AppendLine("      CURRENCYCODE , ")
                sqlStat.AppendLine("      TARGETYM , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      EXRATE , ")
                sqlStat.AppendLine("      REMARK , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine("      @COMPCODE , ")
                sqlStat.AppendLine("      @COUNTRYCODE , ")
                sqlStat.AppendLine("      @CURRENCYCODE , ")
                sqlStat.AppendLine("      @TARGETYM , ")
                sqlStat.AppendLine("      @STYMD , ")
                sqlStat.AppendLine("      @ENDYMD , ")
                sqlStat.AppendLine("      @EXRATE , ")
                sqlStat.AppendLine("      @REMARK , ")
                sqlStat.AppendLine("      @DELFLG , ")
                sqlStat.AppendLine(" @INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COUNTRYCODE"))
                        .Add("@CURRENCYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CURRENCYCODE"))
                        .Add("@TARGETYM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TARGETYM"))
                        .Add("@STYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@EXRATE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("EXRATE"))
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0020_EXRATE"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 為替レートマスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0021_EXRATEAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,UPDTERMID     = @UPDTERMID ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE APPLYID       = @APPLYID")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("APPLYID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            li.Add(Convert.ToString(dtRow.Item("APPLYID")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li

        End Function
    End Class

    ''' <summary>
    ''' タンクマスタ関連処理
    ''' </summary>
    Private Class GBM00006
        Inherits ApprovalMasterClass '基底クラスを継承
        Private Const CONST_MAPID As String = "GBM00006"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyTank"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド
            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ID
            dt.Columns.Add("COMPCODE", GetType(String))             '会社コード
            dt.Columns.Add("TANKNO", GetType(String))               'タンク番号
            dt.Columns.Add("STYMD", GetType(String))                '開始年月日
            dt.Columns.Add("ENDYMD", GetType(String))               '終了年月日
            dt.Columns.Add("PROPERTY", GetType(String))             '所属
            dt.Columns.Add("LMOF", GetType(String))                 '所有形態（自社、リース他）
            dt.Columns.Add("LEASESTAT", GetType(String))            'リース
            dt.Columns.Add("REPAIRSTAT", GetType(String))           '修理状態
            dt.Columns.Add("INSPECTDATE5", GetType(String))         '検査日(５年)
            dt.Columns.Add("INSPECTDATE2P5", GetType(String))       '検査日（２．５年）
            dt.Columns.Add("NEXTINSPECTDATE", GetType(String))      '次回検査日
            dt.Columns.Add("NEXTINSPECTTYPE", GetType(String))      '次回検査種別
            dt.Columns.Add("JAPFIREAPPROVED", GetType(String))      'JP消防検査有無
            dt.Columns.Add("MANUFACTURER", GetType(String))         '製造メーカー
            dt.Columns.Add("MANUFACTURESERIALNO", GetType(String))  '製造時の管理番号
            dt.Columns.Add("DATEOFMANUFACTURE", GetType(String))    '製造日
            dt.Columns.Add("MATERIAL", GetType(String))             '材質
            dt.Columns.Add("STRUCT", GetType(String))               '構造
            dt.Columns.Add("USDOTAPPROVED", GetType(String))        '荷重試験実施の有無（AARも兼ねる）
            dt.Columns.Add("NOMINALCAPACITY", GetType(String))      '公称容量
            dt.Columns.Add("TANKCAPACITY", GetType(String))         '容量
            dt.Columns.Add("MAXGROSSWEIGHT", GetType(String))       '最大重量
            dt.Columns.Add("NETWEIGHT", GetType(String))            '重量
            dt.Columns.Add("FREAMDIMENSION_H", GetType(String))     '外法寸法 高さ
            dt.Columns.Add("FREAMDIMENSION_W", GetType(String))     '外法寸法 横
            dt.Columns.Add("FREAMDIMENSION_L", GetType(String))     '外法寸法 縦
            dt.Columns.Add("HEATING", GetType(String))              '加熱ラインのライン数及び有効面積
            dt.Columns.Add("HEATING_SUB", GetType(String))          '加熱ラインのライン数及び有効面積サブ
            dt.Columns.Add("DISCHARGE", GetType(String))            '液出し口の位置（下部又は上部）
            dt.Columns.Add("NOOFBOTTMCLOSURES", GetType(String))    '液出し口における閉鎖装置の数
            dt.Columns.Add("IMCOCLASS", GetType(String))            'IMO上のポータブルタンクのタイプ規格（例：TXX)
            dt.Columns.Add("FOOTVALUETYPE", GetType(String))        'フート弁のメーカー
            dt.Columns.Add("BACKVALUETYPE", GetType(String))        '液出し口のバルブのメーカー
            dt.Columns.Add("TOPDISVALUETYPE", GetType(String))      '上部積込口のバルブの種類及びメーカー
            dt.Columns.Add("AIRINLETVALUE", GetType(String))        'エアラインのバルブの種類及びメーカ-
            dt.Columns.Add("BAFFLES", GetType(String))              '防波板の有無
            dt.Columns.Add("TYPEOFPREVACVALUE", GetType(String))    '安全弁の種類、使用及びメーカー
            dt.Columns.Add("BURSTDISCFITTED", GetType(String))      '破裂板の有無
            dt.Columns.Add("TYPEOFTHERM", GetType(String))          '温度計の種類
            dt.Columns.Add("TYPEOFMANLID_CENTER", GetType(String))  'マンホールの大きさセンター
            dt.Columns.Add("TYPEOFMANLID_FRONT", GetType(String))   'マンホールの大きさフロント
            dt.Columns.Add("TYPEOFMLSEAL", GetType(String))         'マンホールパッキンの種類
            dt.Columns.Add("WORKINGPRESSURE", GetType(String))      '常用圧力
            dt.Columns.Add("TESTPRESSURE", GetType(String))         '試験圧力
            dt.Columns.Add("REMARK1", GetType(String))              '備考１
            dt.Columns.Add("REMARK2", GetType(String))              '備考２
            dt.Columns.Add("FAULTS", GetType(String))               'その他記載事項
            dt.Columns.Add("BASERAGEYY", GetType(String))           '令年
            dt.Columns.Add("BASERAGEMM", GetType(String))           '令月
            dt.Columns.Add("BASERAGE", GetType(String))             '令累月
            dt.Columns.Add("BASELEASE", GetType(String))            '車両所有
            dt.Columns.Add("MARUKANSEAL", GetType(String))          'マルカンシール
            dt.Columns.Add("REMARK", GetType(String))               '備考
            dt.Columns.Add("DELFLG", GetType(String))               '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("STEPSTATE", GetType(String))            'ステップ状況
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(TA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,TA.COMPCODE")
            sqlStat.AppendLine("      ,TA.TANKNO")
            sqlStat.AppendLine("      ,convert(nvarchar, TA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, TA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,TA.PROPERTY")
            sqlStat.AppendLine("      ,TA.LMOF")
            sqlStat.AppendLine("      ,TA.LEASESTAT")
            sqlStat.AppendLine("      ,TA.REPAIRSTAT")
            sqlStat.AppendLine("      ,convert(nvarchar, TA.INSPECTDATE5 , 111) as INSPECTDATE5")
            sqlStat.AppendLine("      ,convert(nvarchar, TA.INSPECTDATE2P5 , 111) as INSPECTDATE2P5")
            sqlStat.AppendLine("      ,convert(nvarchar, TA.NEXTINSPECTDATE , 111) as NEXTINSPECTDATE")
            sqlStat.AppendLine("      ,TA.NEXTINSPECTTYPE")
            sqlStat.AppendLine("      ,TA.JAPFIREAPPROVED")
            sqlStat.AppendLine("      ,TA.MANUFACTURER")
            sqlStat.AppendLine("      ,TA.MANUFACTURESERIALNO")
            sqlStat.AppendLine("      ,convert(nvarchar, TA.DATEOFMANUFACTURE , 111) as DATEOFMANUFACTURE")
            sqlStat.AppendLine("      ,TA.MATERIAL")
            sqlStat.AppendLine("      ,TA.STRUCT")
            sqlStat.AppendLine("      ,TA.USDOTAPPROVED")
            sqlStat.AppendLine("      ,TA.NOMINALCAPACITY")
            sqlStat.AppendLine("      ,TA.TANKCAPACITY")
            sqlStat.AppendLine("      ,TA.MAXGROSSWEIGHT")
            sqlStat.AppendLine("      ,TA.NETWEIGHT")
            sqlStat.AppendLine("      ,TA.FREAMDIMENSION_H")
            sqlStat.AppendLine("      ,TA.FREAMDIMENSION_W")
            sqlStat.AppendLine("      ,TA.FREAMDIMENSION_L")
            sqlStat.AppendLine("      ,TA.HEATING")
            sqlStat.AppendLine("      ,TA.HEATING_SUB")
            sqlStat.AppendLine("      ,TA.DISCHARGE")
            sqlStat.AppendLine("      ,TA.NOOFBOTTMCLOSURES")
            sqlStat.AppendLine("      ,TA.IMCOCLASS")
            sqlStat.AppendLine("      ,TA.FOOTVALUETYPE")
            sqlStat.AppendLine("      ,TA.BACKVALUETYPE")
            sqlStat.AppendLine("      ,TA.TOPDISVALUETYPE")
            sqlStat.AppendLine("      ,TA.AIRINLETVALUE")
            sqlStat.AppendLine("      ,TA.BAFFLES")
            sqlStat.AppendLine("      ,TA.TYPEOFPREVACVALUE")
            sqlStat.AppendLine("      ,TA.BURSTDISCFITTED")
            sqlStat.AppendLine("      ,TA.TYPEOFTHERM")
            sqlStat.AppendLine("      ,TA.TYPEOFMANLID_CENTER")
            sqlStat.AppendLine("      ,TA.TYPEOFMANLID_FRONT")
            sqlStat.AppendLine("      ,TA.TYPEOFMLSEAL")
            sqlStat.AppendLine("      ,TA.WORKINGPRESSURE")
            sqlStat.AppendLine("      ,TA.TESTPRESSURE")
            sqlStat.AppendLine("      ,TA.REMARK1")
            sqlStat.AppendLine("      ,TA.REMARK2")
            sqlStat.AppendLine("      ,TA.FAULTS")
            sqlStat.AppendLine("      ,TA.BASERAGEYY")
            sqlStat.AppendLine("      ,TA.BASERAGEMM")
            sqlStat.AppendLine("      ,TA.BASERAGE")
            sqlStat.AppendLine("      ,TA.BASELEASE")
            sqlStat.AppendLine("      ,TA.MARUKANSEAL")
            sqlStat.AppendLine("      ,TA.REMARK")
            sqlStat.AppendLine("      ,TA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0022_TANKAPPLY TA") 'タンクマスタ(申請)
            sqlStat.AppendLine("    ON  TA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  TA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  TA.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function

        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' タンクマスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0006_TANK ")
                sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE ")
                sqlStat.AppendLine("   AND TANKNO = @TANKNO ")
                sqlStat.AppendLine("   AND STYMD = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0006_TANK ")
                sqlStat.AppendLine("  SET ENDYMD = @ENDYMD , ")
                sqlStat.AppendLine("      PROPERTY = @PROPERTY , ")
                sqlStat.AppendLine("      LMOF = @LMOF , ")
                sqlStat.AppendLine("      LEASESTAT = @LEASESTAT , ")
                sqlStat.AppendLine("      REPAIRSTAT = @REPAIRSTAT , ")
                sqlStat.AppendLine("      INSPECTDATE5 = @INSPECTDATE5 , ")
                sqlStat.AppendLine("      INSPECTDATE2P5 = @INSPECTDATE2P5 , ")
                sqlStat.AppendLine("      NEXTINSPECTDATE = @NEXTINSPECTDATE , ")
                sqlStat.AppendLine("      NEXTINSPECTTYPE = @NEXTINSPECTTYPE , ")
                sqlStat.AppendLine("      JAPFIREAPPROVED = @JAPFIREAPPROVED , ")
                sqlStat.AppendLine("      MANUFACTURER = @MANUFACTURER , ")
                sqlStat.AppendLine("      MANUFACTURESERIALNO = @MANUFACTURESERIALNO , ")
                sqlStat.AppendLine("      DATEOFMANUFACTURE = @DATEOFMANUFACTURE , ")
                sqlStat.AppendLine("      MATERIAL = @MATERIAL , ")
                sqlStat.AppendLine("      STRUCT = @STRUCT , ")
                sqlStat.AppendLine("      USDOTAPPROVED = @USDOTAPPROVED , ")
                sqlStat.AppendLine("      NOMINALCAPACITY = @NOMINALCAPACITY , ")
                sqlStat.AppendLine("      TANKCAPACITY = @TANKCAPACITY , ")
                sqlStat.AppendLine("      MAXGROSSWEIGHT = @MAXGROSSWEIGHT , ")
                sqlStat.AppendLine("      NETWEIGHT = @NETWEIGHT , ")
                sqlStat.AppendLine("      FREAMDIMENSION_H = @FREAMDIMENSION_H , ")
                sqlStat.AppendLine("      FREAMDIMENSION_W = @FREAMDIMENSION_W , ")
                sqlStat.AppendLine("      FREAMDIMENSION_L = @FREAMDIMENSION_L , ")
                sqlStat.AppendLine("      HEATING = @HEATING , ")
                sqlStat.AppendLine("      HEATING_SUB = @HEATING_SUB , ")
                sqlStat.AppendLine("      DISCHARGE = @DISCHARGE , ")
                sqlStat.AppendLine("      NOOFBOTTMCLOSURES = @NOOFBOTTMCLOSURES , ")
                sqlStat.AppendLine("      IMCOCLASS = @IMCOCLASS , ")
                sqlStat.AppendLine("      FOOTVALUETYPE = @FOOTVALUETYPE , ")
                sqlStat.AppendLine("      BACKVALUETYPE = @BACKVALUETYPE , ")
                sqlStat.AppendLine("      TOPDISVALUETYPE = @TOPDISVALUETYPE , ")
                sqlStat.AppendLine("      AIRINLETVALUE = @AIRINLETVALUE , ")
                sqlStat.AppendLine("      BAFFLES = @BAFFLES , ")
                sqlStat.AppendLine("      TYPEOFPREVACVALUE = @TYPEOFPREVACVALUE , ")
                sqlStat.AppendLine("      BURSTDISCFITTED = @BURSTDISCFITTED , ")
                sqlStat.AppendLine("      TYPEOFTHERM = @TYPEOFTHERM , ")
                sqlStat.AppendLine("      TYPEOFMANLID_CENTER = @TYPEOFMANLID_CENTER , ")
                sqlStat.AppendLine("      TYPEOFMANLID_FRONT = @TYPEOFMANLID_FRONT , ")
                sqlStat.AppendLine("      TYPEOFMLSEAL = @TYPEOFMLSEAL , ")
                sqlStat.AppendLine("      WORKINGPRESSURE = @WORKINGPRESSURE , ")
                sqlStat.AppendLine("      TESTPRESSURE = @TESTPRESSURE , ")
                sqlStat.AppendLine("      REMARK1 = @REMARK1 , ")
                sqlStat.AppendLine("      REMARK2 = @REMARK2 , ")
                sqlStat.AppendLine("      FAULTS = @FAULTS , ")
                sqlStat.AppendLine("      BASERAGEYY = @BASERAGEYY , ")
                sqlStat.AppendLine("      BASERAGEMM = @BASERAGEMM , ")
                sqlStat.AppendLine("      BASERAGE = @BASERAGE , ")
                sqlStat.AppendLine("      BASELEASE = @BASELEASE , ")
                sqlStat.AppendLine("      MARUKANSEAL = @MARUKANSEAL , ")
                sqlStat.AppendLine("      REMARK = @REMARK , ")
                sqlStat.AppendLine("      DELFLG = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD             = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER            = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID          = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD         = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE              = @COMPCODE ")
                sqlStat.AppendLine("   AND TANKNO       = @TANKNO ")
                sqlStat.AppendLine("   AND STYMD       = @STYMD ")
                sqlStat.AppendLine("   ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0006_TANK ( ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      TANKNO , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      PROPERTY , ")
                sqlStat.AppendLine("      LMOF , ")
                sqlStat.AppendLine("      LEASESTAT , ")
                sqlStat.AppendLine("      REPAIRSTAT , ")
                sqlStat.AppendLine("      INSPECTDATE5 , ")
                sqlStat.AppendLine("      INSPECTDATE2P5 , ")
                sqlStat.AppendLine("      NEXTINSPECTDATE , ")
                sqlStat.AppendLine("      NEXTINSPECTTYPE , ")
                sqlStat.AppendLine("      JAPFIREAPPROVED , ")
                sqlStat.AppendLine("      MANUFACTURER , ")
                sqlStat.AppendLine("      MANUFACTURESERIALNO , ")
                sqlStat.AppendLine("      DATEOFMANUFACTURE , ")
                sqlStat.AppendLine("      MATERIAL , ")
                sqlStat.AppendLine("      STRUCT , ")
                sqlStat.AppendLine("      USDOTAPPROVED , ")
                sqlStat.AppendLine("      NOMINALCAPACITY , ")
                sqlStat.AppendLine("      TANKCAPACITY , ")
                sqlStat.AppendLine("      MAXGROSSWEIGHT , ")
                sqlStat.AppendLine("      NETWEIGHT , ")
                sqlStat.AppendLine("      FREAMDIMENSION_H , ")
                sqlStat.AppendLine("      FREAMDIMENSION_W , ")
                sqlStat.AppendLine("      FREAMDIMENSION_L , ")
                sqlStat.AppendLine("      HEATING , ")
                sqlStat.AppendLine("      HEATING_SUB , ")
                sqlStat.AppendLine("      DISCHARGE , ")
                sqlStat.AppendLine("      NOOFBOTTMCLOSURES , ")
                sqlStat.AppendLine("      IMCOCLASS , ")
                sqlStat.AppendLine("      FOOTVALUETYPE , ")
                sqlStat.AppendLine("      BACKVALUETYPE , ")
                sqlStat.AppendLine("      TOPDISVALUETYPE , ")
                sqlStat.AppendLine("      AIRINLETVALUE , ")
                sqlStat.AppendLine("      BAFFLES , ")
                sqlStat.AppendLine("      TYPEOFPREVACVALUE , ")
                sqlStat.AppendLine("      BURSTDISCFITTED , ")
                sqlStat.AppendLine("      TYPEOFTHERM , ")
                sqlStat.AppendLine("      TYPEOFMANLID_CENTER , ")
                sqlStat.AppendLine("      TYPEOFMANLID_FRONT , ")
                sqlStat.AppendLine("      TYPEOFMLSEAL , ")
                sqlStat.AppendLine("      WORKINGPRESSURE , ")
                sqlStat.AppendLine("      TESTPRESSURE , ")
                sqlStat.AppendLine("      REMARK1 , ")
                sqlStat.AppendLine("      REMARK2 , ")
                sqlStat.AppendLine("      FAULTS , ")
                sqlStat.AppendLine("      BASERAGEYY , ")
                sqlStat.AppendLine("      BASERAGEMM , ")
                sqlStat.AppendLine("      BASERAGE , ")
                sqlStat.AppendLine("      BASELEASE , ")
                sqlStat.AppendLine("      MARUKANSEAL , ")
                sqlStat.AppendLine("      REMARK , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine("      @COMPCODE , ")
                sqlStat.AppendLine("      @TANKNO , ")
                sqlStat.AppendLine("      @STYMD , ")
                sqlStat.AppendLine("      @ENDYMD , ")
                sqlStat.AppendLine("      @PROPERTY , ")
                sqlStat.AppendLine("      @LMOF , ")
                sqlStat.AppendLine("      @LEASESTAT , ")
                sqlStat.AppendLine("      @REPAIRSTAT , ")
                sqlStat.AppendLine("      @INSPECTDATE5 , ")
                sqlStat.AppendLine("      @INSPECTDATE2P5 , ")
                sqlStat.AppendLine("      @NEXTINSPECTDATE , ")
                sqlStat.AppendLine("      @NEXTINSPECTTYPE , ")
                sqlStat.AppendLine("      @JAPFIREAPPROVED , ")
                sqlStat.AppendLine("      @MANUFACTURER , ")
                sqlStat.AppendLine("      @MANUFACTURESERIALNO , ")
                sqlStat.AppendLine("      @DATEOFMANUFACTURE , ")
                sqlStat.AppendLine("      @MATERIAL , ")
                sqlStat.AppendLine("      @STRUCT , ")
                sqlStat.AppendLine("      @USDOTAPPROVED , ")
                sqlStat.AppendLine("      @NOMINALCAPACITY , ")
                sqlStat.AppendLine("      @TANKCAPACITY , ")
                sqlStat.AppendLine("      @MAXGROSSWEIGHT , ")
                sqlStat.AppendLine("      @NETWEIGHT , ")
                sqlStat.AppendLine("      @FREAMDIMENSION_H , ")
                sqlStat.AppendLine("      @FREAMDIMENSION_W , ")
                sqlStat.AppendLine("      @FREAMDIMENSION_L , ")
                sqlStat.AppendLine("      @HEATING , ")
                sqlStat.AppendLine("      @HEATING_SUB , ")
                sqlStat.AppendLine("      @DISCHARGE , ")
                sqlStat.AppendLine("      @NOOFBOTTMCLOSURES , ")
                sqlStat.AppendLine("      @IMCOCLASS , ")
                sqlStat.AppendLine("      @FOOTVALUETYPE , ")
                sqlStat.AppendLine("      @BACKVALUETYPE , ")
                sqlStat.AppendLine("      @TOPDISVALUETYPE , ")
                sqlStat.AppendLine("      @AIRINLETVALUE , ")
                sqlStat.AppendLine("      @BAFFLES , ")
                sqlStat.AppendLine("      @TYPEOFPREVACVALUE , ")
                sqlStat.AppendLine("      @BURSTDISCFITTED , ")
                sqlStat.AppendLine("      @TYPEOFTHERM , ")
                sqlStat.AppendLine("      @TYPEOFMANLID_CENTER , ")
                sqlStat.AppendLine("      @TYPEOFMANLID_FRONT , ")
                sqlStat.AppendLine("      @TYPEOFMLSEAL , ")
                sqlStat.AppendLine("      @WORKINGPRESSURE , ")
                sqlStat.AppendLine("      @TESTPRESSURE , ")
                sqlStat.AppendLine("      @REMARK1 , ")
                sqlStat.AppendLine("      @REMARK2 , ")
                sqlStat.AppendLine("      @FAULTS , ")
                sqlStat.AppendLine("      @BASERAGEYY , ")
                sqlStat.AppendLine("      @BASERAGEMM , ")
                sqlStat.AppendLine("      @BASERAGE , ")
                sqlStat.AppendLine("      @BASELEASE , ")
                sqlStat.AppendLine("      @MARUKANSEAL , ")
                sqlStat.AppendLine("      @REMARK , ")
                sqlStat.AppendLine("      @DELFLG , ")
                sqlStat.AppendLine(" @INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@TANKNO", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TANKNO"))
                        .Add("@STYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@PROPERTY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("PROPERTY"))
                        .Add("@LMOF", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LMOF"))
                        .Add("@LEASESTAT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("LEASESTAT"))
                        .Add("@REPAIRSTAT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REPAIRSTAT"))
                        .Add("@INSPECTDATE5", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("INSPECTDATE5"))
                        .Add("@INSPECTDATE2P5", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("INSPECTDATE2P5"))
                        .Add("@NEXTINSPECTDATE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NEXTINSPECTDATE"))
                        .Add("@NEXTINSPECTTYPE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NEXTINSPECTTYPE"))
                        .Add("@JAPFIREAPPROVED", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("JAPFIREAPPROVED"))
                        .Add("@MANUFACTURER", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MANUFACTURER"))
                        .Add("@MANUFACTURESERIALNO", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MANUFACTURESERIALNO"))
                        .Add("@DATEOFMANUFACTURE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DATEOFMANUFACTURE"))
                        .Add("@MATERIAL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MATERIAL"))
                        .Add("@STRUCT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STRUCT"))
                        .Add("@USDOTAPPROVED", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("USDOTAPPROVED"))
                        .Add("@NOMINALCAPACITY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NOMINALCAPACITY"))
                        .Add("@TANKCAPACITY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TANKCAPACITY"))
                        .Add("@MAXGROSSWEIGHT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MAXGROSSWEIGHT"))
                        .Add("@NETWEIGHT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NETWEIGHT"))
                        .Add("@FREAMDIMENSION_H", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FREAMDIMENSION_H"))
                        .Add("@FREAMDIMENSION_W", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FREAMDIMENSION_W"))
                        .Add("@FREAMDIMENSION_L", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FREAMDIMENSION_L"))
                        .Add("@HEATING", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("HEATING"))
                        .Add("@HEATING_SUB", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("HEATING_SUB"))
                        .Add("@DISCHARGE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DISCHARGE"))
                        .Add("@NOOFBOTTMCLOSURES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NOOFBOTTMCLOSURES"))
                        .Add("@IMCOCLASS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("IMCOCLASS"))
                        .Add("@FOOTVALUETYPE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FOOTVALUETYPE"))
                        .Add("@BACKVALUETYPE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BACKVALUETYPE"))
                        .Add("@TOPDISVALUETYPE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TOPDISVALUETYPE"))
                        .Add("@AIRINLETVALUE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("AIRINLETVALUE"))
                        .Add("@BAFFLES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BAFFLES"))
                        .Add("@TYPEOFPREVACVALUE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TYPEOFPREVACVALUE"))
                        .Add("@BURSTDISCFITTED", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BURSTDISCFITTED"))
                        .Add("@TYPEOFTHERM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TYPEOFTHERM"))
                        .Add("@TYPEOFMANLID_CENTER", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TYPEOFMANLID_CENTER"))
                        .Add("@TYPEOFMANLID_FRONT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TYPEOFMANLID_FRONT"))
                        .Add("@TYPEOFMLSEAL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TYPEOFMLSEAL"))
                        .Add("@WORKINGPRESSURE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("WORKINGPRESSURE"))
                        .Add("@TESTPRESSURE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TESTPRESSURE"))
                        .Add("@REMARK1", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK1"))
                        .Add("@REMARK2", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK2"))
                        .Add("@FAULTS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FAULTS"))
                        .Add("@BASERAGEYY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BASERAGEYY"))
                        .Add("@BASERAGEMM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BASERAGEMM"))
                        .Add("@BASERAGE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BASERAGE"))
                        .Add("@BASELEASE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BASELEASE"))
                        .Add("@MARUKANSEAL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MARUKANSEAL"))
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0006_TANK"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

            Dim befAppDir As String = Nothing
            Dim officialDir As String = Nothing
            '承認前ディレクトリ
            befAppDir = COA0019Session.BEFOREAPPROVALDir & "\TANK\" & Trim(Convert.ToString(dtRow.Item("TANKNO")))
            '正式ディレクトリ
            officialDir = COA0019Session.UPLOADFILESDir & "\TANK\" & Trim(Convert.ToString(dtRow.Item("TANKNO")))

            'フォルダが存在する場合承認前から正式フォルダに移動
            If System.IO.Directory.Exists(befAppDir) Then

                'ディレクトリが存在しない場合、作成する
                If System.IO.Directory.Exists(officialDir) = False Then
                    System.IO.Directory.CreateDirectory(officialDir)
                Else
                    '格納フォルダクリア処理
                    For Each tempFile As String In System.IO.Directory.GetFiles(officialDir, "*", System.IO.SearchOption.AllDirectories)
                        'サブフォルダは対象外
                        Try
                            System.IO.File.Delete(tempFile)
                        Catch ex As Exception
                        End Try
                    Next
                End If

                '承認前フォルダのファイルをPDF正式格納フォルダへコピー
                For Each tempFile As String In System.IO.Directory.GetFiles(befAppDir, "*", System.IO.SearchOption.AllDirectories)
                    'ディレクトリ付ファイル名より、ファイル名編集
                    Dim fileName As String = tempFile
                    Do
                        If InStr(fileName, "\") > 0 Then
                            fileName = Mid(fileName, InStr(fileName, "\") + 1, 1024)
                        End If

                    Loop Until InStr(fileName, "\") <= 0

                    'Update_Hフォルダ内PDF→PDF正式格納フォルダへ上書コピー
                    System.IO.File.Copy(tempFile, officialDir & "\" & fileName, True)

                Next

                '集配信用フォルダ格納処理
                Dim COA00034SendDirectory As New COA00034SendDirectory
                Dim pgmDir As String = "\TANK\" & Trim(Convert.ToString(dtRow.Item("TANKNO")))
                COA00034SendDirectory.SendDirectoryCopy(pgmDir, officialDir, "2")

            End If

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim befAppDir As String = Nothing
            Dim sendDir As String = Nothing
            Dim uplDir As String = ""
            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' タンクマスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0022_TANKAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,UPDTERMID     = @UPDTERMID ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE APPLYID       = @APPLYID")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("APPLYID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

            Dim repStr As String = COA0019Session.SYSTEMROOTDir
            uplDir = COA0019Session.BEFOREAPPROVALDir.Replace(repStr, "")

            '承認前ディレクトリ
            befAppDir = COA0019Session.BEFOREAPPROVALDir & "\TANK\" & Trim(Convert.ToString(dtRow.Item("TANKNO")))
            '集配信用フォルダ
            sendDir = COA0019Session.SENDDir & "\SENDSTOR\" & Convert.ToString(HttpContext.Current.Session("APSRVname"))
            sendDir = sendDir & uplDir
            sendDir = sendDir & "\TANK\" & Trim(Convert.ToString(dtRow.Item("TANKNO")))

            'フォルダが存在する場合、ファイル削除
            If System.IO.Directory.Exists(befAppDir) Then
                'PDF格納フォルダクリア処理
                For Each tempFile As String In System.IO.Directory.GetFiles(befAppDir, "*", System.IO.SearchOption.AllDirectories)
                    'サブフォルダは対象外
                    Try
                        System.IO.File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next
            End If

            'フォルダが存在する場合、ファイル削除
            If System.IO.Directory.Exists(sendDir) Then
                '配信用フォルダクリア処理
                For Each tempFile As String In System.IO.Directory.GetFiles(sendDir, "*", System.IO.SearchOption.AllDirectories)
                    'サブフォルダは対象外
                    Try
                        System.IO.File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next
            End If

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            'li.Add(Convert.ToString("Default"))
            li.Add(Convert.ToString(dtRow.Item("APPLYID")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li

        End Function
    End Class

    ''' <summary>
    ''' 顧客マスタ関連処理
    ''' </summary>
    Private Class GBM00004
        Inherits ApprovalMasterClass '基底クラスを継承
        Private Const CONST_MAPID As String = "GBM00004"   '自身のMAPID
        Private Const CONST_EVENTCODE As String = "MasterApplyCustomer"

        ''' <summary>
        ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CreateDataTable() As DataTable
            Dim dt As New DataTable

            '共通項目
            dt.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
            dt.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
            dt.Columns.Add("TIMSTP", GetType(String))               'DBの固定フィールド
            dt.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
            dt.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド
            '画面固有項目
            dt.Columns.Add("APPLYID", GetType(String))              '申請ID
            dt.Columns.Add("COMPCODE", GetType(String))                 '会社コード
            dt.Columns.Add("COUNTRYCODE", GetType(String))                 '国コード
            dt.Columns.Add("CUSTOMERCODE", GetType(String))                 '顧客コード
            dt.Columns.Add("STYMD", GetType(String))                 '開始年月日
            dt.Columns.Add("ENDYMD", GetType(String))                 '終了年月日
            dt.Columns.Add("NAMES", GetType(String))                 '名称（短）
            dt.Columns.Add("NAMEL", GetType(String))                 '名称（長）
            dt.Columns.Add("NAMESEN", GetType(String))                 '名称（短）英語
            dt.Columns.Add("NAMELEN", GetType(String))                 '名称（長）英語
            dt.Columns.Add("CUSTOMERTYPE", GetType(String))                 '顧客タイプ
            dt.Columns.Add("POSTNUM", GetType(String))                 '郵便番号
            dt.Columns.Add("ADDR", GetType(String))                 '荷主住所
            dt.Columns.Add("ADDRJP", GetType(String))                 '荷主住所JP
            dt.Columns.Add("CITY", GetType(String))                 '荷主都市
            dt.Columns.Add("CITYJP", GetType(String))                 '荷主都市JP
            dt.Columns.Add("ADDRBL", GetType(String))                 '荷主住所BL
            dt.Columns.Add("BLNAME", GetType(String))                 '荷主BL名称
            dt.Columns.Add("CONSIGNEEIEC", GetType(String))                 '輸出入者管理コード
            dt.Columns.Add("TEL", GetType(String))                 '電話番号
            dt.Columns.Add("FAX", GetType(String))                 'ＦＡＸ番号
            dt.Columns.Add("CONTACTORG", GetType(String))                 '担当部署
            dt.Columns.Add("CONTACTPERSON", GetType(String))                 '担当者
            dt.Columns.Add("CONTACTMAIL", GetType(String))                 '担当メールアドレス
            dt.Columns.Add("MORG", GetType(String))                 '管理部署
            dt.Columns.Add("ACCAMPCODE", GetType(String))                 '会計・会社コード
            dt.Columns.Add("ACTORICODE", GetType(String))                 '会計・取引先コード
            dt.Columns.Add("ACTORICODES", GetType(String))                '会計・取引先支店コード
            dt.Columns.Add("ACCCURRENCYSEGMENT", GetType(String))         '円貨外貨区分
            dt.Columns.Add("BOTHCLASS ", GetType(String))                 '両建区分
            dt.Columns.Add("TORICODE ", GetType(String))                 　'取引先コード
            dt.Columns.Add("REMARK", GetType(String))                 '備考
            dt.Columns.Add("DELFLG", GetType(String))                 '削除フラグ
            dt.Columns.Add("APPROVALOBJECT", GetType(String))       '承認対象
            dt.Columns.Add("APPROVALORREJECT", GetType(String))     '承認or否認
            dt.Columns.Add("CHECK", GetType(String))                'チェック
            dt.Columns.Add("STEP", GetType(String))                 'ステップ
            dt.Columns.Add("STATUS", GetType(String))               'ステータス
            dt.Columns.Add("CURSTEP", GetType(String))              '承認ステップ
            dt.Columns.Add("STEPSTATE", GetType(String))            'ステップ状況
            dt.Columns.Add("APPROVALTYPE", GetType(String))         '承認区分
            dt.Columns.Add("APPROVERID", GetType(String))           '承認者
            dt.Columns.Add("LASTSTEP", GetType(String))             'ラストステップ

            Return dt
        End Function
        ''' <summary>
        ''' データ取得メソッド
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetData(stYMD As String, endYMD As String) As DataTable
            Dim dt As New DataTable

            Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = "Default"
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            Dim sqlStat As New StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
            sqlStat.AppendLine("      ,TBL.* ")
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       '' AS OPERATION")
            sqlStat.AppendLine("      ,TIMSTP = cast(CA.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
            sqlStat.AppendLine("      ,'0' AS HIDDEN ")
            sqlStat.AppendLine("      ,CA.COMPCODE")
            sqlStat.AppendLine("      ,CA.COUNTRYCODE")
            sqlStat.AppendLine("      ,CA.CUSTOMERCODE")
            sqlStat.AppendLine("      ,convert(nvarchar, CA.STYMD , 111) as STYMD")
            sqlStat.AppendLine("      ,convert(nvarchar, CA.ENDYMD , 111) as ENDYMD")
            sqlStat.AppendLine("      ,CA.NAMES")
            sqlStat.AppendLine("      ,CA.NAMEL")
            sqlStat.AppendLine("      ,CA.NAMESEN")
            sqlStat.AppendLine("      ,CA.NAMELEN")
            sqlStat.AppendLine("      ,CA.CUSTOMERTYPE")
            sqlStat.AppendLine("      ,CA.POSTNUM")
            sqlStat.AppendLine("      ,CA.ADDR")
            sqlStat.AppendLine("      ,CA.ADDRJP")
            sqlStat.AppendLine("      ,CA.CITY")
            sqlStat.AppendLine("      ,CA.CITYJP")
            sqlStat.AppendLine("      ,CA.ADDRBL")
            sqlStat.AppendLine("      ,CA.BLNAME")
            sqlStat.AppendLine("      ,CA.CONSIGNEEIEC")
            sqlStat.AppendLine("      ,CA.TEL")
            sqlStat.AppendLine("      ,CA.FAX")
            sqlStat.AppendLine("      ,CA.CONTACTORG")
            sqlStat.AppendLine("      ,CA.CONTACTPERSON")
            sqlStat.AppendLine("      ,CA.CONTACTMAIL")
            sqlStat.AppendLine("      ,CA.MORG")
            sqlStat.AppendLine("      ,CA.ACCAMPCODE")
            sqlStat.AppendLine("      ,CA.ACTORICODE")
            sqlStat.AppendLine("      ,CA.ACTORICODES")
            sqlStat.AppendLine("      ,CA.ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("      ,CA.BOTHCLASS")
            sqlStat.AppendLine("      ,CA.TORICOMP")
            sqlStat.AppendLine("      ,CA.INCTORICODE")
            sqlStat.AppendLine("      ,CA.EXPTORICODE")
            sqlStat.AppendLine("      ,CA.DEPOSITDAY")
            sqlStat.AppendLine("      ,CA.DEPOSITADDMM")
            sqlStat.AppendLine("      ,CA.OVERDRAWDAY")
            sqlStat.AppendLine("      ,CA.OVERDRAWADDMM")
            sqlStat.AppendLine("      ,CA.HOLIDAYFLG")
            sqlStat.AppendLine("      ,CA.REMARK")
            sqlStat.AppendLine("      ,CA.DELFLG")
            sqlStat.AppendLine("      ,CASE WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
            sqlStat.AppendLine("            WHEN (AH4.STEP = AH3.LASTSTEP AND AH5.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
            sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH4.STEP,'00'))))) + '/' + trim(convert(char,convert(int,AH3.LASTSTEP))) END as STEPSTATE")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV1.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV1.VALUE2,'') END AS APPROVALOBJECT ")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
            sqlStat.AppendLine("      ,'' AS ""CHECK""")
            sqlStat.AppendLine("      ,AH.APPLYID")
            sqlStat.AppendLine("      ,AH.STEP")
            sqlStat.AppendLine("      ,AH.STATUS")
            sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
            sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
            sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE")
            sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
            sqlStat.AppendLine("      ,AH3.LASTSTEP AS LASTSTEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
            sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
            sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
            sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
            sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
            sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
            sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0023_CUSTOMERAPPLY CA") '顧客マスタ(申請)
            sqlStat.AppendLine("    ON  CA.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  CA.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  CA.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
            sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
            sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
            sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")

            sqlStat.AppendLine("  LEFT JOIN ( ") 'LastStep取得
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS LASTSTEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH3 ")
            sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN ( ")
            sqlStat.AppendLine("  SELECT APPLYID,MAX(STEP) AS STEP ")
            sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
            sqlStat.AppendLine("  WHERE COMPCODE  = @COMPCODE ")
            sqlStat.AppendLine("    AND STATUS    > '" & C_APP_STATUS.REVISE & "' ")
            sqlStat.AppendLine("    AND DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  GROUP BY APPLYID ) AS AH4 ")
            sqlStat.AppendLine("    ON  AH4.APPLYID      = AH.APPLYID")

            sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH5 ")
            sqlStat.AppendLine("    ON AH5.APPLYID = AH4.APPLYID ")
            sqlStat.AppendLine("   AND AH5.STEP    = AH4.STEP ")
            sqlStat.AppendLine("   AND AH5.DELFLG <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
            sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

            '申請開始日
            If (String.IsNullOrEmpty(stYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE >= '{0} '", stYMD).AppendLine()
            End If
            '申請終了日
            If (String.IsNullOrEmpty(endYMD) = False) Then
                sqlStat.AppendFormat(" AND AH.APPLYDATE <= '{0} '", endYMD & " 23:59:59:999").AppendLine()
            End If

            sqlStat.AppendLine("   ) TBL")
            sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン

                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENTCODE
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            Return dt
        End Function

        ''' <summary>
        ''' 本マスタ登録処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub MstDbUpdate(dtRow As DataRow)

            Dim nowDate As DateTime = Date.Now
            Dim sqlStat As New Text.StringBuilder
            Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out

            '申請テーブル更新処理
            ApplyMstDbUpdate(dtRow)

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 顧客マスタ更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine(" DECLARE @timestamp as bigint ; ")
                sqlStat.AppendLine(" set @timestamp = 0 ; ")
                sqlStat.AppendLine(" DECLARE timestamp CURSOR FOR ")
                sqlStat.AppendLine(" SELECT CAST(UPDTIMSTP as bigint) as timestamp ")
                sqlStat.AppendLine(" FROM GBM0004_CUSTOMER ")
                sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE ")
                sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE ")
                sqlStat.AppendLine("   AND CUSTOMERCODE = @CUSTOMERCODE ")
                sqlStat.AppendLine("   AND STYMD = @STYMD ")
                sqlStat.AppendLine(" OPEN timestamp ; ")
                sqlStat.AppendLine(" FETCH NEXT FROM timestamp INTO @timestamp ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS = 0 ) ")
                sqlStat.AppendLine("  UPDATE GBM0004_CUSTOMER ")
                sqlStat.AppendLine("  SET ENDYMD = @ENDYMD , ")
                sqlStat.AppendLine("      NAMES = @NAMES , ")
                sqlStat.AppendLine("      NAMEL = @NAMEL , ")
                sqlStat.AppendLine("      NAMESEN = @NAMESEN , ")
                sqlStat.AppendLine("      NAMELEN = @NAMELEN , ")
                sqlStat.AppendLine("      CUSTOMERTYPE = @CUSTOMERTYPE , ")
                sqlStat.AppendLine("      POSTNUM = @POSTNUM , ")
                sqlStat.AppendLine("      ADDR = @ADDR , ")
                sqlStat.AppendLine("      ADDRJP = @ADDRJP , ")
                sqlStat.AppendLine("      CITY   = @CITY , ")
                sqlStat.AppendLine("      CITYJP = @CITYJP , ")
                sqlStat.AppendLine("      ADDRBL = @ADDRBL , ")
                sqlStat.AppendLine("      BLNAME = @BLNAME , ")
                sqlStat.AppendLine("      CONSIGNEEIEC = @CONSIGNEEIEC , ")
                sqlStat.AppendLine("      TEL = @TEL , ")
                sqlStat.AppendLine("      FAX = @FAX , ")
                sqlStat.AppendLine("      CONTACTORG = @CONTACTORG , ")
                sqlStat.AppendLine("      CONTACTPERSON = @CONTACTPERSON , ")
                sqlStat.AppendLine("      CONTACTMAIL = @CONTACTMAIL , ")
                sqlStat.AppendLine("      MORG = @MORG , ")
                sqlStat.AppendLine("      ACCAMPCODE = @ACCAMPCODE , ")
                sqlStat.AppendLine("      ACTORICODE = @ACTORICODE , ")
                sqlStat.AppendLine("      ACTORICODES = @ACTORICODES , ")
                sqlStat.AppendLine("      ACCCURRENCYSEGMENT = @ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      BOTHCLASS = @BOTHCLASS , ")
                sqlStat.AppendLine("      TORICOMP = @TORICOMP , ")
                sqlStat.AppendLine("      INCTORICODE = @INCTORICODE , ")
                sqlStat.AppendLine("      EXPTORICODE = @EXPTORICODE , ")
                sqlStat.AppendLine("      DEPOSITDAY = @DEPOSITDAY , ")
                sqlStat.AppendLine("      DEPOSITADDMM = @DEPOSITADDMM , ")
                sqlStat.AppendLine("      OVERDRAWDAY = @OVERDRAWDAY , ")
                sqlStat.AppendLine("      OVERDRAWADDMM = @OVERDRAWADDMM , ")
                sqlStat.AppendLine("      HOLIDAYFLG = @HOLIDAYFLG , ")
                sqlStat.AppendLine("      REMARK = @REMARK , ")
                sqlStat.AppendLine("      DELFLG = @DELFLG , ")
                sqlStat.AppendLine("      UPDYMD             = @UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER            = @UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID          = @UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD         = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE COMPCODE              = @COMPCODE ")
                sqlStat.AppendLine("   AND COUNTRYCODE       = @COUNTRYCODE ")
                sqlStat.AppendLine("   AND CUSTOMERCODE       = @CUSTOMERCODE ")
                sqlStat.AppendLine("   AND STYMD       = @STYMD ")
                sqlStat.AppendLine("   ; ")
                sqlStat.AppendLine(" IF ( @@FETCH_STATUS <> 0 ) ")
                sqlStat.AppendLine(" INSERT INTO GBM0004_CUSTOMER ( ")
                sqlStat.AppendLine("      COMPCODE , ")
                sqlStat.AppendLine("      COUNTRYCODE , ")
                sqlStat.AppendLine("      CUSTOMERCODE , ")
                sqlStat.AppendLine("      STYMD , ")
                sqlStat.AppendLine("      ENDYMD , ")
                sqlStat.AppendLine("      NAMES , ")
                sqlStat.AppendLine("      NAMEL , ")
                sqlStat.AppendLine("      NAMESEN , ")
                sqlStat.AppendLine("      NAMELEN , ")
                sqlStat.AppendLine("      CUSTOMERTYPE , ")
                sqlStat.AppendLine("      POSTNUM , ")
                sqlStat.AppendLine("      ADDR , ")
                sqlStat.AppendLine("      ADDRJP , ")
                sqlStat.AppendLine("      CITY , ")
                sqlStat.AppendLine("      CITYJP , ")
                sqlStat.AppendLine("      ADDRBL , ")
                sqlStat.AppendLine("      BLNAME , ")
                sqlStat.AppendLine("      CONSIGNEEIEC , ")
                sqlStat.AppendLine("      TEL , ")
                sqlStat.AppendLine("      FAX , ")
                sqlStat.AppendLine("      CONTACTORG , ")
                sqlStat.AppendLine("      CONTACTPERSON , ")
                sqlStat.AppendLine("      CONTACTMAIL , ")
                sqlStat.AppendLine("      MORG , ")
                sqlStat.AppendLine("      ACCAMPCODE , ")
                sqlStat.AppendLine("      ACTORICODE , ")
                sqlStat.AppendLine("      ACTORICODES , ")
                sqlStat.AppendLine("      ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      BOTHCLASS , ")
                sqlStat.AppendLine("      TORICOMP , ")
                sqlStat.AppendLine("      INCTORICODE , ")
                sqlStat.AppendLine("      EXPTORICODE , ")
                sqlStat.AppendLine("      DEPOSITDAY , ")
                sqlStat.AppendLine("      DEPOSITADDMM , ")
                sqlStat.AppendLine("      OVERDRAWDAY , ")
                sqlStat.AppendLine("      OVERDRAWADDMM , ")
                sqlStat.AppendLine("      HOLIDAYFLG , ")
                sqlStat.AppendLine("      REMARK , ")
                sqlStat.AppendLine("      DELFLG , ")
                sqlStat.AppendLine("      INITYMD , ")
                sqlStat.AppendLine("      UPDYMD , ")
                sqlStat.AppendLine("      UPDUSER , ")
                sqlStat.AppendLine("      UPDTERMID , ")
                sqlStat.AppendLine("      RECEIVEYMD ) ")
                sqlStat.AppendLine(" VALUES ( ")
                sqlStat.AppendLine("      @COMPCODE , ")
                sqlStat.AppendLine("      @COUNTRYCODE , ")
                sqlStat.AppendLine("      @CUSTOMERCODE , ")
                sqlStat.AppendLine("      @STYMD , ")
                sqlStat.AppendLine("      @ENDYMD , ")
                sqlStat.AppendLine("      @NAMES , ")
                sqlStat.AppendLine("      @NAMEL , ")
                sqlStat.AppendLine("      @NAMESEN , ")
                sqlStat.AppendLine("      @NAMELEN , ")
                sqlStat.AppendLine("      @CUSTOMERTYPE , ")
                sqlStat.AppendLine("      @POSTNUM , ")
                sqlStat.AppendLine("      @ADDR , ")
                sqlStat.AppendLine("      @ADDRJP , ")
                sqlStat.AppendLine("      @CITY , ")
                sqlStat.AppendLine("      @CITYJP , ")
                sqlStat.AppendLine("      @ADDRBL , ")
                sqlStat.AppendLine("      @BLNAME , ")
                sqlStat.AppendLine("      @CONSIGNEEIEC , ")
                sqlStat.AppendLine("      @TEL , ")
                sqlStat.AppendLine("      @FAX , ")
                sqlStat.AppendLine("      @CONTACTORG , ")
                sqlStat.AppendLine("      @CONTACTPERSON , ")
                sqlStat.AppendLine("      @CONTACTMAIL , ")
                sqlStat.AppendLine("      @MORG , ")
                sqlStat.AppendLine("      @ACCAMPCODE , ")
                sqlStat.AppendLine("      @ACTORICODE , ")
                sqlStat.AppendLine("      @ACTORICODES , ")
                sqlStat.AppendLine("      @ACCCURRENCYSEGMENT , ")
                sqlStat.AppendLine("      @BOTHCLASS , ")
                sqlStat.AppendLine("      @TORICOMP , ")
                sqlStat.AppendLine("      @INCTORICODE , ")
                sqlStat.AppendLine("      @EXPTORICODE , ")
                sqlStat.AppendLine("      @DEPOSITDAY , ")
                sqlStat.AppendLine("      @DEPOSITADDMM , ")
                sqlStat.AppendLine("      @OVERDRAWDAY , ")
                sqlStat.AppendLine("      @OVERDRAWADDMM , ")
                sqlStat.AppendLine("      @HOLIDAYFLG , ")
                sqlStat.AppendLine("      @REMARK , ")
                sqlStat.AppendLine("      @DELFLG , ")
                sqlStat.AppendLine(" @INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); ")
                sqlStat.AppendLine(" CLOSE timestamp ; ")
                sqlStat.AppendLine(" DEALLOCATE timestamp ; ")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    With sqlCmd.Parameters
                        .Add("@COMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COMPCODE"))
                        .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("COUNTRYCODE"))
                        .Add("@CUSTOMERCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CUSTOMERCODE"))
                        .Add("@STYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("STYMD"))
                        .Add("@ENDYMD", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ENDYMD"))
                        .Add("@NAMES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMES"))
                        .Add("@NAMEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMEL"))
                        .Add("@NAMESEN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMESEN"))
                        .Add("@NAMELEN", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("NAMELEN"))
                        .Add("@CUSTOMERTYPE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CUSTOMERTYPE"))
                        .Add("@POSTNUM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("POSTNUM"))
                        .Add("@ADDR", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ADDR"))
                        .Add("@ADDRJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ADDRJP"))
                        .Add("@CITY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CITY"))
                        .Add("@CITYJP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CITYJP"))
                        .Add("@ADDRBL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ADDRBL"))
                        .Add("@BLNAME", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BLNAME"))
                        .Add("@CONSIGNEEIEC", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONSIGNEEIEC"))
                        .Add("@TEL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TEL"))
                        .Add("@FAX", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("FAX"))
                        .Add("@CONTACTORG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONTACTORG"))
                        .Add("@CONTACTPERSON", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONTACTPERSON"))
                        .Add("@CONTACTMAIL", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("CONTACTMAIL"))
                        .Add("@MORG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("MORG"))
                        .Add("@ACCAMPCODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACCAMPCODE"))
                        .Add("@ACTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACTORICODE"))
                        .Add("@ACTORICODES", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACTORICODES"))
                        .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("ACCCURRENCYSEGMENT"))
                        .Add("@BOTHCLASS", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("BOTHCLASS"))
                        .Add("@TORICOMP", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("TORICOMP"))
                        .Add("@INCTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("INCTORICODE"))
                        .Add("@EXPTORICODE", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("EXPTORICODE"))
                        .Add("@DEPOSITDAY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEPOSITDAY"))
                        .Add("@DEPOSITADDMM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DEPOSITADDMM"))
                        .Add("@OVERDRAWDAY", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OVERDRAWDAY"))
                        .Add("@OVERDRAWADDMM", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("OVERDRAWADDMM"))
                        .Add("@HOLIDAYFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("HOLIDAYFLG"))
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("REMARK"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = Convert.ToString(dtRow("DELFLG"))
                        .Add("@INITYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using

                '更新ジャーナル追加
                COA0030Journal.TABLENM = "GBM0004_CUSTOMER"
                COA0030Journal.ACTION = "UPDATE_INSERT"
                COA0030Journal.ROW = dtRow
                COA0030Journal.COA0030SaveJournal()

            End Using

        End Sub
        ''' <summary>
        ''' 申請テーブル更新処理
        ''' </summary>
        ''' <param name="dtRow"></param>
        Public Overrides Sub ApplyMstDbUpdate(dtRow As DataRow)

            Dim sqlStat As New Text.StringBuilder
            Dim nowDate As DateTime = Date.Now
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                '******************************
                ' 顧客マスタ(申請)更新
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBM0023_CUSTOMERAPPLY")
                sqlStat.AppendLine("   SET DELFLG        = '" & CONST_FLAG_YES & "' ")
                sqlStat.AppendLine("      ,UPDYMD        = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER       = @UPDUSER ")
                sqlStat.AppendLine("      ,UPDTERMID     = @UPDTERMID ")
                sqlStat.AppendLine("      ,RECEIVEYMD    = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE APPLYID       = @APPLYID")
                sqlStat.AppendLine("   AND STYMD         = @STYMD")
                sqlStat.AppendLine("   AND DELFLG       <> '" & CONST_FLAG_YES & "'")

                'DB接続
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                    With sqlCmd.Parameters
                        .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(dtRow.Item("APPLYID"))
                        .Add("@STYMD", SqlDbType.Date).Value = Convert.ToString(dtRow.Item("STYMD"))
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = nowDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = COA0019Session.APSRVname
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                End Using
            End Using

        End Sub
        ''' <summary>
        ''' 引き渡し情報取得
        ''' </summary>
        ''' <param name="dtRow"></param>
        ''' <returns></returns>
        Public Overrides Function GetDeliveryInfo(dtRow As DataRow) As List(Of String)
            Dim li As New List(Of String)

            'li.Add(Convert.ToString("Default"))
            li.Add(Convert.ToString(dtRow.Item("APPLYID")))
            li.Add(Convert.ToString(dtRow.Item("STYMD")))
            li.Add(Convert.ToString(dtRow.Item("ENDYMD")))

            Return li

        End Function
    End Class

End Class

