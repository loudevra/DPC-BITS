Imports System.Data
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel

Namespace DPC.Views.Sales.Quotes
    Public Class Quote
        ' Add SingleCalendar ViewModels for both pickers
        Private startDateViewModel As New CalendarController.SingleCalendar()
        Private dueDateViewModel As New CalendarController.SingleCalendar()
        Private _typingTimer As DispatcherTimer

        Private _isInitialized As Boolean = False
        Private _dataTable As DataTable
        Private _QuoteNumber As String

        Public Sub New()
            InitializeComponent()
            SetupDatePickers()

            ' Set up the TextChanged event immediately
            AddHandler SearchText.TextChanged, AddressOf SearchText_TextChanged
            AddHandler cmbLimit.SelectionChanged, AddressOf ComboBoxLimit_SelectionChanged

            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(250)
            }

            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick

            ' Load data after initialization is complete
            AddHandler Me.Loaded, AddressOf UserControl_Loaded
        End Sub


        Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
            If Not _isInitialized Then
                LoadData()
                _isInitialized = True
            End If
        End Sub

        Private Shared Function CalculateValidityDate(validityOption As String, quoteDate As DateTime) As DateTime
            Try
                Dim selection = validityOption.Trim().ToLower()

                Select Case selection
                    Case "48 hours"
                        Return quoteDate.AddHours(48)
                    Case "1 week"
                        Return quoteDate.AddDays(7)
                    Case "2 weeks"
                        Return quoteDate.AddDays(14)
                    Case "3 weeks"
                        Return quoteDate.AddDays(21)
                    Case "1 month"
                        Return quoteDate.AddMonths(1)
                    Case "2 months"
                        Return quoteDate.AddMonths(2)
                    Case "6 months"
                        Return quoteDate.AddMonths(6)
                    Case "1 year"
                        Return quoteDate.AddYears(1)
                    Case Else
                        ' Default to 48 hours if unknown
                        Return quoteDate.AddHours(48)
                End Select

            Catch ex As Exception
                Debug.WriteLine($"Error calculating validity date: {ex.Message}")
                Return quoteDate.AddHours(48)
            End Try
        End Function

        ''' Loads all quotes from the database
        Public Sub LoadData()
            Try
                Dim limit As Integer = Convert.ToInt32(cmbLimit.Text)
                Dim quotes = QuotesController.GetQuotes(limit)

                ' Format quotes for display
                FormatQuotesForDisplay(quotes)

                dataGrid.ItemsSource = quotes
            Catch ex As Exception
                MessageBox.Show($"Error loading data: {ex.Message}")
            End Try
        End Sub


        ''' Handles combo box selection change for display limit
        Private Sub ComboBoxLimit_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If Not _isInitialized Then Return
            LoadData()
        End Sub

        Private Sub OpenEditQuote(sender As Object, e As RoutedEventArgs)
            Dim quote As QuotesModel = TryCast(dataGrid.SelectedItem, QuotesModel)

            If quote IsNot Nothing Then
                Dim cacheModule = GetCacheModule()
                ' Store the quote data in cache
                cacheModule.QuoteNumber = quote.QuoteNumber
                cacheModule.ClientID = quote.ClientID
                cacheModule.ClientName = quote.ClientName
                cacheModule.WarehouseID = quote.WarehouseID
                cacheModule.WarehouseName = quote.WarehouseName
                cacheModule.QuoteDate = quote.QuoteDate
                cacheModule.Validity = quote.Validity
                cacheModule.QuoteNote = quote.QuoteNote
                cacheModule.OrderItems = quote.OrderItems

                ' Navigate to EditQuote
                ViewLoader.DynamicView.NavigateToView("editquote", Me)

            End If
        End Sub

        ''' Helper function to get or create cache module
        Private Function GetCacheModule() As QuoteCacheData
            ' Return a shared/global cache object - adjust based on your app structure
            If Not Application.Current.Properties.Contains("QuoteCache") Then
                Application.Current.Properties("QuoteCache") = New QuoteCacheData()
            End If
            Return DirectCast(Application.Current.Properties("QuoteCache"), QuoteCacheData)
        End Function


        Private Sub DeleteQuote(sender As Object, e As RoutedEventArgs)
            Dim quote As QuotesModel = TryCast(dataGrid.SelectedItem, QuotesModel)

            If quote Is Nothing Then
                MessageBox.Show("Please select a Quote to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            _QuoteNumber = quote.QuoteNumber

            ' Show confirmation dialog
            Dim result = MessageBox.Show("Are you sure you want to delete this quote?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question)

            If result = MessageBoxResult.Yes Then
                DeleteQuoteConfirmation_Closed()
            End If
        End Sub


        Private Sub DeleteQuoteConfirmation_Closed()
            Dim query As String = "DELETE FROM quotes WHERE QuoteNumber = @quoteNumber"
            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString
            Try
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@quoteNumber", _QuoteNumber)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                MessageBox.Show("Quote deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                dataGrid.ItemsSource = Nothing
                LoadData()
            Catch ex As Exception
                MessageBox.Show("Error deleting quote: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub


        ''' Performs the search based on current search text
        Private Sub PerformSearch()
            Try
                Dim searchTextValue As String = SearchText.Text.Trim()
                Dim limit As Integer = Convert.ToInt32(cmbLimit.Text)

                Dim quotes As ObservableCollection(Of QuotesModel)

                If String.IsNullOrWhiteSpace(searchTextValue) Then
                    quotes = QuotesController.GetQuotes(limit)
                Else
                    quotes = QuotesController.SearchQuotes(searchTextValue, limit)
                End If

                ' Format quotes for display
                FormatQuotesForDisplay(quotes)

                dataGrid.ItemsSource = quotes

            Catch ex As Exception
                MessageBox.Show($"Error searching: {ex.Message}")
            End Try
        End Sub

        ''' Updates the warehouse information in the database for an existing quote
        Public Shared Function UpdateQuoteWarehouse(quoteNumber As String, warehouseID As Integer, warehouseName As String) As Boolean
            Try
                Dim query As String = "UPDATE quotes SET WarehouseID = @warehouseID, WarehouseName = @warehouseName, UpdatedAt = @updatedAt WHERE QuoteNumber = @quoteNumber"
                Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@warehouseID", warehouseID)
                        cmd.Parameters.AddWithValue("@warehouseName", warehouseName)
                        cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@quoteNumber", quoteNumber)
                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                        If rowsAffected > 0 Then
                            MessageBox.Show("Warehouse updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                            Return True
                        Else
                            MessageBox.Show("Quote not found or warehouse not updated.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                            Return False
                        End If
                    End Using
                End Using

            Catch ex As MySqlException
                MessageBox.Show("Database error: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Debug.WriteLine("Database error in UpdateQuoteWarehouse: " & ex.Message)
                Return False

            Catch ex As Exception
                MessageBox.Show("Error updating warehouse: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Debug.WriteLine("Error in UpdateQuoteWarehouse: " & ex.Message)
                Return False
            End Try
        End Function


        ''' Alternative: Updates warehouse and other quote details together
        Public Shared Function UpdateQuoteWithWarehouse(quoteNumber As String, warehouseID As Integer, warehouseName As String,
                                                clientID As String, totalTax As Decimal, totalDiscount As Decimal,
                                                totalAmount As Decimal) As Boolean
            Try
                Dim query As String = "UPDATE quotes SET WarehouseID = @warehouseID, WarehouseName = @warehouseName, " &
                             "ClientID = @clientID, TotalTax = @totalTax, TotalDiscount = @totalDiscount, " &
                             "TotalAmount = @totalAmount, UpdatedAt = @updatedAt WHERE QuoteNumber = @quoteNumber"

                Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@warehouseID", warehouseID)
                        cmd.Parameters.AddWithValue("@warehouseName", warehouseName)
                        cmd.Parameters.AddWithValue("@clientID", clientID)
                        cmd.Parameters.AddWithValue("@totalTax", totalTax)
                        cmd.Parameters.AddWithValue("@totalDiscount", totalDiscount)
                        cmd.Parameters.AddWithValue("@totalAmount", totalAmount)
                        cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@quoteNumber", quoteNumber)
                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                        Return rowsAffected > 0
                    End Using
                End Using

            Catch ex As Exception
                Debug.WriteLine("Error in UpdateQuoteWithWarehouse: " & ex.Message)
                Return False
            End Try
        End Function


        ''' Refreshes the data when returning to this view
        Public Sub RefreshData()
            LoadData()
        End Sub

        Private Sub GetDataFromDB()
            If String.IsNullOrWhiteSpace(SearchText.Text) Then
                dataGrid.ItemsSource = Nothing
                dataGrid.ItemsSource = QuotesController.GetQuotes(CInt(cmbLimit.Text))
            Else
                dataGrid.ItemsSource = Nothing
                dataGrid.ItemsSource = QuotesController.SearchQuotes(SearchText.Text, CInt(cmbLimit.Text))
            End If
        End Sub

        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            ' Check if DataGrid has data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Use the ExcelExporter helper with column exclusions
            ExcelExporter.ExportDataGridToExcel(dataGrid, "QuotesExport", "Quotes List")

        End Sub

        ' Setup bindings between DatePickers, Buttons, and ViewModels
        Public Sub SetupDatePickers()
            startDateViewModel.SelectedDate = Nothing
            dueDateViewModel.SelectedDate = Nothing

            StartDatePicker.DataContext = startDateViewModel
            StartDateButton.DataContext = startDateViewModel

            DueDatePicker.DataContext = dueDateViewModel
            DueDateButton.DataContext = dueDateViewModel
        End Sub

        ' Trigger dropdown open
        Private Sub StartDateButton_Click(sender As Object, e As RoutedEventArgs) Handles StartDateButton.Click
            StartDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub DueDateButton_Click(sender As Object, e As RoutedEventArgs) Handles DueDateButton.Click
            DueDatePicker.IsDropDownOpen = True
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


        ' Handle selected date change
        Private Sub StartDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles StartDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    BindingOperations.GetBindingExpression(StartDateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If
        End Sub

        Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DueDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    BindingOperations.GetBindingExpression(DueDateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If
        End Sub
        Friend Sub ShowPopup(parent As UIElement, sender As Object)
            ' Ensure sender is a Button
            Dim button As Button = TryCast(sender, Button)
            If button Is Nothing Then
                Return
            End If

            ' Get the window containing the button
            Dim window As Window = Window.GetWindow(button)
            If window Is Nothing Then
                Return
            End If

            ' Get sidebar width - determine if sidebar is expanded or collapsed
            Dim sidebarWidth As Double = 0

            ' Get parent sidebar if available
            Dim parentControl = TryCast(button.Parent, FrameworkElement)
            While parentControl IsNot Nothing
                If TypeOf parentControl Is StackPanel AndAlso parentControl.Name = "SidebarMenu" Then
                    ' Found the sidebar menu container, get its parent (likely the sidebar)
                    Dim sidebarContainer = TryCast(parentControl.Parent, FrameworkElement)
                    If sidebarContainer IsNot Nothing Then
                        sidebarWidth = sidebarContainer.ActualWidth
                        Exit While
                    End If
                ElseIf TypeOf parentControl.Parent Is DPC.Components.Navigation.Sidebar Then
                    ' Direct parent is sidebar
                    sidebarWidth = CType(parentControl.Parent, FrameworkElement).ActualWidth
                    Exit While
                End If
                parentControl = TryCast(parentControl.Parent, FrameworkElement)
            End While

            ' If we couldn't find sidebar, use a default value
            If sidebarWidth = 0 Then
                ' Default to expanded sidebar width
                sidebarWidth = 260
            End If

            ' Create the popup with proper positioning
            Dim popup As New Popup With {
    .Child = Me,
    .StaysOpen = False,
    .Placement = PlacementMode.Relative,
    .PlacementTarget = button,
    .IsOpen = True,
    .AllowsTransparency = True
}

            ' Calculate optimal position based on sidebar width
            If sidebarWidth <= 80 Then
                ' Sidebar is collapsed - position menu farther right
                popup.HorizontalOffset = 60
                popup.VerticalOffset = -button.ActualHeight * 3 ' Align with button
            Else
                ' Sidebar is expanded - position menu immediately to the right
                popup.HorizontalOffset = sidebarWidth - button.Margin.Left
                popup.VerticalOffset = -button.ActualHeight * 3 ' Align with button
            End If

            ' Store references to event handlers so we can remove them later
            Dim locationChangedHandler As EventHandler = Nothing
            Dim sizeChangedHandler As SizeChangedEventHandler = Nothing

            ' Define event handlers
            locationChangedHandler = Sub(s, e)
                                         If popup.IsOpen Then
                                             ' Recalculate position when window moves
                                             popup.HorizontalOffset = popup.HorizontalOffset
                                             popup.VerticalOffset = popup.VerticalOffset
                                         End If
                                     End Sub

            sizeChangedHandler = Sub(s, e)
                                     If popup.IsOpen Then
                                         ' Recalculate position when window resizes
                                         popup.HorizontalOffset = popup.HorizontalOffset
                                         popup.VerticalOffset = popup.VerticalOffset
                                     End If
                                 End Sub

            ' Add event handlers
            AddHandler window.LocationChanged, locationChangedHandler
            AddHandler window.SizeChanged, sizeChangedHandler

            ' Handle popup closed to cleanup event handlers
            AddHandler popup.Closed, Sub(s, e)
                                         RemoveHandler window.LocationChanged, locationChangedHandler
                                         RemoveHandler window.SizeChanged, sizeChangedHandler
                                     End Sub
        End Sub

        Private Sub NavigateToQuotes(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("navigatetoquotes", Me)
        End Sub

        Private Sub SearchText_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Reset the timer
            _typingTimer.Stop()

            ' Start the timer
            _typingTimer.Start()
        End Sub

        Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
            ' Stop the timer
            _typingTimer.Stop()

            PerformSearch()
        End Sub

        Private Sub dataGrid_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles dataGrid.SelectionChanged
        End Sub

        Private Sub cmbLimit_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbLimit.SelectionChanged
        End Sub

        Private Shared Function GetValidityDate(validitySelection As String, baseDate As DateTime) As String
            Try
                ' Remove extra spaces and convert to lowercase for comparison
                Dim selection = validitySelection.Trim().ToLower()

                ' If it's already a date in format "yyyy-MM-dd", convert to display format
                Dim validityDate As DateTime
                If DateTime.TryParse(selection, validityDate) Then
                    Return validityDate.ToString("MMM d, yyyy")
                End If

                ' Calculate from base date based on ComboBox items
                Select Case selection
                    Case "48 hours"
                        validityDate = baseDate.AddHours(48)
                    Case "1 week"
                        validityDate = baseDate.AddDays(7)
                    Case "2 weeks"
                        validityDate = baseDate.AddDays(14)
                    Case "3 weeks"
                        validityDate = baseDate.AddDays(21)
                    Case "1 month"
                        validityDate = baseDate.AddMonths(1)
                    Case "2 months"
                        validityDate = baseDate.AddMonths(2)
                    Case "6 months"
                        validityDate = baseDate.AddMonths(6)
                    Case "1 year"
                        validityDate = baseDate.AddYears(1)
                End Select

                ' Return in display format: MMM d, yyyy
                Return validityDate.ToString("MMM d, yyyy")

            Catch ex As Exception
                Debug.WriteLine($"Error calculating validity date: {ex.Message}")
                Return baseDate.AddHours(0).ToString("MMM d, yyyy")
            End Try
        End Function

        ''' <summary>
        ''' Formats the quote data before displaying in DataGrid
        ''' Converts all validity dates from database format to display format
        ''' </summary>
        Private Shared Sub FormatQuotesForDisplay(quotes As ObservableCollection(Of QuotesModel))
            Try
                For Each quote In quotes
                    ' Parse QuoteDate from database format (yyyy-MM-dd)
                    Dim quoteDateValue As DateTime = DateTime.Now
                    If Not String.IsNullOrEmpty(quote.QuoteDate) AndAlso quote.QuoteDate <> "-" Then
                        If DateTime.TryParse(quote.QuoteDate, quoteDateValue) Then
                            ' Keep quoteDate as-is in display format
                            quote.QuoteDate = quoteDateValue.ToString("MMM d, yyyy")
                        End If
                    End If

                    ' Calculate and display validity date based on validity option
                    If Not String.IsNullOrEmpty(quote.Validity) AndAlso quote.Validity <> "-" Then
                        ' Check if Validity is an option (like "6 months") or already a date
                        Dim validityDate As DateTime

                        ' Try to parse as date first
                        If DateTime.TryParse(quote.Validity, validityDate) Then
                            ' It's stored as a date in database - just format for display
                            quote.Validity = validityDate.ToString("MMM d, yyyy")
                        Else
                            ' It's stored as an option (like "6 months") - calculate the date
                            validityDate = CalculateValidityDate(quote.Validity, quoteDateValue)
                            quote.Validity = validityDate.ToString("MMM d, yyyy")
                        End If
                    End If
                Next
            Catch ex As Exception
                Debug.WriteLine($"Error formatting quotes: {ex.Message}")
            End Try
        End Sub


        ''' Cache data holder for quote information
        Public Class QuoteCacheData
            Public Property QuoteNumber As String = ""
            Public Property ClientID As String = ""
            Public Property ClientName As String = ""
            Public Property TxtClientDetails As String = ""
            Public Property WarehouseID As String = ""
            Public Property WarehouseName As String = ""
            Public Property QuoteDate As String = ""
            Public Property Validity As String = ""
            Public Property QuoteNote As String = ""
            Public Property TotalTax As Object = 0
            Public Property TotalDiscount As Object = 0
            Public Property TotalPrice As Object = 0
            Public Property OrderItems As Object = Nothing
        End Class


    End Class
End Namespace