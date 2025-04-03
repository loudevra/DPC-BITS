Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Public Class AddAccount
        Public Event AccountAdded As EventHandler

        Public Sub New()
            InitializeComponent()
            GetBusinessLocation(cmbBusinessLocation)
            LoadAccountTypes()
        End Sub

        Public Shared Sub GetBusinessLocation(comboBox As ComboBox)
            Dim query As String = "SELECT locationID, locationName FROM businesslocation"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()
                            While reader.Read()
                                Dim locationName As String = reader("locationName").ToString()
                                Dim locationID As String = reader("locationID").ToString()
                                Dim item As New ComboBoxItem With {
                                    .Content = locationName,
                                    .Tag = locationID
                                }
                                comboBox.Items.Add(item)
                            End While
                            comboBox.SelectedIndex = 0
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub

        Private Sub LoadAccountTypes()
            ' Same as before: fill the account types
            Dim accountTypes = [Enum].GetValues(GetType(AccountType))
            cmbAccountType.ItemsSource = accountTypes
            cmbAccountType.SelectedIndex = 0
        End Sub

        Private Sub BtnAddAccount_Click(sender As Object, e As RoutedEventArgs)
            Try
                ' Parse selected account type
                Dim selectedAccountType As AccountType = CType([Enum].Parse(GetType(AccountType), cmbAccountType.SelectedItem.ToString()), AccountType)

                ' Sanitize the input balance
                Dim sanitizedBalance As New String(txtInitialBalance.Text.Where(Function(c) Char.IsDigit(c) OrElse c = "."c).ToArray())

                ' Ensure the balance is not empty and sanitize it
                If String.IsNullOrEmpty(sanitizedBalance) Then
                    sanitizedBalance = "0.00" ' Default value if empty
                End If

                ' Ensure only one decimal point exists
                If sanitizedBalance.Count(Function(c) c = "."c) > 1 Then
                    MessageBox.Show("Please enter a valid initial balance (only one decimal point allowed).",
                            "Input Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Ensure there is no trailing decimal point (e.g., "10." should become "10.00")
                If sanitizedBalance.EndsWith(".") Then
                    sanitizedBalance &= "00" ' Append zeros to the decimal part
                End If

                ' Attempt to convert the sanitized balance to Decimal
                Dim initialBalance As Decimal
                If Not Decimal.TryParse(sanitizedBalance, initialBalance) Then
                    MessageBox.Show("Please enter a valid initial balance.",
                            "Input Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Round to two decimal places
                initialBalance = Math.Round(initialBalance, 2)

                ' Get selected location ID as a string
                Dim selectedItem As ComboBoxItem = TryCast(cmbBusinessLocation.SelectedItem, ComboBoxItem)

                Dim selectedLocationID As String = ""
                If selectedItem IsNot Nothing AndAlso selectedItem.Tag IsNot Nothing Then
                    selectedLocationID = selectedItem.Tag.ToString() ' Keep locationID as string
                Else
                    MessageBox.Show("Please select a business location.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Create the new account object
                Dim newAccount As New Account() With {
                    .AccountName = txtName.Text,
                    .AccountNo = txtAccountNo.Text,
                    .AccountType = selectedAccountType,
                    .InitialBalance = initialBalance,
                    .Note = txtNote.Text,
                    .BusinessLocation = selectedLocationID ' Store as string
                }

                ' Attempt to add the account
                If AccountController.CreateAccount(newAccount) Then
                    MessageBox.Show("Account added successfully!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                    RaiseEvent AccountAdded(Me, EventArgs.Empty)
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
