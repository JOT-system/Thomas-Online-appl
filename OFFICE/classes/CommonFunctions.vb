Imports BASEDLL
''' <summary>
''' システム全体で利用する関数群
''' </summary>
Public Class CommonFunctions
#Region "DataRow比較関連"
    ''' <summary>
    ''' 引数に指定した両DataRowアイテムの値を指定したフィールドリストにて比較
    ''' (True:全件一致,False:不一致あり)
    ''' </summary>
    ''' <param name="leftRowItem">左側DataRow</param>
    ''' <param name="rightRowItem">右側DataRow</param>
    ''' <param name="CompareFieldList">比較対象フィールド情報</param>
    ''' <returns>True:全件一致,False:不一致あり</returns>
    Public Shared Function CompareDataFields(leftRowItem As DataRow, rightRowItem As DataRow, compareFieldList As List(Of CompareFieldInfo)) As Boolean
        If leftRowItem Is Nothing OrElse rightRowItem Is Nothing OrElse compareFieldList Is Nothing _
            OrElse compareFieldList.Count = 0 Then
            Throw New Exception("CommonFunctions.CompareDataFields ParameterError")
        End If
        '比較するフィールドをループ
        For Each compareField In compareFieldList
            Dim leftVal As String = Convert.ToString(leftRowItem.Item(compareField.FieldName))
            Dim rightVal As String = Convert.ToString(rightRowItem.Item(compareField.FieldName))
            '左DataRowRTrimオプション
            If compareField.RtrimLeftRowItem = True Then
                leftVal = RTrim(leftVal)
            End If
            '右DataRowRTrimオプション
            If compareField.RtrimRightRowItem = True Then
                rightVal = RTrim(rightVal)
            End If
            '大文字小文字無視オプション
            If compareField.IgnoreUpperLower = False Then
                leftVal = leftVal.ToUpper
                rightVal = rightVal.ToUpper
            End If
            If leftVal <> rightVal Then
                Return False
            End If
        Next compareField
        Return True 'ここまで来た場合は全フィールド一致の為True
    End Function
    ''' <summary>
    ''' 引数に指定した両DataRowアイテムの値を指定したフィールド配列にて比較
    ''' ※トリムなし、大文字小文字区別あり
    ''' (True:全件一致,False:不一致あり)
    ''' </summary>
    ''' <param name="leftRowItem"></param>
    ''' <param name="rightRowItem"></param>
    ''' <param name="compareFieldList">フィールド名配列</param>
    ''' <returns></returns>
    ''' <remarks>オーバーロードList(Of CompareFieldInfo))のオーバーロード</remarks>
    Public Shared Function CompareDataFields(leftRowItem As DataRow, rightRowItem As DataRow, ParamArray compareFieldList() As String) As Boolean
        Dim fieldList As List(Of CompareFieldInfo) = CreateCompareFieldList(compareFieldList)
        Return CompareDataFields(leftRowItem, rightRowItem, fieldList)
    End Function
    ''' <summary>
    ''' 比較フィールドリストを生成する
    ''' </summary>
    ''' <param name="fieldNames">フィールド名配列{"フィールド名1","フィールド名2","フィールド名3"}等で指定</param>
    ''' <returns>比較方法はデフォルトのまま作成</returns>
    Public Shared Function CreateCompareFieldList(ParamArray fieldNames() As String) As List(Of CompareFieldInfo)
        If fieldNames Is Nothing OrElse fieldNames.Count = 0 Then
            Return Nothing
        End If
        Dim retVal As New List(Of CompareFieldInfo)
        For Each fieldName In fieldNames
            Dim item As New CompareFieldInfo(fieldName)
            retVal.Add(item)
        Next fieldName
        Return retVal
    End Function

    ''' <summary>
    ''' 比較方式格納クラス
    ''' </summary>
    ''' <remarks>CompareDataFieldsメソッドにて利用</remarks>
    Public Class CompareFieldInfo
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        Public Sub New(fieldName As String)
            Me.FieldName = fieldName
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName"></param>
        ''' <param name="rtrimLeftRowItem"></param>
        ''' <param name="rtrimRightRowItem"></param>
        Public Sub New(fieldName As String, rtrimLeftRowItem As Boolean, rtrimRightRowItem As Boolean)
            Me.FieldName = fieldName
            Me.RtrimLeftRowItem = rtrimLeftRowItem
            Me.RtrimRightRowItem = rtrimRightRowItem
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName"></param>
        ''' <param name="rtrimLeftRowItem"></param>
        ''' <param name="rtrimRightRowItem"></param>
        Public Sub New(fieldName As String, rtrimLeftRowItem As Boolean, rtrimRightRowItem As Boolean, ignoreUpperLower As Boolean)
            Me.FieldName = fieldName
            Me.RtrimLeftRowItem = rtrimLeftRowItem
            Me.RtrimRightRowItem = rtrimRightRowItem
            Me.IgnoreUpperLower = ignoreUpperLower
        End Sub
        ''' <summary>
        ''' 比較対象フィールド名
        ''' </summary>
        ''' <returns></returns>
        Public Property FieldName As String
        ''' <summary>
        ''' 左側DataRowのフィールドにRtrimをかけるか(True:かける,False:かけない(デフォルト))
        ''' </summary>
        ''' <returns></returns>
        Public Property RtrimLeftRowItem As Boolean = False
        ''' <summary>
        ''' 右側DatarowのフィールドにRtrimをかけるか(True:かける,False:かけない(デフォルト))
        ''' </summary>
        ''' <returns></returns>
        Public Property RtrimRightRowItem As Boolean = False
        ''' <summary>
        ''' 大文字小文字無視(True:無視,False:区別する(デフォルト))
        ''' </summary>
        ''' <returns></returns>
        Public Property IgnoreUpperLower As Boolean = True

    End Class
