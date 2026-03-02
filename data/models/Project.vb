Namespace DPC.Data.Model
    Public Class Project
        Public Property ProjectID As Integer
        Public Property ProjectName As String
        Public Property Status As String
        Public Property Customer As String
        Public Property Budget As Long
        Public Property StartDate As DateTime?
        Public Property DueDate As DateTime?
        Public Property CalculationMode As String
        Public Property LinkToCalendar As Boolean
        Public Property AssignedTo As String
        Public Property Note As String
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime
    End Class
End Namespace