

Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Public Class AddAccount

        Public Sub New()
            InitializeComponent()
            LoadBusinessLocations()
            LoadAccountTypes()
        End Sub

        Private Sub LoadBusinessLocations()
            Try
                ' Suppose GetBusinessLocations() returns an ObservableCollection(Of KeyValuePair(Of Integer,String))
                Dim locations As ObservableCollection(Of KeyValuePair(Of Integer, String)) = WarehouseController.GetBusinessLocations()

                ' Bind directly to the ComboBox, specifying that:
                '  – The string in each KeyValuePair will be displayed
                '  – The Key (integer) in each KeyValuePair is the SelectedValue
                cmbBusinessLocation.ItemsSource = locations
                cmbBusinessLocation.DisplayMemberPath = "Value"
                cmbBusinessLocation.SelectedValuePath = "Key"

                If locations.Count > 0 Then
                    cmbBusinessLocation.SelectedIndex = 0
                End If
            Catch ex As Exception
                MessageBox.Show("Error loading business locations: " & ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

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

                ' Sanitize & parse the balance
                Dim sanitizedBalance As New String(txtInitialBalance.Text.
                    Where(Function(c) Char.IsDigit(c) OrElse c = "."c).ToArray())

                Dim initialBalance As Decimal
                If Not Decimal.TryParse(sanitizedBalance, initialBalance) Then
                    MessageBox.Show("Please enter a valid initial balance.",
                                    "Input Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Get the selected location ID from SelectedValue 
                '   (this is now an Integer thanks to SelectedValuePath)
                Dim selectedLocationID As Integer = CInt(cmbBusinessLocation.SelectedValue)

                Dim newAccount As New Account() With {
                    .Name = txtName.Text,
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