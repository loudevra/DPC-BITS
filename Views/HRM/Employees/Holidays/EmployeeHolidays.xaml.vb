Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports Microsoft.Win32
Imports DPC.DPC.Data.Controllers
Imports DPC.Data.Controllers

Namespace DPC.Views.HRM.Employees.Holidays

    Public Class EmployeeHolidays
        Inherits UserControl

        ' Live list bound to the DataGrid
        Public Property HolidayList As New ObservableCollection(Of HolidayModel)()

        Public Sub New()
            InitializeComponent()
            dataGrid.ItemsSource = HolidayList
        End Sub

        ' ── Load from DB when the view is first shown ──────────────────────────
        Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            RefreshData()
        End Sub

        ' Public so AddHoliday form can call it after Insert / Update
        Public Sub RefreshData()
            HolidayController.LoadHolidays(HolidayList)
        End Sub

        ' ── Add New ────────────────────────────────────────────────────────────
        Private Sub AddHolidayControl(sender As Object, e As RoutedEventArgs)
            Dim addHolidayControl As New DPC.Components.Forms.AddHoliday()
            addHolidayControl.ParentHolidayList = HolidayList
            addHolidayControl.ParentView = Me          ' so it can call RefreshData()

            Dim parentWindow = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, addHolidayControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub

        ' ── Export to CSV ──────────────────────────────────────────────────────
        Private Sub ExportToExcel_Click(sender As Object, e As RoutedEventArgs)
            Try
                If HolidayList Is Nothing OrElse HolidayList.Count = 0 Then
                    MessageBox.Show("No data available to export.", "Export to Excel", MessageBoxButton.OK, MessageBoxImage.Information)
                    Return
                End If

                Dim saveFileDialog As New SaveFileDialog()
                saveFileDialog.Filter = "Excel CSV (*.csv)|*.csv"
                saveFileDialog.FileName = "Employee_Holidays_Export.csv"
                saveFileDialog.Title = "Save Employee Holidays"

                If saveFileDialog.ShowDialog() = True Then
                    Using writer As New StreamWriter(saveFileDialog.FileName)
                        writer.WriteLine("ID,From Date,To Date,Days,Note")

                        For Each holiday In HolidayList
                            Dim cleanNote As String = ""
                            If Not String.IsNullOrEmpty(holiday.Note) Then
                                cleanNote = $"""{holiday.Note.Replace("""", """""")}"""
                            End If
                            writer.WriteLine($"{holiday.ID},{holiday.FromDate},{holiday.ToDate},{holiday.Days},{cleanNote}")
                        Next
                    End Using

                    MessageBox.Show("Data successfully exported!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If
            Catch ex As Exception
                MessageBox.Show($"An error occurred while exporting: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' ── Search ─────────────────────────────────────────────────────────────
        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim searchText As String = txtSearch.Text.ToLower().Trim()

            If String.IsNullOrEmpty(searchText) Then
                dataGrid.ItemsSource = HolidayList
            Else
                Dim filteredList = HolidayList.Where(Function(h)
                                                         Return h.ID.ToString().Contains(searchText) OrElse
                                                                (h.Note IsNot Nothing AndAlso h.Note.ToLower().Contains(searchText)) OrElse
                                                                (h.FromDate IsNot Nothing AndAlso h.FromDate.ToLower().Contains(searchText))
                                                     End Function).ToList()
                dataGrid.ItemsSource = filteredList
            End If
        End Sub

        ' ── Delete ─────────────────────────────────────────────────────────────
        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            Dim item = TryCast(btn.CommandParameter, HolidayModel)

            If item IsNot Nothing Then
                Dim result = MessageBox.Show("Are you sure you want to delete this holiday?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question)
                If result = MessageBoxResult.Yes Then
                    ' Delete from DB first, then refresh
                    If HolidayController.DeleteHoliday(item.HolidayID) Then
                        RefreshData()
                    End If
                End If
            End If
        End Sub

        ' ── Edit ───────────────────────────────────────────────────────────────
        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            Dim item = TryCast(btn.CommandParameter, HolidayModel)

            If item IsNot Nothing Then
                Dim editForm As New DPC.Components.Forms.AddHoliday()
                editForm.ParentHolidayList = HolidayList
                editForm.ParentView = Me           ' so it can call RefreshData()
                editForm.PrepareForEdit(item)

                Dim parentWindow = Window.GetWindow(Me)
                PopupHelper.OpenPopupWithControl(sender, editForm, "windowcenter", True, -50, 0, parentWindow)
            End If
        End Sub

    End Class

    ' ── Model ─────────────────────────────────────────────────────────────────
    Public Class HolidayModel
        Public Property ID As Integer          ' display row number (#)
        Public Property HolidayID As Integer   ' actual DB primary key
        Public Property FromDate As String
        Public Property ToDate As String
        Public Property Days As Integer
        Public Property Note As String
        Public Property Action As String
    End Class

End Namespace