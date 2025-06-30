Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers


Namespace DPC.Components.Forms
    Public Class PayrollTransactionControl
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
            SetupControllerReferences() ' Ensures viewmodel is connected
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
