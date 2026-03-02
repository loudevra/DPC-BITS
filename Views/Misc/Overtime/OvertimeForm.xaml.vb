Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Windows.Controls

Namespace DPC.Views.Misc.CashAdvance

    Public Class OvertimeRequestForm
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        ' ==========================================
        ' 1. TIME CALCULATION LOGIC (New Feature)
        ' ==========================================
        ' This runs automatically whenever you type in Start Time or End Time
        Private Sub Time_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtStartTime.TextChanged, txtEndTime.TextChanged
            CalculateHours()
        End Sub

        Private Sub CalculateHours()
            Dim startT As DateTime
            Dim endT As DateTime

            ' 1. Check if both text boxes have valid time formats
            If DateTime.TryParse(txtStartTime.Text, startT) AndAlso DateTime.TryParse(txtEndTime.Text, endT) Then

                ' 2. Calculate the difference
                Dim duration As TimeSpan = endT - startT

                ' 3. Handle Overnight Shifts (e.g., 10:00 PM to 2:00 AM)
                ' If End Time is earlier than Start Time, we assume it's the next day.
                If duration.TotalMinutes < 0 Then
                    duration = duration.Add(TimeSpan.FromDays(1))
                End If

                ' 4. Display the result (Formatted to 2 decimal places, e.g., "4.00")
                txtHours.Text = duration.TotalHours.ToString("F2")
            Else
                ' If inputs are invalid or empty, clear the hours box
                txtHours.Text = ""
            End If
        End Sub

        ' ==========================================
        ' 2. AUTO-FILL DATE LOGIC
        ' ==========================================
        Private Sub MainDate_Changed(sender As Object, e As SelectionChangedEventArgs) Handles CashAdvanceDatePicker.SelectedDateChanged
            Dim mainDate As DateTime? = CashAdvanceDatePicker.SelectedDate

            If mainDate.HasValue Then
                ' A. Fill Middle Date
                If dtOvertimeDate IsNot Nothing Then dtOvertimeDate.SelectedDate = mainDate

                ' B. Fill Request Date
                If RequestDate IsNot Nothing Then RequestDate.SelectedDate = mainDate

                ' C. Approval Date is EXCLUDED
            End If
        End Sub

        ' ==========================================
        ' 3. CLICK HANDLERS
        ' ==========================================
        Private Sub CashAdvanceDate_Click(sender As Object, e As RoutedEventArgs)
            CashAdvanceDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub OvertimeDate_Click(sender As Object, e As RoutedEventArgs)
            dtOvertimeDate.IsDropDownOpen = True
        End Sub

        Private Sub RequestDate_Click(sender As Object, e As RoutedEventArgs)
            If RequestDate IsNot Nothing Then RequestDate.IsDropDownOpen = True
        End Sub

        Private Sub ApprovalDate_Click(sender As Object, e As RoutedEventArgs)
            If ApprovalDate IsNot Nothing Then ApprovalDate.IsDropDownOpen = True
        End Sub

        ' ==========================================
        ' 4. SUBMIT BUTTON
        ' ==========================================
        Private Sub BtnSubmit_Click(sender As Object, e As RoutedEventArgs) Handles BtnSubmit.Click
            Dim rawDate As DateTime? = dtOvertimeDate.SelectedDate
            Dim sTime As String = txtStartTime.Text
            Dim eTime As String = txtEndTime.Text
            Dim totalHours As String = txtHours.Text

            ' Validation
            If rawDate Is Nothing Then
                MessageBox.Show("Please select a date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            If String.IsNullOrWhiteSpace(totalHours) Then
                MessageBox.Show("Please enter valid Start and End times to calculate hours.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            MessageBox.Show($"Request Submitted!" & vbCrLf &
                            $"Date: {rawDate.Value.ToString("MMMM dd, yyyy")}" & vbCrLf &
                            $"Shift: {sTime} - {eTime}" & vbCrLf &
                            $"Total: {totalHours} Hours", "Success")
        End Sub

    End Class
End Namespace