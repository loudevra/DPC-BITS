Imports System.IO
Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports Microsoft.Win32

Namespace DPC.Views.Misc.Documents
    Partial Public Class AddDocument
        Inherits Window

        Private _documentController As New DocumentController()
        Private _selectedFilePath As String = ""
        Private _currentEmployeeID As Integer

        Public Sub New(employeeID As Integer)
            InitializeComponent()

            ' Store the employee ID
            _currentEmployeeID = employeeID
        End Sub

        Private Sub BtnChooseFile_Click(sender As Object, e As RoutedEventArgs)
            ' Create open file dialog
            Dim openFileDialog As New OpenFileDialog With {
                .Filter = "Document Files|*.docx;*.doc;*.txt;*.pdf;*.xls;*.xlsx",
                .Title = "Select a Document"
            }

            If openFileDialog.ShowDialog() = True Then
                _selectedFilePath = openFileDialog.FileName
                TxtFileName.Text = Path.GetFileName(_selectedFilePath)

                ' Enable upload button if title is not empty
                BtnUpload.IsEnabled = Not String.IsNullOrWhiteSpace(TxtTitle.Text)
            End If
        End Sub

        Private Sub TxtTitle_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Enable upload button if file is selected and title is not empty
            BtnUpload.IsEnabled = Not String.IsNullOrWhiteSpace(_selectedFilePath) AndAlso
                                 Not String.IsNullOrWhiteSpace(TxtTitle.Text)
        End Sub

        Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs)
            DialogResult = False
            Close()
        End Sub

        Private Sub BtnUpload_Click(sender As Object, e As RoutedEventArgs)
            Try
                ' Validate inputs
                If String.IsNullOrWhiteSpace(TxtTitle.Text) Then
                    MessageBox.Show("Please enter a document title.", "Validation Error",
                                   MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                If String.IsNullOrWhiteSpace(_selectedFilePath) Then
                    MessageBox.Show("Please select a file to upload.", "Validation Error",
                                   MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                ' Get file info
                Dim fileInfo As New FileInfo(_selectedFilePath)
                Dim fileName As String = fileInfo.Name
                Dim fileSize As Long = fileInfo.Length
                Dim fileExtension As String = Base64Utility.GetFileExtension(fileName)
                Dim fileType As String = Base64Utility.GetFileType(fileExtension)

                ' Convert file to Base64
                Dim fileContent As String = Base64Utility.EncodeFileToBase64(_selectedFilePath)

                ' Create new document
                Dim document As New Document(
                    _currentEmployeeID,
                    TxtTitle.Text,
                    fileName,
                    fileContent,
                    fileType,
                    fileSize
                )

                ' Add document to database
                If _documentController.AddDocument(document) Then
                    MessageBox.Show("Document uploaded successfully.", "Success",
                                   MessageBoxButton.OK, MessageBoxImage.Information)
                    DialogResult = True
                    Close()
                Else
                    MessageBox.Show("Failed to upload document. Please try again.", "Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Catch ex As Exception
                MessageBox.Show($"Error uploading document: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class
End Namespace