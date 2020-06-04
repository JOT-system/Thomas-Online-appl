''' <summary>
''' 共通定数定義モジュール
''' CONST名、およびCLASS名についてはプリフィックスC_を付与してください。
''' </summary>
Public Module CommonConst
    'ほぼ単一でしか使用しない場合
    'Public const AAAA As String = "定数！！！！"
    '********************************************
    'URL関連
    '********************************************
    ''' <summary>
    ''' ログインURL
    ''' </summary>
    Public Const C_LOGIN_URL As String = "~/COM00001LOGON.aspx"
    ''' <summary>
    ''' アップロード処理用ハンドラーURL
    ''' </summary>
    Public Const C_UPLOAD_HANDLER_URL As String = "~/COH0001FILEUP.ashx"
    '********************************************
    'コード関連
    '********************************************
    ''' <summary>
    ''' JOT AGENT(JPA00001)
    ''' </summary>
    Public Const C_JOT_AGENT As String = "JPA00001"
    ''' <summary>
    ''' システムコード 海外(GB)
    ''' </summary>
    Public Const C_SYSCODE_GB As String = "GB"
    ''' <summary>
    ''' サーバー番号取得キー(FIXVALUEのCLASSと紐づけるキー)
    ''' </summary>
    Public Const C_SERVERSEQ As String = "SERVERSEQ"
    '********************************************
    '添付ファイル共通処理用
    '********************************************
    Public Const C_DTNAME_ATTACHMENT As String = "DT_ATTACHMENT"
    ''' <summary>
    ''' 言語設定
    ''' </summary>
    Public Class C_LANG
        ''' <summary>
        ''' 日本語
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property JA As String = "JA"
        ''' <summary>
        ''' 英語
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property EN As String = "EN"
    End Class
    ''' <summary>
    ''' ヘッダー日付フォーマット
    ''' </summary>
    Public Class C_HEADER_DATE_FORMAT
        ''' <summary>
        ''' 日本語表示時のフォーマット
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property JA As String = "yyyy年MM月dd日 HH時mm分"
        ''' <summary>
        ''' 英語表示時のフォーマット
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property EN As String = "yyyy-MM-dd HH:mm"
    End Class
    ''' <summary>
    ''' シーケンス名関連
    ''' </summary>
    Public Class C_SQLSEQ
        ''' <summary>
        ''' ブレーカーID用SEQ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BREAKER As String = "GBQ0001_BREAKER"
        ''' <summary>
        ''' マスタ申請用SEQ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property MASTER As String = "GBQ0002_MASTER"
        ''' <summary>
        ''' オーダーNO用SEQ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ORDER As String = "GBQ0003_ORDER"
        ''' <summary>
        ''' ブレーカー申請用SEQ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BREAKERWORK As String = "GBQ0004_BREAKERWORK"
        ''' <summary>
        ''' ノンブレーカー用ODERNO SEQ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NONBREAKER As String = "GBQ0005_NONBREAKER"
        ''' <summary>
        ''' オーダー申請用SEQ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ORDERAPPLY As String = "GBQ0006_ORDERAPPLY"
        ''' <summary>
        ''' タンク申請用SEQ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property TKAAPPLY As String = "GBQ0008_TKAAPPLY"
        ''' <summary>
        ''' SOQ締め申請用SEQ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property SCLOSEAPPLY As String = "GBQ0009_SCLOSEAPPLY"
        ''' <summary>
        ''' B/LNO用SEQ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BL As String = "GBQ0013_BL"
        ''' <summary>
        ''' リース協定書申請
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property LEASEAGREEMENTAPPLY As String = "GBQ0012_LEASEAGREEMENTAPPLY"
    End Class
    '1つの括りで複数ある場合は以下のように書いたほうが使うときに候補が出て便利です。
    '直下の場合「RETURNCODE.」で候補が出て「RETURNCODE.NORMAL」選択可能
    '却下の場合はPublic constで、
    ''' <summary>
    ''' BASEDLLのリターンコード
    ''' </summary>
    Public Class C_MESSAGENO
        ''' <summary>
        ''' 正常("00000")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMAL As String = "00000"
        ''' <summary>
        ''' オンラインサービスは停止しています。("00001")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ONLINESTOP As String = "00001"
        ''' <summary>
        ''' ユーザＩＤ、パスワードを入力して下さい("00002")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property INPUTIDPASS As String = "00002"
        ''' <summary>
        ''' デフォルトサーバと相違するサーバに接続します。再度「実行」ボタンを押下してください("00003")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CONNECTOTHERSERVER As String = "00003"
        ''' <summary>
        ''' パスワード有効期限が近づいています。パスワード変更を行ってください("00004")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PASSEXPIRESOON As String = "00004"
        ''' <summary>
        ''' 表追加　正常終了("00005")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALLISTADDED As String = "00005"
        ''' <summary>
        ''' クリアー　正常終了("00006")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALCLEAR As String = "00006"
        ''' <summary>
        ''' 絞り込み正常("00007")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALEXTRUCT As String = "00007"
        ''' <summary>
        ''' DB更新正常終了("00008")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALDBENTRY As String = "00008"
        ''' <summary>
        ''' インポート　正常終了。("00009")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALIMPORT As String = "00009"
        ''' <summary>
        ''' パスワード有効期限は?01です。
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PASSEXPIREINFO As String = "00011"
        ''' <summary>
        ''' もう１度入力してください（再入力値不一致）。("00012")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property REINPUTVALUE As String = "00012"
        ''' <summary>
        ''' アップロード　正常終了。("00020")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALUPLOAD As String = "00020"
        ''' <summary>
        ''' ダウンロード　正常終了。("00021")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALDOWNLOAD As String = "00021"
        ''' <summary>
        ''' コピー　正常終了("00015")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALCOPY As String = "00015"
        ''' <summary>
        ''' 登録正常終了("00017")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALENTRY As String = "00017"
        ''' <summary>
        ''' 費用登録　正常終了("00018")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMALENTRYCOST As String = "00018"
        ''' <summary>
        ''' セッションが切れています("00019")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property SESSIONEXPIRED As String = "00019"
        ''' <summary>
        ''' ユーザＩＤ、パスワードに誤りが有ります。("10001")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property WRONGIDPASS As String = "10001"
        ''' <summary>
        ''' この操作を行うために必要なアクセス権がありません("10002")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ACCESSDENIED As String = "10002"
        ''' <summary>
        ''' 端末ＩＤに誤りがあります。("10003")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property WRONGTERMID As String = "10003"
        ''' <summary>
        ''' 選択不可能な値です("10004")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property INVALIDINPUT As String = "10004"
        ''' <summary>
        ''' 取得データ0件("10005")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NODATA As String = "10005"
        ''' <summary>
        ''' 申請中のレコードは更新できません。("10006")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property HASAPPLYINGRECORD As String = "10006"
        ''' <summary>
        ''' 更新出来ないレコードが発生しました(既に他端末で更新済み)。("10007")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CANNOTUPDATE As String = "10007"
        ''' <summary>
        ''' 更新出来ないレコードが発生しました(右Boxのエラー詳細を参照 )。("10008")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RIGHTBIXOUT As String = "10008"
        ''' <summary>
        ''' エラーが存在します。(権限無)("10009")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NOAUTHERROR As String = "10009"
        ''' <summary>
        ''' 訂正中("10011")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property REVISING As String = "10011"
        ''' <summary>
        ''' 既に更新済みです。("10012")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ALREADYUDPATED As String = "10012"
        ''' <summary>
        ''' マスタ間の使用可否が不一致です。("10013")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property UNMATCHMASTERUSE As String = "10013"
        ''' <summary>
        ''' 無効の国連番号が選択されています。("10014")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property INVALIDUNNO As String = "10014"
        ''' <summary>
        ''' 更新対象件数が０件です。("10015")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NOENTRYDATA As String = "10015"
        ''' <summary>
        ''' 未保存の費用データがあります、保存してください("10016")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NOSAVECOSTITEM As String = "10016"
        ''' <summary>
        ''' 同じ名前が登録されています。("10017")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DUPLICATENAME As String = "10017"
        ''' <summary>
        ''' 入力した値は使用禁止です。("10018")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PROHIBITCHAR As String = "10018"
        ''' <summary>
        ''' 必須費用項目が削除されています。("10019")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DELETEREQUIREDCOST As String = "10019"
        ''' <summary>
        ''' 対象ファイルが存在しません。("10020")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property FILENOTEXISTS As String = "10020"
        ''' <summary>
        ''' 未引当のタンクが存在しません("10022")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NONALLOCATETANKEXISTS As String = "10022"
        ''' <summary>
        ''' タンク選択件数超過("10023")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property TOOMANYALOCATETANKS As String = "10023"
        ''' <summary>
        ''' 申請内容が未入力です。("10025")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPLYREASONNOINPUT As String = "10025"
        ''' <summary>
        ''' 申請内容が未入力の申請をスキップしました。("10026")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property SKIPAPPLYITEM As String = "10026"
        ''' <summary>
        ''' 未保存の項目があります、保存してください。("10027")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property HASNOSAVEITEMS As String = "10027"
        ''' <summary>
        ''' HIREAGEがマイナス("10030")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property HIREAGEISNAGATIVE As String = "10030"
        ''' <summary>
        ''' PDFリスト存在チェック("10038")
        ''' ※現状マスタになし！
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PDFLISTEXISTS As String = "10038"
        ''' <summary>
        ''' 未入力チェックエラー("30001")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property REQUIREDVALUE As String = "30001"
        ''' <summary>
        ''' 有効年月日指定エラー("30002")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property VALIDITYINPUT As String = "30002"
        ''' <summary>
        ''' 選択不可エラー("30003")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property UNSELECTABLEERR As String = "30003"
        ''' <summary>
        ''' 入力値エラー("30004")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property INPUTERROR As String = "30004"
        ''' <summary>
        ''' 参照されたアカウントは現在ロックアウトされているため、ログオンできない可能性があります。("70001")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ACCOUNTLOCKED As String = "70001"
        ''' <summary>
        ''' ファイル形式が正しくありません。("70004")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property INCORRECTFILETYPE As String = "70004"
        ''' <summary>
        ''' アップロードできる数を超えています。("70005")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property TOOMANYUPLOADFILES As String = "70005"
        ''' <summary>
        ''' 申請中の為、ファイルアップロードできません。("70006")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CANNOTUPLOADAPPLYING As String = "70006"
        '*********************************
        '管理者へ連絡関連
        '*********************************
        ''' <summary>
        ''' システム管理者へ連絡("20001")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property SYSTEMADM As String = "20001"
        ''' <summary>
        ''' システム管理者へ連絡してください(CODE:80001)。
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DLLIFERROR As String = "80001"
        ''' <summary>
        ''' システム管理者へ連絡("80003")
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>システム管理者関係は要整理</remarks>
        Public Shared ReadOnly Property SYSTEMADM80003 As String = "80003"
        ''' <summary>
        ''' システム管理者へ連絡(CODE:89001)。
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property EXCEPTION As String = "89001"
        '*********************************
        '申請関連
        '*********************************
        ''' <summary>
        ''' 申請　正常終了。("00016")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPLYSUCCESS As String = "00016"
        ''' <summary>
        ''' 承認　正常終了("00013")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALSUCCESS As String = "00013"
        ''' <summary>
        ''' 否認　正常終了("00014")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property REJECTSUCCESS As String = "00014"
        '*********************************
        '確認メッセージ関連
        '*********************************
        ''' <summary>
        ''' 編集中のレコードが存在します。終了してもよろしいですか？("00022")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CONFIRMCLOSE As String = "00022"
        ''' <summary>
        ''' 削除します、よろしいですか？("00023")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CONFIRMDELETE As String = "00023"
        ''' <summary>
        ''' POL/PODを変更しますか？("00024")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CONFIRMPORTMODIFIED As String = "00024"
        ''' <summary>
        ''' 編集中のレコードが存在します。出力してもよろしいですか？("00026")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CONFIRMOUTPUT As String = "00026"
    End Class

    ''' <summary>
    ''' メッセージタイプ
    ''' </summary>
    Public Class C_NAEIW
        ''' <summary>
        ''' 正常メッセージ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NORMAL As String = "N"
        ''' <summary>
        ''' アブノーマルエラー
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ABNORMAL As String = "A"
        ''' <summary>
        ''' エラー
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property [ERROR] As String = "E"
        ''' <summary>
        ''' 情報
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property INFORMATION As String = "I"
        ''' <summary>
        ''' 警告
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property WARNING As String = "W"
        ''' <summary>
        ''' 確認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property QUESTION As String = "Q"
    End Class

    ''' <summary>
    ''' 実行区分
    ''' </summary>
    Public Class C_RUNKBN
        ''' <summary>
        ''' オンライン
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ONLINE As String = "ONLINE"
        ''' <summary>
        ''' バッチ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BATCH As String = "BATCH"
    End Class
    ''' <summary>
    ''' FIXVALUEクラス
    ''' </summary>
    Public Class C_FIXVALUECLAS
        ''' <summary>
        ''' ブレーカー除外費用コード("BREAKEREXCLUSION")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BREX As String = "BREAKEREXCLUSION"
        ''' <summary>
        ''' 数字の0～9を英語に置換("CONVERT")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CONV_NUM_ENG As String = "CONVERT"
        ''' <summary>
        ''' USD小数点位置("DECIMALPLACES")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property USD_DECIMALPLACES As String = "DECIMALPLACES"
    End Class
    ''' <summary>
    ''' 業者マスタ
    ''' </summary>
    Public Class C_TRADER
        ''' <summary>
        ''' 分類
        ''' </summary>
        Public Class [CLASS]
            ''' <summary>
            ''' AGENT("AGENT")
            ''' </summary>
            ''' <returns></returns>
            Public Shared ReadOnly Property AGENT As String = "AGENT"
            ''' <summary>
            ''' CARRIER("CARRIER")
            ''' </summary>
            ''' <returns></returns>
            Public Shared ReadOnly Property CARRIER As String = "CARRIER"
            ''' <summary>
            ''' TRUCKER("TRUCKER")
            ''' </summary>
            ''' <returns></returns>
            Public Shared ReadOnly Property TRUCKER As String = "TRUCKER"
        End Class
    End Class
    ''' <summary>
    ''' 顧客タイプ
    ''' </summary>
    Public Class C_CUSTOMERTYPE
        ''' <summary>
        ''' 顧客タイプSHIPPER("1")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property SHIPPER As String = "1"
        ''' <summary>
        ''' 顧客タイプCONSIGNEE("2")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CONSIGNEE As String = "2"
        ''' <summary>
        ''' 顧客タイプ共通("3")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property COMMON As String = "3"
    End Class
    ''' <summary>
    ''' ブレーカータイプ
    ''' </summary>
    ''' <remarks>文言ではなくGBT0001_BR_INFO.BRTYPEやGBT0004_ODR_BASE.BRTYPE
    ''' での利用想定なので利用時注意</remarks>
    Public Class C_BRTYPE
        ''' <summary>
        ''' セールスブレーカー("SALES")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property SALES As String = "SALES"
        ''' <summary>
        ''' オペレーションブレーカー("OPERATION")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property OPERATION As String = "OPERATION"
        ''' <summary>
        ''' リペアブレーカー("REPAIR")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property REPAIR As String = "REPAIR"
        ''' <summary>
        ''' ノンブレーカー("NONBREAKER")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NONBR As String = "NONBREAKER"
        ''' <summary>
        ''' リースブレーカー("LEASE")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property LEASE As String = "LEASE"
    End Class
    ''' <summary>
    ''' セールスブレーカー申請イベント
    ''' </summary>
    Public Class C_BRSEVENT
        ''' <summary>
        ''' 費用入力依頼（POL）
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property COSTIN_POL As String = "BRS_CostIn_POL"

        ''' <summary>
        ''' 費用入力依頼（POD）
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property COSTIN_POD As String = "BRS_CostIn_POD"

        ''' <summary>
        ''' 費用入力完了（POL）
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property COSTFN_POL As String = "BRS_CostFn_POL"

        ''' <summary>
        ''' 費用入力完了（POD）
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property COSTFN_POD As String = "BRS_CostFn_POD"

        ''' <summary>
        ''' 承認申請
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPLY As String = "BRS_Apply"

        ''' <summary>
        ''' 承認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALOK As String = "BRS_ApprovalOK"

        ''' <summary>
        ''' 否認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALNG As String = "BRS_ApprovalNG"
        ''' <summary>
        ''' 削除（POL）
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DELETE_POL As String = "BRS_Delete_POL"

        ''' <summary>
        ''' 削除（POD）
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DELETE_POD As String = "BRS_Delete_POD"
    End Class
    ''' <summary>
    ''' オーダー申請イベント
    ''' </summary>
    Public Class C_ODREVENT
        ''' <summary>
        ''' 承認申請
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPLY As String = "ODR_Apply"

        ''' <summary>
        ''' 承認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALOK As String = "ODR_ApprovalOK"

        ''' <summary>
        ''' 否認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALNG As String = "ODR_ApprovalNG"
    End Class
    ''' <summary>
    ''' リペア申請イベント
    ''' </summary>
    Public Class C_BRREVENT
        ''' <summary>
        ''' 承認申請
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPLY As String = "BRR_Apply"

        ''' <summary>
        ''' 承認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALOK As String = "BRR_ApprovalOK"

        ''' <summary>
        ''' 否認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALNG As String = "BRR_ApprovalNG"
    End Class
    ''' <summary>
    ''' タンク引当申請イベント(TanK Allocate Event)
    ''' </summary>
    Public Class C_TKAEVENT
        ''' <summary>
        ''' 承認申請
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPLY As String = "TKA_Apply"

        ''' <summary>
        ''' 承認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALOK As String = "TKA_ApprovalOK"

        ''' <summary>
        ''' 否認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALNG As String = "TKA_ApprovalNG"
    End Class
    ''' <summary>
    ''' SOA CLOSE申請イベント(SOA CLOSE EVENT)
    ''' </summary>
    Public Class C_SCLOSEEVENT
        ''' <summary>
        ''' 承認申請
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPLY As String = "SCLOSE_Apply"

        ''' <summary>
        ''' 承認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALOK As String = "SCLOSE_Approved"

        ''' <summary>
        ''' 否認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALNG As String = "SCLOSE_Rejected"
    End Class
    Public Class C_LEASEEVENT
        ''' <summary>
        ''' 承認申請
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPLY As String = "LEASE_Apply"

        ''' <summary>
        ''' 承認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALOK As String = "LEASE_ApprovalOK"

        ''' <summary>
        ''' 否認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALNG As String = "LEASE_ApprovalNG"
    End Class
    ''' <summary>
    ''' ユーザーマスタ申請イベント
    ''' </summary>
    Public Class C_USEMSTEVENT
        ''' <summary>
        ''' 承認申請
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPLY As String = "MSTUser_Apply"

        ''' <summary>
        ''' 承認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALOK As String = "MSTUser_ApprovalOK"

        ''' <summary>
        ''' 否認
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property APPROVALNG As String = "MSTUser_ApprovalNG"
    End Class
End Module
