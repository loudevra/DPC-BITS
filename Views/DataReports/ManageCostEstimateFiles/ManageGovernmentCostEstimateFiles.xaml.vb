Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Warehouse
Imports Microsoft.Win32
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.GridFS
Imports DPC.SplashScreen

Namespace DPC.Views.DataReports.ManageGovernmentCostEstimateFiles
    Public Class ManageGovernmentCostEstimateFiles
        Inherits UserControl

        ' Observable collection for data binding
        Private _costEstimateFiles As ObservableCollection(Of CostEstimateFileModel)
        Private _gridFS As GridFSBucket
        Private _mongoDatabase As IMongoDatabase
        Private _fsFilesCollection As IMongoCollection(Of BsonDocument)
        Private _currentPage As Integer = 1
        Private _pageSize As Integer = 10
        Private _totalRecords As Integer = 0
        Private _searchText As String = String.Empty
        Private _uploadDate As DateTime? = Nothing

        Public Sub New()
            InitializeComponent()
            Initialize()
        End Sub

        Private Sub Initialize()
            Try
                ' Initialize MongoDB connections
                _mongoDatabase = DPC.SplashScreen.GetMongoDatabaseConnection()
                _gridFS = DPC.SplashScreen.GetGridFSConnection()

                ' Get the fs.files collection (GridFS metadata collection)
                _fsFilesCollection = _mongoDatabase.GetCollection(Of BsonDocument)("fs.files")

                ' Initialize collection
                _costEstimateFiles = New ObservableCollection(Of CostEstimateFileModel)()

                ' Bind DataGrid
                dgFiles.ItemsSource = _costEstimateFiles

                ' Set up event handlers
                AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
                AddHandler cboPageSize.SelectionChanged, AddressOf CboPageSize_SelectionChanged

                ' Load initial data
                LoadCostEstimateFiles()

            Catch ex As Exception
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

