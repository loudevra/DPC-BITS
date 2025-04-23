Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Models

Namespace DPC.Data.Controllers
    Public Class AccountController
        Public Shared Reload As Boolean

        ' Function to create a new account
        Public Shared Function CreateAccount(acc As Account) As Boolean
            acc.AccountID = GenerateAccountID()

            ' Log the generated ID for debugging
            Console.WriteLine($"Generated AccountID: {acc.AccountID}, Length: {acc.AccountID.Length}")

            Dim query As String = "INSERT INTO accounts (AccountID, AccountName, AccountNo, AccountType, InitialBalance, Note, BusinessLocation, CreatedAt, UpdatedAt) " &
                          "VALUES (@AccountID, @AccountName, @AccountNo, @AccountType, @InitialBalance, @Note, @BusinessLocation, @CreatedAt, @UpdatedAt)"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        ' Convert possible null values to DBNull.Value for the database
                        cmd.Parameters.AddWithValue("@AccountID", acc.AccountID)
                        cmd.Parameters.AddWithValue("@AccountName", If(String.IsNullOrEmpty(acc.AccountName), DBNull.Value, acc.AccountName))
                        cmd.Parameters.AddWithValue("@AccountNo", If(String.IsNullOrEmpty(acc.AccountNo), DBNull.Value, acc.AccountNo))
                        cmd.Parameters.AddWithValue("@AccountType", acc.AccountType.ToString())
                        cmd.Parameters.AddWithValue("@InitialBalance", acc.InitialBalance)
                        cmd.Parameters.AddWithValue("@Note", If(String.IsNullOrEmpty(acc.Note), DBNull.Value, acc.Note))
                        cmd.Parameters.AddWithValue("@BusinessLocation", If(String.IsNullOrEmpty(acc.BusinessLocation), DBNull.Value, acc.BusinessLocation))
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now)

                        ' Log all parameter values for debugging
                        Console.WriteLine("All parameter values:")
                        For Each param As MySqlParameter In cmd.Parameters
                            Console.WriteLine($"{param.ParameterName}: {param.Value}, Type: {param.Value.GetType().Name}")
                        Next

                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0
                    End Using

                Catch ex As MySqlException
                    ' Specific handling for MySQL exceptions with more detailed error info
                    Console.WriteLine($"MySQL Error Number: {ex.Number}")
                    Console.WriteLine($"MySQL Error Message: {ex.Message}")
                    Console.WriteLine($"MySQL Error State: {ex.SqlState}")
                    MessageBox.Show($"Database Error: {ex.Message} (Error Code: {ex.Number})", "MySQL Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                Catch ex As Exception
                    ' General exception handling
                    Console.WriteLine($"Exception Type: {ex.GetType().Name}")
                    Console.WriteLine($"Exception Message: {ex.Message}")
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}")
                    MessageBox.Show("Error creating account: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

        ' Function to fetch all accounts
        Public Shared Function GetAllAccounts() As ObservableCollection(Of Account)
            Dim accounts As New ObservableCollection(Of Account)
            Dim query As String = "SELECT * FROM accounts"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim account As New Account With {
                                    .AccountID = reader("AccountID").ToString(),
                                    .AccountName = reader("AccountName").ToString(),
                                    .AccountNo = If(reader("AccountNo") IsNot DBNull.Value, reader("AccountNo").ToString(), ""),
                                    .AccountType = CType([Enum].Parse(GetType(AccountType), reader("AccountType").ToString()), AccountType),
                                    .InitialBalance = If(reader("InitialBalance") IsNot DBNull.Value, Convert.ToDecimal(reader("InitialBalance")), 0),
                                    .Note = If(reader("Note") IsNot DBNull.Value, reader("Note").ToString(), ""),
                                    .BusinessLocation = If(reader("BusinessLocation") IsNot DBNull.Value, reader("BusinessLocation").ToString(), ""),
                                    .CreatedAt = Convert.ToDateTime(reader("CreatedAt")),
                                    .UpdatedAt = Convert.ToDateTime(reader("UpdatedAt"))
                                }
                                accounts.Add(account)
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching accounts: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Console.WriteLine($"Exception details: {ex}")
                End Try
            End Using

            Return accounts
        End Function

        ' Function to generate AccountID in format 60MMDDYYYYXXXX
        Private Shared Function GenerateAccountID() As String
            Dim prefix As String = "60"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy")

            ' Get the counter with error handling
            Dim counter As Integer = 0
            Try
                counter = GetNextAccountCounter(datePart)
                Console.WriteLine($"Generated counter: {counter}")
            Catch ex As Exception
                Console.WriteLine($"Error in GetNextAccountCounter: {ex.Message}")
                counter = 1
            End Try

            Dim counterPart As String = counter.ToString("D4")
            Dim accountId As String = prefix & datePart & counterPart

            ' Ensure the ID is not exceeding length limits
            If accountId.Length > 20 Then
                Console.WriteLine($"Warning: AccountID exceeds 20 characters: {accountId}")
            End If

            Return accountId
        End Function

        ' Function to get the next Account counter with improved error handling
        Private Shared Function GetNextAccountCounter(datePart As String) As Integer
            ' Safe SQL query using parameterized query
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(AccountID, 11, 4) AS UNSIGNED)) FROM accounts WHERE AccountID LIKE @Pattern"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        ' Use parameter to prevent SQL injection
                        cmd.Parameters.AddWithValue("@Pattern", "60" & datePart & "%")

                        Dim result As Object = cmd.ExecuteScalar()
                        Console.WriteLine($"Counter query result: {If(result Is Nothing, "NULL", result.ToString())}")

                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            ' No existing accounts for today
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    Console.WriteLine($"Error in getting next counter: {ex.Message}")
                    MessageBox.Show("Error generating Account ID: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    ' Return a default value in case of error
                    Return 1
                End Try
            End Using
        End Function
    End Class
End Namespace