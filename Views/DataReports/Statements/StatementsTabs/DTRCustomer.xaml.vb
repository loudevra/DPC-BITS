Imports DPC.DPC.Data.Controllers


Namespace DPC.Views.DataReports.Statements.StatementsTabs
    Partial Public Class DTRCustomer
        Inherits UserControl

        Private fromDateViewModel As New CalendarController.SingleCalendar()
        Private toDateViewModel As New CalendarController.SingleCalendar()

        Public Sub New()
            InitializeComponent()
            SetupControllerReferences()
        End Sub

        ' Bind each picker and button to its own SingleCalendar ViewModel
        Public Sub SetupControllerReferences()
            fromDateViewModel.SelectedDate = Nothing
            toDateViewModel.SelectedDate = Nothing

            FromDatePicker.DataContext = fromDateViewModel
            FromDateButton.DataContext = fromDateViewModel

            ToDatePicker.DataContext = toDateViewModel
            ToDateButton.DataContext = toDateViewModel
        End Sub

        ' Open From date calendar
        Private Sub FromDateButton_Click(sender As Object, e As RoutedEventArgs)
            FromDatePicker.IsDropDownOpen = True
        End Sub

        ' Open To date calendar
        Private Sub ToDateButton_Click(sender As Object, e As RoutedEventArgs)
            ToDatePicker.IsDropDownOpen = True
        End Sub

        ' Handle From date selection
        Private Sub FromDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles FromDatePicker.SelectedDateChanged
            Dim datePicker As DatePicker = TryCast(sender, DatePicker)
            If datePicker IsNot Nothing AndAlso datePicker.DataContext IsNot Nothing Then
                Dim vm As CalendarController.SingleCalendar = TryCast(datePicker.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = datePicker.SelectedDate
                    BindingOperations.GetBindingExpression(FromDateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If
        End Sub

        ' Handle To date selection
        Private Sub ToDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles ToDatePicker.SelectedDateChanged
            Dim datePicker As DatePicker = TryCast(sender, DatePicker)
            If datePicker IsNot Nothing AndAlso datePicker.DataContext IsNot Nothing Then
                Dim vm As CalendarController.SingleCalendar = TryCast(datePicker.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = datePicker.SelectedDate
                    BindingOperations.GetBindingExpression(ToDateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If
        End Sub

    End Class
End Namespace