#Region "Data Loading"

        ''' <summary>
        ''' Loads cost estimate files from MongoDB GridFS using fs.files collection
        ''' </summary>
        Private Async Sub LoadCostEstimateFiles()
            Try
                ' Show loading indicator (optional)
                mainGrid.IsEnabled = False
                Cursor = Cursors.Wait

                Await Task.Run(Sub() LoadFilesAsync())

                ' Update pagination
                UpdatePagination()

            Catch ex As Exception
                MessageBox.Show($"Error loading files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                mainGrid.IsEnabled = True
                Cursor = Cursors.Arrow
            End Try
        End Sub

        ''' <summary>
        ''' Async method to load files from GridFS fs.files collection
        ''' </summary>
        Private Async Function LoadFilesAsync() As Task
            Try
                ' Build filter for fs.files collection
                Dim filterBuilder = Builders(Of BsonDocument).Filter
                Dim filters As New List(Of FilterDefinition(Of BsonDocument))()

                ' Exclude files starting with "GPCE-"
                Dim exclusionFilter = filterBuilder.Regex("filename", New BsonRegularExpression("^(GPCE-)"))
                filters.Add(exclusionFilter)

                ' Get selected file code filter
                Dim selectedFileCode As String = String.Empty
                Dispatcher.Invoke(Sub()
                                      If cboFileCode IsNot Nothing AndAlso cboFileCode.SelectedItem IsNot Nothing Then
                                          Dim selectedItem = TryCast(cboFileCode.SelectedItem, ComboBoxItem)
                                          If selectedItem IsNot Nothing Then
                                              selectedFileCode = selectedItem.Content.ToString()
                                          End If
                                      End If
                                  End Sub)

                ' Add file code filter if not "All"
                If Not String.IsNullOrEmpty(selectedFileCode) AndAlso selectedFileCode <> "All" Then
                    ' Filter files that start with the selected code (e.g., "BL-", "PO-", etc.)
                    Dim fileCodeFilter = filterBuilder.Regex("filename", New BsonRegularExpression($"^{selectedFileCode}-", "i"))
                    filters.Add(fileCodeFilter)
                End If

                ' Add search filter if search text is provided
                If Not String.IsNullOrWhiteSpace(_searchText) Then
                    Dim searchFilter = filterBuilder.Regex("filename", New BsonRegularExpression(_searchText, "i"))
                    filters.Add(searchFilter)
                End If

                ' Add upload date filter (exact date match)
                If _uploadDate.HasValue Then
                    ' Get start of selected date (00:00:00)
                    Dim dateStart = _uploadDate.Value.Date.ToUniversalTime()
                    ' Get end of selected date (23:59:59)
                    Dim dateEnd = _uploadDate.Value.Date.AddDays(1).ToUniversalTime()

                    ' Filter for files uploaded on the selected date
                    Dim dateFilter = filterBuilder.And(
                        filterBuilder.Gte(Of BsonDateTime)("uploadDate", New BsonDateTime(dateStart)),
                        filterBuilder.Lt(Of BsonDateTime)("uploadDate", New BsonDateTime(dateEnd))
                    )
                    filters.Add(dateFilter)
                End If

                ' Combine all filters
                Dim filter As FilterDefinition(Of BsonDocument)
                If filters.Count > 1 Then
                    filter = filterBuilder.And(filters)
                Else
                    filter = filters(0)
                End If

                ' Get total count for pagination
                _totalRecords = CInt(Await _fsFilesCollection.CountDocumentsAsync(filter))

                ' Calculate skip value for pagination
                Dim skip As Integer = (_currentPage - 1) * _pageSize

                ' Build sort (newest first)
                Dim sort = Builders(Of BsonDocument).Sort.Descending("uploadDate")

                ' Fetch files with pagination from fs.files collection
                Dim files = Await _fsFilesCollection.Find(filter) _
                    .Sort(sort) _
                    .Skip(skip) _
                    .Limit(_pageSize) _
                    .ToListAsync()

                ' Update collection on UI thread
                Dispatcher.Invoke(Sub()
                                      _costEstimateFiles.Clear()
                                      For Each file In files
                                          ' Extract metadata from BsonDocument
                                          Dim fileId As String = file("_id").ToString()
                                          Dim fileName As String = If(file.Contains("filename"), file("filename").AsString, "Unknown")
                                          Dim fileLength As Long = If(file.Contains("length"), file("length").AsInt64, 0)
                                          Dim uploadDate As DateTime = If(file.Contains("uploadDate"), file("uploadDate").ToUniversalTime(), DateTime.MinValue)

                                          ' Get content type from metadata if exists
                                          Dim contentType As String = "Unknown"

                                          If file.Contains("filename") Then
                                              Dim extension As String = System.IO.Path.GetExtension(fileName).ToLower()

                                              Select Case extension
                                                  Case ".pdf"
                                                      contentType = "PDF Document"
                                                  Case ".jpg", ".jpeg", ".png"
                                                      contentType = "Image"
                                                  Case ".xlsx", ".xls"
                                                      contentType = "Excel Spreadsheet"
                                                  Case ".docx", ".doc"
                                                      contentType = "Word Document"
                                                  Case ".txt"
                                                      contentType = "Text Document"
                                                  Case Else
                                                      If Not String.IsNullOrEmpty(extension) Then
                                                          contentType = extension.ToUpper().Replace(".", "") & " File"
                                                      End If
                                              End Select
                                          End If

                                          _costEstimateFiles.Add(New CostEstimateFileModel With {
                                              .FileId = fileId,
                                              .FileName = fileName,
                                              .FileSize = FormatFileSize(fileLength),
                                              .UploadDate = uploadDate.ToLocalTime(),
                                              .ContentType = contentType
                                          })
                                      Next
                                  End Sub)

            Catch ex As Exception
                Throw New Exception($"Error in LoadFilesAsync: {ex.Message}", ex)
            End Try
        End Function

