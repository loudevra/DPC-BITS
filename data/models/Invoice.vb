Namespace DPC.Data.Model
    Public Class Checker
        Public Property SalesRep As String
        Public Property CheckedBy As String
        Public Property ApprovedBy As String
    End Class

    Public Class OrderItems
        ' --- UI & Logic Toggles ---
        Public Property IsHeaderRow As Boolean = False
        Public Property IsSubtotalRow As Boolean = False
        Public Property IsCategoryHeader As Boolean = False

        ' --- Product Information ---
        Public Property CategoryName As String = ""
        Public Property ProductName As String = ""
        Public Property ProductID As String = ""
        Public Property Description As String = ""
        Public Property ProductDescription As String = ""
        Public Property ProductDescriptionVisibility As Visibility = Visibility.Collapsed

        ' --- Delivery & Tracking (CRITICAL FOR P1/P2 LOGIC) ---
        Public Property Quantity As String = "0"
        Public Property SerialNumber As String = ""
        Public Property MaxAllowed As String = ""

        ' --- Financials ---
        Public Property UnitPrice As String = ""
        Public Property LinePrice As String = ""

        <Newtonsoft.Json.JsonIgnore>
        Public Property ProductImage As BitmapImage
    End Class
End Namespace