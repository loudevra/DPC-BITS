Imports System.Diagnostics.Metrics
Imports MySql.Data.MySqlClient
Public Class NewSuppliers
    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs)
        InsertSupplierData()
    End Sub
    Private Sub InsertSupplierData()
        Dim connectionString As String = "server=localhost;userid=root;password=;database=dpc"


        If String.IsNullOrWhiteSpace(CompanyRepresentative.Text) OrElse
           String.IsNullOrWhiteSpace(Company.Text) OrElse
           String.IsNullOrWhiteSpace(Phone.Text) OrElse
           String.IsNullOrWhiteSpace(Email.Text) OrElse
           String.IsNullOrWhiteSpace(CompanyAddress.Text) OrElse
           String.IsNullOrWhiteSpace(City.Text) OrElse
           String.IsNullOrWhiteSpace(Region.Text) OrElse
           String.IsNullOrWhiteSpace(Country.Text) OrElse
           String.IsNullOrWhiteSpace(PostalCode.Text) OrElse
           String.IsNullOrWhiteSpace(TinId.Text) Then

            MessageBox.Show("Please fill in all required fields!", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            Exit Sub
        End If

        Using conn As New MySqlConnection(connectionString)
            Try
                conn.Open()

                Dim query As String = "INSERT INTO supplier (representative, companyName, phoneNumber, email, officeAddress, city, region, country, postalcode, tinId) 
                                   VALUES (@representative, @companyName, @phoneNumber, @email, @officeAddress, @city, @region, @country, @postalcode, @tinId)"

                Using cmd As New MySqlCommand(query, conn)

                    cmd.Parameters.AddWithValue("@representative", CompanyRepresentative.Text)
                    cmd.Parameters.AddWithValue("@companyName", Company.Text)
                    cmd.Parameters.AddWithValue("@phoneNumber", Phone.Text)
                    cmd.Parameters.AddWithValue("@email", Email.Text)
                    cmd.Parameters.AddWithValue("@officeAddress", CompanyAddress.Text)
                    cmd.Parameters.AddWithValue("@city", City.Text)
                    cmd.Parameters.AddWithValue("@region", Region.Text)
                    cmd.Parameters.AddWithValue("@country", Country.Text)
                    cmd.Parameters.AddWithValue("@postalcode", PostalCode.Text)
                    cmd.Parameters.AddWithValue("@tinId", TinId.Text)


                    cmd.ExecuteNonQuery()


                    MessageBox.Show("Supplier added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End Using

            Catch ex As MySqlException
                MessageBox.Show("Database Error: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)

            Catch ex As Exception
                MessageBox.Show("Unexpected Error: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Using
    End Sub


End Class
