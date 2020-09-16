Imports System.Data.SqlClient

Imports BASEDLL
Public Structure GBA00001UnNo

    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' ListBox(キー、名称)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX As ListBox

    ''' <summary>
    ''' 名称保持用ディクショナリ
    ''' </summary>
    ''' <returns></returns>
    Public Property UnNoKeyValue As Dictionary(Of String, String)

    Const TBL_UNNO As String = "GBM0007_UNNO"

    ''' <summary>
    ''' <para>国連番号リスト取得</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00001getLeftListUnNo()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = ""

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAME"
            Else
                NameCol = "NAME_EN"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(UNNO) as UNNO, rtrim(HAZARDCLASS) as HAZARDCLASS, rtrim(PACKINGGROUP) as PACKINGGROUP, {0} as NAME ", NameCol)
            sqlStat.AppendFormat(" FROM {0} ", TBL_UNNO).AppendLine()
            sqlStat.AppendLine("   WHERE STYMD   <= @P1 ")
            sqlStat.AppendLine("   AND   ENDYMD  >= @P1 ")
            sqlStat.AppendLine("   AND   DELFLG  = @P2 ")
            sqlStat.AppendLine("   ORDER BY UNNO, HAZARDCLASS, PACKINGGROUP ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    UnNoKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0},{1},{2}", sqlDr("UNNO"), sqlDr("HAZARDCLASS"), sqlDr("PACKINGGROUP")))
                        'listitem.Attributes.Add("data_names", sqlDr("NAME"))
                        LISTBOX.Items.Add(listitem)
                        UnNoKeyValue.Add(listitem.Text, Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

End Structure

''' <summary>
''' マスタ申請ID管理
''' </summary>
Public Structure GBA00002MasterApplyID

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <returns></returns>
    Public Property COMPCODE As String

    ''' <summary>
    ''' システムコード
    ''' </summary>
    ''' <returns></returns>
    Public Property SYSCODE As String

    ''' <summary>
    ''' マスタキー
    ''' </summary>
    ''' <returns></returns>
    Public Property KEYCODE As String

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <returns></returns>
    Public Property MAPID As String

    ''' <summary>
    ''' イベントコード
    ''' </summary>
    ''' <returns></returns>
    Public Property EVENTCODE As String

    ''' <summary>
    ''' サブコード
    ''' </summary>
    ''' <returns></returns>
    Public Property SUBCODE As String

    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' 申請ID
    ''' </summary>
    ''' <returns></returns>
    Public Property APPLYID As String

    Const TBL_FIXVALUE As String = "COS0017_FIXVALUE"
    Const TBL_APPROVAL As String = "COS0022_APPROVAL"
    Const SEQ_MASTER As String = "GBQ0002_MASTER"
    Const SUB_COMMON As String = "Common"

    ''' <summary>
    ''' <para>申請ID取得</para>
    ''' <para>入力プロパティ(COMPCODE,SYSCODE,KEYCODE,MAPID,EVENTCODE,SUBCODE,USERID)</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' <para>              (APPLYID(申請ID):値ありはID、値なしは申請不要)</para>
    ''' </summary>
    Public Sub COA0032getgApplyID()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = ""
        Dim errMessage As String

        Try

            If IsNothing(COMPCODE) And COMPCODE = "" Then
                ERR = C_MESSAGENO.DLLIFERROR

                COA0000DllMessage.MessageCode = ERR
                COA0000DllMessage.COA0000GetMesssage()
                If (COA0019Session.LANGLOG <> C_LANG.JA) Then
                    errMessage = COA0000DllMessage.MessageStrEN
                Else
                    errMessage = COA0000DllMessage.MessageStrJA
                End If

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = errMessage & "(COMPCODE)"
                COA0003LogFile.MESSAGENO = ERR
                COA0003LogFile.COA0003WriteLog()
                Return
            End If

            If IsNothing(SYSCODE) And SYSCODE = "" Then
                ERR = C_MESSAGENO.DLLIFERROR

                COA0000DllMessage.MessageCode = ERR
                COA0000DllMessage.COA0000GetMesssage()
                If (COA0019Session.LANGLOG <> C_LANG.JA) Then
                    errMessage = COA0000DllMessage.MessageStrEN
                Else
                    errMessage = COA0000DllMessage.MessageStrJA
                End If

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = errMessage & "(SYSCODE)"
                COA0003LogFile.MESSAGENO = ERR
                COA0003LogFile.COA0003WriteLog()
                Return
            End If

            If IsNothing(KEYCODE) And KEYCODE = "" Then
                ERR = C_MESSAGENO.DLLIFERROR

                COA0000DllMessage.MessageCode = ERR
                COA0000DllMessage.COA0000GetMesssage()
                If (COA0019Session.LANGLOG <> C_LANG.JA) Then
                    errMessage = COA0000DllMessage.MessageStrEN
                Else
                    errMessage = COA0000DllMessage.MessageStrJA
                End If

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = errMessage & "(KEYCODE)"
                COA0003LogFile.MESSAGENO = ERR
                COA0003LogFile.COA0003WriteLog()
                Return
            End If

            ERR = C_MESSAGENO.NODATA

            For Each target As String In {Me.SUBCODE, SUB_COMMON}

                'SQL文の作成
                Dim sqlStat As New System.Text.StringBuilder
                sqlStat.AppendFormat("   SELECT APPROVALTYPE FROM {0} ", TBL_APPROVAL)
                sqlStat.AppendLine("     WHERE COMPCODE  = @P1 ")
                sqlStat.AppendLine("     AND   MAPID     = @P2 ")
                sqlStat.AppendLine("     AND   EVENTCODE = @P3 ")
                sqlStat.AppendLine("     AND   SUBCODE   = @P4 ")
                sqlStat.AppendLine("     AND   STEP      = '" & C_APP_FIRSTSTEP & "' ")
                'sqlStat.AppendLine("     AND   APPROVALTYPE  = '3' ")
                sqlStat.AppendLine("     AND   STYMD    <= @P5 ")
                sqlStat.AppendLine("     AND   ENDYMD   >= @P5 ")
                sqlStat.AppendLine("     AND   DELFLG    = @P6 ")

                Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon))
                    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)

                        sqlConn.Open()
                        Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                        Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                        Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                        Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                        Dim PARA5 As SqlParameter = sqlCmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                        Dim PARA6 As SqlParameter = sqlCmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar)
                        PARA1.Value = Me.COMPCODE
                        PARA2.Value = Me.MAPID
                        PARA3.Value = Me.EVENTCODE
                        PARA4.Value = target
                        PARA5.Value = Date.Now
                        PARA6.Value = CONST_FLAG_NO

                        Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                            While sqlDr.Read
                                ERR = C_MESSAGENO.NORMAL
                                If Convert.ToString(sqlDr("APPROVALTYPE")) = "3" Then
                                    Me.APPLYID = ""
                                    retValue = "No Data"
                                    Exit While
                                End If
                            End While
                        End Using
                    End Using

                    If ERR = C_MESSAGENO.NORMAL Then
                        If retValue = "" Then
                            'SQL文の作成
                            Dim sqlStat1 As New System.Text.StringBuilder
                            sqlStat1.AppendLine("   SELECT ")
                            sqlStat1.AppendLine("     'APM' ")
                            sqlStat1.AppendLine("       + LEFT(CONVERT(char,getdate(),12),4) ")
                            sqlStat1.AppendLine("       + '_' ")
                            sqlStat1.AppendFormat("     + RIGHT('000000' + TRIM(CONVERT(char,NEXT VALUE FOR {0})),5) ", SEQ_MASTER).AppendLine()
                            sqlStat1.AppendLine("       + '_' ")
                            sqlStat1.AppendLine("       + ( ")
                            sqlStat1.AppendFormat("        SELECT VALUE1 FROM {0} ", TBL_FIXVALUE).AppendLine()
                            sqlStat1.AppendLine("          WHERE COMPCODE = @P1 ")
                            sqlStat1.AppendLine("          AND   SYSCODE  = @P2 ")
                            sqlStat1.AppendLine("          AND   CLASS    = '" & C_SERVERSEQ & "' ")
                            sqlStat1.AppendLine("          AND   KEYCODE  = @P3 ")
                            sqlStat1.AppendLine("          AND   STYMD    <= @P4 ")
                            sqlStat1.AppendLine("          AND   ENDYMD   >= @P4 ")
                            sqlStat1.AppendLine("          AND   DELFLG   = @P5 ")
                            sqlStat1.AppendLine("         ) as APPLYID ")

                            Using sqlCmd As New SqlCommand(sqlStat1.ToString, sqlConn)

                                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
                                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 20)
                                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 20)
                                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                                Dim PARA5 As SqlParameter = sqlCmd.Parameters.Add("@P5", System.Data.SqlDbType.Char, 1)
                                PARA1.Value = GBC_COMPCODE_D
                                PARA2.Value = Me.SYSCODE
                                PARA3.Value = Me.KEYCODE
                                PARA4.Value = Date.Now
                                PARA5.Value = CONST_FLAG_NO
                                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                                    While sqlDr.Read
                                        retValue = Convert.ToString(sqlDr("APPLYID"))
                                        Exit While
                                    End While
                                End Using
                                ERR = C_MESSAGENO.NORMAL
                            End Using
                            Me.APPLYID = retValue
                        End If
                        Exit For
                    End If
                End Using
            Next

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

End Structure

''' <summary>
''' 
''' </summary>
Public Structure GBA00003UserSetting

    Public Shared Property COUNTRYCODE As String
        Get
            Return Convert.ToString(HttpContext.Current.Session("CountryCode"))
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("CountryCode") = Value
        End Set
    End Property
    Public Shared Property OFFICECODE As String
        Get
            Return Convert.ToString(HttpContext.Current.Session("OfficeCode"))
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("OfficeCode") = Value
        End Set
    End Property

    Public Shared Property OFFICENAME As String
        Get
            Return Convert.ToString(HttpContext.Current.Session("OfficeName"))
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("OfficeName") = Value
        End Set
    End Property

    Public Shared Property TAXRATE As String
        Get
            Return Convert.ToString(HttpContext.Current.Session("TaxRate"))
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("TaxRate") = Value
        End Set
    End Property

    Public Shared Property DATEFORMAT As String
        Get
            Return Convert.ToString(HttpContext.Current.Session("DateFormat"))
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("DateFormat") = Value
        End Set
    End Property

    Public Shared Property DATEYMFORMAT As String
        Get
            Return Convert.ToString(HttpContext.Current.Session("DateYMFormat"))
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("DateYMFormat") = Value
        End Set
    End Property

    Public Shared Property DECIMALPLACES As String
        Get
            Return Convert.ToString(HttpContext.Current.Session("DecimalPlaces"))
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("DecimalPlaces") = Value
        End Set
    End Property
    ''' <summary>
    ''' ログインユーザーのユーザマスタ上のORG
    ''' </summary>
    ''' <returns></returns>
    Public Shared Property USERORG As String
        Get
            Return Convert.ToString(HttpContext.Current.Session("UserOrg"))
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("UserOrg") = Value
        End Set
    End Property
    ''' <summary>
    ''' JOTユーザー判定(True=JOTユーザー,False=通常ユーザー)
    ''' </summary>
    ''' <returns></returns>
    Public Shared Property IS_JOTUSER As Boolean
        Get
            Return Convert.ToBoolean(HttpContext.Current.Session("IsJotUser"))
        End Get
        Set(value As Boolean)
            HttpContext.Current.Session("IsJotUser") = value
        End Set
    End Property
    ''' <summary>
    ''' JPOperationか判定
    ''' </summary>
    ''' <returns>True:JpOperationユーザー(課税フラグ表示),False:非JpOparationユーザー</returns>
    ''' <remarks>ブレーカー費用、オーダー一覧の課税フラグの表示、デフォルトチェックOn/Offで利用</remarks>
    Public Shared ReadOnly Property IS_JPOPERATOR As Boolean
        Get
            Return {"AgentJ", "JOTJ"}.Contains(COA0019Session.PROFID.Trim)
        End Get
    End Property
    ''' <summary>
    ''' Agent Topユーザー判定(True=Agent Topユーザー,False=通常ユーザー)
    ''' </summary>
    ''' <returns></returns>
    Public Shared Property IS_AGENTTOPUSER As Boolean
        Get
            Return Convert.ToBoolean(HttpContext.Current.Session("IsAgentTopUser"))
        End Get
        Set(value As Boolean)
            HttpContext.Current.Session("IsAgentTopUser") = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <returns></returns>
    Public Property USERID As String

    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    Const TBL_USER As String = "COS0005_USER"
    Const TBL_FIXVALUE As String = "COS0017_FIXVALUE"
    Const TBL_ORG As String = "COS0021_ORG"
    Const TBL_COUNTRY As String = "GBM0001_COUNTRY"
    Const TBL_TRADER As String = "GBM0005_TRADER"
    Const CLASS_JOTCOUNTRY As String = "JOTCOUNTRYORG"

    ''' <summary>
    ''' <para>GB ユーザ設定追加</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00003GetUserSetting()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = ""
        Dim errMessage As String

        Try
            ' デフォルト値
            TAXRATE = ""
            DATEFORMAT = "yyyy/MM/dd"
            DATEYMFORMAT = "yyyy/MM"
            DECIMALPLACES = ""

            If IsNothing(USERID) And USERID = "" Then
                ERR = C_MESSAGENO.DLLIFERROR

                COA0000DllMessage.MessageCode = ERR()
                COA0000DllMessage.COA0000GetMesssage()
                If (COA0019Session.LANGLOG <> C_LANG.JA) Then
                    errMessage = COA0000DllMessage.MessageStrEN
                Else
                    errMessage = COA0000DllMessage.MessageStrJA
                End If

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = errMessage & "(USERID)"
                COA0003LogFile.MESSAGENO = ERR()
                COA0003LogFile.COA0003WriteLog()
                Return
            End If

            'SQL文の作成

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendLine("   rtrim(C.TAXRATE) as TAXRATE, rtrim(C.DATEFORMAT) as DATEFORMAT, ")
            sqlStat.AppendLine("   rtrim(C.DECIMALPLACES) as DECIMALPLACES, rtrim(C.COUNTRYCODE) as COUNTRYCODE, rtrim(D.CARRIERCODE) as CARRIERCODE , rtrim(D.NAMES) as NAMES, ")
            sqlStat.AppendLine("   rtrim(A.ORG) as USERORG, ")
            sqlStat.AppendLine("   rtrim(A.PROFID) as PROFID, ")
            sqlStat.AppendLine("   '0' as ISJOTUSER ")
            sqlStat.AppendFormat(" FROM {0} A", TBL_USER).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} B", TBL_ORG).AppendLine()
            sqlStat.AppendLine("     ON  B.STYMD   <= @P1 ")
            sqlStat.AppendLine("     AND B.ENDYMD  >= @P1 ")
            sqlStat.AppendLine("     AND B.DELFLG  = @P2 ")
            sqlStat.AppendLine("     AND B.ORGCODE = A.ORG ")
            sqlStat.AppendLine("     AND B.SYSCODE = @P4 ")
            sqlStat.AppendLine("     AND B.COMPCODE = A.COMPCODE ")
            sqlStat.AppendFormat(" INNER JOIN {0} C", TBL_COUNTRY).AppendLine()
            sqlStat.AppendLine("     ON  C.STYMD   <= @P1 ")
            sqlStat.AppendLine("     AND C.ENDYMD  >= @P1 ")
            sqlStat.AppendLine("     AND C.DELFLG  = @P2 ")
            sqlStat.AppendLine("     AND C.ORGCODE = B.MORGCODE ")
            sqlStat.AppendLine("     AND C.COMPCODE = A.COMPCODE ")
            sqlStat.AppendFormat(" INNER JOIN {0} D", TBL_TRADER).AppendLine()
            sqlStat.AppendLine("     ON  D.STYMD   <= @P1 ")
            sqlStat.AppendLine("     AND D.ENDYMD  >= @P1 ")
            sqlStat.AppendLine("     AND D.DELFLG  = @P2 ")
            sqlStat.AppendLine("     AND D.COMPCODE = A.COMPCODE ")
            sqlStat.AppendLine("     AND D.COUNTRYCODE = C.COUNTRYCODE ")
            sqlStat.AppendLine("     AND D.MORG = A.ORG ")
            sqlStat.AppendLine("   WHERE A.STYMD   <= @P1 ")
            sqlStat.AppendLine("   AND   A.ENDYMD  >= @P1 ")
            sqlStat.AppendLine("   AND   A.DELFLG  = @P2 ")
            sqlStat.AppendLine("   AND   A.USERID  = @P3 ")
            sqlStat.AppendLine("   AND   B.MORGCODE <> @P5 ")
            sqlStat.AppendLine("   UNION ")
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendLine("   rtrim(D.TAXRATE) as TAXRATE, rtrim(D.DATEFORMAT) as DATEFORMAT, ")
            sqlStat.AppendLine("   rtrim(D.DECIMALPLACES) as DECIMALPLACES, rtrim(D.COUNTRYCODE) as COUNTRYCODE, rtrim(E.CARRIERCODE) as CARRIERCODE , rtrim(E.NAMES) as NAMES, ")
            sqlStat.AppendLine("   rtrim(A.ORG) as USERORG, ")
            sqlStat.AppendLine("   rtrim(A.PROFID) as PROFID, ")
            sqlStat.AppendLine("   '1' as ISJOTUSER ")
            sqlStat.AppendFormat(" FROM {0} A", TBL_USER).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} B", TBL_ORG).AppendLine()
            sqlStat.AppendLine("     ON  B.STYMD   <= @P1 ")
            sqlStat.AppendLine("     AND B.ENDYMD  >= @P1 ")
            sqlStat.AppendLine("     AND B.DELFLG  = @P2 ")
            sqlStat.AppendLine("     AND B.ORGCODE = A.ORG ")
            sqlStat.AppendLine("     AND B.SYSCODE = @P4 ")
            sqlStat.AppendLine("     AND B.COMPCODE = A.COMPCODE ")
            sqlStat.AppendFormat(" INNER JOIN {0} C", TBL_FIXVALUE).AppendLine()
            sqlStat.AppendLine("     ON  C.STYMD   <= @P1 ")
            sqlStat.AppendLine("     AND C.ENDYMD  >= @P1 ")
            sqlStat.AppendLine("     AND C.DELFLG  = @P2 ")
            sqlStat.AppendFormat("   AND C.CLASS = '{0}' ", CLASS_JOTCOUNTRY).AppendLine()
            sqlStat.AppendLine("     AND C.KEYCODE = A.ORG ")
            sqlStat.AppendLine("     AND C.COMPCODE = '" & GBC_COMPCODE_D & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} D", TBL_COUNTRY).AppendLine()
            sqlStat.AppendLine("     ON  D.STYMD   <= @P1 ")
            sqlStat.AppendLine("     AND D.ENDYMD  >= @P1 ")
            sqlStat.AppendLine("     AND D.DELFLG  = @P2 ")
            sqlStat.AppendLine("     AND D.ORGCODE = C.VALUE1 ")
            sqlStat.AppendLine("     AND D.COMPCODE = A.COMPCODE ")
            sqlStat.AppendFormat(" INNER JOIN {0} E", TBL_TRADER).AppendLine()
            sqlStat.AppendLine("     ON  E.STYMD   <= @P1 ")
            sqlStat.AppendLine("     AND E.ENDYMD  >= @P1 ")
            sqlStat.AppendLine("     AND E.DELFLG  = @P2 ")
            sqlStat.AppendLine("     AND E.COMPCODE = A.COMPCODE ")
            sqlStat.AppendLine("     AND E.COUNTRYCODE = D.COUNTRYCODE ")
            sqlStat.AppendLine("     AND E.MORG = A.ORG ")
            sqlStat.AppendLine("   WHERE A.STYMD   <= @P1 ")
            sqlStat.AppendLine("   AND   A.ENDYMD  >= @P1 ")
            sqlStat.AppendLine("   AND   A.DELFLG  = @P2 ")
            sqlStat.AppendLine("   AND   A.USERID  = @P3 ")
            sqlStat.AppendLine("   AND   B.MORGCODE = @P5 ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 20)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 20)
                Dim PARA5 As SqlParameter = sqlCmd.Parameters.Add("@P5", System.Data.SqlDbType.Char, 20)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = USERID
                PARA4.Value = COA0019Session.SYSCODE
                PARA5.Value = GBC_JOT_ORG
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    ERR = C_MESSAGENO.NODATA
                    While sqlDr.Read
                        COUNTRYCODE = Convert.ToString(sqlDr("COUNTRYCODE"))
                        OFFICENAME = Convert.ToString(sqlDr("NAMES"))
                        OFFICECODE = Convert.ToString(sqlDr("CARRIERCODE"))
                        TAXRATE = Convert.ToString(sqlDr("TAXRATE"))
                        DATEFORMAT = Convert.ToString(sqlDr("DATEFORMAT"))
                        DATEYMFORMAT = Regex.Replace(Convert.ToString(sqlDr("DATEFORMAT")), "(^d+.|.d+)", String.Empty)
                        DECIMALPLACES = Convert.ToString(sqlDr("DECIMALPLACES"))
                        USERORG = Convert.ToString(sqlDr("USERORG"))
                        IS_JOTUSER = False
                        IS_AGENTTOPUSER = False
                        If Convert.ToString(sqlDr("ISJOTUSER")) = "1" Then
                            IS_JOTUSER = True
                        End If
                        If Convert.ToString(sqlDr("PROFID")) = "AgentTOP" Then
                            IS_AGENTTOPUSER = True
                        End If
                        ERR = C_MESSAGENO.NORMAL
                    End While
                End Using
            End Using

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

End Structure

''' <summary>
''' 国関連情報取得
''' </summary>
Public Structure GBA00004CountryRelated

    ''' <summary>
    ''' 国コード
    ''' </summary>
    ''' <returns></returns>
    Public Property COUNTRYCODE As String

    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' ListBox(代理店)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_OFFICE As ListBox

    ''' <summary>
    ''' ListBox(港)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_PORT As ListBox

    ''' <summary>
    ''' ListBox(デポ)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_DEPOT As ListBox

    ''' <summary>
    ''' ListBox(荷主)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_SHIPPER As ListBox

    ''' <summary>
    ''' ListBox(顧客)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_CUSTOMER As ListBox

    ''' <summary>
    ''' ListBox(荷受人)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_CONSIGNEE As ListBox

    ''' <summary>
    ''' ListBox(Agent)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_AGENT As ListBox

    ''' <summary>
    ''' ListBox(Forwarder)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_FORWARDER As ListBox

    ''' <summary>
    ''' ListBox(Vender)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_VENDER As ListBox
    ''' <summary>
    ''' ListBox(Other)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_OTHER As ListBox

    ''' <summary>
    ''' 名称保持用ディクショナリ(代理店)
    ''' </summary>
    ''' <returns></returns>
    Public Property OfficeKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(港)
    ''' </summary>
    ''' <returns></returns>
    Public Property PortKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(デポ)
    ''' </summary>
    ''' <returns></returns>
    Public Property DepotKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(Agent)
    ''' </summary>
    ''' <returns></returns>
    Public Property AgentKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(荷主)
    ''' </summary>
    ''' <returns></returns>
    Public Property ShipperKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(荷受人)
    ''' </summary>
    ''' <returns></returns>
    Public Property ConsigneeKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(顧客)
    ''' </summary>
    ''' <returns></returns>
    Public Property CustomerKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(Forwarder)
    ''' </summary>
    ''' <returns></returns>
    Public Property ForwarderKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(Vender)
    ''' </summary>
    ''' <returns></returns>
    Public Property VenderKeyValue As Dictionary(Of String, String)
    ''' <summary>
    ''' 名称保持用ディクショナリ(Other)
    ''' </summary>
    ''' <returns></returns>
    Public Property OtherKeyValue As Dictionary(Of String, String)


    ''' <summary>
    ''' <para>国関連代理店リスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListOffice()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        '●In PARAMチェック
        If IsNothing(COUNTRYCODE) Then

            ERR = C_MESSAGENO.DLLIFERROR
            COA0000DllMessage.MessageCode = ERR
            COA0000DllMessage.COA0000GetMesssage()
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errMessage = COA0000DllMessage.MessageStrEN
            Else
                errMessage = COA0000DllMessage.MessageStrJA
            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = errMessage & "(COUNTRYCODE)"
            COA0003LogFile.MESSAGENO = ERR
            COA0003LogFile.COA0003WriteLog()
            Return

        End If

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(TR.CARRIERCODE) as CARRIERCODE, TR.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0001_COUNTRY   as CR ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG ")
            sqlStat.AppendLine("   ON    CR.ORGCODE      = OG.MORGCODE ")
            sqlStat.AppendLine("   AND   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine(" INNER JOIN GBM0005_TRADER  as TR ")
            sqlStat.AppendLine("   ON    OG.ORGCODE      = TR.MORG ")
            sqlStat.AppendLine("   AND   TR.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   TR.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   TR.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   CR.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   CR.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   CR.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   CR.COUNTRYCODE  = @P3 ")
            sqlStat.AppendLine(" ORDER BY TR.CARRIERCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = COUNTRYCODE

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    OfficeKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CARRIERCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CARRIERCODE")))
                        LISTBOX_OFFICE.Items.Add(listitem)
                        OfficeKeyValue.Add(Convert.ToString(sqlDr("CARRIERCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_OFFICE.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>国関連港リスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListPort()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        '●In PARAMチェック
        If IsNothing(COUNTRYCODE) Then

            ERR = C_MESSAGENO.DLLIFERROR
            COA0000DllMessage.MessageCode = ERR
            COA0000DllMessage.COA0000GetMesssage()
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errMessage = COA0000DllMessage.MessageStrEN
            Else
                errMessage = COA0000DllMessage.MessageStrJA
            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = errMessage & "(COUNTRYCODE)"
            COA0003LogFile.MESSAGENO = ERR
            COA0003LogFile.COA0003WriteLog()
            Return

        End If

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "AREANAME"
            Else
                NameCol = "AREANAME"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(PT.PORTCODE) as PORTCODE, PT.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0001_COUNTRY   as CR ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG ")
            sqlStat.AppendLine("   ON    CR.ORGCODE      = OG.MORGCODE ")
            sqlStat.AppendLine("   AND   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG2 ")
            sqlStat.AppendLine("   ON    OG.ORGCODE      = OG2.MORGCODE ")
            sqlStat.AppendLine("   And   OG2.ORGLEVEL    = @P4 ")
            sqlStat.AppendLine("   AND   OG2.STYMD      <= @P1 ")
            sqlStat.AppendLine("   AND   OG2.ENDYMD     >= @P1 ")
            sqlStat.AppendLine("   AND   OG2.DELFLG      = @P2 ")
            sqlStat.AppendLine(" INNER JOIN GBM0002_PORT  as PT ")
            sqlStat.AppendLine("   ON    OG2.ORGCODE     = PT.ORGCODE ")
            sqlStat.AppendLine("   AND   PT.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   PT.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   PT.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   CR.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   CR.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   CR.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   CR.COUNTRYCODE  = @P3 ")
            sqlStat.AppendLine(" ORDER BY PT.PORTCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = COUNTRYCODE
                PARA4.Value = GBC_ORGLEVEL.PORT

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    PortKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("PORTCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("PORTCODE")))
                        LISTBOX_PORT.Items.Add(listitem)
                        PortKeyValue.Add(Convert.ToString(sqlDr("PORTCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_PORT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>国関連デポリスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListDepot()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        '●In PARAMチェック
        If IsNothing(COUNTRYCODE) Then

            ERR = C_MESSAGENO.DLLIFERROR
            COA0000DllMessage.MessageCode = ERR
            COA0000DllMessage.COA0000GetMesssage()
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errMessage = COA0000DllMessage.MessageStrEN
            Else
                errMessage = COA0000DllMessage.MessageStrJA
            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = errMessage & "(COUNTRYCODE)"
            COA0003LogFile.MESSAGENO = ERR
            COA0003LogFile.COA0003WriteLog()
            Return

        End If

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(DP.DEPOTCODE) as DEPOTCODE, DP.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0001_COUNTRY   as CR ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG ")
            sqlStat.AppendLine("   ON    CR.ORGCODE      = OG.MORGCODE ")
            sqlStat.AppendLine("   AND   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG2 ")
            sqlStat.AppendLine("   ON    OG.ORGCODE      = OG2.MORGCODE ")
            sqlStat.AppendLine("   And   OG2.ORGLEVEL    = @P4 ")
            sqlStat.AppendLine("   AND   OG2.STYMD      <= @P1 ")
            sqlStat.AppendLine("   AND   OG2.ENDYMD     >= @P1 ")
            sqlStat.AppendLine("   AND   OG2.DELFLG      = @P2 ")
            sqlStat.AppendLine(" INNER JOIN GBM0003_DEPOT  as DP ")
            sqlStat.AppendLine("   ON    OG2.ORGCODE     = DP.ORGCODE ")
            sqlStat.AppendLine("   AND   DP.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   DP.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   DP.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   CR.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   CR.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   CR.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   CR.COUNTRYCODE  = @P3 ")
            sqlStat.AppendLine(" ORDER BY DP.DEPOTCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = COUNTRYCODE
                PARA4.Value = GBC_ORGLEVEL.DEPOT

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    DepotKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("DEPOTCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("DEPOTCODE")))
                        LISTBOX_DEPOT.Items.Add(listitem)
                        DepotKeyValue.Add(Convert.ToString(sqlDr("DEPOTCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_DEPOT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>業者「その他」リスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListOther()
        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("SELECT * FROM (")
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(DP.DEPOTCODE) as CODE, DP.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0001_COUNTRY   as CR ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG ")
            sqlStat.AppendLine("   ON    CR.ORGCODE      = OG.MORGCODE ")
            sqlStat.AppendLine("   AND   OG.STYMD       <= @ENTYMD ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @ENTYMD ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @DELFLG ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG2 ")
            sqlStat.AppendLine("   ON    OG.ORGCODE      = OG2.MORGCODE ")
            sqlStat.AppendLine("   And   OG2.ORGLEVEL    = @ORGLEVEL ")
            sqlStat.AppendLine("   AND   OG2.STYMD      <= @ENTYMD ")
            sqlStat.AppendLine("   AND   OG2.ENDYMD     >= @ENTYMD ")
            sqlStat.AppendLine("   AND   OG2.DELFLG      = @DELFLG ")
            sqlStat.AppendLine(" INNER JOIN GBM0003_DEPOT  as DP ")
            sqlStat.AppendLine("   ON    OG2.ORGCODE     = DP.ORGCODE ")
            sqlStat.AppendLine("   AND   DP.STYMD       <= @ENTYMD ")
            sqlStat.AppendLine("   AND   DP.ENDYMD      >= @ENTYMD ")
            sqlStat.AppendLine("   AND   DP.DELFLG       = @DELFLG ")
            sqlStat.AppendLine(" WHERE   CR.STYMD       <= @ENTYMD ")
            sqlStat.AppendLine("   AND   CR.ENDYMD      >= @ENTYMD ")
            sqlStat.AppendLine("   AND   CR.DELFLG       = @DELFLG ")
            If Not IsNothing(COUNTRYCODE) Then
                sqlStat.AppendLine("   AND   CR.COUNTRYCODE  = @COUNTRYCODE ")
            End If

            sqlStat.AppendLine(" UNION ALL ")

            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(CARRIERCODE) as CODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0005_TRADER    ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @COMPCODE ")
            sqlStat.AppendLine("   AND   STYMD       <= @ENTYMD ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @ENTYMD ")
            sqlStat.AppendLine("   AND   DELFLG       = @DELFLG ")
            sqlStat.AppendLine("   AND   (    CLASS   = '" & C_TRADER.CLASS.TRUCKER & "' ")
            sqlStat.AppendLine("           OR CLASS   = '" & C_TRADER.CLASS.AGENT & "' ")
            sqlStat.AppendLine("           OR CLASS   = '" & C_TRADER.CLASS.CARRIER & "' ")
            sqlStat.AppendLine("         )")
            If Not IsNothing(COUNTRYCODE) Then
                sqlStat.AppendLine("   AND   COUNTRYCODE  = @COUNTRYCODE ")
            End If
            sqlStat.AppendLine(") OTHERS")
            sqlStat.AppendLine(" ORDER BY OTHERS.CODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = COA0019Session.APSRVCamp
                    .Add("@ENTYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@ORGLEVEL", System.Data.SqlDbType.NVarChar).Value = GBC_ORGLEVEL.DEPOT
                    .Add("@DELFLG", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    If Not IsNothing(Me.COUNTRYCODE) Then
                        .Add("@COUNTRYCODE", System.Data.SqlDbType.NVarChar).Value = COUNTRYCODE
                    End If
                End With

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    Me.OtherKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CODE")))
                        Me.LISTBOX_OTHER.Items.Add(listitem)
                        Me.OtherKeyValue.Add(Convert.ToString(sqlDr("CODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_OTHER.Items.Count > 0 Then
                Me.ERR = C_MESSAGENO.NORMAL
            Else
                Me.ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try
    End Sub


    ''' <summary>
    ''' <para>国関連荷主リスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListShipper()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMES"
            Else
                NameCol = "NAMESEN"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(CUSTOMERCODE) as CUSTOMERCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0004_CUSTOMER ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            If Not IsNothing(COUNTRYCODE) Then
                sqlStat.AppendLine("   AND   COUNTRYCODE  = @P2 ")
            End If
            sqlStat.AppendLine("   AND   STYMD       <= @P3 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P3 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P4 ")
            sqlStat.AppendLine("   AND   CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "') ")
            sqlStat.AppendLine(" ORDER BY CUSTOMERCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 1)
                PARA1.Value = COA0019Session.APSRVCamp
                If Not IsNothing(COUNTRYCODE) Then
                    Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    PARA2.Value = COUNTRYCODE
                End If
                PARA3.Value = Date.Now
                PARA4.Value = CONST_FLAG_NO

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    ShipperKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CUSTOMERCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CUSTOMERCODE")))
                        LISTBOX_SHIPPER.Items.Add(listitem)
                        ShipperKeyValue.Add(Convert.ToString(sqlDr("CUSTOMERCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_SHIPPER.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>国関連荷受人リスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListConsignee()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMES"
            Else
                NameCol = "NAMESEN"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(CUSTOMERCODE) as CUSTOMERCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0004_CUSTOMER ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            If Not IsNothing(COUNTRYCODE) Then
                sqlStat.AppendLine("   AND   COUNTRYCODE  = @P2 ")
            End If
            sqlStat.AppendLine("   AND   STYMD       <= @P3 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P3 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P4 ")
            sqlStat.AppendLine("   AND   CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "') ")
            sqlStat.AppendLine(" ORDER BY CUSTOMERCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 1)
                PARA1.Value = COA0019Session.APSRVCamp
                If Not IsNothing(COUNTRYCODE) Then
                    Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    PARA2.Value = COUNTRYCODE
                End If
                PARA3.Value = Date.Now
                PARA4.Value = CONST_FLAG_NO

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    ConsigneeKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CUSTOMERCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CUSTOMERCODE")))
                        LISTBOX_CONSIGNEE.Items.Add(listitem)
                        ConsigneeKeyValue.Add(Convert.ToString(sqlDr("CUSTOMERCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_CONSIGNEE.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>国関連顧客リスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListCustomer()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMES"
            Else
                NameCol = "NAMESEN"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(CUSTOMERCODE) as CUSTOMERCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0004_CUSTOMER ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            If Not IsNothing(COUNTRYCODE) Then
                sqlStat.AppendLine("   AND   COUNTRYCODE  = @P2 ")
            End If
            sqlStat.AppendLine("   AND   STYMD       <= @P3 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P3 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P4 ")
            sqlStat.AppendLine(" ORDER BY CUSTOMERCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 1)
                PARA1.Value = COA0019Session.APSRVCamp
                If Not IsNothing(COUNTRYCODE) Then
                    Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    PARA2.Value = COUNTRYCODE
                End If
                PARA3.Value = Date.Now
                PARA4.Value = CONST_FLAG_NO

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    CustomerKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CUSTOMERCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CUSTOMERCODE")))
                        LISTBOX_CUSTOMER.Items.Add(listitem)
                        CustomerKeyValue.Add(Convert.ToString(sqlDr("CUSTOMERCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_CUSTOMER.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>国関連Agentリスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListAgent()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(CARRIERCODE) as CARRIERCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0005_TRADER    ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            sqlStat.AppendLine("   AND   STYMD       <= @P2 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P2 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P3 ")
            sqlStat.AppendLine("   AND   CLASS        = '" & C_TRADER.CLASS.AGENT & "' ")
            If Not IsNothing(COUNTRYCODE) Then
                sqlStat.AppendLine("   AND   COUNTRYCODE  = @P4 ")
            End If
            sqlStat.AppendLine(" ORDER BY CARRIERCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 1)
                PARA1.Value = COA0019Session.APSRVCamp
                PARA2.Value = Date.Now
                PARA3.Value = CONST_FLAG_NO
                If Not IsNothing(COUNTRYCODE) Then
                    Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA4.Value = COUNTRYCODE
                End If

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    AgentKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CARRIERCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CARRIERCODE")))
                        LISTBOX_AGENT.Items.Add(listitem)
                        AgentKeyValue.Add(Convert.ToString(sqlDr("CARRIERCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_AGENT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>国関連Forwarderリスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListForwarder()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(CARRIERCODE) as CARRIERCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0005_TRADER    ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            sqlStat.AppendLine("   AND   STYMD       <= @P2 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P2 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P3 ")
            'sqlStat.AppendLine("   AND   CLASS        = 'FORWARDER' ")
            sqlStat.AppendLine("   AND   CLASS        = '" & C_TRADER.CLASS.CARRIER & "' ")
            If Not IsNothing(COUNTRYCODE) Then
                sqlStat.AppendLine("   AND   COUNTRYCODE  = @P4 ")
            End If
            sqlStat.AppendLine(" ORDER BY CARRIERCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 1)
                PARA1.Value = COA0019Session.APSRVCamp
                PARA2.Value = Date.Now
                PARA3.Value = CONST_FLAG_NO
                If Not IsNothing(COUNTRYCODE) Then
                    Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA4.Value = COUNTRYCODE
                End If

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    ForwarderKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CARRIERCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CARRIERCODE")))
                        LISTBOX_FORWARDER.Items.Add(listitem)
                        ForwarderKeyValue.Add(Convert.ToString(sqlDr("CARRIERCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_FORWARDER.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>国関連Venderリスト取得</para>
    ''' <para>国コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00004getLeftListVender()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(CARRIERCODE) as CARRIERCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0005_TRADER    ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            sqlStat.AppendLine("   AND   STYMD       <= @P2 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P2 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P3 ")
            'sqlStat.AppendLine("   AND   CLASS        = 'VENDER' ")
            sqlStat.AppendLine("   AND   CLASS        = '" & C_TRADER.CLASS.TRUCKER & "' ")
            If Not IsNothing(COUNTRYCODE) Then
                sqlStat.AppendLine("   AND   COUNTRYCODE  = @P4 ")
            End If
            sqlStat.AppendLine(" ORDER BY CARRIERCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 1)
                PARA1.Value = COA0019Session.APSRVCamp
                PARA2.Value = Date.Now
                PARA3.Value = CONST_FLAG_NO
                If Not IsNothing(COUNTRYCODE) Then
                    Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                    PARA4.Value = COUNTRYCODE
                End If

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    VenderKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CARRIERCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CARRIERCODE")))
                        LISTBOX_VENDER.Items.Add(listitem)
                        VenderKeyValue.Add(Convert.ToString(sqlDr("CARRIERCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_VENDER.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

End Structure
''' <summary>
''' 代理店関連情報取得
''' </summary>
Public Structure GBA00005OfficeRelated
    ''' <summary>
    ''' 代理店コード
    ''' </summary>
    ''' <returns></returns>
    Public Property OFFICECODE As String
    ''' <summary>
    ''' [OUT]国コード
    ''' </summary>
    ''' <returns></returns>
    Public Property COUNTRYCODE As String
    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' ListBox(港)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_PORT As ListBox

    ''' <summary>
    ''' ListBox(デポ)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_DEPOT As ListBox

    ''' <summary>
    ''' 名称保持用ディクショナリ(港)
    ''' </summary>
    ''' <returns></returns>
    Public Property PortKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(デポ)
    ''' </summary>
    ''' <returns></returns>
    Public Property DepotKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' <para>代理店関連港リスト取得</para>
    ''' <para>代理店コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00005getLeftListPort()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        '●In PARAMチェック
        If IsNothing(OFFICECODE) Then

            ERR = C_MESSAGENO.DLLIFERROR
            COA0000DllMessage.MessageCode = ERR
            COA0000DllMessage.COA0000GetMesssage()
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errMessage = COA0000DllMessage.MessageStrEN
            Else
                errMessage = COA0000DllMessage.MessageStrJA
            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = errMessage & "(OFFICECODE)"
            COA0003LogFile.MESSAGENO = ERR
            COA0003LogFile.COA0003WriteLog()
            Return

        End If

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "AREANAME"
            Else
                NameCol = "AREANAME"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(PT.PORTCODE) as PORTCODE, PT.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0005_TRADER    as TR ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG ")
            sqlStat.AppendLine("   ON    TR.MORG          = OG.ORGCODE ")
            sqlStat.AppendLine("   AND   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG2 ")
            sqlStat.AppendLine("   ON    OG.ORGCODE      = OG2.MORGCODE ")
            sqlStat.AppendLine("   AND   OG2.STYMD      <= @P1 ")
            sqlStat.AppendLine("   AND   OG2.ENDYMD     >= @P1 ")
            sqlStat.AppendLine("   AND   OG2.DELFLG      = @P2 ")
            sqlStat.AppendLine("   AND   OG2.ORGLEVEL    = @P4 ")
            sqlStat.AppendLine(" INNER JOIN GBM0002_PORT  as PT ")
            sqlStat.AppendLine("   ON    OG2.ORGCODE     = PT.ORGCODE ")
            sqlStat.AppendLine("   AND   PT.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   PT.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   PT.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   TR.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   TR.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   TR.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   TR.CARRIERCODE  = @P3 ")
            sqlStat.AppendLine(" ORDER BY PT.PORTCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = OFFICECODE
                PARA4.Value = GBC_ORGLEVEL.PORT

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    PortKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("PORTCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("PORTCODE")))
                        LISTBOX_PORT.Items.Add(listitem)
                        PortKeyValue.Add(Convert.ToString(sqlDr("PORTCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_PORT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>代理店関連デポリスト取得</para>
    ''' <para>代理店コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00005getLeftListDepot()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        '●In PARAMチェック
        If IsNothing(OFFICECODE) Then

            ERR = C_MESSAGENO.DLLIFERROR
            COA0000DllMessage.MessageCode = ERR
            COA0000DllMessage.COA0000GetMesssage()
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errMessage = COA0000DllMessage.MessageStrEN
            Else
                errMessage = COA0000DllMessage.MessageStrJA
            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = errMessage & "(OFFICECODE)"
            COA0003LogFile.MESSAGENO = ERR
            COA0003LogFile.COA0003WriteLog()
            Return

        End If

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(DP.DEPOTCODE) as DEPOTCODE, DP.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0005_TRADER    as TR ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG ")
            sqlStat.AppendLine("   ON    TR.MORG          = OG.ORGCODE ")
            sqlStat.AppendLine("   AND   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG2 ")
            sqlStat.AppendLine("   ON    OG.ORGCODE      = OG2.MORGCODE ")
            sqlStat.AppendLine("   AND   OG2.STYMD      <= @P1 ")
            sqlStat.AppendLine("   AND   OG2.ENDYMD     >= @P1 ")
            sqlStat.AppendLine("   AND   OG2.DELFLG      = @P2 ")
            sqlStat.AppendLine("   AND   OG2.ORGLEVEL    = @P4 ")
            sqlStat.AppendLine(" INNER JOIN GBM0003_DEPOT  as DP ")
            sqlStat.AppendLine("   ON    OG2.ORGCODE     = DP.ORGCODE ")
            sqlStat.AppendLine("   AND   DP.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   DP.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   DP.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   TR.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   TR.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   TR.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   TR.CARRIERCODE  = @P3 ")
            sqlStat.AppendLine(" ORDER BY DP.DEPOTCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = OFFICECODE
                PARA4.Value = GBC_ORGLEVEL.DEPOT

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    DepotKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("DEPOTCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("DEPOTCODE")))
                        LISTBOX_DEPOT.Items.Add(listitem)
                        DepotKeyValue.Add(Convert.ToString(sqlDr("DEPOTCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_DEPOT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>代理店関連国取得</para>
    ''' <para>代理店コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    ''' <remarks>1代理店は1国と結びつく前提。崩れた場合は要検討</remarks>
    Public Sub GBA00005getCountry()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        '●In PARAMチェック
        If IsNothing(OFFICECODE) Then

            ERR = C_MESSAGENO.DLLIFERROR
            COA0000DllMessage.MessageCode = ERR
            COA0000DllMessage.COA0000GetMesssage()
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errMessage = COA0000DllMessage.MessageStrEN
            Else
                errMessage = COA0000DllMessage.MessageStrJA
            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = errMessage & "(OFFICECODE)"
            COA0003LogFile.MESSAGENO = ERR
            COA0003LogFile.COA0003WriteLog()
            Return

        End If

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(CT.COUNTRYCODE) as COUNTRYCODE, CT.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0005_TRADER    as TR ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG ")
            sqlStat.AppendLine("   ON    TR.MORG          = OG.ORGCODE ")
            sqlStat.AppendLine("   AND   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG2 ")
            sqlStat.AppendLine("   ON    OG.MORGCODE      = OG2.ORGCODE ")
            sqlStat.AppendLine("   AND   OG2.STYMD      <= @P1 ")
            sqlStat.AppendLine("   AND   OG2.ENDYMD     >= @P1 ")
            sqlStat.AppendLine("   AND   OG2.DELFLG      = @P2 ")
            sqlStat.AppendLine("   AND   OG2.ORGLEVEL    = @P4 ")
            sqlStat.AppendLine(" INNER JOIN GBM0001_COUNTRY  as CT ")
            sqlStat.AppendLine("   ON    OG2.ORGCODE     = CT.ORGCODE ")
            sqlStat.AppendLine("   AND   CT.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   CT.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   CT.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   TR.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   TR.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   TR.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   TR.CARRIERCODE  = @P3 ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = OFFICECODE
                PARA4.Value = GBC_ORGLEVEL.COUNTRY

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    While sqlDr.Read
                        Me.COUNTRYCODE = Convert.ToString(sqlDr("COUNTRYCODE"))
                        Exit While
                    End While
                End Using
            End Using

            If Me.COUNTRYCODE IsNot Nothing AndAlso Me.COUNTRYCODE <> "" Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
End Structure
''' <summary>
''' 港関連情報取得
''' </summary>
Public Structure GBA00006PortRelated
    ''' <summary>
    ''' 港コード
    ''' </summary>
    ''' <returns></returns>
    Public Property PORTCODE As String

    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' ListBox(代理店)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_OFFICE As ListBox

    ''' <summary>
    ''' ListBox(デポ)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_DEPOT As ListBox

    ''' <summary>
    ''' 名称保持用ディクショナリ(代理店)
    ''' </summary>
    ''' <returns></returns>
    Public Property OfficeKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(デポ)
    ''' </summary>
    ''' <returns></returns>
    Public Property DepotKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' <para>港関連代理店リスト取得</para>
    ''' <para>代理店コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00006getLeftListOffice()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        '●In PARAMチェック
        If IsNothing(PORTCODE) Then

            ERR = C_MESSAGENO.DLLIFERROR
            COA0000DllMessage.MessageCode = ERR
            COA0000DllMessage.COA0000GetMesssage()
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errMessage = COA0000DllMessage.MessageStrEN
            Else
                errMessage = COA0000DllMessage.MessageStrJA
            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = errMessage & "(OFFICECODE)"
            COA0003LogFile.MESSAGENO = ERR
            COA0003LogFile.COA0003WriteLog()
            Return

        End If

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(TR.CARRIERCODE) as CARRIERCODE, TR.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0002_PORT    as PT ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG ")
            sqlStat.AppendLine("   ON    PT.ORGCODE      = OG.ORGCODE ")
            sqlStat.AppendLine("   AND   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG2 ")
            sqlStat.AppendLine("   ON    OG.MORGCODE     = OG2.ORGCODE ")
            sqlStat.AppendLine("   AND   OG2.STYMD      <= @P1 ")
            sqlStat.AppendLine("   AND   OG2.ENDYMD     >= @P1 ")
            sqlStat.AppendLine("   AND   OG2.DELFLG      = @P2 ")
            sqlStat.AppendLine(" INNER JOIN GBM0005_TRADER as TR ")
            sqlStat.AppendLine("   ON    OG2.ORGCODE     = TR.MORG ")
            sqlStat.AppendLine("   AND   TR.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   TR.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   TR.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   PT.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   PT.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   PT.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   PT.PORTCODE     = @P3 ")
            sqlStat.AppendLine(" ORDER BY TR.CARRIERCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = PORTCODE

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    OfficeKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CARRIERCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CARRIERCODE")))
                        LISTBOX_OFFICE.Items.Add(listitem)
                        OfficeKeyValue.Add(Convert.ToString(sqlDr("CARRIERCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_OFFICE.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>港関連デポリスト取得</para>
    ''' <para>代理店コード</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00006getLeftListDepot()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        '●In PARAMチェック
        If IsNothing(PORTCODE) Then

            ERR = C_MESSAGENO.DLLIFERROR
            COA0000DllMessage.MessageCode = ERR
            COA0000DllMessage.COA0000GetMesssage()
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errMessage = COA0000DllMessage.MessageStrEN
            Else
                errMessage = COA0000DllMessage.MessageStrJA
            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = errMessage & "(OFFICECODE)"
            COA0003LogFile.MESSAGENO = ERR
            COA0003LogFile.COA0003WriteLog()
            Return

        End If

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(DP.DEPOTCODE) as DEPOTCODE, DP.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM  GBM0002_PORT    as PT ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG ")
            sqlStat.AppendLine("   ON    PT.ORGCODE      = OG.ORGCODE ")
            sqlStat.AppendLine("   AND   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine(" INNER JOIN COS0021_ORG  as OG2 ")
            sqlStat.AppendLine("   ON    OG.MORGCODE     = OG2.MORGCODE ")
            sqlStat.AppendLine("   AND   OG2.ORGLEVEL    = @P4 ")
            sqlStat.AppendLine("   AND   OG2.STYMD      <= @P1 ")
            sqlStat.AppendLine("   AND   OG2.ENDYMD     >= @P1 ")
            sqlStat.AppendLine("   AND   OG2.DELFLG      = @P2 ")
            sqlStat.AppendLine(" INNER JOIN GBM0003_DEPOT as DP ")
            sqlStat.AppendLine("   ON    OG2.ORGCODE     = DP.ORGCODE ")
            sqlStat.AppendLine("   AND   DP.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   DP.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   DP.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   PT.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   PT.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   PT.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   PT.PORTCODE     = @P3 ")
            sqlStat.AppendLine(" ORDER BY DP.DEPOTCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = PORTCODE
                PARA4.Value = GBC_ORGLEVEL.DEPOT

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    DepotKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("DEPOTCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("DEPOTCODE")))
                        LISTBOX_DEPOT.Items.Add(listitem)
                        DepotKeyValue.Add(Convert.ToString(sqlDr("DEPOTCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_DEPOT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' ポートコード、名称リスト取得
    ''' </summary>
    ''' <param name="countryCode">国コード（未指定は無条件）</param>
    ''' <param name="portCode">港コード（未指定は無条件）</param>
    ''' <returns>コード、コード+":"+名称、名称、国コード、エリアコードのデータテーブル</returns>
    Public Shared Function GBA00006getPortCodeValue(Optional countryCode As String = "", Optional portCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT PORTCODE")
        sqlStat.AppendLine("      ,AREANAME AS NAME")
        sqlStat.AppendLine("      ,PORTCODE + ':' + AREANAME AS LISTBOXNAME")
        sqlStat.AppendLine("      ,COUNTRYCODE AS COUNTRYCODE")
        sqlStat.AppendLine("      ,AREACODE    AS AREACODE")
        sqlStat.AppendLine("  FROM GBM0002_PORT")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        If portCode <> "" Then
            sqlStat.AppendLine("   AND PORTCODE    = @PORTCODE")
        End If
        If countryCode <> "" Then
            sqlStat.AppendLine("   AND COUNTRYCODE    = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY PORTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@PORTCODE", SqlDbType.NVarChar).Value = portCode
                .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = countryCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With

            'SQLパラメータ設定
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
End Structure

''' <summary>
''' 組織関連情報取得
''' </summary>
Public Structure GBA00007OrganizationRelated
    ''' <summary>
    ''' ユーザーID
    ''' </summary>
    ''' <returns></returns>
    Public Property USERID As String
    ''' <summary>
    ''' [IN]ユーザーマスタのORG
    ''' </summary>
    ''' <returns></returns>
    Public Property USERORG As String
    ''' <summary>
    ''' [IN]JOT除外フラグ(検索結果からJOT関連のオフィスを除外する)1:除外、それ以外含める
    ''' </summary>
    ''' <returns></returns>
    Public Property OPTJOTEXCLUSION As String
    ''' <summary>
    ''' [IN]MORG(Country)
    ''' </summary>
    ''' <returns></returns>
    Public Property MORGC As String
    ''' <summary>
    ''' [IN]MORG(Office)
    ''' </summary>
    ''' <returns></returns>
    Public Property MORGO As String
    ''' <summary>
    ''' [IN]MORG(Port)
    ''' </summary>
    ''' <returns></returns>
    Public Property MORGP As String
    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' ListBox(国)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_COUNTRY As ListBox

    ''' <summary>
    ''' ListBox(代理店)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_OFFICE As ListBox

    ''' <summary>
    ''' ListBox(港)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_PORT As ListBox

    ''' <summary>
    ''' ListBox(デポ)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_DEPOT As ListBox

    ''' <summary>
    ''' ListBox(組織(国))
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_ORG_COUNTRY As ListBox

    ''' <summary>
    ''' ListBox(組織(代理店))
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_ORG_OFFICE As ListBox

    ''' <summary>
    ''' ListBox(組織(港))
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_ORG_PORT As ListBox

    ''' <summary>
    ''' ListBox(組織(デポ))
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_ORG_DEPOT As ListBox

    ''' <summary>
    ''' 名称保持用ディクショナリ(国)
    ''' </summary>
    ''' <returns></returns>
    Public Property CountryKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(代理店)
    ''' </summary>
    ''' <returns></returns>
    Public Property OfficeKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(港)
    ''' </summary>
    ''' <returns></returns>
    Public Property PortKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(デポ)
    ''' </summary>
    ''' <returns></returns>
    Public Property DepotKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(組織(国))
    ''' </summary>
    ''' <returns></returns>
    Public Property OrgCountryKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(組織(代理店))
    ''' </summary>
    ''' <returns></returns>
    Public Property OrgOfficeKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(組織(港))
    ''' </summary>
    ''' <returns></returns>
    Public Property OrgPortKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' 名称保持用ディクショナリ(組織(デポ))
    ''' </summary>
    ''' <returns></returns>
    Public Property OrgDepotKeyValue As Dictionary(Of String, String)

    ''' <summary>
    ''' <para>組織関連国リスト取得</para>
    ''' <para>ユーザーID</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00007getLeftListCountry()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try
            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(CT.COUNTRYCODE) as COUNTRYCODE, CT.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM COS0021_ORG AS OG ")
            sqlStat.AppendLine(" INNER JOIN GBM0001_COUNTRY AS CT ")
            sqlStat.AppendLine("    ON   CT.ORGCODE      = OG.ORGCODE ")
            sqlStat.AppendLine("   AND   CT.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   CT.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   CT.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   OG.ORGLEVEL     = @P3 ")
            sqlStat.AppendLine(" ORDER BY CT.COUNTRYCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = GBC_ORGLEVEL.COUNTRY

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    CountryKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("COUNTRYCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("COUNTRYCODE")))
                        LISTBOX_COUNTRY.Items.Add(listitem)
                        CountryKeyValue.Add(Convert.ToString(sqlDr("COUNTRYCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_COUNTRY.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>組織関連代理店リスト取得</para>
    ''' <para>ユーザーID</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00007getLeftListOffice()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If
            Dim userOrg As String = ""
            If Me.USERORG IsNot Nothing Then
                userOrg = Me.USERORG
            End If
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(TR.CARRIERCODE) as CARRIERCODE, TR.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM COS0021_ORG AS OG ")
            sqlStat.AppendLine(" INNER JOIN GBM0005_TRADER AS TR ")
            sqlStat.AppendLine("    ON   TR.MORG         = OG.ORGCODE ")
            sqlStat.AppendLine("   AND   TR.STYMD       <= @SEYMD ")
            sqlStat.AppendLine("   AND   TR.ENDYMD      >= @SEYMD ")
            sqlStat.AppendLine("   AND   TR.DELFLG       = @DELFLG ")
            sqlStat.AppendLine(" WHERE   OG.STYMD       <= @SEYMD ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @SEYMD ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @DELFLG ")
            sqlStat.AppendLine("   AND   OG.ORGLEVEL     = @ORGLEVEL ")
            If userOrg <> "" AndAlso Not userOrg.StartsWith("JO") Then
                sqlStat.AppendLine("   AND   EXISTS (SELECT 1 ")
                sqlStat.AppendLine("                   FROM COS0021_ORG AS OGS")
                sqlStat.AppendLine("                  WHERE   OGS.ORGCODE      =  @USERORG")
                sqlStat.AppendLine("                    AND   OGS.MORGCODE     = OG.MORGCODE ")
                sqlStat.AppendLine("                    AND   OGS.STYMD       <= @SEYMD ")
                sqlStat.AppendLine("                    AND   OGS.ENDYMD      >= @SEYMD ")
                sqlStat.AppendLine("                    AND   OGS.DELFLG       = @DELFLG) ")
            End If
            If Me.OPTJOTEXCLUSION IsNot Nothing AndAlso Me.OPTJOTEXCLUSION = "1" Then
                sqlStat.AppendLine("   AND  OG.MORGCODE <> 'JO000000'")
            End If
            sqlStat.AppendLine(" ORDER BY TR.CARRIERCODE ")
            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@SEYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = CONST_FLAG_NO
                    .Add("@ORGLEVEL", System.Data.SqlDbType.NVarChar).Value = GBC_ORGLEVEL.OFFICE
                    .Add("@USERORG", System.Data.SqlDbType.NVarChar).Value = userOrg
                End With

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    OfficeKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("CARRIERCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("CARRIERCODE")))
                        LISTBOX_OFFICE.Items.Add(listitem)
                        OfficeKeyValue.Add(Convert.ToString(sqlDr("CARRIERCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_OFFICE.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>組織関連港リスト取得</para>
    ''' <para>ユーザーID</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00007getLeftListPort()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "AREANAME"
            Else
                NameCol = "AREANAME"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(PT.PORTCODE) as PORTCODE, PT.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM COS0021_ORG AS OG ")
            sqlStat.AppendLine(" INNER JOIN GBM0002_PORT AS PT ")
            sqlStat.AppendLine("    ON   PT.ORGCODE      = OG.ORGCODE ")
            sqlStat.AppendLine("   AND   PT.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   PT.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   PT.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   OG.ORGLEVEL     = @P3 ")
            sqlStat.AppendLine(" ORDER BY PT.PORTCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = GBC_ORGLEVEL.PORT

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    PortKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("PORTCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("PORTCODE")))
                        LISTBOX_PORT.Items.Add(listitem)
                        PortKeyValue.Add(Convert.ToString(sqlDr("PORTCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_PORT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>組織関連デポリスト取得</para>
    ''' <para>ユーザーID</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00007getLeftListDepot()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMESJP"
            Else
                NameCol = "NAMES"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(DP.DEPOTCODE) as DEPOTCODE, DP.{0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM COS0021_ORG AS OG ")
            sqlStat.AppendLine(" INNER JOIN GBM0003_DEPOT AS DP ")
            sqlStat.AppendLine("    ON   DP.ORGCODE      = OG.ORGCODE ")
            sqlStat.AppendLine("   AND   DP.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   DP.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   DP.DELFLG       = @P2 ")
            sqlStat.AppendLine(" WHERE   OG.STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   OG.ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   OG.DELFLG       = @P2 ")
            sqlStat.AppendLine("   AND   OG.ORGLEVEL     = @P3 ")
            sqlStat.AppendLine(" ORDER BY DP.DEPOTCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO
                PARA3.Value = GBC_ORGLEVEL.DEPOT

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    DepotKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("DEPOTCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("DEPOTCODE")))
                        LISTBOX_DEPOT.Items.Add(listitem)
                        DepotKeyValue.Add(Convert.ToString(sqlDr("DEPOTCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_DEPOT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>国の組織リスト取得</para>
    ''' <para>ユーザーID</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00007getLeftListOrgCountry()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMES"
            Else
                NameCol = "NAMES_EN"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(ORGCODE) as ORGCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM COS0021_ORG ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            sqlStat.AppendLine("   AND   STYMD       <= @P2 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P2 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P3 ")
            sqlStat.AppendLine("   AND   ORGLEVEL     = @P4 ")
            sqlStat.AppendLine(" ORDER BY ORGCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 1)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = COA0019Session.APSRVCamp
                PARA2.Value = Date.Now
                PARA3.Value = CONST_FLAG_NO
                PARA4.Value = GBC_ORGLEVEL.COUNTRY

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    OrgCountryKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("ORGCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("ORGCODE")))
                        LISTBOX_ORG_COUNTRY.Items.Add(listitem)
                        OrgCountryKeyValue.Add(Convert.ToString(sqlDr("ORGCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_ORG_COUNTRY.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>代理店の組織リスト取得</para>
    ''' <para>ユーザーID</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00007getLeftListOrgOffice()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMES"
            Else
                NameCol = "NAMES_EN"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(ORGCODE) as ORGCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM COS0021_ORG ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            sqlStat.AppendLine("   AND   STYMD       <= @P2 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P2 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P3 ")
            sqlStat.AppendLine("   AND   ORGLEVEL     = @P4 ")
            If Me.MORGC IsNot Nothing AndAlso Me.MORGC <> "" Then
                sqlStat.AppendLine("   AND  MORGCODE = @MORGC")
            End If
            If Me.MORGO IsNot Nothing AndAlso Me.MORGO <> "" Then
                sqlStat.AppendLine("   AND  ORGCODE = @MORGO")
            End If
            sqlStat.AppendLine(" ORDER BY ORGCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 1)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = COA0019Session.APSRVCamp
                PARA2.Value = Date.Now
                PARA3.Value = CONST_FLAG_NO
                PARA4.Value = GBC_ORGLEVEL.OFFICE
                If Me.MORGC IsNot Nothing AndAlso Me.MORGC <> "" Then
                    Dim MORGC As SqlParameter = sqlCmd.Parameters.Add("@MORGC", System.Data.SqlDbType.NVarChar)
                    MORGC.Value = Me.MORGC
                End If
                If Me.MORGO IsNot Nothing AndAlso Me.MORGO <> "" Then
                    Dim MORGO As SqlParameter = sqlCmd.Parameters.Add("@MORGO", System.Data.SqlDbType.NVarChar)
                    MORGO.Value = Me.MORGO
                End If

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    OrgOfficeKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("ORGCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("ORGCODE")))
                        LISTBOX_ORG_OFFICE.Items.Add(listitem)
                        OrgOfficeKeyValue.Add(Convert.ToString(sqlDr("ORGCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_ORG_OFFICE.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>港の組織リスト取得</para>
    ''' <para>ユーザーID</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00007getLeftListOrgPort()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMES"
            Else
                NameCol = "NAMES_EN"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(ORGCODE) as ORGCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM COS0021_ORG ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            sqlStat.AppendLine("   AND   STYMD       <= @P2 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P2 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P3 ")
            sqlStat.AppendLine("   AND   ORGLEVEL     = @P4 ")
            If Me.MORGC IsNot Nothing AndAlso Me.MORGC <> "" Then
                sqlStat.AppendLine("   AND  MORGCODE in ( SELECT distinct rtrim(ORGCODE) FROM COS0021_ORG")
                sqlStat.AppendLine("                      WHERE COMPCODE     = @P1")
                sqlStat.AppendLine("                      AND   STYMD       <= @P2")
                sqlStat.AppendLine("                      AND   ENDYMD      >= @P2")
                sqlStat.AppendLine("                      AND   DELFLG       = @P3")
                sqlStat.AppendLine("                      AND   ORGLEVEL     = @LEVELO ")
                sqlStat.AppendLine("                      AND   MORGCODE     = @MORGC )")
            End If
            If Me.MORGO IsNot Nothing AndAlso Me.MORGO <> "" Then
                sqlStat.AppendLine("   AND  MORGCODE = @MORGO")
            End If
            sqlStat.AppendLine(" ORDER BY ORGCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 1)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = COA0019Session.APSRVCamp
                PARA2.Value = Date.Now
                PARA3.Value = CONST_FLAG_NO
                PARA4.Value = GBC_ORGLEVEL.PORT
                If Me.MORGC IsNot Nothing AndAlso Me.MORGC <> "" Then
                    Dim MORGC As SqlParameter = sqlCmd.Parameters.Add("@MORGC", System.Data.SqlDbType.NVarChar)
                    MORGC.Value = Me.MORGC
                    Dim LEVELO As SqlParameter = sqlCmd.Parameters.Add("@LEVELO", System.Data.SqlDbType.NVarChar)
                    LEVELO.Value = GBC_ORGLEVEL.OFFICE
                End If
                If Me.MORGO IsNot Nothing AndAlso Me.MORGO <> "" Then
                    Dim MORGO As SqlParameter = sqlCmd.Parameters.Add("@MORGO", System.Data.SqlDbType.NVarChar)
                    MORGO.Value = Me.MORGO
                End If

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    OrgPortKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("ORGCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("ORGCODE")))
                        LISTBOX_ORG_PORT.Items.Add(listitem)
                        OrgPortKeyValue.Add(Convert.ToString(sqlDr("ORGCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_ORG_PORT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>デポの組織リスト取得</para>
    ''' <para>ユーザーID</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00007getLeftListOrgDepot()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = Nothing
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim NameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                NameCol = "NAMES"
            Else
                NameCol = "NAMES_EN"
            End If

            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("   rtrim(ORGCODE) as ORGCODE, {0} as NAME ", NameCol)
            sqlStat.AppendLine(" FROM COS0021_ORG ")
            sqlStat.AppendLine(" WHERE   COMPCODE     = @P1 ")
            sqlStat.AppendLine("   AND   STYMD       <= @P2 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P2 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P3 ")
            sqlStat.AppendLine("   AND   ORGLEVEL     = @P4 ")
            If Me.MORGC IsNot Nothing AndAlso Me.MORGC <> "" Then
                sqlStat.AppendLine("   AND  MORGCODE in ( SELECT distinct rtrim(ORGCODE) FROM COS0021_ORG")
                sqlStat.AppendLine("                      WHERE COMPCODE     = @P1")
                sqlStat.AppendLine("                      AND   STYMD       <= @P2")
                sqlStat.AppendLine("                      AND   ENDYMD      >= @P2")
                sqlStat.AppendLine("                      AND   DELFLG       = @P3")
                sqlStat.AppendLine("                      AND   ORGLEVEL     = @LEVELO ")
                sqlStat.AppendLine("                      AND   MORGCODE     = @MORGC )")
            End If
            If Me.MORGO IsNot Nothing AndAlso Me.MORGO <> "" Then
                sqlStat.AppendLine("   AND  MORGCODE = @MORGO")
            End If
            If Me.MORGP IsNot Nothing AndAlso Me.MORGP <> "" Then
                'MORGではオフィス単位でしか判断できないためORGCODEで判定
                sqlStat.AppendLine("   AND  LEFT(ORGCODE,6) = left(@MORGP,6)")
            End If
            sqlStat.AppendLine(" ORDER BY ORGCODE ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 1)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = COA0019Session.APSRVCamp
                PARA2.Value = Date.Now
                PARA3.Value = CONST_FLAG_NO
                PARA4.Value = GBC_ORGLEVEL.DEPOT
                If Me.MORGC IsNot Nothing AndAlso Me.MORGC <> "" Then
                    Dim MORGC As SqlParameter = sqlCmd.Parameters.Add("@MORGC", System.Data.SqlDbType.NVarChar)
                    MORGC.Value = Me.MORGC
                    Dim LEVELO As SqlParameter = sqlCmd.Parameters.Add("@LEVELO", System.Data.SqlDbType.NVarChar)
                    LEVELO.Value = GBC_ORGLEVEL.OFFICE
                End If
                If Me.MORGO IsNot Nothing AndAlso Me.MORGO <> "" Then
                    Dim MORGO As SqlParameter = sqlCmd.Parameters.Add("@MORGO", System.Data.SqlDbType.NVarChar)
                    MORGO.Value = Me.MORGO
                End If
                If Me.MORGP IsNot Nothing AndAlso Me.MORGP <> "" Then
                    Dim MORGP As SqlParameter = sqlCmd.Parameters.Add("@MORGP", System.Data.SqlDbType.NVarChar)
                    MORGP.Value = Me.MORGP
                End If

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    OrgDepotKeyValue = New Dictionary(Of String, String)
                    While sqlDr.Read
                        Dim listitem = New ListItem(String.Format("{0}:{1}", sqlDr("ORGCODE"), sqlDr("NAME")), Convert.ToString(sqlDr("ORGCODE")))
                        LISTBOX_ORG_DEPOT.Items.Add(listitem)
                        OrgDepotKeyValue.Add(Convert.ToString(sqlDr("ORGCODE")), Convert.ToString(sqlDr("NAME")))
                    End While
                End Using
            End Using

            If Me.LISTBOX_ORG_DEPOT.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

End Structure
Public Structure GBA00008Country

    ''' <summary>
    ''' 国コード
    ''' </summary>
    ''' <returns></returns>
    Public Property COUNTRYCODE As String

    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' 国テーブル
    ''' </summary>
    ''' <returns></returns>
    Public Property COUNTRY_TABLE As DataTable

    ''' <summary>
    ''' 国コードリストボックス
    ''' </summary>
    ''' <returns></returns>
    Public Property COUNTRY_LISTBOX As ListBox
    ''' <summary>
    ''' 国情報取得
    ''' </summary>
    Public Sub getCountryInfo()

        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim sqlStat As New StringBuilder
        Dim retRate As String = "0"
        Dim retRateDecimalPlaces As String = "0"
        Dim retRoundFlg As String = ""
        Dim retVal As String() = Nothing

        Try

            sqlStat.AppendLine("SELECT *")
            sqlStat.AppendLine("  FROM GBM0001_COUNTRY ")
            sqlStat.AppendLine(" WHERE COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND COUNTRYCODE  = @COUNTRYCODE")
            sqlStat.AppendLine("   AND STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                COUNTRY_TABLE = New DataTable
                sqlCon.Open() '接続オープン
                'SQLパラメータ設定
                Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar)
                Dim paramCountry As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar)
                Dim paramStYmd As SqlParameter = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
                Dim paramEndYmd As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
                Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                'パラメータに値設定
                paramCompCode.Value = COA0019Session.APSRVCamp
                paramCountry.Value = COUNTRYCODE
                paramStYmd.Value = Date.Now
                paramEndYmd.Value = Date.Now
                paramDelFlg.Value = CONST_FLAG_YES
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(COUNTRY_TABLE)
                End Using

                If COUNTRY_TABLE IsNot Nothing Then
                    Me.ERR = C_MESSAGENO.NORMAL
                Else
                    Me.ERR = C_MESSAGENO.NODATA
                End If

            End Using

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' 国一覧リストボックス生成
    ''' </summary>
    Public Sub getCountryList()
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get

        'パラメータCOUNTRY_LISTBOXがない場合はインスタンス生成
        If Me.COUNTRY_LISTBOX Is Nothing Then
            Me.COUNTRY_LISTBOX = New ListBox
        End If
        'アイテムのクリア
        Me.COUNTRY_LISTBOX.Items.Clear()
        Try
            'SQL文の作成
            Dim nameCol As String
            If COA0019Session.LANGDISP = C_LANG.JA Then
                nameCol = "NAMESJP"
            Else
                nameCol = "NAMES"
            End If

            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT COUNTRYCODE AS CODE")
            sqlStat.AppendFormat("      ,{0}       AS NAME", nameCol).AppendLine()
            sqlStat.AppendFormat("      ,COUNTRYCODE + ':' + {0} AS DISPLAYNAME", nameCol).AppendLine()
            sqlStat.AppendLine("  FROM GBM0001_COUNTRY ")
            sqlStat.AppendLine(" WHERE COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" ORDER BY ORGCODE")
            'DB接続
            Dim retDt As New DataTable
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
                  sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン
                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = COA0019Session.APSRVCamp
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(retDt)
                End Using 'sqlDa

                If retDt Is Nothing Then
                    Me.ERR = C_MESSAGENO.NODATA
                    Return
                End If
            End Using 'sqlCon 'sqlCmd

            With Me.COUNTRY_LISTBOX
                .DataValueField = "CODE"
                .DataTextField = "DISPLAYNAME"
                .DataSource = retDt
                .DataBind()
            End With

            Me.ERR = C_MESSAGENO.NORMAL
        Catch ex As Exception
            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub 'getCountryList

End Structure

''' <summary>
''' メール送信設定
''' </summary>
Public Structure GBA00009MailSendSet

    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <returns></returns>
    Public Property COMPCODE As String

    ''' <summary>
    ''' イベントコード
    ''' </summary>
    ''' <returns></returns>
    Public Property EVENTCODE As String

    ''' <summary>
    ''' サブコード
    ''' </summary>
    ''' <returns></returns>
    Public Property MAILSUBCODE As String

    ''' <summary>
    ''' ブレーカーID
    ''' </summary>
    ''' <returns></returns>
    Public Property BRID As String

    ''' <summary>
    ''' ブレーカーSUBCODE
    ''' </summary>
    ''' <returns></returns>
    Public Property BRSUBID As String

    ''' <summary>
    ''' ブレーカーBASEID
    ''' </summary>
    ''' <returns></returns>
    Public Property BRBASEID As String

    ''' <summary>
    ''' ブレーカーROUND(1or2)
    ''' </summary>
    ''' <returns></returns>
    Public Property BRROUND As String

    ''' <summary>
    ''' 承認最終STEP
    ''' </summary>
    ''' <returns></returns>
    Public Property LASTSTEP As String

    ''' <summary>
    ''' オーダーデータID
    ''' </summary>
    ''' <returns></returns>
    Public Property ODRDATAID As String

    ''' <summary>
    ''' 申請ID
    ''' </summary>
    ''' <returns></returns>
    Public Property APPLYID As String

    ''' <summary>
    ''' 申請ステップ
    ''' </summary>
    ''' <returns></returns>
    Public Property APPLYSTEP As String

    ''' <summary>
    ''' 更新種別（マスタ申請）
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDATETYPE As String

    ''' <summary>
    ''' ユーザID（ユーザマスタ申請）
    ''' </summary>
    ''' <returns></returns>
    Public Property USERID As String

    ''' <summary>
    ''' オーダー番号
    ''' </summary>
    ''' <returns></returns>
    Public Property ORDERNO As String
    ''' <summary>
    ''' 契約書NO(リースBR)
    ''' </summary>
    ''' <returns></returns>
    Public Property CONTRACTNO As String
    ''' <summary>
    ''' 協定書NO(リースBR)
    ''' </summary>
    ''' <returns></returns>
    Public Property AGREEMENTNO As String
    ''' <summary>
    ''' [IN]締月(SOA BillingCloseのみで使用する想定)
    ''' </summary>
    ''' <returns></returns>
    Public Property REPORTINGMONTH As String
    ''' <summary>
    ''' [IN]締グループ(SOA締め対象の国または"JOT")
    ''' </summary>
    ''' <returns></returns>
    Public Property CLOINGGROUP As String

    Const TBL_BRI As String = "GBT0001_BR_INFO"
    Const TBL_BR As String = "GBT0002_BR_BASE"
    Const TBL_BRV As String = "GBT0003_BR_VALUE"
    Const TBL_ODB As String = "GBT0004_ODR_BASE"
    Const TBL_ODV As String = "GBT0005_ODR_VALUE"
    Const TBL_ODV2 As String = "GBT0007_ODR_VALUE2"
    Const TBL_USER As String = "COS0005_USER"
    Const TBL_COUNTRY As String = "GBM0001_COUNTRY"
    Const TBL_PORT As String = "GBM0002_PORT"
    Const TBL_DEPO As String = "GBM0003_DEPOT"
    Const TBL_CUSTOMER As String = "GBM0004_CUSTOMER"
    Const TBL_VENDER As String = "GBM0005_TRADER"
    Const TBL_TANK As String = "GBM0006_TANK"
    Const TBL_PRODUCT As String = "GBM0008_PRODUCT"
    Const TBL_CHARGECODE As String = "GBM0010_CHARGECODE"
    Const TBL_FIXVALUE As String = "COS0017_FIXVALUE"
    Const TBL_MAST As String = "COS0023_MAILSETTING"
    Const TBL_HIST As String = "COT0003_MAILSENDHIST"
    Const TBL_A_MAST As String = "COS0022_APPROVAL"
    Const TBL_A_HIST As String = "COT0002_APPROVALHIST"
    ''' <summary>
    ''' リース契約書テーブル
    ''' </summary>
    Const TBL_L_CTR As String = "GBT0010_LBR_CONTRACT"
    ''' <summary>
    ''' リース協定書テーブル
    ''' </summary>
    Const TBL_L_AGR As String = "GBT0011_LBR_AGREEMENT"
    ''' <summary>
    ''' SOA締月管理テーブル
    ''' </summary>
    Const TBL_CLOSED As String = "GBT0006_CLOSINGDAY"

    Const JOTORG As String = "JO010000"

    ''' <summary>
    ''' <para>メール送信設定</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00009setMailToBR()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim COA0033Mail As New COA0033Mail
        Dim retValue As String = ""

        Try

            '置き換え文字列
            Dim paraTable As New DataTable
            Dim baseTable As New DataTable

            If IsNothing(LASTSTEP) Then
                LASTSTEP = ""
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendLine("   rtrim(BR.TERMTYPE) as P_BR_TERMTYPEC, rtrim(FV.VALUE2) as P_BR_TERM, ")
            sqlStat.AppendLine("   rtrim(VJ.NAMES) as P_BR_OF_JOT, rtrim(VJ.CONTACTMAIL) as P_Addr_JOT, ")
            sqlStat.AppendLine("   rtrim(VO.NAMES) as P_BR_OF_ORG, rtrim(VO.MAIL_ORGANIZER) as P_Addr_ORG, ")
            sqlStat.AppendLine("   rtrim(VL1.NAMES) as P_BR_OF_POL1, rtrim(VL1.MAIL_POL) as P_Addr_POL1, ")
            sqlStat.AppendLine("   rtrim(VD1.NAMES) as P_BR_OF_POD1, rtrim(VD1.MAIL_POD) as P_Addr_POD1, ")
            sqlStat.AppendLine("   rtrim(isnull(VL2.NAMES,'-')) as P_BR_OF_POL2, rtrim(isnull(VL2.MAIL_POL,'')) as P_Addr_POL2, ")
            sqlStat.AppendLine("   rtrim(isnull(VD2.NAMES,'-')) as P_BR_OF_POD2, rtrim(isnull(VD2.MAIL_POD,'')) as P_Addr_POD2, ")
            sqlStat.AppendLine("   rtrim(BR.LOADPORT1) as P_BR_PO_POL1C, rtrim(POL1.AREANAME) as P_BR_PO_POL1, ")
            sqlStat.AppendLine("   rtrim(BR.DISCHARGEPORT1) as P_BR_PO_POD1C, rtrim(POD1.AREANAME) as P_BR_PO_POD1, ")
            sqlStat.AppendLine("   rtrim(isnull(BR.LOADPORT2,'-')) as P_BR_PO_POL2C, rtrim(isnull(POL2.AREANAME,'-')) as P_BR_PO_POL2, ")
            sqlStat.AppendLine("   rtrim(isnull(BR.DISCHARGEPORT2,'-')) as P_BR_PO_POD2C, rtrim(isnull(POD2.AREANAME,'-')) as P_BR_PO_POD2, ")
            sqlStat.AppendLine("   rtrim(BRIO.REMARK) as P_BR_SPI_ORG, ")
            sqlStat.AppendLine("   rtrim(UPPER(LEFT(BRIO.BRTYPE,1)) + LOWER(SUBSTRING(BRIO.BRTYPE,2,len(BRIO.BRTYPE)-1))) as P_BR_BRTYPE, ")
            sqlStat.AppendLine("   rtrim(BRIL1.REMARK) as P_BR_SPI_POL1, ")
            sqlStat.AppendLine("   rtrim(BRID1.REMARK) as P_BR_SPI_POD1, ")
            sqlStat.AppendLine("   rtrim(isnull(BRIL2.REMARK,'-')) as P_BR_SPI_POL2, ")
            sqlStat.AppendLine("   rtrim(isnull(BRID2.REMARK,'-')) as P_BR_SPI_POD2, ")
            sqlStat.AppendLine("   rtrim(BR.APPLYTEXT) as P_BR_APPLYTEXT, ")
            sqlStat.AppendLine("   rtrim(AH.APPLICANTID) As P_Addr_APPLICANT, rtrim(AH.APPROVEDTEXT) As P_BR_APPROVEDTEXT, ")
            sqlStat.AppendLine("   rtrim(BR.SHIPPER) as P_BR_SHIPPERC, ")
            sqlStat.AppendLine("   rtrim(isnull(CU.NAMESEN, CUVD.NAMEL)) as P_BR_SHIPPER, ")
            sqlStat.AppendLine("   rtrim(BR.PRODUCTCODE) as P_BR_PRODUCTC, rtrim(PU.PRODUCTNAME) as P_BR_PRODUCT ")
            sqlStat.AppendFormat(" FROM {0} BR ", TBL_BR).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VJ ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VJ.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND VJ.MORG = '{0}' ", JOTORG).AppendLine()
            sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VJ.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VO ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VO.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VO.CARRIERCODE = BR.AGENTORGANIZER ")
            sqlStat.AppendLine("       AND VO.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VO.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VL1 ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VL1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VL1.CARRIERCODE = BR.AGENTPOL1 ")
            sqlStat.AppendLine("       AND VL1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VL1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VL1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VL1.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} VD1 ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VD1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VD1.CARRIERCODE = BR.AGENTPOD1 ")
            sqlStat.AppendLine("       AND VD1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VD1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VD1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VD1.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} VL2 ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VL2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VL2.CARRIERCODE = BR.AGENTPOL2 ")
            sqlStat.AppendLine("       AND VL2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VL2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VL2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VL2.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} VD2 ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VD2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VD2.CARRIERCODE = BR.AGENTPOD2 ")
            sqlStat.AppendLine("       AND VD2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VD2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VD2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VD2.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} POL1 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POL1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POL1.COUNTRYCODE = BR.LOADCOUNTRY1 ")
            sqlStat.AppendLine("       AND POL1.PORTCODE = BR.LOADPORT1 ")
            sqlStat.AppendLine("       AND POL1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POL1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POL1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POD1 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POD1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POD1.COUNTRYCODE = BR.DISCHARGECOUNTRY1 ")
            sqlStat.AppendLine("       AND POD1.PORTCODE = BR.DISCHARGEPORT1 ")
            sqlStat.AppendLine("       AND POD1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POD1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POD1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POL2 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POL2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POL2.COUNTRYCODE = BR.LOADCOUNTRY2 ")
            sqlStat.AppendLine("       AND POL2.PORTCODE = BR.LOADPORT2 ")
            sqlStat.AppendLine("       AND POL2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POL2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POL2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POD2 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POD2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POD2.COUNTRYCODE = BR.DISCHARGECOUNTRY2 ")
            sqlStat.AppendLine("       AND POD2.PORTCODE = BR.DISCHARGEPORT2 ")
            sqlStat.AppendLine("       AND POD2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POD2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POD2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} BRIO ", TBL_BRI).AppendLine()
            sqlStat.AppendLine("     ON  BRIO.BRID = @BRID ")
            sqlStat.AppendLine("       AND BRIO.SUBID = @BRSUBID ")
            sqlStat.AppendLine("       AND BRIO.TYPE = 'INFO' ")
            sqlStat.AppendLine("       AND BRIO.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND BRIO.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND BRIO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} BRIL1 ", TBL_BRI).AppendLine()
            sqlStat.AppendLine("     ON  BRIL1.BRID = @BRID ")
            sqlStat.AppendLine("       AND BRIL1.SUBID = @BRSUBID ")
            sqlStat.AppendLine("       AND BRIL1.TYPE = 'POL1' ")
            sqlStat.AppendLine("       AND BRIL1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND BRIL1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND BRIL1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} BRID1 ", TBL_BRI).AppendLine()
            sqlStat.AppendLine("     ON  BRID1.BRID = @BRID ")
            sqlStat.AppendLine("       AND BRID1.SUBID = @BRSUBID ")
            sqlStat.AppendLine("       AND BRID1.TYPE = 'POD1' ")
            sqlStat.AppendLine("       AND BRID1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND BRID1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND BRID1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} BRIL2 ", TBL_BRI).AppendLine()
            sqlStat.AppendLine("     ON  BRIL2.BRID = @BRID ")
            sqlStat.AppendLine("       AND BRIL2.SUBID = @BRSUBID ")
            sqlStat.AppendLine("       AND BRIL2.TYPE = 'POL2' ")
            sqlStat.AppendLine("       AND BRIL2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND BRIL2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND BRIL2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} BRID2 ", TBL_BRI).AppendLine()
            sqlStat.AppendLine("     ON  BRID2.BRID = @BRID ")
            sqlStat.AppendLine("       AND BRID2.SUBID = @BRSUBID ")
            sqlStat.AppendLine("       AND BRID2.TYPE = 'POD2' ")
            sqlStat.AppendLine("       AND BRID2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND BRID2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND BRID2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} CU ", TBL_CUSTOMER).AppendLine()
            sqlStat.AppendFormat("     ON  CU.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND CU.COUNTRYCODE = BR.LOADCOUNTRY1 ")
            sqlStat.AppendLine("       AND CU.CUSTOMERCODE = BR.SHIPPER ")
            sqlStat.AppendLine("       AND CU.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND CU.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND CU.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} CUVD ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  CUVD.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND CUVD.CARRIERCODE = BR.SHIPPER ")
            sqlStat.AppendLine("       AND CUVD.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND CUVD.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND CUVD.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND CUVD.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} PU ", TBL_PRODUCT).AppendLine()
            sqlStat.AppendLine("       ON  PU.PRODUCTCODE = BR.PRODUCTCODE ")
            sqlStat.AppendLine("       AND PU.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND PU.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND PU.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} FV ", TBL_FIXVALUE).AppendLine()
            sqlStat.AppendLine("       ON  FV.COMPCODE = '" & GBC_COMPCODE_D & "' ")
            sqlStat.AppendFormat("     AND FV.SYSCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            sqlStat.AppendLine("       AND FV.CLASS = 'TERM' ")
            sqlStat.AppendLine("       AND FV.KEYCODE = BR.TERMTYPE ")
            sqlStat.AppendLine("       AND FV.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND FV.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND FV.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} AH ", TBL_A_HIST).AppendLine()
            sqlStat.AppendFormat("     ON  AH.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND AH.APPLYID = '{0}' ", APPLYID).AppendLine()
            sqlStat.AppendFormat("     AND AH.STEP = '{0}' ", LASTSTEP).AppendLine()
            sqlStat.AppendLine("       AND AH.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendLine("   WHERE BR.BRID   = @BRID ")
            sqlStat.AppendLine("   AND   BR.BRBASEID  = @BRBASEID ")
            sqlStat.AppendLine("   AND   BR.DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@BRID", System.Data.SqlDbType.NVarChar).Value = BRID
                    .Add("@BRSUBID", System.Data.SqlDbType.NVarChar).Value = BRSUBID
                    .Add("@BRBASEID", System.Data.SqlDbType.NVarChar).Value = BRBASEID
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda As New SqlDataAdapter(sqlCmd)
                    sqlda.Fill(paraTable)
                End Using

            End Using

            'メール設定取得
            'SQL文の作成
            Dim sqlStat2 As New System.Text.StringBuilder
            sqlStat2.AppendLine("   SELECT ")
            sqlStat2.AppendLine("     COMPCODE, SYSTEMCODE, EVENTCODE, SUBCODE, ")
            sqlStat2.AppendLine("     TOADDRESS, CC, BCC, REPLYTO, SUBJECT, BODY ")
            sqlStat2.AppendFormat(" FROM {0} BR ", TBL_MAST).AppendLine()
            sqlStat2.AppendFormat(" WHERE COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat2.AppendFormat(" AND   SYSTEMCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            sqlStat2.AppendLine("   AND   EVENTCODE  = @EVENTCODE ")
            sqlStat2.AppendLine("   AND   SUBCODE  = rtrim(@SUBCODE) ")
            sqlStat2.AppendLine("   AND   STYMD <= @NOWDATE ")
            sqlStat2.AppendLine("   AND   ENDYMD >= @NOWDATE ")
            sqlStat2.AppendLine("   AND   DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn2 As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd2 As New SqlCommand(sqlStat2.ToString, sqlConn2)
                sqlConn2.Open()
                With sqlCmd2.Parameters
                    .Add("@EVENTCODE", System.Data.SqlDbType.NVarChar).Value = EVENTCODE
                    .Add("@SUBCODE", System.Data.SqlDbType.NVarChar).Value = MAILSUBCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda2 As New SqlDataAdapter(sqlCmd2)
                    sqlda2.Fill(baseTable)
                End Using

            End Using

            ' 宛先関連(TOADDRESS, CC, BCC, REPLYTO)
            Dim keyAddress As String() = New String() {"TOADDRESS", "CC", "BCC", "REPLYTO"}
            Dim dicAddress As New Dictionary(Of String, String)(keyAddress.Length)

            Dim workAddress As String
            For Each key In keyAddress

                workAddress = baseTable.Rows(0).Item(key).ToString
                workAddress = workAddress.Replace("P_Addr_JOT", paraTable.Rows(0).Item("P_Addr_JOT").ToString)
                workAddress = workAddress.Replace("P_Addr_ORG", paraTable.Rows(0).Item("P_Addr_ORG").ToString)
                workAddress = workAddress.Replace("P_BR_BRTYPE", paraTable.Rows(0).Item("P_BR_BRTYPE").ToString)
                workAddress = workAddress.Replace("P_Addr_POL1", paraTable.Rows(0).Item("P_Addr_POL1").ToString)
                workAddress = workAddress.Replace("P_Addr_POD1", paraTable.Rows(0).Item("P_Addr_POD1").ToString)
                workAddress = workAddress.Replace("P_Addr_POL2", paraTable.Rows(0).Item("P_Addr_POL2").ToString)
                workAddress = workAddress.Replace("P_Addr_POD2", paraTable.Rows(0).Item("P_Addr_POD2").ToString)
                If BRROUND <> "2" Then
                    workAddress = workAddress.Replace("P_Addr_POLx", paraTable.Rows(0).Item("P_Addr_POL1").ToString)
                    workAddress = workAddress.Replace("P_Addr_PODx", paraTable.Rows(0).Item("P_Addr_POD1").ToString)
                Else
                    workAddress = workAddress.Replace("P_Addr_POLx", paraTable.Rows(0).Item("P_Addr_POL2").ToString)
                    workAddress = workAddress.Replace("P_Addr_PODx", paraTable.Rows(0).Item("P_Addr_POD2").ToString)
                End If

                dicAddress(key) = workAddress

            Next

            ' 件名、本文関連(SUBJECT, BODY)
            Dim keyText As String() = New String() {"SUBJECT", "BODY"}
            Dim dicText As New Dictionary(Of String, String)(keyText.Length)

            Dim workText As String
            For Each keyT In keyText

                workText = baseTable.Rows(0).Item(keyT).ToString
                workText = workText.Replace("P_BR_ID", BRID)
                workText = workText.Replace("P_BR_TERM", paraTable.Rows(0).Item("P_BR_TERM").ToString)
                workText = workText.Replace("P_BR_BRTYPE", paraTable.Rows(0).Item("P_BR_BRTYPE").ToString)
                workText = workText.Replace("P_BR_OF_JOT", paraTable.Rows(0).Item("P_BR_OF_JOT").ToString)
                workText = workText.Replace("P_BR_OF_ORG", paraTable.Rows(0).Item("P_BR_OF_ORG").ToString)
                workText = workText.Replace("P_BR_OF_POL1", paraTable.Rows(0).Item("P_BR_OF_POL1").ToString)
                workText = workText.Replace("P_BR_OF_POD1", paraTable.Rows(0).Item("P_BR_OF_POD1").ToString)
                workText = workText.Replace("P_BR_OF_POL2", paraTable.Rows(0).Item("P_BR_OF_POL2").ToString)
                workText = workText.Replace("P_BR_OF_POD2", paraTable.Rows(0).Item("P_BR_OF_POD2").ToString)
                If BRROUND <> "2" Then
                    workText = workText.Replace("P_BR_OF_POLx", paraTable.Rows(0).Item("P_BR_OF_POL1").ToString)
                    workText = workText.Replace("P_BR_OF_PODx", paraTable.Rows(0).Item("P_BR_OF_POD1").ToString)
                Else
                    workText = workText.Replace("P_BR_OF_POLx", paraTable.Rows(0).Item("P_BR_OF_POL2").ToString)
                    workText = workText.Replace("P_BR_OF_PODx", paraTable.Rows(0).Item("P_BR_OF_POD2").ToString)
                End If
                workText = workText.Replace("P_BR_PO_POL1", paraTable.Rows(0).Item("P_BR_PO_POL1").ToString)
                workText = workText.Replace("P_BR_PO_POD1", paraTable.Rows(0).Item("P_BR_PO_POD1").ToString)
                workText = workText.Replace("P_BR_PO_POL2", paraTable.Rows(0).Item("P_BR_PO_POL2").ToString)
                workText = workText.Replace("P_BR_PO_POD2", paraTable.Rows(0).Item("P_BR_PO_POD2").ToString)
                workText = workText.Replace("P_BR_SHIPPER", paraTable.Rows(0).Item("P_BR_SHIPPER").ToString)
                workText = workText.Replace("P_BR_PRODUCT", paraTable.Rows(0).Item("P_BR_PRODUCT").ToString)

                workText = workText.Replace("P_BR_SPI_ORG", paraTable.Rows(0).Item("P_BR_SPI_ORG").ToString)
                workText = workText.Replace("P_BR_SPI_POL1", paraTable.Rows(0).Item("P_BR_SPI_POL1").ToString)
                workText = workText.Replace("P_BR_SPI_POD1", paraTable.Rows(0).Item("P_BR_SPI_POD1").ToString)
                workText = workText.Replace("P_BR_SPI_POL2", paraTable.Rows(0).Item("P_BR_SPI_POL2").ToString)
                workText = workText.Replace("P_BR_SPI_POD2", paraTable.Rows(0).Item("P_BR_SPI_POD2").ToString)
                If BRROUND <> "2" Then
                    workText = workText.Replace("P_BR_SPI_POLx", paraTable.Rows(0).Item("P_BR_SPI_POL1").ToString)
                    workText = workText.Replace("P_BR_SPI_PODx", paraTable.Rows(0).Item("P_BR_SPI_POD1").ToString)
                Else
                    workText = workText.Replace("P_BR_SPI_POLx", paraTable.Rows(0).Item("P_BR_SPI_POL2").ToString)
                    workText = workText.Replace("P_BR_SPI_PODx", paraTable.Rows(0).Item("P_BR_SPI_POD2").ToString)
                End If
                workText = workText.Replace("P_BR_APPLYTEXT", paraTable.Rows(0).Item("P_BR_APPLYTEXT").ToString)
                workText = workText.Replace("P_BR_APPROVEDTEXT", paraTable.Rows(0).Item("P_BR_APPROVEDTEXT").ToString)
                workText = workText.Replace("P_BR_OF_USER", COA0019Session.USERNAME)

                dicText(keyT) = workText

            Next

            'メール設定
            COA0033Mail.I_COMPCODE = GBC_COMPCODE
            COA0033Mail.I_SYSCODE = COA0019Session.SYSCODE
            COA0033Mail.I_EVENTCODE = EVENTCODE
            COA0033Mail.I_SUBCODE = MAILSUBCODE
            COA0033Mail.I_ID = BRID
            COA0033Mail.I_TOADDRESS = dicAddress("TOADDRESS")
            COA0033Mail.I_CC = dicAddress("CC")
            COA0033Mail.I_BCC = dicAddress("BCC")
            COA0033Mail.I_REPLYTO = dicAddress("REPLYTO")
            COA0033Mail.I_SUBJECT = dicText("SUBJECT")
            COA0033Mail.I_BODY = dicText("BODY")
            COA0033Mail.COA0033setMailSend()

            Me.ERR = C_MESSAGENO.NORMAL

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>メール送信設定</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00009setMailToRepBR()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim COA0033Mail As New COA0033Mail
        Dim retValue As String = ""
        Dim costValue As String = ""
        Dim costList As String = ""
        Dim costApprove As String = ""
        Dim costTotal As Decimal = 0

        Try

            '置き換え文字列
            Dim paraTable As New DataTable
            Dim costTable As New DataTable
            Dim baseTable As New DataTable

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   Select ")
            sqlStat.AppendLine("   rtrim(BR.BRID) as P_BR_ID, rtrim(BR.TANKNO) as P_BR_TANKNO, ")
            sqlStat.AppendLine("   rtrim(BR.APPLYTEXT) as P_BR_APPLYTEXT, ")
            sqlStat.AppendLine("   rtrim(VJ.NAMES) as P_BR_OF_JOT, rtrim(VJ.CONTACTMAIL) as P_Addr_JOT, ")
            sqlStat.AppendLine("   rtrim(VO.NAMES) as P_BR_OF_ORG, rtrim(VO.MAIL_ORGANIZER) as P_Addr_ORG, ")
            sqlStat.AppendLine("   rtrim(US.STAFFNAMES_EN) As P_US_STAFFNAMES, rtrim(US.EMAIL) As P_US_EMAIL, ")
            sqlStat.AppendLine("   rtrim(AH.APPLICANTID) As P_Addr_APPLICANT, rtrim(AH.APPROVEDTEXT) As P_BR_APPROVEDTEXT ")
            sqlStat.AppendFormat(" FROM {0} BR ", TBL_BR).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VJ ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     On  VJ.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND VJ.MORG = '{0}' ", JOTORG).AppendLine()
            sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VJ.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VO ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VO.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VO.CARRIERCODE = BR.AGENTORGANIZER ")
            sqlStat.AppendLine("       AND VO.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VO.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} AH ", TBL_A_HIST).AppendLine()
            sqlStat.AppendFormat("     ON  AH.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("       AND AH.APPLYID = '{0}' ", APPLYID).AppendLine()
            sqlStat.AppendFormat("     AND AH.STEP = '{0}' ", LASTSTEP).AppendLine()
            sqlStat.AppendLine("       AND AH.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} US ", TBL_USER).AppendLine()
            sqlStat.AppendLine("       ON  US.USERID = AH.APPLICANTID ")
            sqlStat.AppendLine("       AND US.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND US.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND US.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendLine("   WHERE BR.BRID   = @BRID ")
            sqlStat.AppendLine("   AND   BR.BRBASEID  = @BRBASEID ")
            sqlStat.AppendLine("   AND   BR.DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@BRID", System.Data.SqlDbType.NVarChar).Value = BRID
                    '.Add("@BRSUBID", System.Data.SqlDbType.NVarChar).Value = BRSUBID
                    .Add("@BRBASEID", System.Data.SqlDbType.NVarChar).Value = BRBASEID
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda As New SqlDataAdapter(sqlCmd)
                    sqlda.Fill(paraTable)
                End Using

            End Using

            ' 明細情報取得
            Dim sqlStat3 As New System.Text.StringBuilder
            sqlStat3.AppendLine("   SELECT ")
            sqlStat3.AppendLine("     rtrim(BRV.COSTCODE) as COSTCODE, BRV.USD, BRV.CURRENCYCODE, ")
            sqlStat3.AppendLine("     rtrim(CC.NAMES) as COSTNAME, ")
            sqlStat3.AppendLine("     CASE WHEN BRV.REPAIRFLG = '1' THEN 'APPROVED' ELSE 'REJECTED' END STATUS")
            sqlStat3.AppendFormat(" FROM {0} BRIO", TBL_BRI).AppendLine()
            sqlStat3.AppendFormat(" INNER JOIN {0} BRV ", TBL_BRV).AppendLine()
            sqlStat3.AppendLine("       ON BRV.BRID   = BRIO.BRID ")
            sqlStat3.AppendLine("       AND   BRV.BRVALUEID  = BRIO.LINKID ")
            sqlStat3.AppendLine("       AND   BRV.DELFLG  <> '" & CONST_FLAG_YES & "' ")
            sqlStat3.AppendFormat(" INNER JOIN {0} CC ", TBL_CHARGECODE).AppendLine()
            sqlStat3.AppendFormat("      ON  CC.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat3.AppendLine("       AND CC.COSTCODE = BRV.COSTCODE ")
            sqlStat3.AppendLine("       AND ( CC.LDKBN = SUBSTRING(BRV.DTLPOLPOD,3,1) OR CC.LDKBN = 'B' ) ")
            sqlStat3.AppendLine("       AND CC.STYMD <= @NOWDATE ")
            sqlStat3.AppendLine("       AND CC.ENDYMD >= @NOWDATE ")
            sqlStat3.AppendLine("       AND CC.DELFLG  <> '" & CONST_FLAG_YES & "' ")
            sqlStat3.AppendLine("   WHERE BRIO.BRID = @BRID ")
            sqlStat3.AppendLine("       AND BRIO.SUBID = @BRSUBID ")
            sqlStat3.AppendLine("       AND BRIO.TYPE = 'POL1' ")
            sqlStat3.AppendLine("       AND BRIO.STYMD <= @NOWDATE ")
            sqlStat3.AppendLine("       AND BRIO.ENDYMD >= @NOWDATE ")
            sqlStat3.AppendLine("       AND BRIO.DELFLG <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn3 As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd3 As New SqlCommand(sqlStat3.ToString, sqlConn3)
                sqlConn3.Open()
                With sqlCmd3.Parameters
                    .Add("@BRID", System.Data.SqlDbType.NVarChar).Value = BRID
                    .Add("@BRSUBID", System.Data.SqlDbType.NVarChar).Value = BRSUBID
                    '.Add("@SUBCODE", System.Data.SqlDbType.NVarChar).Value = MAILSUBCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlDr As SqlDataReader = sqlCmd3.ExecuteReader()
                    Dim i As Long = 1
                    While sqlDr.Read
                        costValue = "Cost Item" & i & ":" & Convert.ToString(sqlDr("COSTNAME")) & " $" & Convert.ToString(sqlDr("USD"))
                        costList = costList & costValue & vbCrLf
                        costApprove = costApprove & costValue & "-" & Convert.ToString(sqlDr("STATUS")) & vbCrLf
                        costTotal = costTotal + Convert.ToDecimal(sqlDr("USD"))
                        i = i + 1
                    End While
                End Using
            End Using

            'メール設定取得
            'SQL文の作成
            Dim sqlStat2 As New System.Text.StringBuilder
            sqlStat2.AppendLine("   SELECT ")
            sqlStat2.AppendLine("     COMPCODE, SYSTEMCODE, EVENTCODE, SUBCODE, ")
            sqlStat2.AppendLine("     TOADDRESS, CC, BCC, REPLYTO, SUBJECT, BODY ")
            sqlStat2.AppendFormat(" FROM {0} ", TBL_MAST).AppendLine()
            sqlStat2.AppendFormat(" WHERE COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat2.AppendFormat(" AND   SYSTEMCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            sqlStat2.AppendLine("   AND   EVENTCODE  = @EVENTCODE ")
            sqlStat2.AppendLine("   AND   SUBCODE  = rtrim(@SUBCODE) ")
            sqlStat2.AppendLine("   AND   STYMD <= @NOWDATE ")
            sqlStat2.AppendLine("   AND   ENDYMD >= @NOWDATE ")
            sqlStat2.AppendLine("   AND   DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn2 As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd2 As New SqlCommand(sqlStat2.ToString, sqlConn2)
                sqlConn2.Open()
                With sqlCmd2.Parameters
                    .Add("@EVENTCODE", System.Data.SqlDbType.NVarChar).Value = EVENTCODE
                    .Add("@SUBCODE", System.Data.SqlDbType.NVarChar).Value = MAILSUBCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda2 As New SqlDataAdapter(sqlCmd2)
                    sqlda2.Fill(baseTable)
                End Using

            End Using

            ' パラメータ展開
            Dim workTOADDRESS As String = ""
            Dim workCC As String = ""
            Dim workBCC As String = ""
            Dim workREPLYTO As String = ""
            Dim workSUBJECT As String = ""
            Dim workBODY As String = ""

            ' 宛先関連(TOADDRESS, CC, BCC, REPLYTO)
            Dim keyAddress As String() = New String() {"TOADDRESS", "CC", "BCC", "REPLYTO"}
            Dim dicAddress As New Dictionary(Of String, String)(keyAddress.Length)

            Dim workAddress As String
            For Each key In keyAddress

                workAddress = baseTable.Rows(0).Item(key).ToString
                workAddress = workAddress.Replace("P_Addr_JOT", paraTable.Rows(0).Item("P_Addr_JOT").ToString)
                workAddress = workAddress.Replace("P_Addr_APPLICANT", paraTable.Rows(0).Item("P_US_EMAIL").ToString)

                dicAddress(key) = workAddress

            Next

            ' 件名、本文関連(SUBJECT, BODY)
            Dim keyText As String() = New String() {"SUBJECT", "BODY"}
            Dim dicText As New Dictionary(Of String, String)(keyText.Length)

            Dim workText As String
            For Each keyT In keyText

                workText = baseTable.Rows(0).Item(keyT).ToString
                workText = workText.Replace("P_BR_ID", paraTable.Rows(0).Item("P_BR_ID").ToString)
                workText = workText.Replace("P_BR_OF_ORG", paraTable.Rows(0).Item("P_BR_OF_ORG").ToString)
                workText = workText.Replace("P_BR_TANKNO", paraTable.Rows(0).Item("P_BR_TANKNO").ToString)
                workText = workText.Replace("P_BRV_COSTTOTAL", costTotal.ToString)
                workText = workText.Replace("P_BRV_COST", costList.ToString)
                workText = workText.Replace("P_BRV_APPROVEDCOST", costApprove.ToString)
                workText = workText.Replace("P_BR_TANKNO", paraTable.Rows(0).Item("P_BR_TANKNO").ToString)
                workText = workText.Replace("P_BR_APPLYTEXT", paraTable.Rows(0).Item("P_BR_APPLYTEXT").ToString)
                workText = workText.Replace("P_BR_APPROVEDTEXT", paraTable.Rows(0).Item("P_BR_APPROVEDTEXT").ToString)
                workText = workText.Replace("P_BR_USER", COA0019Session.USERNAME)

                dicText(keyT) = workText

            Next

            'メール設定
            COA0033Mail.I_COMPCODE = GBC_COMPCODE
            COA0033Mail.I_SYSCODE = COA0019Session.SYSCODE
            COA0033Mail.I_EVENTCODE = EVENTCODE
            COA0033Mail.I_SUBCODE = MAILSUBCODE
            COA0033Mail.I_ID = BRID
            COA0033Mail.I_TOADDRESS = dicAddress("TOADDRESS")
            COA0033Mail.I_CC = dicAddress("CC")
            COA0033Mail.I_BCC = dicAddress("BCC")
            COA0033Mail.I_REPLYTO = dicAddress("REPLYTO")
            COA0033Mail.I_SUBJECT = dicText("SUBJECT")
            COA0033Mail.I_BODY = dicText("BODY")
            COA0033Mail.COA0033setMailSend()

            Me.ERR = C_MESSAGENO.NORMAL

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>メール送信設定</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00009setMailToNonBR()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim COA0033Mail As New COA0033Mail
        Dim retValue As String = ""

        Try

            '置き換え文字列
            Dim paraTable As New DataTable
            Dim baseTable As New DataTable

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   Select ")
            sqlStat.AppendLine("   rtrim(ODV.ORDERNO) As P_OD_ORDERNO, rtrim(ODV.DTLOFFICE) As P_OD_OFFICECODE, ")
            sqlStat.AppendLine("   rtrim(ODV.TANKNO) As P_OD_TANKNO, rtrim(ODV.COSTCODE) As P_OD_COSTCODE, ")
            sqlStat.AppendLine("   rtrim(ODV.COUNTRYCODE) As P_OD_COUNTRYCODE, rtrim(ODV.CURRENCYCODE) As P_OD_CURRENCYCODE, ")
            sqlStat.AppendLine("   rtrim(ODV.AMOUNTORD) As P_OD_AMOUNTORD, rtrim(ODV.LOCALBR) As P_OD_LOCALBR, ")
            sqlStat.AppendLine("   rtrim(ODV.LOCALRATE) As P_OD_LOCALRATE, rtrim(ODV.APPLYTEXT) As P_OD_APPLYTEXT,")
            sqlStat.AppendLine("   rtrim(ODV.APPLYID) As P_OD_APPLYID, ")
            sqlStat.AppendLine("   RTrim(VO.NAMES) As P_OD_OFFICE, ")
            sqlStat.AppendLine("   rtrim(VJ.NAMES) As P_BR_OF_JOT, rtrim(VJ.CONTACTMAIL) As P_Addr_JOT, ")
            sqlStat.AppendLine("   rtrim(AH.APPLICANTID) As P_Addr_APPLICANT, rtrim(AH.APPROVEDTEXT) As P_AP_APPROVEDTEXT, ")
            sqlStat.AppendLine("   rtrim(US.STAFFNAMES_EN) As P_US_STAFFNAMES, rtrim(US.EMAIL) As P_US_EMAIL, ")
            sqlStat.AppendLine("   case when isnull(CC.CLASS2,'') <> '' then rtrim(isnull(VC.NAMESEN,'')) else isnull(VT.NAMES,isnull(VD.NAMES,'')) end As P_V_NAME ")
            sqlStat.AppendFormat(" FROM {0} ODV ", TBL_ODV).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VJ ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     On  VJ.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND VJ.MORG = '{0}' ", JOTORG).AppendLine()
            sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VJ.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VO ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VO.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VO.CARRIERCODE = ODV.DTLOFFICE ")
            sqlStat.AppendLine("       AND VO.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VO.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} AH ", TBL_A_HIST).AppendLine()
            sqlStat.AppendFormat("     ON  AH.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND AH.APPLYID = ODV.APPLYID ")
            sqlStat.AppendFormat("     AND AH.STEP = '{0}' ", APPLYSTEP).AppendLine()
            sqlStat.AppendLine("       AND AH.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} US ", TBL_USER).AppendLine()
            sqlStat.AppendLine("       ON  US.USERID = AH.APPLICANTID ")
            sqlStat.AppendLine("       AND US.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND US.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND US.DELFLG <> '" & CONST_FLAG_YES & "' ")

            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} VC ", TBL_CUSTOMER).AppendLine()
            sqlStat.AppendFormat("     ON  VC.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VC.CUSTOMERCODE = ODV.CONTRACTORFIX ")
            sqlStat.AppendLine("       AND VC.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VC.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VC.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} VT ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VT.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VT.CARRIERCODE = ODV.CONTRACTORFIX ")
            sqlStat.AppendLine("       AND VT.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VT.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VT.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} VD ", TBL_DEPO).AppendLine()
            sqlStat.AppendFormat("     ON  VD.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VD.DEPOTCODE = ODV.CONTRACTORFIX ")
            sqlStat.AppendLine("       AND VD.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VD.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VD.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} CC ", TBL_CHARGECODE).AppendLine()
            sqlStat.AppendFormat("      ON  CC.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND CC.COSTCODE = ODV.COSTCODE ")
            sqlStat.AppendLine("       AND CC.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND CC.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND CC.DELFLG  <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendLine("       AND CC.CLASS2  <> '' ")　'CLASS2が空でないものは収益

            sqlStat.AppendLine("   WHERE ODV.DATAID   = @ODRDATAID ")
            sqlStat.AppendLine("   AND   ODV.DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@ODRDATAID", System.Data.SqlDbType.NVarChar).Value = ODRDATAID
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda As New SqlDataAdapter(sqlCmd)
                    sqlda.Fill(paraTable)
                End Using

            End Using

            'メール設定取得
            'SQL文の作成
            Dim sqlStat2 As New System.Text.StringBuilder
            sqlStat2.AppendLine("   SELECT ")
            sqlStat2.AppendLine("     COMPCODE, SYSTEMCODE, EVENTCODE, SUBCODE, ")
            sqlStat2.AppendLine("     TOADDRESS, CC, BCC, REPLYTO, SUBJECT, BODY ")
            sqlStat2.AppendFormat(" FROM {0} ", TBL_MAST).AppendLine()
            sqlStat2.AppendFormat(" WHERE COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat2.AppendFormat(" AND   SYSTEMCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            sqlStat2.AppendLine("   AND   EVENTCODE  = @EVENTCODE ")
            sqlStat2.AppendLine("   AND   SUBCODE  = rtrim(@SUBCODE) ")
            sqlStat2.AppendLine("   AND   STYMD <= @NOWDATE ")
            sqlStat2.AppendLine("   AND   ENDYMD >= @NOWDATE ")
            sqlStat2.AppendLine("   AND   DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn2 As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd2 As New SqlCommand(sqlStat2.ToString, sqlConn2)
                sqlConn2.Open()
                With sqlCmd2.Parameters
                    .Add("@EVENTCODE", System.Data.SqlDbType.NVarChar).Value = EVENTCODE
                    .Add("@SUBCODE", System.Data.SqlDbType.NVarChar).Value = MAILSUBCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda2 As New SqlDataAdapter(sqlCmd2)
                    sqlda2.Fill(baseTable)
                End Using

            End Using

            ' パラメータ展開
            Dim workTOADDRESS As String = ""
            Dim workCC As String = ""
            Dim workBCC As String = ""
            Dim workREPLYTO As String = ""
            Dim workSUBJECT As String = ""
            Dim workBODY As String = ""

            ' 宛先関連(TOADDRESS, CC, BCC, REPLYTO)
            Dim keyAddress As String() = New String() {"TOADDRESS", "CC", "BCC", "REPLYTO"}
            Dim dicAddress As New Dictionary(Of String, String)(keyAddress.Length)

            Dim workAddress As String
            For Each key In keyAddress

                workAddress = baseTable.Rows(0).Item(key).ToString
                workAddress = workAddress.Replace("P_Addr_JOT", paraTable.Rows(0).Item("P_Addr_JOT").ToString)
                workAddress = workAddress.Replace("P_Addr_APPLICANT", paraTable.Rows(0).Item("P_US_EMAIL").ToString)

                dicAddress(key) = workAddress

            Next

            ' 件名、本文関連(SUBJECT, BODY)
            Dim keyText As String() = New String() {"SUBJECT", "BODY"}
            Dim dicText As New Dictionary(Of String, String)(keyText.Length)

            Dim workText As String
            For Each keyT In keyText

                workText = baseTable.Rows(0).Item(keyT).ToString
                workText = workText.Replace("P_AP_ID", paraTable.Rows(0).Item("P_OD_APPLYID").ToString)
                workText = workText.Replace("P_US_STAFFNAMES", paraTable.Rows(0).Item("P_US_STAFFNAMES").ToString)
                workText = workText.Replace("P_OD_ORDERNO", paraTable.Rows(0).Item("P_OD_ORDERNO").ToString)
                workText = workText.Replace("P_OD_OFFICE", paraTable.Rows(0).Item("P_OD_OFFICE").ToString)
                workText = workText.Replace("P_OD_TANKNO", paraTable.Rows(0).Item("P_OD_TANKNO").ToString)
                If paraTable.Rows(0).Item("P_OD_CURRENCYCODE").ToString <> GBC_CUR_USD Then
                    workText = workText.Replace("P_OD_AMOUNT", paraTable.Rows(0).Item("P_OD_AMOUNTORD").ToString)
                    workText = workText.Replace("P_OD_CURRENCY", paraTable.Rows(0).Item("P_OD_CURRENCYCODE").ToString)
                    workText = workText.Replace("P_OD_LOCALBR", paraTable.Rows(0).Item("P_OD_LOCALBR").ToString)
                Else
                    workText = workText.Replace("P_OD_AMOUNT", paraTable.Rows(0).Item("P_OD_AMOUNTORD").ToString)
                    'workText = workText.Replace("P_OD_CURRENCYCODE", GBC_CUR_USD)
                    workText = workText.Replace("P_OD_CURRENCY", GBC_CUR_USD)
                End If
                workText = workText.Replace("P_OD_APPLYTEXT", paraTable.Rows(0).Item("P_OD_APPLYTEXT").ToString)
                workText = workText.Replace("P_AP_APPROVEDTEXT", paraTable.Rows(0).Item("P_AP_APPROVEDTEXT").ToString)
                workText = workText.Replace("P_OD_USER", COA0019Session.USERNAME)
                workText = workText.Replace("P_V_NAME", paraTable.Rows(0).Item("P_V_NAME").ToString)

                dicText(keyT) = workText

            Next

            'メール設定
            COA0033Mail.I_COMPCODE = GBC_COMPCODE
            COA0033Mail.I_SYSCODE = COA0019Session.SYSCODE
            COA0033Mail.I_EVENTCODE = EVENTCODE
            COA0033Mail.I_SUBCODE = MAILSUBCODE
            COA0033Mail.I_ID = paraTable.Rows(0).Item("P_OD_ORDERNO").ToString
            COA0033Mail.I_TOADDRESS = dicAddress("TOADDRESS")
            COA0033Mail.I_CC = dicAddress("CC")
            COA0033Mail.I_BCC = dicAddress("BCC")
            COA0033Mail.I_REPLYTO = dicAddress("REPLYTO")
            COA0033Mail.I_SUBJECT = dicText("SUBJECT")
            COA0033Mail.I_BODY = dicText("BODY")
            COA0033Mail.COA0033setMailSend()

            Me.ERR = C_MESSAGENO.NORMAL

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>メール送信設定</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00009setMailToOdr()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim COA0033Mail As New COA0033Mail
        Dim retValue As String = ""

        Try

            '置き換え文字列
            Dim paraTable As New DataTable
            Dim baseTable As New DataTable

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendLine("   rtrim(ODV.ORDERNO) as P_OD_ORDERNO, rtrim(ODV.DTLOFFICE) as P_OD_OFFICECODE, ")
            sqlStat.AppendLine("   rtrim(ODV.TANKNO) as P_OD_TANKNO, rtrim(ODV.COSTCODE) as P_OD_COSTCODE, ")
            sqlStat.AppendLine("   rtrim(ODV.COUNTRYCODE) as P_OD_COUNTRYCODE, rtrim(ODV.CURRENCYCODE) as P_OD_CURRENCYCODE, ")
            sqlStat.AppendLine("   rtrim(ODV.AMOUNTBR) as P_OD_AMOUNTBR, rtrim(ODV.AMOUNTORD) as P_OD_AMOUNTORD, ")
            sqlStat.AppendLine("   rtrim(ODV.AMOUNTFIX) as P_OD_AMOUNTFIX, rtrim(ODV.LOCALBR) as P_OD_LOCALBR, ")
            sqlStat.AppendLine("   rtrim(ODV.LOCALRATE) as P_OD_LOCALRATE, rtrim(ODV.APPLYTEXT) as P_OD_APPLYTEXT,")
            sqlStat.AppendLine("   rtrim(ODV.APPLYID) as P_OD_APPLYID, rtrim(ODV.BRID) as P_OD_BRID,")
            sqlStat.AppendLine("   case rtrim(ODV.DTLPOLPOD) ")
            sqlStat.AppendLine("     when 'POL1' then rtrim(POL1.AREANAME) ")
            sqlStat.AppendLine("     when 'POD1' then rtrim(POL1.AREANAME) ")
            sqlStat.AppendLine("     when 'POL2' then rtrim(isnull(POL2.AREANAME,'')) ")
            sqlStat.AppendLine("     when 'POD2' then rtrim(isnull(POL2.AREANAME,'')) ")
            sqlStat.AppendLine("   end as P_OD_PO_POL, ")
            sqlStat.AppendLine("   rtrim(ODB.SHIPPER) as P_OD_SHIPPERC, rtrim(CUS.NAMESEN) as P_OD_SHIPPER, ")
            sqlStat.AppendLine("   case rtrim(ODV.DTLPOLPOD) ")
            sqlStat.AppendLine("     when 'POL1' then rtrim(POD1.AREANAME) ")
            sqlStat.AppendLine("     when 'POD1' then rtrim(POD1.AREANAME) ")
            sqlStat.AppendLine("     when 'POL2' then rtrim(isnull(POD2.AREANAME,'')) ")
            sqlStat.AppendLine("     when 'POD2' then rtrim(isnull(POD2.AREANAME,'')) ")
            sqlStat.AppendLine("   end as P_OD_PO_POD, ")
            sqlStat.AppendLine("   rtrim(ODB.CONSIGNEE) as P_OD_CONSIGNEEC, rtrim(isnull(CUC.NAMESEN,'')) as P_OD_CONSIGNEE, ")
            sqlStat.AppendLine("   rtrim(VJ.NAMES) as P_BR_OF_JOT, rtrim(VJ.CONTACTMAIL) as P_Addr_JOT, ")
            sqlStat.AppendLine("   RTrim(VO.NAMES) As P_OD_OFFICE, ")
            sqlStat.AppendLine("   rtrim(AH.APPLICANTID) as P_Addr_APPLICANT, rtrim(AH.APPROVEDTEXT) as P_AP_APPROVEDTEXT, ")
            sqlStat.AppendLine("   rtrim(US.STAFFNAMES_EN) as P_US_STAFFNAMES, rtrim(US.EMAIL) as P_US_EMAIL ")
            sqlStat.AppendFormat(" FROM {0} ODV ", TBL_ODV).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VJ ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VJ.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND VJ.MORG = '{0}' ", JOTORG).AppendLine()
            sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VJ.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VO ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VO.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VO.CARRIERCODE = ODV.DTLOFFICE ")
            sqlStat.AppendLine("       AND VO.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VO.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()

            sqlStat.AppendFormat(" INNER JOIN {0} ODB ", TBL_ODB).AppendLine()
            sqlStat.AppendLine("       ON  ODB.ORDERNO = ODV.ORDERNO ")
            sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")

            sqlStat.AppendFormat(" INNER JOIN {0} AH ", TBL_A_HIST).AppendLine()
            sqlStat.AppendFormat("     ON  AH.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND AH.APPLYID = ODV.APPLYID ")
            sqlStat.AppendFormat("     AND AH.STEP = '{0}' ", APPLYSTEP).AppendLine()
            sqlStat.AppendLine("       AND AH.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} US ", TBL_USER).AppendLine()
            sqlStat.AppendLine("       ON  US.USERID = AH.APPLICANTID ")
            sqlStat.AppendLine("       AND US.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND US.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND US.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POL1 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POL1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POL1.COUNTRYCODE = ODB.LOADCOUNTRY1 ")
            sqlStat.AppendLine("       AND POL1.PORTCODE = ODB.LOADPORT1 ")
            sqlStat.AppendLine("       AND POL1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POL1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POL1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POD1 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POD1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POD1.COUNTRYCODE = ODB.DISCHARGECOUNTRY1 ")
            sqlStat.AppendLine("       AND POD1.PORTCODE = ODB.DISCHARGEPORT1 ")
            sqlStat.AppendLine("       AND POD1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POD1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POD1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POL2 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POL2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POL2.COUNTRYCODE = ODB.LOADCOUNTRY2 ")
            sqlStat.AppendLine("       AND POL2.PORTCODE = ODB.LOADPORT2 ")
            sqlStat.AppendLine("       AND POL2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POL2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POL2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POD2 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POD2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POD2.COUNTRYCODE = ODB.DISCHARGECOUNTRY2 ")
            sqlStat.AppendLine("       AND POD2.PORTCODE = ODB.DISCHARGEPORT2 ")
            sqlStat.AppendLine("       AND POD2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POD2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POD2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" INNER JOIN {0} CUS ", TBL_CUSTOMER).AppendLine()
            sqlStat.AppendFormat("  LEFT OUTER JOIN {0} CUS ", TBL_CUSTOMER).AppendLine()
            sqlStat.AppendFormat("     ON  CUS.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND CUS.COUNTRYCODE = ODB.LOADCOUNTRY1 ")
            sqlStat.AppendLine("       AND CUS.CUSTOMERCODE = ODB.SHIPPER ")
            sqlStat.AppendLine("       AND CUS.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND CUS.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND CUS.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} CUC ", TBL_CUSTOMER).AppendLine()
            sqlStat.AppendFormat("     ON  CUC.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND CUC.COUNTRYCODE = ODB.DISCHARGECOUNTRY1 ")
            sqlStat.AppendLine("       AND CUC.CUSTOMERCODE = ODB.CONSIGNEE ")
            sqlStat.AppendLine("       AND CUC.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND CUC.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND CUC.DELFLG <> '" & CONST_FLAG_YES & "' ")

            sqlStat.AppendLine("   WHERE ODV.DATAID   = @ODRDATAID ")
            sqlStat.AppendLine("   AND   ODV.DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@ODRDATAID", System.Data.SqlDbType.NVarChar).Value = ODRDATAID
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda As New SqlDataAdapter(sqlCmd)
                    sqlda.Fill(paraTable)
                End Using

            End Using

            'メール設定取得
            'SQL文の作成
            Dim sqlStat2 As New System.Text.StringBuilder
            sqlStat2.AppendLine("   SELECT ")
            sqlStat2.AppendLine("     COMPCODE, SYSTEMCODE, EVENTCODE, SUBCODE, ")
            sqlStat2.AppendLine("     TOADDRESS, CC, BCC, REPLYTO, SUBJECT, BODY ")
            sqlStat2.AppendFormat(" FROM {0} ", TBL_MAST).AppendLine()
            sqlStat2.AppendFormat(" WHERE COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat2.AppendFormat(" AND   SYSTEMCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            sqlStat2.AppendLine("   AND   EVENTCODE  = @EVENTCODE ")
            sqlStat2.AppendLine("   AND   SUBCODE  = rtrim(@SUBCODE) ")
            sqlStat2.AppendLine("   AND   STYMD <= @NOWDATE ")
            sqlStat2.AppendLine("   AND   ENDYMD >= @NOWDATE ")
            sqlStat2.AppendLine("   AND   DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn2 As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd2 As New SqlCommand(sqlStat2.ToString, sqlConn2)
                sqlConn2.Open()
                With sqlCmd2.Parameters
                    .Add("@EVENTCODE", System.Data.SqlDbType.NVarChar).Value = EVENTCODE
                    .Add("@SUBCODE", System.Data.SqlDbType.NVarChar).Value = MAILSUBCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda2 As New SqlDataAdapter(sqlCmd2)
                    sqlda2.Fill(baseTable)
                End Using

            End Using

            ' パラメータ展開
            Dim workTOADDRESS As String = ""
            Dim workCC As String = ""
            Dim workBCC As String = ""
            Dim workREPLYTO As String = ""
            Dim workSUBJECT As String = ""
            Dim workBODY As String = ""

            ' 宛先関連(TOADDRESS, CC, BCC, REPLYTO)
            Dim keyAddress As String() = New String() {"TOADDRESS", "CC", "BCC", "REPLYTO"}
            Dim dicAddress As New Dictionary(Of String, String)(keyAddress.Length)

            Dim workAddress As String
            For Each key In keyAddress

                workAddress = baseTable.Rows(0).Item(key).ToString
                workAddress = workAddress.Replace("P_Addr_JOT", paraTable.Rows(0).Item("P_Addr_JOT").ToString)
                workAddress = workAddress.Replace("P_Addr_APPLICANT", paraTable.Rows(0).Item("P_US_EMAIL").ToString)

                dicAddress(key) = workAddress

            Next

            ' 件名、本文関連(SUBJECT, BODY)
            Dim keyText As String() = New String() {"SUBJECT", "BODY"}
            Dim dicText As New Dictionary(Of String, String)(keyText.Length)

            Dim workText As String
            For Each keyT In keyText

                workText = baseTable.Rows(0).Item(keyT).ToString
                workText = workText.Replace("P_AP_ID", paraTable.Rows(0).Item("P_OD_APPLYID").ToString)
                workText = workText.Replace("P_US_STAFFNAMES", paraTable.Rows(0).Item("P_US_STAFFNAMES").ToString)
                workText = workText.Replace("P_OD_ORDERNO", paraTable.Rows(0).Item("P_OD_ORDERNO").ToString)
                workText = workText.Replace("P_OD_TANKNO", paraTable.Rows(0).Item("P_OD_TANKNO").ToString)
                workText = workText.Replace("P_OD_BRID", paraTable.Rows(0).Item("P_OD_BRID").ToString)

                workText = workText.Replace("P_OD_PO_POL", paraTable.Rows(0).Item("P_OD_PO_POL").ToString)
                workText = workText.Replace("P_OD_PO_POD", paraTable.Rows(0).Item("P_OD_PO_POD").ToString)
                workText = workText.Replace("P_OD_SHIPPER", paraTable.Rows(0).Item("P_OD_SHIPPER").ToString)
                workText = workText.Replace("P_OD_CONSIGNEE", paraTable.Rows(0).Item("P_OD_CONSIGNEE").ToString)

                workText = workText.Replace("P_OD_CURRENCY", paraTable.Rows(0).Item("P_OD_CURRENCYCODE").ToString)
                workText = workText.Replace("P_OD_AMOUNTBR", paraTable.Rows(0).Item("P_OD_AMOUNTBR").ToString)
                workText = workText.Replace("P_OD_AMOUNTODR", paraTable.Rows(0).Item("P_OD_AMOUNTORD").ToString)
                workText = workText.Replace("P_OD_AMOUNTFIX", paraTable.Rows(0).Item("P_OD_AMOUNTFIX").ToString)

                workText = workText.Replace("P_OD_APPLYTEXT", paraTable.Rows(0).Item("P_OD_APPLYTEXT").ToString)
                workText = workText.Replace("P_AP_APPROVEDTEXT", paraTable.Rows(0).Item("P_AP_APPROVEDTEXT").ToString)

                workText = workText.Replace("P_AP_OF", paraTable.Rows(0).Item("P_OD_OFFICE").ToString)
                workText = workText.Replace("P_BR_ID", paraTable.Rows(0).Item("P_OD_BRID").ToString)
                workText = workText.Replace("P_AP_TANK", paraTable.Rows(0).Item("P_OD_TANKNO").ToString)
                workText = workText.Replace("P_AP_USER", COA0019Session.USERNAME)

                dicText(keyT) = workText

            Next

            'メール設定
            COA0033Mail.I_COMPCODE = GBC_COMPCODE
            COA0033Mail.I_SYSCODE = COA0019Session.SYSCODE
            COA0033Mail.I_EVENTCODE = EVENTCODE
            COA0033Mail.I_SUBCODE = MAILSUBCODE
            COA0033Mail.I_ID = paraTable.Rows(0).Item("P_OD_ORDERNO").ToString
            COA0033Mail.I_TOADDRESS = dicAddress("TOADDRESS")
            COA0033Mail.I_CC = dicAddress("CC")
            COA0033Mail.I_BCC = dicAddress("BCC")
            COA0033Mail.I_REPLYTO = dicAddress("REPLYTO")
            COA0033Mail.I_SUBJECT = dicText("SUBJECT")
            COA0033Mail.I_BODY = dicText("BODY")
            COA0033Mail.COA0033setMailSend()

            Me.ERR = C_MESSAGENO.NORMAL

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub


    ''' <summary>
    ''' <para>メール送信設定</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00009setMailToUserM()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim COA0033Mail As New COA0033Mail
        Dim retValue As String = ""

        Const TBL_USERAPPLY As String = "COS0020_USERAPPLY"

        Try

            '置き換え文字列
            Dim paraTable As New DataTable
            Dim baseTable As New DataTable

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   Select ")
            sqlStat.AppendLine("   rtrim(UA.APPLYID) As P_USER_APPLYID, ")
            sqlStat.AppendLine("   rtrim(UA.USERID) As P_USER_ID, rtrim(UA.ORG) As P_USER_OFFICECODE, ")
            sqlStat.AppendLine("   CASE WHEN rtrim(UA.STAFFNAMES_EN) <> '' THEN rtrim(UA.STAFFNAMES_EN) ")
            sqlStat.AppendLine("   Else rtrim(UA.STAFFNAMES) End As P_USER_NAME, ")
            sqlStat.AppendLine("   RTrim(VO.NAMES) As P_USER_OFFICE, ")
            sqlStat.AppendLine("   rtrim(VJ.NAMES) As P_JOT_OFFICE, rtrim(VJ.CONTACTMAIL) As P_Addr_JOT, ")
            sqlStat.AppendLine("   rtrim(AH.APPLICANTID) As P_Addr_APPLICANT, rtrim(AH.APPROVEDTEXT) As P_AP_APPROVEDTEXT, ")
            sqlStat.AppendLine("   rtrim(US.STAFFNAMES_EN) as P_US_STAFFNAMES, rtrim(US.EMAIL) as P_US_EMAIL ")
            sqlStat.AppendFormat(" FROM {0} UA ", TBL_USERAPPLY).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VJ ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     On  VJ.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND VJ.MORG = '{0}' ", JOTORG).AppendLine()
            sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VJ.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VO ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VO.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VO.MORG = UA.ORG ")
            sqlStat.AppendLine("       AND VO.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VO.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} AH ", TBL_A_HIST).AppendLine()
            sqlStat.AppendFormat("     ON  AH.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.Append("           AND AH.APPLYID = UA.APPLYID ")
            sqlStat.AppendLine("       AND AH.STEP = @APPLYSTEP ")
            sqlStat.AppendLine("       AND AH.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} US ", TBL_USER).AppendLine()
            sqlStat.AppendLine("       ON  US.USERID = AH.APPLICANTID ")
            sqlStat.AppendLine("       AND US.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND US.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND US.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendLine("   WHERE UA.USERID   = @USERID ")
            sqlStat.AppendLine("   AND   UA.DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@USERID", System.Data.SqlDbType.NVarChar).Value = USERID
                    .Add("@APPLYSTEP", System.Data.SqlDbType.NVarChar).Value = LASTSTEP
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda As New SqlDataAdapter(sqlCmd)
                    sqlda.Fill(paraTable)
                End Using

            End Using

            'メール設定取得
            'SQL文の作成
            Dim sqlStat2 As New System.Text.StringBuilder
            sqlStat2.AppendLine("   SELECT ")
            sqlStat2.AppendLine("     COMPCODE, SYSTEMCODE, EVENTCODE, SUBCODE, ")
            sqlStat2.AppendLine("     TOADDRESS, CC, BCC, REPLYTO, SUBJECT, BODY ")
            sqlStat2.AppendFormat(" FROM {0} ", TBL_MAST).AppendLine()
            sqlStat2.AppendFormat(" WHERE COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat2.AppendFormat(" AND   SYSTEMCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            sqlStat2.AppendLine("   AND   EVENTCODE  = @EVENTCODE ")
            sqlStat2.AppendLine("   AND   SUBCODE  = rtrim(@SUBCODE) ")
            sqlStat2.AppendLine("   AND   STYMD <= @NOWDATE ")
            sqlStat2.AppendLine("   AND   ENDYMD >= @NOWDATE ")
            sqlStat2.AppendLine("   AND   DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn2 As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd2 As New SqlCommand(sqlStat2.ToString, sqlConn2)
                sqlConn2.Open()
                With sqlCmd2.Parameters
                    .Add("@EVENTCODE", System.Data.SqlDbType.NVarChar).Value = EVENTCODE
                    .Add("@SUBCODE", System.Data.SqlDbType.NVarChar).Value = MAILSUBCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda2 As New SqlDataAdapter(sqlCmd2)
                    sqlda2.Fill(baseTable)
                End Using

            End Using

            ' パラメータ展開
            Dim workTOADDRESS As String = ""
            Dim workCC As String = ""
            Dim workBCC As String = ""
            Dim workREPLYTO As String = ""
            Dim workSUBJECT As String = ""
            Dim workBODY As String = ""
            Dim workUPDTYPE As String = ""

            ' 申請タイプ
            Select Case UPDATETYPE
                Case "1"
                    workUPDTYPE = GBC_MAT_UPDTYPE.ADD
                Case "2"
                    workUPDTYPE = GBC_MAT_UPDTYPE.UPD
                Case "3"
                    workUPDTYPE = GBC_MAT_UPDTYPE.DEL
            End Select

            ' 宛先関連(TOADDRESS, CC, BCC, REPLYTO)
            Dim keyAddress As String() = New String() {"TOADDRESS", "CC", "BCC", "REPLYTO"}
            Dim dicAddress As New Dictionary(Of String, String)(keyAddress.Length)

            Dim workAddress As String
            For Each key In keyAddress

                workAddress = baseTable.Rows(0).Item(key).ToString
                workAddress = workAddress.Replace("P_Addr_JOT", paraTable.Rows(0).Item("P_Addr_JOT").ToString)
                workAddress = workAddress.Replace("P_Addr_APPLICANT", paraTable.Rows(0).Item("P_US_EMAIL").ToString)

                dicAddress(key) = workAddress

            Next

            ' 件名、本文関連(SUBJECT, BODY)
            Dim keyText As String() = New String() {"SUBJECT", "BODY"}
            Dim dicText As New Dictionary(Of String, String)(keyText.Length)

            Dim workText As String
            For Each keyT In keyText

                workText = baseTable.Rows(0).Item(keyT).ToString
                workText = workText.Replace("P_AP_ID", paraTable.Rows(0).Item("P_USER_APPLYID").ToString)
                workText = workText.Replace("P_USER_OFFICE", paraTable.Rows(0).Item("P_USER_OFFICE").ToString)
                workText = workText.Replace("P_USER_NAME", paraTable.Rows(0).Item("P_USER_NAME").ToString)
                workText = workText.Replace("P_USER_ID", paraTable.Rows(0).Item("P_USER_ID").ToString)
                workText = workText.Replace("P_UPDTYPE", workUPDTYPE)
                workText = workText.Replace("P_AP_APPROVEDTEXT", paraTable.Rows(0).Item("P_AP_APPROVEDTEXT").ToString)

                dicText(keyT) = workText

            Next

            'メール設定
            COA0033Mail.I_COMPCODE = GBC_COMPCODE
            COA0033Mail.I_SYSCODE = COA0019Session.SYSCODE
            COA0033Mail.I_EVENTCODE = EVENTCODE
            COA0033Mail.I_SUBCODE = MAILSUBCODE
            COA0033Mail.I_ID = paraTable.Rows(0).Item("P_USER_ID").ToString
            COA0033Mail.I_TOADDRESS = dicAddress("TOADDRESS")
            COA0033Mail.I_CC = dicAddress("CC")
            COA0033Mail.I_BCC = dicAddress("BCC")
            COA0033Mail.I_REPLYTO = dicAddress("REPLYTO")
            COA0033Mail.I_SUBJECT = dicText("SUBJECT")
            COA0033Mail.I_BODY = dicText("BODY")
            COA0033Mail.COA0033setMailSend()

            Me.ERR = C_MESSAGENO.NORMAL

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' <para>メール送信設定</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00009setMailToTank()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim COA0033Mail As New COA0033Mail
        Dim retValue As String = ""

        Try

            '置き換え文字列
            Dim paraTable As New DataTable
            Dim baseTable As New DataTable

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT TOP 1")
            sqlStat.AppendLine("   rtrim(ODV.ORDERNO) as P_OD_ORDERNO, rtrim(ODV.DTLOFFICE) as P_OD_OFFICECODE, ")
            sqlStat.AppendLine("   rtrim(ODV.TANKNO) as P_OD_TANKNO, ")
            sqlStat.AppendLine("   rtrim(ODV.COUNTRYCODE) as P_OD_COUNTRYCODE, rtrim(ODV.CURRENCYCODE) as P_OD_CURRENCYCODE, ")
            sqlStat.AppendLine("   rtrim(ODV.LOCALRATE) as P_OD_LOCALRATE, rtrim(ODV2.APPLYTEXT) as P_OD_APPLYTEXT,")
            sqlStat.AppendLine("   rtrim(ODV2.APPLYID) as P_OD_APPLYID, rtrim(ODV.BRID) as P_OD_BRID,")
            sqlStat.AppendLine("   case rtrim(ODV.DTLPOLPOD) ")
            sqlStat.AppendLine("     when 'POL1' then rtrim(POL1.AREANAME) ")
            sqlStat.AppendLine("     when 'POD1' then rtrim(POL1.AREANAME) ")
            sqlStat.AppendLine("     when 'POL2' then rtrim(isnull(POL2.AREANAME,'')) ")
            sqlStat.AppendLine("     when 'POD2' then rtrim(isnull(POL2.AREANAME,'')) ")
            sqlStat.AppendLine("   end as P_OD_PO_POL, ")
            sqlStat.AppendLine("   rtrim(ODB.SHIPPER) as P_OD_SHIPPERC, rtrim(CUS.NAMESEN) as P_OD_SHIPPER, ")
            sqlStat.AppendLine("   case rtrim(ODV.DTLPOLPOD) ")
            sqlStat.AppendLine("     when 'POL1' then rtrim(POD1.AREANAME) ")
            sqlStat.AppendLine("     when 'POD1' then rtrim(POD1.AREANAME) ")
            sqlStat.AppendLine("     when 'POL2' then rtrim(isnull(POD2.AREANAME,'')) ")
            sqlStat.AppendLine("     when 'POD2' then rtrim(isnull(POD2.AREANAME,'')) ")
            sqlStat.AppendLine("   end as P_OD_PO_POD, ")
            sqlStat.AppendLine("   rtrim(ODB.CONSIGNEE) as P_OD_CONSIGNEEC, rtrim(isnull(CUC.NAMESEN,'')) as P_OD_CONSIGNEE, ")
            sqlStat.AppendLine("   rtrim(VJ.NAMES) as P_BR_OF_JOT, rtrim(VJ.CONTACTMAIL) as P_Addr_JOT, ")
            sqlStat.AppendLine("   RTrim(VO.NAMES) As P_OD_OFFICE, ")
            sqlStat.AppendLine("   rtrim(AH.APPLICANTID) as P_Addr_APPLICANT, rtrim(AH.APPROVEDTEXT) as P_AP_APPROVEDTEXT, ")
            sqlStat.AppendLine("   rtrim(US.STAFFNAMES_EN) as P_US_STAFFNAMES, rtrim(US.EMAIL) as P_US_EMAIL, ")
            sqlStat.AppendLine("   CASE WHEN rtrim(TNK.NEXTINSPECTTYPE) <> '' THEN rtrim(TNK.NEXTINSPECTTYPE) + 'y test' ELSE '' END as P_TK_NEXTINSPECTTYPE, ")
            sqlStat.AppendLine("   FORMAT(TNK.NEXTINSPECTDATE,'yyyy/MM/dd') as P_TK_NEXTINSPECTDATE, ")
            sqlStat.AppendLine("   CASE WHEN TNK.REPAIRSTAT = 'Y' THEN 'Under repair' ELSE 'Not under repair' END as P_TK_REPAIRSTAT ")
            sqlStat.AppendFormat(" FROM {0} ODV ", TBL_ODV).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VJ ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VJ.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND VJ.MORG = '{0}' ", JOTORG).AppendLine()
            sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VJ.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VO ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VO.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VO.CARRIERCODE = ODV.DTLOFFICE ")
            sqlStat.AppendLine("       AND VO.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VO.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()

            sqlStat.AppendFormat(" INNER JOIN {0} ODB ", TBL_ODB).AppendLine()
            sqlStat.AppendLine("       ON  ODB.ORDERNO = ODV.ORDERNO ")
            sqlStat.AppendLine("       AND ODB.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND ODB.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND ODB.DELFLG <> '" & CONST_FLAG_YES & "' ")

            sqlStat.AppendFormat(" INNER JOIN {0} ODV2 ", TBL_ODV2).AppendLine()
            sqlStat.AppendLine("        ON ODV2.ORDERNO = ODV.ORDERNO ")
            sqlStat.AppendLine("       AND ODV2.TANKSEQ = ODV.TANKSEQ ")
            sqlStat.AppendLine("       AND ODV2.APPLYID = @APPLYID ")
            sqlStat.AppendLine("       AND ODV2.DELFLG <> '" & CONST_FLAG_YES & "' ")

            sqlStat.AppendFormat(" INNER JOIN {0} AH ", TBL_A_HIST).AppendLine()
            sqlStat.AppendFormat("     ON  AH.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND AH.APPLYID = ODV2.APPLYID ")
            sqlStat.AppendFormat("     AND AH.STEP = '{0}' ", APPLYSTEP).AppendLine()
            sqlStat.AppendLine("       AND AH.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} US ", TBL_USER).AppendLine()
            sqlStat.AppendLine("       ON  US.USERID = AH.APPLICANTID ")
            sqlStat.AppendLine("       AND US.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND US.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND US.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" INNER JOIN {0} POL1 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POL1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POL1.COUNTRYCODE = ODB.LOADCOUNTRY1 ")
            sqlStat.AppendLine("       AND POL1.PORTCODE = ODB.LOADPORT1 ")
            sqlStat.AppendLine("       AND POL1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POL1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POL1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" INNER JOIN {0} POD1 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POD1 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POD1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POD1.COUNTRYCODE = ODB.DISCHARGECOUNTRY1 ")
            sqlStat.AppendLine("       AND POD1.PORTCODE = ODB.DISCHARGEPORT1 ")
            sqlStat.AppendLine("       AND POD1.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POD1.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POD1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POL2 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POL2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POL2.COUNTRYCODE = ODB.LOADCOUNTRY2 ")
            sqlStat.AppendLine("       AND POL2.PORTCODE = ODB.LOADPORT2 ")
            sqlStat.AppendLine("       AND POL2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POL2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POL2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POD2 ", TBL_PORT).AppendLine()
            sqlStat.AppendFormat("     ON  POD2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND POD2.COUNTRYCODE = ODB.DISCHARGECOUNTRY2 ")
            sqlStat.AppendLine("       AND POD2.PORTCODE = ODB.DISCHARGEPORT2 ")
            sqlStat.AppendLine("       AND POD2.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND POD2.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND POD2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} CUS ", TBL_CUSTOMER).AppendLine()
            sqlStat.AppendFormat("     ON  CUS.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND CUS.CUSTOMERCODE = ODB.SHIPPER ")
            sqlStat.AppendLine("       AND CUS.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND CUS.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND CUS.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} CUC ", TBL_CUSTOMER).AppendLine()
            sqlStat.AppendFormat("     ON  CUC.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND CUC.CUSTOMERCODE = ODB.CONSIGNEE ")
            sqlStat.AppendLine("       AND CUC.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND CUC.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND CUC.DELFLG <> '" & CONST_FLAG_YES & "' ")

            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} TNK ", TBL_TANK).AppendLine()
            sqlStat.AppendFormat("     ON  TNK.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND TNK.TANKNO = ODV.TANKNO ")
            sqlStat.AppendLine("       AND TNK.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND TNK.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND TNK.DELFLG <> '" & CONST_FLAG_YES & "' ")

            sqlStat.AppendLine("   WHERE ODV.ORDERNO   = @ORDERNO ")
            sqlStat.AppendLine("   AND   ODV.DELFLG  <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendLine("   AND   ODV.DTLPOLPOD  = 'POL1' ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@ORDERNO", System.Data.SqlDbType.NVarChar).Value = ORDERNO
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@APPLYID", System.Data.SqlDbType.NVarChar).Value = APPLYID
                End With

                Using sqlda As New SqlDataAdapter(sqlCmd)
                    sqlda.Fill(paraTable)
                End Using

            End Using

            'メール設定取得
            'SQL文の作成
            Dim sqlStat2 As New System.Text.StringBuilder
            sqlStat2.AppendLine("   SELECT ")
            sqlStat2.AppendLine("     COMPCODE, SYSTEMCODE, EVENTCODE, SUBCODE, ")
            sqlStat2.AppendLine("     TOADDRESS, CC, BCC, REPLYTO, SUBJECT, BODY ")
            sqlStat2.AppendFormat(" FROM {0} ", TBL_MAST).AppendLine()
            sqlStat2.AppendFormat(" WHERE COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat2.AppendFormat(" AND   SYSTEMCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            sqlStat2.AppendLine("   AND   EVENTCODE  = @EVENTCODE ")
            sqlStat2.AppendLine("   AND   SUBCODE  = rtrim(@SUBCODE) ")
            sqlStat2.AppendLine("   AND   STYMD <= @NOWDATE ")
            sqlStat2.AppendLine("   AND   ENDYMD >= @NOWDATE ")
            sqlStat2.AppendLine("   AND   DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn2 As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd2 As New SqlCommand(sqlStat2.ToString, sqlConn2)
                sqlConn2.Open()
                With sqlCmd2.Parameters
                    .Add("@EVENTCODE", System.Data.SqlDbType.NVarChar).Value = EVENTCODE
                    .Add("@SUBCODE", System.Data.SqlDbType.NVarChar).Value = MAILSUBCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda2 As New SqlDataAdapter(sqlCmd2)
                    sqlda2.Fill(baseTable)
                End Using

            End Using

            ' パラメータ展開
            Dim workTOADDRESS As String = ""
            Dim workCC As String = ""
            Dim workBCC As String = ""
            Dim workREPLYTO As String = ""
            Dim workSUBJECT As String = ""
            Dim workBODY As String = ""

            ' 宛先関連(TOADDRESS, CC, BCC, REPLYTO)
            Dim keyAddress As String() = New String() {"TOADDRESS", "CC", "BCC", "REPLYTO"}
            Dim dicAddress As New Dictionary(Of String, String)(keyAddress.Length)

            Dim workAddress As String
            For Each key In keyAddress

                workAddress = baseTable.Rows(0).Item(key).ToString
                workAddress = workAddress.Replace("P_Addr_JOT", paraTable.Rows(0).Item("P_Addr_JOT").ToString)
                workAddress = workAddress.Replace("P_Addr_APPLICANT", paraTable.Rows(0).Item("P_US_EMAIL").ToString)

                dicAddress(key) = workAddress

            Next

            ' 件名、本文関連(SUBJECT, BODY)
            Dim keyText As String() = New String() {"SUBJECT", "BODY"}
            Dim dicText As New Dictionary(Of String, String)(keyText.Length)

            Dim workText As String
            For Each keyT In keyText

                workText = baseTable.Rows(0).Item(keyT).ToString
                workText = workText.Replace("P_AP_ID", paraTable.Rows(0).Item("P_OD_APPLYID").ToString)
                workText = workText.Replace("P_US_STAFFNAMES", paraTable.Rows(0).Item("P_US_STAFFNAMES").ToString)
                workText = workText.Replace("P_OD_ORDERNO", paraTable.Rows(0).Item("P_OD_ORDERNO").ToString)
                workText = workText.Replace("P_OD_TANKNO", paraTable.Rows(0).Item("P_OD_TANKNO").ToString)
                workText = workText.Replace("P_OD_BRID", paraTable.Rows(0).Item("P_OD_BRID").ToString)

                workText = workText.Replace("P_OD_PO_POL", paraTable.Rows(0).Item("P_OD_PO_POL").ToString)
                workText = workText.Replace("P_OD_PO_POD", paraTable.Rows(0).Item("P_OD_PO_POD").ToString)
                workText = workText.Replace("P_OD_SHIPPER", paraTable.Rows(0).Item("P_OD_SHIPPER").ToString)
                workText = workText.Replace("P_OD_CONSIGNEE", paraTable.Rows(0).Item("P_OD_CONSIGNEE").ToString)

                workText = workText.Replace("P_OD_CURRENCY", paraTable.Rows(0).Item("P_OD_CURRENCYCODE").ToString)
                workText = workText.Replace("P_OD_APPLYTEXT", paraTable.Rows(0).Item("P_OD_APPLYTEXT").ToString)
                workText = workText.Replace("P_AP_APPROVEDTEXT", paraTable.Rows(0).Item("P_AP_APPROVEDTEXT").ToString)

                workText = workText.Replace("P_AP_OF", paraTable.Rows(0).Item("P_OD_OFFICE").ToString)
                workText = workText.Replace("P_BR_ID", paraTable.Rows(0).Item("P_OD_BRID").ToString)
                workText = workText.Replace("P_AP_TANK", paraTable.Rows(0).Item("P_OD_TANKNO").ToString)
                workText = workText.Replace("P_AP_USER", COA0019Session.USERNAME)

                workText = workText.Replace("P_TK_NEXTINSPECTTYPE", paraTable.Rows(0).Item("P_TK_NEXTINSPECTTYPE").ToString)
                workText = workText.Replace("P_TK_NEXTINSPECTDATE", paraTable.Rows(0).Item("P_TK_NEXTINSPECTDATE").ToString)
                workText = workText.Replace("P_TK_REPAIRSTAT", paraTable.Rows(0).Item("P_TK_REPAIRSTAT").ToString)

                dicText(keyT) = workText

            Next

            'メール設定
            COA0033Mail.I_COMPCODE = GBC_COMPCODE
            COA0033Mail.I_SYSCODE = COA0019Session.SYSCODE
            COA0033Mail.I_EVENTCODE = EVENTCODE
            COA0033Mail.I_SUBCODE = MAILSUBCODE
            'COA0033Mail.I_ID = paraTable.Rows(0).Item("P_OD_ORDERNO").ToString
            COA0033Mail.I_ID = paraTable.Rows(0).Item("P_OD_TANKNO").ToString
            COA0033Mail.I_TOADDRESS = dicAddress("TOADDRESS")
            COA0033Mail.I_CC = dicAddress("CC")
            COA0033Mail.I_BCC = dicAddress("BCC")
            COA0033Mail.I_REPLYTO = dicAddress("REPLYTO")
            COA0033Mail.I_SUBJECT = dicText("SUBJECT")
            COA0033Mail.I_BODY = dicText("BODY")
            COA0033Mail.COA0033setMailSend()

            Me.ERR = C_MESSAGENO.NORMAL

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>メール送信設定(リースブレーカー)</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00009setMailToLeaseBr()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim COA0033Mail As New COA0033Mail
        Dim retValue As String = ""

        Try

            '置き換え文字列
            Dim paraTable As New DataTable
            Dim baseTable As New DataTable

            If IsNothing(LASTSTEP) Then
                LASTSTEP = ""
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            '承認情報取得
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("      ,AGR.CONTRACTNO                            AS P_LB_CONTRACTNO")
            sqlStat.AppendLine("      ,AGR.AGREEMENTNO                           AS P_LB_AGREEMENTNO")
            sqlStat.AppendLine("      ,convert(nvarchar, CTR.CONTRACTFROM , 111) AS P_LB_CONTRACTFROM")
            sqlStat.AppendLine("      ,CTR.ENABLED                               AS P_LB_ENABLED")
            sqlStat.AppendLine("      ,AGR.LEASETYPE                             AS P_LB_LEASETYPE")
            sqlStat.AppendLine("      ,ISNULL(FVTYP.VALUE2,'')                   AS P_LB_LEASETYPENAME")
            sqlStat.AppendLine("      ,AGR.LEASETERM                             AS P_LB_LEASETERM")
            sqlStat.AppendLine("      ,ISNULL(FVLRM.VALUE2,'')                   AS P_LB_LEASETERMNAME")
            sqlStat.AppendLine("      ,ISNULL(SP.NAMESEN,'')                     AS P_LB_SHIPPER")
            sqlStat.AppendLine("      ,AGR.LEASEPAYMENTTYPE                      AS P_LB_LEASEPAYMENTTYPE")
            sqlStat.AppendLine("      ,ISNULL(FVLPM.VALUE2,'')                   AS P_LB_LEASEPAYMENTTYPENAME")
            sqlStat.AppendLine("      ,AGR.LEASEPAYMENTKIND                      AS P_LB_LEASEPAYMENTKIND")
            sqlStat.AppendLine("      ,ISNULL(FVLPK.VALUE2,'')                   AS P_LB_LEASEPAYMENTKINDNAME")
            sqlStat.AppendLine("      ,AGR.PRODUCTCODE                           AS P_LB_PRODUCTCODE")
            sqlStat.AppendLine("      ,rtrim(ISNULL(PU.PRODUCTNAME,''))          AS P_LB_PRODUCT")
            sqlStat.AppendLine("      ,AGR.LEASEPAYMENTS                         AS P_LB_LEASEPAYMENTS")
            sqlStat.AppendLine("      ,AGR.AUTOEXTEND                            AS P_LB_AUTOEXTEND")
            sqlStat.AppendLine("      ,AGR.AUTOEXTENDKIND                        AS P_LB_AUTOEXTENDKIND")
            sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FVEXK.VALUE1,'') ")
            sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FVEXK.VALUE2,'') END AS P_LB_AUTOEXTENDKINDNAME")
            sqlStat.AppendLine("      ,AGR.RELEASE                               AS P_LB_RELEASE ")
            sqlStat.AppendLine("      ,AGR.CURRENCY                              AS P_LB_CURRENCY")
            sqlStat.AppendLine("      ,AGR.DELFLG                                AS P_LB_DELFLG")
            sqlStat.AppendLine("      ,AH.APPROVEDTEXT                           AS P_LB_APPROVEDTEXT")
            sqlStat.AppendLine("      ,AH.APPLYID                                AS P_LB_APPLYID")
            sqlStat.AppendLine("      ,AH.STEP                                   AS P_LB_STEP")
            sqlStat.AppendLine("      ,AH.STATUS                                 AS P_LB_STATUS")
            sqlStat.AppendLine("      ,AGR.LASTSTEP                              AS P_LB_LASTSTEP")
            sqlStat.AppendLine("      ,AP.APPROVALTYPE                           AS P_LB_APPROVALTYPE")
            sqlStat.AppendLine("      ,rtrim(VJ.NAMES) as P_LB_OF_JOT, rtrim(VJ.CONTACTMAIL)    as P_Addr_JOT ")
            sqlStat.AppendLine("      ,rtrim(VO.NAMES) as P_LB_OF_ORG, rtrim(VO.MAIL_ORGANIZER) as P_Addr_ORG, ")
            sqlStat.AppendFormat("  FROM {0} AGR", TBL_L_AGR).AppendLine() '協定書(申請対象)テーブル
            sqlStat.AppendFormat("  INNER JOIN {0} CTR", TBL_L_CTR).AppendLine() '契約書テーブル
            sqlStat.AppendLine("    ON  CTR.CONTRACTNO   = AGR.CONTRACTNO")
            sqlStat.AppendLine("   AND  CTR.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  CTR.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  CTR.DELFLG      <> @DELFLG")
            sqlStat.AppendFormat(" INNER JOIN {0} VJ ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VJ.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND VJ.MORG = '{0}' ", JOTORG).AppendLine()
            sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VJ.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat(" INNER JOIN {0} VO ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VO.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VO.CARRIERCODE = CTR.ORGANIZER ")
            sqlStat.AppendLine("       AND VO.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VO.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            sqlStat.AppendFormat("  LEFT JOIN {0} SP", TBL_CUSTOMER).AppendLine() 'SHIPPER名称用JOIN
            sqlStat.AppendLine("    ON  SP.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  SP.CUSTOMERCODE = CTR.SHIPPER")
            sqlStat.AppendLine("   AND  SP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  SP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  SP.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("   AND  SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
            sqlStat.AppendFormat("  LEFT JOIN {0} FV1", TBL_FIXVALUE).AppendLine() '承認対象名称用JOIN
            sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
            sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
            sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
            sqlStat.AppendFormat("  LEFT JOIN {0} FV2", TBL_FIXVALUE).AppendLine() '承認否認名称用JOIN
            sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
            sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
            sqlStat.AppendFormat("  LEFT JOIN {0} FVLRM", TBL_FIXVALUE) 'リースターム名称用JOIN
            sqlStat.AppendLine("    ON  FVLRM.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FVLRM.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FVLRM.CLASS        = 'LEASETERM'")
            sqlStat.AppendLine("   AND  FVLRM.KEYCODE      = AGR.LEASETERM")
            sqlStat.AppendLine("   AND  FVLRM.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FVLRM.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FVLRM.DELFLG      <> @DELFLG")
            sqlStat.AppendFormat("  LEFT JOIN {0} FVTYP", TBL_FIXVALUE).AppendLine() 'リースタイプ名称用JOIN
            sqlStat.AppendLine("    ON  FVTYP.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FVTYP.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FVTYP.CLASS        = 'LEASEPAYMENT'")
            sqlStat.AppendLine("   AND  FVTYP.KEYCODE      = AGR.LEASETYPE")
            sqlStat.AppendLine("   AND  FVTYP.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FVTYP.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FVTYP.DELFLG      <> @DELFLG")
            sqlStat.AppendFormat("  LEFT JOIN {0} FVLPM", TBL_FIXVALUE).AppendLine() '支払い月名称用JOIN
            sqlStat.AppendLine("    ON  FVLPM.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FVLPM.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FVLPM.CLASS        = 'LEASEPAYMENTMONTH'")
            sqlStat.AppendLine("   AND  FVLPM.KEYCODE      = AGR.LEASEPAYMENTTYPE")
            sqlStat.AppendLine("   AND  FVLPM.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FVLPM.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FVLPM.DELFLG      <> @DELFLG")
            sqlStat.AppendFormat("  LEFT JOIN {0} FVLPK", TBL_FIXVALUE).AppendLine() '支払い種別名称用JOIN
            sqlStat.AppendLine("    ON  FVLPK.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FVLPK.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FVLPK.CLASS        = 'LEASEPAYMENTKIND'")
            sqlStat.AppendLine("   AND  FVLPK.KEYCODE      = AGR.LEASEPAYMENTKIND")
            sqlStat.AppendLine("   AND  FVLPK.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FVLPK.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FVLPK.DELFLG      <> @DELFLG")
            sqlStat.AppendFormat("  LEFT JOIN {0} FVEXK", TBL_FIXVALUE).AppendLine() '自動延長種類名称用JOIN
            sqlStat.AppendLine("    ON  FVEXK.COMPCODE     = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("   AND  FVEXK.SYSCODE      = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("   AND  FVEXK.CLASS        = 'AUTOEXTENDKIND'")
            sqlStat.AppendLine("   AND  FVEXK.KEYCODE      = AGR.AUTOEXTENDKIND")
            sqlStat.AppendLine("   AND  FVEXK.STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND  FVEXK.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND  FVEXK.DELFLG      <> @DELFLG")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} AH ", TBL_A_HIST).AppendLine()
            sqlStat.AppendFormat("     ON  AH.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND AH.APPLYID = '{0}' ", APPLYID).AppendLine()
            sqlStat.AppendFormat("     AND AH.STEP = '{0}' ", LASTSTEP).AppendLine()
            sqlStat.AppendLine("       AND AH.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat(" LEFT OUTER JOIN {0} PU ", TBL_PRODUCT).AppendLine()
            sqlStat.AppendLine("       ON  PU.PRODUCTCODE = AGR.PRODUCTCODE ")
            sqlStat.AppendLine("       AND PU.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND PU.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND PU.DELFLG <> '" & CONST_FLAG_YES & "' ")

            sqlStat.AppendLine(" WHERE AGR.CONTRACTNO    = @CONTRACTNO")
            sqlStat.AppendLine("   AND AGR.AGREEMENTNO   = @AGREEMENTNO")
            sqlStat.AppendLine("   AND AGR.DELFLG       <> @DELFLG")
#Region "参考値"


            'sqlStat.AppendLine("   SELECT ")
            'sqlStat.AppendLine("   rtrim(BR.TERMTYPE) as P_BR_TERMTYPEC, rtrim(FV.VALUE2) as P_BR_TERM, ")
            'sqlStat.AppendLine("   rtrim(VJ.NAMES) as P_BR_OF_JOT, rtrim(VJ.CONTACTMAIL) as P_Addr_JOT, ")
            'sqlStat.AppendLine("   rtrim(VO.NAMES) as P_BR_OF_ORG, rtrim(VO.MAIL_ORGANIZER) as P_Addr_ORG, ")
            'sqlStat.AppendLine("   rtrim(VL1.NAMES) as P_BR_OF_POL1, rtrim(VL1.MAIL_POL) as P_Addr_POL1, ")
            'sqlStat.AppendLine("   rtrim(VD1.NAMES) as P_BR_OF_POD1, rtrim(VD1.MAIL_POD) as P_Addr_POD1, ")
            'sqlStat.AppendLine("   rtrim(isnull(VL2.NAMES,'-')) as P_BR_OF_POL2, rtrim(isnull(VL2.MAIL_POL,'')) as P_Addr_POL2, ")
            'sqlStat.AppendLine("   rtrim(isnull(VD2.NAMES,'-')) as P_BR_OF_POD2, rtrim(isnull(VD2.MAIL_POD,'')) as P_Addr_POD2, ")
            'sqlStat.AppendLine("   rtrim(BR.LOADPORT1) as P_BR_PO_POL1C, rtrim(POL1.AREANAME) as P_BR_PO_POL1, ")
            'sqlStat.AppendLine("   rtrim(BR.DISCHARGEPORT1) as P_BR_PO_POD1C, rtrim(POD1.AREANAME) as P_BR_PO_POD1, ")
            'sqlStat.AppendLine("   rtrim(isnull(BR.LOADPORT2,'-')) as P_BR_PO_POL2C, rtrim(isnull(POL2.AREANAME,'-')) as P_BR_PO_POL2, ")
            'sqlStat.AppendLine("   rtrim(isnull(BR.DISCHARGEPORT2,'-')) as P_BR_PO_POD2C, rtrim(isnull(POD2.AREANAME,'-')) as P_BR_PO_POD2, ")
            'sqlStat.AppendLine("   rtrim(BRIO.REMARK) as P_BR_SPI_ORG, ")
            'sqlStat.AppendLine("   rtrim(UPPER(LEFT(BRIO.BRTYPE,1)) + LOWER(SUBSTRING(BRIO.BRTYPE,2,len(BRIO.BRTYPE)-1))) as P_BR_BRTYPE, ")
            'sqlStat.AppendLine("   rtrim(BRIL1.REMARK) as P_BR_SPI_POL1, ")
            'sqlStat.AppendLine("   rtrim(BRID1.REMARK) as P_BR_SPI_POD1, ")
            'sqlStat.AppendLine("   rtrim(isnull(BRIL2.REMARK,'-')) as P_BR_SPI_POL2, ")
            'sqlStat.AppendLine("   rtrim(isnull(BRID2.REMARK,'-')) as P_BR_SPI_POD2, ")
            'sqlStat.AppendLine("   rtrim(BR.APPLYTEXT) as P_BR_APPLYTEXT, ")
            'sqlStat.AppendLine("   rtrim(AH.APPLICANTID) As P_Addr_APPLICANT, rtrim(AH.APPROVEDTEXT) As P_BR_APPROVEDTEXT, ")
            'sqlStat.AppendLine("   rtrim(BR.SHIPPER) as P_BR_SHIPPERC, ")
            'sqlStat.AppendLine("   rtrim(isnull(CU.NAMESEN, CUVD.NAMEL)) as P_BR_SHIPPER, ")
            'sqlStat.AppendLine("   rtrim(BR.PRODUCTCODE) as P_BR_PRODUCTC, rtrim(PU.PRODUCTNAME) as P_BR_PRODUCT ")
            'sqlStat.AppendFormat(" FROM {0} BR ", TBL_BR).AppendLine()
            'sqlStat.AppendFormat(" INNER JOIN {0} VJ ", TBL_VENDER).AppendLine()
            'sqlStat.AppendFormat("     ON  VJ.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendFormat("     AND VJ.MORG = '{0}' ", JOTORG).AppendLine()
            'sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat("     AND VJ.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            'sqlStat.AppendFormat(" INNER JOIN {0} VO ", TBL_VENDER).AppendLine()
            'sqlStat.AppendFormat("     ON  VO.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND VO.CARRIERCODE = BR.AGENTORGANIZER ")
            'sqlStat.AppendLine("       AND VO.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND VO.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND VO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat("     AND VO.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            'sqlStat.AppendFormat(" INNER JOIN {0} VL1 ", TBL_VENDER).AppendLine()
            'sqlStat.AppendFormat("     ON  VL1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND VL1.CARRIERCODE = BR.AGENTPOL1 ")
            'sqlStat.AppendLine("       AND VL1.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND VL1.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND VL1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat("     AND VL1.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} VD1 ", TBL_VENDER).AppendLine()
            'sqlStat.AppendFormat("     ON  VD1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND VD1.CARRIERCODE = BR.AGENTPOD1 ")
            'sqlStat.AppendLine("       AND VD1.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND VD1.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND VD1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat("     AND VD1.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} VL2 ", TBL_VENDER).AppendLine()
            'sqlStat.AppendFormat("     ON  VL2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND VL2.CARRIERCODE = BR.AGENTPOL2 ")
            'sqlStat.AppendLine("       AND VL2.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND VL2.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND VL2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat("     AND VL2.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} VD2 ", TBL_VENDER).AppendLine()
            'sqlStat.AppendFormat("     ON  VD2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND VD2.CARRIERCODE = BR.AGENTPOD2 ")
            'sqlStat.AppendLine("       AND VD2.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND VD2.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND VD2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat("     AND VD2.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            'sqlStat.AppendFormat(" INNER JOIN {0} POL1 ", TBL_PORT).AppendLine()
            'sqlStat.AppendFormat("     ON  POL1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND POL1.COUNTRYCODE = BR.LOADCOUNTRY1 ")
            'sqlStat.AppendLine("       AND POL1.PORTCODE = BR.LOADPORT1 ")
            'sqlStat.AppendLine("       AND POL1.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND POL1.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND POL1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POD1 ", TBL_PORT).AppendLine()
            'sqlStat.AppendFormat("     ON  POD1.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND POD1.COUNTRYCODE = BR.DISCHARGECOUNTRY1 ")
            'sqlStat.AppendLine("       AND POD1.PORTCODE = BR.DISCHARGEPORT1 ")
            'sqlStat.AppendLine("       AND POD1.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND POD1.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND POD1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POL2 ", TBL_PORT).AppendLine()
            'sqlStat.AppendFormat("     ON  POL2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND POL2.COUNTRYCODE = BR.LOADCOUNTRY2 ")
            'sqlStat.AppendLine("       AND POL2.PORTCODE = BR.LOADPORT2 ")
            'sqlStat.AppendLine("       AND POL2.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND POL2.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND POL2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} POD2 ", TBL_PORT).AppendLine()
            'sqlStat.AppendFormat("     ON  POD2.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND POD2.COUNTRYCODE = BR.DISCHARGECOUNTRY2 ")
            'sqlStat.AppendLine("       AND POD2.PORTCODE = BR.DISCHARGEPORT2 ")
            'sqlStat.AppendLine("       AND POD2.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND POD2.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND POD2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" INNER JOIN {0} BRIO ", TBL_BRI).AppendLine()
            'sqlStat.AppendLine("     ON  BRIO.BRID = @BRID ")
            'sqlStat.AppendLine("       AND BRIO.SUBID = @BRSUBID ")
            'sqlStat.AppendLine("       AND BRIO.TYPE = 'INFO' ")
            'sqlStat.AppendLine("       AND BRIO.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRIO.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRIO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" INNER JOIN {0} BRIL1 ", TBL_BRI).AppendLine()
            'sqlStat.AppendLine("     ON  BRIL1.BRID = @BRID ")
            'sqlStat.AppendLine("       AND BRIL1.SUBID = @BRSUBID ")
            'sqlStat.AppendLine("       AND BRIL1.TYPE = 'POL1' ")
            'sqlStat.AppendLine("       AND BRIL1.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRIL1.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRIL1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} BRID1 ", TBL_BRI).AppendLine()
            'sqlStat.AppendLine("     ON  BRID1.BRID = @BRID ")
            'sqlStat.AppendLine("       AND BRID1.SUBID = @BRSUBID ")
            'sqlStat.AppendLine("       AND BRID1.TYPE = 'POD1' ")
            'sqlStat.AppendLine("       AND BRID1.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRID1.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRID1.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} BRIL2 ", TBL_BRI).AppendLine()
            'sqlStat.AppendLine("     ON  BRIL2.BRID = @BRID ")
            'sqlStat.AppendLine("       AND BRIL2.SUBID = @BRSUBID ")
            'sqlStat.AppendLine("       AND BRIL2.TYPE = 'POL2' ")
            'sqlStat.AppendLine("       AND BRIL2.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRIL2.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRIL2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} BRID2 ", TBL_BRI).AppendLine()
            'sqlStat.AppendLine("     ON  BRID2.BRID = @BRID ")
            'sqlStat.AppendLine("       AND BRID2.SUBID = @BRSUBID ")
            'sqlStat.AppendLine("       AND BRID2.TYPE = 'POD2' ")
            'sqlStat.AppendLine("       AND BRID2.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRID2.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND BRID2.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} CU ", TBL_CUSTOMER).AppendLine()
            'sqlStat.AppendFormat("     ON  CU.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND CU.COUNTRYCODE = BR.LOADCOUNTRY1 ")
            'sqlStat.AppendLine("       AND CU.CUSTOMERCODE = BR.SHIPPER ")
            'sqlStat.AppendLine("       AND CU.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND CU.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND CU.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} CUVD ", TBL_VENDER).AppendLine()
            'sqlStat.AppendFormat("     ON  CUVD.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendLine("       AND CUVD.CARRIERCODE = BR.SHIPPER ")
            'sqlStat.AppendLine("       AND CUVD.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND CUVD.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND CUVD.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat("     AND CUVD.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} PU ", TBL_PRODUCT).AppendLine()
            'sqlStat.AppendLine("       ON  PU.PRODUCTCODE = BR.PRODUCTCODE ")
            'sqlStat.AppendLine("       AND PU.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND PU.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND PU.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" INNER JOIN {0} FV ", TBL_FIXVALUE).AppendLine()
            'sqlStat.AppendLine("       ON  FV.COMPCODE = '" & GBC_COMPCODE_D & "' ")
            'sqlStat.AppendFormat("     AND FV.SYSCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            'sqlStat.AppendLine("       AND FV.CLASS = 'TERM' ")
            'sqlStat.AppendLine("       AND FV.KEYCODE = BR.TERMTYPE ")
            'sqlStat.AppendLine("       AND FV.STYMD <= @NOWDATE ")
            'sqlStat.AppendLine("       AND FV.ENDYMD >= @NOWDATE ")
            'sqlStat.AppendLine("       AND FV.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendFormat(" LEFT OUTER JOIN {0} AH ", TBL_A_HIST).AppendLine()
            'sqlStat.AppendFormat("     ON  AH.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            'sqlStat.AppendFormat("     AND AH.APPLYID = '{0}' ", APPLYID).AppendLine()
            'sqlStat.AppendFormat("     AND AH.STEP = '{0}' ", LASTSTEP).AppendLine()
            'sqlStat.AppendLine("       AND AH.DELFLG <> '" & CONST_FLAG_YES & "' ")
            'sqlStat.AppendLine("   WHERE BR.BRID   = @BRID ")
            'sqlStat.AppendLine("   AND   BR.BRBASEID  = @BRBASEID ")
            'sqlStat.AppendLine("   AND   BR.DELFLG  <> '" & CONST_FLAG_YES & "' ")
#End Region
            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@CONTRACTNO", System.Data.SqlDbType.NVarChar).Value = Me.CONTRACTNO
                    .Add("@AGREEMENTNO", System.Data.SqlDbType.NVarChar).Value = Me.AGREEMENTNO
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda As New SqlDataAdapter(sqlCmd)
                    sqlda.Fill(paraTable)
                End Using

            End Using

            'メール設定取得
            'SQL文の作成
            Dim sqlStat2 As New System.Text.StringBuilder
            sqlStat2.AppendLine("   SELECT ")
            sqlStat2.AppendLine("     COMPCODE, SYSTEMCODE, EVENTCODE, SUBCODE, ")
            sqlStat2.AppendLine("     TOADDRESS, CC, BCC, REPLYTO, SUBJECT, BODY ")
            sqlStat2.AppendFormat(" FROM {0} BR ", TBL_MAST).AppendLine()
            sqlStat2.AppendFormat(" WHERE COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat2.AppendFormat(" AND   SYSTEMCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            sqlStat2.AppendLine("   AND   EVENTCODE  = @EVENTCODE ")
            sqlStat2.AppendLine("   AND   SUBCODE  = rtrim(@SUBCODE) ")
            sqlStat2.AppendLine("   AND   STYMD <= @NOWDATE ")
            sqlStat2.AppendLine("   AND   ENDYMD >= @NOWDATE ")
            sqlStat2.AppendLine("   AND   DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn2 As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd2 As New SqlCommand(sqlStat2.ToString, sqlConn2)
                sqlConn2.Open()
                With sqlCmd2.Parameters
                    .Add("@EVENTCODE", System.Data.SqlDbType.NVarChar).Value = EVENTCODE
                    .Add("@SUBCODE", System.Data.SqlDbType.NVarChar).Value = MAILSUBCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda2 As New SqlDataAdapter(sqlCmd2)
                    sqlda2.Fill(baseTable)
                End Using

            End Using

            ' 宛先関連(TOADDRESS, CC, BCC, REPLYTO)
            Dim keyAddress As String() = New String() {"TOADDRESS", "CC", "BCC", "REPLYTO"}
            Dim dicAddress As New Dictionary(Of String, String)(keyAddress.Length)

            Dim workAddress As String
            For Each key In keyAddress

                workAddress = baseTable.Rows(0).Item(key).ToString
                workAddress = workAddress.Replace("P_Addr_JOT", paraTable.Rows(0).Item("P_Addr_JOT").ToString)
                workAddress = workAddress.Replace("P_Addr_ORG", paraTable.Rows(0).Item("P_Addr_ORG").ToString)

                dicAddress(key) = workAddress
            Next

            ' 件名、本文関連(SUBJECT, BODY)
            Dim keyText As String() = New String() {"SUBJECT", "BODY"}
            Dim dicText As New Dictionary(Of String, String)(keyText.Length)

            Dim workText As String
            For Each keyT In keyText

                workText = baseTable.Rows(0).Item(keyT).ToString
                workText = workText.Replace("P_LB_CONTRACTNO", Me.CONTRACTNO)
                workText = workText.Replace("P_LB_AGREEMENTNO", Me.AGREEMENTNO)

                workText = workText.Replace("P_LB_LEASETYPENAME", paraTable.Rows(0).Item("P_LB_LEASETYPENAME").ToString)
                workText = workText.Replace("P_LB_LEASETERMNAME", paraTable.Rows(0).Item("P_LB_LEASETERMNAME").ToString)

                workText = workText.Replace("P_LB_OF_JOT", paraTable.Rows(0).Item("P_LB_OF_JOT").ToString)
                workText = workText.Replace("P_LB_OF_ORG", paraTable.Rows(0).Item("P_LB_OF_ORG").ToString)

                workText = workText.Replace("P_LB_SHIPPER", paraTable.Rows(0).Item("P_LB_SHIPPER").ToString)
                workText = workText.Replace("P_LB_PRODUCT", paraTable.Rows(0).Item("P_LB_PRODUCT").ToString)

                workText = workText.Replace("P_BR_SPI_ORG", paraTable.Rows(0).Item("P_BR_SPI_ORG").ToString)
                workText = workText.Replace("P_BR_SPI_POL1", paraTable.Rows(0).Item("P_BR_SPI_POL1").ToString)
                workText = workText.Replace("P_BR_SPI_POD1", paraTable.Rows(0).Item("P_BR_SPI_POD1").ToString)
                workText = workText.Replace("P_BR_SPI_POL2", paraTable.Rows(0).Item("P_BR_SPI_POL2").ToString)
                workText = workText.Replace("P_BR_SPI_POD2", paraTable.Rows(0).Item("P_BR_SPI_POD2").ToString)
                If BRROUND <> "2" Then
                    workText = workText.Replace("P_BR_SPI_POLx", paraTable.Rows(0).Item("P_BR_SPI_POL1").ToString)
                    workText = workText.Replace("P_BR_SPI_PODx", paraTable.Rows(0).Item("P_BR_SPI_POD1").ToString)
                Else
                    workText = workText.Replace("P_BR_SPI_POLx", paraTable.Rows(0).Item("P_BR_SPI_POL2").ToString)
                    workText = workText.Replace("P_BR_SPI_PODx", paraTable.Rows(0).Item("P_BR_SPI_POD2").ToString)
                End If
                workText = workText.Replace("P_BR_APPLYTEXT", paraTable.Rows(0).Item("P_BR_APPLYTEXT").ToString)
                workText = workText.Replace("P_BR_APPROVEDTEXT", paraTable.Rows(0).Item("P_BR_APPROVEDTEXT").ToString)
                workText = workText.Replace("P_BR_OF_USER", COA0019Session.USERNAME)

                dicText(keyT) = workText

            Next

            'メール設定
            COA0033Mail.I_COMPCODE = GBC_COMPCODE
            COA0033Mail.I_SYSCODE = COA0019Session.SYSCODE
            COA0033Mail.I_EVENTCODE = EVENTCODE
            COA0033Mail.I_SUBCODE = MAILSUBCODE
            COA0033Mail.I_ID = BRID
            COA0033Mail.I_TOADDRESS = dicAddress("TOADDRESS")
            COA0033Mail.I_CC = dicAddress("CC")
            COA0033Mail.I_BCC = dicAddress("BCC")
            COA0033Mail.I_REPLYTO = dicAddress("REPLYTO")
            COA0033Mail.I_SUBJECT = dicText("SUBJECT")
            COA0033Mail.I_BODY = dicText("BODY")
            COA0033Mail.COA0033setMailSend()

            Me.ERR = C_MESSAGENO.NORMAL

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
    ''' <summary>
    ''' <para>メール送信設定</para>
    ''' <para>なし</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    ''' <remarks>SOA Close時のメール送信</remarks>
    Public Sub GBA00009setMailToBliingClose()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim COA0033Mail As New COA0033Mail
        Dim retValue As String = ""

        Try

            '置き換え文字列
            Dim paraTable As New DataTable
            Dim baseTable As New DataTable

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT rtrim(CD.COUNTRYCODE)                    as P_AP_COUNTRYCODE")
            sqlStat.AppendLine("         ,rtrim(CD.COUNTRYCODE) +  replace(rtrim(CD.REPORTMONTH),'/','')  as P_MAILHISTORY_ID")
            sqlStat.AppendLine("         ,rtrim(ISNULL(CNRY.NAMES,CD.COUNTRYCODE)) as P_AP_COUNTRY")
            sqlStat.AppendLine("         ,FORMAT(CONVERT(Date, CD.REPORTMONTH + '/01'),'MM-yyyy') as P_AP_MONTH")
            sqlStat.AppendLine("         ,rtrim(CD.APPLYOFFICE)                    as P_AP_OFFICECODE")
            sqlStat.AppendLine("         ,rtrim(isnull(CNRY.OFFICENAME,VO.NAMES))  as P_AP_OFFICE ")
            '申請者
            sqlStat.AppendLine("         ,rtrim(US.STAFFNAMES_EN)                  as P_US_STAFFNAMES")
            sqlStat.AppendLine("         ,rtrim(US.EMAIL)                          as P_US_EMAIL")
            'JOT情報
            sqlStat.AppendLine("         ,rtrim(VJ.NAMES)                          as P_AP_OFFICE_JOT")
            sqlStat.AppendLine("         ,rtrim(VJ.CONTACTMAIL)                    as P_Addr_JOT")
            '国メールアドレス
            sqlStat.AppendLine("         ,rtrim(isnull(CNRY.MAIL_COUNTRY,''))      as P_Addr_COUNTRY")

            '申請者情報
            sqlStat.AppendLine("         ,rtrim(AH.APPLICANTID)                    as P_Addr_APPLICANT")
            '否認コメント
            sqlStat.AppendLine("         ,AH.APPROVEDTEXT                          as P_AP_APPLYTEXT")
            '承認時付与情報
            sqlStat.AppendLine("         ,CD.PAYABLE                               as P_AP_PAYABLE")
            sqlStat.AppendLine("         ,CD.RECEIVABLE                            as P_AP_RECEIVABLE")
            sqlStat.AppendLine("         ,CD.NETSETTLEMENTDUE                      as P_AP_NETSETTLEMENTDUE")
            sqlStat.AppendFormat(" FROM {0} CD ", TBL_CLOSED).AppendLine()
            'JOT名称JOIN
            sqlStat.AppendFormat(" INNER JOIN {0} VJ ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VJ.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendFormat("     AND VJ.MORG = '{0}' ", JOTORG).AppendLine()
            sqlStat.AppendLine("       AND VJ.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VJ.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VJ.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            '申請オフィス名JOIN
            sqlStat.AppendFormat(" LEFT JOIN {0} VO ", TBL_VENDER).AppendLine()
            sqlStat.AppendFormat("     ON  VO.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND VO.CARRIERCODE = CD.APPLYOFFICE ")
            sqlStat.AppendLine("       AND VO.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND VO.DELFLG <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendFormat("     AND VO.CLASS = '{0}' ", C_TRADER.CLASS.AGENT).AppendLine()
            '申請履歴JOIN
            sqlStat.AppendFormat(" INNER JOIN {0} AH ", TBL_A_HIST).AppendLine()
            sqlStat.AppendFormat("     ON  AH.COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat.AppendLine("       AND RTRIM(AH.APPLYID) = RTRIM(CD.APPLYID) ")
            sqlStat.AppendFormat("     AND AH.STEP = '{0}' ", APPLYSTEP).AppendLine()
            sqlStat.AppendLine("       AND AH.DELFLG <> '" & CONST_FLAG_YES & "' ")
            '申請者名JOIN
            sqlStat.AppendFormat(" INNER JOIN {0} US ", TBL_USER).AppendLine()
            sqlStat.AppendLine("       ON  US.USERID = AH.APPLICANTID ")
            sqlStat.AppendLine("       AND US.STYMD <= @NOWDATE ")
            sqlStat.AppendLine("       AND US.ENDYMD >= @NOWDATE ")
            sqlStat.AppendLine("       AND US.DELFLG <> '" & CONST_FLAG_YES & "' ")
            '国名JOIN
            sqlStat.AppendFormat(" LEFT JOIN {0} CNRY ", TBL_COUNTRY).AppendLine()
            sqlStat.AppendLine("       ON  CNRY.COUNTRYCODE = CD.COUNTRYCODE ")
            sqlStat.AppendLine("       AND CNRY.STYMD      <= @NOWDATE ")
            sqlStat.AppendLine("       AND CNRY.ENDYMD     >= @NOWDATE ")
            sqlStat.AppendLine("       AND CNRY.DELFLG     <> '" & CONST_FLAG_YES & "' ")
            sqlStat.AppendLine("   WHERE RTRIM(CD.APPLYID)   = @APPLYID ")
            sqlStat.AppendLine("     AND CD.INITYMD   = (SELECT MAX(CDS.INITYMD) ")
            sqlStat.AppendLine("                           FROM GBT0006_CLOSINGDAY CDS")
            sqlStat.AppendLine("                          WHERE RTRIM(CDS.APPLYID)   = @APPLYID) ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                With sqlCmd.Parameters
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@APPLYID", System.Data.SqlDbType.NVarChar).Value = APPLYID
                End With

                Using sqlda As New SqlDataAdapter(sqlCmd)
                    sqlda.Fill(paraTable)
                End Using

            End Using

            'メール設定取得
            'SQL文の作成
            Dim sqlStat2 As New System.Text.StringBuilder
            sqlStat2.AppendLine("   SELECT ")
            sqlStat2.AppendLine("     COMPCODE, SYSTEMCODE, EVENTCODE, SUBCODE, ")
            sqlStat2.AppendLine("     TOADDRESS, CC, BCC, REPLYTO, SUBJECT, BODY ")
            sqlStat2.AppendFormat(" FROM {0} ", TBL_MAST).AppendLine()
            sqlStat2.AppendFormat(" WHERE COMPCODE = '{0}' ", GBC_COMPCODE).AppendLine()
            sqlStat2.AppendFormat(" AND   SYSTEMCODE = '{0}' ", COA0019Session.SYSCODE).AppendLine()
            sqlStat2.AppendLine("   AND   EVENTCODE  = @EVENTCODE ")
            sqlStat2.AppendLine("   AND   SUBCODE  = rtrim(@SUBCODE) ")
            sqlStat2.AppendLine("   AND   STYMD <= @NOWDATE ")
            sqlStat2.AppendLine("   AND   ENDYMD >= @NOWDATE ")
            sqlStat2.AppendLine("   AND   DELFLG  <> '" & CONST_FLAG_YES & "' ")

            Using sqlConn2 As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd2 As New SqlCommand(sqlStat2.ToString, sqlConn2)
                sqlConn2.Open()
                With sqlCmd2.Parameters
                    .Add("@EVENTCODE", System.Data.SqlDbType.NVarChar).Value = EVENTCODE
                    .Add("@SUBCODE", System.Data.SqlDbType.NVarChar).Value = MAILSUBCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With

                Using sqlda2 As New SqlDataAdapter(sqlCmd2)
                    sqlda2.Fill(baseTable)
                End Using

            End Using

            ' パラメータ展開
            Dim workTOADDRESS As String = ""
            Dim workCC As String = ""
            Dim workBCC As String = ""
            Dim workREPLYTO As String = ""
            Dim workSUBJECT As String = ""
            Dim workBODY As String = ""

            ' 宛先関連(TOADDRESS, CC, BCC, REPLYTO)
            Dim keyAddress As String() = New String() {"TOADDRESS", "CC", "BCC", "REPLYTO"}
            Dim dicAddress As New Dictionary(Of String, String)(keyAddress.Length)

            Dim workAddress As String
            For Each key In keyAddress

                workAddress = baseTable.Rows(0).Item(key).ToString
                workAddress = workAddress.Replace("P_Addr_JOT", paraTable.Rows(0).Item("P_Addr_JOT").ToString)
                workAddress = workAddress.Replace("P_Addr_APPLICANT", paraTable.Rows(0).Item("P_US_EMAIL").ToString)
                workAddress = workAddress.Replace("P_Addr_COUNTRY", paraTable.Rows(0).Item("P_Addr_COUNTRY").ToString)
                dicAddress(key) = workAddress

            Next

            ' 件名、本文関連(SUBJECT, BODY)
            Dim keyText As String() = New String() {"SUBJECT", "BODY"}
            Dim dicText As New Dictionary(Of String, String)(keyText.Length)

            Dim workText As String
            For Each keyT In keyText
                workText = baseTable.Rows(0).Item(keyT).ToString
                For Each col As DataColumn In paraTable.Columns
                    workText = workText.Replace(col.ColumnName, paraTable.Rows(0).Item(col.ColumnName).ToString)
                Next
                dicText(keyT) = workText
            Next

            'メール設定
            COA0033Mail.I_COMPCODE = GBC_COMPCODE
            COA0033Mail.I_SYSCODE = COA0019Session.SYSCODE
            COA0033Mail.I_EVENTCODE = EVENTCODE
            COA0033Mail.I_SUBCODE = MAILSUBCODE
            COA0033Mail.I_ID = paraTable.Rows(0).Item("P_MAILHISTORY_ID").ToString
            COA0033Mail.I_TOADDRESS = dicAddress("TOADDRESS")
            COA0033Mail.I_CC = dicAddress("CC")
            COA0033Mail.I_BCC = dicAddress("BCC")
            COA0033Mail.I_REPLYTO = dicAddress("REPLYTO")
            COA0033Mail.I_SUBJECT = dicText("SUBJECT")
            COA0033Mail.I_BODY = dicText("BODY")
            COA0033Mail.COA0033setMailSend()

            Me.ERR = C_MESSAGENO.NORMAL

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub
End Structure

Public Structure GBA00010ExRate

    ''' <summary>
    ''' 国コード
    ''' </summary>
    ''' <returns></returns>
    Public Property COUNTRYCODE As String
    ''' <summary>
    ''' [IN]指定年月(yyyy/MM形式)
    ''' </summary>
    ''' <returns></returns>
    Public Property TARGETYM As String
    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' 為替レートテーブル
    ''' </summary>
    ''' <returns></returns>
    Public Property EXRATE_TABLE As DataTable
    ''' <summary>
    ''' [OUT]先頭行の為替レート
    ''' </summary>
    ''' <returns></returns>
    Public Property EXRATEFIRSTROW As String

    ''' <summary>
    ''' 為替レート情報取得
    ''' </summary>
    Public Sub getExRateInfo()

        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim sqlStat As New StringBuilder

        Try
            sqlStat.AppendLine("SELECT *")
            sqlStat.AppendLine("  FROM GBM0020_EXRATE ")
            sqlStat.AppendLine(" WHERE COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND COUNTRYCODE  = @COUNTRYCODE")
            sqlStat.AppendLine("   AND STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
            If Me.TARGETYM IsNot Nothing OrElse Me.TARGETYM <> "" Then
                sqlStat.AppendLine("   AND  TARGETYM = @TARGETYM")
            End If

            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                EXRATE_TABLE = New DataTable
                sqlCon.Open() '接続オープン
                'SQLパラメータ設定
                Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar)
                Dim paramCountry As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar)
                Dim paramStYmd As SqlParameter = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
                Dim paramEndYmd As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
                Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                Dim paramTargetYm As SqlParameter = Nothing
                'パラメータに値設定
                paramCompCode.Value = COA0019Session.APSRVCamp
                paramCountry.Value = COUNTRYCODE
                paramStYmd.Value = Date.Now
                paramEndYmd.Value = Date.Now
                paramDelFlg.Value = CONST_FLAG_YES

                If Me.TARGETYM IsNot Nothing OrElse Me.TARGETYM <> "" Then
                    paramTargetYm = sqlCmd.Parameters.Add("@TARGETYM", SqlDbType.Date)
                    paramTargetYm.Value = Me.TARGETYM & "/01"
                End If

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(EXRATE_TABLE)
                End Using

                If EXRATE_TABLE.Rows.Count > 0 Then
                    Me.EXRATEFIRSTROW = Convert.ToString(Me.EXRATE_TABLE.Rows(0).Item("EXRATE"))
                    Me.ERR = C_MESSAGENO.NORMAL
                Else
                    Me.EXRATEFIRSTROW = "0"
                    Me.ERR = C_MESSAGENO.NODATA
                End If

            End Using

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

End Structure

''' <summary>
''' <para>申請ID取得</para>
''' <para>入力プロパティ(COMPCODE,SYSCODE,CLASS,KEYCODE)</para>
''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
''' </summary>
Structure GBA00011ApplyID

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <returns></returns>
    Public Property COMPCODE As String

    ''' <summary>
    ''' システムコード
    ''' </summary>
    ''' <returns></returns>
    Public Property SYSCODE As String

    ''' <summary>
    ''' マスタキー
    ''' </summary>
    ''' <returns></returns>
    Public Property KEYCODE As String

    ''' <summary>
    ''' 申請区分（1桁）
    ''' </summary>
    ''' <returns></returns>
    Public Property DIVISION As String

    ''' <summary>
    ''' シーケンスオブジェクトID
    ''' </summary>
    ''' <returns></returns>
    Public Property SEQOBJID As String

    ''' <summary>
    ''' シーケンス桁数
    ''' </summary>
    ''' <returns></returns>
    Public Property SEQLEN As Integer

    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' 申請ID
    ''' </summary>
    ''' <returns></returns>
    Public Property APPLYID As String

    Const TBL_FIXVALUE As String = "COS0017_FIXVALUE"
    Const TBL_APPROVAL As String = "COS0022_APPROVAL"
    Const TBL_APPROVALHIST As String = "COT0002_APPROVALHIST"
    Const SEQ_APPLY As String = "COQ0001_APPLY"
    Const SUB_COMMON As String = "Common"

    ''' <summary>
    ''' <para>申請ID取得</para>
    ''' <para>入力プロパティ(COMPCODE,SYSCODE,CLASS,KEYCODE)</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00011getApplyID()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim retValue As String = ""
        Dim errMessage As String

        Try

            If IsNothing(COMPCODE) And COMPCODE = "" Then
                ERR = C_MESSAGENO.DLLIFERROR

                COA0000DllMessage.MessageCode = ERR
                COA0000DllMessage.COA0000GetMesssage()
                If (COA0019Session.LANGLOG <> C_LANG.JA) Then
                    errMessage = COA0000DllMessage.MessageStrEN
                Else
                    errMessage = COA0000DllMessage.MessageStrJA
                End If

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = errMessage & "(COMPCODE)"
                COA0003LogFile.MESSAGENO = ERR
                COA0003LogFile.COA0003WriteLog()
                Return
            End If

            If IsNothing(SYSCODE) And SYSCODE = "" Then
                ERR = C_MESSAGENO.DLLIFERROR

                COA0000DllMessage.MessageCode = ERR
                COA0000DllMessage.COA0000GetMesssage()
                If (COA0019Session.LANGLOG <> C_LANG.JA) Then
                    errMessage = COA0000DllMessage.MessageStrEN
                Else
                    errMessage = COA0000DllMessage.MessageStrJA
                End If

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = errMessage & "(SYSCODE)"
                COA0003LogFile.MESSAGENO = ERR
                COA0003LogFile.COA0003WriteLog()
                Return
            End If

            If IsNothing(KEYCODE) And KEYCODE = "" Then
                ERR = C_MESSAGENO.DLLIFERROR

                COA0000DllMessage.MessageCode = ERR
                COA0000DllMessage.COA0000GetMesssage()
                If (COA0019Session.LANGLOG <> C_LANG.JA) Then
                    errMessage = COA0000DllMessage.MessageStrEN
                Else
                    errMessage = COA0000DllMessage.MessageStrJA
                End If

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = errMessage & "(KEYCODE)"
                COA0003LogFile.MESSAGENO = ERR
                COA0003LogFile.COA0003WriteLog()
                Return
            End If

            If IsNothing(SEQOBJID) And SEQOBJID = "" Then
                ERR = C_MESSAGENO.DLLIFERROR

                COA0000DllMessage.MessageCode = ERR
                COA0000DllMessage.COA0000GetMesssage()
                If (COA0019Session.LANGLOG <> C_LANG.JA) Then
                    errMessage = COA0000DllMessage.MessageStrEN
                Else
                    errMessage = COA0000DllMessage.MessageStrJA
                End If

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = errMessage & "(SEQOBJID)"
                COA0003LogFile.MESSAGENO = ERR
                COA0003LogFile.COA0003WriteLog()
                Return
            End If

            If IsNothing(SEQLEN) And Not IsNumeric(SEQLEN) Then
                ERR = C_MESSAGENO.DLLIFERROR

                COA0000DllMessage.MessageCode = ERR
                COA0000DllMessage.COA0000GetMesssage()
                If (COA0019Session.LANGLOG <> C_LANG.JA) Then
                    errMessage = COA0000DllMessage.MessageStrEN
                Else
                    errMessage = COA0000DllMessage.MessageStrJA
                End If

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = errMessage & "(SEQLEN)"
                COA0003LogFile.MESSAGENO = ERR
                COA0003LogFile.COA0003WriteLog()
                Return
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine("   SELECT ")
            sqlStat.AppendFormat("     'AP{0}' ", DIVISION).AppendLine()
            sqlStat.AppendLine("       + LEFT(CONVERT(char,getdate(),12),4) ")
            sqlStat.AppendLine("       + '_' ")
            sqlStat.AppendFormat("     + RIGHT('0000000000' + TRIM(CONVERT(char,NEXT VALUE FOR {0})),{1}) ", SEQOBJID, SEQLEN).AppendLine()
            sqlStat.AppendLine("       + '_' ")
            sqlStat.AppendLine("       + ( ")
            sqlStat.AppendFormat("        SELECT VALUE1 FROM {0} ", TBL_FIXVALUE).AppendLine()
            sqlStat.AppendLine("          WHERE COMPCODE = @P1 ")
            sqlStat.AppendLine("          AND   SYSCODE  = @P2 ")
            sqlStat.AppendLine("          AND   CLASS    = '" & C_SERVERSEQ & "' ")
            sqlStat.AppendLine("          AND   KEYCODE  = @P3 ")
            sqlStat.AppendLine("          AND   STYMD    <= @P4 ")
            sqlStat.AppendLine("          AND   ENDYMD   >= @P4 ")
            sqlStat.AppendLine("          AND   DELFLG   = @P5 ")
            sqlStat.AppendLine("         ) as APPLYID ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 20)
                Dim PARA3 As SqlParameter = sqlCmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 20)
                Dim PARA4 As SqlParameter = sqlCmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                Dim PARA5 As SqlParameter = sqlCmd.Parameters.Add("@P5", System.Data.SqlDbType.Char, 1)
                PARA1.Value = Me.COMPCODE
                PARA2.Value = Me.SYSCODE
                PARA3.Value = Me.KEYCODE
                PARA4.Value = Date.Now
                PARA5.Value = CONST_FLAG_NO
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    While sqlDr.Read
                        retValue = Convert.ToString(sqlDr("APPLYID"))
                        Exit While
                    End While
                End Using
                ERR = C_MESSAGENO.NORMAL

            End Using
            Me.APPLYID = retValue

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE                          '
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

End Structure
''' <summary>
''' タンク情報取得
''' </summary>
Public Structure GBA00012TankInfo
    Private Const CONST_TBL_CONTRACT As String = "GBT0010_LBR_CONTRACT"
    Private Const CONST_TBL_AGREEMENT As String = "GBT0011_LBR_AGREEMENT"
    Private Const CONST_TBL_LEASETANK As String = "GBT0012_RESRVLEASETANK"


    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String

    ''' <summary>
    ''' ListBox(タンク)
    ''' </summary>
    ''' <returns></returns>
    Public Property LISTBOX_TANK As ListBox
    ''' <summary>
    ''' [OUT]タンクステータス用テーブル
    ''' </summary>
    ''' <returns>データテーブル</returns>
    ''' <remarks>タンク動静で使用するデータテーブル
    ''' GBA00012getTankStatusTableを実行の上生成
    ''' </remarks>
    Public Property TANKSTATUS_TABLE As DataTable
    ''' <summary>
    ''' [IN]引当対象か(0:全て取得(デフォルト),1:引当対象のみ取得(通常),2:協定書リースタンク引当,
    ''' 3:リース起因セールスタンク引当,4:リースアウトタンク引当,5:リースインタンク引当)
    ''' </summary>
    ''' <returns></returns>
    Public Property ISALLOCATEONLY As Integer
    ''' <summary>
    ''' [IN]引当オーダー番号
    ''' </summary>
    ''' <returns></returns>
    Public Property ALLOCATEORDERNO As String
    ''' <summary>
    ''' [IN]引当済タンクNo
    ''' </summary>
    ''' <returns></returns>
    Public Property TANKNOLIST As List(Of String)
    ''' <summary>
    ''' [IN]国コード
    ''' </summary>
    ''' <returns>[IN]国コード</returns>
    ''' <remarks>GBA00012getTankStatusTableの条件で利用する国コード(未指定時はすべて)</remarks>
    Public Property COUNTRYCODE As String
    ''' <summary>
    ''' [IN]荷主コード
    ''' </summary>
    ''' <returns></returns>
    Public Property SHIPPERCODE As String
    ''' <summary>
    ''' [IN]積載品コード
    ''' </summary>
    ''' <returns></returns>
    Public Property PRODUCTCODE As String
    ''' <summary>
    ''' [IN]オーガナイザー
    ''' </summary>
    ''' <returns></returns>
    Public Property AGENTORGANIZER As String
    ''' <summary>
    ''' [IN]リペアフラグ
    ''' </summary>
    ''' <returns></returns>
    Public Property REPFLG As String
    ''' <summary>
    ''' [IN]発地港
    ''' </summary>
    ''' <returns></returns>
    Public Property POLPORT As String

    ''' <summary>
    ''' <para>タンクリスト取得</para>
    ''' <para>出力プロパティ(ERR(処理結果コード):正常終了("00000")、以外エラー)</para>
    ''' </summary>
    Public Sub GBA00012getLeftListTank()

        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim errMessage As String = Nothing

        Try

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            sqlStat.AppendLine(" SELECT ")
            sqlStat.AppendLine("       TANKNO               ")
            sqlStat.AppendLine(" FROM  GBM0006_TANK         ")
            sqlStat.AppendLine(" WHERE   STYMD       <= @P1 ")
            sqlStat.AppendLine("   AND   ENDYMD      >= @P1 ")
            sqlStat.AppendLine("   AND   DELFLG       = @P2 ")
            sqlStat.AppendLine(" ORDER BY TANKNO ")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
                sqlConn.Open()
                Dim PARA1 As SqlParameter = sqlCmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = sqlCmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 1)
                PARA1.Value = Date.Now
                PARA2.Value = CONST_FLAG_NO

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    While sqlDr.Read
                        Dim listitem = New ListItem(Convert.ToString(sqlDr("TANKNO")))
                        LISTBOX_TANK.Items.Add(listitem)
                    End While
                End Using
            End Using

            If Me.LISTBOX_TANK.Items.Count > 0 Then
                ERR = C_MESSAGENO.NORMAL
            Else
                ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception

            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try

    End Sub

    ''' <summary>
    ''' タンク動静用のデータテーブルを作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GBA00012getTankStatusTable()
        Dim COA0000DllMessage As New COA0000DllMessage
        Dim COA0003LogFile As New COA0003LogFile                    'LogOutput DirString Get
        Dim errMessage As String = Nothing
        TANKSTATUS_TABLE = New DataTable
        Try
            If Me.COUNTRYCODE Is Nothing Then
                Me.COUNTRYCODE = ""
            End If
            If Me.ALLOCATEORDERNO Is Nothing Then
                Me.ALLOCATEORDERNO = ""
            End If
            If Me.SHIPPERCODE Is Nothing Then
                Me.SHIPPERCODE = ""
            End If
            If Me.PRODUCTCODE Is Nothing Then
                Me.PRODUCTCODE = ""
            End If
            If Me.AGENTORGANIZER Is Nothing Then
                Me.AGENTORGANIZER = ""
            End If
            If Me.REPFLG Is Nothing Then
                Me.REPFLG = ""
            End If
            If Me.POLPORT Is Nothing Then
                Me.POLPORT = ""
            End If

            'SQL文の作成
            Dim sqlStat As New System.Text.StringBuilder
            '共通テーブル定義START
            sqlStat.AppendLine("with")
            'ベースはタンクマスタ
            sqlStat.AppendLine("WITH_BASE as (")
            sqlStat.AppendLine("  select TM.TANKNO,TM.REPAIRSTAT, TM.TANKCAPACITY, TM.NETWEIGHT,TM.JAPFIREAPPROVED,TM.NOMINALCAPACITY,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when TM.INSPECTDATE2P5 <> @InitDate then convert(char,TM.INSPECTDATE2P5,111)")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as T2_5ACT,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when TM.INSPECTDATE5 <> @InitDate then convert(char,TM.INSPECTDATE5,111)")
            sqlStat.AppendLine("       end as T5ACT,")
            sqlStat.AppendLine("       TM.NEXTINSPECTTYPE as T_NEXTTYPE,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when TM.NEXTINSPECTDATE <> @InitDate then convert(char,TM.NEXTINSPECTDATE,111)")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as T_NEXTDATE,")
            sqlStat.AppendLine("       isnull(POD.ORDERNO,'') as POD_ORDERNO, POD.DTLPOLPOD as POD_DTLPOLPOD, POD.DTLOFFICE as POD_DTLOFFICE, POD.ACTUALDATE as POD_ACTUALDATE, POD.ACTIONID as POD_ACTIONID, POD.DTLPOLPOD2 as  POD_DTLPOLPOD2, POD.DATEFIELD as  POD_DATEFIELD,")
            sqlStat.AppendLine("       case POD.DTLPOLPOD")
            sqlStat.AppendLine("         when 'POL1' then POD_B.LOADCOUNTRY1")
            sqlStat.AppendLine("         when 'POL2' then POD_B.LOADCOUNTRY2")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as POD_POLCOUNTRY,")
            sqlStat.AppendLine("       case POD.DTLPOLPOD")
            sqlStat.AppendLine("         when 'POL1' then POD_B.LOADPORT1")
            sqlStat.AppendLine("         when 'POL2' then POD_B.LOADPORT2")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as POD_POLPORT,")
            sqlStat.AppendLine("       case POD.DTLPOLPOD")
            sqlStat.AppendLine("         when 'POL1' then POD_B.DISCHARGECOUNTRY1")
            sqlStat.AppendLine("         when 'POL2' then POD_B.DISCHARGECOUNTRY2")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as POD_PODCOUNTRY,")
            sqlStat.AppendLine("       case POD.DTLPOLPOD")
            sqlStat.AppendLine("         when 'POL1' then POD_B.DISCHARGEPORT1")
            sqlStat.AppendLine("         when 'POL2' then POD_B.DISCHARGEPORT2")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as POD_PODPORT,")
            sqlStat.AppendLine("       POD_B.TIP as POD_TIP,")
            sqlStat.AppendLine("       POL.ORDERNO as POL_ORDERNO, POL.TANKSEQ as POL_TANKSEQ, POL.DTLPOLPOD as POL_DTLPOLPOD, POL.DTLOFFICE as POL_DTLOFFICE,")
            sqlStat.AppendLine("       isnull(convert(char,POL.SCHEDELDATE,111),'') as POL_SCHEDELDATE")
            sqlStat.AppendLine("       ,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when POL.ORDERNO <> isnull(POD.ORDERNO,'') then isnull(POL_B.LOADCOUNTRY1,'')")
            sqlStat.AppendLine("         when POL.ORDERNO = isnull(POD.ORDERNO,'') then isnull(POL_B.LOADCOUNTRY2,'')")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as POL_POLCOUNTRY,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when POL.ORDERNO <> isnull(POD.ORDERNO,'') then isnull(POL_B.LOADPORT1,'')")
            sqlStat.AppendLine("         when POL.ORDERNO = isnull(POD.ORDERNO,'') then isnull(POL_B.LOADPORT2,'')")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as POL_POLPORT,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when POL.ORDERNO <> isnull(POD.ORDERNO,'') then isnull(POL_B.DISCHARGECOUNTRY1,'')")
            sqlStat.AppendLine("         when POL.ORDERNO = isnull(POD.ORDERNO,'') then isnull(POL_B.DISCHARGECOUNTRY2,'')")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as POL_PODCOUNTRY,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when POL.ORDERNO <> isnull(POD.ORDERNO,'') then isnull(POL_B.DISCHARGEPORT1,'')")
            sqlStat.AppendLine("         when POL.ORDERNO = isnull(POD.ORDERNO,'') then isnull(POL_B.DISCHARGEPORT2,'')")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as POL_PODPORT,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("          when POL.DATEFIELD = 'ETD' then 'ETA'")
            sqlStat.AppendLine("          when POL.DATEFIELD = 'ETD1' then 'ETA1'")
            sqlStat.AppendLine("          when POL.DATEFIELD = 'ETD2' then 'ETA2'")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as POL_ETADATAFIELD")
            sqlStat.AppendLine("  from   GBM0006_TANK TM with(nolock) ")
            '直近着地
            sqlStat.AppendLine("  left outer join (")
            sqlStat.AppendLine("    select RANK() OVER(PARTITION BY OVPOD.TANKNO ORDER BY OVPOD.TANKNO, OVPOD.ACTUALDATE desc, OVPOD.DTLPOLPOD desc, OVPOD.DISPSEQ  desc) AS ORDERSORT,")
            sqlStat.AppendLine("    OVPOD.TANKNO, OVPOD.ORDERNO, OVPOD.DTLPOLPOD, OVPOD.DTLOFFICE, OVPOD.ACTUALDATE, OVPOD.ACTIONID,")
            sqlStat.AppendLine("    case OVPOD.DTLPOLPOD")
            sqlStat.AppendLine("      when 'POL1' then 'POD1'")
            sqlStat.AppendLine("      when 'POL2' then 'POD2'")
            sqlStat.AppendLine("    end as DTLPOLPOD2,")
            sqlStat.AppendLine("    case OVPOD.DATEFIELD")
            sqlStat.AppendLine("      when 'ETD' then 'ETA'")
            sqlStat.AppendLine("      when 'ETD1' then 'ETA1'")
            sqlStat.AppendLine("      when 'ETD2' then 'ETA2'")
            sqlStat.AppendLine("    end as DATEFIELD")
            sqlStat.AppendLine("    from GBT0005_ODR_VALUE as OVPOD with(nolock) ")
            sqlStat.AppendLine("    where OVPOD.DATEFIELD in ('ETD','ETD1','ETD2')")
            sqlStat.AppendLine("    and   OVPOD.ACTUALDATE <> @InitDate")
            sqlStat.AppendLine("    and   OVPOD.DELFLG <> @DelFlg")
            sqlStat.AppendLine("    ) as POD")
            sqlStat.AppendLine("    ON POD.TANKNO = TM.TANKNO")
            sqlStat.AppendLine("    and   POD.ORDERSORT = 1")
            'オーダーベース(着地)
            sqlStat.AppendLine("  left outer join GBT0004_ODR_BASE as POD_B with(nolock)")
            sqlStat.AppendLine("    on POD_B.ORDERNO = POD.ORDERNO")
            sqlStat.AppendLine("    and POD_B.DELFLG <> @DelFlg")
            '着地後の発地
            sqlStat.AppendLine("  left outer join (")
            sqlStat.AppendLine("    select RANK() OVER(PARTITION BY OVPOL.TANKNO ORDER BY OVPOL.TANKNO, OVPOL.SCHEDELDATE) AS ORDERSORT,")
            sqlStat.AppendLine("    OVPOL.ORDERNO, OVPOL.TANKNO, OVPOL.TANKSEQ, OVPOL.DTLPOLPOD, OVPOL.DTLOFFICE, OVPOL.SCHEDELDATE, OVPOL.ACTUALDATE, OVPOL.DATEFIELD")
            sqlStat.AppendLine("    from GBT0005_ODR_VALUE as OVPOL with(nolock) ")
            sqlStat.AppendLine("    where OVPOL.DATEFIELD in ('ETD','ETD1','ETD2')")
            sqlStat.AppendLine("    and   OVPOL.SCHEDELDATE <> @InitDate")
            sqlStat.AppendLine("    and   OVPOL.ACTUALDATE = @InitDate")
            sqlStat.AppendLine("    and   OVPOL.DELFLG <> @DelFlg")
            sqlStat.AppendLine("    ) as POL")
            sqlStat.AppendLine("    ON POL.TANKNO = TM.TANKNO")
            sqlStat.AppendLine("    and   POL.ORDERSORT = 1")
            'and POL.TANKNO = POD.TANKNO
            sqlStat.AppendLine("    and   ( POL.SCHEDELDATE > isnull(POD.ACTUALDATE,@InitDate) or POL.ACTUALDATE > isnull(POD.ACTUALDATE,@InitDate) )")
            'オーダーベース(発地)
            sqlStat.AppendLine("  left outer join GBT0004_ODR_BASE as POL_B with(nolock)")
            sqlStat.AppendLine("    on POL_B.ORDERNO = POL.ORDERNO")
            sqlStat.AppendLine("    and  POL_B.DELFLG <> @DelFlg")
            sqlStat.AppendLine("  where  TM.COMPCODE = '01'")
            sqlStat.AppendLine("  and    TM.DELFLG <> @DelFlg")
            sqlStat.AppendLine("  and    TM.STYMD <= getdate()")
            sqlStat.AppendLine("  and    TM.ENDYMD >= getdate()")
            sqlStat.AppendLine(")")
            '直近ステータス
            sqlStat.AppendLine(",WITH_STATUS as (")
            sqlStat.AppendLine("    select")
            'sqlStat.AppendLine("        RANK() OVER(PARTITION BY OVSTAT.TANKNO ORDER BY (CASE WHEN OVSTAT.ACTUALDATE = @InitDate THEN '9999/12/12' else OVSTAT.ACTUALDATE END) desc, OVSTAT.DISPSEQ desc) as RECENT,")
            sqlStat.AppendLine("        RANK() OVER(PARTITION BY OVSTAT.TANKNO ORDER BY (CASE WHEN OVSTAT.ACTUALDATE = @InitDate THEN '9999/12/12' else OVSTAT.ACTUALDATE END) desc, convert(char(10),OVSTAT.INITYMD,111) desc, OVSTAT.DISPSEQ desc) as RECENT,")
            sqlStat.AppendLine("        OVSTAT.TANKNO, OVSTAT.ACTIONID,OVSTAT.ACTUALDATE,OVSTAT.DISPSEQ")
            sqlStat.AppendLine("    from GBT0005_ODR_VALUE as OVSTAT with(nolock) ")
            sqlStat.AppendLine("    where OVSTAT.DELFLG <> @DelFlg")
            sqlStat.AppendLine("    and   OVSTAT.TANKNO <> ''")
            sqlStat.AppendLine("    and   OVSTAT.ACTIONID <> ''")
            sqlStat.AppendLine("    and  ((OVSTAT.ACTUALDATE <> @InitDate ) or ( (OVSTAT.ACTIONID = 'TKAL' or OVSTAT.ACTIONID = 'TAED' or OVSTAT.ACTIONID = 'TAEC') and  OVSTAT.ACTUALDATE =  @InitDate))")
            sqlStat.AppendLine(")")
            '直近３積載品
            sqlStat.AppendLine(",WITH_P3HIST as (")
            sqlStat.AppendLine("    select TANKNO,[1] as HIST1,[2] as HIST2,[3] as HIST3")
            sqlStat.AppendLine("    from")
            sqlStat.AppendLine("    (")
            sqlStat.AppendLine("        select RANK() OVER(PARTITION BY OV.TANKNO ORDER BY OV.ACTUALDATE desc) as RECENT,")
            sqlStat.AppendLine("               OV.TANKNO, P.PRODUCTNAME")
            sqlStat.AppendLine("        from GBT0005_ODR_VALUE as OV with(nolock) ")
            sqlStat.AppendLine("        inner join GBT0004_ODR_BASE OB with(nolock)")
            sqlStat.AppendLine("        on OB.ORDERNO = OV.ORDERNO")
            sqlStat.AppendLine("        and OB.DELFLG <> @DelFlg")
            sqlStat.AppendLine("        inner join GBM0008_PRODUCT P with(nolock)")
            sqlStat.AppendLine("        on P.COMPCODE = '01'")
            sqlStat.AppendLine("        and P.PRODUCTCODE = OB.PRODUCTCODE")
            sqlStat.AppendLine("        and P.DELFLG <> @DelFlg")
            sqlStat.AppendLine("        where OV.ACTIONID = 'LOAD'")
            sqlStat.AppendLine("        and   OV.DELFLG <> @DelFlg")
            sqlStat.AppendLine("        and   OV.ACTUALDATE <> @InitDate")
            sqlStat.AppendLine("    ) as RECNT_LOAD")
            sqlStat.AppendLine("    PIVOT (")
            sqlStat.AppendLine("        max(PRODUCTNAME) for RECENT in ([1],[2],[3])")
            sqlStat.AppendLine("    ) as PivotTable")
            sqlStat.AppendLine(")")
            'デポイン
            sqlStat.AppendLine(",WITH_DEPOTIN as (")
            sqlStat.AppendLine("  select")
            sqlStat.AppendLine("    RANK() OVER(PARTITION BY OVDPIN.ORDERNO,OVDPIN.TANKNO,OVDPIN.DTLPOLPOD ORDER BY min(OVDPIN.ACTUALDATE) ASC) AS ORDERSORT,")
            sqlStat.AppendLine("    OVDPIN.ORDERNO, OVDPIN.TANKNO, OVDPIN.DTLPOLPOD, OVDPIN.CONTRACTORFIX,")
            sqlStat.AppendLine("    isnull(DEPO.DEPOTCODE,'') as DEPOTCODE,")
            sqlStat.AppendLine("    isnull(DEPO.NAMES,'') as NAMES,")
            sqlStat.AppendLine("    isnull(DEPO.LOCATION,'') as LOCATION,")
            sqlStat.AppendLine("    min(OVDPIN.ACTUALDATE) as ACTUALDATE")
            sqlStat.AppendLine("  from GBT0005_ODR_VALUE as OVDPIN with(nolock) ")
            sqlStat.AppendLine("  inner join GBM0010_CHARGECODE CC with(nolock)")
            sqlStat.AppendLine("    on  CC.COMPCODE = '01'")
            sqlStat.AppendLine("    and OVDPIN.COSTCODE = CC.COSTCODE")
            sqlStat.AppendLine("    and '1' = CASE WHEN OVDPIN.DTLPOLPOD LIKE 'POL%' AND CC.LDKBN IN ('B','L') THEN '1' ")
            sqlStat.AppendLine("                   WHEN OVDPIN.DTLPOLPOD LIKE 'POD%' AND CC.LDKBN IN ('B','D') THEN '1' ")
            sqlStat.AppendLine("                   WHEN OVDPIN.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            sqlStat.AppendLine("                   ELSE '1'")
            sqlStat.AppendLine("              END")
            sqlStat.AppendLine("    and CC.CLASS4 = @DeptClass")
            sqlStat.AppendLine("  left outer join GBM0003_DEPOT as DEPO with(nolock)")
            sqlStat.AppendLine("    on  DEPO.COMPCODE = '01'")
            sqlStat.AppendLine("    and DEPO.DEPOTCODE = OVDPIN.CONTRACTORFIX")
            sqlStat.AppendLine("  where OVDPIN.ACTUALDATE <> @InitDate")
            sqlStat.AppendLine("  and   OVDPIN.DELFLG <> @DelFlg")
            sqlStat.AppendLine("  group by OVDPIN.ORDERNO,OVDPIN.TANKNO,OVDPIN.DTLPOLPOD,OVDPIN.CONTRACTORFIX,DEPO.DEPOTCODE,DEPO.NAMES,DEPO.LOCATION")
            sqlStat.AppendLine(")")
            'デポアウト
            sqlStat.AppendLine(", WITH_DEPOTOUT as (")
            sqlStat.AppendLine("  select")
            sqlStat.AppendLine("    RANK() OVER(PARTITION BY OVDPOUT.ORDERNO,OVDPOUT.TANKNO ORDER BY min(OVDPOUT.ACTUALDATE) ASC) AS ORDERSORT,")
            sqlStat.AppendLine("    OVDPOUT.ORDERNO, OVDPOUT.TANKNO, OVDPOUT.DTLPOLPOD, OVDPOUT.CONTRACTORFIX,")
            sqlStat.AppendLine("    isnull(DEPO.DEPOTCODE,'') as DEPOTCODE,")
            sqlStat.AppendLine("    isnull(DEPO.NAMES,'') as NAMES,")
            sqlStat.AppendLine("    isnull(DEPO.LOCATION,'') as LOCATION,")
            sqlStat.AppendLine("    min(OVDPOUT.ACTUALDATE) as ACTUALDATE")
            sqlStat.AppendLine("  from GBT0005_ODR_VALUE as OVDPOUT  with(nolock) ")
            sqlStat.AppendLine("  inner join GBM0010_CHARGECODE CC with(nolock)")
            sqlStat.AppendLine("    on  CC.COMPCODE = '01'")
            sqlStat.AppendLine("    and OVDPOUT.COSTCODE = CC.COSTCODE")
            sqlStat.AppendLine("    and '1' = CASE WHEN OVDPOUT.DTLPOLPOD LIKE 'POL%' AND CC.LDKBN IN ('B','L') THEN '1' ")
            sqlStat.AppendLine("                   WHEN OVDPOUT.DTLPOLPOD LIKE 'POD%' AND CC.LDKBN IN ('B','D') THEN '1' ")
            sqlStat.AppendLine("                   WHEN OVDPOUT.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            sqlStat.AppendLine("                   ELSE '1'")
            sqlStat.AppendLine("              END")
            sqlStat.AppendLine("    and CC.CLASS4 = @DeptClass")
            sqlStat.AppendLine("  left outer join GBM0003_DEPOT as DEPO with(nolock)")
            sqlStat.AppendLine("    on  DEPO.COMPCODE = '01'")
            sqlStat.AppendLine("    and DEPO.DEPOTCODE = OVDPOUT.CONTRACTORFIX")
            sqlStat.AppendLine("  where OVDPOUT.ACTUALDATE <> @InitDate")
            sqlStat.AppendLine("  and   OVDPOUT.DELFLG <> @DelFlg")
            sqlStat.AppendLine("  group by OVDPOUT.ORDERNO,OVDPOUT.TANKNO,OVDPOUT.DTLPOLPOD,OVDPOUT.CONTRACTORFIX,DEPO.DEPOTCODE,DEPO.NAMES,DEPO.LOCATION")
            sqlStat.AppendLine(")")
            '申請中
            sqlStat.AppendLine(", WITH_APPLY as (")
            sqlStat.AppendLine("  select")
            sqlStat.AppendLine("    OV2.ORDERNO, OV2.TANKSEQ, OV2.TRILATERAL, OV2.APPLYID, OV2.APPLYTEXT,")
            sqlStat.AppendLine("    AH.STATUS, AH.APPROVEDTEXT")
            sqlStat.AppendLine("  from GBT0007_ODR_VALUE2 as OV2 with(nolock) ")
            sqlStat.AppendLine("  inner join COT0002_APPROVALHIST AH with(nolock)")
            sqlStat.AppendLine("    on AH.COMPCODE = '" & GBC_COMPCODE & "'")
            sqlStat.AppendLine("    and  AH.APPLYID = OV2.APPLYID")
            sqlStat.AppendLine("    and  AH.STEP = OV2.LASTSTEP")
            sqlStat.AppendLine("    and  AH.DELFLG <> '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("  where OV2.DELFLG <> '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("  and   OV2.APPLYID <> ''")
            sqlStat.AppendLine(")")
            'リースタンク
            sqlStat.AppendLine(", WITH_LEASETANK as (")
            sqlStat.AppendLine("  select LTI.TANKNO,LTI.LEASESTYMD,LTI.LEASEENDYMDSCR,LTI.LEASEENDYMD")
            sqlStat.AppendLine("        ,CTR.CONTRACTFROM,CTR.ENABLED")
            sqlStat.AppendLine("        ,AGR.PRODUCTCODE")
            sqlStat.AppendLine("        ,CTR.SHIPPER")
            sqlStat.AppendLine("        ,CTR.ORGANIZER")
            sqlStat.AppendFormat("   from {0} as LTI with(nolock) ", CONST_TBL_LEASETANK).AppendLine()
            sqlStat.AppendFormat("   inner join  {0} AGR with(nolock)", CONST_TBL_AGREEMENT).AppendLine()
            sqlStat.AppendLine("       on AGR.CONTRACTNO = LTI.CONTRACTNO")
            sqlStat.AppendLine("      and AGR.AGREEMENTNO = LTI.AGREEMENTNO")
            sqlStat.AppendLine("      and AGR.DELFLG <> '" & CONST_FLAG_YES & "'")
            sqlStat.AppendFormat("   inner join  {0} CTR with(nolock)", CONST_TBL_CONTRACT).AppendLine()
            sqlStat.AppendLine("       on CTR.CONTRACTNO = LTI.CONTRACTNO")
            sqlStat.AppendLine("      and CTR.DELFLG <> '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("   where LTI.DELFLG <> '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("     and (   LTI.LEASEENDYMD = @InitDate")
            sqlStat.AppendLine("          or LTI.LEASEENDYMD > GETDATE()")
            sqlStat.AppendLine("         )")
            sqlStat.AppendLine(")")
            '共通テーブル定義END
            'タンクステータス
            sqlStat.AppendLine("select B.TANKNO,")
            sqlStat.AppendLine("       isnull(ST.ACTIONID,'') as ACTY,")
            sqlStat.AppendLine("       isnull(convert(char,ST.ACTUALDATE,111),'') as RECENTDATE,")
            sqlStat.AppendLine("       B.REPAIRSTAT AS CANPROVISION,")
            sqlStat.AppendLine("       B.T2_5ACT AS A2_5YTEST,")
            sqlStat.AppendLine("       B.T5ACT AS A5YTEST,")
            sqlStat.AppendLine("       B.T_NEXTTYPE,")
            sqlStat.AppendLine("       B.T_NEXTDATE,")
            sqlStat.AppendLine("       isnull(P3HIST.HIST1,'') as PD_HIST1,")
            sqlStat.AppendLine("       isnull(P3HIST.HIST2,'') as PD_HIST2,")
            sqlStat.AppendLine("       isnull(P3HIST.HIST3,'') as PD_HIST3,")
            sqlStat.AppendLine("       B.NETWEIGHT as TAREWEIGHT,")
            sqlStat.AppendLine("       B.TANKCAPACITY as CAPACITY,")
            sqlStat.AppendLine("       B.NOMINALCAPACITY as TYPE,")
            sqlStat.AppendLine("       B.JAPFIREAPPROVED as FDA,")
            sqlStat.AppendLine("       isnull(B.POD_ACTIONID,'') as POD_ACTIONID,")
            sqlStat.AppendLine("       B.POD_ORDERNO as ORDERNOIN,")
            sqlStat.AppendLine("       B.POD_POLCOUNTRY as POD_POLCOUNTRY,")
            sqlStat.AppendLine("       B.POD_POLPORT as FROMAREA,")
            sqlStat.AppendLine("       B.POD_PODCOUNTRY as POD_PODCOUNTRY,")
            sqlStat.AppendLine("       B.POD_PODPORT as TOAREA,")
            sqlStat.AppendLine("       B.POD_ACTUALDATE  as ETDARR,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when OVETA.ACTUALDATE > @InitDate then convert(char,OVETA.ACTUALDATE,111)")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as ETAARR,")
            sqlStat.AppendLine("       isnull(convert(char,OVDISC.ACTUALDATE,111),'') as DISCHDATE,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when OVETA.ACTUALDATE > @InitDate then convert(char,DATEADD(DAY, B.POD_TIP, OVETA.ACTUALDATE),111)")
            sqlStat.AppendLine("         else ''")
            sqlStat.AppendLine("       end as DEMMSTART,")
            sqlStat.AppendLine("       isnull(convert(char,OVDPIN.ACTUALDATE,111),'') as DEPOTINDATE,")
            sqlStat.AppendLine("       isnull(OVDPIN.DEPOTCODE,'') as DEPO_DEPOTCODE,")
            sqlStat.AppendLine("       isnull(OVDPIN.NAMES,'') as DEPO_NAMES,")
            sqlStat.AppendLine("       isnull(OVDPIN.LOCATION,'') as LOCATION,")
            sqlStat.AppendLine("       isnull(convert(char,ETYC.ACTUALDATE,111),'') as CLEANDATE,")
            sqlStat.AppendLine("       isnull(B.POL_ORDERNO,'') as ORDERNOOUT,")
            sqlStat.AppendLine("       isnull(APPLY.APPLYID,'') as TKAL_APPLYID,")
            sqlStat.AppendLine("       isnull(APPLY.STATUS,'') as TKAL_STATUS,")
            sqlStat.AppendLine("       isnull(APPLY.APPLYTEXT,'') as TKAL_APPLYTEXT,")
            sqlStat.AppendLine("       isnull(APPLY.APPROVEDTEXT,'') as TKAL_APPROVEDTEXT,")
            sqlStat.AppendLine("       case")
            sqlStat.AppendLine("         when OVALLOC.ACTUALDATE <> @InitDate then isnull(convert(char,OVALLOC.ACTUALDATE,111),'')")
            sqlStat.AppendLine("         else isnull(convert(char,OVALLOC.SCHEDELDATE,111),'')")
            sqlStat.AppendLine("       end as ALLOCATIONDATE,")
            sqlStat.AppendLine("       isnull(convert(char,DEPOTOUT.ACTUALDATE,111),'') as DEPOTOUT,")
            sqlStat.AppendLine("       isnull(convert(char,POLLOAD.ACTUALDATE,111),'') as LADENDATE,")
            ' 速度改善
            'sqlStat.AppendLine("       isnull(POLLOAD_P.PRODUCTNAME,'') as NEXTPRODUCT,")
            sqlStat.AppendLine("       isnull(POLLOAD_B.PRODUCTNAME,'') as NEXTPRODUCT,")
            sqlStat.AppendLine("       B.POL_SCHEDELDATE as ETDDATE,")
            sqlStat.AppendLine("       isnull(convert(char,POLETA.SCHEDELDATE,111),'') as ETADATE,")
            sqlStat.AppendLine("       B.POL_POLCOUNTRY,")
            sqlStat.AppendLine("       B.POL_POLPORT,")
            sqlStat.AppendLine("       B.POL_PODCOUNTRY,")
            sqlStat.AppendLine("       B.POL_PODPORT,")
            sqlStat.AppendLine("       isnull(PORTPOD.AREANAME,'') as DESTINATION,")
            sqlStat.AppendLine("       isnull((select '1' from WITH_LEASETANK where WITH_LEASETANK.TANKNO = B.TANKNO group by WITH_LEASETANK.TANKNO),'0')")
            sqlStat.AppendLine("        as LEASETANK")
            'ベース
            sqlStat.AppendLine("from WITH_BASE as B")
            '発地港
            sqlStat.AppendLine("left outer join GBM0002_PORT as PORTPOD with(nolock)")
            sqlStat.AppendLine("on PORTPOD.COMPCODE = '01'")
            sqlStat.AppendLine("and PORTPOD.PORTCODE = B.POL_PODPORT")
            sqlStat.AppendLine("and PORTPOD.COUNTRYCODE = B.POL_PODCOUNTRY")
            sqlStat.AppendLine("and PORTPOD.DELFLG <> @DelFlg")
            '発地予定
            sqlStat.AppendLine("left outer join GBT0005_ODR_VALUE as POLETA with(nolock)")
            sqlStat.AppendLine("on POLETA.ORDERNO = B.POL_ORDERNO")
            sqlStat.AppendLine("and POLETA.TANKNO = B.TANKNO")
            'and POLETA.DTLPOLPOD = B.POL_DTLPOLPOD
            sqlStat.AppendLine("and POLETA.DATEFIELD = B.POL_ETADATAFIELD")
            'and POLETA.ACTUALDATE <> @InitDate
            sqlStat.AppendLine("and  POLETA.DELFLG <> @DelFlg")
            '着地予定
            sqlStat.AppendLine("left outer join GBT0005_ODR_VALUE as OVETA with(nolock)")
            sqlStat.AppendLine("on OVETA.ORDERNO = B.POD_ORDERNO")
            sqlStat.AppendLine("and OVETA.TANKNO = B.TANKNO")
            sqlStat.AppendLine("and OVETA.DATEFIELD = B.POD_DATEFIELD")
            sqlStat.AppendLine("and OVETA.DELFLG <> @DelFlg")
            '直近ステータス
            sqlStat.AppendLine("left outer join WITH_STATUS as ST")
            sqlStat.AppendLine("on  ST.TANKNO = B.TANKNO")
            sqlStat.AppendLine("and ST.RECENT = 1")
            '直近３積載品
            sqlStat.AppendLine("left outer join WITH_P3HIST as P3HIST")
            sqlStat.AppendLine("on P3HIST.TANKNO = B.TANKNO")
            'DISCHARGE
            sqlStat.AppendLine("left outer join GBT0005_ODR_VALUE as OVDISC with(nolock)")
            sqlStat.AppendLine("on OVDISC.ORDERNO = B.POD_ORDERNO")
            sqlStat.AppendLine("and OVDISC.TANKNO = B.TANKNO")
            sqlStat.AppendLine("and  OVDISC.DTLPOLPOD = B.POD_DTLPOLPOD2")
            sqlStat.AppendLine("and OVDISC.ACTIONID = 'DLRY'")
            sqlStat.AppendLine("and OVDISC.ACTUALDATE <> @InitDate")
            sqlStat.AppendLine("and OVDISC.DELFLG <> @DelFlg")
            'デポイン
            sqlStat.AppendLine("left outer join WITH_DEPOTIN as OVDPIN with(nolock)")
            sqlStat.AppendLine("ON OVDPIN.ORDERNO = B.POD_ORDERNO")
            sqlStat.AppendLine("and OVDPIN.TANKNO = B.TANKNO")
            sqlStat.AppendLine("and OVDPIN.DTLPOLPOD = B.POD_DTLPOLPOD2")
            sqlStat.AppendLine("and OVDPIN.ORDERSORT = 1")
            sqlStat.AppendLine("and OVDPIN.DEPOTCODE <> ''")
            '直近のクリーニング
            sqlStat.AppendLine("left outer join (")
            sqlStat.AppendLine("  select OVETYC.TANKNO, max(OVETYC.ACTUALDATE) as ACTUALDATE")
            sqlStat.AppendLine("  from GBT0005_ODR_VALUE as OVETYC with(nolock) ")
            sqlStat.AppendLine("  where OVETYC.ACTIONID = 'ETYC'")
            sqlStat.AppendLine("  and   OVETYC.ACTUALDATE <> @InitDate")
            sqlStat.AppendLine("  and   OVETYC.DELFLG <> @DelFlg")
            sqlStat.AppendLine("  group by OVETYC.TANKNO")
            sqlStat.AppendLine("  ) as ETYC")
            sqlStat.AppendLine("on ETYC.TANKNO = B.TANKNO")
            'アロケーション
            sqlStat.AppendLine("left outer join GBT0005_ODR_VALUE as OVALLOC with(nolock)")
            sqlStat.AppendLine("on OVALLOC.ORDERNO = B.POL_ORDERNO")
            sqlStat.AppendLine("and OVALLOC.TANKNO = B.TANKNO")
            sqlStat.AppendLine("and OVALLOC.DTLPOLPOD = B.POL_DTLPOLPOD")
            sqlStat.AppendLine("and OVALLOC.ACTIONID <> ''")
            sqlStat.AppendLine("and OVALLOC.DELFLG <> @DelFlg")
            sqlStat.AppendLine("and OVALLOC.DISPSEQ = ( select min(DISPSEQ) from GBT0005_ODR_VALUE with(nolock) ")
            sqlStat.AppendLine("                        where ORDERNO = OVALLOC.ORDERNO")
            sqlStat.AppendLine("                        and   TANKNO = OVALLOC.TANKNO")
            sqlStat.AppendLine("                        and   DTLPOLPOD = OVALLOC.DTLPOLPOD")
            sqlStat.AppendLine("                        and   ACTIONID <> ''")
            sqlStat.AppendLine("                        and   DELFLG <> @DelFlg")
            sqlStat.AppendLine("                        and   DISPSEQ <> ''")
            sqlStat.AppendLine("                        group by ORDERNO,TANKNO,DTLPOLPOD )")
            '申請ステータス
            sqlStat.AppendLine("left outer join WITH_APPLY as APPLY")
            sqlStat.AppendLine("ON  APPLY.ORDERNO = B.POL_ORDERNO")
            sqlStat.AppendLine("and APPLY.TANKSEQ = B. POL_TANKSEQ")
            sqlStat.AppendLine("and APPLY.TRILATERAL = '1'")
            'デポアウト
            sqlStat.AppendLine("left outer join WITH_DEPOTOUT as DEPOTOUT")
            sqlStat.AppendLine("ON DEPOTOUT.ORDERNO = B.POL_ORDERNO")
            sqlStat.AppendLine("and DEPOTOUT.TANKNO = B.TANKNO")
            sqlStat.AppendLine("and DEPOTOUT.DTLPOLPOD = B.POL_DTLPOLPOD")
            sqlStat.AppendLine("and DEPOTOUT.ORDERSORT = 1")
            sqlStat.AppendLine("and DEPOTOUT.DEPOTCODE <> ''")
            'ローディング
            sqlStat.AppendLine("left outer join GBT0005_ODR_VALUE as POLLOAD with(nolock)")
            sqlStat.AppendLine("on POLLOAD.ORDERNO = B.POL_ORDERNO")
            sqlStat.AppendLine("and POLLOAD.TANKNO = B.TANKNO")
            sqlStat.AppendLine("and POLLOAD.DTLPOLPOD = B.POL_DTLPOLPOD")
            sqlStat.AppendLine("and POLLOAD.ACTIONID = 'LOAD'")
            sqlStat.AppendLine("and POLLOAD.ACTUALDATE <> @InitDate")
            sqlStat.AppendLine("and POLLOAD.DELFLG <> @DelFlg")
            '速度改善
            'sqlStat.AppendLine("left outer join GBT0004_ODR_BASE as POLLOAD_B with(nolock)")
            'sqlStat.AppendLine("  on POLLOAD_B.ORDERNO = POLLOAD.ORDERNO")
            'sqlStat.AppendLine(" and POLLOAD_B.DELFLG <> @DelFlg")
            'sqlStat.AppendLine("left outer join GBM0008_PRODUCT as POLLOAD_P with(nolock)")
            'sqlStat.AppendLine("  on POLLOAD_P.COMPCODE = '01'")
            'sqlStat.AppendLine(" and POLLOAD_P.PRODUCTCODE = POLLOAD_B.PRODUCTCODE")
            'sqlStat.AppendLine(" and POLLOAD_P.DELFLG <> @DelFlg")
            sqlStat.AppendLine("left outer join ( ")
            sqlStat.AppendLine("    select wb.ORDERNO,wp.PRODUCTNAME ")
            sqlStat.AppendLine("    from GBT0004_ODR_BASE as wb with(nolock) ")
            sqlStat.AppendLine("      left outer join GBM0008_PRODUCT wp with(nolock) ")
            sqlStat.AppendLine("        on wp.COMPCODE = '01' ")
            sqlStat.AppendLine("       and wp.PRODUCTCODE = wb.PRODUCTCODE ")
            sqlStat.AppendLine("       and wp.DELFLG <> @DelFlg")
            sqlStat.AppendLine("    where wb.DELFLG <> @DelFlg")
            sqlStat.AppendLine("    ) as POLLOAD_B ")
            sqlStat.AppendLine("    on POLLOAD_B.ORDERNO = POLLOAD.ORDERNO")
            '直近のリースアウト
            sqlStat.AppendLine("left outer join (")
            sqlStat.AppendLine("  select OVLESD.TANKNO, max(OVLESD.ACTUALDATE) as ACTUALDATE")
            sqlStat.AppendLine("  from GBT0005_ODR_VALUE as OVLESD with(nolock) ")
            sqlStat.AppendLine("  where OVLESD.ACTIONID = 'LESD'")
            sqlStat.AppendLine("  and   OVLESD.ACTUALDATE <> @InitDate")
            sqlStat.AppendLine("  and   OVLESD.DELFLG <> @DelFlg")
            sqlStat.AppendLine("  group by OVLESD.TANKNO")
            sqlStat.AppendLine("  ) as LESD")
            sqlStat.AppendLine("on LESD.TANKNO = B.TANKNO")
            '直近のリースイン
            sqlStat.AppendLine("left outer join (")
            sqlStat.AppendLine("  select OVLEIN.TANKNO, max(OVLEIN.ACTUALDATE) as ACTUALDATE")
            sqlStat.AppendLine("  from GBT0005_ODR_VALUE as OVLEIN with(nolock) ")
            sqlStat.AppendLine("  where OVLEIN.ACTIONID = 'LEIN'")
            sqlStat.AppendLine("  and   OVLEIN.ACTUALDATE <> @InitDate")
            sqlStat.AppendLine("  and   OVLEIN.DELFLG <> @DelFlg")
            sqlStat.AppendLine("  group by OVLEIN.TANKNO")
            sqlStat.AppendLine("  ) as LEIN")
            sqlStat.AppendLine("on LEIN.TANKNO = B.TANKNO")
            ' リース引き当て済みタンクチェック用
            sqlStat.AppendLine("left outer join WITH_LEASETANK WLTI")
            sqlStat.AppendLine("on WLTI.TANKNO = B.TANKNO")
            sqlStat.AppendLine("where 1 = 1")
            'If Me.ISALLOCATEONLY <> 1 Then
            'sqlStat.AppendLine("  and (@SelCountry = '' or (@SelCountry <> '' and (B.POD_PODCOUNTRY = @SelCountry or B.POL_PODCOUNTRY = @SelCountry)))")
            'If Me.ISALLOCATEONLY <> 1 AndAlso Me.COUNTRYCODE = "" Then ←　条件間違い
            If Me.ISALLOCATEONLY <> 1 AndAlso Me.COUNTRYCODE <> "" Then
                sqlStat.AppendLine("  and (B.POD_PODCOUNTRY = @SelCountry or B.POL_PODCOUNTRY = @SelCountry)")
            End If
            If Me.ISALLOCATEONLY = 1 Then
                sqlStat.AppendLine("  and (")
                sqlStat.AppendLine("           (isnull(ST.ACTIONID,'') in (@AllocActionId1,@AllocActionId2,'')")
                'sqlStat.AppendLine("       and (@SelCountry = '' or (@SelCountry <> '' and (B.POD_PODCOUNTRY = @SelCountry or B.POL_PODCOUNTRY = @SelCountry))))")
                If Me.COUNTRYCODE = "" Then
                    sqlStat.AppendLine("       )")
                Else
                    sqlStat.AppendLine("       and (B.POD_PODCOUNTRY = @SelCountry or B.POL_PODCOUNTRY = @SelCountry))")
                End If
                'sqlStat.AppendLine("       or")
                'sqlStat.AppendLine("           (    isnull(B.POL_ORDERNO,'') = @AllocateOrderNo  ")
                ''sqlStat.AppendLine("            and isnull(ST.ACTIONID,'') in (@AllocActionId3,@AllocActionId4,@AllocActionId5)) ")
                'sqlStat.AppendLine("           ) ") '↑を生かす場合は当行をコメントアウト
                'sqlStat.AppendLine("            (")
                If Me.TANKNOLIST IsNot Nothing AndAlso Me.TANKNOLIST.Count > 0 Then
                    Dim tankNoInCond As String = ""
                    For Each tmpTankNo As String In Me.TANKNOLIST
                        If tankNoInCond = "" Then
                            tankNoInCond = tankNoInCond & "'" & tmpTankNo & "'"
                        Else
                            tankNoInCond = tankNoInCond & ",'" & tmpTankNo & "'"
                        End If
                    Next
                    sqlStat.AppendLine("       or")
                    sqlStat.AppendLine("            (")
                    sqlStat.AppendFormat("            B.TANKNO IN ({0})", tankNoInCond).AppendLine()
                    sqlStat.AppendLine("            )")
                End If
                sqlStat.AppendLine("      )")
            ElseIf Me.ISALLOCATEONLY = 2 Then
                '協定書(リース)タンク引き当て条件
                sqlStat.AppendLine("  and (")
                sqlStat.AppendLine("       (")
                sqlStat.AppendLine("           (isnull(ST.ACTIONID,'') in (@AllocActionId1,@AllocActionId2,''))")
                ' not exists を  left outer joinに変更
                'sqlStat.AppendLine("    and not exists (select 1  ")
                'sqlStat.AppendLine("                     from WITH_LEASETANK WLTI")
                'sqlStat.AppendLine("                    where WLTI.TANKNO = B.TANKNO)")
                sqlStat.AppendLine("    and WLTI.TANKNO is null ")
                sqlStat.AppendLine("       )")
                If Me.TANKNOLIST IsNot Nothing AndAlso Me.TANKNOLIST.Count > 0 Then
                    Dim tankNoInCond As String = ""
                    For Each tmpTankNo As String In Me.TANKNOLIST
                        If tankNoInCond = "" Then
                            tankNoInCond = tankNoInCond & "'" & tmpTankNo & "'"
                        Else
                            tankNoInCond = tankNoInCond & ",'" & tmpTankNo & "'"
                        End If
                    Next
                    sqlStat.AppendLine("       or")
                    sqlStat.AppendLine("            (")
                    sqlStat.AppendFormat("            B.TANKNO IN ({0})", tankNoInCond).AppendLine()
                    sqlStat.AppendLine("            )")

                End If
                sqlStat.AppendLine("      )")
            ElseIf Me.ISALLOCATEONLY = 3 Then
                'リース起因セールス、オペタンク引当(未実装)
                sqlStat.AppendLine("  and (")
                sqlStat.AppendLine("       (")
                If Not String.IsNullOrEmpty(Me.POLPORT) Then 'リース輸送時はSHIP以降も対象
                    sqlStat.AppendLine("          ((isnull(ST.ACTIONID,'') in (@AllocActionId1,@AllocActionId2,@AllocActionIdLeaseOut))")
                    sqlStat.AppendLine("           or (isnull(ST.ACTIONID,'') in (@AllocActionId3,@AllocActionId4,@AllocActionId5,@AllocActionId6)")
                    sqlStat.AppendLine("              and B.POD_POLPORT = @POLPORT))")
                Else
                    sqlStat.AppendLine("           (isnull(ST.ACTIONID,'') in (@AllocActionId1,@AllocActionId2,@AllocActionIdLeaseOut))")
                End If
                sqlStat.AppendLine("    and    LESD.ACTUALDATE is not null ") 'リースアウトが存在していることが前提
                sqlStat.AppendLine("    and    LESD.ACTUALDATE >= isnull(LEIN.ACTUALDATE,@initdate) ") 'リースアウト日付 >= リースイン日付（現在の状態がリースアウトである）
                sqlStat.AppendLine("    and    exists (select 1  ") '使用可能なリース契約タンク
                sqlStat.AppendLine("                     from WITH_LEASETANK WLTI")
                sqlStat.AppendLine("                    where WLTI.TANKNO      = B.TANKNO")
                'If Me.REPFLG <> "1" Then
                '    sqlStat.AppendLine("                      and WLTI.SHIPPER     = @SHIPPERCODE")
                '    'sqlStat.AppendLine("                      and WLTI.PRODUCTCODE = @PRODUCTCODE")
                '    sqlStat.AppendLine("                      and WLTI.ORGANIZER   = @ORGANIZER")
                'End If
                If Me.REPFLG <> "1" Then '20190910 TORICOMPで出すように修正
                    Dim customerInstat As String = GetCustomerInStat(Me.SHIPPERCODE)
                    If customerInstat = "" Then
                        sqlStat.AppendLine("                      and WLTI.SHIPPER     = @SHIPPERCODE")
                    Else
                        sqlStat.AppendFormat("                      and WLTI.SHIPPER    IN ({0})", customerInstat).AppendLine()
                    End If
                    'sqlStat.AppendLine("                      and WLTI.PRODUCTCODE = @PRODUCTCODE")
                    sqlStat.AppendLine("                      and WLTI.ORGANIZER   = @ORGANIZER")
                End If
                sqlStat.AppendLine("                      and WLTI.CONTRACTFROM  <= LESD.ACTUALDATE") 'リース契約開始日     <= リースアウト
                sqlStat.AppendLine("                      and WLTI.ENABLED        = '" & CONST_FLAG_YES & "'") 'リース契約契約終了日 >= リースアウト
                sqlStat.AppendLine("                   )")
                sqlStat.AppendLine("       )")
                If Me.TANKNOLIST IsNot Nothing AndAlso Me.TANKNOLIST.Count > 0 Then
                    Dim tankNoInCond As String = ""
                    For Each tmpTankNo As String In Me.TANKNOLIST
                        If tankNoInCond = "" Then
                            tankNoInCond = tankNoInCond & "'" & tmpTankNo & "'"
                        Else
                            tankNoInCond = tankNoInCond & ",'" & tmpTankNo & "'"
                        End If
                    Next
                    sqlStat.AppendLine("       or")
                    sqlStat.AppendLine("            (")
                    sqlStat.AppendFormat("            B.TANKNO IN ({0})", tankNoInCond).AppendLine()
                    sqlStat.AppendLine("            )")

                End If
                sqlStat.AppendLine("      )")

            ElseIf Me.ISALLOCATEONLY = 4 Then
                'リースアウトタンク引当条件
                'リースタンク引き当て条件
                sqlStat.AppendLine("  and (")
                sqlStat.AppendLine("       (")
                sqlStat.AppendLine("           (isnull(ST.ACTIONID,'') in (@AllocActionId1,@AllocActionId2,''))")
                sqlStat.AppendLine("    and exists (select 1  ")
                sqlStat.AppendLine("                  from WITH_LEASETANK WLTI")
                sqlStat.AppendLine("                 where WLTI.TANKNO = B.TANKNO")
                sqlStat.AppendLine("               )")
                sqlStat.AppendLine("       )")
                If Me.TANKNOLIST IsNot Nothing AndAlso Me.TANKNOLIST.Count > 0 Then
                    Dim tankNoInCond As String = ""
                    For Each tmpTankNo As String In Me.TANKNOLIST
                        If tankNoInCond = "" Then
                            tankNoInCond = tankNoInCond & "'" & tmpTankNo & "'"
                        Else
                            tankNoInCond = tankNoInCond & ",'" & tmpTankNo & "'"
                        End If
                    Next
                    sqlStat.AppendLine("       or")
                    sqlStat.AppendLine("            (")
                    sqlStat.AppendFormat("            B.TANKNO IN ({0})", tankNoInCond).AppendLine()
                    sqlStat.AppendLine("            )")

                End If
                sqlStat.AppendLine("      )")

            ElseIf Me.ISALLOCATEONLY = 5 Then
                'リースインタンク引当条件
                sqlStat.AppendLine("  and (")
                sqlStat.AppendLine("       (")
                sqlStat.AppendLine("           (isnull(ST.ACTIONID,'') in (@AllocActionId1,@AllocActionId2,@AllocActionIdLeaseOut))")
                sqlStat.AppendLine("    and    LESD.ACTUALDATE is not null ") 'リースアウトが存在していることが前提
                sqlStat.AppendLine("    and    LESD.ACTUALDATE >= isnull(LEIN.ACTUALDATE,@initdate) ") 'リースアウト日付 >= リースイン日付（現在の状態がリースアウトである）
                sqlStat.AppendLine("    and    exists (select 1  ") '使用可能なリース契約タンク
                sqlStat.AppendLine("                     from WITH_LEASETANK WLTI")
                sqlStat.AppendLine("                    where WLTI.TANKNO        = B.TANKNO")
                sqlStat.AppendLine("                      and WLTI.CONTRACTFROM  <= LESD.ACTUALDATE") 'リース契約開始日     <= リースアウト
                sqlStat.AppendLine("                      and WLTI.ENABLED       = '" & CONST_FLAG_YES & "'") 'リース契約契約終了日 >= リースアウト
                sqlStat.AppendLine("                   )")
                sqlStat.AppendLine("       )")
                If Me.TANKNOLIST IsNot Nothing AndAlso Me.TANKNOLIST.Count > 0 Then
                    Dim tankNoInCond As String = ""
                    For Each tmpTankNo As String In Me.TANKNOLIST
                        If tankNoInCond = "" Then
                            tankNoInCond = tankNoInCond & "'" & tmpTankNo & "'"
                        Else
                            tankNoInCond = tankNoInCond & ",'" & tmpTankNo & "'"
                        End If
                    Next
                    sqlStat.AppendLine("       or")
                    sqlStat.AppendLine("            (")
                    sqlStat.AppendFormat("            B.TANKNO IN ({0})", tankNoInCond).AppendLine()
                    sqlStat.AppendLine("            )")

                End If
                sqlStat.AppendLine("      )")

            End If
            sqlStat.AppendLine("order by B.TANKNO")

            Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
                , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)

                With sqlCmd.Parameters
                    .Add("@InitDate", SqlDbType.DateTime).Value = "1900/01/01"
                    .Add("@DeptClass", SqlDbType.NVarChar).Value = "デポ"
                    .Add("@SelCountry", SqlDbType.NVarChar).Value = Me.COUNTRYCODE
                    .Add("@DelFlg", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@AllocActionId1", SqlDbType.NVarChar).Value = "ETYD"
                    .Add("@AllocActionId2", SqlDbType.NVarChar).Value = "ETYC"
                    '引当時の自身のオーダーの際に表示するALLOCATEのACTIONID
                    '.Add("@AllocActionId3", SqlDbType.NVarChar).Value = "TKAL"
                    '.Add("@AllocActionId4", SqlDbType.NVarChar).Value = "TAEC"
                    '.Add("@AllocActionId5", SqlDbType.NVarChar).Value = "TAED"
                    'HISリース引当時のオーダーの際に表示するALLOCATEのACTIONID
                    .Add("@AllocActionId3", SqlDbType.NVarChar).Value = "SHIP"
                    .Add("@AllocActionId4", SqlDbType.NVarChar).Value = "TRAV"
                    .Add("@AllocActionId5", SqlDbType.NVarChar).Value = "TRSH"
                    .Add("@AllocActionId6", SqlDbType.NVarChar).Value = "ARVD"

                    '引当対象のオーダーNo
                    .Add("@AllocateOrderNo", SqlDbType.NVarChar).Value = Me.ALLOCATEORDERNO
                    'リース起因セールスの引き当て条件
                    .Add("@SHIPPERCODE", SqlDbType.NVarChar).Value = Me.SHIPPERCODE
                    .Add("@PRODUCTCODE", SqlDbType.NVarChar).Value = Me.PRODUCTCODE
                    .Add("@ORGANIZER", SqlDbType.NVarChar).Value = Me.AGENTORGANIZER
                    .Add("@AllocActionIdLeaseOut", SqlDbType.NVarChar).Value = "LESD"
                    .Add("@POLPORT", SqlDbType.NVarChar).Value = Me.POLPORT
                End With
                sqlConn.Open()
                sqlCmd.CommandTimeout = 180
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(Me.TANKSTATUS_TABLE)
                End Using
            End Using

            If Me.TANKSTATUS_TABLE.Rows.Count > 0 Then
                Me.ERR = C_MESSAGENO.NORMAL
            Else
                Me.ERR = C_MESSAGENO.NODATA
            End If

        Catch ex As Exception
            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' 顧客コードを元にTORICOMPコードが同一のIn句用顧客コード取得
    ''' </summary>
    ''' <param name="customerCode">顧客コード(親)</param>
    ''' <returns>引数を元にTORICOMPコードが同一の顧客コード(In句)部分を生成</returns>
    ''' <remarks>本体での一度SQLの抽出だと負荷が大きくなるので分離</remarks>
    Private Function GetCustomerInStat(customerCode As String) As String
        Dim sqlStat As New Text.StringBuilder
        Dim retCode As String = ""
        sqlStat.AppendLine("SELECT CUS.CUSTOMERCODE")
        sqlStat.AppendLine("  FROM GBM0004_CUSTOMER CUS")
        sqlStat.AppendLine(" WHERE CUS.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("   AND CUS.TORICOMP <> ''")
        sqlStat.AppendLine("   AND CUS.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("   AND CUS.TORICOMP IN (SELECT CUSSUB.TORICOMP")
        sqlStat.AppendLine("                          FROM GBM0004_CUSTOMER CUSSUB")
        sqlStat.AppendLine("                         WHERE CUSSUB.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("                           AND CUSSUB.CUSTOMERCODE = @CUSTOMERCODE")
        sqlStat.AppendLine("                           AND CUSSUB.TORICOMP <> ''")
        sqlStat.AppendLine("                           AND CUSSUB.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("                       )")

        Using sqlConn As New SqlConnection(Convert.ToString(COA0019Session.DBcon)) _
            , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
            'パラメータの設定
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@CUSTOMERCODE", SqlDbType.NVarChar).Value = customerCode
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd) _
                , dt As New DataTable
                sqlDa.Fill(dt)

                If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                    retCode = String.Join(",", (From rowItem In dt Select "'" & Convert.ToString(rowItem("CUSTOMERCODE")) & "'").ToArray)
                End If
            End Using
        End Using
        Return retCode
    End Function
End Structure
''' <summary>
''' SOAデータ取得
''' </summary>
''' <remarks>GBT0004</remarks>
Public Structure GBA00013SoaInfo
    ''' <summary>
    ''' エラーコード(00000=正常)
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String
    ''' <summary>
    ''' [IN]ソート用マップID(未指定時はソートなし及び、連番項目なし(LINECNT))
    ''' </summary>
    ''' <returns></returns>
    Public Property SORTMAPID As String
    ''' <summary>
    ''' [IN]ソート用MAPVARIANT(未指定時はソートなし及び、連番項目なし(LINECNT))
    ''' </summary>
    ''' <returns></returns>
    Public Property SORTMAPVARIANT As String
    ''' <summary>
    ''' [IN]国コード
    ''' </summary>
    ''' <returns></returns>
    Public Property COUNTRYCODE As String
    ''' <summary>
    ''' [IN]精算年月("yyyy/MM"指定または"ALL")
    ''' </summary>
    ''' <returns></returns>
    Public Property REPORTMONTH As String
    ''' <summary>
    ''' [IN]日付範囲From("yyyy/MM/dd" REPORTMONTHがALL時のみ利用可能)
    ''' </summary>
    ''' <returns></returns>
    Public Property ACTUALDATEFROM As String
    ''' <summary>
    ''' [IN]日付範囲To("yyyy/MM/dd" REPORTMONTHがALL時のみ利用可能)
    ''' </summary>
    ''' <returns></returns>
    Public Property ACTUALDATETO As String
    ''' <summary>
    ''' [IN]OFFICE(現在考慮なし)
    ''' </summary>
    ''' <returns></returns>
    Public Property OFFICE As String
    ''' <summary>
    ''' [IN]INVOICEDBYTYPE("OJ"=JOTのみ,"IJ"=JOT含む(無条件と同じ),EJ=JOT含まない)
    ''' </summary>
    ''' <returns></returns>
    Public Property INVOICEDBYTYPE As String
    ''' <summary>
    ''' [IN]業者コード
    ''' </summary>
    ''' <returns></returns>
    Public Property VENDER As String
    ''' <summary>
    ''' [IN]SOATYPE(FIXVALUEのAGENTSOAの選択し参照)
    ''' </summary>
    ''' <returns></returns>
    Public Property SOATYPE As String
    ''' <summary>
    ''' [IN]SOA画面で非表示の費用データも取得（デマレージ等）
    ''' "1"：SOA画面非表示のデータも取得、未指定は除外。
    ''' SOATYPEの入力が優先されますので全費用を取る場合はSOATYPE未指定にする事
    ''' </summary>
    ''' <returns></returns>
    Public Property SHOULDGETALLCOST As String
    ''' <summary>
    ''' SOA締め処理
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>通常はREPORTMONTHと一致にするが、このプロパティに"1"を設定した場合は
    ''' REPORTMONTH以降のデータを条件とする</remarks>
    Public Property SOACLOSEPROC As String
    ''' <summary>
    ''' [OUT]SOAリスト
    ''' USDAMOUNTがGBT0008_JOTSOA_VALUE.AMOUNTPAY
    ''' LOCALAMOUNTがGBT0008_JOTSOA_VALUE.LOCALPAYにそれぞれ格納されます。
    ''' </summary>
    ''' <returns></returns>
    Public Property SOADATATABLE As DataTable
    Public Sub GBA00013getSoaDataTable()
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Try
            '************************************
            'ソート順取得
            '************************************
            Dim sortOrder As String = ""
            If Me.SORTMAPID IsNot Nothing AndAlso Me.SORTMAPID <> "" _
               AndAlso Me.SORTMAPVARIANT IsNot Nothing AndAlso Me.SORTMAPVARIANT <> "" Then
                Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

                COA0020ProfViewSort.MAPID = Me.SORTMAPID
                COA0020ProfViewSort.VARI = Me.SORTMAPVARIANT
                COA0020ProfViewSort.TAB = ""
                COA0020ProfViewSort.COA0020getProfViewSort()
                If COA0020ProfViewSort.ERR <> C_MESSAGENO.NORMAL Then
                    Me.ERR = COA0020ProfViewSort.ERR
                    Return
                End If
                sortOrder = COA0020ProfViewSort.SORTSTR

            End If
            '************************************
            '未設定パラメータの初期化(nothing → "")
            '************************************
            ParamInit()
            '************************************
            'SQL生成
            '************************************
            'ユーザーの言語に応じ日本語⇔英語フィールド設定
            Dim textCostTblField As String = "NAMESJP"
            If COA0019Session.LANGDISP <> C_LANG.JA Then
                textCostTblField = "NAMES"
            End If
            Dim sqlWarnBaseDate As String = " (CASE WHEN @WARNDATE = '' THEN TBL.BILLINGYMD_DTM ELSE @WARNDATE END) "
            Dim sqlStat As New StringBuilder()
            '**************************
            'WITH句生成 START
            '**************************
            '警告表示のn月先のnを取得
            sqlStat.AppendLine("WITH ")
            sqlStat.AppendLine(" W_LIMITMONTH AS (") 'START W_LIMITMONTH
            sqlStat.AppendLine("   SELECT TOP 1 VALUE1")
            sqlStat.AppendLine("        , VALUE2")
            sqlStat.AppendLine("     FROM COS0017_FIXVALUE with(nolock) ")
            sqlStat.AppendLine("    WHERE COMPCODE = '" & GBC_COMPCODE_D & "' ")
            sqlStat.AppendLine("      AND SYSCODE  = '" & C_SYSCODE_GB & "' ")
            sqlStat.AppendLine("      AND CLASS    = 'SOALOWERLIMITMONTH' ")
            sqlStat.AppendLine("      AND KEYCODE  = '-' ")
            sqlStat.AppendLine("      AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine(" )") 'END W_LIMITMONTH

            '締め日関連With
            sqlStat.AppendLine(" ,")
            sqlStat.AppendLine(" W_CLOSINGDAY AS (") 'START W_CLOSINGDAY
            sqlStat.AppendLine("   SELECT CL.COUNTRYCODE")
            sqlStat.AppendLine("         ,CL.BILLINGYMD ")
            sqlStat.AppendLine("         ,CL.REPORTMONTH ")
            sqlStat.AppendLine("         ,FORMAT(DATEADD(month,1,DATEADD(day,-1,CL.BILLINGYMD)),'yyyy/MM') AS CLOSINGMONTH")
            sqlStat.AppendLine("         ,(DAY(CL.BILLINGYMD) -1)                                          AS CALCDAY")
            sqlStat.AppendLine("         ,FORMAT(CASE WHEN DAY(CASE WHEN @WARNDATE = '' THEN CL.BILLINGYMD ELSE @WARNDATE END)<=(DAY(CL.BILLINGYMD) -1) OR (DAY(CL.BILLINGYMD) -1) = 0 ")
            sqlStat.AppendLine("                      THEN DATEADD(month,0,(CASE WHEN @WARNDATE = '' THEN CL.BILLINGYMD ELSE @WARNDATE END))")
            sqlStat.AppendLine("                      ELSE DATEADD(month,1,(CASE WHEN @WARNDATE = '' THEN CL.BILLINGYMD ELSE @WARNDATE END))")
            sqlStat.AppendLine("                  END,'yyyy/MM') AS CURRENT_CLOSE_MONTH") '条件指定の月、未指定時は当テーブルの実際の締め月を判定
            sqlStat.AppendLine("         ,FORMAT(DATEADD(month,(LM.VALUE1 * -1),")
            sqlStat.AppendLine("                 CASE WHEN DAY(CASE WHEN @WARNDATE = '' THEN CL.BILLINGYMD ELSE @WARNDATE END)<=(DAY(CL.BILLINGYMD) -1) OR (DAY(CL.BILLINGYMD) -1) = 0 ")
            sqlStat.AppendLine("                      THEN DATEADD(month,0,(CASE WHEN @WARNDATE = '' THEN CL.BILLINGYMD ELSE @WARNDATE END))")
            sqlStat.AppendLine("                      ELSE DATEADD(month,1,(CASE WHEN @WARNDATE = '' THEN CL.BILLINGYMD ELSE @WARNDATE END))")
            sqlStat.AppendLine("                  END)")
            sqlStat.AppendLine("                 ,'yyyy/MM') AS AUTOCLOSE_MONTH")
            sqlStat.AppendLine("         ,FORMAT(DATEADD(month,(LM.VALUE2 * -1),")
            sqlStat.AppendLine("                 CASE WHEN DAY(CASE WHEN @WARNDATE = '' THEN CL.BILLINGYMD ELSE @WARNDATE END)<=(DAY(CL.BILLINGYMD) -1) OR (DAY(CL.BILLINGYMD) -1) = 0 ")
            sqlStat.AppendLine("                      THEN DATEADD(month,0,(CASE WHEN @WARNDATE = '' THEN CL.BILLINGYMD ELSE @WARNDATE END))")
            sqlStat.AppendLine("                      ELSE DATEADD(month,1,(CASE WHEN @WARNDATE = '' THEN CL.BILLINGYMD ELSE @WARNDATE END))")
            sqlStat.AppendLine("                  END)")
            sqlStat.AppendLine("                 ,'yyyy/MM') AS AUTOCLOSE_MONTH_LONG")
            sqlStat.AppendLine("     FROM GBT0006_CLOSINGDAY CL with(nolock) ")
            sqlStat.AppendLine("         ,W_LIMITMONTH LM")
            sqlStat.AppendLine("    WHERE CL.STYMD           <= @NOWDATE")
            sqlStat.AppendLine("      AND CL.ENDYMD          >= @NOWDATE")
            sqlStat.AppendLine("      AND CL.DELFLG          <> @DELFLG")
            sqlStat.AppendLine("      AND EXISTS (SELECT CLDS.COUNTRYCODE,MAX(CLDS.REPORTMONTH) AS REPORTMONTH")
            sqlStat.AppendLine("                    FROM GBT0006_CLOSINGDAY CLDS with(nolock) ")
            sqlStat.AppendLine("                   WHERE CLDS.STYMD        <= @NOWDATE")
            sqlStat.AppendLine("                     AND CLDS.ENDYMD         >= @NOWDATE")
            sqlStat.AppendLine("                     AND CLDS.DELFLG          <> @DELFLG")
            sqlStat.AppendLine("                   GROUP BY CLDS.COUNTRYCODE")
            sqlStat.AppendLine("                  HAVING CLDS.COUNTRYCODE      = CL.COUNTRYCODE")
            sqlStat.AppendLine("                     AND MAX(CLDS.REPORTMONTH) = CL.REPORTMONTH")
            sqlStat.AppendLine("                 )")
            sqlStat.AppendLine(" )")  'END W_CLOSINGDAY

            'JOTのエージェントを取得(INVOICED BYで判定用)
            sqlStat.AppendLine(" ,")
            sqlStat.AppendLine(" W_JOTAGENT AS (") 'START 
            sqlStat.AppendLine("   SELECT TR.CARRIERCODE")
            sqlStat.AppendLine("     FROM GBM0005_TRADER TR with(nolock) ")
            sqlStat.AppendLine("    WHERE TR.STYMD  <= @NOWDATE")
            sqlStat.AppendLine("      AND TR.ENDYMD >= @NOWDATE")
            sqlStat.AppendLine("      AND TR.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      AND EXISTS (SELECT 1")
            sqlStat.AppendLine("                    FROM COS0017_FIXVALUE FXV with(nolock) ")
            sqlStat.AppendLine("                   WHERE FXV.COMPCODE   = 'Default'")
            sqlStat.AppendLine("                     AND FXV.SYSCODE    = 'GB'")
            sqlStat.AppendLine("                     AND FXV.CLASS      = 'JOTCOUNTRYORG'")
            sqlStat.AppendLine("                     AND FXV.KEYCODE     = TR.MORG")
            sqlStat.AppendLine("                     AND FXV.STYMD     <= @NOWDATE")
            sqlStat.AppendLine("                     AND FXV.ENDYMD    >= @NOWDATE")
            sqlStat.AppendLine("                     AND FXV.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("                 )")
            sqlStat.AppendLine(")")
            'WITH句生成 END
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       TBL.* ")
            If sortOrder <> "" Then
                sqlStat.AppendLine("      ,ROW_NUMBER() OVER(ORDER BY " & sortOrder & ") As LINECNT")
            End If
            'sqlStat.AppendLine("      ,CASE WHEN TBL.REPORTYMD_BASE < CLOSINGMONTH THEN CLOSINGMONTH ELSE TBL.REPORTYMD_BASE END AS REPORTYMD")
            sqlStat.AppendLine("      ,CASE WHEN NOT(TBL.JOTSOAVL_REPORTMONTH IS NULL OR TBL.JOTSOAVL_REPORTMONTH = '') THEN TBL.JOTSOAVL_REPORTMONTH WHEN TBL.REPORTYMD_BASE < CLOSINGMONTH THEN CLOSINGMONTH ELSE TBL.REPORTYMD_BASE END AS REPORTYMD")
            'sqlStat.AppendLine("      ,TBL.REPORTYMD_BASE + '(' +  CLOSINGMONTH + ')' AS REPORTYMD")

            sqlStat.AppendLine("      ,TBL.REPORTYMD_BASE AS REPORTYMDORG")
            sqlStat.AppendLine("      ,CASE WHEN TBL.REPORTYMD_BASE IS NULL OR TBL.REPORTYMD_BASE = '' THEN '-' ELSE TBL.REPORTYMD_BASE END AS REPORTMONTHORG")
            'SOA時点のレート
            sqlStat.AppendLine("      ,TBL.EXRATE AS LOCALRATESOA")
            'sqlStat.AppendLine("      ,CASE WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.USDAMOUNT_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBL.USDAMOUNT_BOFORE_ROUND ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.USDAMOUNT_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.USDAMOUNT_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.USDAMOUNT_BOFORE_ROUND,TBL.USDDECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.USDAMOUNT_BOFORE_ROUND END AS USDAMOUNT ")
            'sqlStat.AppendLine("      ,CASE WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.LOCALAMOUNT_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE <> '" & GBC_CUR_USD & "' THEN TBL.LOCALAMOUNT_BOFORE_ROUND ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.LOCALAMOUNT_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.LOCALAMOUNT_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.LOCALAMOUNT_BOFORE_ROUND,TBL.DECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.LOCALAMOUNT_BOFORE_ROUND END AS LOCALAMOUNT ")
            'オーダー作成時点でのレート計算結果
            'sqlStat.AppendLine("      ,CASE WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.USDAMOUNTODR_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBL.USDAMOUNTODR_BOFORE_ROUND ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.USDAMOUNTODR_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.USDAMOUNTODR_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.USDAMOUNTODR_BOFORE_ROUND,TBL.USDDECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.USDAMOUNTODR_BOFORE_ROUND END AS AMOUNTPAYODR ")
            'sqlStat.AppendLine("      ,CASE WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.LOCALAMOUNTODR_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE <> '" & GBC_CUR_USD & "' THEN TBL.LOCALAMOUNTODR_BOFORE_ROUND ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.LOCALAMOUNTODR_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.LOCALAMOUNTODR_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.LOCALAMOUNTODR_BOFORE_ROUND,TBL.DECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.LOCALAMOUNTODR_BOFORE_ROUND END AS LOCALPAYODR ")

            'UAG_USD端数処理後
            'sqlStat.AppendLine("      ,CASE WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.UAG_USD_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBL.UAG_USD_BOFORE_ROUND") 'ドル換算の場合はそのまま
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.UAG_USD_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.UAG_USD_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.UAG_USD_BOFORE_ROUND,TBL.USDDECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.UAG_USD_BOFORE_ROUND END AS UAG_USD ")
            'UAG_LOCAL端数処理後
            'sqlStat.AppendLine("      ,CASE WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.UAG_LOCAL_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE <> '" & GBC_CUR_USD & "' THEN UAG_LOCAL_BOFORE_ROUND ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.UAG_LOCAL_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.UAG_LOCAL_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.UAG_LOCAL_BOFORE_ROUND,TBL.DECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.UAG_LOCAL_BOFORE_ROUND END AS UAG_LOCAL ")
            'USD_USD
            'sqlStat.AppendLine("      ,CASE WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.UAG_USD_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBL.UAG_USD_BOFORE_ROUND") 'ドル換算の場合はそのまま
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.UAG_USD_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.UAG_USD_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.UAG_USD_BOFORE_ROUND,TBL.USDDECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.UAG_USD_BOFORE_ROUND END AS USD_USD ")
            'USD_LOCAL
            'sqlStat.AppendLine("      ,CASE WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR((TBL.UAG_USD_BOFORE_ROUND * TBL.SOARATE) * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE <> '" & GBC_CUR_USD & "' THEN UAG_LOCAL_BOFORE_ROUND ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR((TBL.UAG_USD_BOFORE_ROUND * TBL.SOARATE) * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  (TBL.UAG_USD_BOFORE_ROUND * TBL.SOARATE) * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  (TBL.UAG_USD_BOFORE_ROUND * TBL.SOARATE),TBL.DECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE (TBL.UAG_USD_BOFORE_ROUND * TBL.EXRATE) END AS USD_LOCAL ")
            'LOCAL_USD
            sqlStat.AppendLine("      ,CASE WHEN TBL.EXRATE = 0 OR TBL.EXRATE = '' THEN '' ELSE")
            sqlStat.AppendLine("       CASE WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR((TBL.UAG_LOCAL_BOFORE_ROUND / TBL.SOARATE) * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  (TBL.UAG_LOCAL_BOFORE_ROUND / TBL.SOARATE) * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  (TBL.UAG_LOCAL_BOFORE_ROUND / TBL.SOARATE),TBL.USDDECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE (TBL.UAG_LOCAL_BOFORE_ROUND / TBL.EXRATE) END")
            sqlStat.AppendLine("            END AS LOCAL_USD")
            'LOCAL_LOCAL端数処理後
            sqlStat.AppendLine("      ,CASE WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.UAG_LOCAL_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.UAG_LOCAL_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.UAG_LOCAL_BOFORE_ROUND,TBL.DECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.UAG_LOCAL_BOFORE_ROUND END AS LOCAL_LOCAL ")
            sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
            If sortOrder <> "" Then
                sqlStat.AppendLine("      ,('SYS' + right('00000' + trim(convert(char,ROW_NUMBER() OVER(ORDER BY " & sortOrder & "))), 5)) AS SYSKEY")
            End If
            sqlStat.AppendLine("      ,CASE WHEN TBL.REPORTYMD_BASE <> '' AND TBL.REPORTYMD_BASE <> '-' AND TBL.REPORTYMD_BASE <= TBL.AUTOCLOSE_MONTH      THEN '1' ELSE '0' END AS ISAUTOCLOSE")
            sqlStat.AppendLine("      ,CASE WHEN TBL.REPORTYMD_BASE <> '' AND TBL.REPORTYMD_BASE <> '-' AND TBL.REPORTYMD_BASE <= TBL.AUTOCLOSE_MONTH_LONG THEN '1' ELSE '0' END AS ISAUTOCLOSELONG")
            sqlStat.AppendLine("      ,CASE WHEN TBL.REPORTYMD_BASE = '' OR TBL.REPORTYMD_BASE = '-' THEN '' WHEN TBL.REPORTYMD_BASE <> '' AND TBL.REPORTYMD_BASE > TBL.CURRENT_CLOSE_MONTH THEN '1' ELSE '0' END AS ISFUTUREMONTH")
            'sqlStat.AppendLine("      ,TBL.REPORTYMD_BASE  AS REPORTYMD")
            '画面表示用ローカルレート（船社レート加味） START(20191021)
            sqlStat.AppendLine("         ,CASE WHEN TBL.COSTTYPE = '1' AND ISNULL(TBL.EXSHIPRATE1,0.0) > 0.0 THEN TBL.EXSHIPRATE1 ELSE TBL.LOCALRATE END AS DISPLOCALRATE")
            '画面表示用ローカルレート（船社レート加味） END(20191021)
            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT TBLSUB.*")
            sqlStat.AppendLine("      ,ISNULL(USREXR.EXRATE,'') AS EXRATE")
            sqlStat.AppendLine("      ,CASE WHEN USREXR.EXRATE IS NULL OR USREXR.EXRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX") 'ドル換算の場合はそのまま
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX / USREXR.EXRATE") 'ローカル換算の場合はドル
            sqlStat.AppendLine("        END AS USDAMOUNT_BOFORE_ROUND")
            sqlStat.AppendLine("      ,CASE WHEN USREXR.EXRATE IS NULL OR USREXR.EXRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX * USREXR.EXRATE") 'ドル換算の場合はローカル
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX") 'ローカル換算の場合はそのまま
            sqlStat.AppendLine("        END AS LOCALAMOUNT_BOFORE_ROUND")

            'UAG_x関連
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.LOCALRATE IS NULL OR TBLSUB.LOCALRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX") 'ドル換算の場合はそのまま
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX / TBLSUB.LOCALRATE") 'ローカル換算の場合はドル
            sqlStat.AppendLine("        END AS UAG_USD_BOFORE_ROUND")

            sqlStat.AppendLine("       ,CASE WHEN TBLSUB.LOCALRATE IS NULL OR TBLSUB.LOCALRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX * TBLSUB.LOCALRATE") 'ドル換算の場合はローカル
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX") 'ローカル換算の場合はそのまま
            sqlStat.AppendLine("        END AS UAG_LOCAL_BOFORE_ROUND")


            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.LOCALRATE IS NULL OR TBLSUB.LOCALRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX") 'ドル換算の場合はそのまま
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX / TBLSUB.LOCALRATE") 'ローカル換算の場合はドル
            sqlStat.AppendLine("        END AS USDAMOUNTODR_BOFORE_ROUND")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.LOCALRATE IS NULL OR TBLSUB.LOCALRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX * TBLSUB.LOCALRATE") 'ドル換算の場合はローカル
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX") 'ローカル換算の場合はそのまま
            sqlStat.AppendLine("        END AS LOCALAMOUNTODR_BOFORE_ROUND")

            sqlStat.AppendLine("      ,CNTY.DECIMALPLACES     AS DECIMALPLACES")
            sqlStat.AppendLine("      ,CNTY.ROUNDFLG          AS ROUNDFLG")
            'sqlStat.AppendLine("      ,CNTY.TAXRATE           AS TAXRATE")
            sqlStat.AppendLine("      ,CNTY_A.TAXRATE           AS TAXRATE") '消費税率はActualDate基準
            sqlStat.AppendLine("      ,LBR_A.TAXRATE          AS TAXRATE_L")
            sqlStat.AppendLine("      ,SOARATE.EXRATE         AS SOARATE")
            sqlStat.AppendLine("      ,OV2_1.EXSHIPRATE       AS EXSHIPRATE_1")
            sqlStat.AppendLine("      ,OV2_2.EXSHIPRATE       AS EXSHIPRATE_2")
            sqlStat.AppendLine("      ,USDDECIMAL.VALUE1      AS USDDECIMALPLACES")
            sqlStat.AppendLine("      ,USDDECIMAL.VALUE2      AS USDROUNDFLG")
            sqlStat.AppendLine("      ,CLD.BILLINGYMD         AS BILLINGYMD_DTM")
            sqlStat.AppendLine("      ,CASE CLD.BILLINGYMD WHEN '1900/01/01' THEN '' ELSE FORMAT(CLD.BILLINGYMD,'yyyy/MM/dd') END AS BILLINGYMD")
            sqlStat.AppendLine("      ,CLD.CLOSINGMONTH AS CLOSINGMONTH")
            sqlStat.AppendLine("      ,CLD.CALCDAY")
            sqlStat.AppendLine("      ,CLD.CURRENT_CLOSE_MONTH")
            sqlStat.AppendLine("      ,CLD.AUTOCLOSE_MONTH")
            sqlStat.AppendLine("      ,CLD.AUTOCLOSE_MONTH_LONG")
            sqlStat.AppendLine("      ,JOTSOAVL.REPORTMONTH AS JOTSOAVL_REPORTMONTH")
            sqlStat.AppendLine("      ,CASE WHEN  TBLSUB.SOAAPPDATE = '' OR TBLSUB.SOAAPPDATE >= (CASE CLD.BILLINGYMD WHEN '1900/01/01' THEN '' ELSE FORMAT(CLD.BILLINGYMD,'yyyy/MM/dd') END) THEN '' ELSE '1' END AS ISBILLINGCLOSED")

            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.BRTYPE IN ('" & C_BRTYPE.REPAIR & "','" & C_BRTYPE.NONBR & "','" & C_BRTYPE.LEASE & "')  THEN CASE WHEN TBLSUB.ACTUALDATEDTM = '1900/01/01' OR TBLSUB.ACTUALDATEDTM IS NULL THEN '-' WHEN CLD.CALCDAY <> 0 THEN FORMAT(DATEADD(month,1,DATEADD(day, -1 * CLD.CALCDAY,TBLSUB.ACTUALDATEDTM)),'yyyy/MM') ELSE FORMAT(TBLSUB.ACTUALDATEDTM,'yyyy/MM') END ")

            sqlStat.AppendLine("            WHEN TBLSUB.DTLPOLPOD  IN ('POL1','Organizer') THEN CASE WHEN TBLSUB.RECOEDDATE    = '1900/01/01' OR TBLSUB.RECOEDDATE IS NULL THEN '-' WHEN CLD.CALCDAY <> 0 THEN FORMAT(DATEADD(month,1,DATEADD(day, -1 * CLD.CALCDAY,TBLSUB.RECOEDDATE)),'yyyy/MM') ELSE FORMAT(TBLSUB.RECOEDDATE,'yyyy/MM') END ")
            'sqlStat.AppendLine("            WHEN TBLSUB.COSTCODE = 'S0102-01' THEN CASE WHEN TBLSUB.SCHEDELDATE = '1900/01/01' OR TBLSUB.SCHEDELDATE = '' OR TBLSUB.SCHEDELDATE IS NULL THEN '-' WHEN CLD.CALCDAY <> 0 THEN FORMAT(DATEADD(month,1,DATEADD(day, -1 * CLD.CALCDAY,CONVERT(date,TBLSUB.SCHEDELDATE))),'yyyy/MM') ELSE FORMAT(CONVERT(date,TBLSUB.SCHEDELDATE),'yyyy/MM') END ")
            sqlStat.AppendLine("            WHEN TBLSUB.COSTCODE = 'S0102-01' THEN CASE WHEN TBLSUB.SCHEDELDATE = '' THEN '-' WHEN CLD.CALCDAY <> 0 THEN FORMAT(DATEADD(month,1,DATEADD(day, -1 * CLD.CALCDAY,CONVERT(date,TBLSUB.SCHEDELDATE))),'yyyy/MM') ELSE FORMAT(CONVERT(date,TBLSUB.SCHEDELDATE),'yyyy/MM') END ")
            sqlStat.AppendLine("            ELSE CASE WHEN TBLSUB.ACTUALDATEDTM = '1900/01/01' OR TBLSUB.ACTUALDATEDTM IS NULL THEN '-' WHEN CLD.CALCDAY <> 0 THEN FORMAT(DATEADD(month,1,DATEADD(day, -1 * CLD.CALCDAY,TBLSUB.ACTUALDATEDTM)),'yyyy/MM') ELSE FORMAT(TBLSUB.ACTUALDATEDTM,'yyyy/MM') END END AS REPORTYMD_BASE ")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) THEN 'on' ELSE '' END AS JOT")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.CURRENCYCODE + '(' + ISNULL(CNTY.CURRENCYCODE,'') + ')' ELSE TBLSUB.CURRENCYCODE END AS DISPLAYCURRANCYCODE ")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.SOAAPPDATE = '' OR TBLSUB.SOAAPPDATE IS NULL THEN '' ELSE 'on' END AS SOACHECK")
            sqlStat.AppendLine("      ,OV2_1.EXSHIPRATE AS EXSHIPRATE1")
            sqlStat.AppendLine(" FROM(")

            sqlStat.AppendLine("SELECT '' AS OPERATION")
            sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
            sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
            sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
            sqlStat.AppendLine("     , '1' AS 'SELECT' ")
            sqlStat.AppendLine("     , '0' AS HIDDEN ")
            sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
            sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
            sqlStat.AppendLine("     , OBS.BRTYPE    AS BRTYPR")
            sqlStat.AppendLine("     , VL.TANKSEQ    AS TANKSEQ ")
            sqlStat.AppendLine("     , VL.DTLPOLPOD  AS DTLPOLPOD")
            sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
            sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
            sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
            sqlStat.AppendFormat("     , ISNULL(CST.{0},'') AS COSTNAME", textCostTblField).AppendLine()
            sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTIONID")
            sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
            sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
            sqlStat.AppendLine("     , VL.ORIGINDESTINATION AS ORIGINDESTINATION")

            sqlStat.AppendLine("     , VL.COUNTRYCODE   AS COUNTRYCODE")
            sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
            sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
            sqlStat.AppendLine("     , VL.AMOUNTBR      AS AMOUNTBR")
            sqlStat.AppendLine("     , VL.AMOUNTORD     AS AMOUNTORD")
            '            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE  WHEN '1900/01/01' THEN VL.AMOUNTORD ELSE VL.AMOUNTFIX END AS AMOUNTFIX")
            sqlStat.AppendLine("     , VL.AMOUNTFIX AS AMOUNTFIX")
            sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
            sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
            sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")

            '業者名
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSBR.NAMESEN  WHEN CST.CLASS4 = '{0}' THEN DPBR.NAMES ELSE TRBR.NAMES END AS CONTRACTORNAMEBR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSODR.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPODR.NAMES ELSE TRODR.NAMES END AS CONTRACTORNAMEODR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSFIX.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPFIX.NAMES ELSE TRFIX.NAMES END AS CONTRACTORNAMEFIX ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendLine("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CUSBR.NAMESEN IS NOT NULL)  THEN ISNULL(CUSBR.NAMESEN,'')  ELSE COALESCE(DPBR.NAMES,TRBR.NAMES,'')   END AS CONTRACTORNAMEBR ")
            sqlStat.AppendLine("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CUSODR.NAMESEN IS NOT NULL) THEN ISNULL(CUSODR.NAMESEN,'') ELSE COALESCE(DPODR.NAMES,TRODR.NAMES,'') END AS CONTRACTORNAMEODR ")
            sqlStat.AppendLine("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CUSFIX.NAMESEN IS NOT NULL) THEN ISNULL(CUSFIX.NAMESEN,'') ELSE COALESCE(DPFIX.NAMES,TRFIX.NAMES,'') END AS CONTRACTORNAMEFIX ")


            sqlStat.AppendLine("     , CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")
            sqlStat.AppendLine("     , VL.LOCALRATE      AS LOCALRATE")
            sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
            sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
            sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
            sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")
            sqlStat.AppendLine("     , VL.AMOUNTPAY      AS AMOUNTPAY")
            sqlStat.AppendLine("     , VL.LOCALPAY       AS LOCALPAY")
            sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
            sqlStat.AppendLine("     , VL.BRID           AS BRID")
            sqlStat.AppendLine("     , '1'               AS BRCOST") 'SOAの場合は削除させない
            sqlStat.AppendLine("     , ''                AS ACTYNO")
            sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")
            sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'Organizer' THEN '00000' ELSE RIGHT(VL.DTLPOLPOD,1) + REPLACE(REPLACE(VL.DTLPOLPOD,'POL','000'),'POD','001') END AS AGENTKBNSORT")
            sqlStat.AppendLine("     , CASE WHEN ISNULL(VL.DISPSEQ,'') = '' THEN '1' ")
            sqlStat.AppendLine("            ELSE '0' END AS DISPSEQISEMPTY")
            '**実績日付入力可否判定フィールドSTART**
            sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD <> 'Organizer' AND VL.BRADDEDCOST IN('','1') THEN '1'")
            sqlStat.AppendLine("            WHEN OBS.BRTYPE = '" & C_BRTYPE.REPAIR & "' THEN '1'")
            sqlStat.AppendLine("            ELSE '0'")
            sqlStat.AppendLine("       END AS CAN_ENTRY_ACTUALDATE") '実績日付入力可否
            '**実績日付入力可否判定フィールドEND**
            sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.AGENTPOL1")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.AGENTPOL2")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.AGENTPOD1")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.AGENTPOD2")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN OBS.AGENTORGANIZER")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN VL.AGENTORGANIZER")
            sqlStat.AppendLine("            ELSE '' END AS AGENT")

            sqlStat.AppendLine("     , ISNULL(CST.CLASS1, '') AS CHARGE_CLASS1")
            sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'') AS CHARGE_CLASS4")
            sqlStat.AppendLine("     , ISNULL(CST.SOACODE,'')         AS SOACODE")
            sqlStat.AppendLine("     , LEFT(ISNULL(CST.SOACODE,''),3) AS SOASHORTCODE")
            sqlStat.AppendLine("     , CASE SHIPREC.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE SHIPREC.ACTUALDATE END AS RECOEDDATE")
            sqlStat.AppendLine("     , ISNULL(SHIPREC.ACTUALDATE,'1900/01/01') AS SHIPDATE")
            sqlStat.AppendLine("     , ISNULL(DOUTREC.ACTUALDATE,'1900/01/01') AS DOUTDATE")
            sqlStat.AppendLine("     , OBS.BRTYPE AS BRTYPE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE VL.ACTUALDATE END AS ACTUALDATEDTM")
            sqlStat.AppendLine("     , VL.DATEFIELD")

            sqlStat.AppendLine("     , CST.DATA     AS DATA ")
            sqlStat.AppendLine("     , CST.JOTCODE  AS JOTCODE ")
            sqlStat.AppendLine("     , CST.ACCODE   AS ACCODE ")
            sqlStat.AppendLine("     , CASE WHEN CST.CLASS2 <> '' THEN '1' ELSE '2' END AS 'COSTTYPE' ")

            sqlStat.AppendLine("     , AH.STATUS AS STATUSCODE")

            sqlStat.AppendLine("     , VL.STYMD  AS STYMD")
            sqlStat.AppendLine("     , VL.ENDYMD AS ENDYMD")
            sqlStat.AppendLine("     , VL.REMARK AS REMARK")
            sqlStat.AppendLine("     , VL.BRADDEDCOST AS BRADDEDCOST")
            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL with(nolock) ")

            'sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OBS")
            sqlStat.AppendLine("  INNER JOIN GBT0004_ODR_BASE OBS with(nolock)")
            sqlStat.AppendLine("    ON OBS.ORDERNO = VL.ORDERNO")
            sqlStat.AppendLine("   AND OBS.DELFLG    <> @DELFLG")

            sqlStat.AppendLine("  LEFT JOIN (")
            sqlStat.AppendLine("             SELECT SHIPRECSUB.ORDERNO")
            sqlStat.AppendLine("                  , SHIPRECSUB.TANKSEQ")
            sqlStat.AppendLine("                  , MAX(SHIPRECSUB.ACTUALDATE) AS ACTUALDATE")
            sqlStat.AppendLine("               FROM GBT0005_ODR_VALUE SHIPRECSUB with(nolock) ")
            sqlStat.AppendLine("              WHERE SHIPRECSUB.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("                AND SHIPRECSUB.ACTIONID  IN ('SHIP','RPEC','RPED','RPHC','RPHD')")
            sqlStat.AppendLine("                AND SHIPRECSUB.DTLPOLPOD = 'POL1'")
            sqlStat.AppendLine("             GROUP BY SHIPRECSUB.ORDERNO,SHIPRECSUB.TANKSEQ")
            sqlStat.AppendLine("            ) SHIPREC")
            sqlStat.AppendLine("    ON SHIPREC.ORDERNO = VL.ORDERNO")
            sqlStat.AppendLine("   AND SHIPREC.TANKSEQ = VL.TANKSEQ")
            sqlStat.AppendLine("  LEFT JOIN (")
            sqlStat.AppendLine("             SELECT DOUTRECSUB.ORDERNO")
            sqlStat.AppendLine("                  , DOUTRECSUB.TANKSEQ")
            sqlStat.AppendLine("                  , MAX(DOUTRECSUB.ACTUALDATE) AS ACTUALDATE")
            sqlStat.AppendLine("               FROM GBT0005_ODR_VALUE DOUTRECSUB with(nolock) ")
            sqlStat.AppendLine("              WHERE DOUTRECSUB.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("                AND DOUTRECSUB.ACTIONID  IN ('DOUT')")
            sqlStat.AppendLine("             GROUP BY DOUTRECSUB.ORDERNO,DOUTRECSUB.TANKSEQ")
            sqlStat.AppendLine("            ) DOUTREC")
            sqlStat.AppendLine("    ON DOUTREC.ORDERNO = VL.ORDERNO")
            sqlStat.AppendLine("   AND DOUTREC.TANKSEQ = VL.TANKSEQ")
            sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST with(nolock)")
            sqlStat.AppendLine("    ON CST.COMPCODE  = @COMPCODE")
            sqlStat.AppendLine("   AND CST.COSTCODE  = VL.COSTCODE")
            sqlStat.AppendLine("   AND '1' = CASE WHEN VL.DTLPOLPOD LIKE 'POL%' AND CST.LDKBN IN ('B','L') THEN '1' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'POD%' AND CST.LDKBN IN ('B','D') THEN '1' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'Organizer' AND CST.LDKBN IN ('D') THEN '' ")
            sqlStat.AppendLine("                  ELSE '1'")
            sqlStat.AppendLine("             END")
            sqlStat.AppendLine("   AND CST.STYMD     <= @NOWDATE")
            sqlStat.AppendLine("   AND CST.ENDYMD    >= @NOWDATE")
            sqlStat.AppendLine("   AND CST.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH with(nolock)") '承認履歴
            sqlStat.AppendLine("    ON  AH.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AH.APPLYID      = VL.APPLYID")
            sqlStat.AppendLine("   AND  AH.STEP         = VL.LASTSTEP")
            sqlStat.AppendLine("   AND  AH.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV with(nolock)") 'STATUS用JOIN
            sqlStat.AppendLine("    ON  FV.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN AH.STATUS IS NOT NULL THEN AH.STATUS ")
            sqlStat.AppendLine("                               WHEN VL.AMOUNTORD <> VL.AMOUNTFIX THEN '" & C_APP_STATUS.APPAGAIN & "'")
            sqlStat.AppendLine("                               ELSE NULL")
            sqlStat.AppendLine("                           END")
            sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TRD with(nolock)")
            sqlStat.AppendLine("    ON  TRD.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  TRD.CARRIERCODE  = VL.DTLOFFICE")
            sqlStat.AppendLine("   AND  TRD.DELFLG      <> @DELFLG")

            '*BR_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = TRBR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRBR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRBR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = DPBR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPBR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPBR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = CUSBR.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSBR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSBR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSBR.DELFLG      <> @DELFLG")
            '*BR_CONTRACTOR名取得JOIN END

            '*ODR_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRODR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPODR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = CUSODR.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSODR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSODR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSODR.DELFLG      <> @DELFLG")
            '*ODR_CONTRACTOR名取得JOIN END

            '*FIX_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRFIX.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPFIX.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = CUSFIX.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSFIX.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSFIX.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSFIX.DELFLG      <> @DELFLG")
            '*FIX_CONTRACTOR名取得JOIN END

            sqlStat.AppendLine(" WHERE VL.DELFLG    <> @DELFLG")
            If Me.REPORTMONTH = "ALL" AndAlso Me.ACTUALDATEFROM <> "" AndAlso Me.ACTUALDATETO <> "" Then
                sqlStat.AppendLine("   AND VL.ACTUALDATE BETWEEN @ACTUALDATEFROM AND @ACTUALDATETO ")
            End If
            'sqlStat.AppendLine("   AND EXISTS(SELECT 1 ") '基本情報が削除されていたら対象外
            'sqlStat.AppendLine("                FROM GBT0004_ODR_BASE OBSS with(nolock) ")
            'sqlStat.AppendLine("               WHERE OBSS.ORDERNO = VL.ORDERNO")
            'sqlStat.AppendLine("                 AND OBSS.DELFLG    <> @DELFLG)")
            sqlStat.AppendLine("   AND NOT EXISTS (SELECT 1 ") 'デマレッジ終端アクションはタンク動静のみ表示
            sqlStat.AppendLine("                     FROM GBM0010_CHARGECODE CSTS with(nolock) ")
            sqlStat.AppendLine("                    WHERE CSTS.COMPCODE = @COMPCODE")
            sqlStat.AppendLine("                      AND CSTS.COSTCODE = VL.COSTCODE")
            sqlStat.AppendLine("                      AND CSTS.CLASS10  = '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("                      AND CSTS.STYMD   <= @NOWDATE")
            sqlStat.AppendLine("                      AND CSTS.ENDYMD  >= @NOWDATE")
            sqlStat.AppendLine("                      AND CSTS.DELFLG  <> @DELFLG")
            sqlStat.AppendLine("                  )")
            sqlStat.AppendLine("   AND NOT (OBS.BRTYPE IN ('SALES','OPERATION') AND VL.TANKNO = '')")
            'sqlStat.AppendLine(sqlDateCond.ToString)
            sqlStat.AppendLine(" UNION ALL")
            'ノンブレーカー分
            sqlStat.AppendLine("SELECT '' AS OPERATION")
            sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
            sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
            sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
            sqlStat.AppendLine("     , '1' AS 'SELECT' ")
            sqlStat.AppendLine("     , '0' AS HIDDEN ")
            sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
            sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
            sqlStat.AppendLine("     , ''    AS BRTYPR") 'ノンブレーカーはBase情報なし
            sqlStat.AppendLine("     , VL.TANKSEQ    AS TANKSEQ ")
            sqlStat.AppendLine("     , VL.DTLPOLPOD  AS DTLPOLPOD")
            sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
            sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
            sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
            sqlStat.AppendFormat("     , ISNULL(CST.{0},'')   AS COSTNAME", textCostTblField).AppendLine()
            sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTIONID")
            sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
            sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
            sqlStat.AppendLine("     , VL.ORIGINDESTINATION AS ORIGINDESTINATION")
            sqlStat.AppendLine("     , VL.COUNTRYCODE   AS COUNTRYCODE")
            sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
            sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
            sqlStat.AppendLine("     , VL.AMOUNTBR      AS AMOUNTBR")
            sqlStat.AppendLine("     , VL.AMOUNTORD     AS AMOUNTORD")
            '            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE  WHEN '1900/01/01' THEN VL.AMOUNTORD ELSE VL.AMOUNTFIX END AS AMOUNTFIX")
            sqlStat.AppendLine("     , VL.AMOUNTFIX AS AMOUNTFIX")
            sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
            sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
            sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")

            '業者名
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSBR.NAMESEN  WHEN CST.CLASS4 = '{0}' THEN DPBR.NAMES ELSE TRBR.NAMES END AS CONTRACTORNAMEBR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSODR.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPODR.NAMES ELSE TRODR.NAMES END AS CONTRACTORNAMEODR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSFIX.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPFIX.NAMES ELSE TRFIX.NAMES END AS CONTRACTORNAMEFIX ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSBR.NAMESEN  ELSE COALESCE(DPBR.NAMES,TRBR.NAMES,'') END AS CONTRACTORNAMEBR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSODR.NAMESEN ELSE COALESCE(DPODR.NAMES,TRODR.NAMES,'') END AS CONTRACTORNAMEODR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSFIX.NAMESEN ELSE COALESCE(DPFIX.NAMES,TRFIX.NAMES,'') END AS CONTRACTORNAMEFIX ", GBC_CHARGECLASS4.DEPOT).AppendLine()

            sqlStat.AppendLine("     , CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")
            sqlStat.AppendLine("     , VL.LOCALRATE      AS LOCALRATE")
            sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
            sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
            sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
            sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")
            sqlStat.AppendLine("     , VL.AMOUNTPAY      AS AMOUNTPAY")
            sqlStat.AppendLine("     , VL.LOCALPAY       AS LOCALPAY")
            sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
            sqlStat.AppendLine("     , VL.BRID           AS BRID")
            sqlStat.AppendLine("     , '1'               AS BRCOST") 'SOAの場合は削除させない
            sqlStat.AppendLine("     , ''                AS ACTYNO")
            sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")
            sqlStat.AppendLine("     , '000000' AS AGENTKBNSORT")
            sqlStat.AppendLine("     , ''       AS DISPSEQISEMPTY")
            sqlStat.AppendLine("     , '0'      AS CAN_ENTRY_ACTUALDATE") '実績日付入力可否

            sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENT")
            sqlStat.AppendLine("     , ISNULL(CST.CLASS1, '') AS CHARGE_CLASS1")
            sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'')  AS CHARGE_CLASS4")
            sqlStat.AppendLine("     , ISNULL(CST.SOACODE,'')         AS SOACODE")
            sqlStat.AppendLine("     , LEFT(ISNULL(CST.SOACODE,''),3) AS SOASHORTCODE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE VL.ACTUALDATE END AS RECOEDDATE")
            sqlStat.AppendLine("     , '1900/01/01' AS SHIPDATE")
            sqlStat.AppendLine("     , '1900/01/01' AS DOUTDATE")
            sqlStat.AppendLine("     , 'NONBREAKER' AS BRTYPE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE VL.ACTUALDATE END AS ACTUALDATEDTM")
            sqlStat.AppendLine("     , VL.DATEFIELD")

            sqlStat.AppendLine("     , CST.DATA     AS DATA ")
            sqlStat.AppendLine("     , CST.JOTCODE  AS JOTCODE ")
            sqlStat.AppendLine("     , CST.ACCODE   AS ACCODE ")
            sqlStat.AppendLine("     , CASE WHEN CST.CLASS2 <> '' THEN '1' ELSE '2' END AS 'COSTTYPE' ")

            sqlStat.AppendLine("     , AH.STATUS AS STATUSCODE")
            sqlStat.AppendLine("     , VL.STYMD  AS STYMD")
            sqlStat.AppendLine("     , VL.ENDYMD AS ENDYMD")
            sqlStat.AppendLine("     , VL.REMARK AS REMARK")
            sqlStat.AppendLine("     , VL.BRADDEDCOST AS BRADDEDCOST")

            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL with(nolock) ")
            sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST with(nolock)")
            sqlStat.AppendLine("    ON CST.COMPCODE  = @COMPCODE")
            sqlStat.AppendLine("   AND CST.COSTCODE  = VL.COSTCODE")
            sqlStat.AppendLine("   AND CST.NONBR     = '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("   AND CST.STYMD     <= @NOWDATE")
            sqlStat.AppendLine("   AND CST.ENDYMD    >= @NOWDATE")
            sqlStat.AppendLine("   AND CST.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH with(nolock)") '承認履歴
            sqlStat.AppendLine("    On  AH.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   And  AH.APPLYID      = VL.APPLYID")
            sqlStat.AppendLine("   And  AH.STEP         = VL.LASTSTEP")
            sqlStat.AppendLine("   And  AH.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV with(nolock)") 'STATUS用JOIN
            sqlStat.AppendLine("    On  FV.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN (AH.STATUS IS NOT NULL AND AH.STATUS <> '') THEN AH.STATUS ")
            sqlStat.AppendLine("                               WHEN CST.NONBR = '" & CONST_FLAG_YES & "' AND CST.CLASS2 <> '' THEN '" & C_APP_STATUS.APPAGAIN & "'")
            sqlStat.AppendLine("                               ELSE NULL")
            sqlStat.AppendLine("                           END")
            sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TRD with(nolock)")
            sqlStat.AppendLine("    ON  TRD.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  TRD.CARRIERCODE  = VL.DTLOFFICE")
            sqlStat.AppendLine("   AND  TRD.DELFLG      <> @DELFLG")
            '*BR_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = TRBR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRBR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRBR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = DPBR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPBR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPBR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = CUSBR.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSBR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSBR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSBR.DELFLG      <> @DELFLG")
            '*BR_CONTRACTOR名取得JOIN END

            '*ODR_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRODR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPODR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = CUSODR.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSODR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSODR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSODR.DELFLG      <> @DELFLG")
            '*ODR_CONTRACTOR名取得JOIN END

            '*FIX_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRFIX.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPFIX.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = CUSFIX.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSFIX.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSFIX.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSFIX.DELFLG      <> @DELFLG")
            '*FIX_CONTRACTOR名取得JOIN END

            sqlStat.AppendLine("WHERE VL.DELFLG     <> @DELFLG ")
            sqlStat.AppendLine("  AND VL.ORDERNO  LIKE 'NB%' ")
            sqlStat.AppendLine("  AND VL.BRID        = '' ")
            If Me.REPORTMONTH = "ALL" AndAlso Me.ACTUALDATEFROM <> "" AndAlso Me.ACTUALDATETO <> "" Then
                sqlStat.AppendLine("  AND VL.ACTUALDATE BETWEEN @ACTUALDATEFROM AND @ACTUALDATETO ")
            End If
            sqlStat.AppendLine("  ) TBLSUB")

            '締め月のレート
            sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE SOARATE with(nolock)")
            sqlStat.AppendLine("         ON SOARATE.COMPCODE      = @COMPCODE")
            sqlStat.AppendLine("        And SOARATE.COUNTRYCODE   = @COUNTRY")
            sqlStat.AppendLine("        And SOARATE.TARGETYM      = DateAdd(Day, 1 - DatePart(Day, @TARGETYM), @TARGETYM)")
            sqlStat.AppendLine("        AND SOARATE.DELFLG       <> @DELFLG")

            '消費税率（ActualDate基準）
            sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CNTY_A with(nolock)")
            sqlStat.AppendLine("         On CNTY_A.COUNTRYCODE  = TBLSUB.COUNTRYCODE")
            sqlStat.AppendLine("        And CNTY_A.STYMD       <= isnull(TBLSUB.ACTUALDATEDTM,@NOWDATE)")
            sqlStat.AppendLine("        And CNTY_A.ENDYMD      >= isnull(TBLSUB.ACTUALDATEDTM,@NOWDATE)")
            sqlStat.AppendLine("        And CNTY_A.DELFLG      <> @DELFLG ")

            'リース契約消費税(※リース項目が固定・・・)
            sqlStat.AppendLine("  LEFT JOIN GBT0011_LBR_AGREEMENT LBR_A with(nolock)")
            sqlStat.AppendLine("         ON LBR_A.RELATEDORDERNO   = TBLSUB.ORDERNO")
            sqlStat.AppendLine("        And LBR_A.DELFLG           <> @DELFLG")
            sqlStat.AppendLine("        AND TBLSUB.COSTCODE in ('S0103-01','S0103-02','S0103-03')")

            '船社レート(第１輸送)
            sqlStat.AppendLine("  LEFT JOIN GBT0007_ODR_VALUE2 OV2_1 with(nolock)")
            sqlStat.AppendLine("         ON OV2_1.ORDERNO            = TBLSUB.ORDERNO")
            sqlStat.AppendLine("        And OV2_1.DELFLG             <> @DELFLG")
            sqlStat.AppendLine("        And OV2_1.TANKSEQ            = '001'")
            sqlStat.AppendLine("        AND OV2_1.TRILATERAL         = '1'")

            '船社レート(第２輸送)
            sqlStat.AppendLine("  LEFT JOIN GBT0007_ODR_VALUE2 OV2_2 with(nolock)")
            sqlStat.AppendLine("         ON OV2_2.ORDERNO            = TBLSUB.ORDERNO")
            sqlStat.AppendLine("        And OV2_2.DELFLG             <> @DELFLG")
            sqlStat.AppendLine("        And OV2_2.TANKSEQ            = '001'")
            sqlStat.AppendLine("        AND OV2_2.TRILATERAL         = '2'")

            sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE USREXR with(nolock)")
            sqlStat.AppendLine("         ON USREXR.COMPCODE      = @COMPCODE")
            sqlStat.AppendLine("        AND USREXR.CURRENCYCODE  = (SELECT CTRSUB.CURRENCYCODE ")
            sqlStat.AppendLine("                                      FROM GBM0001_COUNTRY CTRSUB with(nolock) ")
            sqlStat.AppendLine("                                     WHERE CTRSUB.COUNTRYCODE = TBLSUB.COUNTRYCODE")
            sqlStat.AppendLine("                                       AND CTRSUB.STYMD      <= @NOWDATE")
            sqlStat.AppendLine("                                       AND CTRSUB.ENDYMD     >= @NOWDATE")
            sqlStat.AppendLine("                                       AND CTRSUB.DELFLG     <> @DELFLG )")
            sqlStat.AppendLine("        AND USREXR.TARGETYM      = DateAdd(Day, 1 - DatePart(Day, @TARGETYM), @TARGETYM)")
            sqlStat.AppendLine("        AND USREXR.DELFLG       <> @DELFLG")
            'SOA締め日JOIN START
            sqlStat.AppendLine("  LEFT JOIN W_CLOSINGDAY CLD with(nolock)")
            'sqlStat.AppendLine("         ON CLD.COUNTRYCODE      = CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) THEN '" & GBC_JOT_SOA_COUNTRY & "' ELSE TBLSUB.COUNTRYCODE END")
            'sqlStat.AppendLine("         ON CLD.COUNTRYCODE      = CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) THEN '" & GBC_JOT_SOA_COUNTRY & "' ELSE @COUNTRY END")
            If Me.COUNTRYCODE <> "" AndAlso Me.COUNTRYCODE <> "ALL" Then
                sqlStat.AppendLine("         ON CLD.COUNTRYCODE      = CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) THEN '" & GBC_JOT_SOA_COUNTRY & "' ELSE @COUNTRY END")
            Else
                sqlStat.AppendLine("         ON CLD.COUNTRYCODE      = CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA)")
                sqlStat.AppendLine("                                       THEN '" & GBC_JOT_SOA_COUNTRY & "'")
                sqlStat.AppendLine("                                       ELSE (SELECT COUNTRYCODE FROM GBM0005_TRADER WHIT (nolock) WHERE COMPCODE = @COMPCODE AND CARRIERCODE = TBLSUB.INVOICEDBY AND DELFLG <> @DELFLG ) ")
                sqlStat.AppendLine("                                   END")
            End If

            'SOA締め日JOIN END
            '国ごとの表示桁数取得用JOIN START
            'USD以外
            sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CNTY with(nolock)")
            sqlStat.AppendLine("         On CNTY.COUNTRYCODE  = TBLSUB.COUNTRYCODE")
            sqlStat.AppendLine("        And CNTY.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("        And CNTY.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("        And CNTY.DELFLG           <> @DELFLG ")
            'USD
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE USDDECIMAL with(nolock)")
            sqlStat.AppendLine("         On USDDECIMAL.COMPCODE   = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.SYSCODE    = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.CLASS      = '" & C_FIXVALUECLAS.USD_DECIMALPLACES & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.KEYCODE    = '" & GBC_CUR_USD & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.DELFLG    <> @DELFLG")

            'JOTSOA(締めたデータを取得
            'sqlStat.AppendLine("  LEFT JOIN GBT0008_JOTSOA_VALUE JOTSOAVL")
            'sqlStat.AppendLine("         ON JOTSOAVL.DATAIDODR   = TBLSUB.DATAID")
            'sqlStat.AppendLine("        AND JOTSOAVL.SOAAPPDATE <> '1900/01/01'")
            'sqlStat.AppendLine("        AND JOTSOAVL.CLOSINGMONTH = JOTSOAVL.REPORTMONTH")
            'sqlStat.AppendLine("        AND JOTSOAVL.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN (SELECT DISTINCT JOTSOAVLS.REPORTMONTH,JOTSOAVLS.DATAIDODR FROM GBT0008_JOTSOA_VALUE JOTSOAVLS with(nolock) ")
            'sqlStat.AppendLine("        WHERE JOTSOAVLS.DATAIDODR   = TBLSUB.DATAID")
            sqlStat.AppendLine("        WHERE JOTSOAVLS.SOAAPPDATE <> '1900/01/01'")
            sqlStat.AppendLine("        AND JOTSOAVLS.CLOSINGMONTH = JOTSOAVLS.REPORTMONTH")
            sqlStat.AppendLine("        AND JOTSOAVLS.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("             ) JOTSOAVL")
            sqlStat.AppendLine("         ON JOTSOAVL.DATAIDODR   = TBLSUB.DATAID")
            '国ごとの表示桁数取得用JOIN END

            '******************************
            '検索画面条件の付与 START
            '******************************
            sqlStat.AppendLine("WHERE 1 = 1 ")
            If Me.INVOICEDBYTYPE <> "" Then
                'INVOICEDBYTYPE
                Select Case Me.INVOICEDBYTYPE
                    Case "OJ" 'JOTのみ
                        sqlStat.AppendLine("  AND TBLSUB.INVOICEDBY    IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) ")
                    Case "IJ" 'JOT含む '無条件と同じ
                    Case "EJ" 'JOT含まない
                        sqlStat.AppendLine("  AND TBLSUB.INVOICEDBY    NOT IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) ")
                End Select
            End If
            '業者コード
            If Me.VENDER <> "" Then
                sqlStat.AppendLine("  AND TBLSUB.CONTRACTORFIX = @VENDER ")
            End If
            'SOATYPE
            If Me.SOATYPE <> "" Then
                sqlStat.AppendLine("AND EXISTS ( SELECT 1 FROM GBM0010_CHARGECODE CSTSUB with(nolock) ")
                sqlStat.AppendLine("              WHERE CSTSUB.COMPCODE  = @COMPCODE")
                sqlStat.AppendLine("                AND CSTSUB.COSTCODE  = TBLSUB.COSTCODE")
                sqlStat.AppendLine("                AND '1' = CASE WHEN TBLSUB.DTLPOLPOD LIKE 'POL%' AND CSTSUB.LDKBN IN ('B','L') THEN '1' ")
                sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'POD%' AND CSTSUB.LDKBN IN ('B','D') THEN '1' ")
                sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'Organizer' AND CSTSUB.LDKBN IN ('D') THEN '' ")
                sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'PO%'  THEN '' ")
                sqlStat.AppendLine("                          ELSE '1'")
                sqlStat.AppendLine("                          END")
                sqlStat.AppendLine("                AND CSTSUB.DELFLG   <> @DELFLG")
                sqlStat.AppendLine("                AND CSTSUB.SOA  = (SELECT TOP 1 FVS.VALUE3 ")
                sqlStat.AppendLine("                                     FROM COS0017_FIXVALUE FVS with(nolock) ")
                sqlStat.AppendLine("                                    WHERE FVS.COMPCODE = '" & GBC_COMPCODE_D & "'")
                sqlStat.AppendLine("                                      AND FVS.SYSCODE  = '" & C_SYSCODE_GB & "'")
                sqlStat.AppendLine("                                      AND FVS.CLASS    = 'AGENTSOA'")
                sqlStat.AppendLine("                                      AND FVS.KEYCODE  = @AGENTSOA")
                sqlStat.AppendLine("                                      AND FVS.DELFLG  <> @DELFLG)")
                sqlStat.AppendLine("           )")
            ElseIf Me.SHOULDGETALLCOST = "" Then
                sqlStat.AppendLine("AND EXISTS ( SELECT 1 FROM GBM0010_CHARGECODE CSTSUB with(nolock) ")
                sqlStat.AppendLine("              WHERE CSTSUB.COMPCODE  = @COMPCODE")
                sqlStat.AppendLine("                AND CSTSUB.COSTCODE  = TBLSUB.COSTCODE")
                sqlStat.AppendLine("                AND '1' = CASE WHEN TBLSUB.DTLPOLPOD LIKE 'POL%' AND CSTSUB.LDKBN IN ('B','L') THEN '1' ")
                sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'POD%' AND CSTSUB.LDKBN IN ('B','D') THEN '1' ")
                sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'Organizer' AND CSTSUB.LDKBN IN ('D') THEN '' ")
                sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'PO%'  THEN '' ")
                sqlStat.AppendLine("                          ELSE '1'")
                sqlStat.AppendLine("                          END")
                sqlStat.AppendLine("                AND CSTSUB.DELFLG   <> @DELFLG")
                sqlStat.AppendLine("                AND CSTSUB.SOA  IN (SELECT FVS.VALUE3 ")
                sqlStat.AppendLine("                                      FROM COS0017_FIXVALUE FVS with(nolock) ")
                sqlStat.AppendLine("                                     WHERE FVS.COMPCODE = '" & GBC_COMPCODE_D & "'")
                sqlStat.AppendLine("                                       AND FVS.SYSCODE  = '" & C_SYSCODE_GB & "'")
                sqlStat.AppendLine("                                       AND FVS.CLASS    = 'AGENTSOA'")
                sqlStat.AppendLine("                                       AND FVS.DELFLG  <> @DELFLG)")
                sqlStat.AppendLine("           )")

            End If

            '国コード
            If Me.COUNTRYCODE <> "" AndAlso Me.COUNTRYCODE <> "ALL" Then
                'sqlStat.AppendLine("  And TBLSUB.COUNTRYCODE = @COUNTRY ")
                'INVOICED BYの属する国で絞る
                sqlStat.AppendLine("  AND EXISTS ( SELECT 1 ")
                sqlStat.AppendLine("                 FROM GBM0005_TRADER TRINV with(nolock) ")
                sqlStat.AppendLine("                WHERE TRINV.COMPCODE = @COMPCODE")
                sqlStat.AppendLine("                  AND TRINV.COUNTRYCODE = @COUNTRY")
                sqlStat.AppendLine("                  AND TRINV.CARRIERCODE = TBLSUB.INVOICEDBY")
                sqlStat.AppendLine("                  AND TRINV.DELFLG <> @DELFLG")
                sqlStat.AppendLine("             )")
            End If

            'Office
            If Me.OFFICE <> "" Then
                'OFFICE
                sqlStat.AppendLine("   And (    TBLSUB.INVOICEDBY = @OFFICECODE")
                sqlStat.AppendLine("       )")
            End If

            '******************************
            '検索画面条件の付与 END
            '******************************
            sqlStat.AppendLine("  ) TBL")
            '******************************
            '計上月絞り込み条件START
            '******************************
            sqlStat.AppendLine(" WHERE 1=1")
            Dim warnDate As String = ""
            If Me.SOACLOSEPROC = "1" AndAlso Me.REPORTMONTH <> "" Then
                sqlStat.AppendLine("  AND (@REPORTMONTH <= (CASE WHEN NOT(TBL.JOTSOAVL_REPORTMONTH IS NULL OR TBL.JOTSOAVL_REPORTMONTH = '') THEN TBL.JOTSOAVL_REPORTMONTH WHEN TBL.REPORTYMD_BASE < CLOSINGMONTH THEN CLOSINGMONTH ELSE TBL.REPORTYMD_BASE END)")
                sqlStat.AppendLine("       OR (TBL.SHIPDATE <> '1900/01/01' AND (CASE WHEN NOT(TBL.JOTSOAVL_REPORTMONTH IS NULL OR TBL.JOTSOAVL_REPORTMONTH = '') THEN TBL.JOTSOAVL_REPORTMONTH WHEN TBL.REPORTYMD_BASE < CLOSINGMONTH THEN CLOSINGMONTH ELSE TBL.REPORTYMD_BASE END) = '-'))")
                warnDate = Me.REPORTMONTH & "/01"
            ElseIf Me.REPORTMONTH <> "" AndAlso Me.REPORTMONTH <> "ALL" Then
                'sqlStat.AppendLine("  AND @REPORTMONTH = (CASE WHEN TBL.REPORTYMD_BASE < CLOSINGMONTH THEN CLOSINGMONTH ELSE TBL.REPORTYMD_BASE END)")
                sqlStat.AppendLine("  AND @REPORTMONTH = (CASE WHEN NOT(TBL.JOTSOAVL_REPORTMONTH IS NULL OR TBL.JOTSOAVL_REPORTMONTH = '') THEN TBL.JOTSOAVL_REPORTMONTH WHEN TBL.REPORTYMD_BASE < CLOSINGMONTH THEN CLOSINGMONTH ELSE TBL.REPORTYMD_BASE END)")
                warnDate = Me.REPORTMONTH & "/01"
                'ElseIf Me.REPORTMONTH = "ALL" AndAlso Me.ACTUALDATEFROM <> "" AndAlso Me.ACTUALDATETO <> "" Then
                '    sqlStat.AppendLine("  AND TBL.ACTUALDATE BETWEEN @ACTUALDATEFROM AND @ACTUALDATETO ")
            End If

            '******************************
            '計上月絞り込み条件END
            '******************************
            If sortOrder <> "" Then
                sqlStat.AppendLine(" ORDER BY " & sortOrder)
            End If

            Dim dtDbResult As New DataTable
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン
                sqlCmd.CommandTimeout = 240
                'SQLパラメータ設定
                With sqlCmd.Parameters

                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    If Me.REPORTMONTH <> "" AndAlso Me.REPORTMONTH <> "ALL" Then
                        .Add("@TARGETYM", SqlDbType.Date).Value = Me.REPORTMONTH & "/01" 'Date.Now
                    Else
                        .Add("@TARGETYM", SqlDbType.Date).Value = Date.Now
                    End If

                    '.Add("@JOTAGENT", SqlDbType.NVarChar).Value = C_JOT_AGENT

                    .Add("@VENDER", SqlDbType.NVarChar).Value = Me.VENDER
                    .Add("@COUNTRY", SqlDbType.NVarChar).Value = Me.COUNTRYCODE
                    .Add("@AGENTSOA", SqlDbType.NVarChar).Value = Me.SOATYPE
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.OFFICE

                    .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = Me.REPORTMONTH
                    .Add("@ACTUALDATEFROM", SqlDbType.NVarChar).Value = Me.ACTUALDATEFROM
                    .Add("@ACTUALDATETO", SqlDbType.NVarChar).Value = Me.ACTUALDATETO
                    .Add("@WARNDATE", SqlDbType.NVarChar).Value = warnDate
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With
                '取得結果をDataTableに転送
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dtDbResult)
                End Using
            End Using
            '戻りデータテーブル設定
            If dtDbResult IsNot Nothing AndAlso dtDbResult.Rows.Count > 0 Then
                Me.ERR = C_MESSAGENO.NORMAL
            Else
                Me.ERR = C_MESSAGENO.NODATA
            End If
            Me.SOADATATABLE = dtDbResult

        Catch ex As Exception
            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try
    End Sub
    ''' <summary>
    ''' JOTSOAデータテーブル取得処理
    ''' </summary>
    Public Sub GBA00013getJOTSoaDataTable()
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Try
            '************************************
            'ソート順取得
            '************************************
            Dim sortOrder As String = ""
            If Me.SORTMAPID IsNot Nothing AndAlso Me.SORTMAPID <> "" _
               AndAlso Me.SORTMAPVARIANT IsNot Nothing AndAlso Me.SORTMAPVARIANT <> "" Then
                Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

                COA0020ProfViewSort.MAPID = Me.SORTMAPID
                COA0020ProfViewSort.VARI = Me.SORTMAPVARIANT
                COA0020ProfViewSort.TAB = ""
                COA0020ProfViewSort.COA0020getProfViewSort()
                If COA0020ProfViewSort.ERR <> C_MESSAGENO.NORMAL Then
                    Me.ERR = COA0020ProfViewSort.ERR
                    Return
                End If
                sortOrder = COA0020ProfViewSort.SORTSTR

            End If
            '************************************
            '未設定パラメータの初期化(nothing → "")
            '************************************
            ParamInit()
            '************************************
            'SQL生成
            '************************************
            'ユーザーの言語に応じ日本語⇔英語フィールド設定
            Dim textCostTblField As String = "NAMESJP"
            If COA0019Session.LANGDISP <> C_LANG.JA Then
                textCostTblField = "NAMES"
            End If

            Dim sqlStat As New StringBuilder()
            '**************************
            'WITH句生成 START
            '**************************
            sqlStat.AppendLine("WITH ")
            'JOTのエージェントを取得(INVOICED BYで判定用)
            sqlStat.AppendLine(" W_JOTAGENT AS (") 'START 
            sqlStat.AppendLine("   SELECT TR.CARRIERCODE")
            sqlStat.AppendLine("     FROM GBM0005_TRADER TR with(nolock) ")
            sqlStat.AppendLine("    WHERE TR.STYMD  <= @NOWDATE")
            sqlStat.AppendLine("      AND TR.ENDYMD >= @NOWDATE")
            sqlStat.AppendLine("      AND TR.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      AND EXISTS (SELECT 1")
            sqlStat.AppendLine("                    FROM COS0017_FIXVALUE FXV with(nolock) ")
            sqlStat.AppendLine("                   WHERE FXV.COMPCODE   = 'Default'")
            sqlStat.AppendLine("                     AND FXV.SYSCODE    = 'GB'")
            sqlStat.AppendLine("                     AND FXV.CLASS      = 'JOTCOUNTRYORG'")
            sqlStat.AppendLine("                     AND FXV.KEYCODE     = TR.MORG")
            sqlStat.AppendLine("                     AND FXV.STYMD     <= @NOWDATE")
            sqlStat.AppendLine("                     AND FXV.ENDYMD    >= @NOWDATE")
            sqlStat.AppendLine("                     AND FXV.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("                 )")
            sqlStat.AppendLine(")")
            'WITH句生成 END
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       TBL.* ")
            If sortOrder <> "" Then
                sqlStat.AppendLine("      ,ROW_NUMBER() OVER(ORDER BY " & sortOrder & ") As LINECNT")
            End If
            sqlStat.AppendLine("      ,CASE WHEN TBL.REPORTYMD_BASE < CLOSINGMONTH THEN CLOSINGMONTH ELSE TBL.REPORTYMD_BASE END AS REPORTYMD")

            sqlStat.AppendLine("      ,TBL.REPORTYMD_BASE AS REPORTYMDORG")

            'sqlStat.AppendLine("      ,CASE WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.USDAMOUNT_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBL.USDAMOUNT_BOFORE_ROUND ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.USDAMOUNT_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.USDAMOUNT_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.USDAMOUNT_BOFORE_ROUND,TBL.USDDECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.USDAMOUNT_BOFORE_ROUND END AS USDAMOUNT ")

            'sqlStat.AppendLine("      ,CASE WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.LOCALAMOUNT_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.CURRENCYCODE <> '" & GBC_CUR_USD & "' THEN TBL.LOCALAMOUNT_BOFORE_ROUND ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.LOCALAMOUNT_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.LOCALAMOUNT_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.LOCALAMOUNT_BOFORE_ROUND,TBL.DECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.LOCALAMOUNT_BOFORE_ROUND END AS LOCALAMOUNT ")

            sqlStat.AppendLine("      ,''  AS DELETEFLAG ")


            If sortOrder <> "" Then
                sqlStat.AppendLine("      ,('SYS' + right('00000' + trim(convert(char,ROW_NUMBER() OVER(ORDER BY " & sortOrder & "))), 5)) AS SYSKEY")
            End If

            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT TBLSUB.*")
            sqlStat.AppendLine("      ,ISNULL(USREXR.EXRATE,'') AS EXRATE")

            sqlStat.AppendLine("      ,CASE WHEN USREXR.EXRATE IS NULL OR USREXR.EXRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX") 'ドル換算の場合はそのまま
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX / USREXR.EXRATE") 'ローカル換算の場合はドル
            sqlStat.AppendLine("        END AS USDAMOUNT_BOFORE_ROUND")

            sqlStat.AppendLine("       ,CASE WHEN USREXR.EXRATE IS NULL OR USREXR.EXRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX * USREXR.EXRATE") 'ドル換算の場合はローカル
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX") 'ローカル換算の場合はそのまま
            sqlStat.AppendLine("        END AS LOCALAMOUNT_BOFORE_ROUND")

            sqlStat.AppendLine("      ,CNTY.DECIMALPLACES AS DECIMALPLACES")
            sqlStat.AppendLine("      ,CNTY.ROUNDFLG      AS ROUNDFLG")
            'sqlStat.AppendLine("      ,CNTY.TAXRATE       AS TAXRATE")
            sqlStat.AppendLine("      ,CNTY_A.TAXRATE       AS TAXRATE") '消費税率はActualDate基準
            sqlStat.AppendLine("      ,LBR_A.TAXRATE       AS TAXRATE_L")
            sqlStat.AppendLine("      ,SOARATE.EXRATE         AS SOARATE")
            sqlStat.AppendLine("      ,OV2_1.EXSHIPRATE       AS EXSHIPRATE_1")
            sqlStat.AppendLine("      ,OV2_2.EXSHIPRATE       AS EXSHIPRATE_2")
            sqlStat.AppendLine("      ,USDDECIMAL.VALUE1      AS USDDECIMALPLACES")
            sqlStat.AppendLine("      ,USDDECIMAL.VALUE2      AS USDROUNDFLG")
            sqlStat.AppendLine("      ,CASE CLD.BILLINGYMD WHEN '1900/01/01' THEN '' ELSE FORMAT(CLD.BILLINGYMD,'yyyy/MM/dd') END AS BILLINGYMD")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,1,BILLINGYMD),'yyyy/MM') AS CLOSINGMONTH")
            sqlStat.AppendLine("      ,CASE WHEN  TBLSUB.SOAAPPDATE = '' OR TBLSUB.SOAAPPDATE >= (CASE CLD.BILLINGYMD WHEN '1900/01/01' THEN '' ELSE FORMAT(CLD.BILLINGYMD,'yyyy/MM/dd') END) THEN '' ELSE '1' END AS ISBILLINGCLOSED")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.BRTYPE IN ('" & C_BRTYPE.REPAIR & "','" & C_BRTYPE.NONBR & "','" & C_BRTYPE.LEASE & "')  THEN CASE WHEN TBLSUB.ACTUALDATEDTM = '1900/01/01' OR TBLSUB.ACTUALDATEDTM IS NULL THEN '-' WHEN DAY(TBLSUB.ACTUALDATEDTM)>=26 THEN FORMAT(DATEADD(month,1,TBLSUB.ACTUALDATEDTM),'yyyy/MM') ELSE FORMAT(TBLSUB.ACTUALDATEDTM,'yyyy/MM') END ")
            '            sqlStat.AppendLine("            WHEN TBLSUB.RECOEDDATE IS NULL                 THEN '-' ")
            sqlStat.AppendLine("            WHEN TBLSUB.DTLPOLPOD  IN ('POL1','Organizer') THEN CASE WHEN TBLSUB.RECOEDDATE    = '1900/01/01' OR TBLSUB.RECOEDDATE IS NULL THEN '-' WHEN DAY(TBLSUB.RECOEDDATE)>=26 THEN FORMAT(DATEADD(month,1,TBLSUB.RECOEDDATE),'yyyy/MM') ELSE FORMAT(TBLSUB.RECOEDDATE,'yyyy/MM') END ")
            sqlStat.AppendLine("            ELSE CASE WHEN TBLSUB.ACTUALDATEDTM = '1900/01/01' OR TBLSUB.ACTUALDATEDTM IS NULL THEN '-' WHEN DAY(TBLSUB.ACTUALDATEDTM)>=26 THEN FORMAT(DATEADD(month,1,TBLSUB.ACTUALDATEDTM),'yyyy/MM') ELSE FORMAT(TBLSUB.ACTUALDATEDTM,'yyyy/MM') END END AS REPORTYMD_BASE ")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA with(nolock) ) THEN 'on' ELSE '' END AS JOT")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.ACTUALDATE <> '' AND TBLSUB.ACTUALDATE <= (SELECT TOP 1 FORMAT(CASE WHEN DAY(GETDATE())>=26 THEN DATEADD(month,(VALUE1 * -1) + 1,GETDATE()) ELSE DATEADD(month,VALUE1 * -1,GETDATE()) END,'yyyy/MM') + '/25' FROM COS0017_FIXVALUE with(nolock) WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='SOALOWERLIMITMONTH' AND KEYCODE='-' AND DELFLG <> @DELFLG) THEN '1' ELSE '0' END AS ISAUTOCLOSE")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.ACTUALDATE <> '' AND TBLSUB.ACTUALDATE <= (SELECT TOP 1 FORMAT(CASE WHEN DAY(GETDATE())>=26 THEN DATEADD(month,(VALUE2 * -1) + 1,GETDATE()) ELSE DATEADD(month,VALUE2 * -1,GETDATE()) END,'yyyy/MM') + '/25' FROM COS0017_FIXVALUE with(nolock) WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='SOALOWERLIMITMONTH' AND KEYCODE='-' AND DELFLG <> @DELFLG) THEN '1' ELSE '0' END AS ISAUTOCLOSELONG")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.CURRENCYCODE + '(' + ISNULL(CNTY.CURRENCYCODE,'') + ')' ELSE TBLSUB.CURRENCYCODE END AS DISPLAYCURRANCYCODE ")
            sqlStat.AppendLine(" FROM(")

            sqlStat.AppendLine("SELECT '' AS OPERATION")
            sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
            sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
            sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
            sqlStat.AppendLine("     , '1' AS 'SELECT' ")
            sqlStat.AppendLine("     , '0' AS HIDDEN ")
            sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
            sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
            sqlStat.AppendLine("     , OBS.BRTYPE    AS BRTYPR")
            sqlStat.AppendLine("     , VL.TANKSEQ    AS TANKSEQ ")
            sqlStat.AppendLine("     , VL.DTLPOLPOD  AS DTLPOLPOD")
            sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
            sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
            sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
            sqlStat.AppendFormat("     , ISNULL(CST.{0},'') AS COSTNAME", textCostTblField).AppendLine()
            sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTIONID")
            sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
            sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
            sqlStat.AppendLine("     , VL.ORIGINDESTINATION AS ORIGINDESTINATION")

            sqlStat.AppendLine("     , VL.COUNTRYCODE   AS COUNTRYCODE")
            sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
            sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
            sqlStat.AppendLine("     , VL.AMOUNTBR      AS AMOUNTBR")
            sqlStat.AppendLine("     , VL.AMOUNTORD     AS AMOUNTORD")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE  WHEN '1900/01/01' THEN VL.AMOUNTORD ELSE VL.AMOUNTFIX END AS AMOUNTFIX")
            sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
            sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
            sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")

            sqlStat.AppendLine("     , VL.REPORTMONTH AS REPORTMONTH")
            sqlStat.AppendLine("     , CASE WHEN VL.REPORTMONTH = '' THEN '' ELSE VL.REPORTMONTH + '/01' END AS REPORTMONTHH")

            sqlStat.AppendLine("     , VL.REPORTMONTHORG AS REPORTMONTHORG")

            '業者名
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSBR.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPBR.NAMES ELSE TRBR.NAMES END AS CONTRACTORNAMEBR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSODR.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPODR.NAMES ELSE TRODR.NAMES END AS CONTRACTORNAMEODR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSFIX.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPFIX.NAMES ELSE TRFIX.NAMES END AS CONTRACTORNAMEFIX ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendLine("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CUSBR.NAMESEN IS NOT NULL)  THEN ISNULL(CUSBR.NAMESEN,'')  ELSE COALESCE(DPBR.NAMES,TRBR.NAMES,'')   END AS CONTRACTORNAMEBR ")
            sqlStat.AppendLine("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CUSODR.NAMESEN IS NOT NULL) THEN ISNULL(CUSODR.NAMESEN,'') ELSE COALESCE(DPODR.NAMES,TRODR.NAMES,'') END AS CONTRACTORNAMEODR ")
            sqlStat.AppendLine("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CUSFIX.NAMESEN IS NOT NULL) THEN ISNULL(CUSFIX.NAMESEN,'') ELSE COALESCE(DPFIX.NAMES,TRFIX.NAMES,'') END AS CONTRACTORNAMEFIX ")

            sqlStat.AppendLine("     , CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")
            sqlStat.AppendLine("     , VL.LOCALRATE      AS LOCALRATE")
            sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
            sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
            sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
            sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")
            sqlStat.AppendLine("     , VL.AMOUNTPAY      AS AMOUNTPAY")
            sqlStat.AppendLine("     , VL.LOCALPAY       AS LOCALPAY")

            sqlStat.AppendLine("     , VL.UAG_USD        AS UAG_USD")
            sqlStat.AppendLine("     , VL.UAG_LOCAL      AS UAG_LOCAL")
            sqlStat.AppendLine("     , VL.USD_USD        AS USD_USD")
            sqlStat.AppendLine("     , VL.USD_LOCAL      AS USD_LOCAL")
            sqlStat.AppendLine("     , VL.LOCAL_USD      AS LOCAL_USD")
            sqlStat.AppendLine("     , VL.LOCAL_LOCAL    AS LOCAL_LOCAL")

            sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
            sqlStat.AppendLine("     , VL.BRID           AS BRID")
            sqlStat.AppendLine("     , '1'               AS BRCOST") 'SOAの場合は削除させない
            sqlStat.AppendLine("     , ''                AS ACTYNO")
            sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")
            sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'Organizer' THEN '00000' ELSE RIGHT(VL.DTLPOLPOD,1) + REPLACE(REPLACE(VL.DTLPOLPOD,'POL','000'),'POD','001') END AS AGENTKBNSORT")
            sqlStat.AppendLine("     , CASE WHEN ISNULL(VL.DISPSEQ,'') = '' THEN '1' ")
            sqlStat.AppendLine("            ELSE '0' END AS DISPSEQISEMPTY")
            sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.AGENTPOL1")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.AGENTPOL2")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.AGENTPOD1")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.AGENTPOD2")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN OBS.AGENTORGANIZER")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN VL.AGENTORGANIZER")
            sqlStat.AppendLine("            ELSE '' END AS AGENT")

            sqlStat.AppendLine("     , ISNULL(CST.CLASS1, '') AS CHARGE_CLASS1")
            sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'') AS CHARGE_CLASS4")
            sqlStat.AppendLine("     , VL.SOACODE AS SOACODE")
            sqlStat.AppendLine("     , CASE SHIPREC.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE SHIPREC.ACTUALDATE END AS RECOEDDATE")
            sqlStat.AppendLine("     , OBS.BRTYPE AS BRTYPE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE VL.ACTUALDATE END AS ACTUALDATEDTM")

            sqlStat.AppendLine("      , CST.DATA             AS DATA ")
            sqlStat.AppendLine("      , CST.JOTCODE          AS JOTCODE ")
            sqlStat.AppendLine("      , CST.ACCODE           AS ACCODE ")
            sqlStat.AppendLine("      , VL.LOCALRATESOA     AS LOCALRATESOA ")
            sqlStat.AppendLine("      , VL.AMOUNTPAYODR     AS AMOUNTPAYODR ")
            sqlStat.AppendLine("      , VL.LOCALPAYODR      AS LOCALPAYODR ")
            sqlStat.AppendLine("      , VL.CLOSINGMONTH     AS CLOSINGMONTH_JOTVAL ")
            sqlStat.AppendLine("      , VL.REMARK           AS REMARK ")
            sqlStat.AppendLine("  FROM GBT0008_JOTSOA_VALUE VL with(nolock) ")
            sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OBS with(nolock)")
            sqlStat.AppendLine("    ON OBS.ORDERNO = VL.ORDERNO")
            sqlStat.AppendLine("   AND OBS.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN (")
            sqlStat.AppendLine("             SELECT SHIPRECSUB.ORDERNO")
            sqlStat.AppendLine("                  , SHIPRECSUB.TANKSEQ")
            sqlStat.AppendLine("                  , MAX(SHIPRECSUB.ACTUALDATE) AS ACTUALDATE")
            sqlStat.AppendLine("               FROM GBT0008_JOTSOA_VALUE SHIPRECSUB with(nolock) ")
            sqlStat.AppendLine("              WHERE SHIPRECSUB.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("                AND SHIPRECSUB.ACTIONID  IN ('SHIP','RPEC','RPED','RPHC','RPHD')")
            sqlStat.AppendLine("                AND SHIPRECSUB.DTLPOLPOD = 'POL1'")
            sqlStat.AppendLine("             GROUP BY SHIPRECSUB.ORDERNO,SHIPRECSUB.TANKSEQ")
            sqlStat.AppendLine("            ) SHIPREC")
            sqlStat.AppendLine("    ON SHIPREC.ORDERNO = VL.ORDERNO")
            sqlStat.AppendLine("   AND SHIPREC.TANKSEQ = VL.TANKSEQ")
            sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST with(nolock)")
            sqlStat.AppendLine("    ON CST.COMPCODE  = @COMPCODE")
            sqlStat.AppendLine("   AND CST.COSTCODE  = VL.COSTCODE")
            sqlStat.AppendLine("   AND '1' = CASE WHEN VL.DTLPOLPOD LIKE 'POL%' AND CST.LDKBN IN ('B','L') THEN '1' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'POD%' AND CST.LDKBN IN ('B','D') THEN '1' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'Organizer' AND CST.LDKBN IN ('D') THEN '' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            sqlStat.AppendLine("             ELSE '1'")
            sqlStat.AppendLine("             END")
            sqlStat.AppendLine("   AND CST.STYMD     <= @NOWDATE")
            sqlStat.AppendLine("   AND CST.ENDYMD    >= @NOWDATE")
            sqlStat.AppendLine("   AND CST.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH with(nolock)") '承認履歴
            sqlStat.AppendLine("    ON  AH.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AH.APPLYID      = VL.APPLYID")
            sqlStat.AppendLine("   AND  AH.STEP         = VL.LASTSTEP")
            sqlStat.AppendLine("   AND  AH.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV with(nolock)") 'STATUS用JOIN
            sqlStat.AppendLine("    ON  FV.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN AH.STATUS IS NOT NULL THEN AH.STATUS ")
            sqlStat.AppendLine("                               WHEN VL.AMOUNTORD <> VL.AMOUNTFIX THEN '" & C_APP_STATUS.APPAGAIN & "'")
            sqlStat.AppendLine("                               ELSE NULL")
            sqlStat.AppendLine("                           END")
            sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TRD with(nolock)")
            sqlStat.AppendLine("    ON  TRD.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  TRD.CARRIERCODE  = VL.DTLOFFICE")
            sqlStat.AppendLine("   AND  TRD.DELFLG      <> @DELFLG")

            '*BR_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = TRBR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRBR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRBR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = DPBR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPBR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPBR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = CUSBR.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSBR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSBR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSBR.DELFLG      <> @DELFLG")
            '*BR_CONTRACTOR名取得JOIN END

            '*ODR_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRODR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPODR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = CUSODR.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSODR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSODR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSODR.DELFLG      <> @DELFLG")
            '*ODR_CONTRACTOR名取得JOIN END

            '*FIX_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRFIX.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPFIX.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = CUSFIX.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSFIX.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSFIX.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSFIX.DELFLG      <> @DELFLG")
            '*FIX_CONTRACTOR名取得JOIN END

            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TR with(nolock)")
            sqlStat.AppendLine("        ON  VL.AGENTORGANIZER = TR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TR.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("      LEFT JOIN GBM0001_COUNTRY CT with(nolock)")
            sqlStat.AppendLine("        ON  VL.COUNTRYCODE = CT.COUNTRYCODE ")
            sqlStat.AppendLine("       AND  CT.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CT.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  CT.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  CT.DELFLG      <> @DELFLG")

            sqlStat.AppendLine(" WHERE VL.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("   AND EXISTS(SELECT 1 ") '基本情報が削除されていたら対象外
            sqlStat.AppendLine("                FROM GBT0004_ODR_BASE OBSS with(nolock) ")
            sqlStat.AppendLine("               WHERE OBSS.ORDERNO = VL.ORDERNO")
            sqlStat.AppendLine("                 AND OBSS.DELFLG    <> @DELFLG)")
            sqlStat.AppendLine("   AND NOT EXISTS (SELECT 1 ") 'デマレッジ終端アクションはタンク動静のみ表示
            sqlStat.AppendLine("                     FROM GBM0010_CHARGECODE CSTS with(nolock) ")
            sqlStat.AppendLine("                    WHERE CSTS.COMPCODE = @COMPCODE")
            sqlStat.AppendLine("                      AND CSTS.COSTCODE = VL.COSTCODE")
            sqlStat.AppendLine("                      AND CSTS.CLASS10  = '1'")
            sqlStat.AppendLine("                      AND CSTS.STYMD   <= @NOWDATE")
            sqlStat.AppendLine("                      AND CSTS.ENDYMD  >= @NOWDATE")
            sqlStat.AppendLine("                      AND CSTS.DELFLG  <> @DELFLG")
            sqlStat.AppendLine("                  )")
            sqlStat.AppendLine(" UNION ALL")
            'ノンブレーカー分
            sqlStat.AppendLine("SELECT '' AS OPERATION")
            sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
            sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
            sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
            sqlStat.AppendLine("     , '1' AS 'SELECT' ")
            sqlStat.AppendLine("     , '0' AS HIDDEN ")
            sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
            sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
            sqlStat.AppendLine("     , ''    AS BRTYPR") 'ノンブレーカーはBase情報なし
            sqlStat.AppendLine("     , VL.TANKSEQ    AS TANKSEQ ")
            sqlStat.AppendLine("     , VL.DTLPOLPOD  AS DTLPOLPOD")
            sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
            sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
            sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
            sqlStat.AppendFormat("     , ISNULL(CST.{0},'')   AS COSTNAME", textCostTblField).AppendLine()
            sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTIONID")
            sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
            sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
            sqlStat.AppendLine("     , VL.ORIGINDESTINATION AS ORIGINDESTINATION")
            sqlStat.AppendLine("     , VL.COUNTRYCODE   AS COUNTRYCODE")
            sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
            sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
            sqlStat.AppendLine("     , VL.AMOUNTBR      AS AMOUNTBR")
            sqlStat.AppendLine("     , VL.AMOUNTORD     AS AMOUNTORD")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE  WHEN '1900/01/01' THEN VL.AMOUNTORD ELSE VL.AMOUNTFIX END AS AMOUNTFIX")
            sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
            sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
            sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")

            sqlStat.AppendLine("     , VL.REPORTMONTH AS REPORTMONTH")
            sqlStat.AppendLine("     , CASE WHEN VL.REPORTMONTH = '' THEN '' ELSE VL.REPORTMONTH + '/01' END AS REPORTMONTHH")

            sqlStat.AppendLine("     , VL.REPORTMONTHORG AS REPORTMONTHORG")

            '業者名
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSBR.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPBR.NAMES ELSE TRBR.NAMES END AS CONTRACTORNAMEBR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSODR.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPODR.NAMES ELSE TRODR.NAMES END AS CONTRACTORNAMEODR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            'sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSFIX.NAMESEN WHEN CST.CLASS4 = '{0}' THEN DPFIX.NAMES ELSE TRFIX.NAMES END AS CONTRACTORNAMEFIX ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSBR.NAMESEN ELSE COALESCE(DPBR.NAMES,TRBR.NAMES,'') END AS CONTRACTORNAMEBR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSODR.NAMESEN ELSE COALESCE(DPODR.NAMES,TRODR.NAMES,'') END AS CONTRACTORNAMEODR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS2 <> '' THEN CUSFIX.NAMESEN ELSE COALESCE(DPFIX.NAMES,TRFIX.NAMES,'') END AS CONTRACTORNAMEFIX ", GBC_CHARGECLASS4.DEPOT).AppendLine()

            sqlStat.AppendLine("     , CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")
            sqlStat.AppendLine("     , VL.LOCALRATE      AS LOCALRATE")
            sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
            sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
            sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
            sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")
            sqlStat.AppendLine("     , VL.AMOUNTPAY      AS AMOUNTPAY")
            sqlStat.AppendLine("     , VL.LOCALPAY       AS LOCALPAY")

            sqlStat.AppendLine("     , VL.UAG_USD        AS UAG_USD")
            sqlStat.AppendLine("     , VL.UAG_LOCAL      AS UAG_LOCAL")
            sqlStat.AppendLine("     , VL.USD_USD        AS USD_USD")
            sqlStat.AppendLine("     , VL.USD_LOCAL      AS USD_LOCAL")
            sqlStat.AppendLine("     , VL.LOCAL_USD      AS LOCAL_USD")
            sqlStat.AppendLine("     , VL.LOCAL_LOCAL    AS LOCAL_LOCAL")

            sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
            sqlStat.AppendLine("     , VL.BRID           AS BRID")
            sqlStat.AppendLine("     , '1'               AS BRCOST") 'SOAの場合は削除させない
            sqlStat.AppendLine("     , ''                AS ACTYNO")
            sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")
            sqlStat.AppendLine("     , '000000' AS AGENTKBNSORT")
            sqlStat.AppendLine("     , ''       AS DISPSEQISEMPTY")
            sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENT")
            sqlStat.AppendLine("     , ISNULL(CST.CLASS1, '') AS CHARGE_CLASS1")
            sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'')  AS CHARGE_CLASS4")
            sqlStat.AppendLine("     , VL.SOACODE AS SOACODE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE VL.ACTUALDATE END AS RECOEDDATE")
            sqlStat.AppendLine("     , 'NONBREAKER' AS BRTYPE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE VL.ACTUALDATE END AS ACTUALDATEDTM")

            sqlStat.AppendLine("      , CST.DATA             AS DATA ")
            sqlStat.AppendLine("      , CST.JOTCODE          AS JOTCODE ")
            sqlStat.AppendLine("      , CST.ACCODE           AS ACCODE ")
            sqlStat.AppendLine("      , VL.LOCALRATESOA     AS LOCALRATESOA ")
            sqlStat.AppendLine("      , VL.AMOUNTPAYODR     AS AMOUNTPAYODR ")
            sqlStat.AppendLine("      , VL.LOCALPAYODR      AS LOCALPAYODR ")
            sqlStat.AppendLine("      , VL.CLOSINGMONTH     AS CLOSINGMONTH_JOTVAL ")
            sqlStat.AppendLine("      , VL.REMARK           AS REMARK ")
            sqlStat.AppendLine("  FROM GBT0008_JOTSOA_VALUE VL with(nolock) ")
            sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST with(nolock)")
            sqlStat.AppendLine("    ON CST.COMPCODE  = @COMPCODE")
            sqlStat.AppendLine("   AND CST.COSTCODE  = VL.COSTCODE")
            sqlStat.AppendLine("   AND CST.NONBR     = '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("   AND CST.STYMD     <= @NOWDATE")
            sqlStat.AppendLine("   AND CST.ENDYMD    >= @NOWDATE")
            sqlStat.AppendLine("   AND CST.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH with(nolock)") '承認履歴
            sqlStat.AppendLine("    On  AH.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   And  AH.APPLYID      = VL.APPLYID")
            sqlStat.AppendLine("   And  AH.STEP         = VL.LASTSTEP")
            sqlStat.AppendLine("   And  AH.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV with(nolock)") 'STATUS用JOIN
            sqlStat.AppendLine("    On  FV.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN (AH.STATUS IS NOT NULL AND AH.STATUS <> '') THEN AH.STATUS ")
            sqlStat.AppendLine("                               WHEN CST.NONBR = '" & CONST_FLAG_YES & "' AND CST.CLASS2 <> '' THEN '" & C_APP_STATUS.APPAGAIN & "'")
            sqlStat.AppendLine("                               ELSE NULL")
            sqlStat.AppendLine("                           END")
            sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TRD with(nolock)")
            sqlStat.AppendLine("    ON  TRD.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  TRD.CARRIERCODE  = VL.DTLOFFICE")
            sqlStat.AppendLine("   AND  TRD.DELFLG      <> @DELFLG")
            '*BR_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = TRBR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRBR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRBR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = DPBR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPBR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPBR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSBR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = CUSBR.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSBR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSBR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSBR.DELFLG      <> @DELFLG")
            '*BR_CONTRACTOR名取得JOIN END

            '*ODR_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRODR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPODR.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSODR with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = CUSODR.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSODR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSODR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSODR.DELFLG      <> @DELFLG")
            '*ODR_CONTRACTOR名取得JOIN END

            '*FIX_CONTRACTOR名取得JOIN START
            sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRFIX.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPFIX.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSFIX with(nolock)")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = CUSFIX.CUSTOMERCODE ")
            sqlStat.AppendLine("       AND  CUSFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CUSFIX.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CUSFIX.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CUSFIX.DELFLG      <> @DELFLG")
            '*FIX_CONTRACTOR名取得JOIN END

            sqlStat.AppendLine("WHERE VL.DELFLG     <> @DELFLG ")
            sqlStat.AppendLine("  AND VL.ORDERNO  LIKE 'NB%' ")
            sqlStat.AppendLine("  AND VL.BRID        = '' ")
            sqlStat.AppendLine("  ) TBLSUB")

            '国ごとの表示桁数取得用JOIN START
            'USD以外
            sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CNTY with(nolock)")
            sqlStat.AppendLine("         ON CNTY.COUNTRYCODE      = TBLSUB.COUNTRYCODE")
            sqlStat.AppendLine("        AND CNTY.STYMD       <= @NOWDATE")
            sqlStat.AppendLine("        AND CNTY.ENDYMD      >= @NOWDATE")
            sqlStat.AppendLine("        AND CNTY.DELFLG           <> @DELFLG")
            'USD
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE USDDECIMAL with(nolock)")
            sqlStat.AppendLine("         ON USDDECIMAL.COMPCODE   = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.SYSCODE    = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.CLASS      = '" & C_FIXVALUECLAS.USD_DECIMALPLACES & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.KEYCODE    = '" & GBC_CUR_USD & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.DELFLG    <> @DELFLG")

            '締め月のレート
            sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE SOARATE with(nolock)")
            sqlStat.AppendLine("         ON SOARATE.COMPCODE      = @COMPCODE")
            sqlStat.AppendLine("        And SOARATE.COUNTRYCODE   = @COUNTRYCODE")
            sqlStat.AppendLine("        And SOARATE.TARGETYM      = DateAdd(Day, 1 - DatePart(Day, @TARGETYM), @TARGETYM)")
            sqlStat.AppendLine("        AND SOARATE.DELFLG       <> @DELFLG")

            '消費税率（ActualDate基準）
            sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CNTY_A with(nolock)")
            sqlStat.AppendLine("         On CNTY_A.COUNTRYCODE  = TBLSUB.COUNTRYCODE")
            sqlStat.AppendLine("        And CNTY_A.STYMD       <= isnull(TBLSUB.ACTUALDATEDTM,@NOWDATE)")
            sqlStat.AppendLine("        And CNTY_A.ENDYMD      >= isnull(TBLSUB.ACTUALDATEDTM,@NOWDATE)")
            sqlStat.AppendLine("        And CNTY_A.DELFLG      <> @DELFLG ")

            'リース契約消費税(※リース項目が固定・・・)
            sqlStat.AppendLine("  LEFT JOIN GBT0011_LBR_AGREEMENT LBR_A with(nolock)")
            sqlStat.AppendLine("         ON LBR_A.RELATEDORDERNO   = TBLSUB.ORDERNO")
            sqlStat.AppendLine("        And LBR_A.DELFLG           <> @DELFLG")
            sqlStat.AppendLine("        AND TBLSUB.COSTCODE in ('S0103-01','S0103-02','S0103-03')")

            '船社レート(第１輸送)
            sqlStat.AppendLine("  LEFT JOIN GBT0007_ODR_VALUE2 OV2_1 with(nolock)")
            sqlStat.AppendLine("         ON OV2_1.ORDERNO            = TBLSUB.ORDERNO")
            sqlStat.AppendLine("        And OV2_1.DELFLG             <> @DELFLG")
            sqlStat.AppendLine("        And OV2_1.TANKSEQ            = '001'")
            sqlStat.AppendLine("        AND OV2_1.TRILATERAL         = '1'")

            '船社レート(第２輸送)
            sqlStat.AppendLine("  LEFT JOIN GBT0007_ODR_VALUE2 OV2_2 with(nolock)")
            sqlStat.AppendLine("         ON OV2_2.ORDERNO            = TBLSUB.ORDERNO")
            sqlStat.AppendLine("        And OV2_2.DELFLG             <> @DELFLG")
            sqlStat.AppendLine("        And OV2_2.TANKSEQ            = '001'")
            sqlStat.AppendLine("        AND OV2_2.TRILATERAL         = '2'")
            '国ごとの表示桁数取得用JOIN END

            sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE USREXR with(nolock)")
            sqlStat.AppendLine("         ON USREXR.COMPCODE      = @COMPCODE")
            '
            sqlStat.AppendLine("        AND USREXR.CURRENCYCODE  = (SELECT CTRSUB.CURRENCYCODE ")
            sqlStat.AppendLine("                                      FROM GBM0001_COUNTRY CTRSUB with(nolock) ")
            sqlStat.AppendLine("                                     WHERE CTRSUB.COUNTRYCODE = TBLSUB.COUNTRYCODE")
            sqlStat.AppendLine("                                       AND CTRSUB.STYMD      <= @NOWDATE")
            sqlStat.AppendLine("                                       AND CTRSUB.ENDYMD     >= @NOWDATE")
            sqlStat.AppendLine("                                       AND CTRSUB.DELFLG     <> @DELFLG )")
            sqlStat.AppendLine("        AND USREXR.TARGETYM      = DateAdd(Day, 1 - DatePart(Day, @TARGETYM), @TARGETYM)")
            sqlStat.AppendLine("        AND USREXR.DELFLG       <> @DELFLG")
            'SOA締め日JOIN START
            sqlStat.AppendLine("  LEFT JOIN GBT0006_CLOSINGDAY CLD with(nolock)")
            'sqlStat.AppendLine("         ON CLD.COUNTRYCODE      = CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) THEN '" & GBC_JOT_SOA_COUNTRY & "' ELSE TBLSUB.COUNTRYCODE END")
            'sqlStat.AppendLine("         ON CLD.COUNTRYCODE      = CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA with(nolock) ) THEN '" & GBC_JOT_SOA_COUNTRY & "' ELSE @COUNTRYCODE END")
            If Me.COUNTRYCODE <> "" AndAlso Me.COUNTRYCODE <> "ALL" Then
                sqlStat.AppendLine("         ON CLD.COUNTRYCODE      = CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) THEN '" & GBC_JOT_SOA_COUNTRY & "' ELSE @COUNTRYCODE END")
            Else
                sqlStat.AppendLine("         ON CLD.COUNTRYCODE      = CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA)")
                sqlStat.AppendLine("                                       THEN '" & GBC_JOT_SOA_COUNTRY & "'")
                sqlStat.AppendLine("                                       ELSE (SELECT COUNTRYCODE FROM GBM0005_TRADER WHIT (nolock) WHERE COMPCODE = @COMPCODE AND CARRIERCODE = TBLSUB.INVOICEDBY AND DELFLG <> @DELFLG ) ")
                sqlStat.AppendLine("                                   END")
            End If
            sqlStat.AppendLine("        AND CLD.STYMD           <= @NOWDATE")
            sqlStat.AppendLine("        AND CLD.ENDYMD          >= @NOWDATE")
            sqlStat.AppendLine("        AND CLD.DELFLG          <> @DELFLG")
            sqlStat.AppendLine("        AND EXISTS (SELECT CLDS.COUNTRYCODE,MAX(CLDS.REPORTMONTH) AS REPORTMONTH")
            sqlStat.AppendLine("                      FROM GBT0006_CLOSINGDAY CLDS with(nolock) ")
            sqlStat.AppendLine("                     WHERE CLDS.STYMD          <= @NOWDATE")
            sqlStat.AppendLine("                       AND CLDS.ENDYMD         >= @NOWDATE")
            sqlStat.AppendLine("                       AND CLDS.DELFLG          <> @DELFLG")
            sqlStat.AppendLine("                     GROUP BY CLDS.COUNTRYCODE")
            sqlStat.AppendLine("                    HAVING CLDS.COUNTRYCODE      = CLD.COUNTRYCODE")
            sqlStat.AppendLine("                       AND MAX(CLDS.REPORTMONTH) = CLD.REPORTMONTH")
            sqlStat.AppendLine("                   )")
            'SOA締め日JOIN END

            '******************************
            '検索画面条件の付与 START
            '******************************
            sqlStat.AppendLine("WHERE 1 = 1 ")
            If Me.INVOICEDBYTYPE <> "" Then
                'INVOICEDBYTYPE
                Select Case Me.INVOICEDBYTYPE
                    Case "OJ" 'JOTのみ
                        sqlStat.AppendLine("  AND TBLSUB.INVOICEDBY    IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) ")
                    Case "IJ" 'JOT含む '無条件と同じ
                    Case "EJ" 'JOT含まない
                        sqlStat.AppendLine("  AND TBLSUB.INVOICEDBY    NOT IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) ")
                End Select
            End If
            sqlStat.AppendLine("AND EXISTS ( SELECT 1 FROM GBM0010_CHARGECODE CSTSUB with(nolock) ")
            sqlStat.AppendLine("              WHERE CSTSUB.COMPCODE  = @COMPCODE")
            sqlStat.AppendLine("                AND CSTSUB.COSTCODE  = TBLSUB.COSTCODE")
            sqlStat.AppendLine("                AND '1' = CASE WHEN TBLSUB.DTLPOLPOD LIKE 'POL%' AND CSTSUB.LDKBN IN ('B','L') THEN '1' ")
            sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'POD%' AND CSTSUB.LDKBN IN ('B','D') THEN '1' ")
            sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'Organizer' AND CSTSUB.LDKBN IN ('D') THEN '' ")
            sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            sqlStat.AppendLine("                          ELSE '1'")
            sqlStat.AppendLine("                          END")
            sqlStat.AppendLine("                AND CSTSUB.DELFLG   <> @DELFLG")
            'sqlStat.AppendLine("                AND CSTSUB.SOA  IN (SELECT FVS.VALUE3 ")
            'sqlStat.AppendLine("                                      FROM COS0017_FIXVALUE FVS ")
            'sqlStat.AppendLine("                                     WHERE FVS.COMPCODE = '" & GBC_COMPCODE_D & "'")
            'sqlStat.AppendLine("                                       AND FVS.SYSCODE  = '" & C_SYSCODE_GB & "'")
            'sqlStat.AppendLine("                                       AND FVS.CLASS    = 'AGENTSOA'")
            'sqlStat.AppendLine("                                       AND FVS.DELFLG  <> @DELFLG)")
            sqlStat.AppendLine("           )")

            '******************************
            '非表示費用コード END
            '******************************
            If Me.COUNTRYCODE <> "" Then
                'sqlStat.AppendLine("  AND @COUNTRYCODE = TBL.COUNTRYCODE")
                'INVOICED BYの属する国で絞る
                sqlStat.AppendLine("  AND EXISTS ( SELECT 1 ")
                sqlStat.AppendLine("                 FROM GBM0005_TRADER TRINV with(nolock) ")
                sqlStat.AppendLine("                WHERE TRINV.COMPCODE = @COMPCODE")
                sqlStat.AppendLine("                  AND TRINV.COUNTRYCODE = @COUNTRYCODE")
                sqlStat.AppendLine("                  AND TRINV.CARRIERCODE = TBLSUB.INVOICEDBY")
                sqlStat.AppendLine("                  AND TRINV.DELFLG <> @DELFLG")
                sqlStat.AppendLine("             )")
            End If

            sqlStat.AppendLine("  ) TBL")
            '******************************
            '計上月絞り込み条件START
            '******************************
            sqlStat.AppendLine(" WHERE 1=1")
            If Me.REPORTMONTH <> "" Then
                sqlStat.AppendLine("  AND @REPORTMONTH = TBL.REPORTMONTH")
                sqlStat.AppendLine("  AND @REPORTMONTH = TBL.CLOSINGMONTH_JOTVAL")
            End If


            '******************************
            '計上月絞り込み条件END
            '******************************
            If sortOrder <> "" Then
                sqlStat.AppendLine(" ORDER BY " & sortOrder)
            End If

            Dim dtDbResult As New DataTable
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン
                sqlCmd.CommandTimeout = 240
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
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    '.Add("@SOAAPPDATEFROM", SqlDbType.Date).Value = soaAppDateFrom
                    '.Add("@SOAAPPDATETO", SqlDbType.Date).Value = soaAppDateTo
                    '.Add("@TARGETYM", SqlDbType.Date).Value = Date.Now
                    If Me.REPORTMONTH <> "" AndAlso Me.REPORTMONTH <> "ALL" Then
                        .Add("@TARGETYM", SqlDbType.Date).Value = Me.REPORTMONTH & "/01" 'Date.Now
                    Else
                        .Add("@TARGETYM", SqlDbType.Date).Value = Date.Now
                    End If
                    '.Add("@JOTAGENT", SqlDbType.NVarChar).Value = C_JOT_AGENT

                    If Me.REPORTMONTH <> "" Then
                        .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = Me.REPORTMONTH
                    End If

                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = Me.COUNTRYCODE
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                End With
                '取得結果をDataTableに転送
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dtDbResult)
                End Using
            End Using

            '戻りデータテーブル設定
            If dtDbResult IsNot Nothing AndAlso dtDbResult.Rows.Count > 0 Then
                Me.ERR = C_MESSAGENO.NORMAL
            Else
                Me.ERR = C_MESSAGENO.NODATA
            End If
            Me.SOADATATABLE = dtDbResult

        Catch ex As Exception
            Me.ERR = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = Me.ERR
            COA0003LogFile.COA0003WriteLog()

        End Try
    End Sub
    ''' <summary>
    ''' 入力パラメータ（プロパティの初期化）
    ''' </summary>
    Private Sub ParamInit()
        If Me.COUNTRYCODE Is Nothing Then
            Me.COUNTRYCODE = ""
        End If
        If Me.INVOICEDBYTYPE Is Nothing Then
            Me.INVOICEDBYTYPE = ""
        End If
        If Me.OFFICE Is Nothing Then
            Me.OFFICE = ""
        End If
        If Me.REPORTMONTH Is Nothing Then
            Me.REPORTMONTH = ""
        End If
        If Me.ACTUALDATEFROM Is Nothing Then
            Me.ACTUALDATEFROM = ""
        End If
        If Me.ACTUALDATETO Is Nothing Then
            Me.ACTUALDATETO = ""
        End If
        If Me.SOATYPE Is Nothing Then
            Me.SOATYPE = ""
        End If
        If Me.VENDER Is Nothing Then
            Me.VENDER = ""
        End If
        If Me.SHOULDGETALLCOST Is Nothing Then
            Me.SHOULDGETALLCOST = ""
        End If
    End Sub

End Structure

''' <summary>
''' プロダクトマスタ関連
''' </summary>
Public Structure GBA00014Product

    ''' <summary>
    ''' プロダクトのコード、名称リスト取得
    ''' </summary>
    ''' <param name="productCode"></param>
    ''' <param name="enabled">使用可否（未指定時は無条件）</param>
    ''' <returns>コード、コード+":"+名称、名称、IMDGCODE、UNNO、GRAVITY、HAZARDCLASSのデータテーブル</returns>
    Public Shared Function GBA00014getProductCodeValue(Optional productCode As String = "", Optional enabled As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        Dim textField As String = "PRODUCTNAME"

        'SQL文作成(TODO:ORGもキーだが今のところ未設定)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT rtrim(PRODUCTCODE) AS CODE")
        sqlStat.AppendFormat("      ,rtrim({0}) AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,rtrim(PRODUCTCODE) + ':' + rtrim({0})  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("        ,rtrim(IMDGCODE) AS IMDGCODE")
        sqlStat.AppendLine("        ,rtrim(UNNO) AS UNNO")
        sqlStat.AppendLine("        ,rtrim(GRAVITY) AS GRAVITY")
        sqlStat.AppendLine("        ,rtrim(HAZARDCLASS) AS HAZARDCLASS")
        sqlStat.AppendLine("  FROM GBM0008_PRODUCT")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")

        If productCode <> "" Then
            sqlStat.AppendLine("   AND PRODUCTCODE    = @PRODUCTCODE")
        End If

        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        If enabled <> "" Then
            sqlStat.AppendLine("   AND ENABLED      = @ENABLED")
        End If
        sqlStat.AppendLine("ORDER BY PRODUCTCODE ")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@PRODUCTCODE", SqlDbType.NVarChar).Value = productCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar).Value = enabled
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
End Structure
''' <summary>
''' リース関連処理
''' </summary>
Public Structure GBA00015Lease
    Private Const CONST_CONTRACTSEQ_NAME = "GBQ0010_LEASECONTRUCTNO"
    Private Const CONST_AGREEMENTSEQ_NAME = "GBQ0011_LEASEAGREEMENTNO"
    Private Const CONST_TBL_AGREEMENT = "GBT0011_LBR_AGREEMENT"
    ''' <summary>
    ''' 契約書Noを取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewContractNo(Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim contractNo As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If

            Dim sqlStat As New Text.StringBuilder
            sqlStat.AppendLine("Select  'LSC' ")
            sqlStat.AppendLine("      + left(convert(char,getdate(),12),4)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR " & CONST_CONTRACTSEQ_NAME & ")),4)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + (SELECT VALUE1")
            sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
            sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
            sqlStat.AppendLine("            AND KEYCODE = @KEYCODE)")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = C_SERVERSEQ
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVname
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get GetNewContractNo error")
                    End If

                    contractNo = Convert.ToString(dt.Rows(0).Item(0))
                End Using
            End Using
            Return contractNo
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try

    End Function
    ''' <summary>
    ''' 協定書No取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewAgreementNo(Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim agreementNo As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If

            Dim sqlStat As New Text.StringBuilder
            sqlStat.AppendLine("Select  'LSA' ")
            sqlStat.AppendLine("      + left(convert(char,getdate(),12),4)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR " & CONST_AGREEMENTSEQ_NAME & ")),4)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + (SELECT VALUE1")
            sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
            sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
            sqlStat.AppendLine("            AND KEYCODE = @KEYCODE)")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = C_SERVERSEQ
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVname
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get GetNewContractNo error")
                    End If

                    agreementNo = Convert.ToString(dt.Rows(0).Item(0))
                End Using
            End Using
            Return agreementNo
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try
    End Function
    ''' <summary>
    ''' 協定書テーブル新規追加
    ''' </summary>
    ''' <param name="contructNo"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="tran"></param>
    ''' <param name="procDate"></param>
    Public Shared Sub InsertAgreement(contructNo As String, dtContract As DataTable, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False

        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Try
            Dim newAgreementNo As String = GBA00015Lease.GetNewAgreementNo(sqlCon, tran)
            Dim drContract As DataRow = dtContract.Rows(0)
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendFormat("INSERT INTO {0} (", CONST_TBL_AGREEMENT).AppendLine()
            sqlStat.AppendLine("   CONTRACTNO ")
            sqlStat.AppendLine("  ,AGREEMENTNO ")
            sqlStat.AppendLine("  ,STYMD  ")
            sqlStat.AppendLine("  ,LEASEPAYMENTTYPE")
            sqlStat.AppendLine("  ,LEASEPAYMENTKIND")
            sqlStat.AppendLine("  ,AUTOEXTEND")
            sqlStat.AppendLine("  ,TAXKIND")
            sqlStat.AppendLine("  ,DELFLG")
            sqlStat.AppendLine("  ,INITYMD")
            sqlStat.AppendLine("  ,UPDYMD")
            sqlStat.AppendLine("  ,UPDUSER")
            sqlStat.AppendLine("  ,UPDTERMID")
            sqlStat.AppendLine("  ,RECEIVEYMD")
            sqlStat.AppendLine(") VALUES (")
            sqlStat.AppendLine("   @CONTRACTNO ")
            sqlStat.AppendLine("  ,@AGREEMENTNO ")
            sqlStat.AppendLine("  ,@STYMD  ")
            sqlStat.AppendLine("  ,@LEASEPAYMENTTYPE")
            sqlStat.AppendLine("  ,@LEASEPAYMENTKIND")
            sqlStat.AppendLine("  ,@AUTOEXTEND")
            sqlStat.AppendLine("  ,@TAXKIND")
            sqlStat.AppendLine("  ,@DELFLG")
            sqlStat.AppendLine("  ,@INITYMD")
            sqlStat.AppendLine("  ,@UPDYMD")
            sqlStat.AppendLine("  ,@UPDUSER")
            sqlStat.AppendLine("  ,@UPDTERMID")
            sqlStat.AppendLine("  ,@RECEIVEYMD")
            sqlStat.AppendLine(")")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = contructNo
                    .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = newAgreementNo
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@LEASEPAYMENTTYPE", SqlDbType.NVarChar).Value = Convert.ToString(drContract("LEASEPAYMENTTYPE"))
                    .Add("@LEASEPAYMENTKIND", SqlDbType.NVarChar).Value = Convert.ToString(drContract("LEASEPAYMENTKIND"))
                    .Add("@AUTOEXTEND", SqlDbType.NVarChar).Value = Convert.ToString(drContract("AUTOEXTEND"))
                    .Add("@TAXKIND", SqlDbType.NVarChar).Value = Convert.ToString(drContract("TAXKIND"))

                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                End With
                sqlCmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try
    End Sub
End Structure
''' <summary>
''' ブレーカー申請時処理
''' </summary>
Public Structure GBA00016BreakerApplyProc
    ''' <summary>
    ''' [IN]ブレーカーID
    ''' </summary>
    ''' <returns></returns>
    Public Property brId As String
    ''' <summary>
    ''' [IN]申請No
    ''' </summary>
    ''' <returns></returns>
    Public Property ApplyId As String
    ''' <summary>
    ''' [IN]最終承認ステップ
    ''' </summary>
    ''' <returns></returns>
    Public Property LastStep As String
    ''' <summary>
    ''' [IN]処理日(未設定ならシステム日付)
    ''' </summary>
    ''' <returns></returns>
    Public Property ProcDateTime As Date
    ''' <summary>
    ''' [IN]総額変更(要求)
    ''' </summary>
    ''' <returns></returns>
    Public Property AmtRequest As String

    ''' <summary>
    ''' ブレーカー申請時に合わせてブレーカー情報を更新する処理
    ''' </summary>
    Public Sub GBA00016BreakerDataApplyUpdate()
        Dim sqlStat As New Text.StringBuilder

        '*ブレーカー関連付けテーブル申請情報登録
        sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
        sqlStat.AppendLine("   SET APPLYID   = @APPLYID")
        sqlStat.AppendLine("      ,LASTSTEP  = @LASTSTEP")
        sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
        sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
        sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE BRID      = @BRID")
        sqlStat.AppendLine("   AND TYPE      = @TYPE")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
        sqlStat.AppendLine(";")
        '*ブレーカー基本テーブル AMTREQUEST,AMTDISCOUNT更新  
        sqlStat.AppendLine("UPDATE GBT0002_BR_BASE")
        sqlStat.AppendLine("   SET AMTPRINCIPAL = @AMTREQUEST ")
        sqlStat.AppendLine("      ,AMTDISCOUNT  = @AMTDISCOUNT ")
        sqlStat.AppendLine("      ,UPDYMD       = @UPDYMD ")
        sqlStat.AppendLine("      ,UPDUSER      = @UPDUSER ")
        sqlStat.AppendLine("      ,RECEIVEYMD   = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE BRID         = @BRID")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine(";")
        '*ブレーカー明細テーブル BILLINGフィールド更新(ブレーカー費用項目のBILLINGフィールドを開放した場合このSQLで塗り替えられるので要変更)
        sqlStat.AppendLine("UPDATE GBT0003_BR_VALUE")
        sqlStat.AppendLine("   SET BILLING      = (CASE WHEN BS.BILLINGCATEGORY = '" & GBC_DELIVERYCLASS.SHIPPER & "' THEN '1' ELSE '0' END) ")
        sqlStat.AppendLine("      ,UPDYMD       = @UPDYMD ")
        sqlStat.AppendLine("      ,UPDUSER      = @UPDUSER ")
        sqlStat.AppendLine("      ,RECEIVEYMD   = @RECEIVEYMD ")
        sqlStat.AppendLine("  FROM GBT0003_BR_VALUE VL")
        sqlStat.AppendLine("  INNER JOIN GBT0002_BR_BASE BS")
        sqlStat.AppendLine("     ON BS.BRID       = VL.BRID")
        sqlStat.AppendLine("    AND BS.DELFLG    <> @DELFLG")
        sqlStat.AppendLine(" WHERE VL.BRID         = @BRID")
        sqlStat.AppendLine("   AND VL.DELFLG      <> @DELFLG")
        sqlStat.AppendLine(";")

        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            Dim amtPrinStr As String = GetDiscountValue(sqlCon)

            With sqlCmd.Parameters
                .Add("@BRID", SqlDbType.NVarChar, 20).Value = Me.brId
                '関連付け情報更新用パラメータ
                .Add("@TYPE", SqlDbType.NVarChar, 20).Value = "INFO"
                .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = Me.ApplyId
                .Add("@LASTSTEP", SqlDbType.NVarChar, 20).Value = Me.LastStep
                '基本情報更新用パラメータ
                .Add("@AMTREQUEST", SqlDbType.Float).Value = Me.AmtRequest
                .Add("@AMTDISCOUNT", SqlDbType.Float).Value = amtPrinStr

                '共通パラメータ
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@UPDYMD", SqlDbType.DateTime).Value = ProcDateTime
                .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            End With
            'TRANSACTIONを開始しSQLを実行
            Using trn As SqlTransaction = sqlCon.BeginTransaction
                sqlCmd.Transaction = trn
                sqlCmd.ExecuteNonQuery()
                trn.Commit()
            End Using

        End Using
    End Sub
    ''' <summary>
    ''' TOTALコストを取得
    ''' </summary>
    ''' <returns>費用項目のUSD</returns>
    Private Function GetDiscountValue(sqlCon As SqlConnection) As String

        Dim dtValue As New DataTable
        Dim dtBase As New DataTable
        Dim sqlStatCost As New Text.StringBuilder
        sqlStatCost.AppendLine("SELECT VL.USD")
        sqlStatCost.AppendLine("  FROM GBT0003_BR_VALUE VL")
        sqlStatCost.AppendLine(" WHERE VL.BRID         = @BRID")
        sqlStatCost.AppendLine("   AND VL.DELFLG      <> @DELFLG")
        Dim sqlStatBase As New Text.StringBuilder
        sqlStatBase.AppendLine("SELECT BS.JOTHIREAGE")
        sqlStatBase.AppendLine("      ,BS.COMMERCIALFACTOR")
        sqlStatBase.AppendLine("      ,BS.FEE")
        sqlStatBase.AppendLine("      ,BS.AMTPRINCIPAL")
        sqlStatBase.AppendLine("  FROM GBT0002_BR_BASE BS")
        sqlStatBase.AppendLine(" WHERE BS.BRID         = @BRID")
        sqlStatBase.AppendLine("   AND BS.DELFLG      <> @DELFLG")

        Using sqlCmd As New SqlCommand()
            sqlCmd.Connection = sqlCon
            With sqlCmd.Parameters
                .Add("@BRID", SqlDbType.NVarChar, 20).Value = Me.brId
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            '費用項目の抽出
            sqlCmd.CommandText = sqlStatCost.ToString()

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtValue)
            End Using
            '基本情報の抽出
            sqlCmd.CommandText = sqlStatBase.ToString()

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtBase)
            End Using
        End Using
        '全費目合計を算出
        Dim qtotalCost = From item In dtValue Select Decimal.Parse(Convert.ToString(item("USD")))
        Dim totalCost As Decimal = 0
        If qtotalCost.Any Then
            totalCost = qtotalCost.Sum
        End If
        Dim totalInvoice As Decimal = totalCost
        If dtBase Is Nothing OrElse dtBase.Rows.Count = 0 Then
            'データが取れない場合
            Return "0"
        End If

        Dim drBase As DataRow = dtBase.Rows(0)
        'BASE情報のFEE(COMMISSION:手数料)を加算
        If IsNumeric(drBase.Item("FEE")) Then
            totalInvoice = totalInvoice + Decimal.Parse(Convert.ToString(drBase.Item("FEE")))
        End If
        'Hireage,Adjustmentを加算しTTLINVOICEを算出
        If IsNumeric(drBase.Item("JOTHIREAGE")) Then
            totalInvoice = totalInvoice + Decimal.Parse(Convert.ToString(drBase.Item("JOTHIREAGE")))
        End If
        If IsNumeric(drBase.Item("COMMERCIALFACTOR")) Then
            totalInvoice = totalInvoice + Decimal.Parse(Convert.ToString(drBase.Item("COMMERCIALFACTOR")))
        End If
        '値引算出
        Dim amtRequest As Decimal = If(IsNumeric(Me.AmtRequest), Decimal.Parse(Me.AmtRequest), 0)
        Dim amtDiscount As Decimal = 0
        If Not (amtRequest = 0) Then
            amtDiscount = amtRequest - totalInvoice
        End If
        Return Convert.ToString(amtDiscount)
    End Function
End Structure


