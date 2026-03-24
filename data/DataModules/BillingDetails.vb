Module BillingDetails
    Public HeaderTitle As String = "Billing Statement"
    Public SubmitButtonText As String = "Statement"

    Public Sub ClearBillingDetails()
        ' Reset Strings to Empty
        HeaderTitle = "Billing Statement"
        DRDocumentReference = "Statement"
    End Sub
End Module