#End Region

#Region "File Operations"

        ''' <summary>
        ''' Downloads a file from GridFS
        ''' </summary>
        Public Async Function DownloadFile(fileId As String, fileName As String) As Task(Of Boolean)
            Try
                ' Open save file dialog
                Dim saveDialog As New SaveFileDialog With {
                    .FileName = fileName,
                    .Filter = "All Files (*.*)|*.*"
                }

                If saveDialog.ShowDialog() = True Then
                    mainGrid.IsEnabled = False
                    Cursor = Cursors.Wait

                    Await Task.Run(Async Function()
                                       Using fileStream As New FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write)
                                           Await _gridFS.DownloadToStreamAsync(New ObjectId(fileId), fileStream)
                                       End Using
                                   End Function)

                    MessageBox.Show("File downloaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    Return True
                End If

                Return False

            Catch ex As Exception
                MessageBox.Show($"Error downloading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            Finally
                mainGrid.IsEnabled = True
                Cursor = Cursors.Arrow
            End Try
        End Function

        ''' <summary>
        ''' Deletes a file from GridFS (removes from both fs.files and fs.chunks)
        ''' </summary>
        Public Async Function DeleteFile(fileId As String, fileName As String) As Task(Of Boolean)
            Try
                Dim result = MessageBox.Show($"Are you sure you want to delete '{fileName}'?",
                                            "Confirm Delete",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question)

                If result = MessageBoxResult.Yes Then
                    mainGrid.IsEnabled = False
                    Cursor = Cursors.Wait

                    Await Task.Run(Sub()
                                       ' GridFS.Delete removes from both fs.files and fs.chunks
                                       _gridFS.Delete(New ObjectId(fileId))
                                   End Sub)

                    MessageBox.Show("File deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                    ' Reload the file list
                    LoadCostEstimateFiles()

                    Return True
                End If

                Return False

            Catch ex As Exception
                MessageBox.Show($"Error deleting file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            Finally
                mainGrid.IsEnabled = True
                Cursor = Cursors.Arrow
            End Try
        End Function

        ''' <summary>
        ''' Uploads a file to GridFS (stores in fs.files and fs.chunks)
        ''' </summary>
        Public Async Function UploadFile() As Task(Of Boolean)
            Try
                Dim openDialog As New OpenFileDialog With {
                    .Filter = "All Files (*.*)|*.*",
                    .Multiselect = False
                }

                If openDialog.ShowDialog() = True Then
                    mainGrid.IsEnabled = False
                    Cursor = Cursors.Wait

                    Await Task.Run(Async Function()
                                       Using fileStream As New FileStream(openDialog.FileName, FileMode.Open, FileAccess.Read)
                                           ' Create metadata for the file
                                           Dim options As New GridFSUploadOptions With {
    .Metadata = New BsonDocument From {
        {"contentType", Path.GetExtension(openDialog.FileName)}, ' <--- THIS LINE GRABS THE FILE TYPE
        {"uploadedBy", Environment.UserName},
        {"uploadedDate", DateTime.UtcNow},
        {"originalPath", openDialog.FileName}
    }
}

                                           ' Upload to GridFS (automatically stores in fs.files and fs.chunks)
                                           Await _gridFS.UploadFromStreamAsync(Path.GetFileName(openDialog.FileName), fileStream, options)
                                       End Using
                                   End Function)

                    MessageBox.Show("File uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                    ' Reload the file list
                    LoadCostEstimateFiles()

                    Return True
                End If

                Return False

            Catch ex As Exception
                MessageBox.Show($"Error uploading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            Finally
                mainGrid.IsEnabled = True
                Cursor = Cursors.Arrow
            End Try
        End Function

        ''' <summary>
        ''' Gets file details from fs.files collection by ID
        ''' </summary>
        Public Async Function GetFileDetails(fileId As String) As Task(Of BsonDocument)
            Try
                Dim filter As New BsonDocument("_id", New ObjectId(fileId))
                Dim fileDocument = Await _fsFilesCollection.Find(filter).FirstOrDefaultAsync()
                Return fileDocument
            Catch ex As Exception
                MessageBox.Show($"Error getting file details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Checks if a file exists in fs.files collection
        ''' </summary>
        Public Async Function FileExists(fileName As String) As Task(Of Boolean)
            Try
                Dim filterBuilder = Builders(Of BsonDocument).Filter
                Dim filter = filterBuilder.Eq(Of String)("filename", fileName)
                Dim count = Await _fsFilesCollection.CountDocumentsAsync(filter)
                Return count > 0
            Catch ex As Exception
                Return False
            End Try
        End Function

#End Region

#Region "Helper Functions"

        ''' <summary>
        ''' Formats file size to human-readable format
        ''' </summary>
        Private Function FormatFileSize(bytes As Long) As String
            If bytes = 0 Then Return "0 B"

            Dim units As String() = {"B", "KB", "MB", "GB", "TB"}
            Dim unitIndex As Integer = 0
            Dim size As Double = bytes

            While size >= 1024 AndAlso unitIndex < units.Length - 1
                size /= 1024
                unitIndex += 1
            End While

            Return $"{size:F2} {units(unitIndex)}"
        End Function

#End Region

#Region "Pagination"

        ''' <summary>
        ''' Updates pagination controls
        ''' </summary>
        Private Sub UpdatePagination()
            paginationPanel.Children.Clear()

            Dim totalPages As Integer = Math.Ceiling(_totalRecords / _pageSize)

            If totalPages <= 1 Then Return

            ' Previous button
            Dim btnPrevious As New Button With {
                .Content = "Previous",
                .Margin = New Thickness(5, 0, 5, 0),
                .Padding = New Thickness(10, 5, 10, 5),
                .IsEnabled = _currentPage > 1
            }
            AddHandler btnPrevious.Click, Sub() NavigateToPage(_currentPage - 1)
            paginationPanel.Children.Add(btnPrevious)

            ' Page numbers
            Dim startPage As Integer = Math.Max(1, _currentPage - 2)
            Dim endPage As Integer = Math.Min(totalPages, _currentPage + 2)

            For i As Integer = startPage To endPage
                Dim pageNum As Integer = i
                Dim btnPage As New Button With {
                    .Content = i.ToString(),
                    .Margin = New Thickness(2, 0, 2, 0),
                    .Padding = New Thickness(10, 5, 10, 5),
                    .FontWeight = If(i = _currentPage, FontWeights.Bold, FontWeights.Normal),
                    .Background = If(i = _currentPage, Brushes.LightBlue, Brushes.White)
                }
                AddHandler btnPage.Click, Sub() NavigateToPage(pageNum)
                paginationPanel.Children.Add(btnPage)
            Next

            ' Next button
            Dim btnNext As New Button With {
                .Content = "Next",
                .Margin = New Thickness(5, 0, 5, 0),
                .Padding = New Thickness(10, 5, 10, 5),
                .IsEnabled = _currentPage < totalPages
            }
            AddHandler btnNext.Click, Sub() NavigateToPage(_currentPage + 1)
            paginationPanel.Children.Add(btnNext)

            ' Page info
            Dim txtPageInfo As New TextBlock With {
                .Text = $"Page {_currentPage} of {totalPages} ({_totalRecords} total files)",
                .Margin = New Thickness(15, 0, 0, 0),
                .VerticalAlignment = VerticalAlignment.Center,
                .FontFamily = New FontFamily("Lexend"),
                .Foreground = New SolidColorBrush(Color.FromRgb(&H47, &H47, &H47))
            }
            paginationPanel.Children.Add(txtPageInfo)
        End Sub

        ''' <summary>
        ''' Navigates to specific page
        ''' </summary>
        Private Sub NavigateToPage(pageNumber As Integer)
            _currentPage = pageNumber
            LoadCostEstimateFiles()
        End Sub

        ''' <summary>
        ''' Handles file code filter selection change
        ''' </summary>
        Private Sub CboFileCode_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboFileCode.SelectionChanged
            ' Reset to first page when filter changes
            _currentPage = 1
            ' Reload files with the current filter
            LoadCostEstimateFiles()
        End Sub

#End Region

#Region "Event Handlers"

        ''' <summary>
        ''' Handles search text change
        ''' </summary>
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            _searchText = txtSearch.Text.Trim()
            _currentPage = 1 ' Reset to first page on search
            LoadCostEstimateFiles()
        End Sub

        ''' <summary>
        ''' Handles page size change
        ''' </summary>
        Private Sub CboPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If cboPageSize.SelectedItem IsNot Nothing Then
                Dim selectedItem = TryCast(cboPageSize.SelectedItem, ComboBoxItem)
                If selectedItem IsNot Nothing Then
                    _pageSize = Integer.Parse(selectedItem.Content.ToString())
                    _currentPage = 1 ' Reset to first page
                    LoadCostEstimateFiles()
                End If
            End If
        End Sub

        ''' <summary>
        ''' Download button click handler
        ''' </summary>
        Private Async Sub BtnDownload_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim fileModel = TryCast(btn.Tag, CostEstimateFileModel)
                If fileModel IsNot Nothing Then
                    Await DownloadFile(fileModel.FileId, fileModel.FileName)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Delete button click handler
        ''' </summary>
        Private Async Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim fileModel = TryCast(btn.Tag, CostEstimateFileModel)
                If fileModel IsNot Nothing Then
                    Await DeleteFile(fileModel.FileId, fileModel.FileName)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Upload button click handler
        ''' </summary>
        Private Async Sub BtnUpload_Click(sender As Object, e As RoutedEventArgs)
            Await UploadFile()
        End Sub

        ''' <summary>
        ''' Handles upload date picker change
        ''' </summary>
        Private Sub DpUploadDate_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            _uploadDate = dpUploadDate.SelectedDate
            _currentPage = 1
            LoadCostEstimateFiles()
        End Sub

        ''' <summary>
        ''' Clears the selected date from the date picker
        ''' </summary>
        Private Sub ClearDateButton_Click(sender As Object, e As RoutedEventArgs)
            dpUploadDate.SelectedDate = Nothing
            _uploadDate = Nothing
            _currentPage = 1
            LoadCostEstimateFiles()
        End Sub
#End Region

    End Class

#Region "Data Model"

    ''' <summary>
    ''' Model for Cost Estimate File from fs.files collection
    ''' </summary>
    Public Class CostEstimateFileModel
        Implements INotifyPropertyChanged

        Private _fileId As String
        Private _fileName As String
        Private _fileSize As String
        Private _uploadDate As DateTime
        Private _contentType As String

        Public Property FileId As String
            Get
                Return _fileId
            End Get
            Set(value As String)
                _fileId = value
                OnPropertyChanged("FileId")
            End Set
        End Property

        Public Property FileName As String
            Get
                Return _fileName
            End Get
            Set(value As String)
                _fileName = value
                OnPropertyChanged("FileName")
            End Set
        End Property

        Public Property FileSize As String
            Get
                Return _fileSize
            End Get
            Set(value As String)
                _fileSize = value
                OnPropertyChanged("FileSize")
            End Set
        End Property

        Public Property UploadDate As DateTime
            Get
                Return _uploadDate
            End Get
            Set(value As DateTime)
                _uploadDate = value
                OnPropertyChanged("UploadDate")
            End Set
        End Property

        Public Property ContentType As String
            Get
                Return _contentType
            End Get
            Set(value As String)
                _contentType = value
                OnPropertyChanged("ContentType")
            End Set
        End Property

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Sub OnPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
    End Class

#End Region

End Namespace