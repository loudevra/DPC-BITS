Imports DocumentFormat.OpenXml.Wordprocessing
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model

Namespace DPC.Views.HRM.Employees.Employees
    ' Static Class to hold the selected employee for editing
    Public Class EditEmployee
        Public Sub New()
            InitializeComponent()
            ' Loads data into ComboBoxes and initializes the form
            LoadUserRoles()
            LoadBusinessLocations()
            LoadDepartments()

            Dim emp As Employee = EditEmployeeService.SelectedEmployee

            ' Initialize the form with employee details if provided
            txtUsername.Text = If(emp?.Username, String.Empty)
            cmbBusinessLocation.SelectedValue = If(emp?.BusinessLocationID, 0)
            cmbUserRole.SelectedValue = If(emp?.UserRoleID, 0)
            txtEmail.Text = If(emp?.Email, String.Empty)
            txtName.Text = If(emp?.Name, String.Empty)
            txtStreetAddress.Text = If(emp?.StreetAddress, String.Empty)
            txtCity.Text = If(emp?.City, String.Empty)
            txtRegion.Text = If(emp?.Region, String.Empty)
            txtCountry.Text = If(emp?.Country, String.Empty)
            txtPostalCode.Text = If(emp?.PostalCode, String.Empty)
            txtPhone.Text = If(emp?.Phone, String.Empty)
            txtSalary.Text = If(emp?.Salary.ToString(), String.Empty)
            txtSalesCommission.Text = If(emp?.SalesCommission.ToString(), String.Empty)
            cmbDepartments.SelectedValue = If(emp?.Department, String.Empty)
        End Sub

#Region "Functions for loads"
        Private Sub LoadUserRoles()
            Dim roles = EmployeeController.GetUserRoles()
            cmbUserRole.DisplayMemberPath = "Value"
            cmbUserRole.SelectedValuePath = "Key"
            cmbUserRole.ItemsSource = roles
        End Sub

        ' Load Business Locations into ComboBox
        Private Sub LoadBusinessLocations()
            Dim locations = EmployeeController.GetBusinessLocations()
            cmbBusinessLocation.DisplayMemberPath = "Value"
            cmbBusinessLocation.SelectedValuePath = "Key"
            cmbBusinessLocation.ItemsSource = locations
        End Sub


        Private Sub LoadDepartments()
            Dim departments = EmployeeController.GetDepartments()
            cmbDepartments.ItemsSource = departments
        End Sub

        Private Function InsertDB() As Boolean
            Dim businessLocationID As Integer = If(cmbBusinessLocation.SelectedValue IsNot Nothing, Convert.ToInt32(cmbBusinessLocation.SelectedValue), 0)
            Dim userRoleID As Integer = If(cmbUserRole.SelectedValue IsNot Nothing, Convert.ToInt32(cmbUserRole.SelectedValue), 0)
            Dim updatedSalary As Decimal
            If Not Decimal.TryParse(txtSalary.Text, updatedSalary) Then updatedSalary = 0

            Dim updatedCommission As Decimal
            If Not Decimal.TryParse(txtSalesCommission.Text, updatedCommission) Then updatedCommission = 0

            Dim updatedDepartment As String = If(cmbDepartments.SelectedValue IsNot Nothing, cmbDepartments.SelectedValue.ToString(), String.Empty)

            Console.WriteLine($"{businessLocationID}")
            Console.WriteLine($"{userRoleID}")
            Dim Success As Boolean = EmployeeController.EditEmployeeInfo(txtUsername.Text, businessLocationID, userRoleID, txtEmail.Text, txtPassword.Text, txtName.Text, txtStreetAddress.Text, txtCity.Text, txtRegion.Text, txtCountry.Text, txtPostalCode.Text, txtPhone.Text, updatedSalary, updatedCommission, updatedDepartment)

            Try
                If Success Then
                    MessageBox.Show("Employee information updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    ViewLoader.DynamicView.NavigateToView("viewemployee", Me)
                End If
                Return Success
            Catch ex As Exception
                Return Success
            End Try
        End Function
#End Region

        Private Sub btnEditEmployee_Click(sender As Object, e As RoutedEventArgs)
            InsertDB()
        End Sub
    End Class
End Namespace