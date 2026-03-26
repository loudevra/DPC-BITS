Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.ComponentModel
Imports System.Windows.Data
Imports System.IO
Imports Microsoft.Win32
Imports System.Text

Namespace DPC.Views.HRM.Employees.Attendance
    Public Class AttendanceEmployee
        Inherits UserControl

        ' Data storage that automatically updates the UI
        Private AttendanceList As New ObservableCollection(Of AttendanceRecord)()
        Private currentId As Integer = 1

        Public Sub New()
            InitializeComponent()
            ' Bind the DataGrid to our collection
            dataGrid.ItemsSource = AttendanceList
        End Sub

        ' 1. Open Add Popup
        Private Sub AddAttendanceControl(sender As Object, e As RoutedEventArgs)
            Dim addAttendanceControl As New DPC.Components.Forms.AddAttendance()

            ' Subscribe to the custom Add event from the popup
            AddHandler addAttendanceControl.OnAttendanceAdded, AddressOf HandleNewAttendance

            Dim parentWindow = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, addAttendanceControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub

        ' 2. Method triggered when the Add popup fires a success event
        Private Sub HandleNewAttendance(employeeName As String, attendanceDate As String, timeIn As String, timeOut As String, note As String)
            Dim newRecord As New AttendanceRecord With {
                .ID = currentId,
                .EmployeeName = employeeName,
                .AttendanceDate = attendanceDate,
                .TimeIn = timeIn,
                .TimeOut = timeOut,
                .Note = note
            }

            AttendanceList.Add(newRecord)
            currentId += 1
        End Sub

        ' 3. Triggered every time the user types a letter in the search box
        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If dataGrid Is Nothing OrElse dataGrid.ItemsSource Is Nothing Then Return

            Dim view As ICollectionView = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource)

            If view IsNot Nothing Then
                view.Filter = AddressOf FilterAttendance
                view.Refresh()
            End If
        End Sub

        ' 4. Checks each row to see if it matches the search text
        Private Function FilterAttendance(item As Object) As Boolean
            Dim record As AttendanceRecord = TryCast(item, AttendanceRecord)
            If record Is Nothing Then Return False

            Dim searchText As String = txtSearch.Text.ToLower()

            If String.IsNullOrWhiteSpace(searchText) Then Return True

            Return (record.EmployeeName IsNot Nothing AndAlso record.EmployeeName.ToLower().Contains(searchText)) OrElse
                   (record.AttendanceDate IsNot Nothing AndAlso record.AttendanceDate.ToLower().Contains(searchText)) OrElse
                   (record.Note IsNot Nothing AndAlso record.Note.ToLower().Contains(searchText))
        End Function

        ' 5. Export to Excel functionality
        Private Sub BtnExportExcel_Click(sender As Object, e As RoutedEventArgs)
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("There is no data to export.", "Export Empty", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            Dim saveFileDialog As New SaveFileDialog()
            saveFileDialog.Filter = "Excel CSV File (*.csv)|*.csv"
            saveFileDialog.FileName = "EmployeeAttendance_" & DateTime.Now.ToString("yyyyMMdd") & ".csv"

            If saveFileDialog.ShowDialog() = True Then
                Try
                    Dim csvContent As New StringBuilder()
                    csvContent.AppendLine("ID,Employee Name,Date,Time In,Time Out,Note")

                    For Each item In dataGrid.Items
                        Dim record As AttendanceRecord = TryCast(item, AttendanceRecord)
                        If record IsNot Nothing Then
                            Dim noteSafe As String = """" & (If(record.Note, "")).Replace("""", """""") & """"
                            Dim row As String = $"{record.ID},{record.EmployeeName},{record.AttendanceDate},{record.TimeIn},{record.TimeOut},{noteSafe}"
                            csvContent.AppendLine(row)
                        End If
                    Next

                    File.WriteAllText(saveFileDialog.FileName, csvContent.ToString())
                    MessageBox.Show("Data successfully exported to Excel!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information)

                Catch ex As Exception
                    MessageBox.Show("Error exporting data: " & ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        ' 6. DELETE Functionality
        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim record As AttendanceRecord = TryCast(btn.DataContext, AttendanceRecord)

                If record IsNot Nothing Then
                    Dim result = MessageBox.Show($"Are you sure you want to delete the attendance record for {record.EmployeeName}?",
                                                 "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)

                    If result = MessageBoxResult.Yes Then
                        AttendanceList.Remove(record)
                    End If
                End If
            End If
        End Sub

        ' 7. EDIT Functionality (BULLETPROOF DATE VERSION)
        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim record As AttendanceRecord = TryCast(btn.DataContext, AttendanceRecord)

                If record IsNot Nothing Then
                    Dim editForm As New DPC.Components.Forms.AddAttendance()

                    ' --- CHANGE UI TO LOOK LIKE AN EDIT FORM ---
                    editForm.TxtTitle.Text = "Edit Attendance"
                    editForm.TxtBtnAdd.Text = "Update"
                    editForm.IconTitle.Kind = MaterialDesignThemes.Wpf.PackIconKind.SquareEditOutline

                    ' 1. Pre-fill the standard text fields
                    editForm.TxtEmployee.Text = record.EmployeeName
                    editForm.TxtNote.Text = record.Note

                    ' 2. BULLETPROOF DATE: Force the text to display exactly what is in the table NO MATTER WHAT
                    editForm.TxtDateDisplay.Text = record.AttendanceDate

                    ' Now try to silently sync the hidden calendar picker to match that date
                    Dim parsedDate As DateTime
                    If DateTime.TryParse(record.AttendanceDate, parsedDate) Then
                        editForm.SingleDatePicker.SelectedDate = parsedDate
                    End If

                    ' 3. Safely parse and set Time In
                    Dim parsedTimeIn As DateTime
                    If DateTime.TryParse(record.TimeIn, parsedTimeIn) Then
                        editForm.TpStartTime.SelectedTime = parsedTimeIn
                    End If

                    ' 4. Safely parse and set Time Out
                    Dim parsedTimeOut As DateTime
                    If DateTime.TryParse(record.TimeOut, parsedTimeOut) Then
                        editForm.TpEndTime.SelectedTime = parsedTimeOut
                    End If

                    ' Intercept the Add event to update existing record
                    AddHandler editForm.OnAttendanceAdded, Sub(empName, attDate, tIn, tOut, noteVal)

                                                               ' Create a completely new record to force WPF to redraw it instantly
                                                               Dim updatedRecord As New AttendanceRecord With {
                                                                   .ID = record.ID,
                                                                   .EmployeeName = empName,
                                                                   .AttendanceDate = attDate,
                                                                   .TimeIn = tIn,
                                                                   .TimeOut = tOut,
                                                                   .Note = noteVal
                                                               }

                                                               ' Find the old record and swap it with the updated one
                                                               Dim index = AttendanceList.IndexOf(record)
                                                               If index >= 0 Then
                                                                   AttendanceList(index) = updatedRecord
                                                               End If

                                                           End Sub

                    Dim parentWindow = Window.GetWindow(Me)
                    PopupHelper.OpenPopupWithControl(sender, editForm, "windowcenter", True, -50, 0, parentWindow)
                End If
            End If
        End Sub

    End Class

    ' Our Data Model mapping to the table columns
    Public Class AttendanceRecord
        Public Property ID As Integer
        Public Property EmployeeName As String
        Public Property AttendanceDate As String
        Public Property TimeIn As String
        Public Property TimeOut As String
        Public Property Note As String
    End Class
End Namespace