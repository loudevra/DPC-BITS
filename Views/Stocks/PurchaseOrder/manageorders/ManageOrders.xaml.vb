Imports System.ComponentModel
Imports System.Data
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers

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

        Private Sub GetItemsFromDB()
            If String.IsNullOrWhiteSpace(SearchText.Text) Then
                dataGrid.ItemsSource = Nothing
                dataGrid.ItemsSource = PurchaseOrderController.GetOrders(CInt(cmbLimit.Text))
            Else
                dataGrid.ItemsSource = Nothing
                dataGrid.ItemsSource = PurchaseOrderController.GetOrdersSearch(SearchText.Text, CInt(cmbLimit.Text))
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