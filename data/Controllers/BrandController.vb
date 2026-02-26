Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient


Namespace DPC.Data.Controllers
    Public Class BrandController
        Public Shared Function GetBrands() As ObservableCollection(Of Brand)
            Dim brandList As New ObservableCollection(Of Brand)()
            Dim query As String = "SELECT b.brandID, b.BrandName, 
                            c.CategoryName AS Category,
                            (SELECT COUNT(*) FROM supplier WHERE brandID = b.brandID) AS TotalSupplier
                            FROM brand b
                            LEFT JOIN category c ON b.categoryID = c.categoryID"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                brandList.Add(New Brand With {
                            .ID = reader.GetInt32("brandID"),
                            .Name = reader.GetString("BrandName"),
                            .Category = If(reader.IsDBNull(reader.GetOrdinal("Category")), "", reader.GetString("Category")),
                            .TotalSupplier = reader.GetInt32("TotalSupplier")
                        })
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching brands: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return brandList
        End Function

        Public Shared Sub InsertBrand(brandName As String, categoryID As Integer)
            If String.IsNullOrWhiteSpace(brandName) Then
                MessageBox.Show("Brand name cannot be empty.")
                Return
            End If

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Check for duplicate brand
                    Dim checkQuery As String = "SELECT COUNT(*) FROM brand WHERE brandName = @BrandName"
                    Using checkCmd As New MySqlCommand(checkQuery, conn)
                        checkCmd.Parameters.AddWithValue("@BrandName", brandName)
                        Dim result As Object = checkCmd.ExecuteScalar()
                        Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)
                        If count > 0 Then
                            MessageBox.Show("Brand already exists.")
                            Return
                        End If
                    End Using

                    ' Insert brand with categoryID
                    Dim query As String = "INSERT INTO brand (BrandName, categoryID) VALUES (@BrandName, @CategoryID)"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@BrandName", brandName)
                        cmd.Parameters.AddWithValue("@CategoryID", categoryID)
                        cmd.ExecuteNonQuery()
                    End Using

                    MessageBox.Show("Brand added successfully!")
                End Using
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
            End Try
        End Sub

        ' Method to save brand (insert or update)
        Public Shared Sub SaveBrand(brandName As String, Optional brandId As Integer? = Nothing)
            If String.IsNullOrWhiteSpace(brandName) Then
                MessageBox.Show("Brand name cannot be empty.")
                Return
            End If

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    If brandId.HasValue AndAlso brandId.Value > 0 Then
                        ' UPDATE MODE
                        ' Check if brand exists
                        Dim existsQuery As String = "SELECT COUNT(*) FROM brand WHERE BrandID = @BrandID"
                        Using existsCmd As New MySqlCommand(existsQuery, conn)
                            existsCmd.Parameters.AddWithValue("@BrandID", brandId.Value)
                            Dim result As Object = existsCmd.ExecuteScalar()
                            Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)
                            If count = 0 Then
                                MessageBox.Show("Brand not found.")
                                Return
                            End If
                        End Using

                        ' Check for duplicate brand name (excluding current brand)
                        Dim checkQuery As String = "SELECT COUNT(*) FROM brand WHERE brandName = @BrandName AND BrandID <> @BrandID"
                        Using checkCmd As New MySqlCommand(checkQuery, conn)
                            checkCmd.Parameters.AddWithValue("@BrandName", brandName)
                            checkCmd.Parameters.AddWithValue("@BrandID", brandId.Value)
                            Dim result As Object = checkCmd.ExecuteScalar()
                            Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)
                            If count > 0 Then
                                MessageBox.Show("Brand name already exists.")
                                Return
                            End If
                        End Using

                        ' Update brand
                        Dim updateQuery As String = "UPDATE brand SET BrandName = @BrandName WHERE BrandID = @BrandID"
                        Using updateCmd As New MySqlCommand(updateQuery, conn)
                            updateCmd.Parameters.AddWithValue("@BrandName", brandName)
                            updateCmd.Parameters.AddWithValue("@BrandID", brandId.Value)
                            Dim rowsAffected As Integer = updateCmd.ExecuteNonQuery()

                            If rowsAffected > 0 Then
                                MessageBox.Show("Brand updated successfully!")
                            Else
                                MessageBox.Show("No changes were made.")
                            End If
                        End Using
                    Else
                        ' INSERT MODE
                        ' Check for duplicate brand
                        Dim checkQuery As String = "SELECT COUNT(*) FROM brand WHERE brandName = @BrandName"
                        Using checkCmd As New MySqlCommand(checkQuery, conn)
                            checkCmd.Parameters.AddWithValue("@BrandName", brandName)
                            Dim result As Object = checkCmd.ExecuteScalar()
                            Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)
                            If count > 0 Then
                                MessageBox.Show("Brand already exists.")
                                Return
                            End If
                        End Using

                        ' Insert brand
                        Dim insertQuery As String = "INSERT INTO brand (BrandName) VALUES (@BrandName)"
                        Using insertCmd As New MySqlCommand(insertQuery, conn)
                            insertCmd.Parameters.AddWithValue("@BrandName", brandName)
                            insertCmd.ExecuteNonQuery()
                        End Using
                        MessageBox.Show("Brand added successfully!")
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
            End Try

        End Sub


    End Class
End Namespace
