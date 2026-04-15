Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.IO
Imports Microsoft.Win32
Imports System.Text
Imports MySql.Data.MySqlClient
Imports System.Linq

Namespace DPC.Views.HRM.Employees.Attendance
    Public Class AttendanceEmployee
        Inherits UserControl

        Private AttendanceList As New ObservableCollection(Of AttendanceRecord)()
        Private _pageSize As Integer = 10

        Public Sub New()
            InitializeComponent()
            LoadAttendanceFromDatabase()
        End Sub

        Private Sub LoadAttendanceFromDatabase()
            Try
                AttendanceList.Clear()
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(
                        "SELECT ID, EmployeeName, AttendanceDate, TimeIn, TimeOut, Note
                           FROM attendance
                          ORDER BY ID", conn)
                        Using reader = cmd.ExecuteReader()
                            While reader.Read()
                                AttendanceList.Add(New AttendanceRecord With {
                                    .ID = Convert.ToInt32(reader("ID")),
                                    .EmployeeName = reader("EmployeeName").ToString(),
                                    .AttendanceDate = reader("AttendanceDate").ToString(),
                                    .TimeIn = reader("TimeIn").ToString(),
                                    .TimeOut = reader("TimeOut").ToString(),
                                    .Note = If(IsDBNull(reader("Note")), "", reader("Note").ToString())
                                })
                            End While
                        End Using
                    End Using
                End Using
                RefreshDisplay()
            Catch ex As Exception
                MessageBox.Show("Error loading attendance: " & ex.Message, "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub RefreshDisplay()
            Dim searchText As String = If(txtSearch?.Text, "").ToLower()

            Dim filtered = AttendanceList.Where(Function(r)
                                                    If String.IsNullOrWhiteSpace(searchText) Then Return True
                                                    Return (r.EmployeeName?.ToLower().Contains(searchText) = True) OrElse
                                                           (r.AttendanceDate?.ToLower().Contains(searchText) = True) OrElse
                                                           (r.Note?.ToLower().Contains(searchText) = True)
                                                End Function).Take(_pageSize).ToList()

            dataGrid.ItemsSource = filtered
        End Sub

        Private Sub CmbShowCount_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If Not IsLoaded Then Return   ' ← add this guard
            Dim selected = TryCast(CmbShowCount.SelectedItem, ComboBoxItem)
            If selected IsNot Nothing Then
                _pageSize = Convert.ToInt32(selected.Content)
                RefreshDisplay()
            End If
        End Sub

        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            RefreshDisplay()
        End Sub

        Private Sub AddAttendanceControl(sender As Object, e As RoutedEventArgs)
            Dim addAttendanceControl As New DPC.Components.Forms.AddAttendance()
            AddHandler addAttendanceControl.OnAttendanceAdded, AddressOf HandleNewAttendance
            Dim parentWindow = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, addAttendanceControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub

        Private Sub HandleNewAttendance(employeeName As String, attendanceDate As String,
                                        timeIn As String, timeOut As String, note As String)
            Try
                Dim newId As Integer
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(
                        "INSERT INTO attendance (EmployeeName, AttendanceDate, TimeIn, TimeOut, Note)
                         VALUES (@emp, @date, @tin, @tout, @note);
                         SELECT LAST_INSERT_ID();", conn)
                        cmd.Parameters.AddWithValue("@emp", employeeName)
                        cmd.Parameters.AddWithValue("@date", attendanceDate)
                        cmd.Parameters.AddWithValue("@tin", timeIn)
                        cmd.Parameters.AddWithValue("@tout", timeOut)
                        cmd.Parameters.AddWithValue("@note", note)
                        newId = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using

                AttendanceList.Add(New AttendanceRecord With {
                    .ID = newId,
                    .EmployeeName = employeeName,
                    .AttendanceDate = attendanceDate,
                    .TimeIn = timeIn,
                    .TimeOut = timeOut,
                    .Note = note
                })
                RefreshDisplay()

            Catch ex As Exception
                MessageBox.Show("Error saving attendance: " & ex.Message, "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub BtnExportExcel_Click(sender As Object, e As RoutedEventArgs)
            If AttendanceList.Count = 0 Then
                MessageBox.Show("There is no data to export.", "Export Empty",
                                MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            Dim dlg As New SaveFileDialog()
            dlg.Filter = "Excel CSV File (*.csv)|*.csv"
            dlg.FileName = "EmployeeAttendance_" & DateTime.Now.ToString("yyyyMMdd") & ".csv"

            If dlg.ShowDialog() = True Then
                Try
                    Dim csv As New StringBuilder()
                    csv.AppendLine("ID,Employee Name,Date,Time In,Time Out,Note")
                    For Each r In AttendanceList
                        Dim noteSafe As String = """" & (If(r.Note, "")).Replace("""", """""") & """"
                        csv.AppendLine($"{r.ID},{r.EmployeeName},{r.AttendanceDate},{r.TimeIn},{r.TimeOut},{noteSafe}")
                    Next
                    File.WriteAllText(dlg.FileName, csv.ToString())
                    MessageBox.Show("Data successfully exported!", "Export Success",
                                   MessageBoxButton.OK, MessageBoxImage.Information)
                Catch ex As Exception
                    MessageBox.Show("Error exporting: " & ex.Message, "Export Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim record As AttendanceRecord = TryCast(btn.DataContext, AttendanceRecord)
            If record Is Nothing Then Return

            Dim result = MessageBox.Show(
                $"Are you sure you want to delete the attendance record for {record.EmployeeName}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
            If result <> MessageBoxResult.Yes Then Return

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand("DELETE FROM attendance WHERE ID = @id", conn)
                        cmd.Parameters.AddWithValue("@id", record.ID)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
                AttendanceList.Remove(record)
                RefreshDisplay()
            Catch ex As Exception
                MessageBox.Show("Error deleting record: " & ex.Message, "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim record As AttendanceRecord = TryCast(btn.DataContext, AttendanceRecord)
            If record Is Nothing Then Return

            Dim editForm As New DPC.Components.Forms.AddAttendance()
            editForm.TxtTitle.Text = "Edit Attendance"
            editForm.TxtBtnAdd.Text = "Update"
            editForm.IconTitle.Kind = MaterialDesignThemes.Wpf.PackIconKind.SquareEditOutline
            editForm.TxtEmployee.Text = record.EmployeeName
            editForm.TxtNote.Text = record.Note
            editForm.TxtDateDisplay.Text = record.AttendanceDate
            editForm.TpStartTime.Text = record.TimeIn
            editForm.TpEndTime.Text = record.TimeOut

            Dim parsedDate As DateTime
            If DateTime.TryParse(record.AttendanceDate, parsedDate) Then
                editForm.SingleDatePicker.SelectedDate = parsedDate
            End If

            AddHandler editForm.OnAttendanceAdded,
                Sub(empName, attDate, tIn, tOut, noteVal)
                    Try
                        Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                            conn.Open()
                            Using cmd As New MySqlCommand(
                                "UPDATE attendance
                                    SET EmployeeName   = @emp,
                                        AttendanceDate = @date,
                                        TimeIn         = @tin,
                                        TimeOut        = @tout,
                                        Note           = @note
                                  WHERE ID = @id", conn)
                                cmd.Parameters.AddWithValue("@emp", empName)
                                cmd.Parameters.AddWithValue("@date", attDate)
                                cmd.Parameters.AddWithValue("@tin", tIn)
                                cmd.Parameters.AddWithValue("@tout", tOut)
                                cmd.Parameters.AddWithValue("@note", noteVal)
                                cmd.Parameters.AddWithValue("@id", record.ID)
                                cmd.ExecuteNonQuery()
                            End Using
                        End Using

                        Dim index = AttendanceList.IndexOf(record)
                        If index >= 0 Then
                            AttendanceList(index) = New AttendanceRecord With {
                                .ID = record.ID,
                                .EmployeeName = empName,
                                .AttendanceDate = attDate,
                                .TimeIn = tIn,
                                .TimeOut = tOut,
                                .Note = noteVal
                            }
                        End If
                        RefreshDisplay()

                    Catch ex As Exception
                        MessageBox.Show("Error updating record: " & ex.Message, "Database Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error)
                    End Try
                End Sub

            Dim parentWindow = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, editForm, "windowcenter", True, -50, 0, parentWindow)
        End Sub

    End Class

    Public Class AttendanceRecord
        Public Property ID As Integer
        Public Property EmployeeName As String
        Public Property AttendanceDate As String
        Public Property TimeIn As String
        Public Property TimeOut As String
        Public Property Note As String
    End Class
End Namespace