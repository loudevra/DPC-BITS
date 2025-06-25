Module StatementDetails
    Public InvoiceNumberCache As String
    Public InvoiceDateCache As String
    Public DueDateCache As String
    Public TaxCache As String
    Public TotalCostCache As String
    Public OrderItemsCache As List(Of Dictionary(Of String, String))
    Public signature As Boolean
    Public ImageCache As String
    Public PathCache As String
    Public SupplierName, OfficeAddress, City, Region, Country, Phone, Email As String

End Module
