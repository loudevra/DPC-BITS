Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient

Namespace DPC.Data.Controllers
    Public Class BrandController
        Public Shared Function GetBrands() As ObservableCollection(Of Brand)
            Dim brandList As New ObservableCollection(Of Brand)()
            Dim query As String = "SELECT brandID, brandname FROM brand;"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                brandList.Add(New Brand With {
                                    .ID = reader.GetInt32("brandid"), ' Changed to GetInt32 for auto-increment
                                    .Name = reader.GetString("brandname")
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

        Public Shared Sub InsertBrand(brandName As String)
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

                        ' Safely check for NULL and convert to Int32
                        Dim result As Object = checkCmd.ExecuteScalar()
                        Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)

                        If count > 0 Then
                            MessageBox.Show("Brand already exists.")
                            Return
                        End If
                    End Using

                    ' Insert brand without BrandID
                    Dim query As String = "INSERT INTO brand (BrandName) VALUES (@BrandName)"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@BrandName", brandName)
                        cmd.ExecuteNonQuery()
                    End Using

                    MessageBox.Show("Brand added successfully!")
                End Using
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
            End Try
        End Sub
    End Class
End Namespace
