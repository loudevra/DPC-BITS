Imports System.ComponentModel
Imports System.Data
Imports System.Linq
Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model

Namespace DPC.Views.Stocks.PurchaseOrder.ManageOrders
    Public Class ManageOrders
        Inherits UserControl

        ' ViewModel for Date Range
        Public Property DateRangeVM As New DateRangeViewModel()
        Private _typingTimer As DispatcherTimer

        ' Constructor
        Public Sub New()
            InitializeComponent()
            DataContext = DateRangeVM ' Bind DataContext to ViewModel

            ' Initialize typing timer for search delay
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(250)
            }

            GetItemsFromDB()

            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick
            AddHandler cmbLimit.SelectionChanged, AddressOf GetItemsFromDB

            ' Add handlers for date pickers to trigger search when dates change
            AddHandler StartDatePicker.SelectedDateChanged, AddressOf OnDateRangeChanged
            AddHandler EndDatePicker.SelectedDateChanged, AddressOf OnDateRangeChanged
        End Sub

        Private Sub DataGrid_CellClick(sender As Object, e As MouseButtonEventArgs)
            Dim depObj As DependencyObject = TryCast(e.OriginalSource, DependencyObject)

            Dim cell = TryCast(depObj, TextBlock)

            If TypeOf cell Is TextBlock Then
                ' Show popup near the clicked cell
                PopupText.Text = cell.Text
                CellValuePopup.PlacementTarget = sender
                CellValuePopup.IsOpen = True
            End If

        End Sub


        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            ' Check if DataGrid has data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Use the ExcelExporter helper with column exclusions
            ExcelExporter.ExportDataGridToExcel(dataGrid, "PurchaseOrdersExport", "PO List")

        End Sub

        Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
            ' Stop the timer
            _typingTimer.Stop()

            GetItemsFromDB()
        End Sub

        ' Event handler for date range changes
        Private Sub OnDateRangeChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Only trigger search if both dates are selected
            If DateRangeVM.StartDate.HasValue AndAlso DateRangeVM.EndDate.HasValue Then
                GetItemsFromDB()
            End If
        End Sub

        ' Clear date range filter
        Private Sub ClearDateRange_Click(sender As Object, e As RoutedEventArgs)
            ' Reset dates to Nothing to disable filtering
            DateRangeVM.StartDate = Nothing
            DateRangeVM.EndDate = Nothing

            ' Refresh the data to show all records
            GetItemsFromDB()
        End Sub

        Private Sub GetItemsFromDB()
            ' Get all orders first
            Dim allOrders = PurchaseOrderController.GetOrders(CInt(cmbLimit.Text))

            ' Apply search filter if text is provided
            If Not String.IsNullOrWhiteSpace(SearchText.Text) Then
                allOrders = PurchaseOrderController.GetOrdersSearch(SearchText.Text, CInt(cmbLimit.Text))
            End If

            ' Apply date range filter only if both dates are selected
            If DateRangeVM.StartDate.HasValue AndAlso DateRangeVM.EndDate.HasValue Then
                Dim filteredOrders = allOrders.Where(Function(order)
                                                         Dim orderDate As Date
                                                         ' Parse the date from "MMMM d, yyyy" format
                                                         If Date.TryParseExact(order.OrderDate, "MMMM d, yyyy",
                                         System.Globalization.CultureInfo.InvariantCulture,
                                         Globalization.DateTimeStyles.None, orderDate) Then
                                                             Return orderDate >= DateRangeVM.StartDate.Value.Date AndAlso
                               orderDate <= DateRangeVM.EndDate.Value.Date
                                                         End If
                                                         Return False
                                                     End Function).ToList()

                dataGrid.ItemsSource = Nothing
                dataGrid.ItemsSource = New ObservableCollection(Of PurchaseOrderModel)(filteredOrders)
            Else
                ' No date filter applied - show all results
                dataGrid.ItemsSource = Nothing
                dataGrid.ItemsSource = allOrders
            End If
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

        Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Reset the timer
            _typingTimer.Stop()

            ' Start the timer
            _typingTimer.Start()

        End Sub
    End Class

    ' ViewModel for Date Range Picker
    Public Class DateRangeViewModel
        Implements INotifyPropertyChanged

        Private _startDate As Date? = Nothing ' No default filter
        Private _endDate As Date? = Nothing ' No default filter

        ' Start Date
        Public Property StartDate As Date?
            Get
                Return _startDate
            End Get
            Set(value As Date?)
                _startDate = value
                OnPropertyChanged(NameOf(StartDate))
            End Set
        End Property

        ' End Date
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