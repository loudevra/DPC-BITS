Imports System.IO

Imports Microsoft.Win32
Imports MongoDB.Driver
Imports MongoDB.Bson

Namespace DPC.Views.DataReports.UploadFiles
    Public Class UploadFiles
        Private ReadOnly collectionName As String = "media_files"

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Async Sub SelectFile_Click(sender As Object, e As RoutedEventArgs) Handles btnSelectFile.Click
            Try
                ' Open file dialog
                Dim openFileDialog As New OpenFileDialog()
                openFileDialog.Title = "Select File to Upload"
                openFileDialog.Filter = "All Files (*.*)|*.*|Images (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif|Documents (*.pdf;*.docx;*.txt)|*.pdf;*.docx;*.txt"
                openFileDialog.Multiselect = False

                If openFileDialog.ShowDialog() = True Then
                    ' Get file info
                    Dim fileInfo As New FileInfo(openFileDialog.FileName)

                    ' Check file size (optional - limit to 16MB for regular collection)
                    If fileInfo.Length > 16 * 1024 * 1024 Then
                        MessageBox.Show("File is too large. Maximum size is 16MB.", "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Return
                    End If

                    ' Read file as byte array
                    Dim fileBytes() As Byte = File.ReadAllBytes(openFileDialog.FileName)

                    ' Convert to Base64
                    Dim base64String As String = Convert.ToBase64String(fileBytes)

                    ' Upload to MongoDB
                    Await UploadFileToMongoDBAsync(fileInfo.Name, fileInfo.Extension, fileInfo.Length, base64String)

                    MessageBox.Show($"File '{fileInfo.Name}' uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                MessageBox.Show($"Error selecting/uploading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Async Function UploadFileToMongoDBAsync(fileName As String, fileExtension As String, fileSize As Long, base64Data As String) As Task
            Try
                ' Use your existing connection function
                Dim database As IMongoDatabase = SplashScreen.GetMongoDatabaseConnection()
                Dim collection As IMongoCollection(Of BsonDocument) = database.GetCollection(Of BsonDocument)(collectionName)

                ' Create document to insert
                Dim document As New BsonDocument From {
                    {"fileName", fileName},
                    {"fileExtension", fileExtension},
                    {"fileSize", fileSize},
                    {"uploadDate", DateTime.Now},
                    {"fileData", base64Data},
                    {"contentType", GetContentType(fileExtension)}
                }

                ' Insert into MongoDB
                Await collection.InsertOneAsync(document)

            Catch ex As Exception
                Throw New Exception($"Failed to upload to MongoDB: {ex.Message}")
            End Try
        End Function

        Private Function GetContentType(fileExtension As String) As String
            Select Case fileExtension.ToLower()
                Case ".jpg", ".jpeg"
                    Return "image/jpeg"
                Case ".png"
                    Return "image/png"
                Case ".gif"
                    Return "image/gif"
                Case ".pdf"
                    Return "application/pdf"
                Case ".docx"
                    Return "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                Case ".txt"
                    Return "text/plain"
                Case Else
                    Return "application/octet-stream"
            End Select
        End Function
    End Class
End Namespace