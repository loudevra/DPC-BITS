Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Stocks.PurchaseOrder.Delivery
    Public Class ManageDeliveryReceipt
        Inherits UserControl

        ' ViewModels for the custom DatePicker behavior
        Private startDateViewModel As New CalendarController.SingleCalendar()
        Private dueDateViewModel As New CalendarController.SingleCalendar()
        Private _typingTimer As DispatcherTimer

        Private _isInitialized As Boolean = False
        Private _currentDRNumber As String

        Public Sub New()
            InitializeComponent()
            SetupDatePickers()

            ' Initialize the search delay timer
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(250)
            }
            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick

            ' Load data when the control is ready
            AddHandler Me.Loaded, AddressOf UserControl_Loaded
        End Sub

        Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
            If Not _isInitialized Then
                ' Wire up events once InitializeComponent is finished
                AddHandler SearchText.TextChanged, AddressOf SearchText_TextChanged
                AddHandler cmbLimit.SelectionChanged, AddressOf ComboBoxLimit_SelectionChanged

                LoadData()
                _isInitialized = True
            End If
        End Sub

        ''' <summary>
        ''' Loads Delivery Receipt data into the DataGrid based on the selected limit
        ''' </summary>
        Public Sub LoadData()
            Try
                ' Extract limit from ComboBoxItem
                Dim limit As Integer = 10
                If cmbLimit.SelectedItem IsNot Nothing Then
                    limit = Convert.ToInt32(CType(cmbLimit.SelectedItem, ComboBoxItem).Content)
                End If

                ' Call the DeliveryReceiptController to fetch records
                Dim receipts = DeliveryReceiptController.GetDeliveryReceipts(limit)

                FormatDeliveryReceiptsForDisplay(receipts)
                dataGrid.ItemsSource = receipts
            Catch ex As Exception
                MessageBox.Show($"Error loading delivery receipts: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Formats the model dates into user-friendly strings for the UI
        ''' </summary>
        Private Shared Sub FormatDeliveryReceiptsForDisplay(receipts As ObservableCollection(Of DeliveryReceiptModel))
            Try
                For Each receipt In receipts
                    ' Format Delivery Receipt Date
                    If Not String.IsNullOrEmpty(receipt.DRDate) AndAlso receipt.DRDate <> "-" Then
                        Dim dt As DateTime
                        If DateTime.TryParse(receipt.DRDate, dt) Then
                            receipt.DRDate = dt.ToString("MMM d, yyyy")
                        End If
                    End If

                    ' Format the system timestamp if a display property is available
                    If receipt.DateAdded <> DateTime.MinValue Then
                        receipt.DateAddedDisplay = receipt.DateAdded.ToString("MMM d, yyyy")
                    End If
                Next
            Catch ex As Exception
                Debug.WriteLine($"Error formatting delivery receipts: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Logic for the "Edit" button inside the DataGrid rows
        ''' </summary>
        Private Sub OpenEditDeliveryReceipt(sender As Object, e As RoutedEventArgs)
            Dim receipt As DeliveryReceiptModel = TryCast(dataGrid.SelectedItem, DeliveryReceiptModel)
            DeliveryDetails.ClearDeliveryDetails()

            If receipt IsNot Nothing Then
                DeliveryDetails.DRNumber = receipt.DRNumber
                DeliveryDetails.DRReferenceInvoice = receipt.ReferenceInvoice
                DeliveryDetails.DRClientName = receipt.ClientName
                DeliveryDetails.DRClientDetails = receipt.ClientDetails
                DeliveryDetails.DRDate = receipt.DRDate
                DeliveryDetails.DRShippingMethod = receipt.ShippingMethod
                DeliveryDetails.DRDeliveryNotes = receipt.DeliveryNotes
                DeliveryDetails.DRApprovedBy = receipt.ApprovedBy
                DeliveryDetails.DRPaymentTerm = receipt.PaymentTerm

                ViewLoader.DynamicView.NavigateToView("newdelivery", Me)
            End If
        End Sub

        ''' <summary>
        ''' Logic for the "Delete" button inside the DataGrid rows
        ''' </summary>
        Private Sub DeleteDeliveryReceipt(sender As Object, e As RoutedEventArgs)
            Dim receipt As DeliveryReceiptModel = TryCast(dataGrid.SelectedItem, DeliveryReceiptModel)
            If receipt Is Nothing Then Return

            Dim result = MessageBox.Show($"Are you sure you want to delete Delivery Receipt {receipt.DRNumber}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)

            If result = MessageBoxResult.Yes Then
                Try
                    Dim query As String = "DELETE FROM deliveryreceipts WHERE DRNumber = @drNumber"
                    Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                        conn.Open()
                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@drNumber", receipt.DRNumber)
                            cmd.ExecuteNonQuery()
                        End Using
                    End Using

                    MessageBox.Show("Delivery receipt deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    LoadData()
                Catch ex As Exception
                    MessageBox.Show("Error deleting receipt: " & ex.Message)
                End Try
            End If
        End Sub

        ' Search Logic with Typing Delay
        Private Sub SearchText_TextChanged(sender As Object, e As TextChangedEventArgs)
            _typingTimer.Stop()
            _typingTimer.Start()
        End Sub

        Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
            _typingTimer.Stop()
            PerformSearch()
        End Sub

        Private Sub PerformSearch()
            Try
                Dim limit As Integer = 10
                If cmbLimit.SelectedItem IsNot Nothing Then
                    limit = Convert.ToInt32(CType(cmbLimit.SelectedItem, ComboBoxItem).Content)
                End If

                ' Optional: Add date filter parameters if your controller supports it
                Dim results = DeliveryReceiptController.SearchDeliveryReceipts(SearchText.Text.Trim(), limit)
                FormatDeliveryReceiptsForDisplay(results)
                dataGrid.ItemsSource = results
            Catch ex As Exception
                Debug.WriteLine("Search error: " & ex.Message)
            End Try
        End Sub

        ' XAML Bound Event: Cell Click Popup
        Private Sub DataGrid_CellClick(sender As Object, e As MouseButtonEventArgs)
            Dim depObj As DependencyObject = TryCast(e.OriginalSource, DependencyObject)
            Dim cell = TryCast(depObj, TextBlock)

            If cell IsNot Nothing Then
                PopupText.Text = cell.Text
                CellValuePopup.PlacementTarget = TryCast(sender, UIElement)
                CellValuePopup.IsOpen = True
            End If
        End Sub

        ' DatePicker Handlers
        Private Sub StartDateButton_Click(sender As Object, e As RoutedEventArgs)
            StartDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub DueDateButton_Click(sender As Object, e As RoutedEventArgs)
            DueDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub StartDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            startDateViewModel.SelectedDate = StartDatePicker.SelectedDate
            PerformSearch() ' Re-run search on date change
        End Sub

        Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            dueDateViewModel.SelectedDate = DueDatePicker.SelectedDate
            PerformSearch() ' Re-run search on date change
        End Sub

        ' Excel Export
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            If dataGrid.Items.Count > 0 Then
                ExcelExporter.ExportDataGridToExcel(dataGrid, "Delivery_Receipts", "Delivery Receipt Report")
            End If
        End Sub

        Private Sub NavigateToNewDelivery(sender As Object, e As RoutedEventArgs)
            ' Redirect to your Add Delivery Receipt View
            ViewLoader.DynamicView.NavigateToView("newdelivery", Me)
        End Sub

        Private Sub ComboBoxLimit_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If _isInitialized Then LoadData()
        End Sub

        Public Sub SetupDatePickers()
            StartDatePicker.DataContext = startDateViewModel
            StartDateButton.DataContext = startDateViewModel
            DueDatePicker.DataContext = dueDateViewModel
            DueDateButton.DataContext = dueDateViewModel
        End Sub
    End Class
End Namespace