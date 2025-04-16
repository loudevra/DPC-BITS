Imports System.Collections.ObjectModel
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports System.Data

Namespace DPC.Views.HRM.Employees.Employees
    Partial Public Class EmployeesView
        Inherits Window

        Private Employees As New ObservableCollection(Of Employee)()

        ' Declare Pagination Variables
        Private CurrentPage As Integer = 1
        Private TotalPages As Integer = 1
        Private PageSize As Integer = 10

        Public Sub New()
            InitializeComponent()
            LoadEmployees()

            Dim topNavBar As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Child = topNavBar

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

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
        ''' Open Add Employee Window
        ''' </summary>
        Private Sub AddEmployee(sender As Object, e As RoutedEventArgs)
            Dim addEmployeeWindow As New AddEmployee()
            addEmployeeWindow.Show()
            Me.Close()
        End Sub

        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim searchText As String = txtSearch.Text.Trim()
            SearchEmployee(searchText)
        End Sub

        Private Sub SearchEmployee(query As String)
            Dim conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
            Try
                conn.Open()
                Dim sql As String = "SELECT * FROM employee WHERE EmployeeID LIKE @query OR Username LIKE @query"
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

    End Class
End Namespace
