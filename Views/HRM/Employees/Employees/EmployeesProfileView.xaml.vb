Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.DPC.Data.Helpers

Namespace DPC.Views.HRM.Employees.Employees

    Public Class EmployeesProfileView
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub EmployeesProfileView_Loaded(sender As Object, e As RoutedEventArgs) _
            Handles Me.Loaded
            LoadEmployee(EmployeeProfileService.SelectedEmployee)
        End Sub

        Public Sub LoadEmployee(employee As Employee)
            If employee Is Nothing Then Return

            ' Left panel
            txtEmployeeName.Text = If(String.IsNullOrWhiteSpace(employee.Name), "—", employee.Name)
            txtPosition.Text = If(String.IsNullOrWhiteSpace(employee.RoleName), "—", employee.RoleName)
            txtDepartment.Text = If(String.IsNullOrWhiteSpace(employee.Department), "—", employee.Department)

            ' Status badge
            If employee.Status?.ToUpper() = "INACTIVE" Then
                StatusBadge.Background = New System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(250, 219, 216))
                StatusDot.Foreground = New System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(192, 57, 43))
                StatusLabel.Foreground = New System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(192, 57, 43))
                StatusLabel.Text = "INACTIVE"
            Else
                StatusBadge.Background = New System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(167, 225, 131))
                StatusDot.Foreground = New System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(69, 107, 46))
                StatusLabel.Foreground = New System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(69, 107, 46))
                StatusLabel.Text = "ACTIVE"
            End If

            ' Right panel
            txtProfileUsername.Text = If(String.IsNullOrWhiteSpace(employee.Username), "—", employee.Username)
            txtProfileEmail.Text = If(String.IsNullOrWhiteSpace(employee.Email), "—", employee.Email)
            txtProfilePhone.Text = If(String.IsNullOrWhiteSpace(employee.Phone), "—", employee.Phone)
            txtProfileLocation.Text = If(String.IsNullOrWhiteSpace(employee.LocationName), "—", employee.LocationName)

            Dim parts As New List(Of String)
            If Not String.IsNullOrWhiteSpace(employee.StreetAddress) Then parts.Add(employee.StreetAddress)
            If Not String.IsNullOrWhiteSpace(employee.City) Then parts.Add(employee.City)
            If Not String.IsNullOrWhiteSpace(employee.Region) Then parts.Add(employee.Region)
            If Not String.IsNullOrWhiteSpace(employee.Country) Then parts.Add(employee.Country)
            If Not String.IsNullOrWhiteSpace(employee.PostalCode) Then parts.Add(employee.PostalCode)
            txtProfileAddress.Text = If(parts.Count > 0, String.Join(", ", parts), "—")

            txtProfileSalary.Text = If(employee.Salary = 0, "N/A", "₱" & employee.Salary.ToString("N2"))
            txtProfileCommission.Text = If(employee.SalesCommission = 0, "N/A", employee.SalesCommission.ToString("N2") & "%")

            txtProfileCreatedAt.Text = employee.CreatedAt.ToString("MMMM dd, yyyy")
            txtProfileUpdatedAt.Text = employee.UpdatedAt.ToString("MMMM dd, yyyy")
        End Sub

    End Class

End Namespace