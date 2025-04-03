

Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Public Class AddAccount

        Public Sub New()
            InitializeComponent()
            GetBusinessLocations()
            LoadAccountTypes()
        End Sub
        Public Shared Function GetBusinessLocations() As Dictionary(Of Integer, String)
            Dim locations As New Dictionary(Of Integer, String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT locationID, locationName FROM businessLocation"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                locations.Add(reader.GetInt32(0), reader.GetString(1))
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error fetching business locations: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return locations
        End Function



        Private Sub LoadAccountTypes()
            ' Same as before: fill the account types
            Dim accountTypes = [Enum].GetValues(GetType(AccountType))
            cmbAccountType.ItemsSource = accountTypes
            cmbAccountType.SelectedIndex = 0
        End Sub

        Private Sub BtnAddAccount_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim selectedAccountType As AccountType = CType([Enum].Parse(GetType(AccountType),
                                                    cmbAccountType.SelectedItem.ToString()), AccountType)

                Dim sanitizedBalance As New String(txtInitialBalance.Text.
                    Where(Function(c) Char.IsDigit(c) OrElse c = "."c).ToArray())

                Dim initialBalance As Decimal
                If Not Decimal.TryParse(sanitizedBalance, initialBalance) Then
                    MessageBox.Show("Please enter a valid initial balance.",
                                    "Input Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                Dim selectedLocationID As Integer = CInt(cmbBusinessLocation.SelectedValue)

                Dim newAccount As New Account() With {
                    .AccountName = txtName.Text,
                    .AccountNo = txtAccountNo.Text,
                    .AccountType = selectedAccountType,
                    .InitialBalance = initialBalance,
                    .Note = txtNote.Text,
                    .BusinessLocation = selectedLocationID
                }

                If AccountController.CreateAccount(newAccount) Then
                    MessageBox.Show("Account added successfully!",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Else
                    MessageBox.Show("Failed to add account.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message,
                                "Input Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class
End Namespace