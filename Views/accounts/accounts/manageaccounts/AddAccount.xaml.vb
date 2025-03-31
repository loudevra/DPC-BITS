

Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Public Class AddAccount

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BtnAddAccount_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim newAccount As New Account() With {
                .Name = txtName.Text,
                .AccountNo = txtAccountNo.Text,
                .AccountType = cmbAccountType.Text,
                .InitialBalance = Decimal.Parse(txtInitialBalance.Text),
                .Note = txtNote.Text,
                .BusinessLocation = cmbBusinessLocation.Text
            }

                If AccountController.CreateAccount(newAccount) Then
                    MessageBox.Show("Account added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Else
                    MessageBox.Show("Failed to add account.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message, "Input Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class
End Namespace