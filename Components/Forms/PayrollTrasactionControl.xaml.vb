Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Views.HRM.Employees.Payroll

' 1. Import the namespace where we just put the model!
Imports DPC.Views.HRM.Employees.Payroll

Namespace DPC.Components.Forms
    Public Class PayrollTransactionControl
        Inherits UserControl

        ' 2. Now this will perfectly recognize the model!
        Public Event OnTransactionAdded(newRecord As PayrollTxModel)
        Private _recordBeingEdited As PayrollTxModel = Nothing

        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
            SetupControllerReferences()
        End Sub

        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            Dim parent = TryCast(Me.Parent, ContentControl)
            If parent IsNot Nothing Then
                Dim container = TryCast(parent.Parent, Panel)
                If container IsNot Nothing Then container.Children.Remove(parent)
            Else
                Dim parentPopup = TryCast(Me.Parent, Popup)
                If parentPopup IsNot Nothing Then
                    parentPopup.IsOpen = False
                Else
                    Dim parentWindow = Window.GetWindow(Me)
                    If parentWindow IsNot Nothing Then parentWindow.Close()
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
            Dim minDate As DateTime = DateTime.Today

            ' If we are editing a transaction, check if the calendar already has an older date loaded
            If SingleDatePicker.SelectedDate.HasValue AndAlso SingleDatePicker.SelectedDate.Value < DateTime.Today Then
                minDate = SingleDatePicker.SelectedDate.Value
            End If

            SingleDatePicker.DisplayDateStart = minDate
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

        Private Sub BtnAdd_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedDate As String = "N/A"
            Dim calendarViewModel As CalendarController.SingleCalendar = TryCast(DateButton.DataContext, CalendarController.SingleCalendar)
            If calendarViewModel IsNot Nothing AndAlso calendarViewModel.FormattedDate IsNot Nothing Then
                selectedDate = calendarViewModel.FormattedDate
            End If

            ' 3. Create the record!
            Dim newRecord As New PayrollTxModel() With {
                .Date = selectedDate,
                .Employee = txtEmployee.Text,
                .Account = txtAccount.Text,
                .Debit = txtAmount.Text,
                .Credit = "0.00",
                .Method = txtMethod.Text,
                .Actions = "Edit / Delete"
            }

            RaiseEvent OnTransactionAdded(newRecord)
            BtnClose_Click(Nothing, Nothing)
        End Sub
        ' The Main Page calls this to fill the form with existing data
        Public Sub SetEditMode(record As PayrollTxModel)
            _recordBeingEdited = record

            ' 1. Fill the standard textboxes
            txtEmployee.Text = record.Employee
            txtAccount.Text = record.Account
            txtMethod.Text = record.Method

            ' Determine if it's a Debit or Credit and put it in the Amount box
            If Not String.IsNullOrWhiteSpace(record.Debit) AndAlso record.Debit <> "0.00" Then
                txtAmount.Text = record.Debit
            Else
                txtAmount.Text = record.Credit
            End If

            ' ==========================================================
            ' 2. RESTORE THE DATE!
            ' Convert the string (e.g., "Mar 22, 2026") back to a DateTime
            ' ==========================================================
            Dim parsedDate As DateTime
            If DateTime.TryParse(record.Date, parsedDate) Then
                ' Grab your specific calendar ViewModel
                Dim calendarViewModel As CalendarController.SingleCalendar = TryCast(DateButton.DataContext, CalendarController.SingleCalendar)

                If calendarViewModel IsNot Nothing Then
                    ' Set the date and force the UI button to refresh its text!
                    calendarViewModel.SelectedDate = parsedDate
                    BindingOperations.GetBindingExpression(DateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If

        End Sub

    End Class
End Namespace