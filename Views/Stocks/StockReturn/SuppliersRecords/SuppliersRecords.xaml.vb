Imports System.ComponentModel
Imports System.Windows.Controls
Imports DPC.DPC.Data.Controllers
Imports System.Windows.Input
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.Stocks.StockReturn.SupplierRecords
    Public Class SuppliersRecords
        Inherits UserControl

        ' ViewModel for Date Range
        Public Property DateRangeVM As New CalendarController.MultiCalendar()

        ' Constructor
        Public Sub New()
            InitializeComponent()
            DataContext = DateRangeVM ' Bind DataContext to ViewModel
        End Sub

        ' Open Start Date Picker when clicking the text
        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            StartDatePicker.IsDropDownOpen = True
        End Sub

        ' Open End Date Picker when clicking the text
        Private Sub EndDate_Click(sender As Object, e As RoutedEventArgs)
            EndDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddNew.Click
            ViewLoader.DynamicView.NavigateToView("newsuppliers", Nothing)
        End Sub
    End Class
End Namespace