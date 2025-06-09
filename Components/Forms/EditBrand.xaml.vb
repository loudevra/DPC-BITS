Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Views.Stocks.Suppliers.ManageBrands


Namespace DPC.Components.Forms
    Public Class EditBrand
        Public Event BrandAdded()
        Public brandID As Integer?
        Public manageBrands As ManageBrands

        Public Sub New()
            InitializeComponent()
        End Sub

        ' Method to save brand (insert or update)
        Public Sub SaveBrand(brandName As String)
            SaveBrand(brandName, Nothing)
        End Sub

        ' Add this at the top of your EditBrand class (outside any methods)
        Public Shared Event BrandDataChanged()

        Public Sub SaveBrand(brandName As String, brandId As Integer?)
            If String.IsNullOrWhiteSpace(brandName) Then
                MessageBox.Show("Brand name cannot be empty.")
                Return
            End If



            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    If brandId.HasValue And brandId.Value > 0 Then
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
                                ' Raise the event to refresh DataGrid
                                RaiseEvent BrandDataChanged()
                                ' Close the dialog/usercontrol
                                CloseEditDialog()
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
                        ' Raise the event to refresh DataGrid
                        RaiseEvent BrandDataChanged()
                        ' Close the dialog/usercontrol
                        CloseEditDialog()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
            End Try
        End Sub

        ' Helper method to close the edit dialog
        Private Sub CloseEditDialog()
            Try
                ' If this is in a Window
                Dim parentWindow As Window = Window.GetWindow(Me)
                If parentWindow IsNot Nothing Then
                    parentWindow.Close()
                End If

                ' If this is a UserControl that needs to be hidden
                Me.Visibility = Visibility.Collapsed
            Catch ex As Exception
                ' Handle any closing errors
            End Try
        End Sub

        Private Sub SaveBrand_Click(sender As Object, e As RoutedEventArgs)
            ' Get the brand name from your UI (adjust the control name as needed)
            Dim brandName As String = TxtBrand.Text ' Replace with your actual textbox name


            ' Call your SaveBrand method
            SaveBrand(brandName, brandID)

            ' Refreshes datagrid after update or add
            manageBrands.dataGrid.ItemsSource = Nothing
            manageBrands.InitializeControls()

        End Sub


    End Class
End Namespace
