Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Controllers
    Public Class EmployeeController
        ' Function to create a new employee
        Public Shared Function CreateEmployee(emp As Employee) As Boolean
            ' Generate the custom Employee ID
            emp.EmployeeID = GenerateEmployeeID()

            Dim query As String = "INSERT INTO Employee (EmployeeID, Username, Email, Password, UserRoleID, BusinessLocationID, Name, " &
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
                        cmd.Parameters.AddWithValue("@Password", emp.Password) ' Hash before storing
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
            End Using ' ✅ Connection is automatically returned to the pool
        End Function

        ' Function to generate EmployeeID in format 10MMDDYYYYXXXX
        Private Shared Function GenerateEmployeeID() As String
            Dim prefix As String = "10"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextEmployeeCounter()

            ' Format counter to be 4 digits (e.g., 0001, 0025, 0150)
            Dim counterPart As String = counter.ToString("D4")

            ' Concatenate to get full EmployeeID
            Return prefix & datePart & counterPart
        End Function

        ' Function to get the next Employee counter (last 4 digits)
        Private Shared Function GetNextEmployeeCounter() As Integer
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(EmployeeID, 11, 4) AS UNSIGNED)) FROM Employee WHERE EmployeeID LIKE '10" & DateTime.Now.ToString("MMddyyyy") & "%'"

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
    End Class
End Namespace
