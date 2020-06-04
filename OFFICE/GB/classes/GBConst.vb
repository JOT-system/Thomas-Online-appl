''' <summary>
''' 海外事業定義モジュール
''' CONST名、およびCLASS名についてはプリフィックスGBC_を付与してください。
''' </summary>
Public Module GBConst
    'ほぼ単一でしか使用しない場合
    'Public const AAAA As String = "定数！！！！"
    ''' <summary>
    ''' 海外(GB)専用の会社コード
    ''' </summary>
    Public Const GBC_COMPCODE As String = "01"
    ''' <summary>
    ''' 海外(GB)専用の会社コード(Default)
    ''' </summary>
    Public Const GBC_COMPCODE_D As String = "Default" '上に統一した際は一気に変換
    ''' <summary>
    ''' 通貨コードUSD("USD")
    ''' </summary>
    Public Const GBC_CUR_USD As String = "USD"
    ''' <summary>
    ''' JOT組織コード
    ''' </summary>
    Public Const GBC_JOT_ORG As String = "JO000000"
    ''' <summary>
    ''' JOTSOA締め情報を取得するための国コード
    ''' </summary>
    Public Const GBC_JOT_SOA_COUNTRY As String = "JOT"
    ''' <summary>
    ''' 費用項目(デマレッジ)
    ''' </summary>
    Public Const GBC_COSTCODE_DEMURRAGE As String = "S0102-01"
    ''' <summary>
    ''' 費用項目:売上総額(Breaker Total Invoicing)
    ''' </summary>
    Public Const GBC_COSTCODE_SALES As String = "A0001-01"
    ''' <summary>
    ''' 費用項目：元請輸送収入(Freight Revenue)
    ''' </summary>
    Public Const GBC_COSTCODE_FREIGHT_REVENUE As String = "A0100-01"
    ''' <summary>
    ''' 費用項目：元請輸送収入(Freight Revenue)仮計上
    ''' </summary>
    Public Const GBC_COSTCODE_PROVISIONAL As String = "A0100-02"
    ''' <summary>
    ''' 費用項目:リース料(JOT Hirage)
    ''' </summary>
    Public Const GBC_COSTCODE_JOTHIRAGE As String = "S0101-01"
    ''' <summary>
    ''' 費用項目:リース料調整額(JOT Hirage(Adjustment))
    ''' </summary>
    Public Const GBC_COSTCODE_JOTHIRAGEA As String = "S0101-02"
    ''' <summary>
    ''' 費用項目:リース料その他(Hireage Other)
    ''' </summary>
    Public Const GBC_COSTCODE_HIRAGEOTHER As String = "S0101-02" '"S0102-03"
    ''' <summary>
    ''' 費用項目:リース料
    ''' </summary>
    Public Const GBC_COSTCODE_LEASE As String = "O8000-01"
    ''' <summary>
    ''' 費用項目:手数料
    ''' </summary>
    Public Const GBC_COSTCODE_AGENTCOM As String = "T0201-01"

    Public Class GBC_CHARGECLASS4
        ''' <summary>
        ''' 代理店
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property AGENT As String = "代理店"
        'Public Shared ReadOnly Property AGENT As String = "A"
        ''' <summary>
        ''' 運送会社
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property FORWARDER As String = "船社"
        'Public Shared ReadOnly Property FORWARDER As String = "F"
        ''' <summary>
        ''' 船社
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CURRIER As String = "運送会社"
        'Public Shared ReadOnly Property CURRIER As String = "C"
        ''' <summary>
        ''' 港
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PORT As String = "港"
        'Public Shared ReadOnly Property PORT As String = "P"
        ''' <summary>
        ''' 港＿内航
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PORT_I As String = "港＿内航"
        'Public Shared ReadOnly Property PORT_I As String = "PI"
        ''' <summary>
        ''' デポ
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DEPOT As String = "デポ"
        'Public Shared ReadOnly Property DEPOT As String = "D"
        ''' <summary>
        ''' 顧客
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CUSTOMER As String = "顧客"
        'Public Shared ReadOnly Property CUSTOMER As String = "CU"
        ''' <summary>
        ''' その他
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property OTHER As String = "その他"
        'Public Shared ReadOnly Property OTHER As String = "O"
    End Class
    ''' <summary>
    ''' 組織レベル
    ''' </summary>
    Public Class GBC_ORGLEVEL
        ''' <summary>
        ''' 組織レベル:国("01000")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property COUNTRY As String = "01000"
        ''' <summary>
        ''' 組織レベル:オフィス("00100")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property OFFICE As String = "00100"
        ''' <summary>
        ''' 組織レベル:港("00010")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PORT As String = "00010"
        ''' <summary>
        ''' 組織レベル:デポ("00001")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DEPOT As String = "00001"
    End Class
    ''' <summary>
    ''' 小数点処理フラグ
    ''' </summary>
    Public Class GBC_ROUNDFLG
        ''' <summary>
        ''' 切上("U")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property UP As String = "U"
        ''' <summary>
        ''' 切捨("D")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DOWN As String = "D"
        ''' <summary>
        ''' 四捨五入("R")
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ROUND As String = "R"
    End Class

    ''' <summary>
    ''' マスタ申請タイプ
    ''' </summary>
    Public Class GBC_MAT_UPDTYPE
        ''' <summary>
        ''' 登録
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property ADD As String = "Add"
        ''' <summary>
        ''' 変更
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property UPD As String = "Change"
        ''' <summary>
        ''' 削除
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DEL As String = "Delete"
    End Class

    ''' <summary>
    ''' 所属
    ''' </summary>
    Public Class GBC_PROPERTY
        ''' <summary>
        ''' 化成品部
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DOMESTIC As String = "DOMESTIC"
        ''' <summary>
        ''' 海外事業部
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property INTERNATIONAL As String = "INTERNATIONAL"
        ''' <summary>
        ''' リース会社
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property LEASE As String = "LEASE"
        ''' <summary>
        ''' 他社代行
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property SOC As String = "SOC"
    End Class
    ''' <summary>
    ''' 課税区分
    ''' </summary>
    Public Class GBC_TAXATION
        ''' <summary>
        ''' 課税
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property TAX As String = "on"
        ''' <summary>
        ''' 非課税
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property FREE As String = ""
    End Class

    ''' <summary>
    ''' 発着区分
    ''' </summary>
    Public Class GBC_DELIVERYCLASS
        ''' <summary>
        ''' Shipper
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property SHIPPER As String = "SHIPPER"
        ''' <summary>
        ''' Consignee
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property CONSIGNEE As String = "CONSIGNEE"
    End Class
End Module
