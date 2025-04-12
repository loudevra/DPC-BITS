Imports System.Data
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient

Namespace DPC.Data.Controllers
    Public Class PromoCodeController
        Public Shared Function AddPromoCode(promo As PromoCode) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "INSERT INTO promocodes (Code, Amount, Quantity, ValidUntil, IsLinked, Account, Note, Status) " &
                                          "VALUES (@Code, @Amount, @Quantity, @ValidUntil, @IsLinked, @Account, @Note, @Status)"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@Code", promo.Code)
                        cmd.Parameters.AddWithValue("@Amount", promo.Amount)
                        cmd.Parameters.AddWithValue("@Quantity", promo.Quantity)
                        cmd.Parameters.AddWithValue("@ValidUntil", promo.ValidUntil)
                        cmd.Parameters.AddWithValue("@IsLinked", promo.IsLinked)
                        cmd.Parameters.AddWithValue("@Account", If(String.IsNullOrEmpty(promo.Account), DBNull.Value, promo.Account))
                        cmd.Parameters.AddWithValue("@Note", promo.Note)
                        cmd.Parameters.AddWithValue("@Status", "Active") ' Default status for new promo codes
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
                    Dim query As String = "SELECT ID, Code, Amount, Quantity, ValidUntil, IsLinked, Account, Note, " &
                                         "CASE " &
                                         "    WHEN ValidUntil < CURDATE() THEN 'Expired' " &
                                         "    WHEN Quantity <= 0 THEN 'Used' " &
                                         "    ELSE Status " &
                                         "END AS Status, " &
                                         "CreatedAt " &
                                         "FROM promocodes " &
                                         "ORDER BY CreatedAt DESC"
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

        Public Shared Function GetPromoCodeStats() As Dictionary(Of String, Integer)
            Dim stats As New Dictionary(Of String, Integer) From {
                {"Active", 0},
                {"Used", 0},
                {"Expired", 0},
                {"Total", 0}
            }

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Get Active count (valid date and quantity > 0)
                    Dim activeQuery As String = "SELECT COUNT(*) FROM promocodes WHERE ValidUntil >= CURDATE() AND Quantity > 0 AND Status = 'Active'"
                    Using activeCmd As New MySqlCommand(activeQuery, conn)
                        stats("Active") = Convert.ToInt32(activeCmd.ExecuteScalar())
                    End Using

                    ' Get Used count (quantity = 0)
                    Dim usedQuery As String = "SELECT COUNT(*) FROM promocodes WHERE Quantity <= 0"
                    Using usedCmd As New MySqlCommand(usedQuery, conn)
                        stats("Used") = Convert.ToInt32(usedCmd.ExecuteScalar())
                    End Using

                    ' Get Expired count (passed valid date)
                    Dim expiredQuery As String = "SELECT COUNT(*) FROM promocodes WHERE ValidUntil < CURDATE()"
                    Using expiredCmd As New MySqlCommand(expiredQuery, conn)
                        stats("Expired") = Convert.ToInt32(expiredCmd.ExecuteScalar())
                    End Using

                    ' Get Total count
                    Dim totalQuery As String = "SELECT COUNT(*) FROM promocodes"
                    Using totalCmd As New MySqlCommand(totalQuery, conn)
                        stats("Total") = Convert.ToInt32(totalCmd.ExecuteScalar())
                    End Using
                End Using

                Return stats
            Catch ex As Exception
                MessageBox.Show("Error getting promo code statistics: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return stats ' Return default values in case of error
            End Try
        End Function

        Public Shared Function UpdatePromoCode(promo As PromoCode) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "UPDATE promocodes SET " &
                                         "Code = @Code, " &
                                         "Amount = @Amount, " &
                                         "Quantity = @Quantity, " &
                                         "ValidUntil = @ValidUntil, " &
                                         "IsLinked = @IsLinked, " &
                                         "Account = @Account, " &
                                         "Note = @Note " &
                                         "WHERE ID = @ID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ID", promo.ID)
                        cmd.Parameters.AddWithValue("@Code", promo.Code)
                        cmd.Parameters.AddWithValue("@Amount", promo.Amount)
                        cmd.Parameters.AddWithValue("@Quantity", promo.Quantity)
                        cmd.Parameters.AddWithValue("@ValidUntil", promo.ValidUntil)
                        cmd.Parameters.AddWithValue("@IsLinked", promo.IsLinked)
                        cmd.Parameters.AddWithValue("@Account", If(String.IsNullOrEmpty(promo.Account), DBNull.Value, promo.Account))
                        cmd.Parameters.AddWithValue("@Note", promo.Note)

                        Return cmd.ExecuteNonQuery() > 0
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error updating promo code: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        Public Shared Function DeletePromoCode(id As Integer) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "DELETE FROM promocodes WHERE ID = @ID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ID", id)
                        Return cmd.ExecuteNonQuery() > 0
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error deleting promo code: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        Public Shared Function GetPromoCodeById(id As Integer) As PromoCode
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT * FROM promocodes WHERE ID = @ID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ID", id)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                Return New PromoCode With {
                                    .ID = Convert.ToInt32(reader("ID")),
                                    .Code = reader("Code").ToString(),
                                    .Amount = Convert.ToDecimal(reader("Amount")),
                                    .Quantity = Convert.ToInt32(reader("Quantity")),
                                    .ValidUntil = Convert.ToDateTime(reader("ValidUntil")),
                                    .IsLinked = Convert.ToBoolean(reader("IsLinked")),
                                    .Account = If(reader("Account") Is DBNull.Value, String.Empty, reader("Account").ToString()),
                                    .Note = reader("Note").ToString(),
                                    .CreatedAt = Convert.ToDateTime(reader("CreatedAt"))
                                }
                            End If
                        End Using
                    End Using
                End Using
                Return Nothing ' Return null if promo code not found
            Catch ex As Exception
                MessageBox.Show("Error retrieving promo code: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return Nothing
            End Try
        End Function
    End Class
End Namespace