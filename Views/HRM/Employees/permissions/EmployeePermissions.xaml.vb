Imports System.Collections.ObjectModel
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Model

Namespace DPC.Views.HRM.Employees.Permissions
    Class EmployeePermissions
        Private CurrentPage As Integer = 1
        Private EmployeesPerPage As Integer = 10
        Private Employees As ObservableCollection(Of Employee)
        Private FilteredEmployees As ObservableCollection(Of Employee)

        Public Sub New()
            InitializeComponent()
            LoadEmployees()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar
        End Sub

        Private Sub LoadEmployees()
            ' Simulating Data - Replace this with actual data retrieval from MySQL
            Employees = New ObservableCollection(Of Employee) From {
                New Employee With {.Id = 1, .Name = "John Doe", .Role = "Inventory Manager"},
                New Employee With {.Id = 2, .Name = "Jane Smith", .Role = "Sales Person"},
                New Employee With {.Id = 3, .Name = "Michael Brown", .Role = "Sales Manager"},
                New Employee With {.Id = 4, .Name = "Sarah Johnson", .Role = "Business Manager"},
                New Employee With {.Id = 5, .Name = "Robert Wilson", .Role = "Business Owner"},
                New Employee With {.Id = 6, .Name = "Emily Davis", .Role = "Project Manager"}
            }

            ' Convert roles to boolean properties
            For Each emp In Employees
                emp.IsInventoryManager = (emp.Role = "Inventory Manager")
                emp.IsSalesPerson = (emp.Role = "Sales Person")
                emp.IsSalesManager = (emp.Role = "Sales Manager")
                emp.IsBusinessManager = (emp.Role = "Business Manager")
                emp.IsBusinessOwner = (emp.Role = "Business Owner")
                emp.IsProjectManager = (emp.Role = "Project Manager")
            Next

            ApplyPagination()
        End Sub

        Private Sub ApplyPagination()
            Dim pagedData = Employees.Skip((CurrentPage - 1) * EmployeesPerPage).Take(EmployeesPerPage)
            FilteredEmployees = New ObservableCollection(Of Employee)(pagedData)
            PermissionsDataGrid.ItemsSource = FilteredEmployees
            UpdatePagination()
        End Sub

        Private Sub UpdatePagination()
            Dim totalPages As Integer = Math.Ceiling(Employees.Count / EmployeesPerPage)
            PageInfoText.Text = $"{CurrentPage} / {If(totalPages = 0, 1, totalPages)}"
        End Sub

        Private Sub PrevPage_Click(sender As Object, e As RoutedEventArgs)
            If CurrentPage > 1 Then
                CurrentPage -= 1
                ApplyPagination()
            End If
        End Sub

        Private Sub NextPage_Click(sender As Object, e As RoutedEventArgs)
            Dim totalPages As Integer = Math.Ceiling(Employees.Count / EmployeesPerPage)
            If CurrentPage < totalPages Then
                CurrentPage += 1
                ApplyPagination()
            End If
        End Sub

        Private Sub UpdatePermissions_Click(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Permissions Updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub
    End Class
End Namespace
