Imports System.Text.RegularExpressions
Imports System.Windows
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model

Namespace DPC.Views.HRM.Employees
    Public Class AddEmployee
        Public Sub New()
            InitializeComponent()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Child = topNavBar

            LoadUserRoles()
            LoadBusinessLocations()
        End Sub
        Private Sub BtnAddEmployee_Click(sender As Object, e As RoutedEventArgs)
            ' Validate required fields
            If String.IsNullOrWhiteSpace(txtUsername.Text) OrElse
               String.IsNullOrWhiteSpace(txtEmail.Text) OrElse
               String.IsNullOrWhiteSpace(txtPassword.Password) OrElse
               String.IsNullOrWhiteSpace(txtName.Text) OrElse
               String.IsNullOrWhiteSpace(txtPhone.Text) Then

                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Email validation
            Dim emailPattern As String = "^[^@\s]+@[^@\s]+\.[^@\s]+$"
            If Not Regex.IsMatch(txtEmail.Text, emailPattern) Then
                MessageBox.Show("Invalid email format.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Salary & Sales Commission Parsing
            Dim salary As Decimal
            Dim salesCommission As Decimal

            If Not Decimal.TryParse(txtSalary.Text, salary) Then
                MessageBox.Show("Invalid salary amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            If Not Decimal.TryParse(txtSalesCommission.Text, salesCommission) Then
                MessageBox.Show("Invalid sales commission amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Create new Employee object
            Dim newEmployee As New Employee With {
                .Username = txtUsername.Text,
                .Email = txtEmail.Text,
                .Password = txtPassword.Password, ' Hash before storing in production
                .UserRoleID = CType(cmbUserRole.SelectedValue, Integer),
                .BusinessLocationID = CType(cmbBusinessLocation.SelectedValue, Integer),
                .Name = txtName.Text,
                .StreetAddress = txtStreetAddress.Text,
                .City = txtCity.Text,
                .Region = txtRegion.Text,
                .Country = txtCountry.Text,
                .PostalCode = txtPostalCode.Text,
                .Phone = txtPhone.Text,
                .Salary = salary,
                .SalesCommission = salesCommission,
                .Department = txtDepartment.Text,
                .CreatedAt = DateTime.Now,
                .UpdatedAt = DateTime.Now
            }

            ' Insert into Database
            If EmployeeController.CreateEmployee(newEmployee) Then
                MessageBox.Show("Employee added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Me.Close() ' Close the form
            Else
                MessageBox.Show("Failed to add employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub
        ' Load User Roles into ComboBox
        Private Sub LoadUserRoles()
            Dim roles = EmployeeController.GetUserRoles()
            cmbUserRole.ItemsSource = roles
            cmbUserRole.DisplayMemberPath = "Value"
            cmbUserRole.SelectedValuePath = "Key"
        End Sub

        ' Load Business Locations into ComboBox
        Private Sub LoadBusinessLocations()
            Dim locations = EmployeeController.GetBusinessLocations()
            cmbBusinessLocation.ItemsSource = locations
            cmbBusinessLocation.DisplayMemberPath = "Value"
            cmbBusinessLocation.SelectedValuePath = "Key"
        End Sub

    End Class
End Namespace
