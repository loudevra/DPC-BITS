Imports MongoDB.Bson

Namespace DPC.Data.Models
    Public Class Files
        Public Property _id As ObjectId
        Public Property FileName As String
        Public Property ChunkSize As Integer
        Public Property FileUploadDate As DateTime
        Public Property Metadata As FileMetadata
    End Class

    Public Class FileMetadata
        Public Property UploadedBy As String
        Public Property Source As String
        Public Property QuoteNumber As String
    End Class

End Namespace