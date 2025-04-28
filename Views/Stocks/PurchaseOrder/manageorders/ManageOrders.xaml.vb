Imports System.ComponentModel
Imports System.Windows
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.Stocks.PurchaseOrder.ManageOrders

    Public Class ManageOrders
        Inherits Window

        ' ViewModel for Date Range
        Public Property DateRangeVM As New DateRangeViewModel()

        ' Constructor
        Public Sub New()
            InitializeComponent()
            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNavBar As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNavBar

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
            ViewLoader.DynamicView.NavigateToView("neworder", Me)
        End Sub
    End Class

    ' ViewModel for Date Range Picker
    Public Class DateRangeViewModel
        Implements INotifyPropertyChanged

        Private _startDate As Date? = Date.Now
        Private _endDate As Date? = Date.Now.AddDays(1) ' Tomorrow

        ' Start Date (Today)
        Public Property StartDate As Date?
            Get
                Return _startDate
            End Get
            Set(value As Date?)
                _startDate = value
                OnPropertyChanged(NameOf(StartDate))
            End Set
        End Property

        ' End Date (Tomorrow)
        Public Property EndDate As Date?
            Get
                Return _endDate
            End Get
            Set(value As Date?)
                _endDate = value
                OnPropertyChanged(NameOf(EndDate))
            End Set
        End Property

        ' Event to handle property changes
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Overridable Sub OnPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

    End Class

End Namespace
