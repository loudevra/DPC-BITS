Namespace DPC.Data.Model
    Public Class Project

        ' ── Existing / Legacy Properties (kept for backward compatibility) ──
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
        Public Property AssignedToName As String
        Public Property Note As String
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime

        ' ── New Properties ──
        Public Property ProjectDate As DateTime?
        Public Property ReferenceNumber As String
        Public Property ProjectTitle As String
        Public Property Category As String
        Public Property ProjectType As String
        Public Property ContactPerson As String
        Public Property ContactNumber As String
        Public Property EmailAddress As String
        Public Property AreaOfDelivery As String
        Public Property PreBidDate As DateTime?
        Public Property ClosingDate As DateTime?
        Public Property ABC As Long
        Public Property BidRFQOffer As Long
        Public Property ReceiveDate As DateTime?
        Public Property ModeOfSubmission As String
        Public Property Remarks As String
        Public Property AssignSales As String
        Public Property BidDocsLink As String

    End Class
End Namespace
