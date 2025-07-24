

Namespace DPC.Data.Models
    Public Class CashAdvanceEntry
        Public Property Amount As String
        Public Property DateRequested As String
        Public Property Reason As String
    End Class

    Public Class EditCashAdvanceJsonToList
        Public Property Amount As String
        Public Property DateRequested As String
        Public Property Reason As String
    End Class

    Public Class CashAdvanceRetrieval

        Public Property CashAdvanceID As String
        Public Property EmployeeID As String
        Public Property EmployeeName As String
        Public Property JobTitle As String
        Public Property Department As String
        Public Property TotalAmount As String
        Public Property CArequestDate As String
        Public Property Status As String

    End Class

End Namespace
