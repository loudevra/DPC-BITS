' EmployeesView.xaml.vb
Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient

Namespace DPC.Views.HRM.Employees.Employees
    Partial Public Class EmployeesView
        Inherits UserControl

        ' UI elements for direct access
        Private popup As Popup
        Private recentlyClosed As Boolean = False
        Private Employees As New ObservableCollection(Of Employee)()

        ' Declare Pagination Variables
        Private CurrentPage As Integer = 1
        Private TotalPages As Integer = 1
        Private PageSize As Integer = 10

        Public Sub New()
            InitializeComponent()
            ' LoadEmployees is now handled by the Loaded event to prevent NullReference errors
        End Sub

        ''' <summary>
        ''' Event fired when the UserControl is fully rendered. Safe to load data here.
        ''' </summary>
        Private Sub EmployeesView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            LoadEmployees()
        End Sub

        ''' <summary>
        ''' Handles changing the items per page (10, 25, 50, 100)
        ''' </summary>
        Private Sub ComboPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedItem As ComboBoxItem = TryCast(ComboPageSize.SelectedItem, ComboBoxItem)

            If selectedItem IsNot Nothing AndAlso IsLoaded Then
                ' Update the PageSize variable
                PageSize = Integer.Parse(selectedItem.Content.ToString())

                ' Reset to page 1 whenever page size changes
                CurrentPage = 1

                ' Re-load data
                LoadEmployees()
            End If
        End Sub

        ''' <summary>
        ''' Manually bubbles the mouse wheel event to the parent container
        ''' </summary>
        Private Sub DataGrid_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs)
            If Not e.Handled Then
                e.Handled = True
                Dim eventArg As New MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                eventArg.RoutedEvent = UIElement.MouseWheelEvent
                eventArg.Source = sender

                ' Find the parent to bubble the event up
                Dim parent As UIElement = TryCast(DirectCast(sender, Control).Parent, UIElement)
                If parent IsNot Nothing Then
                    parent.RaiseEvent(eventArg)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Load Employees from Database using SQL LIMIT and OFFSET
        ''' </summary>
        Private Sub LoadEmployees()
            ' Safety Check: Ensure DataGrid exists
            If EmployeesDataGrid Is Nothing Then Exit Sub

            Employees.Clear()

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' 1. Calculate Total Records and Pages
                    Dim countQuery As String = "SELECT COUNT(*) FROM employee"
                    Using countCmd As New MySqlCommand(countQuery, conn)
                        Dim totalRecords As Integer = Convert.ToInt32(countCmd.ExecuteScalar())
                        TotalPages = Math.Ceiling(totalRecords / PageSize)
                        If TotalPages = 0 Then TotalPages = 1
                    End Using

                    ' 2. Calculate Offset for current page
                    Dim offset As Integer = (CurrentPage - 1) * PageSize

                    ' 3. Fetch Paginated Data
                    Dim query As String = "SELECT e.*, r.RoleName, l.LocationName FROM employee e " &
                                          "LEFT JOIN userroles r ON e.UserRoleID = r.RoleID " &
                                          "LEFT JOIN businesslocation l ON e.BusinessLocationID = l.LocationID " &
                                          "ORDER BY e.CreatedAt DESC " &
                                          "LIMIT @limit OFFSET @offset"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@limit", PageSize)
                        cmd.Parameters.AddWithValue("@offset", offset)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Employees.Add(New Employee() With {
                                    .EmployeeID = reader("EmployeeID").ToString(),
                                    .Username = reader("Username").ToString(),
                                    .Department = If(IsDBNull(reader("Department")), "", reader("Department").ToString()),
                                    .Status = If(IsDBNull(reader("Status")), "", reader("Status").ToString()),
                                    .Email = reader("Email").ToString(),
                                    .Name = reader("Name").ToString(),
                                    .RoleName = If(IsDBNull(reader("RoleName")), "", reader("RoleName").ToString()),
                                    .LocationName = If(IsDBNull(reader("LocationName")), "", reader("LocationName").ToString()),
                                    .CreatedAt = Convert.ToDateTime(reader("CreatedAt")),
                                    .UpdatedAt = Convert.ToDateTime(reader("UpdatedAt"))
                                })
                            End While
                        End Using
                    End Using
                End Using

                ' Refresh UI Binding
                EmployeesDataGrid.ItemsSource = Nothing
                EmployeesDataGrid.ItemsSource = Employees
                UpdatePagination()

            Catch ex As Exception
                MessageBox.Show($"Error loading employees: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub ViewEmployee(sender As Object, e As RoutedEventArgs)
            Dim selectedEmployee As Employee = CType(EmployeesDataGrid.SelectedItem, Employee)
            If selectedEmployee IsNot Nothing Then
                MessageBox.Show($"Employee: {selectedEmployee.Name}" & vbCrLf &
                                $"Role: {selectedEmployee.RoleName}" & vbCrLf &
                                $"Location: {selectedEmployee.LocationName}", "Employee Info", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub

        Private Sub UpdatePagination()
            ' Safety check for UI elements
            If TxtPageNumber IsNot Nothing Then
                TxtPageNumber.Text = $"Page {CurrentPage} of {TotalPages}"
                BtnFirstPage.IsEnabled = (CurrentPage > 1)
                BtnPrevPage.IsEnabled = (CurrentPage > 1)
                BtnNextPage.IsEnabled = (CurrentPage < TotalPages)
                BtnLastPage.IsEnabled = (CurrentPage < TotalPages)
            End If
        End Sub

        ' Pagination Controls
        Private Sub BtnFirstPage_Click(sender As Object, e As RoutedEventArgs)
            CurrentPage = 1
            LoadEmployees()
        End Sub

        Private Sub BtnPrevPage_Click(sender As Object, e As RoutedEventArgs)
            If CurrentPage > 1 Then
                CurrentPage -= 1
                LoadEmployees()
            End If
        End Sub

        Private Sub BtnNextPage_Click(sender As Object, e As RoutedEventArgs)
            If CurrentPage < TotalPages Then
                CurrentPage += 1
                LoadEmployees()
            End If
        End Sub

        Private Sub BtnLastPage_Click(sender As Object, e As RoutedEventArgs)
            CurrentPage = TotalPages
            LoadEmployees()
        End Sub

        Private Sub AddEmployee(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("addnewemployee", Me)
        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim searchText As String = txtSearch.Text.Trim()
            If String.IsNullOrEmpty(searchText) Then
                LoadEmployees()
            Else
                SearchEmployee(searchText)
            End If
        End Sub

        Private Sub SearchEmployee(query As String)
            Dim conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
            Try
                conn.Open()
                Dim sql As String = "SELECT e.*, r.RoleName, l.LocationName " &
                                    "FROM employee e " &
                                    "JOIN userroles r ON e.UserRoleID = r.RoleID " &
                                    "JOIN businesslocation l ON e.BusinessLocationID = l.LocationID " &
                                    "WHERE e.EmployeeID LIKE @query OR e.Name LIKE @query " &
                                    "ORDER BY e.CreatedAt DESC"

                Dim cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@query", "%" & query & "%")

                Dim adapter As New MySqlDataAdapter(cmd)
                Dim dt As New DataTable()
                adapter.Fill(dt)

                EmployeesDataGrid.ItemsSource = dt.DefaultView
            Catch ex As Exception
                MessageBox.Show("Search error: " & ex.Message)
            Finally
                conn.Close()
            End Try
        End Sub

        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            If EmployeesDataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Dim columnsToExclude As New List(Of String) From {"Actions", "Status"}
            ExcelExporter.ExportDataGridToExcel(EmployeesDataGrid, columnsToExclude, "EmployeesExport", "Employees List")
        End Sub

        Private Sub EditEmployee_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedEmployee As Employee = CType(EmployeesDataGrid.SelectedItem, Employee)

            If selectedEmployee IsNot Nothing Then
                EditEmployeeService.SelectedEmployee = EmployeeController.GetEmployeeInfo(selectedEmployee.EmployeeID)
                ViewLoader.DynamicView.NavigateToView("hrmeditemployee", Me)
            Else
                MessageBox.Show("Please select an employee first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub

        Private Sub DeleteEmployee_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedEmployee As Employee = CType(EmployeesDataGrid.SelectedItem, Employee)
            If selectedEmployee Is Nothing Then Return

            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            If recentlyClosed Then
                recentlyClosed = False
                Return
            End If

            If popup IsNot Nothing AndAlso popup.IsOpen Then
                popup.IsOpen = False
                recentlyClosed = True
                Return
            End If

            ' Create the modal
            Dim deleteModal As New DPC.Components.ConfirmationModals.HRMDeleteEmployee(selectedEmployee)

            ' Handle refresh on success
            AddHandler deleteModal.DeletedEmployee, AddressOf LoadEmployees

            ' Setup Popup
            popup = New Popup With {
                .Child = deleteModal,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .Placement = PlacementMode.Absolute
            }

            Dim modalWidth As Double = 400
            Dim modalHeight As Double = 300

            popup.HorizontalOffset = (SystemParameters.PrimaryScreenWidth - modalWidth) / 2
            popup.VerticalOffset = (SystemParameters.PrimaryScreenHeight - modalHeight) / 2

            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            popup.IsOpen = True
        End Sub
    End Class
End Namespace