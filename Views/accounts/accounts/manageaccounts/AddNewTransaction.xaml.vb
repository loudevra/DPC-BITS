
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models


Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Public Class AddNewTransaction
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            SetupControllerReferences() ' Ensures viewmodel is connected
            LoadData()
        End Sub

        Private Sub LoadData()
            Dim _accountDetails = AccountController.GetAllAccountsAsKVP()

            cmbAccounts.ItemsSource = _accountDetails
            cmbAccounts.DisplayMemberPath = "Value"
            cmbAccounts.SelectedValuePath = "Key"

        End Sub


        Private Sub InsertData()
            Dim _transaction As New DPC.Data.Models.Transaction With {
                .Code = Code.Text,
                .Contact = Contact.Text,
                .AccountID = cmbAccounts.SelectedValue.ToString(),
                .Amount = Amount.Text,
                .Category = cmbCategory.Text,
                .Method = cmbMethod.Text,
                .Note = Note.Text,
                .TransactionDate = SingleDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                .TransactionTo = If(toggleCustomerSupplier.IsChecked, "Supplier", "Customer"),
                .Type = cmbType.Text
            }

            If AccountController.InsertTransaction(_transaction) Then
                MessageBox.Show("Transaction added successfully")
                ClearFields()
            End If
        End Sub

        Private Sub ClearFields()
            Code.Text = Nothing
            Contact.Text = Nothing
            cmbAccounts.Text = Nothing
            Amount.Text = Nothing
            cmbCategory.Text = Nothing
            cmbMethod.Text = Nothing
            Note.Text = Nothing
            SingleDatePicker.Text = Nothing
            cmbType.Text = Nothing
        End Sub

        ' Setup DataContext and ViewModel bindings
        Private Sub SetupControllerReferences()
            Dim calendarViewModel As New CalendarController.SingleCalendar()
            calendarViewModel.SelectedDate = Nothing

            ' Set DataContext for the DatePicker and Button
            SingleDatePicker.DataContext = calendarViewModel
            DateButton.DataContext = calendarViewModel
        End Sub

        ' Open the calendar popup
        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            SingleDatePicker.IsDropDownOpen = True
        End Sub

        ' Respond to date selection and update the view model
        Private Sub SingleDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles SingleDatePicker.SelectedDateChanged
            Dim datePicker As DatePicker = TryCast(sender, DatePicker)
            If datePicker IsNot Nothing AndAlso datePicker.DataContext IsNot Nothing Then
                Dim calendarViewModel As CalendarController.SingleCalendar = TryCast(datePicker.DataContext, CalendarController.SingleCalendar)
                If calendarViewModel IsNot Nothing Then
                    calendarViewModel.SelectedDate = datePicker.SelectedDate

                    ' Optional: Force UI to refresh FormattedDate if needed
                    BindingOperations.GetBindingExpression(DateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If
        End Sub

    End Class
End Namespace
