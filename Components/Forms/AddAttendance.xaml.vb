Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports DPC.DPC.Data.Controllers

Namespace DPC.Components.Forms
    Public Class AddAttendance
        Inherits UserControl

        ' Event to pass data back to the main screen
        Public Event OnAttendanceAdded(employeeName As String, attendanceDate As String, timeIn As String, timeOut As String, note As String)

        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
            SetupControllerReferences() ' Ensures viewmodel is connected
        End Sub

        ' Add Button Click + Validation + Notifications
        Private Sub BtnAdd_Click(sender As Object, e As RoutedEventArgs)
            Dim empName As String = TxtEmployee.Text.Trim()
            Dim selectedDate As String = If(SingleDatePicker.SelectedDate.HasValue, SingleDatePicker.SelectedDate.Value.ToString("MM-dd-yyyy"), "")
            Dim noteVal As String = TxtNote.Text.Trim()

            ' Extract Time from the visual TimePickers
            Dim tIn As String = If(TpStartTime.SelectedTime.HasValue, TpStartTime.SelectedTime.Value.ToString("hh:mm tt"), "")
            Dim tOut As String = If(TpEndTime.SelectedTime.HasValue, TpEndTime.SelectedTime.Value.ToString("hh:mm tt"), "")

            ' Check if any required field is blank
            If String.IsNullOrWhiteSpace(empName) OrElse
               String.IsNullOrWhiteSpace(selectedDate) OrElse
               String.IsNullOrWhiteSpace(tIn) OrElse
               String.IsNullOrWhiteSpace(tOut) Then

                ' Validation warning popup
                MessageBox.Show("Please fill in all required data fields (Employee, Date, Time In, and Time Out).",
                                "Missing Data",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning)
                Return
            End If

            ' Send data to the datagrid
            RaiseEvent OnAttendanceAdded(empName, selectedDate, tIn, tOut, noteVal)

            ' Success Notification
            MessageBox.Show("Attendance record successfully added!",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information)

            ' Close the popup
            BtnClose_Click(sender, e)
        End Sub

        ' Close Button Handler
        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            Dim parent = TryCast(Me.Parent, ContentControl)
            If parent IsNot Nothing Then
                Dim container = TryCast(parent.Parent, Panel)
                If container IsNot Nothing Then
                    container.Children.Remove(parent)
                End If
            Else
                Dim parentPopup = TryCast(Me.Parent, Popup)
                If parentPopup IsNot Nothing Then
                    parentPopup.IsOpen = False
                Else
                    Dim parentWindow = Window.GetWindow(Me)
                    If parentWindow IsNot Nothing Then
                        parentWindow.Close()
                    End If
                End If
            End If
        End Sub

        Private Sub SetupControllerReferences()
            Dim calendarViewModel As New CalendarController.SingleCalendar()
            calendarViewModel.SelectedDate = Nothing

            SingleDatePicker.DataContext = calendarViewModel
            DateButton.DataContext = calendarViewModel
        End Sub

        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            SingleDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub SingleDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles SingleDatePicker.SelectedDateChanged
            Dim datePicker As DatePicker = TryCast(sender, DatePicker)
            If datePicker IsNot Nothing AndAlso datePicker.DataContext IsNot Nothing Then
                Dim calendarViewModel As CalendarController.SingleCalendar = TryCast(datePicker.DataContext, CalendarController.SingleCalendar)
                If calendarViewModel IsNot Nothing Then
                    calendarViewModel.SelectedDate = datePicker.SelectedDate
                    BindingOperations.GetBindingExpression(DateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If
        End Sub
    End Class
End Namespace