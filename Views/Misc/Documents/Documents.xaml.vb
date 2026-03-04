Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.Misc.Documents
    Partial Public Class Documents
        Inherits UserControl

        Private _documentController As New DocumentController()
        Private _documents As ObservableCollection(Of Document)
        Private _filteredDocuments As ObservableCollection(Of Document)
        Private _currentPage As Integer = 1
        Private _itemsPerPage As Integer = 10
        Private _totalPages As Integer = 1
        Private _currentEmployeeID As Long  ' Changed from Integer to Long
        Private _searchText As String = ""

        Public Sub New(employeeID As Long)  ' Changed from Integer to Long
            InitializeComponent()

            ' Store the employee ID
            _currentEmployeeID = employeeID

            ' Load documents for the current employee
            LoadDocuments()

            ' Initialize combobox selection
            If CmbEntriesCount.Items.Count > 0 Then
                CmbEntriesCount.SelectedIndex = 0
            End If
        End Sub

        ' Add a parameterless constructor for testing/design-time
        Public Sub New()
            InitializeComponent()

            ' Try to get current employee ID
            Try
                _currentEmployeeID = GetCurrentEmployeeID()
            Catch ex As Exception
                ' If we can't get employee ID, show empty state
                _currentEmployeeID = 0
            End Try

            ' Load documents for the current employee
            If _currentEmployeeID > 0 Then
                LoadDocuments()
            Else
                ' Show empty state
                _documents = New ObservableCollection(Of Document)()
                ApplyFilters()
            End If

            ' Initialize combobox selection
            If CmbEntriesCount.Items.Count > 0 Then
                CmbEntriesCount.SelectedIndex = 0
            End If
        End Sub

        Private Function GetCurrentEmployeeID() As Long
            ' Get the logged-in user's employee ID based on their username
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Query using full_name
                    Dim query As String = "SELECT employee_id FROM auth_users WHERE full_name = @fullname LIMIT 1"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@fullname", CacheOnLoggedInName)
                        Dim result = cmd.ExecuteScalar()

                        If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                            Dim employeeIdString As String = result.ToString().Trim()
                            Dim employeeId As Long
                            If Long.TryParse(employeeIdString, employeeId) Then
                                Return employeeId
                            End If
                        End If
                    End Using
                End Using
            Catch ex As Exception
                ' Log or handle error
                Debug.WriteLine($"Error getting employee ID: {ex.Message}")
            End Try

            Throw New InvalidOperationException("Unable to determine current employee ID.")
        End Function

        Private Sub LoadDocuments()
            Try
                ' Debug: Show what employee ID we're using
                Debug.WriteLine($"Loading documents for EmployeeID: {_currentEmployeeID}")

                ' Get documents for the current employee
                _documents = _documentController.GetDocumentsByEmployeeID(_currentEmployeeID)

                ' Debug: Show how many documents were found
                Debug.WriteLine($"Found {_documents.Count} documents")

                ' Debug: List all documents
                For Each doc In _documents
                    Debug.WriteLine($"Document: {doc.DocumentID} - {doc.Title} - EmployeeID: {doc.EmployeeID}")
                Next

                ' Apply filtering
                ApplyFilters()
            Catch ex As Exception
                Debug.WriteLine($"Error loading documents: {ex.Message}")
                MessageBox.Show($"Error loading documents: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub ApplyFilters()
            ' Guard against premature calls before initialization
            If _documents Is Nothing Then Return

            ' Apply search filter
            If String.IsNullOrEmpty(_searchText) Then
                _filteredDocuments = New ObservableCollection(Of Document)(_documents)
            Else
                _filteredDocuments = New ObservableCollection(Of Document)(
                    _documents.Where(Function(d) _
                        d.Title.ToLower().Contains(_searchText.ToLower()) OrElse
                        d.FileName.ToLower().Contains(_searchText.ToLower()) OrElse
                        d.FileType.ToLower().Contains(_searchText.ToLower())
                    )
                )
            End If

            ' Calculate total pages
            _totalPages = Math.Ceiling(_filteredDocuments.Count / CDbl(_itemsPerPage))
            If _totalPages < 1 Then _totalPages = 1

            ' Ensure current page is valid
            If _currentPage > _totalPages Then _currentPage = _totalPages

            ' Apply pagination
            Dim pagedDocuments = _filteredDocuments.Skip((_currentPage - 1) * _itemsPerPage).Take(_itemsPerPage)
            DocumentsDataGrid.ItemsSource = pagedDocuments

            ' Update current page display
            TxtCurrentPage.Text = _currentPage.ToString()

            ' Update button states
            BtnPrevious.IsEnabled = (_currentPage > 1)
            BtnNext.IsEnabled = (_currentPage < _totalPages)

            ' Update empty state visibility
            If _filteredDocuments.Count = 0 Then
                DocumentsDataGrid.Visibility = Visibility.Collapsed
                EmptyStatePanel.Visibility = Visibility.Visible
            Else
                DocumentsDataGrid.Visibility = Visibility.Visible
                EmptyStatePanel.Visibility = Visibility.Collapsed
            End If
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            ' Open the add document window
            Dim addDocumentWindow As New AddDocument(_currentEmployeeID) With {
                .Owner = Window.GetWindow(Me),
                .WindowStartupLocation = WindowStartupLocation.CenterOwner
            }

            If addDocumentWindow.ShowDialog() = True Then
                ' Reload documents after adding
                LoadDocuments()
            End If
        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            _searchText = TxtSearch.Text
            _currentPage = 1
            ApplyFilters()
        End Sub

        Private Sub CmbEntriesCount_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If CmbEntriesCount.SelectedItem IsNot Nothing Then
                Dim selectedItem = TryCast(CmbEntriesCount.SelectedItem, ComboBoxItem)
                If selectedItem IsNot Nothing Then
                    _itemsPerPage = Integer.Parse(selectedItem.Content.ToString())
                    _currentPage = 1
                    ApplyFilters()
                End If
            End If
        End Sub

        Private Sub BtnPrevious_Click(sender As Object, e As RoutedEventArgs)
            If _currentPage > 1 Then
                _currentPage -= 1
                ApplyFilters()
            End If
        End Sub

        Private Sub BtnNext_Click(sender As Object, e As RoutedEventArgs)
            If _currentPage < _totalPages Then
                _currentPage += 1
                ApplyFilters()
            End If
        End Sub

        Private Sub BtnView_Click(sender As Object, e As RoutedEventArgs)
            Dim button = TryCast(sender, Button)
            If button IsNot Nothing AndAlso button.Tag IsNot Nothing Then
                Dim documentID = Integer.Parse(button.Tag.ToString())
                Dim document = _documents.FirstOrDefault(Function(d) d.DocumentID = documentID)

                If document IsNot Nothing Then
                    ' Get the full file path
                    Dim filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads", "documents", document.FileName)

                    If File.Exists(filePath) Then
                        Try
                            ' Open the document with default application
                            Process.Start(New ProcessStartInfo(filePath) With {.UseShellExecute = True})
                        Catch ex As Exception
                            MessageBox.Show($"Error opening document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        End Try
                    Else
                        MessageBox.Show("Document file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    End If
                End If
            End If
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            Dim button = TryCast(sender, Button)
            If button IsNot Nothing AndAlso button.Tag IsNot Nothing Then
                Dim documentID = Integer.Parse(button.Tag.ToString())
                Dim document = _documents.FirstOrDefault(Function(d) d.DocumentID = documentID)

                If document IsNot Nothing Then
                    Dim result = MessageBox.Show(
                        $"Are you sure you want to delete '{document.Title}'?",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question)

                    If result = MessageBoxResult.Yes Then
                        If _documentController.DeleteDocument(documentID, _currentEmployeeID) Then
                            MessageBox.Show("Document deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                            LoadDocuments()
                        Else
                            MessageBox.Show("Failed to delete document.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        End If
                    End If
                End If
            End If
        End Sub

        Private Sub BtnExcel_Click(sender As Object, e As RoutedEventArgs)
            ' Export to Excel functionality
            Try
                ' Implement Excel export logic here
                MessageBox.Show("Excel export functionality to be implemented.", "Export", MessageBoxButton.OK, MessageBoxImage.Information)
            Catch ex As Exception
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Add a public method to refresh the documents list
        Public Sub RefreshDocuments()
            LoadDocuments()
        End Sub
    End Class
End Namespace