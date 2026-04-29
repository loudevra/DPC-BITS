Namespace DPC.Data.Model
    Public Class Project

        ' ── Identity ─────────────────────────────────────────────
        Public Property ProjectID As Integer

        ' ── Basic Info ───────────────────────────────────────────
        Public Property ProjectDate As DateTime?
        Public Property ReferenceNumber As String
        Public Property ProjectTitle As String
        Public Property Category As String
        Public Property ProjectType As String

        ' ── Contact Info ─────────────────────────────────────────
        Public Property ContactPerson As String
        Public Property ContactNumber As String
        Public Property EmailAddress As String
        Public Property AreaOfDelivery As String

        ' ── Dates & Financials ───────────────────────────────────
        Public Property PreBidDate As DateTime?
        Public Property ClosingDate As DateTime?
        Public Property ABC As Long
        Public Property BidRFQOffer As Long
        Public Property ReceiveDate As DateTime?

        ' ── Submission Info ──────────────────────────────────────
        Public Property ModeOfSubmission As String

        ' ── Status & Assignment ──────────────────────────────────
        Public Property Status As String
        Public Property Remarks As String
        Public Property AssignSales As String
        Public Property IsAwarded As Boolean

        ' ── Note ─────────────────────────────────────────────────
        Public Property Note As String

        ' ── Timestamps ───────────────────────────────────────────
        Public Property CreatedAt As DateTime?
        Public Property UpdatedAt As DateTime?

    End Class
End Namespace