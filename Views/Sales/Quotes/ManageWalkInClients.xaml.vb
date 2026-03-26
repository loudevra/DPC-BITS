Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports System.IO
Imports Microsoft.Win32

Namespace DPC.Views.Sales.Quotes
    Public Class ManageWalkInClients
        Inherits UserControl

        ' ViewModels for the custom DatePicker behavior
        Private startDateViewModel As New CalendarController.SingleCalendar()
        Private dueDateViewModel As New CalendarController.SingleCalendar()
        Private _typingTimer As DispatcherTimer

        Private _isInitialized As Boolean = False
        Private _BillingNumber As String
        Private _Type As String = "Private" ' Default filter for billing

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
        ''' Loads billing data into the DataGrid based on the selected limit
        ''' </summary>
        Public Sub LoadData()
            Try
                ' Extract limit from ComboBoxItem
                Dim limit As Integer = 10
                If cmbLimit.SelectedItem IsNot Nothing Then
                    limit = Convert.ToInt32(CType(cmbLimit.SelectedItem, ComboBoxItem).Content)
                End If

                ' Call the BillingController to fetch records
                Dim statements = BillingController.GetBillingStatements(limit, _Type)

                FormatStatementsForDisplay(statements)
                dataGrid.ItemsSource = statements
            Catch ex As Exception
                MessageBox.Show($"Error loading billing data: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Formats the model dates into user-friendly strings for the UI
        ''' </summary>
        Private Shared Sub FormatStatementsForDisplay(statements As ObservableCollection(Of BillingModel))
            Try
                For Each stmt In statements
                    ' Format Billing Date
                    If Not String.IsNullOrEmpty(stmt.BillingDate) AndAlso stmt.BillingDate <> "-" Then
                        Dim dt As DateTime
                        If DateTime.TryParse(stmt.BillingDate, dt) Then
                            stmt.BillingDate = dt.ToString("MMM d, yyyy")
                        End If
                    End If

                    ' Format DateAdded if used in columns
                    ' stmt.DateAdded is handled by the model
                Next
            Catch ex As Exception
                Debug.WriteLine($"Error formatting: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Logic for the "Edit" button inside the DataGrid rows
        ''' </summary>
        Private Sub OpenEditStatement(sender As Object, e As RoutedEventArgs)
            Dim selectedQuote As BillingModel = TryCast(dataGrid.SelectedItem, BillingModel)

            If selectedQuote IsNot Nothing Then
                TransactionState.ResetRecord()

                With TransactionState.ActiveRecord
                    .EditLabel = "Edit Billing Statement"
                    .EditButtonLabel = "UPDATE BILLING STATEMENT"
                    .DocumentNumber = selectedQuote.BillingNumber
                    .ClientName = selectedQuote.ClientName
                    .ClientId = selectedQuote.ClientID
                    .DocumentDate = selectedQuote.BillingDate
                    .Notes = selectedQuote.BillingNote
                    .WarehouseName = selectedQuote.WarehouseName
                    .WarehouseID = selectedQuote.WarehouseID.ToString()
                    .IsEditMode = True

                    Try
                        Dim jsonString As String = If(selectedQuote.OrderItems IsNot Nothing, selectedQuote.OrderItems.ToString(), "[]")

                        Dim rawData = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(jsonString)

                        .OrderItems.Clear()

                        If rawData IsNot Nothing Then
                            For Each dict In rawData
                                Dim newItem As New OrderItems()

                                newItem.ProductName = If(dict.ContainsKey("ProductName"), dict("ProductName"), "")
                                newItem.Quantity = If(dict.ContainsKey("Quantity"), dict("Quantity"), "0")
                                newItem.Description = If(dict.ContainsKey("Description"), dict("Description"), "")
                                newItem.ProductDescription = If(dict.ContainsKey("ProductDescription"), dict("ProductDescription"), "")
                                newItem.UnitPrice = If(dict.ContainsKey("UnitPrice"), dict("UnitPrice"),
                                                    If(dict.ContainsKey("Rate"), dict("Rate"), "0.00"))

                                newItem.LinePrice = If(dict.ContainsKey("LinePrice"), dict("LinePrice"),
                                                    If(dict.ContainsKey("Amount"), dict("Amount"), "0.00"))

                                Dim isHeaderVal As Boolean = False
                                If dict.ContainsKey("IsHeaderRow") Then
                                    Boolean.TryParse(dict("IsHeaderRow").ToString(), isHeaderVal)
                                End If
                                newItem.IsHeaderRow = isHeaderVal
                                newItem.IsCategoryHeader = isHeaderVal

                                Dim isSubtotalVal As Boolean = False
                                If dict.ContainsKey("IsSubtotalRow") Then
                                    Boolean.TryParse(dict("IsSubtotalRow").ToString(), isSubtotalVal)
                                ElseIf dict.ContainsKey("IsSubotalRow") Then
                                    Boolean.TryParse(dict("IsSubotalRow").ToString(), isSubtotalVal)
                                End If
                                newItem.IsSubtotalRow = isSubtotalVal

                                newItem.ProductDescriptionVisibility = If(String.IsNullOrWhiteSpace(newItem.ProductDescription),
                                                 Visibility.Collapsed, Visibility.Visible)

                                .OrderItems.Add(newItem)
                            Next
                        End If
                    Catch ex As Exception
                        MessageBox.Show("Error parsing quote items: " & ex.Message, "Mapping Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    End Try
                End With

                ViewLoader.DynamicView.NavigateToView("walkinorder", Me)
            End If
        End Sub

        ''' <summary>
        ''' Logic for the "Delete" button inside the DataGrid rows
        ''' </summary>
        Private Sub DeleteStatement(sender As Object, e As RoutedEventArgs)
            Dim statement As BillingModel = TryCast(dataGrid.SelectedItem, BillingModel)
            If statement Is Nothing Then Return

            Dim result = MessageBox.Show($"Are you sure you want to delete Billing Statement {statement.BillingNumber}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)

            If result = MessageBoxResult.Yes Then
                Try
                    ' You can move this query to BillingController for better structure
                    Dim query As String = "DELETE FROM walkinbilling WHERE billingNumber = @billingNumber"
                    Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                        conn.Open()
                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@billingNumber", statement.BillingNumber)
                            cmd.ExecuteNonQuery()
                        End Using
                    End Using

                    MessageBox.Show("Statement deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    LoadData()
                Catch ex As Exception
                    MessageBox.Show("Error deleting: " & ex.Message)
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

                Dim results = BillingController.SearchBillingStatements(SearchText.Text.Trim(), limit, _Type)
                FormatStatementsForDisplay(results)
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
        End Sub

        Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            dueDateViewModel.SelectedDate = DueDatePicker.SelectedDate
        End Sub

        ' ==========================================
        ' EXCEL / CSV EXPORT LOGIC
        ' ==========================================
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            Try
                ' 1. Check if there is data in the grid
                If dataGrid.Items.Count = 0 Then
                    MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                ' 2. Open the Save File Dialog
                Dim saveFileDialog As New SaveFileDialog()
                saveFileDialog.Filter = "CSV (Excel Compatible) (*.csv)|*.csv"
                saveFileDialog.FileName = "Billing_Statements_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
                saveFileDialog.Title = "Export Billing Statements to Excel"

                ' 3. If the user clicks "Save"
                If saveFileDialog.ShowDialog() = True Then

                    ' 4. Create and write to the file
                    Using writer As New StreamWriter(saveFileDialog.FileName)
                        ' Write the Header Row perfectly matching your DataGrid columns in the XAML
                        writer.WriteLine("Billing No.,DR No.,Date,Terms,Tax,Discount,Total Amount,Items")

                        ' 5. Loop through the current items in the DataGrid and write them safely
                        For Each obj In dataGrid.Items
                            Dim item As BillingModel = TryCast(obj, BillingModel)

                            If item IsNot Nothing Then
                                ' Using string interpolation safely handles Null/Empty data automatically
                                ' We replace double quotes with double-double quotes to prevent CSV formatting breaks
                                Dim billNo As String = $"{item.BillingNumber}".Replace("""", """""")
                                Dim drNo As String = $"{item.DRNo}".Replace("""", """""")
                                Dim bDate As String = $"{item.BillingDate}".Replace("""", """""")
                                Dim terms As String = $"{item.PaymentTerms}".Replace("""", """""")
                                Dim tax As String = $"{item.TotalTax}".Replace("""", """""")
                                Dim discount As String = $"{item.TotalDiscount}".Replace("""", """""")
                                Dim total As String = $"{item.TotalAmount}".Replace("""", """""")
                                Dim itemsData As String = $"{item.OrderItems}".Replace("""", """""")

                                ' Wrap in quotes to prevent stray commas inside data from breaking columns
                                writer.WriteLine($"""{billNo}"",""{drNo}"",""{bDate}"",""{terms}"",""{tax}"",""{discount}"",""{total}"",""{itemsData}""")
                            End If
                        Next
                    End Using

                    MessageBox.Show("Billing statements successfully exported to Excel!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                MessageBox.Show($"An error occurred while exporting: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub NavigateToGovernmentQuotes(sender As Object, e As RoutedEventArgs)
            ' Redirect to your Add Billing View
            ViewLoader.DynamicView.NavigateToView("billingform", Me)
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

        Private Function GetCacheModule() As BillingModel
            If Not Application.Current.Properties.Contains("BillingCache") Then
                Application.Current.Properties("BillingCache") = New BillingModel()
            End If
            Return DirectCast(Application.Current.Properties("BillingCache"), BillingModel)
        End Function
    End Class
End Namespace