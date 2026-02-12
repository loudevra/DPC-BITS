Imports System.IO
Imports Microsoft.Win32
Imports MongoDB.Driver
Imports MongoDB.Bson
Imports System.Collections.ObjectModel
Imports MySql.Data.MySqlClient
Imports System.Windows.Controls.Primitives

Namespace DPC.Views.DataReports.UploadFileOnline

    Public Class UploadFileOnline
        Private ReadOnly collectionName As String = "media_files"
        Private files As ObservableCollection(Of MediaFileItem)
        Private _foldersData As New ObservableCollection(Of FolderItem)
        Private _currentlySelectedFolderId As Long = 1
        Private recentlyClosedFolder As Boolean = False

        Public Sub New()
            InitializeComponent()
            files = New ObservableCollection(Of MediaFileItem)()
            dgFiles.ItemsSource = files

            ' Load files when the control is loaded
            AddHandler Loaded, AddressOf UploadFiles_Loaded
            ' Wire up DataGrid row events
            AddHandler dgFiles.LoadingRow, AddressOf DataGrid_LoadingRow

            FoldersList.ItemsSource = _foldersData
        End Sub

        Private Sub DataGrid_LoadingRow(sender As Object, e As DataGridRowEventArgs)
            ' Find the buttons in the row and attach event handlers
            AddHandler e.Row.Loaded, Sub(rowSender, rowArgs)
                                         Dim downloadButton = FindVisualChild(Of Button)(e.Row, "btnDownload")
                                         Dim deleteButton = FindVisualChild(Of Button)(e.Row, "btnDelete")

                                         If downloadButton IsNot Nothing Then
                                             AddHandler downloadButton.Click, AddressOf BtnDownload_Click
                                         End If

                                         If deleteButton IsNot Nothing Then
                                             AddHandler deleteButton.Click, AddressOf BtnDelete_Click
                                         End If
                                     End Sub
        End Sub

        ' Helper method to find visual children
        Private Function FindVisualChild(Of T As DependencyObject)(parent As DependencyObject, childName As String) As T
            If parent Is Nothing Then Return Nothing

            Dim childrenCount As Integer = VisualTreeHelper.GetChildrenCount(parent)
            For i As Integer = 0 To childrenCount - 1
                Dim child = VisualTreeHelper.GetChild(parent, i)
                Dim childType = TryCast(child, T)

                If childType IsNot Nothing AndAlso TypeOf child Is FrameworkElement Then
                    Dim frameworkElement = TryCast(child, FrameworkElement)
                    If frameworkElement.Name = childName Then
                        Return childType
                    End If
                End If

                Dim childOfChild = FindVisualChild(Of T)(child, childName)
                If childOfChild IsNot Nothing Then
                    Return childOfChild
                End If
            Next

            Return Nothing
        End Function

        Private Async Sub UploadFiles_Loaded(sender As Object, e As RoutedEventArgs)
            Folders_Load()
            Await LoadFilesAsync(_currentlySelectedFolderId)
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

                    If Await FileExistsInFolder(fileInfo.Name, fileInfo.Extension, _currentlySelectedFolderId) Then
                        MessageBox.Show($"The file '{fileInfo.Name}' already exists in this folder.", "Duplicate File", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Return
                    End If

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
                    Await UploadFileToMongoDBAsync(_currentlySelectedFolderId, fileInfo.Name, fileInfo.Extension, fileInfo.Length, base64String)

                    MessageBox.Show($"File '{fileInfo.Name}' uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                    ' Reload the files list
                    Await LoadFilesAsync(_currentlySelectedFolderId)
                End If

            Catch ex As Exception
                MessageBox.Show($"Error selecting/uploading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                txtStatus.Text = "Error uploading file"
            End Try
        End Sub

        Private Async Sub Folder_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                _currentlySelectedFolderId = CLng(btn.Tag)
                HighlightSelectedFolder()

                Await LoadFilesAsync(_currentlySelectedFolderId)
            End If
        End Sub

        Private Sub OpenAddFolderPopup(sender As Object, e As RoutedEventArgs) Handles btnAddFolder.Click
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            If recentlyClosedFolder Then
                recentlyClosedFolder = False
                Return
            End If

            Dim addFolderForm As New DPC.Components.Forms.AddFolder()
            AddHandler addFolderForm.AddFolder, AddressOf OnFolderAdded
            Dim parentWindow = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, addFolderForm, "windowcenter", True, 0, 0, parentWindow)
        End Sub


        Private Sub OnFolderAdded()
            PopupHelper.ClosePopup()
            Folders_Load()
        End Sub

        Private Sub Folders_Load()
            _foldersData.Clear()

            Try
                Using DatabaseConn = SplashScreen.GetDatabaseConnection()
                    DatabaseConn.Open()

                    Dim query As String = "SELECT id, name, description FROM folders"

                    Using cmd As New MySqlCommand(query, DatabaseConn)
                        Using reader = cmd.ExecuteReader()
                            While reader.Read()

                                Dim id As Integer = Convert.ToInt32(reader("id"))
                                Dim name As String = reader("name").ToString()
                                Dim description As String = reader("description").ToString()

                                Debug.WriteLine(id)

                                _foldersData.Add(New FolderItem(id, name, description))
                            End While
                        End Using
                    End Using
                End Using

                txtFolderStatus.Text = $"{_foldersData.Count} folder(s) found"
            Catch ex As Exception
                MessageBox.Show("Failed to load folders: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Async Function UploadFileToMongoDBAsync(folderId As Long, fileName As String, fileExtension As String, fileSize As Long, base64Data As String) As Task
            Try
                ' Use your existing connection function
                Dim database As IMongoDatabase = SplashScreen.GetMongoDatabaseConnection()
                Dim collection As IMongoCollection(Of BsonDocument) = database.GetCollection(Of BsonDocument)(collectionName)

                ' Create document to insert
                Dim document As New BsonDocument From {
                    {"_folderId", folderId},
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

        Private Async Function LoadFilesAsync(currentFolderId As Long) As Task
            Try
                txtStatus.Text = "Loading files..."
                files.Clear()

                ' Get database connection
                Dim database As IMongoDatabase = SplashScreen.GetMongoDatabaseConnection()
                Dim collection As IMongoCollection(Of BsonDocument) = database.GetCollection(Of BsonDocument)(collectionName)

                Dim filter = Builders(Of BsonDocument).Filter.Eq(Of Long)("_folderId", currentFolderId)

                ' Get all documents (excluding the file data to improve performance)
                Dim projection = Builders(Of BsonDocument).Projection.Exclude("fileData")
                Dim documents = Await collection.Find(filter).Project(projection).ToListAsync()

                ' Convert to MediaFileItem objects
                For Each doc In documents
                    ' Use ToInt64() to safely handle the NumberLong from Mongo
                    Dim fileSize As Long = If(doc.Contains("fileSize"), doc("fileSize").ToInt64(), 0)

                    Dim fileItem As New MediaFileItem With {
                        .Id = doc("_id").ToString(),
                        .FileName = If(doc.Contains("fileName"), doc("fileName").AsString, "Unknown"),
                        .FileExtension = If(doc.Contains("fileExtension"), doc("fileExtension").AsString, ""),
                        .FileSize = fileSize,
                        .FileSizeFormatted = FormatFileSize(fileSize),
                        .UploadDate = If(doc.Contains("uploadDate"), doc("uploadDate").ToUniversalTime(), DateTime.MinValue),
                        .ContentType = If(doc.Contains("contentType"), doc("contentType").AsString, ""),
                        .FolderId = If(doc.Contains("_folderId"), CInt(doc("_folderId").AsInt64), 0)
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

        ' DOWNLOAD FUNCTION
        Private Async Sub BtnDownload_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim button As Button = CType(sender, Button)
                Dim fileId As String = button.Tag.ToString()

                ' Get the file item from the list to get filename
                Dim fileItem = files.FirstOrDefault(Function(f) f.Id = fileId)
                If fileItem Is Nothing Then
                    MessageBox.Show("File not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                txtStatus.Text = $"Downloading {fileItem.FileName}..."

                ' Retrieve file from MongoDB
                Dim database As IMongoDatabase = SplashScreen.GetMongoDatabaseConnection()
                Dim collection As IMongoCollection(Of BsonDocument) = database.GetCollection(Of BsonDocument)(collectionName)

                ' Create filter using BsonDocument
                Dim filter As New BsonDocument("_id", New ObjectId(fileId))
                Dim document = Await collection.Find(filter).FirstOrDefaultAsync()

                If document Is Nothing Then
                    MessageBox.Show("File not found in database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Get the Base64 data
                Dim base64Data As String = document("fileData").AsString
                Dim fileBytes() As Byte = Convert.FromBase64String(base64Data)

                ' Open Save File Dialog
                Dim saveFileDialog As New SaveFileDialog()
                saveFileDialog.FileName = fileItem.FileName
                saveFileDialog.Filter = $"All Files (*.*)|*.*"

                If saveFileDialog.ShowDialog() = True Then
                    ' Save the file
                    File.WriteAllBytes(saveFileDialog.FileName, fileBytes)
                    MessageBox.Show($"File downloaded successfully to: {saveFileDialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    txtStatus.Text = $"{files.Count} file(s) found"
                Else
                    txtStatus.Text = "Download cancelled"
                End If

            Catch ex As Exception
                MessageBox.Show($"Error downloading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                txtStatus.Text = "Error downloading file"
            End Try
        End Sub

        ' DELETE FUNCTION
        Private Async Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim button As Button = CType(sender, Button)
                Dim fileId As String = button.Tag.ToString()

                ' Get the file item from the list to show filename in confirmation
                Dim fileItem = files.FirstOrDefault(Function(f) f.Id = fileId)
                If fileItem Is Nothing Then
                    MessageBox.Show("File not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Confirm deletion
                Dim result = MessageBox.Show($"Are you sure you want to delete '{fileItem.FileName}'?{Environment.NewLine}{Environment.NewLine}This action cannot be undone.",
                                            "Confirm Delete",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Warning)

                If result = MessageBoxResult.Yes Then
                    txtStatus.Text = $"Deleting {fileItem.FileName}..."

                    ' Delete from MongoDB
                    Dim database As IMongoDatabase = SplashScreen.GetMongoDatabaseConnection()
                    Dim collection As IMongoCollection(Of BsonDocument) = database.GetCollection(Of BsonDocument)(collectionName)

                    ' Create filter using BsonDocument
                    Dim filter As New BsonDocument("_id", New ObjectId(fileId))
                    Await collection.DeleteOneAsync(filter)

                    MessageBox.Show($"File '{fileItem.FileName}' deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                    ' Reload the files list
                    Await LoadFilesAsync(_currentlySelectedFolderId)
                End If

            Catch ex As Exception
                MessageBox.Show($"Error deleting file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                txtStatus.Text = "Error deleting file"
            End Try
        End Sub

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

        Private Async Function FileExistsInFolder(fileName As String, fileExt As String, folderId As Long) As Task(Of Boolean)
            Try
                Dim database As IMongoDatabase = SplashScreen.GetMongoDatabaseConnection()
                Dim collection = database.GetCollection(Of BsonDocument)(collectionName)

                Dim filter = Builders(Of BsonDocument).Filter.And(
                    Builders(Of BsonDocument).Filter.Eq(Of String)("fileName", fileName),
                    Builders(Of BsonDocument).Filter.Eq(Of String)("fileExtension", fileExt),
                    Builders(Of BsonDocument).Filter.Eq(Of Long)("_folderId", folderId)
                )

                ' Count documents that match all three criteria
                Dim count = Await collection.CountDocumentsAsync(filter)
                Return count > 0
            Catch ex As Exception
                Debug.WriteLine("Error checking file existence: " & ex.Message)
                Return False
            End Try
        End Function

        Private Sub HighlightSelectedFolder()
            Dispatcher.BeginInvoke(Sub()
                                       FoldersList.UpdateLayout()

                                       For i As Integer = 0 To FoldersList.Items.Count - 1
                                           Dim itemContainer = TryCast(FoldersList.ItemContainerGenerator.ContainerFromIndex(i), FrameworkElement)
                                           If itemContainer Is Nothing Then Continue For

                                           Dim folderBorder = FindVisualChild(Of Border)(itemContainer, "")
                                           Dim btn = FindVisualChild(Of Button)(itemContainer, "")

                                           If folderBorder IsNot Nothing AndAlso btn IsNot Nothing Then
                                               If CLng(btn.Tag) = _currentlySelectedFolderId Then
                                                   folderBorder.Background = New SolidColorBrush(Colors.LightBlue)
                                               Else
                                                   folderBorder.Background = New SolidColorBrush(Color.FromRgb(241, 243, 244))
                                               End If
                                           End If
                                       Next
                                   End Sub, System.Windows.Threading.DispatcherPriority.Loaded)
        End Sub
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
        Public Property FolderId As Long

        Public ReadOnly Property UploadDateFormatted As String
            Get
                Return UploadDate.ToString("MM/dd/yyyy hh:mm tt")
            End Get
        End Property
    End Class


    'FolderItem Class
    Public Class FolderItem
        Public Property ID As Integer
        Public Property Name As String
        Public Property Description As String
        Public Property ItemCount As Integer
        Public Property CreatedAt As DateTime
        Public Sub New()
            Me.CreatedAt = DateTime.Now
        End Sub

        Public Sub New(id As Integer, name As String, description As String)
            Me.ID = id
            Me.Name = name
            Me.Description = description
            Me.CreatedAt = DateTime.Now
        End Sub
    End Class
End Namespace