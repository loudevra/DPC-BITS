Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Models

Namespace DPC.Data.Controllers
    Public Class AccountController
        ' Function to create a new account
        Public Shared Function CreateAccount(acc As Account) As Boolean
            ' Generate the custom Account ID
            acc.AccountID = GenerateAccountID()

            Dim query As String = "INSERT INTO Accounts (AccountID, AccountName, AccountType, CreatedAt, UpdatedAt) " &
                                  "VALUES (@AccountID, @AccountName, @AccountType, @CreatedAt, @UpdatedAt)"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@AccountID", acc.AccountID)
                        cmd.Parameters.AddWithValue("@AccountName", acc.AccountName)
                        cmd.Parameters.AddWithValue("@AccountType", acc.AccountType)
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now)

                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error creating account: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

        ' Function to fetch all accounts
        Public Shared Function GetAllAccounts() As ObservableCollection(Of Account)
            Dim accounts As New ObservableCollection(Of Account)
            Dim query As String = "SELECT * FROM Accounts"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim account As New Account With {
                                    .AccountID = reader("AccountID").ToString(),
                                    .AccountName = reader("AccountName").ToString(),
                                    .AccountType = reader("AccountType").ToString(),
                                    .CreatedAt = Convert.ToDateTime(reader("CreatedAt")),
                                    .UpdatedAt = Convert.ToDateTime(reader("UpdatedAt"))
                                }
                                accounts.Add(account)
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching accounts: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return accounts
        End Function

        ' Function to generate AccountID in format 60MMDDYYYYXXXX
        Private Shared Function GenerateAccountID() As String
            Dim prefix As String = "60"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy")
            Dim counter As Integer = GetNextAccountCounter(datePart)
            Dim counterPart As String = counter.ToString("D4")
            Return prefix & datePart & counterPart
        End Function

        ' Function to get the next Account counter
        Private Shared Function GetNextAccountCounter(datePart As String) As Integer
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(AccountID, 11, 4) AS UNSIGNED)) FROM Accounts WHERE AccountID LIKE '13" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating Account ID: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function
    End Class
End Namespace
