Imports System.Windows
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model

Namespace DPC.Views.HRM.Employees
    Public Class AddEmployee
        Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
            InitializeComponent()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Child = topNavBar

            ' Populate User Roles Dropdown
            cmbUserRole.ItemsSource = GetUserRoles()
            cmbUserRole.DisplayMemberPath = "RoleName"
            cmbUserRole.SelectedValuePath = "UserRoleID"

            ' Populate Business Locations Dropdown
            cmbBusinessLocation.ItemsSource = GetBusinessLocations()
            cmbBusinessLocation.DisplayMemberPath = "LocationName"
            cmbBusinessLocation.SelectedValuePath = "BusinessLocationID"
        End Sub

        ' Function to fetch User Roles
        Private Function GetUserRoles() As List(Of Object)
            Dim roles As New List(Of Object)
            Using conn As MySql.Data.MySqlClient.MySqlConnection = SplashScreen.GetDatabaseConnection()
                Dim query As String = "SELECT UserRoleID, RoleName FROM UserRoles"
                Dim cmd As New MySql.Data.MySqlClient.MySqlCommand(query, conn)
                Try
                    conn.Open()
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            roles.Add(New With {
                                .UserRoleID = reader("UserRoleID"),
                                .RoleName = reader("RoleName").ToString()
                            })
                        End While
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error loading roles: " & ex.Message)
                End Try
            End Using
            Return roles
        End Function

        ' Function to fetch Business Locations
        Private Function GetBusinessLocations() As List(Of Object)
            Dim locations As New List(Of Object)
            Using conn As MySql.Data.MySqlClient.MySqlConnection = SplashScreen.GetDatabaseConnection()
                Dim query As String = "SELECT BusinessLocationID, LocationName FROM BusinessLocations"
                Dim cmd As New MySql.Data.MySqlClient.MySqlCommand(query, conn)
                Try
                    conn.Open()
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            locations.Add(New With {
                                .BusinessLocationID = reader("BusinessLocationID"),
                                .LocationName = reader("LocationName").ToString()
                            })
                        End While
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error loading locations: " & ex.Message)
                End Try
            End Using
            Return locations
        End Function

        ' Add Employee Button Click
        Private Sub BtnAddEmployee_Click(sender As Object, e As RoutedEventArgs)
            ' Validate Input
            If String.IsNullOrWhiteSpace(txtName.Text) OrElse
               String.IsNullOrWhiteSpace(txtEmail.Text) OrElse
               String.IsNullOrWhiteSpace(txtPhone.Text) OrElse
               String.IsNullOrWhiteSpace(txtStreetAddress.Text) OrElse
               cmbUserRole.SelectedValue Is Nothing OrElse
               cmbBusinessLocation.SelectedValue Is Nothing OrElse
               String.IsNullOrWhiteSpace(txtSalary.Text) OrElse
               String.IsNullOrWhiteSpace(txtDepartment.Text) Then

                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Create Employee Object
            Dim newEmployee As New Employee With {
                .Username = txtEmail.Text.Split("@"c)(0), ' Generate username from email
                .Email = txtEmail.Text,
                .Password = "default123", ' Default password (must be changed later)
                .UserRoleID = Convert.ToInt32(cmbUserRole.SelectedValue),
                .BusinessLocationID = Convert.ToInt32(cmbBusinessLocation.SelectedValue),
                .Name = txtName.Text,
                .StreetAddress = txtStreetAddress.Text,
                .City = "N/A",
                .Region = "N/A",
                .Country = "N/A",
                .PostalCode = "0000",
                .Phone = txtPhone.Text,
                .Salary = Convert.ToDecimal(txtSalary.Text),
                .SalesCommission = 0,
                .Department = txtDepartment.Text,
                .CreatedAt = DateTime.Now,
                .UpdatedAt = DateTime.Now
            }

            ' Insert into Database
            If EmployeeController.CreateEmployee(newEmployee) Then
                MessageBox.Show("Employee added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Me.Close()
            Else
                MessageBox.Show("Failed to add employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub
    End Class
End Namespace
