Imports System.Collections.ObjectModel
Imports System.Data
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient
Imports OpenTK.Graphics.ES11

Namespace DPC.Data.Controllers
    Public Class HRMController
        Public Shared Function LoadDepartment(dataGrid As DataGrid) As Boolean
            Try
                Dim query As String = "SELECT departmentID AS DepartmentID, departmentName AS DepartmentName FROM departments"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    Using cmd As New MySqlCommand(query, conn)
                        Using adapter As New MySqlDataAdapter(cmd)
                            Dim dt As New DataTable()
                            adapter.Fill(dt)

                            ' Assign data to DataGrid
                            dataGrid.ItemsSource = dt.DefaultView
                        End Using
                    End Using
                End Using

                Return True
            Catch ex As Exception
                MessageBox.Show($"An error occurred while loading department data: {ex.Message}")
                Return False
            End Try
        End Function

        Public Shared Function InsertDepartment(departmentName As String) As Boolean
            Try
                If String.IsNullOrWhiteSpace(departmentName) Then
                    MessageBox.Show("Department name cannot be empty.")
                    Return False
                End If

                ' Checking for Existing Department DepartmentName query
                Dim CheckDepartmentQuery As String = "SELECT COUNT(*) FROM departments WHERE departmentName = @departmentName"
                ' Inserting Department Query
                Dim InsertDepartmentQuery As String = "INSERT INTO departments(departmentName, dateCreated, dateModified) VALUES (@departmentName, NOW(), NOW())"
                ' Store the RoleID
                Dim RoleID As Integer = 0 ' Default Value Whenever the there is an error

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using transaction As MySqlTransaction = conn.BeginTransaction
                        Try
                            ' Query command for checking the existing Department DepartmentName
                            Using CheckDepartmentCmd As New MySqlCommand(CheckDepartmentQuery, conn, transaction)
                                CheckDepartmentCmd.Parameters.AddWithValue("@departmentName", departmentName)
                                Dim result = CheckDepartmentCmd.ExecuteScalar()
                                Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)

                                If count > 0 Then
                                    MessageBox.Show("Department name already exists.")
                                    Return False
                                    Exit Function
                                End If
                            End Using

                            ' Query command for inserting the Department if not exist
                            Using InsertDepartmentCmd As New MySqlCommand(InsertDepartmentQuery, conn, transaction)
                                InsertDepartmentCmd.Parameters.AddWithValue("@departmentName", departmentName)
                                InsertDepartmentCmd.ExecuteNonQuery()
                            End Using

                            transaction.Commit()
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show($"Error in Inserting Department - {ex.Message}")
                            Throw
                        End Try
                    End Using
                End Using
                Return True
            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Shared Function EditDepartments(departmentID As Integer, departmentName As String) As Boolean
            Try
                If String.IsNullOrWhiteSpace(departmentName) Then
                    MessageBox.Show("Department name cannot be empty.")
                    Return False
                End If
                ' Check for duplicate department name (excluding the current one)
                Dim CheckExistingDepartmentQuery As String = "SELECT COUNT(*) FROM departments WHERE departmentName = @departmentName AND departmentID <> @departmentID"
                Dim UpdateDepartmentQuery As String = "UPDATE departments SET departmentName = @departmentName, dateModified = NOW() WHERE departmentID = @departmentID"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            ' Check for duplication
                            Using CheckExistingDepartmentCmd As New MySqlCommand(CheckExistingDepartmentQuery, conn, transaction)
                                CheckExistingDepartmentCmd.Parameters.AddWithValue("@departmentName", departmentName)
                                CheckExistingDepartmentCmd.Parameters.AddWithValue("@departmentID", departmentID)
                                Dim result As Object = CheckExistingDepartmentCmd.ExecuteScalar()
                                Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)
                                If count > 0 Then
                                    MessageBox.Show("Department name already exists.")
                                    Return False
                                End If
                            End Using

                            ' Proceed to update
                            Using UpdateDepartmentCmd As New MySqlCommand(UpdateDepartmentQuery, conn, transaction)
                                UpdateDepartmentCmd.Parameters.AddWithValue("@departmentName", departmentName)
                                UpdateDepartmentCmd.Parameters.AddWithValue("@departmentID", departmentID)
                                UpdateDepartmentCmd.ExecuteNonQuery()
                            End Using

                            transaction.Commit()
                            'MessageBox.Show("Successfully Updated")
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show($"Error in Updating Department - {ex.Message}")
                            Throw
                        End Try
                    End Using
                End Using
                Return True
            Catch ex As Exception
                MessageBox.Show($"Error in Updating Root Department - {ex.Message}")
                Return False
            End Try
        End Function

        Public Shared Function DeleteDepartment(departmentID As Integer) As Boolean
            Try

                ' Query for Deleting the department
                Dim DeleteDepartmentQuery = "DELETE FROM departments WHERE departmentID = @departmentID"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            Using DeleteDepartmentCmd As New MySqlCommand(DeleteDepartmentQuery, conn, transaction)
                                DeleteDepartmentCmd.Parameters.AddWithValue("@departmentID", departmentID)
                                DeleteDepartmentCmd.ExecuteNonQuery()

                                transaction.Commit()
                            End Using
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show($"Error in Deleting Department - {ex.Message}")
                            Return False
                        End Try
                    End Using
                End Using
                Return True
            Catch ex As Exception
                MessageBox.Show($"Error in Deleting Root Department - {ex.Message}")
                Return False
            End Try
        End Function
    End Class
End Namespace