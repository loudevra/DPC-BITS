Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace DPC.Components.Forms
    Public Class AddAttendance
        Inherits UserControl

        ' The event that sends data back to the DataGrid
        Public Event OnAttendanceAdded(employeeName As String, attendanceDate As String, timeIn As String, timeOut As String, note As String)

        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
        End Sub
        ' The Click event for the custom Date Button (With Past-Date Restriction)
        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            Dim minDate As DateTime = DateTime.Today

            ' If we are editing a record, check if the text box already holds a past date
            Dim existingDate As DateTime
            If DateTime.TryParse(TxtDateDisplay.Text, existingDate) AndAlso existingDate < DateTime.Today Then
                minDate = existingDate
            End If

            SingleDatePicker.DisplayDateStart = minDate
            SingleDatePicker.IsDropDownOpen = True
        End Sub

        ' Automatically updates the visual text when you pick a date from the calendar
        Private Sub SingleDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles SingleDatePicker.SelectedDateChanged
            If SingleDatePicker.SelectedDate.HasValue Then
                TxtDateDisplay.Text = SingleDatePicker.SelectedDate.Value.ToString("MM-dd-yyyy")
            End If
        End Sub

        ' The main Add/Update Button
        Private Sub BtnAdd_Click(sender As Object, e As RoutedEventArgs)
            Dim empName As String = TxtEmployee.Text.Trim()

            ' 1. Grab the Date text exactly as it appears on the screen
            Dim selectedDate As String = TxtDateDisplay.Text.Trim()
            If selectedDate = "Select a date" Then selectedDate = ""

            ' 2. Grab the Time text exactly as it appears on the screen
            Dim tIn As String = TpStartTime.Text.Trim()
            Dim tOut As String = TpEndTime.Text.Trim()
            Dim noteVal As String = TxtNote.Text.Trim()

            ' Validation check
            If String.IsNullOrWhiteSpace(empName) OrElse
               String.IsNullOrWhiteSpace(selectedDate) OrElse
               String.IsNullOrWhiteSpace(tIn) OrElse
               String.IsNullOrWhiteSpace(tOut) Then

                MessageBox.Show("Please fill in all required data fields (Employee, Date, Time In, and Time Out).",
                                "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Format times perfectly IF the picker recognized them, otherwise just use the text we grabbed above
            If TpStartTime.SelectedTime.HasValue Then tIn = TpStartTime.SelectedTime.Value.ToString("hh:mm tt")
            If TpEndTime.SelectedTime.HasValue Then tOut = TpEndTime.SelectedTime.Value.ToString("hh:mm tt")

            ' Send the data out
            RaiseEvent OnAttendanceAdded(empName, selectedDate, tIn, tOut, noteVal)

            MessageBox.Show("Attendance record successfully saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            ' Close popup
            BtnClose_Click(sender, e)
        End Sub

        ' Closes the form safely
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

    End Class
End Namespace