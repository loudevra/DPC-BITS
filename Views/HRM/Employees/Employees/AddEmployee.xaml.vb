Imports System.Text.RegularExpressions
Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model

Namespace DPC.Views.HRM.Employees.Employees

    Public Class AddEmployee
        Inherits UserControl
        ' Event to notify parent when employee is added
        Public Event EmployeeAdded(employee As Employee)

        Public Sub New()
            InitializeComponent()
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

                ' Clear the form for next entry
                ClearForm()

                ' Raise event to notify parent
                RaiseEvent EmployeeAdded(newEmployee)
            Else
                MessageBox.Show("Failed to add employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        Private Sub ClearForm()
            ' Clear all form fields
            txtUsername.Clear()
            txtEmail.Clear()
            txtPassword.Clear()
            txtName.Clear()
            txtStreetAddress.Clear()
            txtCity.Clear()
            txtRegion.Clear()
            txtCountry.Clear()
            txtPostalCode.Clear()
            txtPhone.Clear()
            txtSalary.Clear()
            txtSalesCommission.Clear()
            txtDepartment.Clear()

            ' Reset comboboxes to first item
            If cmbUserRole.Items.Count > 0 Then cmbUserRole.SelectedIndex = 0
            If cmbBusinessLocation.Items.Count > 0 Then cmbBusinessLocation.SelectedIndex = 0

            ' Hide validation messages
            txtInvalidChars.Visibility = Visibility.Collapsed
            txtValidUsername.Visibility = Visibility.Collapsed
            chkLength.Visibility = Visibility.Collapsed
            chkMaxLength.Visibility = Visibility.Collapsed
            chkUpper.Visibility = Visibility.Collapsed
            chkLower.Visibility = Visibility.Collapsed
            chkSpecial.Visibility = Visibility.Collapsed
            chkNumber.Visibility = Visibility.Collapsed
        End Sub

        Private Sub TxtPassword_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim pwd As String = txtPassword.Text

            ' Check each condition
            Dim lengthValid As Boolean = pwd.Length >= 6
            Dim maxLengthValid As Boolean = pwd.Length < 20
            Dim hasUpper As Boolean = pwd.Any(Function(c) Char.IsUpper(c))
            Dim hasLower As Boolean = pwd.Any(Function(c) Char.IsLower(c))
            Dim hasSpecial As Boolean = pwd.Any(Function(c) Not Char.IsLetterOrDigit(c))
            Dim hasNumber As Boolean = pwd.Any(Function(c) Char.IsDigit(c))

            ' Show indicators next to each rule
            ShowIfSatisfied(chkLength, lengthValid)
            ShowIfSatisfied(chkMaxLength, maxLengthValid)
            ShowIfSatisfied(chkUpper, hasUpper)
            ShowIfSatisfied(chkLower, hasLower)
            ShowIfSatisfied(chkSpecial, hasSpecial)
            ShowIfSatisfied(chkNumber, hasNumber)
        End Sub

        Private Sub TxtUsername_TextChanged(sender As Object, e As TextChangedEventArgs)
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

            ' Toggle both TextBlocks
            txtInvalidChars.Visibility = If(isValid, Visibility.Collapsed, Visibility.Visible)
            txtValidUsername.Visibility = If(isValid, Visibility.Visible, Visibility.Collapsed)
        End Sub

        Private Sub ShowIfSatisfied(textBlock As TextBlock, isMet As Boolean)
            textBlock.Visibility = If(isMet, Visibility.Visible, Visibility.Collapsed)
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

        ' Public method to reset the form
        Public Sub ResetForm()
            ClearForm()
        End Sub
    End Class
End Namespace