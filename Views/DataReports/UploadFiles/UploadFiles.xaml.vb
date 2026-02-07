Imports System.IO
Imports Microsoft.Win32
Imports MongoDB.Driver
Imports MongoDB.Bson
Imports System.Collections.ObjectModel

Namespace DPC.Views.DataReports.UploadFiles
    Public Class UploadFiles
        Private ReadOnly collectionName As String = "media_files"
        Private files As ObservableCollection(Of MediaFileItem)

        Public Sub New()
            InitializeComponent()
            files = New ObservableCollection(Of MediaFileItem)()
            dgFiles.ItemsSource = files

            ' Load files when the control is loaded
            AddHandler Loaded, AddressOf UploadFiles_Loaded
        End Sub

        Private Async Sub UploadFiles_Loaded(sender As Object, e As RoutedEventArgs)
            Await LoadFilesAsync()
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

                    ' Show uploading status
                    txtStatus.Text = $"Uploading {fileInfo.Name}..."

                    ' Read file as byte array
                    Dim fileBytes() As Byte = File.ReadAllBytes(openFileDialog.FileName)

                    ' Convert to Base64
                    Dim base64String As String = Convert.ToBase64String(fileBytes)

                    ' Upload to MongoDB
                    Await UploadFileToMongoDBAsync(fileInfo.Name, fileInfo.Extension, fileInfo.Length, base64String)

                    MessageBox.Show($"File '{fileInfo.Name}' uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                    ' Reload the files list
                    Await LoadFilesAsync()
                End If

            Catch ex As Exception
                MessageBox.Show($"Error selecting/uploading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                txtStatus.Text = "Error uploading file"
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

        Private Async Function LoadFilesAsync() As Task
            Try
                txtStatus.Text = "Loading files..."
                files.Clear()

                ' Get database connection
                Dim database As IMongoDatabase = SplashScreen.GetMongoDatabaseConnection()
                Dim collection As IMongoCollection(Of BsonDocument) = database.GetCollection(Of BsonDocument)(collectionName)

                ' Get all documents (excluding the file data to improve performance)
                Dim projection = Builders(Of BsonDocument).Projection.Exclude("fileData")
                Dim documents = Await collection.Find(New BsonDocument()).Project(projection).ToListAsync()

                ' Convert to MediaFileItem objects
                For Each doc In documents
                    Dim fileSize As Long = If(doc.Contains("fileSize"), doc("fileSize").AsInt64, 0)

                    Dim fileItem As New MediaFileItem With {
                        .Id = doc("_id").ToString(),
                        .FileName = If(doc.Contains("fileName"), doc("fileName").AsString, "Unknown"),
                        .FileExtension = If(doc.Contains("fileExtension"), doc("fileExtension").AsString, ""),
                        .FileSize = fileSize,
                        .FileSizeFormatted = FormatFileSize(fileSize),
                        .UploadDate = If(doc.Contains("uploadDate"), doc("uploadDate").ToUniversalTime(), DateTime.MinValue),
                        .ContentType = If(doc.Contains("contentType"), doc("contentType").AsString, "")
                    }

                    files.Add(fileItem)
                Next

                ' Update status
                txtStatus.Text = $"{files.Count} file(s) found"

            Catch ex As Exception
                MessageBox.Show($"Error loading files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                txtStatus.Text = "Error loading files"
            End Try
        End Function

        Private Function FormatFileSize(bytes As Long) As String
            Dim sizes() As String = {"B", "KB", "MB", "GB", "TB"}
            Dim order As Integer = 0
            Dim size As Double = bytes

            While size >= 1024 AndAlso order < sizes.Length - 1
                order += 1
                size = size / 1024
            End While

            Return $"{size:0.##} {sizes(order)}"
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

    ' MediaFileItem class
    Public Class MediaFileItem
        Public Property Id As String
        Public Property FileName As String
        Public Property FileExtension As String
        Public Property FileSize As Long
        Public Property FileSizeFormatted As String
        Public Property UploadDate As DateTime
        Public Property ContentType As String

        Public ReadOnly Property UploadDateFormatted As String
            Get
                Return UploadDate.ToString("MM/dd/yyyy hh:mm tt")
            End Get
        End Property
    End Class
End Namespace