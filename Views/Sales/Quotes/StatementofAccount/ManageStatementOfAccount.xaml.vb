Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports ClosedXML.Excel ' Make sure to add this at the very top of your file
Imports Microsoft.Win32

Public Class ManageStatementOfAccount
    ' Use the StatementModel to hold your Data
    Public Shared StatementList As New ObservableCollection(Of StatementModel)
    Private Property StatementCollectionView As System.ComponentModel.ICollectionView
    Public Sub New()
        InitializeComponent()

        ' Bind the DataGrid
        dataGrid.ItemsSource = StatementList

        ' Initialize the view for filtering
        StatementCollectionView = CollectionViewSource.GetDefaultView(StatementList)
    End Sub


    ' -------------------------------------------------------------------------
    ' EVENT HANDLERS
    ' -------------------------------------------------------------------------

    Private Sub BtnAddStatement_Click(sender As Object, e As RoutedEventArgs)
        ' Add your navigation logic here to go to the StatementOfAccountForm
        MessageBox.Show("Navigate to Add Statement Form...")
    End Sub

    Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
        ' 1. Check if there is data to export
        If StatementList IsNot Nothing AndAlso StatementList.Count > 0 Then
            Try
                ' 2. Setup Save File Dialog
                Dim saveFileDialog As New SaveFileDialog()
                saveFileDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx"
                saveFileDialog.FileName = "Statement_Report_" & DateTime.Now.ToString("yyyyMMdd")

                If saveFileDialog.ShowDialog() = True Then
                    ' 3. Create the Workbook
                    Using workbook As New XLWorkbook()
                        Dim worksheet = workbook.Worksheets.Add("Statements")

                        ' 4. Define Headers
                        worksheet.Cell(1, 1).Value = "SOA No."
                        worksheet.Cell(1, 2).Value = "Client Name"
                        worksheet.Cell(1, 3).Value = "Date"
                        worksheet.Cell(1, 4).Value = "PO No."
                        worksheet.Cell(1, 5).Value = "Contract Amount"
                        worksheet.Cell(1, 6).Value = "Net Due"

                        ' 5. Format Headers (Bold and Background color)
                        Dim headerRange = worksheet.Range("A1:F1")
                        headerRange.Style.Font.Bold = True
                        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1D3242")
                        headerRange.Style.Font.FontColor = XLColor.White

                        ' 6. Populate Data from your ObservableCollection
                        For i As Integer = 0 To StatementList.Count - 1
                            Dim row As Integer = i + 2 ' Start at row 2
                            Dim data = StatementList(i)

                            worksheet.Cell(row, 1).Value = data.SOANo
                            worksheet.Cell(row, 2).Value = data.ClientName
                            worksheet.Cell(row, 3).Value = data.StatementDate
                            worksheet.Cell(row, 4).Value = data.PONo
                            worksheet.Cell(row, 5).Value = data.ContractAmount
                            worksheet.Cell(row, 6).Value = data.NetAmountDue
                        Next

                        ' 7. Auto-fit columns for a clean look
                        worksheet.Columns().AdjustToContents()

                        ' 8. Save the file
                        workbook.SaveAs(saveFileDialog.FileName)
                        MessageBox.Show("Export Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    End Using
                End If
            Catch ex As Exception
                MessageBox.Show("Error exporting to Excel: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        Else
            MessageBox.Show("No data available to export.", "Empty List", MessageBoxButton.OK, MessageBoxImage.Warning)
        End If
    End Sub


    ' -------------------------------------------------------------------------
    ' DATE FILTER LOGIC
    ' -------------------------------------------------------------------------

    Private Sub FilterDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
        If FilterDatePicker.SelectedDate.HasValue Then
            ' Update the button text to show the selected date
            FilterDateText.Text = FilterDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy")
            ClearDateButton.Visibility = Visibility.Visible
        End If

        ' Trigger the filter refresh
        ApplyAllFilters()
    End Sub

    Private Sub ClearDateButton_Click(sender As Object, e As RoutedEventArgs)
        FilterDatePicker.SelectedDate = Nothing
        FilterDateText.Text = "Select Date"
        ClearDateButton.Visibility = Visibility.Collapsed

        ' Trigger the filter refresh
        ApplyAllFilters()
    End Sub

    ' -------------------------------------------------------------------------
    ' COMBINED FILTERING (Search + Date)
    ' -------------------------------------------------------------------------

    Private Sub ApplyAllFilters()
        If StatementCollectionView IsNot Nothing Then
            ' 1. Pull values from the UI controls
            ' Use "SearchText.Text" (the name of your TextBox)
            Dim searchString As String = SearchText.Text.ToLower()
            Dim selectedDate As Date? = FilterDatePicker.SelectedDate

            StatementCollectionView.Filter = Function(obj)
                                                 Dim item = TryCast(obj, StatementModel)
                                                 If item Is Nothing Then Return False

                                                 ' 2. LOGIC: Search Text Filter
                                                 Dim matchesSearch As Boolean = True
                                                 If Not String.IsNullOrWhiteSpace(searchString) Then
                                                     matchesSearch = (item.SOANo IsNot Nothing AndAlso item.SOANo.ToLower().Contains(searchString)) OrElse
                                                                (item.ClientName IsNot Nothing AndAlso item.ClientName.ToLower().Contains(searchString))
                                                 End If

                                                 ' 3. LOGIC: Date Filter
                                                 Dim matchesDate As Boolean = True
                                                 If selectedDate.HasValue Then
                                                     Dim itemDate As Date
                                                     ' Safely parse the string date from your model
                                                     If Date.TryParse(item.StatementDate, itemDate) Then
                                                         ' Compare only the Date part (ignoring time)
                                                         matchesDate = (itemDate.Date = selectedDate.Value.Date)
                                                     Else
                                                         matchesDate = False
                                                     End If
                                                 End If

                                                 ' 4. Combine: Row must match BOTH search and date
                                                 Return matchesSearch And matchesDate
                                             End Function

            StatementCollectionView.Refresh()
        End If
    End Sub
    ' Ensure this sub is exactly named as it is in your XAML Click event
    Private Sub FilterDateButton_Click(sender As Object, e As RoutedEventArgs)
        ' This opens the date picker dropdown when the button is clicked
        FilterDatePicker.IsDropDownOpen = True
    End Sub
    Private Sub SearchText_TextChanged(sender As Object, e As TextChangedEventArgs)
        ' Simply call the combined filter method
        ApplyAllFilters()
    End Sub

    Private Sub DataGrid_CellClick(sender As Object, e As MouseButtonEventArgs)
        ' Logic to show popup if a cell text is too long
    End Sub

    Private Sub OpenEditStatement(sender As Object, e As RoutedEventArgs)
        Dim btn = TryCast(sender, Button)
        If btn IsNot Nothing Then
            Dim record = TryCast(btn.DataContext, StatementModel)
            If record IsNot Nothing Then
                MessageBox.Show("Editing SOA: " & record.SOANo)
            End If
        End If
    End Sub
    ' -------------------------------------------------------------------------
    ' ACTION BUTTON EVENTS FOR CLIENTS
    ' -------------------------------------------------------------------------

    Private Sub OpenEditClient(sender As Object, e As RoutedEventArgs)
        Dim btn = TryCast(sender, Button)
        If btn IsNot Nothing Then
            ' Change "ClientModel" to whatever class name you use for your clients
            ' Dim record = TryCast(btn.DataContext, ClientModel)
            ' If record IsNot Nothing Then
            '     MessageBox.Show("Editing Client: " & record.ClientName)
            ' End If
            MessageBox.Show("Edit Client Clicked!")
        End If
    End Sub

    Private Sub DeleteClient(sender As Object, e As RoutedEventArgs)
        Dim result = MessageBox.Show("Are you sure you want to delete this client?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
        If result = MessageBoxResult.Yes Then
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                ' Change "ClientModel" to whatever class name you use for your clients
                ' Dim record = TryCast(btn.DataContext, ClientModel)
                ' If record IsNot Nothing Then
                '     YourClientObservableCollection.Remove(record)
                ' End If
                MessageBox.Show("Client Deleted!")
            End If
        End If
    End Sub

    Private Sub DeleteStatement(sender As Object, e As RoutedEventArgs)
        Dim result = MessageBox.Show("Are you sure you want to delete this statement?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
        If result = MessageBoxResult.Yes Then
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim record = TryCast(btn.DataContext, StatementModel)
                If record IsNot Nothing Then
                    StatementList.Remove(record)
                End If
            End If
        End If
    End Sub

End Class

' -------------------------------------------------------------------------
' DATA MODEL
' -------------------------------------------------------------------------
' Make sure this is in your correct namespace if you use them, e.g., Namespace Models

Public Class StatementModel
    ' Existing properties
    Public Property SOANo As String
    Public Property ClientName As String
    Public Property StatementDate As String
    Public Property ContractAmount As String
    Public Property NetAmountDue As String

    ' New properties to fix the errors
    Public Property ClientDetails As String
    Public Property ProjectTitle As String
    Public Property PONo As String
    Public Property SINo As String
    Public Property DRNo As String
    Public Property BSNo As String

    Public Property Subtotal As String
    Public Property TotalPayment As String
    Public Property OutstandingBalance As String
    Public Property LiquidatedDamages As String

    Public Property PODate As String
    Public Property DeliveryPeriod As String
    Public Property RequiredDate As String
    Public Property CompletionDate As String

    Public Property LDDaysDelayed As String
    Public Property LDRate As String
    Public Property LDPerDay As String

    ' Dynamic Lists
    Public Property LineItems As New List(Of LineItemModel)
    Public Property PaymentItems As New List(Of PaymentItemModel)
End Class

Public Class LineItemModel
    Public Property DateStr As String
    Public Property Description As String
    Public Property Qty As String
    Public Property Amount As String
    Public Property Payment As String
    Public Property Balance As String
End Class

Public Class PaymentItemModel
    Public Property DateStr As String
    Public Property Reference As String
    Public Property AmountPaid As String
End Class