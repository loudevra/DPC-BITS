Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Accounts.Accounts.NewAccount
    Public Class AddAccount
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            LoadBusinessLocations()
        End Sub

        ' Load business locations from the database
        Private Sub LoadBusinessLocations()
            Try
                Dim locations = BusinessLocationController.GetAllLocations()
                cmbBusinessLocation.ItemsSource = locations
                cmbBusinessLocation.DisplayMemberPath = "LocationName"
                cmbBusinessLocation.SelectedValuePath = "LocationID"
            Catch ex As Exception
                MessageBox.Show("Error loading business locations: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Declare a custom event to notify the parent control
        Public Event AccountAdded()

        ' Add new account method
        Private Sub AddAccount(sender As Object, e As RoutedEventArgs)
            Try
                Dim accountNo As String = txtAccountNo.Text.Trim()
                Dim name As String = txtName.Text.Trim()
                Dim initialBalance As Decimal = Decimal.Parse(txtInitialBalance.Text)
                Dim note As String = txtNote.Text.Trim()
                Dim accountType As String = cmbAccountType.Text

                ' Get selected business location
                Dim selectedLocation As KeyValuePair(Of Integer, String) = CType(cmbBusinessLocation.SelectedItem, KeyValuePair(Of Integer, String))
                Dim businessLocationID As Integer = selectedLocation.Key

                Dim newAccount As New Account With {
            .AccountNo = accountNo,
            .Name = name,
            .InitialBalance = initialBalance,
            .Note = note,
            .BusinessLocation = businessLocationID,
            .AccountType = accountType
        }

                If AccountController.CreateAccount(newAccount) Then
                    AccountController.Reload = True
                    MessageBox.Show("Account added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    RaiseEvent AccountAdded() ' Notify the parent control
                Else
                    MessageBox.Show("Failed to add account.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Catch ex As Exception
                MessageBox.Show("Error adding account: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Close the popup
        Private Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            Try
                Dim parentWindow = Window.GetWindow(Me)
                parentWindow?.Close()
            Catch ex As Exception
                MessageBox.Show("Error closing window: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class
End Namespace
