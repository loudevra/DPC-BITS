Imports System.Text.RegularExpressions
Imports System.Windows
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Components.Forms

Namespace DPC.Views.HRM.Employees
    Public Class AddEmployee
        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            LoadUserRoles()
            LoadBusinessLocations()
        End Sub
        Private Sub BtnAddEmployee_Click(sender As Object, e As RoutedEventArgs)
            ' Validate required fields
            If String.IsNullOrWhiteSpace(txtUsername.Text) OrElse
               String.IsNullOrWhiteSpace(txtEmail.Text) OrElse
               String.IsNullOrWhiteSpace(txtPassword.Text) OrElse
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
                .Password = txtPassword.Text, ' Hash before storing in production
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
                Dim employeesWindow As New Views.HRM.Employees.Employees.EmployeesView()
                employeesWindow.Show()
                Me.Close() ' Close the form
            Else
                MessageBox.Show("Failed to add employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        Private Sub txtPassword_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim pwd As String = txtPassword.Text

            ' Show each rule only when satisfied
            ShowIfSatisfied(chkLength, pwd.Length >= 6)
            ShowIfSatisfied(chkMaxLength, pwd.Length < 20)
            ShowIfSatisfied(chkUpper, pwd.Any(Function(c) Char.IsUpper(c)))
            ShowIfSatisfied(chkLower, pwd.Any(Function(c) Char.IsLower(c)))
            ShowIfSatisfied(chkSpecial, pwd.Any(Function(c) Not Char.IsLetterOrDigit(c)))
            ShowIfSatisfied(chkNumber, pwd.Any(Function(c) Char.IsDigit(c)))
        End Sub
        Private Sub txtUsername_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim input As String = txtUsername.Text

            ' Check each condition
            Dim hasMinLength As Boolean = input.Length >= 6
            Dim hasMaxLength As Boolean = input.Length < 20
            Dim hasUppercase As Boolean = input.Any(Function(c) Char.IsUpper(c))
            Dim hasLowercase As Boolean = input.Any(Function(c) Char.IsLower(c))
            Dim hasSpecialChar As Boolean = input.Any(Function(c) Not Char.IsLetterOrDigit(c))
            Dim hasDigit As Boolean = input.Any(Function(c) Char.IsDigit(c))

            ' Combine all checks
            Dim isValid As Boolean = hasMinLength AndAlso hasMaxLength AndAlso
                             hasUppercase AndAlso hasLowercase AndAlso
                             hasSpecialChar AndAlso hasDigit

            ' Show red error if NOT valid
            txtInvalidChars.Visibility = If(isValid, Visibility.Collapsed, Visibility.Visible)
        End Sub




        Private Sub ShowIfSatisfied(textBlock As TextBlock, isMet As Boolean)
            textBlock.Visibility = If(isMet, Visibility.Visible, Visibility.Collapsed)
        End Sub

        Private Sub ToggleCriteria(textBlock As TextBlock, isMet As Boolean)
            textBlock.Visibility = If(isMet, Visibility.Collapsed, Visibility.Visible)
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
