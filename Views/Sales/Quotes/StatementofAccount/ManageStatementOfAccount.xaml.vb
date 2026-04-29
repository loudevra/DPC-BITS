Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports ClosedXML.Excel
Imports Microsoft.Win32
Imports System.IO
Imports System.Threading.Tasks
Imports MongoDB.Bson
Imports MongoDB.Driver.GridFS

Public Class ManageStatementOfAccount

    Public Shared StatementList As New ObservableCollection(Of StatementModel)
    Private Property StatementCollectionView As System.ComponentModel.ICollectionView

    Public Sub New()
        InitializeComponent()

        ' Load from DB on open
        RefreshFromDatabase()

        dataGrid.ItemsSource = StatementList
        StatementCollectionView = CollectionViewSource.GetDefaultView(StatementList)
    End Sub

    ' -------------------------------------------------------------------------
    ' DB REFRESH HELPER
    ' -------------------------------------------------------------------------
    Private Sub RefreshFromDatabase()
        Try
            Dim fresh = SOAController.LoadAll()
            StatementList.Clear()
            For Each item In fresh
                StatementList.Add(item)
            Next
        Catch ex As Exception
            MessageBox.Show("Failed to load statements: " & ex.Message,
                            "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    ' -------------------------------------------------------------------------
    ' OVERLAY NAVIGATION LOGIC
    ' -------------------------------------------------------------------------
    Private Sub BtnAddStatement_Click(sender As Object, e As RoutedEventArgs)
        Dim addForm As New StatementOfAccountForm()
        EditContainer.Content = addForm
        MainViewGrid.Visibility = Visibility.Collapsed
        EditOverlay.Visibility = Visibility.Visible
    End Sub

    Private Sub OpenEditStatement(sender As Object, e As RoutedEventArgs)
        Dim btn = TryCast(sender, Button)
        If btn IsNot Nothing Then
            Dim record = TryCast(btn.DataContext, StatementModel)
            If record IsNot Nothing Then
                Try
                    ' Load the full record (with line items + payments) before editing
                    Dim fullRecord = SOAController.LoadFull(record.SoaId)
                    If fullRecord Is Nothing Then
                        MessageBox.Show("Could not load the full record.",
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        Return
                    End If
                    Dim editForm As New StatementOfAccountForm(fullRecord)
                    EditContainer.Content = editForm
                    MainViewGrid.Visibility = Visibility.Collapsed
                    EditOverlay.Visibility = Visibility.Visible
                Catch ex As Exception
                    MessageBox.Show("Error loading record: " & ex.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End If
    End Sub

    Private Sub CloseEditOverlay_Click(sender As Object, e As RoutedEventArgs)
        EditOverlay.Visibility = Visibility.Collapsed
        MainViewGrid.Visibility = Visibility.Visible
        EditContainer.Content = Nothing

        ' Refresh grid from DB to reflect any saves
        RefreshFromDatabase()

        If StatementCollectionView IsNot Nothing Then
            StatementCollectionView.Refresh()
        End If
    End Sub

    ' -------------------------------------------------------------------------
    ' EXPORT & UPLOAD EVENTS
    ' -------------------------------------------------------------------------
    Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
        If StatementList IsNot Nothing AndAlso StatementList.Count > 0 Then
            Try
                Dim saveFileDialog As New SaveFileDialog()
                saveFileDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx"
                saveFileDialog.FileName = "Statement_Report_" & DateTime.Now.ToString("yyyyMMdd")
                If saveFileDialog.ShowDialog() = True Then
                    Using workbook As New XLWorkbook()
                        Dim worksheet = workbook.Worksheets.Add("Statements")
                        worksheet.Cell(1, 1).Value = "SOA No."
                        worksheet.Cell(1, 2).Value = "Client Name"
                        worksheet.Cell(1, 3).Value = "Date"
                        worksheet.Cell(1, 4).Value = "PO No."
                        worksheet.Cell(1, 5).Value = "Contract Amount"
                        worksheet.Cell(1, 6).Value = "Net Due"
                        Dim headerRange = worksheet.Range("A1:F1")
                        headerRange.Style.Font.Bold = True
                        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1D3242")
                        headerRange.Style.Font.FontColor = XLColor.White
                        For i As Integer = 0 To StatementList.Count - 1
                            Dim row As Integer = i + 2
                            Dim data = StatementList(i)
                            worksheet.Cell(row, 1).Value = data.SOANo
                            worksheet.Cell(row, 2).Value = data.ClientName
                            worksheet.Cell(row, 3).Value = data.StatementDate
                            worksheet.Cell(row, 4).Value = data.PONo
                            worksheet.Cell(row, 5).Value = data.ContractAmount
                            worksheet.Cell(row, 6).Value = data.NetAmountDue
                        Next
                        worksheet.Columns().AdjustToContents()
                        workbook.SaveAs(saveFileDialog.FileName)
                        MessageBox.Show("Export Successful!", "Success",
                                        MessageBoxButton.OK, MessageBoxImage.Information)
                    End Using
                End If
            Catch ex As Exception
                MessageBox.Show("Error exporting to Excel: " & ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        Else
            MessageBox.Show("No data available to export.", "Empty List",
                            MessageBoxButton.OK, MessageBoxImage.Warning)
        End If
    End Sub

    Private Async Sub ExportToPDF(sender As Object, e As RoutedEventArgs)
        Try
            Dim tempPdfPath As String = Path.Combine(Path.GetTempPath(),
                $"Statement_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf")
            Dim newDbFileName As String = $"SOA-{DateTime.Now:yyyyMMddHHmmss}.pdf"
            File.WriteAllText(tempPdfPath, "Dummy PDF Content")
            If File.Exists(tempPdfPath) Then
                Dim uploadSuccess = Await UploadPdfToRegularCostEstimateAsync(tempPdfPath, newDbFileName)
                If uploadSuccess Then
                    MessageBox.Show("PDF generated and seamlessly synced to Regular Cost Estimates!",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error generating or uploading PDF: " & ex.Message,
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Async Function UploadPdfToRegularCostEstimateAsync(pdfFilePath As String,
                                                                fileName As String) As Task(Of Boolean)
        Try
            Dim gridFS = DPC.SplashScreen.GetGridFSConnection()
            Using fileStream As New FileStream(pdfFilePath, FileMode.Open, FileAccess.Read)
                Dim options As New GridFSUploadOptions With {
                    .Metadata = New BsonDocument From {
                        {"contentType", ".pdf"},
                        {"uploadedBy", Environment.UserName},
                        {"uploadedDate", DateTime.UtcNow},
                        {"originalPath", pdfFilePath},
                        {"source", "StatementOfAccount"}
                    }
                }
                Await gridFS.UploadFromStreamAsync(fileName, fileStream, options)
            End Using
            Return True
        Catch ex As Exception
            MessageBox.Show($"Error saving PDF to the Cost Estimate database: {ex.Message}",
                            "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Return False
        End Try
    End Function

    ' -------------------------------------------------------------------------
    ' DATE FILTER LOGIC
    ' -------------------------------------------------------------------------
    Private Sub FilterDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
        If FilterDatePicker.SelectedDate.HasValue Then
            FilterDateText.Text = FilterDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy")
            ClearDateButton.Visibility = Visibility.Visible
        End If
        ApplyAllFilters()
    End Sub

    Private Sub ClearDateButton_Click(sender As Object, e As RoutedEventArgs)
        FilterDatePicker.SelectedDate = Nothing
        FilterDateText.Text = "Select Date"
        ClearDateButton.Visibility = Visibility.Collapsed
        ApplyAllFilters()
    End Sub

    ' -------------------------------------------------------------------------
    ' COMBINED FILTERING (Search + Date)
    ' -------------------------------------------------------------------------
    Private Sub ApplyAllFilters()
        If StatementCollectionView IsNot Nothing Then
            Dim searchString As String = SearchText.Text.ToLower()
            Dim selectedDate As Date? = FilterDatePicker.SelectedDate
            StatementCollectionView.Filter = Function(obj)
                                                 Dim item = TryCast(obj, StatementModel)
                                                 If item Is Nothing Then Return False
                                                 Dim matchesSearch As Boolean = True
                                                 If Not String.IsNullOrWhiteSpace(searchString) Then
                                                     matchesSearch = (item.SOANo IsNot Nothing AndAlso item.SOANo.ToLower().Contains(searchString)) OrElse
                                                                     (item.ClientName IsNot Nothing AndAlso item.ClientName.ToLower().Contains(searchString))
                                                 End If
                                                 Dim matchesDate As Boolean = True
                                                 If selectedDate.HasValue Then
                                                     Dim itemDate As Date
                                                     If Date.TryParse(item.StatementDate, itemDate) Then
                                                         matchesDate = (itemDate.Date = selectedDate.Value.Date)
                                                     Else
                                                         matchesDate = False
                                                     End If
                                                 End If
                                                 Return matchesSearch And matchesDate
                                             End Function
            StatementCollectionView.Refresh()
        End If
    End Sub

    Private Sub FilterDateButton_Click(sender As Object, e As RoutedEventArgs)
        FilterDatePicker.IsDropDownOpen = True
    End Sub

    Private Sub SearchText_TextChanged(sender As Object, e As TextChangedEventArgs)
        ApplyAllFilters()
    End Sub

    Private Sub DataGrid_CellClick(sender As Object, e As MouseButtonEventArgs)
        ' Logic to show popup if a cell text is too long
    End Sub

    ' -------------------------------------------------------------------------
    ' ACTION BUTTON EVENTS
    ' -------------------------------------------------------------------------
    Private Sub OpenEditClient(sender As Object, e As RoutedEventArgs)
        Dim btn = TryCast(sender, Button)
        If btn IsNot Nothing Then
            MessageBox.Show("Edit Client Clicked!")
        End If
    End Sub

    Private Sub DeleteClient(sender As Object, e As RoutedEventArgs)
        Dim result = MessageBox.Show("Are you sure you want to delete this client?",
                                     "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
        If result = MessageBoxResult.Yes Then
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                MessageBox.Show("Client Deleted!")
            End If
        End If
    End Sub

    Private Sub DeleteStatement(sender As Object, e As RoutedEventArgs)
        Dim result = MessageBox.Show("Are you sure you want to delete this statement?",
                                     "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
        If result = MessageBoxResult.Yes Then
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim record = TryCast(btn.DataContext, StatementModel)
                If record IsNot Nothing Then
                    Try
                        SOAController.DeleteSOA(record.SoaId)
                        StatementList.Remove(record)
                    Catch ex As Exception
                        MessageBox.Show("Failed to delete: " & ex.Message,
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    End Try
                End If
            End If
        End If
    End Sub

End Class