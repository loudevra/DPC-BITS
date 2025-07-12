Imports System
Imports DPC
Imports DPC.DPC.Data.Controllers
Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.IdentityModel.Tokens
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Helpers ' Import PBKDF2Hasher
Imports Microsoft.AspNetCore.Cryptography.KeyDerivation
Imports System.Windows
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Components.Navigation
Imports System.Collections.ObjectModel ' Required for MessageBox.Show()
Imports MongoDB.Driver
Imports MongoDB.Bson

Namespace DPC.Data.Controllers
    Public Class FilesController
        Public Shared Function LoadFilesFromMongo() As ObservableCollection(Of Files)
            Dim collection = DPC.SplashScreen.GetMongoDatabaseConnection.GetCollection(Of BsonDocument)("fs.files")
            Dim bsonFiles = collection.Find(Builders(Of BsonDocument).Filter.Empty).ToList()

            Dim fileList As New ObservableCollection(Of Files)()

            For Each doc As BsonDocument In bsonFiles
                Dim file As New Files With {
                ._id = doc("_id").AsObjectId,
                .FileName = doc("filename").AsString,
                .ChunkSize = doc("chunkSize").ToInt32(),
                .FileUploadDate = doc("uploadDate").ToUniversalTime()
            }

                If doc.Contains("metadata") AndAlso doc("metadata").IsBsonDocument Then
                    Dim metadataDoc = doc("metadata").AsBsonDocument
                    file.Metadata = New FileMetadata With {
                    .UploadedBy = If(metadataDoc.Contains("uploadedBy"), metadataDoc("uploadedBy").AsString, ""),
                    .Source = If(metadataDoc.Contains("source"), metadataDoc("source").AsString, ""),
                    .QuoteNumber = If(metadataDoc.Contains("quoteNumber"), metadataDoc("quoteNumber").AsString, "")
                }
                End If

                fileList.Add(file)
            Next

            Return fileList
        End Function
    End Class
End Namespace