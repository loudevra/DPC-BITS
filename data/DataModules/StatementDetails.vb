Module StatementDetails
    Public InvoiceNumberCache As String
    Public InvoiceDateCache As String
    Public DueDateCache As String
    Public TaxCache As String
    Public TotalCostCache As String
    Public noteTxt, remarksTxt, paymentTerms As String
    Public OrderItemsCache As List(Of Dictionary(Of String, String))
    Public signature As Boolean
    Public ImageCache As String
    Public PathCache As String
    Public SupplierName, OfficeAddress, City, Region, Country, Phone, Email As String
    Public Term1, Term2, Term3, Term4, Term5, Term6, Term7, Term8, Term9, Term10, Term11, Term12 As String
End Module
