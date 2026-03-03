' ProjectModel.vb
Namespace DPC

    Public Class ProjectModel
        Public Property ProjectID As Integer
        Public Property Task As String
        Public Property Customer As String
        Public Property ABC As String
        Public Property StartDate As DateTime?
        Public Property DueDate As DateTime?
        Public Property Status As String
        Public Property AssignedTo As String
    End Class

End Namespace