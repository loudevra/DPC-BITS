Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports Microsoft.Win32

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
                    _documents.Where(Function(d) d.Title.ToLower().Contains(_searchText.ToLower()))
                )
            End If

            ' Calculate total pages
            _totalPages = Math.Ceiling(_filteredDocuments.Count / _itemsPerPage)
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
                ' Create a temporary file path
                Dim tempFilePath As String = Path.Combine(Path.GetTempPath(), document.FileName)

                Try
                    ' Save the file content to the temp location
                    DocumentController.Base64ToFile(document.FileContent, tempFilePath)

                    ' Open the file with default application
                    Process.Start(tempFilePath)
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

            ' Confirm deletion
            Dim result = MessageBox.Show("Are you sure you want to delete this document?",
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
            ' Export documents to Excel
            Try
                ' Create save file dialog
                Dim saveFileDialog As New SaveFileDialog With {
                    .Filter = "Excel files (*.xlsx)|*.xlsx",
                    .Title = "Export Documents to Excel",
                    .FileName = "Documents_Export_" & DateTime.Now.ToString("yyyyMMdd")
                }

                If saveFileDialog.ShowDialog() = True Then
                    ' Create Excel export (using a helper class that would need to be implemented)
                    Dim excelExporter As New ExcelExporter()
                    excelExporter.ExportDocuments(_filteredDocuments, saveFileDialog.FileName)

                    MessageBox.Show("Documents exported successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If
            Catch ex As Exception
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            _searchText = TxtSearch.Text
            _currentPage = 1
            ApplyFilters()
        End Sub

        Private Sub CmbEntriesCount_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim comboBoxItem = DirectCast(CmbEntriesCount.SelectedItem, ComboBoxItem)
            _itemsPerPage = Integer.Parse(comboBoxItem.Content.ToString())
            _currentPage = 1
            ApplyFilters()
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