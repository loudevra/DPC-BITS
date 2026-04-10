PayrollTxModel
Namespace DPC.SharedModels


    ' Paste the model down here, completely outside the Namespace!
    ' This forces it to be globally visible to the entire project.
    Public Class PayrollTxModel
        Public Property [Date] As String
        Public Property Debit As String
        Public Property Credit As String
        Public Property Account As String
        Public Property Employee As String
        Public Property Method As String
        Public Property Actions As String
    End Class

End Namespace
