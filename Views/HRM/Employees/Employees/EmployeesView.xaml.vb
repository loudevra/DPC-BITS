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
            LoadEmployees()

            ' Initialize Pagination
            UpdatePagination()
        End Sub

        ''' <summary>
        ''' Load Employees from Database
        ''' </summary>
        Private Sub LoadEmployees()
            Employees.Clear()

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT e.*, r.RoleName, l.LocationName FROM employee e " &
                                          "JOIN userroles r ON e.UserRoleID = r.RoleID " &
                                          "JOIN businesslocation l ON e.BusinessLocationID = l.LocationID " &
                                          "ORDER BY e.CreatedAt DESC"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Employees.Add(New Employee() With {
                                    .EmployeeID = reader("EmployeeID").ToString(),
                                    .Username = reader("Username").ToString(),
                                    .Department = reader("Department").ToString(),
                                    .Status = reader("Status").ToString(),
                                    .Email = reader("Email").ToString(),
                                    .Name = reader("Name").ToString(),
                                    .RoleName = reader("RoleName").ToString(),
                                    .LocationName = reader("LocationName").ToString(),
                                    .CreatedAt = Convert.ToDateTime(reader("CreatedAt")),
                                    .UpdatedAt = Convert.ToDateTime(reader("UpdatedAt"))
                                })
                            End While
                        End Using
                    End Using
                End Using

                ' Refresh DataGrid
                EmployeesDataGrid.ItemsSource = Employees

            Catch ex As Exception
                MessageBox.Show($"Error loading employees: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ''' <summary>
        ''' View Employee Details
        ''' </summary>
        Private Sub ViewEmployee(sender As Object, e As RoutedEventArgs)
            Dim selectedEmployee As Employee = CType(EmployeesDataGrid.SelectedItem, Employee)
            If selectedEmployee IsNot Nothing Then
                MessageBox.Show($"Employee: {selectedEmployee.Name}" & vbCrLf &
                                $"Role: {selectedEmployee.RoleName}" & vbCrLf &
                                $"Location: {selectedEmployee.LocationName}", "Employee Info", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub

        ''' <summary>
        ''' Update Pagination Display
        ''' </summary>
        Private Sub UpdatePagination()
            TxtPageNumber.Text = $"Page {CurrentPage} of {TotalPages}"
            BtnFirstPage.IsEnabled = (CurrentPage > 1)
            BtnPrevPage.IsEnabled = (CurrentPage > 1)
            BtnNextPage.IsEnabled = (CurrentPage < TotalPages)
            BtnLastPage.IsEnabled = (CurrentPage < TotalPages)
        End Sub

        ''' <summary>
        ''' Pagination Controls
        ''' </summary>
        Private Sub BtnFirstPage_Click(sender As Object, e As RoutedEventArgs)
            CurrentPage = 1
            UpdatePagination()
        End Sub

        Private Sub BtnPrevPage_Click(sender As Object, e As RoutedEventArgs)
            If CurrentPage > 1 Then
                CurrentPage -= 1
                UpdatePagination()
            End If
        End Sub

        Private Sub BtnNextPage_Click(sender As Object, e As RoutedEventArgs)
            If CurrentPage < TotalPages Then
                CurrentPage += 1
                UpdatePagination()
            End If
        End Sub

        Private Sub BtnLastPage_Click(sender As Object, e As RoutedEventArgs)
            CurrentPage = TotalPages
            UpdatePagination()
        End Sub

        ''' <summary>
        ''' Open Add Employee View
        ''' </summary>
        Private Sub AddEmployee(sender As Object, e As RoutedEventArgs)
            ' Navigate to AddEmployee view using ViewLoader
            ViewLoader.DynamicView.NavigateToView("addnewemployee", Me)
        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim searchText As String = txtSearch.Text.Trim()
            SearchEmployee(searchText)
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

                ' TODO: bind dt to your result display control like a DataGrid or ListView
                EmployeesDataGrid.ItemsSource = dt.DefaultView

            Catch ex As Exception
                MessageBox.Show("Search error: " & ex.Message)
            Finally
                conn.Close()
            End Try
        End Sub

        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            ' Check if DataGrid has data
            If EmployeesDataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Create a list of column headers to exclude
            Dim columnsToExclude As New List(Of String) From {"Actions", "Status"}
            ' Use the ExcelExporter helper with column exclusions
            ExcelExporter.ExportDataGridToExcel(EmployeesDataGrid, columnsToExclude, "EmployeesExport", "Employees List")
        End Sub

        Private Sub EditEmployee_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedEmployee As Employee = CType(EmployeesDataGrid.SelectedItem, Employee)

            If selectedEmployee IsNot Nothing Then
                ' Grab the full employee details from the database using their ID
                EditEmployeeService.SelectedEmployee = EmployeeController.GetEmployeeInfo(selectedEmployee.EmployeeID)
                ViewLoader.DynamicView.NavigateToView("hrmeditemployee", Me)
            Else
                MessageBox.Show("Please select an employee first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub

        Private Sub DeleteEmployee_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedEmployee As Employee = CType(EmployeesDataGrid.SelectedItem, Employee)

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

            ' Create the user control
            Dim deleteModal As New DPC.Components.ConfirmationModals.HRMDeleteEmployee(selectedEmployee)

            ' Handle event
            AddHandler deleteModal.DeletedEmployee, AddressOf LoadEmployees

            ' Set popup manually in the center
            popup = New Popup With {
                .Child = deleteModal,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .Placement = PlacementMode.Absolute
            }

            ' Size of the modal
            Dim modalWidth As Double = 400 ' or actual width of your UserControl
            Dim modalHeight As Double = 300 ' or actual height of your UserControl

            popup.HorizontalOffset = (SystemParameters.PrimaryScreenWidth - modalWidth) / 2
            popup.VerticalOffset = (SystemParameters.PrimaryScreenHeight - modalHeight) / 2

            ' Close logic
            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            popup.IsOpen = True
        End Sub
    End Class
End Namespace