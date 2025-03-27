Imports System.ComponentModel
Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components

Namespace DPC.Views.Stocks.StockReturn.SupplierRecords

    Public Class SuppliersRecords
        Inherits Window

        ' ViewModel for Date Range
        Public Property DateRangeVM As New CalendarController.MultiCalendar()

        ' Constructor
        Public Sub New()
            InitializeComponent()
            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

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

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs) Handles btnAddNew.Click
            Dim NewSupplierWindow As New Views.Stocks.Supplier.NewSuppliers.NewSuppliers()
            NewSupplierWindow.Show()

            ' Close the current window where this UserControl is being used
            Dim currentWindow As Window = Window.GetWindow(Me)
            currentWindow?.Close()
        End Sub
    End Class

    ' ViewModel for Date Range Picker

End Namespace
