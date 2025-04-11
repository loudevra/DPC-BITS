Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Imports System.Diagnostics
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.Misc.Documents
    Partial Public Class Documents
        Inherits Window

        Private _documentController As New DocumentController()
        Private _documents As ObservableCollection(Of Document)
        Private _filteredDocuments As ObservableCollection(Of Document)
        Private _currentPage As Integer = 1
        Private _itemsPerPage As Integer = 10
        Private _totalPages As Integer = 1
        Private _currentEmployeeID As Integer
        Private _searchText As String = ""

        Public Sub New(employeeID As Integer)
            InitializeComponent()

            ' Store the employee ID
            _currentEmployeeID = employeeID

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Child = topNavBar

            ' Load documents for the current employee
            LoadDocuments()

            ' Initialize combobox selection
            If CmbEntriesCount.Items.Count > 0 Then
                CmbEntriesCount.SelectedIndex = 0
            End If
        End Sub

        Private Sub LoadDocuments()
            ' Get documents for the current employee
            _documents = _documentController.GetDocumentsByEmployeeID(_currentEmployeeID)

            ' Apply filtering
            ApplyFilters()
        End Sub

        Private Sub ApplyFilters()
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

            ' Update pagination text
            Dim startItem = Math.Min((_currentPage - 1) * _itemsPerPage + 1, _filteredDocuments.Count)
            Dim endItem = Math.Min(_currentPage * _itemsPerPage, _filteredDocuments.Count)
            TxtPaginationInfo.Text = $"Showing {startItem} to {endItem} of {_filteredDocuments.Count} entries"

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
                .Owner = Me
            }

            If addDocumentWindow.ShowDialog() = True Then
                ' Reload documents after adding new one
                LoadDocuments()
            End If
        End Sub

        Private Sub BtnView_Click(sender As Object, e As RoutedEventArgs)
            ' Get the document ID from the button tag
            Dim button = DirectCast(sender, Button)
            Dim documentID = Convert.ToInt32(button.Tag)

            ' Get the full document with content
            Dim document = _documentController.GetDocumentByID(documentID, _currentEmployeeID)

            If document IsNot Nothing Then
                Try
                    ' Create a temp directory specifically for our app
                    Dim tempDir As String = Path.Combine(Path.GetTempPath(), "DPC_Documents")
                    If Not Directory.Exists(tempDir) Then
                        Directory.CreateDirectory(tempDir)
                    End If

                    ' Create a temporary file path with a unique name
                    Dim tempFilePath As String = Path.Combine(tempDir, $"{document.DocumentID}_{document.FileName}")

                    ' Save the file content to the temp location
                    DocumentController.Base64ToFile(document.FileContent, tempFilePath)

                    ' Open the file with default application
                    Process.Start(New ProcessStartInfo() With {
                        .FileName = tempFilePath,
                        .UseShellExecute = True
                    })
                Catch ex As Exception
                    MessageBox.Show($"Error opening document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            Else
                MessageBox.Show("Document not found or you don't have permission to view it.",
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            ' Get the document ID from the button tag
            Dim button = DirectCast(sender, Button)
            Dim documentID = Convert.ToInt32(button.Tag)

            ' Find the document title
            Dim document = _filteredDocuments.FirstOrDefault(Function(d) d.DocumentID = documentID)
            Dim documentTitle = If(document IsNot Nothing, document.Title, "this document")

            ' Confirm deletion
            Dim result = MessageBox.Show($"Are you sure you want to delete '{documentTitle}'?",
                                       "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question)

            If result = MessageBoxResult.Yes Then
                ' Delete the document
                If _documentController.DeleteDocument(documentID, _currentEmployeeID) Then
                    MessageBox.Show("Document deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    LoadDocuments()
                Else
                    MessageBox.Show("Failed to delete document.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            End If
        End Sub

        Private Sub BtnExcel_Click(sender As Object, e As RoutedEventArgs)
            ' Export documents to Excel using the static ExcelExporter helper
            If _filteredDocuments.Count = 0 Then
                MessageBox.Show("No documents to export.", "Information", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            Dim defaultFileName As String = $"Documents_Export_{DateTime.Now:yyyyMMdd}"
            Dim worksheetName As String = "Documents"

            ' Use the ExcelExporter static method
            If ExcelExporter.ExportCollectionToExcel(_filteredDocuments, defaultFileName, worksheetName) Then
                ' Success message is handled by the ExcelExporter
            End If
        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            _searchText = TxtSearch.Text
            _currentPage = 1
            ApplyFilters()
        End Sub

        Private Sub CmbEntriesCount_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If CmbEntriesCount.SelectedItem IsNot Nothing Then
                Dim comboBoxItem = DirectCast(CmbEntriesCount.SelectedItem, ComboBoxItem)
                If Integer.TryParse(comboBoxItem.Content.ToString(), _itemsPerPage) Then
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
    End Class
End Namespace