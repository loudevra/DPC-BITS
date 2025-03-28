Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class AddBrand
        Public Event BrandAdded()

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BtnAddBrand()
            InsertBrand(TxtBrand.Text)
            RaiseEvent BrandAdded() ' Notify that a brand was added
        End Sub

        Public Shared Sub InsertBrand(brandName As String)
            If String.IsNullOrWhiteSpace(brandName) Then
                MessageBox.Show("Brand name cannot be empty.")
                Return
            End If

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Check for duplicate brand
                    Dim checkQuery As String = "SELECT COUNT(*) FROM Brand WHERE BrandName = @BrandName"
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
                    Dim query As String = "INSERT INTO Brand (BrandName) VALUES (@BrandName)"
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
