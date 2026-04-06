Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Helpers
Imports System.ComponentModel
Imports System.Windows.Data
Imports ClosedXML.Excel
Imports Microsoft.Win32

Namespace DPC.Views.HRM.Employees.Payroll

    ' =======================================================
    ' 1. WE PUT THE MODEL RIGHT HERE! 
    ' Now it is 100% guaranteed to be seen by the compiler.
    ' =======================================================
    Public Class PayrollTxModel
        Public Property [Date] As String
        Public Property Debit As String
        Public Property Credit As String
        Public Property Account As String
        Public Property Employee As String
        Public Property Method As String
        Public Property Actions As String
    End Class

    ' =======================================================
    ' 2. YOUR MAIN PAGE CODE
    ' =======================================================
    Public Class PayrollTransaction
        Inherits UserControl

        Private _transactionList As ObservableCollection(Of PayrollTxModel)

        Public Sub New()
            InitializeComponent()
            _transactionList = New ObservableCollection(Of PayrollTxModel)()
            dataGrid.ItemsSource = _transactionList
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            Dim payrollTransactionControl As New DPC.Components.Forms.PayrollTransactionControl()
            AddHandler payrollTransactionControl.OnTransactionAdded, AddressOf HandleNewTransaction

            Dim parentWindow = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, payrollTransactionControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub

        Private Sub HandleNewTransaction(newTransaction As PayrollTxModel)
            _transactionList.Add(newTransaction)
        End Sub
        ' ==========================================
        ' DELETE BUTTON LOGIC
        ' ==========================================
        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            ' 1. Figure out which button was clicked
            Dim clickedButton = DirectCast(sender, Button)

            ' 2. Grab the specific row data attached to that button
            Dim rowData = DirectCast(clickedButton.DataContext, PayrollTxModel)

            ' 3. Ask for confirmation, then remove it from the list!
            If MessageBox.Show("Are you sure you want to delete this transaction for " & rowData.Employee & "?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                _transactionList.Remove(rowData)
            End If
        End Sub

        ' ==========================================
        ' EDIT BUTTON LOGIC
        ' ==========================================
        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            Dim clickedButton = DirectCast(sender, Button)
            Dim rowData = DirectCast(clickedButton.DataContext, PayrollTxModel)

            Dim payrollTransactionControl As New DPC.Components.Forms.PayrollTransactionControl()

            ' Tell the popup to load this existing data instead of starting blank
            payrollTransactionControl.SetEditMode(rowData)

            ' When the popup finishes saving the edit, we need to refresh the DataGrid
            AddHandler payrollTransactionControl.OnTransactionAdded, Sub(updatedRecord)
                                                                         ' Find the old record and replace it with the new one to update the UI
                                                                         Dim index = _transactionList.IndexOf(rowData)
                                                                         If index >= 0 Then
                                                                             _transactionList(index) = updatedRecord
                                                                         End If
                                                                     End Sub

            ' Open the popup
            Dim parentWindow = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, payrollTransactionControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub
        ' ==========================================
        ' SEARCH BAR LOGIC
        ' ==========================================
        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' 1. Get the "view" that the DataGrid is using to look at your list
            Dim view As ICollectionView = CollectionViewSource.GetDefaultView(_transactionList)

            ' 2. If the search box is empty, remove the filter so everything shows
            If String.IsNullOrWhiteSpace(txtSearch.Text) Then
                view.Filter = Nothing
            Else
                ' 3. Convert the search text to lowercase so the search isn't case-sensitive
                Dim searchText As String = txtSearch.Text.ToLower()

                ' 4. Create the filter rule: check if ANY column contains the typed text
                view.Filter = Function(item As Object)
                                  Dim record As PayrollTxModel = DirectCast(item, PayrollTxModel)

                                  Return (record.Employee IsNot Nothing AndAlso record.Employee.ToLower().Contains(searchText)) OrElse
                                         (record.Account IsNot Nothing AndAlso record.Account.ToLower().Contains(searchText)) OrElse
                                         (record.Date IsNot Nothing AndAlso record.Date.ToLower().Contains(searchText)) OrElse
                                         (record.Method IsNot Nothing AndAlso record.Method.ToLower().Contains(searchText)) OrElse
                                         (record.Debit IsNot Nothing AndAlso record.Debit.ToLower().Contains(searchText)) OrElse
                                         (record.Credit IsNot Nothing AndAlso record.Credit.ToLower().Contains(searchText))
                              End Function
            End If
        End Sub
        ' ==========================================
        ' EXCEL EXPORT LOGIC
        ' ==========================================
        Private Sub BtnExportExcel_Click(sender As Object, e As RoutedEventArgs)
            ' 1. Check if there is actually data to export
            If _transactionList Is Nothing OrElse _transactionList.Count = 0 Then
                MessageBox.Show("There is no data to export.", "Export Empty", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            ' 2. Open a Save File Dialog so the user can choose where to save it
            Dim saveFileDialog As New SaveFileDialog()
            saveFileDialog.Filter = "Excel Files|*.xlsx"
            saveFileDialog.Title = "Save Payroll Transactions"
            ' Give it a smart default name with today's date
            saveFileDialog.FileName = "Payroll_Transactions_" & DateTime.Now.ToString("yyyyMMdd") & ".xlsx"

            If saveFileDialog.ShowDialog() = True Then
                Try
                    ' 3. Create the Excel Workbook
                    Using workbook As New XLWorkbook()
                        Dim worksheet = workbook.Worksheets.Add("Payroll Transactions")

                        ' 4. Create the Headers
                        worksheet.Cell(1, 1).Value = "Date"
                        worksheet.Cell(1, 2).Value = "Debit"
                        worksheet.Cell(1, 3).Value = "Credit"
                        worksheet.Cell(1, 4).Value = "Account"
                        worksheet.Cell(1, 5).Value = "Employee"
                        worksheet.Cell(1, 6).Value = "Method"

                        ' Make the headers bold and light gray!
                        Dim headerRange = worksheet.Range("A1:F1")
                        headerRange.Style.Font.Bold = True
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center

                        ' 5. Loop through your DataGrid list and write the rows
                        Dim row As Integer = 2
                        For Each item In _transactionList
                            worksheet.Cell(row, 1).Value = item.Date
                            worksheet.Cell(row, 2).Value = item.Debit
                            worksheet.Cell(row, 3).Value = item.Credit
                            worksheet.Cell(row, 4).Value = item.Account
                            worksheet.Cell(row, 5).Value = item.Employee
                            worksheet.Cell(row, 6).Value = item.Method
                            row += 1
                        Next

                        ' 6. Auto-fit the columns so the text isn't cut off
                        worksheet.Columns().AdjustToContents()

                        ' 7. Save the file!
                        workbook.SaveAs(saveFileDialog.FileName)
                    End Using

                    MessageBox.Show("Excel file exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information)

                Catch ex As Exception
                    ' If the file is currently open in Excel, it will throw an error, so we catch it safely.
                    MessageBox.Show("Error exporting to Excel. Make sure the file isn't already open." & vbCrLf & ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub


    End Class
End Namespace