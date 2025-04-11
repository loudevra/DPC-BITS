Imports System.Data
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient

Namespace DPC.Data.Controllers
    Public Class PromoCodeController
        Public Shared Function AddPromoCode(promo As PromoCode) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "INSERT INTO promocodes (Code, Amount, Quantity, ValidUntil, IsLinked, Account, Note) " &
                                          "VALUES (@Code, @Amount, @Quantity, @ValidUntil, @IsLinked, @Account, @Note)"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@Code", promo.Code)
                        cmd.Parameters.AddWithValue("@Amount", promo.Amount)
                        cmd.Parameters.AddWithValue("@Quantity", promo.Quantity)
                        cmd.Parameters.AddWithValue("@ValidUntil", promo.ValidUntil)
                        cmd.Parameters.AddWithValue("@IsLinked", promo.IsLinked)
                        cmd.Parameters.AddWithValue("@Account", If(String.IsNullOrEmpty(promo.Account), DBNull.Value, promo.Account))
                        cmd.Parameters.AddWithValue("@Note", promo.Note)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
                Return True
            Catch ex As Exception
                MessageBox.Show("Error adding promo code: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function


        Public Shared Function FetchPromoCodes() As DataTable
            Try
                Dim dt As New DataTable()
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT Code, Amount, Quantity, ValidUntil, IsLinked, Account, Note FROM promocodes"
                    Using cmd As New MySqlCommand(query, conn)
                        Using adapter As New MySqlDataAdapter(cmd)
                            adapter.Fill(dt)
                        End Using
                    End Using
                End Using
                Return dt
            Catch ex As Exception
                MessageBox.Show("Error fetching promo codes: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return Nothing
            End Try
        End Function

    End Class
End Namespace
