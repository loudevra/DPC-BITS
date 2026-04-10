Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient
Imports System.IO
Imports Microsoft.Win32

Namespace DPC.Views.Stocks.PurchaseOrder.Delivery
    Public Class ManageDeliveryReceipt
        Inherits UserControl

        ' ViewModels for the custom DatePicker behavior
        Private startDateViewModel As New CalendarController.SingleCalendar()
        Private dueDateViewModel As New CalendarController.SingleCalendar()
        Private _typingTimer As DispatcherTimer

        Private _isInitialized As Boolean = False
        Private _currentDRNumber As String

        ' User access control properties
        Private _currentUserID As String
        Private _currentUserRole As String
        Private _isAdmin As Boolean = False

        Public Sub New()
            InitializeComponent()
            SetupDatePickers()

            ' Initialize user access control
            InitializeUserAccess()

            ' Initialize the search delay timer
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(250)
            }
            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick

            ' Load data when the control is ready
            AddHandler Me.Loaded, AddressOf UserControl_Loaded
        End Sub

        ''' <summary>
        ''' Initialize user access control based on logged-in user
        ''' </summary>
        Private Sub InitializeUserAccess()
            Try
                ' Get current user ID from global cache
                _currentUserID = CacheOnEmployeeID

                ' Get user role from database
                _currentUserRole = GetCurrentUserRole(_currentUserID)

                ' Check if user is admin (has full access)
                _isAdmin = (_currentUserRole = "Administrator" OrElse _currentUserRole = "Business Owner")

                ' Display access level in UI (optional)
                If Not _isAdmin Then
                    Debug.WriteLine($"User {_currentUserID} has restricted access to delivery receipts")
                End If
            Catch ex As Exception
                MessageBox.Show($"Error initializing user access: {ex.Message}", "Access Control Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                _isAdmin = False ' Default to restricted access on error
            End Try
        End Sub

        ''' <summary>
        ''' Get the role of the current user from the database
        ''' </summary>
        Private Function GetCurrentUserRole(employeeID As String) As String
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT r.RoleName FROM employee e " &
                                        "INNER JOIN userroles r ON e.UserRoleID = r.RoleID " &
                                        "WHERE e.EmployeeID = @EmployeeID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                        Dim result As Object = cmd.ExecuteScalar()
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return result.ToString()
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine($"Error getting user role: {ex.Message}")
            End Try
            Return "User" ' Default role
        End Function

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
        ''' Loads Delivery Receipt data into the DataGrid based on the selected limit and user access
        ''' </summary>
        Public Sub LoadData()
            Try
                ' Extract limit from ComboBoxItem
                Dim limit As Integer = 10
                If cmbLimit.SelectedItem IsNot Nothing Then
                    limit = Convert.ToInt32(CType(cmbLimit.SelectedItem, ComboBoxItem).Content)
                End If

                ' Call the DeliveryReceiptController to fetch records with user filtering
                Dim receipts = DeliveryReceiptController.GetDeliveryReceipts(limit, _currentUserID, _isAdmin)

                FormatDeliveryReceiptsForDisplay(receipts)
                dataGrid.ItemsSource = receipts
            Catch ex As Exception
                MessageBox.Show($"Error loading delivery receipts: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Formats the model dates into user-friendly strings for the UI
        ''' </summary>
        Private Shared Sub FormatDeliveryReceiptsForDisplay(receipts As ObservableCollection(Of UniversalTransactionModel))
            Try
                For Each receipt In receipts
                    ' 1. Format Dates (Your existing code)
                    If Not String.IsNullOrEmpty(receipt.DocumentDate) AndAlso receipt.DocumentDate <> "-" Then
                        Dim dt As DateTime
                        If DateTime.TryParse(receipt.DocumentDate, dt) Then
                            receipt.DocumentDate = dt.ToString("MMM d, yyyy")
                        End If
                    End If

                    If receipt.DateAdded <> DateTime.MinValue Then
                        receipt.DateAdded = receipt.DateAdded.ToString("MMM d, yyyy")
                    End If

                    Dim historyTotals = DeliveryReceiptController.GetAccumulatedDeliveryTotals(receipt.DocumentReference)
                    Dim billingResults = BillingController.SearchBillingStatements(receipt.DocumentReference, 1, "Private")

                    Dim isFinished As Boolean = True

                    If billingResults.Count > 0 Then
                        Dim masterItems = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(billingResults(0).OrderItems)

                        If masterItems IsNot Nothing Then
                            For Each item In masterItems
                                Dim pName = item("ProductName")
                                Dim originalQty As Integer = 0
                                Integer.TryParse(item("Quantity"), originalQty)

                                Dim deliveredSoFar = If(historyTotals.ContainsKey(pName), historyTotals(pName), 0)

                                If originalQty - deliveredSoFar > 0 Then
                                    isFinished = False
                                    Exit For
                                End If
                            Next
                        End If
                    End If

                    ' Assign to the model property used by your XAML DataTrigger
                    receipt.IsFullyDelivered = isFinished
                Next
            Catch ex As Exception
                Debug.WriteLine($"Error formatting delivery receipts: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Logic for the "Edit" button inside the DataGrid rows
        ''' Only allows editing if user owns the record or is admin
        ''' </summary>
        Private Sub OpenEditDeliveryReceipt(sender As Object, e As RoutedEventArgs)
            Dim receipt As UniversalTransactionModel = TryCast(dataGrid.SelectedItem, UniversalTransactionModel)

            If receipt IsNot Nothing Then
                ' Check access control
                If Not _isAdmin AndAlso receipt.CreatedBy <> _currentUserID Then
                    MessageBox.Show("You do not have permission to edit this delivery receipt.",
                                  "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                TransactionState.ResetRecord()

                Dim fullReceiptData = DeliveryReceiptController.GetDeliveryReceiptByDRNumber(receipt.DocumentNumber)

                If fullReceiptData IsNot Nothing Then
                    TransactionState.ActiveRecord = fullReceiptData
                    TransactionState.IsEditMode = True

                    ViewLoader.DynamicView.NavigateToView("newdelivery", Me)
                Else
                    MessageBox.Show("Could not retrieve items for this delivery. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            End If
        End Sub

        Private Sub OpenContinueDeliveryReceipt(sender As Object, e As RoutedEventArgs)
            Dim receipt As UniversalTransactionModel = TryCast(dataGrid.SelectedItem, UniversalTransactionModel)

            ' Check access control for continuing delivery
            If Not _isAdmin AndAlso receipt.CreatedBy <> _currentUserID Then
                MessageBox.Show("You do not have permission to continue this delivery receipt.",
                              "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            DeliveryDetails.ClearDeliveryDetails()

            If receipt IsNot Nothing Then
                Dim currentDR As String = receipt.DocumentNumber
                Dim nextDR As String = ""

                If currentDR.Contains("(P") Then
                    Try
                        Dim parts = currentDR.Split(New String() {"(P"}, StringSplitOptions.None)
                        Dim baseDR = parts(0)
                        Dim numPart = parts(1).Replace(")", "").Trim()
                        Dim versionNum As Integer = 0

                        If Integer.TryParse(numPart, versionNum) Then
                            nextDR = $"{baseDR}(P{versionNum + 1})"
                        Else
                            nextDR = currentDR & "(P1)"
                        End If
                    Catch
                        nextDR = currentDR & "(P1)"
                    End Try
                Else
                    nextDR = currentDR & "(P1)"
                End If

                DeliveryDetails.DRNumber = nextDR
                DeliveryDetails.DRDocumentReference = receipt.DocumentReference
                DeliveryDetails.DRClientName = receipt.ClientName
                DeliveryDetails.DRClientDetails = receipt.ClientDetails
                DeliveryDetails.DRDate = receipt.DocumentDate
                DeliveryDetails.DRShippingMethod = receipt.ShippingMethod
                DeliveryDetails.DRDeliveryNotes = receipt.Notes
                DeliveryDetails.DRApprovedBy = receipt.ApprovedBy
                DeliveryDetails.DRPaymentTerm = receipt.PaymentTerm

                Dim historyTotals = DeliveryReceiptController.GetAccumulatedDeliveryTotals(receipt.DocumentReference)
                Dim billingResults = BillingController.SearchBillingStatements(receipt.DocumentReference, 1, "Private")

                If billingResults.Count > 0 Then
                    Dim masterItems = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(billingResults(0).OrderItems)
                    Dim remainingList As New List(Of Dictionary(Of String, String))

                    For Each masterItem In masterItems
                        Dim pName = masterItem("ProductName")
                        Dim originalQty = 0
                        Integer.TryParse(masterItem("Quantity"), originalQty)

                        Dim deliveredSoFar = If(historyTotals.ContainsKey(pName), historyTotals(pName), 0)
                        Dim balance = originalQty - deliveredSoFar

                        If balance > 0 Then
                            Dim newItem = New Dictionary(Of String, String)(masterItem)
                            newItem("Quantity") = balance.ToString()
                            newItem("MaxAllowed") = balance.ToString()
                            remainingList.Add(newItem)
                        End If
                    Next

                    ' Save to Global Cache
                    DeliveryDetails.DRDeliveryItems = remainingList
                End If

                ViewLoader.DynamicView.NavigateToView("newdelivery", Me)
            End If
        End Sub

        ''' <summary>
        ''' Logic for the "Delete" button inside the DataGrid rows
        ''' Only allows deletion if user owns the record or is admin
        ''' </summary>
        Private Sub DeleteDeliveryReceipt(sender As Object, e As RoutedEventArgs)
            Dim receipt As UniversalTransactionModel = TryCast(dataGrid.SelectedItem, UniversalTransactionModel)
            If receipt Is Nothing Then Return

            ' Check access control
            If Not _isAdmin AndAlso receipt.CreatedBy <> _currentUserID Then
                MessageBox.Show("You do not have permission to delete this delivery receipt.",
                              "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim result = MessageBox.Show($"Are you sure you want to delete Delivery Receipt {receipt.DocumentNumber}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)

            If result = MessageBoxResult.Yes Then
                Try
                    Dim query As String = "DELETE FROM deliveryreceipts WHERE DRNumber = @drNumber"
                    Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                        conn.Open()
                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@drNumber", receipt.DocumentNumber)
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

                ' Use user-filtered search
                Dim results = DeliveryReceiptController.SearchDeliveryReceipts(SearchText.Text.Trim(), limit, _currentUserID, _isAdmin)
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
            If _isInitialized Then PerformSearch() ' Re-run search on date change
        End Sub

        Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            dueDateViewModel.SelectedDate = DueDatePicker.SelectedDate
            If _isInitialized Then PerformSearch() ' Re-run search on date change
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
                saveFileDialog.FileName = "Delivery_Receipts_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
                saveFileDialog.Title = "Export Delivery Receipts to Excel"

                ' 3. If the user clicks "Save"
                If saveFileDialog.ShowDialog() = True Then

                    ' 4. Create and write to the file
                    Using writer As New StreamWriter(saveFileDialog.FileName)
                        ' Write the Header Row perfectly matching your DataGrid columns
                        writer.WriteLine("DR Number,Invoice Ref,DR Date,Status,Client Name,Shipping,Prepared By,System Date")

                        ' 5. Loop through the current items in the DataGrid and write them safely
                        For Each obj In dataGrid.Items
                            Dim item As UniversalTransactionModel = TryCast(obj, UniversalTransactionModel)

                            If item IsNot Nothing Then
                                ' Using string interpolation safely handles Null/Empty data automatically
                                ' We wrap everything in double quotes to prevent internal commas from breaking columns
                                Dim drNum As String = $"{item.DocumentNumber}".Replace("""", """""")
                                Dim invRef As String = $"{item.DocumentReference}".Replace("""", """""")
                                Dim drDate As String = $"{item.DocumentDate}".Replace("""", """""")
                                Dim status As String = If(item.IsFullyDelivered, "Complete", "Partial").Replace("""", """""")
                                Dim client As String = $"{item.ClientName}".Replace("""", """""")
                                Dim shipping As String = $"{item.ShippingMethod}".Replace("""", """""")
                                Dim prepBy As String = $"{item.PreparedBy}".Replace("""", """""")
                                Dim sysDate As String = $"{item.DateAdded}".Replace("""", """""")

                                writer.WriteLine($"""{drNum}"",""{invRef}"",""{drDate}"",""{status}"",""{client}"",""{shipping}"",""{prepBy}"",""{sysDate}""")
                            End If
                        Next
                    End Using

                    MessageBox.Show("Delivery receipt data successfully exported to Excel!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                MessageBox.Show($"An error occurred while exporting: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
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