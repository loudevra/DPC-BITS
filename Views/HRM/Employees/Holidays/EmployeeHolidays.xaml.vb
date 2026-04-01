Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Microsoft.Win32
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.HRM.Employees.Holidays

    Public Class EmployeeHolidays
        Inherits UserControl

        ' 1. THIS IS THE LIVE LIST THAT UPDATES THE TABLE
        Public Property HolidayList As New ObservableCollection(Of HolidayModel)()

        Public Sub New()
            InitializeComponent()

            ' 2. Link the table to the list
            dataGrid.ItemsSource = HolidayList
        End Sub

        Private Sub AddHolidayControl(sender As Object, e As RoutedEventArgs)
            Dim addHolidayControl As New DPC.Components.Forms.AddHoliday()

            ' 3. PASS THE LIST TO THE POPUP
            addHolidayControl.ParentHolidayList = HolidayList

            Dim parentWindow = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, addHolidayControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub
        Private Sub ExportToExcel_Click(sender As Object, e As RoutedEventArgs)
            Try
                ' Check if there is data to export
                If HolidayList Is Nothing OrElse HolidayList.Count = 0 Then
                    MessageBox.Show("No data available to export.", "Export to Excel", MessageBoxButton.OK, MessageBoxImage.Information)
                    Return
                End If

                ' Open a save file dialog
                Dim saveFileDialog As New SaveFileDialog()
                saveFileDialog.Filter = "Excel CSV (*.csv)|*.csv"
                saveFileDialog.FileName = "Employee_Holidays_Export.csv"
                saveFileDialog.Title = "Save Employee Holidays"

                If saveFileDialog.ShowDialog() = True Then
                    ' Write data to the selected file
                    Using writer As New StreamWriter(saveFileDialog.FileName)
                        ' 1. Write the Column Headers
                        writer.WriteLine("ID,From Date,To Date,Days,Note")

                        ' 2. Loop through the list and write the data
                        For Each holiday In HolidayList
                            ' We must handle commas inside the "Note" text so it doesn't break Excel columns
                            Dim cleanNote As String = ""
                            If Not String.IsNullOrEmpty(holiday.Note) Then
                                ' Wrap the note in quotes and escape existing quotes
                                cleanNote = $"""{holiday.Note.Replace("""", """""")}"""
                            End If

                            ' Write the row
                            writer.WriteLine($"{holiday.ID},{holiday.FromDate},{holiday.ToDate},{holiday.Days},{cleanNote}")
                        Next
                    End Using

                    MessageBox.Show("Data successfully exported to Excel!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                MessageBox.Show($"An error occurred while exporting: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim searchText As String = txtSearch.Text.ToLower().Trim()

            ' If the search box is empty, show everything
            If String.IsNullOrEmpty(searchText) Then
                dataGrid.ItemsSource = HolidayList
            Else
                ' Filter the list based on ID (converted to string) or Note
                ' You can add FromDate or ToDate here as well
                Dim filteredList = HolidayList.Where(Function(h)
                                                         Return h.ID.ToString().Contains(searchText) OrElse
                                                                (h.Note IsNot Nothing AndAlso h.Note.ToLower().Contains(searchText)) OrElse
                                                                (h.FromDate IsNot Nothing AndAlso h.FromDate.ToLower().Contains(searchText))
                                                     End Function).ToList()

                ' Update the DataGrid to show only filtered results
                dataGrid.ItemsSource = filteredList
            End If
        End Sub
        ' --- DELETE LOGIC ---
        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            Dim item = TryCast(btn.CommandParameter, HolidayModel)

            If item IsNot Nothing Then
                Dim result = MessageBox.Show("Are you sure you want to delete this holiday?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)
                If result = MessageBoxResult.Yes Then
                    HolidayList.Remove(item)
                    ' Refresh the ID numbering
                    For i As Integer = 0 To HolidayList.Count - 1
                        HolidayList(i).ID = i + 1
                    Next
                End If
            End If
        End Sub

        ' --- EDIT LOGIC ---
        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            Dim item = TryCast(btn.CommandParameter, HolidayModel)

            If item IsNot Nothing Then
                ' Open the same AddHoliday form but in "Edit mode"
                Dim editForm As New DPC.Components.Forms.AddHoliday()
                editForm.ParentHolidayList = HolidayList

                ' We pass the item to a specialized sub in the AddHoliday form
                editForm.PrepareForEdit(item)

                Dim parentWindow = Window.GetWindow(Me)
                PopupHelper.OpenPopupWithControl(sender, editForm, "windowcenter", True, -50, 0, parentWindow)
            End If
        End Sub
    End Class

    ' 4. THE DATA MODEL (Tells the table what columns exist)
    Public Class HolidayModel
        Public Property ID As Integer
        Public Property FromDate As String
        Public Property ToDate As String
        Public Property Days As Integer
        Public Property Note As String
        Public Property Action As String
    End Class

End Namespace