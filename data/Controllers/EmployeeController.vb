Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Security.Cryptography
Imports System.Text
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.HRM.Employees.Employees ' Import PBKDF2Hasher

Namespace DPC.Data.Controllers
    Public Class EmployeeController
        ' Function to create a new employee
        Public Shared Function CreateEmployee(emp As Employee) As Boolean
            ' Generate the custom Employee ID
            emp.EmployeeID = GenerateEmployeeID()

            ' Hash the password before storing
            Dim hashedPassword As String = PBKDF2Hasher.HashPassword(emp.Password)

            Dim query As String = "INSERT INTO employee (EmployeeID, Username, Email, Password, UserRoleID, BusinessLocationID, Name, " &
              "StreetAddress, City, Region, Country, PostalCode, Phone, Salary, SalesCommission, Department, CreatedAt, UpdatedAt) " &
              "VALUES (@EmployeeID, @Username, @Email, @Password, @UserRoleID, @BusinessLocationID, @Name, @StreetAddress, " &
              "@City, @Region, @Country, @PostalCode, @Phone, @Salary, @SalesCommission, @Department, @CreatedAt, @UpdatedAt)"

            ' Get a new connection from the pool
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@EmployeeID", emp.EmployeeID)
                        cmd.Parameters.AddWithValue("@Username", emp.Username)
                        cmd.Parameters.AddWithValue("@Email", emp.Email)
                        cmd.Parameters.AddWithValue("@Password", hashedPassword) ' Storing hashed password
                        cmd.Parameters.AddWithValue("@UserRoleID", emp.UserRoleID)
                        cmd.Parameters.AddWithValue("@BusinessLocationID", emp.BusinessLocationID)
                        cmd.Parameters.AddWithValue("@Name", emp.Name)
                        cmd.Parameters.AddWithValue("@StreetAddress", emp.StreetAddress)
                        cmd.Parameters.AddWithValue("@City", emp.City)
                        cmd.Parameters.AddWithValue("@Region", emp.Region)
                        cmd.Parameters.AddWithValue("@Country", emp.Country)
                        cmd.Parameters.AddWithValue("@PostalCode", emp.PostalCode)
                        cmd.Parameters.AddWithValue("@Phone", emp.Phone)
                        cmd.Parameters.AddWithValue("@Salary", emp.Salary)
                        cmd.Parameters.AddWithValue("@SalesCommission", emp.SalesCommission)
                        cmd.Parameters.AddWithValue("@Department", emp.Department)
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now)

                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error creating employee: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

        ' Function to generate EmployeeID in format 10MMDDYYYYXXXX
        Private Shared Function GenerateEmployeeID() As String
            Dim prefix As String = "10"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextEmployeeCounter(datePart)

            ' Format counter to be 4 digits (e.g., 0001, 0025, 0150)
            Dim counterPart As String = counter.ToString("D4")

            ' Concatenate to get full EmployeeID
            Return prefix & datePart & counterPart
        End Function

        ' Fetch all user roles
        Public Shared Function GetUserRoles() As List(Of KeyValuePair(Of Integer, String))
            Dim roles As New List(Of KeyValuePair(Of Integer, String))
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT RoleID, RoleName FROM userroles"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                roles.Add(New KeyValuePair(Of Integer, String)(reader.GetInt32(0), reader.GetString(1)))
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error fetching user roles: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return roles
        End Function

        ' Fetch all business locations
        Public Class Location
            Public Property Key As Integer
            Public Property Value As String
        End Class

        Public Shared Function GetBusinessLocations() As List(Of KeyValuePair(Of Integer, String))
            Dim locations As New List(Of KeyValuePair(Of Integer, String))
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT locationID, locationName FROM businesslocation"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                locations.Add(New KeyValuePair(Of Integer, String)(reader.GetInt32(0), reader.GetString(1)))
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error fetching business locations: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return locations
        End Function

        Public Shared Function GetDepartments() As List(Of String)
            Dim department As New List(Of String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT departmentName FROM departments"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                department.Add(reader.GetString(0))
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error fetching business locations: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return department
        End Function

        ' Get Employee Info with Role and Location
        Public Shared Function GetEmployeeByID(employeeID As String) As Employee
            Dim employee As Employee = Nothing
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT e.*, r.RoleName, l.LocationName FROM employee e " &
                                          "JOIN userroles r ON e.UserRoleID = r.RoleID " &
                                          "JOIN businesslocation l ON e.businessLocationID = l.LocationID " &
                                          "WHERE e.EmployeeID = @EmployeeID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                employee = New Employee() With {
                                    .EmployeeID = reader("EmployeeID").ToString(),
                                    .Username = reader("Username").ToString(),
                                    .Email = reader("Email").ToString(),
                                    .Name = reader("Name").ToString(),
                                    .RoleName = reader("RoleName").ToString(),
                                    .LocationName = reader("LocationName").ToString(),
                                    .CreatedAt = Convert.ToDateTime(reader("CreatedAt")),
                                    .UpdatedAt = Convert.ToDateTime(reader("UpdatedAt"))
                                }
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error fetching employee: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return employee
        End Function

        ' Function to get the next Employee counter (last 4 digits) with reset condition
        Private Shared Function GetNextEmployeeCounter(datePart As String) As Integer
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(EmployeeID, 11, 4) AS UNSIGNED)) FROM employee " &
                          "WHERE EmployeeID LIKE '10" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()

                        ' If no previous records exist for today, start with 0001
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating Employee ID: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function

        ' Get the specific employee by EmployeeID
        Public Shared Function GetEmployeeInfo(employeeID As String) As Employee
            Dim employee As Employee = Nothing
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT * FROM employee WHERE EmployeeID = @EmployeeID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                employee = New Employee() With {
                                    .EmployeeID = reader("EmployeeID").ToString(),
                                    .Username = reader("Username").ToString(),
                                    .Email = reader("Email").ToString(),
                                    .Password = reader("Password").ToString(),
                                    .UserRoleID = Convert.ToInt32(reader("UserRoleID")),
                                    .BusinessLocationID = Convert.ToInt32(reader("BusinessLocationID")),
                                    .Name = reader("Name").ToString(),
                                    .StreetAddress = reader("StreetAddress").ToString(),
                                    .City = reader("City").ToString(),
                                    .Region = reader("Region").ToString(),
                                    .Country = reader("Country").ToString(),
                                    .PostalCode = reader("PostalCode").ToString(),
                                    .Phone = reader("Phone").ToString(),
                                    .Salary = Convert.ToDecimal(reader("Salary")),
                                    .SalesCommission = Convert.ToDecimal(reader("SalesCommission")),
                                    .Department = reader("Department").ToString(),
                                    .CreatedAt = Convert.ToDateTime(reader("CreatedAt")),
                                    .UpdatedAt = Convert.ToDateTime(reader("UpdatedAt"))
                                }
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error fetching employee: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return employee
        End Function

        ' Editing the employee info
        Public Shared Function EditEmployeeInfo(username As String,
                                                business As Integer,
                                                userrole As Integer,
                                                email As String,
                                                password As String,
                                                fullname As String,
                                                street As String,
                                                city As String,
                                                region As String,
                                                country As String,
                                                postalcode As String,
                                                phone As String,
                                                salary As Decimal,
                                                salarycommission As Decimal,
                                                Department As String) As Boolean
            Try
                Dim newPasswordHasged As String = Nothing
                Dim query As String

                ' Check if the password is empty
                If String.IsNullOrEmpty(password) Then
                    query = "
                                            UPDATE employee SET 
                                                Username = @Username,
                                                Email = @Email,
                                                UserRoleID = @UserRoleID,
                                                BusinessLocationID = @BusinessLocationID,
                                                Name = @Name,
                                                StreetAddress = @StreetAddress,
                                                City = @City,
                                                Region = @Region,
                                                Country = @Country,
                                                PostalCode = @PostalCode,
                                                Phone = @Phone,
                                                Salary = @Salary,
                                                SalesCommission = @SalesCommission,
                                                Department = @Department,
                                                UpdatedAt = CURRENT_TIMESTAMP
                                            WHERE EmployeeID = @EmployeeID
                                        "
                Else
                    newPasswordHasged = PBKDF2Hasher.HashPassword(password)
                    query = "
                                            UPDATE employee SET 
                                                Username = @Username,
                                                Email = @Email,
                                                Password = @Password,
                                                UserRoleID = @UserRoleID,
                                                BusinessLocationID = @BusinessLocationID,
                                                Name = @Name,
                                                StreetAddress = @StreetAddress,
                                                City = @City,
                                                Region = @Region,
                                                Country = @Country,
                                                PostalCode = @PostalCode,
                                                Phone = @Phone,
                                                Salary = @Salary,
                                                SalesCommission = @SalesCommission,
                                                Department = @Department,
                                                UpdatedAt = CURRENT_TIMESTAMP
                                            WHERE EmployeeID = @EmployeeID
                                        "
                End If

                Console.WriteLine($"Password Changed: {newPasswordHasged}")
                Console.WriteLine($"Executing query: {query}")

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@Username", username)
                        cmd.Parameters.AddWithValue("@Email", email)
                        If Not String.IsNullOrEmpty(password) Then
                            cmd.Parameters.AddWithValue("@Password", newPasswordHasged) ' Use hashed password
                        End If
                        cmd.Parameters.AddWithValue("@UserRoleID", userrole)
                        cmd.Parameters.AddWithValue("@BusinessLocationID", business)
                        cmd.Parameters.AddWithValue("@Name", fullname)
                        cmd.Parameters.AddWithValue("@StreetAddress", street)
                        cmd.Parameters.AddWithValue("@City", city)
                        cmd.Parameters.AddWithValue("@Region", region)
                        cmd.Parameters.AddWithValue("@Country", country)
                        cmd.Parameters.AddWithValue("@PostalCode", postalcode)
                        cmd.Parameters.AddWithValue("@Phone", phone)
                        cmd.Parameters.AddWithValue("@Salary", salary)
                        cmd.Parameters.AddWithValue("@SalesCommission", salarycommission)
                        cmd.Parameters.AddWithValue("@Department", Department)
                        cmd.Parameters.AddWithValue("@EmployeeID", EditEmployeeService.SelectedEmployee.EmployeeID)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                Return True
            Catch ex As Exception
                MessageBox.Show($"Error Editing employee: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        Public Shared Function DeleteEmployee(employeeID As String) As Boolean
            Try
                Dim query As String = "DELETE FROM employee WHERE EmployeeID = @EmployeeID"
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error deleting employee: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function
    End Class
End Namespace