#End Region
#Region "メッセージ表示関連"
    ''' <summary>
    ''' メッセージNoを元にフッターラベル＋オプションでポップアップを表示する
    ''' </summary>
    ''' <param name="messageNo">[IN]メッセージNo</param>
    ''' <param name="lblObject">[IN/OUT]対象ラベルオブジェクト</param>
    ''' <param name="naeiw">[IN]省略可 エラーレベル：C_NAEIW.xxxxxを指定(未指定時は'A')</param>
    ''' <param name="pageObject">[IN/OUT]省略可 対象ページオブジェクト、指定した場合はメッセージ表示</param>
    ''' <param name="messageBoxTitle">[IN]省略可 メッセージボックスのタイトルバー文言(省略時は"Message")</param>
    ''' <param name="messagePrefix">[IN]省略可 取得したメッセージの頭につける文言</param>
    ''' <param name="messageSuffix">[IN]省略可 取得したメッセージの末尾につける文言</param>
    ''' <param name="messageParams">メッセージにしていした「?01」を置換するためのリスト</param>
    ''' <param name="messageBoxOnly">[IN]省略可 ラベル設定なしでメッセージボックスのみ表示True:メッセージボックスのみ、False:両方 (未指定はFalse)</param>
    ''' <param name="isThrowBaseDllError">[IN]省略可 BaseDllエラー時上位にスローするか？デフォルトはしない</param>
    Public Shared Sub ShowMessage(ByVal messageNo As String,
                                  ByRef lblObject As Label,
                                  Optional naeiw As String = "",
                                  Optional pageObject As Page = Nothing,
                                  Optional messageBoxTitle As String = "Message",
                                  Optional messagePrefix As String = "",
                                  Optional messageSuffix As String = "",
                                  Optional messageParams As List(Of String) = Nothing,
                                  Optional messageBoxOnly As Boolean = False,
                                  Optional isThrowBaseDllError As Boolean = False,
                                  <System.Runtime.CompilerServices.CallerMemberName> Optional callerMemberName As String = Nothing,
                                  <System.Runtime.CompilerServices.CallerFilePath> Optional callerFilePath As String = Nothing,
                                  <System.Runtime.CompilerServices.CallerLineNumber> Optional callerLineNumber As Integer = 0)
        '一旦初期化
        lblObject.Text = ""

        If naeiw = "" Then
            naeiw = C_NAEIW.ABNORMAL
        End If
        '置換文言パラメータの設定
        Dim messageParamFull As New List(Of String)
        If messageParams IsNot Nothing AndAlso messageParams.Count > 0 Then
            For i As Integer = 0 To messageParams.Count - 1
                messageParamFull.Add(messageParams(i))
                If i = 9 Then
                    Exit For
                End If
            Next
        End If

        For i = messageParamFull.Count To 9
            messageParamFull.Add("")
        Next

        'メッセージの取得
        Dim tmpLabel As New Label
        Dim COA0004LableMessage As New COA0004LableMessage With
            {.MESSAGENO = messageNo,
                .MESSAGEBOX = tmpLabel,
                .NAEIW = naeiw,
                .PARA01 = messageParamFull(0), .PARA02 = messageParamFull(1),
                .PARA03 = messageParamFull(2), .PARA04 = messageParamFull(3),
                .PARA05 = messageParamFull(4), .PARA06 = messageParamFull(5),
                .PARA07 = messageParamFull(6), .PARA08 = messageParamFull(7),
                .PARA09 = messageParamFull(8), .PARA10 = messageParamFull(9)
            }
        'メッセージ取得時のエラーはスローし呼出し元に任せる
        COA0004LableMessage.COA0004getMessage()
        If COA0004LableMessage.ERR <> C_MESSAGENO.NORMAL Then
            If isThrowBaseDllError Then
                Throw New Exception(String.Format("COA0004LableMessage.GetMessageError:Member={0},LineNo={1}", callerMemberName, callerLineNumber))
            Else
                Return '上位にスローしない場合は無反応で終了
            End If
        End If
        tmpLabel = COA0004LableMessage.MESSAGEBOX
        Dim retMsg As String = messagePrefix & tmpLabel.Text & messageSuffix

        If messageBoxOnly = False Then
            lblObject.Font.ClearDefaults() '余計な個別文字設定をクリア
            lblObject.ForeColor = Drawing.Color.FromName("0") '余計な文字色をクリア
            lblObject.Style.Clear()        '余計なスタイル設定をクリア
            lblObject.CssClass = tmpLabel.CssClass
            lblObject.Text = retMsg
        End If
        'メッセーボックス生成
        If pageObject IsNot Nothing Then


            If pageObject.FindControl("pnlCommonMessageWrapper") IsNot Nothing Then
                Dim removeObj = pageObject.FindControl("pnlCommonMessageWrapper")
                pageObject.Controls.Remove(removeObj)
            End If
            Dim pnlWrapper As New Panel With {.ID = "pnlCommonMessageWrapper", .ViewStateMode = ViewStateMode.Disabled}
            Dim pnlMessageBox As New Panel With {.ID = "pnlCommonMessageContents", .ViewStateMode = ViewStateMode.Disabled}
            Dim pnlMessageBoxTitle As New Panel With {.ID = "pnlCommonMessageTitle", .ViewStateMode = ViewStateMode.Disabled}
            Dim btnMessageBoxOkButton As New HtmlInputButton With {.ID = "btnCommonMessageOk", .ViewStateMode = ViewStateMode.Disabled,
                                                                   .Value = "OK"}
            Dim onClickScriptText As New StringBuilder

            onClickScriptText.AppendLine("commonCloseModal('pnlCommonMessageWrapper');")
            onClickScriptText.AppendLine("document.getElementById('pnlCommonMessageWrapper').style.display = 'none';")
            onClickScriptText.AppendLine("focusAfterChange();")
            onClickScriptText.AppendLine("var docLastElms = document.querySelectorAll('script');")
            onClickScriptText.AppendLine("if (docLastElms !== null) {")
            onClickScriptText.AppendLine("    var lastScript = docLastElms[docLastElms.length -1];")
            onClickScriptText.AppendLine("    if (lastScript.innerHTML.indexOf('WebForm_Auto') === 0) {")
            onClickScriptText.AppendLine("        var s = document.createElement('script');")
            onClickScriptText.AppendLine("        s.innerHTML = lastScript.innerHTML;")
            onClickScriptText.AppendLine("        lastScript.innerHTML = '';")
            onClickScriptText.AppendLine("        document.body.appendChild(s);")
            onClickScriptText.AppendLine("    }")
            onClickScriptText.AppendLine("}")
            btnMessageBoxOkButton.Attributes.Add("onclick", onClickScriptText.ToString)
            Dim lblMessageBoxTitleLabel As New Label With {.ID = "lblCommonMessageTitle", .ViewStateMode = ViewStateMode.Disabled,
                                                           .Text = messageBoxTitle}
            Dim pnlMessageBoxText As New Panel With {.ID = "pnlCommonMessageText", .ViewStateMode = ViewStateMode.Disabled}
            Dim lblMessageBoxText As New Label With {.ID = "lblCommonMessageText", .ViewStateMode = ViewStateMode.Disabled,
                                                           .Text = retMsg}
            lblMessageBoxText.Attributes.Add("data-naeiw", naeiw)
            'メッセージボックスオブジェクトの組み立て
            pnlMessageBoxTitle.Controls.Add(btnMessageBoxOkButton)
            pnlMessageBoxTitle.Controls.Add(lblMessageBoxTitleLabel)
            pnlMessageBoxText.Controls.Add(lblMessageBoxText)

            pnlMessageBox.Controls.Add(pnlMessageBoxTitle)
            pnlMessageBox.Controls.Add(pnlMessageBoxText)

            pnlWrapper.Controls.Add(pnlMessageBox)

            pageObject.Form.Parent.Controls.Add(pnlWrapper)
        End If

    End Sub
    ''' <summary>
    ''' 確認メッセージ表示
    ''' </summary>
    ''' <param name="messageNo">[IN]メッセージNo</param>
    ''' <param name="pageObject">[IN/OUT]対象ページオブジェクト</param>
    ''' <param name="naeiw">[IN]省略可 エラーレベル：C_NAEIW.xxxxxを指定(未指定時は'Q'クエスチョン)</param>
    ''' <param name="messageBoxTitle">[IN]省略可 メッセージボックスのタイトルバー文言(省略時は"Message")</param>
    ''' <param name="messagePrefix">[IN]省略可 取得したメッセージの頭につける文言</param>
    ''' <param name="messageSuffix">[IN]省略可 取得したメッセージの末尾につける文言</param>
    ''' <param name="messageParams">メッセージにしていした「?01」を置換するためのリスト</param>
    ''' <param name="isThrowBaseDllError">[IN]省略可 BaseDllエラー時上位にスローするか？デフォルトはしない</param>
    ''' <param name="okButtonText">[IN]省略可 OKボタンの文言</param>
    ''' <param name="cancelButtonText">[IN]省略可 Cancelボタンの文言</param>
    ''' <param name="submitButtonId">ボタンID([ボタンID]_Clickをコールさせるため)</param>
    Public Shared Sub ShowConfirmMessage(ByVal messageNo As String,
                                         ByRef pageObject As Page,
                                         Optional naeiw As String = "",
                                         Optional messageBoxTitle As String = "",
                                         Optional messagePrefix As String = "",
                                         Optional messageSuffix As String = "",
                                         Optional messageParams As List(Of String) = Nothing,
                                         Optional isThrowBaseDllError As Boolean = False,
                                         Optional okButtonText As String = "OK",
                                         Optional cancelButtonText As String = "Cancel",
                                         Optional submitButtonId As String = "btnCommonMessageOk",
                                         <System.Runtime.CompilerServices.CallerMemberName> Optional callerMemberName As String = Nothing,
                                         <System.Runtime.CompilerServices.CallerFilePath> Optional callerFilePath As String = Nothing,
                                         <System.Runtime.CompilerServices.CallerLineNumber> Optional callerLineNumber As Integer = 0)

        If naeiw = "" Then
            naeiw = C_NAEIW.QUESTION
        End If
        '置換文言パラメータの設定
        Dim messageParamFull As New List(Of String)
        If messageParams IsNot Nothing AndAlso messageParams.Count > 0 Then
            For i As Integer = 0 To messageParams.Count - 1
                messageParamFull.Add(messageParams(i))
                If i = 9 Then
                    Exit For
                End If
            Next
        End If
        For i = messageParamFull.Count To 9
            messageParamFull.Add("")
        Next

        'メッセージの取得
        Dim tmpLabel As New Label
        Dim COA0004LableMessage As New COA0004LableMessage With
            {.MESSAGENO = messageNo,
                .MESSAGEBOX = tmpLabel,
                .NAEIW = C_NAEIW.ABNORMAL,
                .PARA01 = messageParamFull(0), .PARA02 = messageParamFull(1),
                .PARA03 = messageParamFull(2), .PARA04 = messageParamFull(3),
                .PARA05 = messageParamFull(4), .PARA06 = messageParamFull(5),
                .PARA07 = messageParamFull(6), .PARA08 = messageParamFull(7),
                .PARA09 = messageParamFull(8), .PARA10 = messageParamFull(9)
            }
        'メッセージ取得時のエラーはスローし呼出し元に任せる
        COA0004LableMessage.COA0004getMessage()
        If COA0004LableMessage.ERR <> C_MESSAGENO.NORMAL Then
            If isThrowBaseDllError Then
                Throw New Exception(String.Format("COA0004LableMessage.GetMessageError:Member={0},LineNo={1}", callerMemberName, callerLineNumber))
            Else
                Return '上位にスローしない場合は無反応で終了
            End If
        End If
        tmpLabel = COA0004LableMessage.MESSAGEBOX
        Dim retMsg As String = messagePrefix & tmpLabel.Text & messageSuffix

        'メッセーボックス生成
        If pageObject IsNot Nothing Then
            If pageObject.FindControl("pnlCommonMessageWrapper") IsNot Nothing Then
                Dim removeObj = pageObject.FindControl("pnlCommonMessageWrapper")
                pageObject.Controls.Remove(removeObj)
            End If

            Dim pnlWrapper As New Panel With {.ID = "pnlCommonMessageWrapper", .ViewStateMode = ViewStateMode.Disabled}
            Dim pnlMessageBox As New Panel With {.ID = "pnlCommonMessageContents", .ViewStateMode = ViewStateMode.Disabled}
            Dim pnlMessageBoxTitle As New Panel With {.ID = "pnlCommonMessageTitle", .ViewStateMode = ViewStateMode.Disabled}
            Dim btnMessageBoxOkButton As New HtmlInputButton With {.ID = "btnCommonMessageOk", .ViewStateMode = ViewStateMode.Disabled,
                                                                   .Value = okButtonText}
            btnMessageBoxOkButton.Attributes.Add("onclick", "commonCloseModal('pnlCommonMessageWrapper'); buttonClick('" & submitButtonId & "');")

            Dim btnMessageBoxCancelButton As New HtmlInputButton With {.ID = "btnCommonMessageCancel", .ViewStateMode = ViewStateMode.Disabled,
                                                                       .Value = cancelButtonText}

            btnMessageBoxCancelButton.Attributes.Add("onclick", "commonCloseModal('pnlCommonMessageWrapper'); document.getElementById('pnlCommonMessageWrapper').style.display = 'none';")
            Dim lblMessageBoxTitleLabel As New Label With {.ID = "lblCommonMessageTitle", .ViewStateMode = ViewStateMode.Disabled,
                                                           .Text = messageBoxTitle}
            Dim pnlMessageBoxText As New Panel With {.ID = "pnlCommonMessageText", .ViewStateMode = ViewStateMode.Disabled}
            Dim lblMessageBoxText As New Label With {.ID = "lblCommonMessageText", .ViewStateMode = ViewStateMode.Disabled,
                                                           .Text = retMsg}
            lblMessageBoxText.Attributes.Add("data-naeiw", naeiw)

            'メッセージボックスオブジェクトの組み立て
            pnlMessageBoxTitle.Controls.Add(btnMessageBoxOkButton)
            pnlMessageBoxTitle.Controls.Add(btnMessageBoxCancelButton)

            pnlMessageBoxTitle.Controls.Add(lblMessageBoxTitleLabel)
            pnlMessageBoxText.Controls.Add(lblMessageBoxText)

            pnlMessageBox.Controls.Add(pnlMessageBoxTitle)
            pnlMessageBox.Controls.Add(pnlMessageBoxText)

            pnlWrapper.Controls.Add(pnlMessageBox)

            pageObject.Form.Parent.Controls.Add(pnlWrapper)
        End If
    End Sub
#End Region
#Region "添付ファイル処理関連"
    ''' <summary>
    ''' ユーザー作業フォルダの掃除
    ''' </summary>
    ''' <param name="baseTempDir"></param>
    Public Shared Sub CleanUserTempDirectory(baseTempDir As String, Optional targetId As String = "")
        Dim attachmentTempDir As String = IO.Path.Combine(COA0019Session.USERTEMPDir, COA0019Session.USERID,
                                                  baseTempDir, targetId)
        '指定フォルダが存在しない場合はルートまでの処理の意味がないので終了
        If IO.Directory.Exists(attachmentTempDir) = False Then
            Return
        End If

        Dim diTarger As New IO.DirectoryInfo(attachmentTempDir)
        Dim qFiles = diTarger.EnumerateFiles("*", IO.SearchOption.AllDirectories)
        For Each fi In qFiles
            Try
                fi.Delete()
            Catch ex As Exception
            End Try
        Next
        Dim qdirs = diTarger.EnumerateDirectories()
        For Each di In qdirs
            Try
                di.Delete(True)
            Catch ex As Exception
            End Try
        Next
    End Sub

    ''' <summary>
    ''' 正式フォルダよりファイル一覧を生成
    ''' </summary>
    ''' <param name="targetId">ブレーカーIDや契約書No、同意書No等</param>
    ''' <param name="baseTempDir">画面ID等指定、ユーザー作業パス＋当パスにアップロード(特にDBとはつなげてないが作業フォルダ名になるので注意</param>
    ''' <param name="baseDir">添付格納先(COA0019Session.UPLOADFILESDir or COA0019Session.BEFOREAPPROVALDir)
    ''' +baseDir+targetIdのフォルダを検索します。</param>
    ''' <param name="isBeforeApplove"></param>
    ''' <returns>ファイル一覧のデータテーブル(FILENAME=ファイル名,
    ''' BASEFILEPATH=コピー元パス,TMPFILEPATH=作業パス（コピー先）,
    ''' FILEEXTENTION=ファイル拡張子,TIMESTMP=ファイル最終更新日,
    ''' DELFLG=削除フラグ(初期値はN))
    ''' </returns>
    ''' <remarks>実際のコピー処理は別で行うこと</remarks>
    Public Shared Function GetInitAttachmentFileList(targetId As String, baseDir As String, baseTempDir As String, Optional isBeforeApplove As Boolean = False, Optional suffix As String = "") As DataTable
        Dim dtFileList As New DataTable(C_DTNAME_ATTACHMENT)
        Dim fileList As New List(Of String) From {"FILENAME", "BASEFILEPATH", "TMPFILEPATH", "FILEEXTENTION", "DELFLG", "TIMESTMP", "ISMODIFIED", "FILESIZE"}
        For Each colName As String In fileList
            dtFileList.Columns.Add(colName, GetType(String))
            dtFileList.Columns(colName).DefaultValue = ""
        Next

        If targetId = "" Then
            Return dtFileList 'IDが無い場合ファイルリストのガワだけを生成
        End If

        Dim retDt As New DataTable
        Dim attachmentTempDir As String = IO.Path.Combine(COA0019Session.USERTEMPDir, COA0019Session.USERID,
                                                          baseTempDir, targetId, suffix)
        If Not IO.Directory.Exists(attachmentTempDir) Then
            IO.Directory.CreateDirectory(attachmentTempDir)
        End If
        'コピー元のディレクトリ取得
        Dim upBaseDir As String = COA0019Session.UPLOADFILESDir
        If isBeforeApplove Then
            upBaseDir = COA0019Session.BEFOREAPPROVALDir
        End If
        Dim attachmentDir As String = IO.Path.Combine(upBaseDir, baseDir, targetId, suffix)

        If IO.Directory.Exists(attachmentDir) = False OrElse
           IO.Directory.GetFiles(attachmentDir).Count = 0 Then
            Return dtFileList
        End If
        'コピー元のディレクトリよりファイル一覧を取得
        Dim currentFiles = IO.Directory.GetFiles(attachmentDir)
        For Each filePath As String In currentFiles
            Dim dr As DataRow = dtFileList.NewRow
            Dim fi As IO.FileInfo = New IO.FileInfo(filePath)
            Dim tmpFilePath As String = IO.Path.Combine(attachmentTempDir, IO.Path.GetFileName(filePath))

            dr("FILENAME") = IO.Path.GetFileName(filePath)
            dr("BASEFILEPATH") = filePath
            dr("TMPFILEPATH") = tmpFilePath
            dr("FILEEXTENTION") = IO.Path.GetExtension(filePath)
            dr("TIMESTMP") = fi.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss.FFF")
            dr("DELFLG") = CONST_FLAG_NO
            dr("ISMODIFIED") = CONST_FLAG_NO
            dr("FILESIZE") = fi.Length
            dtFileList.Rows.Add(dr)
            Try
                IO.File.Copy(filePath, tmpFilePath)
            Catch ex As Exception

            End Try
        Next
        Return dtFileList
    End Function
    ''' <summary>
    ''' アップロードファイル
    ''' </summary>
    ''' <param name="dtAttachment"></param>
    ''' <param name="targetId"></param>
    ''' <param name="baseTempDir"></param>
    ''' <returns></returns>
    Public Shared Function UploadAttachmentFile(dtAttachment As DataTable, ByVal targetId As String, baseTempDir As String, Optional suffix As String = "") As DataTable
        Dim retDt As DataTable = CommonFunctions.DeepCopy(dtAttachment)
        Dim uploadDir As String = IO.Path.Combine(COA0019Session.UPLOADDir, COA0019Session.USERID)
        If targetId = "" Then
            targetId = "##(new)##"
        End If

        Dim attachmentTempDir As String = IO.Path.Combine(COA0019Session.USERTEMPDir, COA0019Session.USERID,
                                                  baseTempDir, targetId, suffix)
        If Not IO.Directory.Exists(attachmentTempDir) Then
            IO.Directory.CreateDirectory(attachmentTempDir)
        End If
        'アップロード直後にも関わらずフォルダが無いまたはファイルが1つも存在しない場合
        If IO.Directory.Exists(uploadDir) = False OrElse
           IO.Directory.GetFiles(uploadDir).Count = 0 Then
            Return retDt
        End If
        'アップロードファイルのループ
        Dim uploadFiles = IO.Directory.GetFiles(uploadDir)
        For Each filePath As String In uploadFiles
            Dim fileName As String = IO.Path.GetFileName(filePath)
            Dim copyFilePath As String = IO.Path.Combine(attachmentTempDir, fileName)
            '同名ファイルが存在する場合は削除フラグを初期化し変更フラグを立てる
            Dim qAttachment = From attachmentItem In retDt Where attachmentItem("FILENAME").Equals(fileName)
            If qAttachment.Any Then
                qAttachment.FirstOrDefault.Item("DELFLG") = CONST_FLAG_NO
                qAttachment.FirstOrDefault.Item("ISMODIFIED") = CONST_FLAG_YES
            Else
                Dim dr As DataRow = retDt.NewRow
                Dim fi As IO.FileInfo = New IO.FileInfo(filePath)

                dr("FILENAME") = IO.Path.GetFileName(filePath)
                dr("BASEFILEPATH") = ""
                dr("TMPFILEPATH") = copyFilePath
                dr("FILEEXTENTION") = IO.Path.GetExtension(filePath)
                dr("TIMESTMP") = fi.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                dr("DELFLG") = CONST_FLAG_NO
                dr("ISMODIFIED") = CONST_FLAG_YES
                dr("FILESIZE") = fi.Length
                retDt.Rows.Add(dr)
            End If

            Try
                IO.File.Copy(filePath, copyFilePath)
                IO.File.Delete(filePath)
            Catch ex As Exception
            End Try

        Next
        Return retDt
    End Function
    ''' <summary>
    ''' 指定したファイルを元にダウンロードフォルダにコピーしURLを生成
    ''' </summary>
    ''' <param name="dtAttachment"></param>
    ''' <param name="targetFileName"></param>
    ''' <returns></returns>
    Public Shared Function GetAttachfileDownloadUrl(dtAttachment As DataTable, targetFileName As String) As String
        Dim drTargetAttachments = From dr In dtAttachment Where dr("FILENAME").Equals(targetFileName)
        '対象ファイル名が設定されていない場合
        If drTargetAttachments.Any = False Then
            Return ""
        End If
        Dim drTargetAttachment As DataRow = drTargetAttachments(0)
        'ユーザー作業フォルダに存在していない場合
        If Not IO.File.Exists(Convert.ToString(drTargetAttachment("TMPFILEPATH"))) Then
            Return ""
        End If
        'URL生成
        Dim printWorkDir As String = IO.Path.Combine(COA0019Session.PRINTWORKDir, COA0019Session.USERID)
        'PRINTWORKのユーザフォルダが存在しない場合は生成
        If Not IO.Directory.Exists(printWorkDir) Then
            IO.Directory.CreateDirectory(printWorkDir)
        End If
        'ユーザー作業フォルダからプリントフォルダにファイルコピー
        IO.File.Copy(Convert.ToString(drTargetAttachment("TMPFILEPATH")),
                     IO.Path.Combine(printWorkDir, targetFileName),
                     True)

        Dim urlString As String = String.Format("{0}://{1}/{4}/{2}/{3}",
                                                HttpContext.Current.Request.Url.Scheme,
                                                HttpContext.Current.Request.Url.Host,
                                                COA0019Session.USERID,
                                                Uri.EscapeUriString(targetFileName),
                                                COA0019Session.PRINTROOTUrl)
        Return urlString
    End Function
    ''' <summary>
    ''' 添付ファイル一覧より圧縮
    ''' </summary>
    ''' <param name="dtAttachment">添付ファイル一覧データテーブル</param>
    ''' <param name="zipFileNameNoExtention">拡張子なしの圧縮ファイル名</param>
    ''' <returns></returns>
    Public Shared Function GetAttachmentCompressedFileUrl(dtAttachment As DataTable, ByVal zipFileNameNoExtention As String) As String
        '添付ファイルなし
        If dtAttachment Is Nothing OrElse dtAttachment.Rows.Count = 0 Then
            Return ""
        End If
        '先頭データRowの実態ファイルパスの親パスを取る
        Dim drTargetAttachment As DataRow = dtAttachment.Rows(0)
        Dim userWorkRootDir As String = IO.Directory.GetParent(Convert.ToString(drTargetAttachment.Item("TMPFILEPATH"))).FullName
        If IO.Directory.Exists(userWorkRootDir) = False Then
            'ここまで来てありえないがルートパスが無い場合はブランク
            Return ""
        End If

        If zipFileNameNoExtention = "" Then
            zipFileNameNoExtention = "compressedFile"
        End If
        Dim zipFileName As String = zipFileNameNoExtention & ".zip"
        'URL生成
        Dim printWorkDir As String = IO.Path.Combine(COA0019Session.PRINTWORKDir, COA0019Session.USERID, "ZIP")
        'PRINTWORKのユーザフォルダが存在しない場合は生成
        If Not IO.Directory.Exists(printWorkDir) Then
            IO.Directory.CreateDirectory(printWorkDir)
        Else
            IO.Directory.Delete(printWorkDir, True)
            IO.Directory.CreateDirectory(printWorkDir)
        End If
        '圧縮処理実施
        Dim zipFilePath As String = IO.Path.Combine(printWorkDir, zipFileName)
        IO.Compression.ZipFile.CreateFromDirectory(userWorkRootDir, zipFilePath,
                                                   System.IO.Compression.CompressionLevel.Optimal, False,
                                                   Text.Encoding.GetEncoding("shift_jis"))
        Dim urlString As String = String.Format("{0}://{1}/{4}/{2}/ZIP/{3}",
                                                HttpContext.Current.Request.Url.Scheme,
                                                HttpContext.Current.Request.Url.Host,
                                                COA0019Session.USERID,
                                                Uri.EscapeUriString(zipFileName),
                                                COA0019Session.PRINTROOTUrl)
        Return urlString
    End Function
    ''' <summary>
    ''' 添付ファイルをユーザーフォルダから正式フォルダにコピー
    ''' </summary>
    ''' <param name="dtAttachment"></param>
    ''' <param name="targetId"></param>
    ''' <param name="baseDir"></param>
    ''' <param name="isBeforeApplove"></param>
    ''' <returns></returns>
    Public Shared Function SaveAttachmentFilesList(dtAttachment As DataTable, targetId As String, baseDir As String, Optional isBeforeApplove As Boolean = False, Optional suffix As String = "", Optional moveApplyToApproval As Boolean = False) As String
        '保存先の正式のディレクトリ取得
        Dim upBaseDir As String = COA0019Session.UPLOADFILESDir
        Dim appFlg As String = "2"
        If isBeforeApplove Then
            upBaseDir = COA0019Session.BEFOREAPPROVALDir
        End If
        '保存先ディレクトリ存在チェック
        Dim extentDir As String = baseDir & "\" & targetId & If(suffix <> "", "\" & suffix, "")
        Dim attachmentDir As String = IO.Path.Combine(upBaseDir, extentDir)
        extentDir = "\" & extentDir

        If IO.Directory.Exists(attachmentDir) = False Then
            IO.Directory.CreateDirectory(attachmentDir)
        End If
        'ファイル一覧のループ
        For Each dr As DataRow In dtAttachment.Rows
            'コピー元のファイルパス
            Dim filePath As String = IO.Path.Combine(attachmentDir, Convert.ToString(dr.Item("TMPFILEPATH")))
            '正式フォルダへのファイルするパス
            Dim targetFilePath As String = IO.Path.Combine(attachmentDir, Convert.ToString(dr.Item("FILENAME")))
            '新規・変更・削除時の処理その他は何もしない
            If dr.Item("DELFLG").Equals(CONST_FLAG_YES) Then
                '削除フラグは正規フォルダより削除
                If IO.File.Exists(targetFilePath) Then
                    Try
                        IO.File.Delete(targetFilePath)
                    Catch ex As Exception
                    End Try
                End If
            ElseIf dr.Item("ISMODIFIED").Equals(CONST_FLAG_YES) OrElse moveApplyToApproval Then
                '新規or変更時は（上書き）コピー
                Try
                    IO.File.Copy(filePath, targetFilePath, True)
                Catch ex As Exception
                End Try
            End If
        Next dr
        '集配信用フォルダ格納処理
        Dim COA00034SendDirectory As New COA00034SendDirectory
        COA00034SendDirectory.SendDirectoryCopy(extentDir, attachmentDir, appFlg)
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 添付ファイルの拡張子、ファイル数チェック
    ''' </summary>
    ''' <param name="dtAttachment">画面に保持している添付ファイルリスト</param>
    ''' <returns></returns>
    Public Shared Function CheckUploadAttachmentFile(dtAttachment As DataTable) As String
        Dim uploadDir As String = IO.Path.Combine(COA0019Session.UPLOADDir, COA0019Session.USERID)
        Dim COA0017FixValue As New COA0017FixValue
        Dim dummyUploadList As ListBox
        'リスト設定(Uploadの設定値,許可拡張子(EXTENSION),最大ファイル数(QUANTITY)取得)
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "UPLOAD"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            dummyUploadList = COA0017FixValue.VALUE1
        Else
            Return COA0017FixValue.ERR
        End If
        '最大数を保持
        Dim maxFileCount As Integer = 0
        If dummyUploadList.Items.FindByValue("QUANTITY") IsNot Nothing Then
            maxFileCount = CInt(dummyUploadList.Items.FindByValue("QUANTITY").Text)
        End If
        '許可拡張子を保持
        Dim allowExtentionList As New List(Of String)
        If dummyUploadList.Items.FindByValue("EXTENSION") IsNot Nothing Then
            Dim allowExtentionStr = ""
            allowExtentionList = dummyUploadList.Items.FindByValue("EXTENSION").Text.ToUpper.Split(","c).ToList
        End If
        '今回転送されたファイル名を格納(ファイル名、拡張子)
        Dim dicUploadFile As New Dictionary(Of String, String)
        Dim qUploadFiles = IO.Directory.EnumerateFiles(uploadDir, "*.*")
        If qUploadFiles.Any = False Then
            'アップロードフォルダにファイル存在なし
            Return C_MESSAGENO.FILENOTEXISTS
        End If
        dicUploadFile = qUploadFiles.ToDictionary(Function(d) IO.Path.GetFileName(d), Function(d) IO.Path.GetExtension(d).ToUpper)
        '******************************
        'ファイル数チェック
        '******************************
        Dim fileNameList As New List(Of String)
        '今回アップロードしかカウントを総数に格納
        Dim totalFileCount As Integer = dicUploadFile.Count
        '既存アップロードのカウントを今回アップロードファイル名を除き加算
        If dtAttachment IsNot Nothing OrElse dtAttachment.Rows.Count > 0 Then
            Dim dtAttachmentCnt As Integer = (From drAttachment As DataRow In dtAttachment
                                              Where dicUploadFile.ContainsKey(Convert.ToString(drAttachment("FILENAME"))) = False).Count
            totalFileCount = totalFileCount + dtAttachmentCnt
        End If
        If totalFileCount > maxFileCount Then
            Return C_MESSAGENO.TOOMANYUPLOADFILES
        End If
        '******************************
        '拡張子チェック
        '******************************
        Dim qNowAllowExtentions = From uploadFile In dicUploadFile
                                  Where Not allowExtentionList.Contains(uploadFile.Value)
        If qNowAllowExtentions.Any = True Then
            '許可しない拡張子のファイルが存在した場合
            Return C_MESSAGENO.INCORRECTFILETYPE
        End If
        'ここまで到達したら正常
        Return C_MESSAGENO.NORMAL
    End Function

#End Region
#Region "その他処理"
    ''' <summary>
    ''' オブジェクトのDeepCopy別インスタンスでオブジェクトのコピーを行う
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="src"></param>
    ''' <returns></returns>
    Public Shared Function DeepCopy(Of T)(src As T) As T
        Using memoryStream = New System.IO.MemoryStream()
            Dim binaryFormatter _
        = New System.Runtime.Serialization _
              .Formatters.Binary.BinaryFormatter()
            binaryFormatter.Serialize(memoryStream, src) ' シリアライズ
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin)
            Return DirectCast(binaryFormatter.Deserialize(memoryStream), T) ' デシリアライズ
        End Using
    End Function
#End Region

    ''' <summary>
    ''' 切り上げ
    ''' </summary>
    ''' <param name="value">対象の数値</param>
    ''' <param name="decimalPlaces">有効小数桁数</param>
    ''' <returns>切り上げした数値</returns>
    Public Shared Function RoundUp(ByVal value As Decimal, ByVal decimalPlaces As UInt32) As Decimal
        Dim rate As Decimal = CDec(Math.Pow(10.0R, decimalPlaces))

        If value < 0 Then
            Return (Math.Ceiling(value * -1D * rate) / rate) * -1D
        Else
            Return Math.Ceiling(value * rate) / rate
        End If
    End Function
End Class

Public Class aa
    Public Shared Sub Ra()

    End Sub


End Class