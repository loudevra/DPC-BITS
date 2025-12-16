Imports System.Windows.Markup
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMEditResidentialClientPersonalInfo
        Private _clientID As String

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Sub New(clientID As String)
            InitializeComponent()
            _clientID = clientID
            GetInfo()

            AddHandler txtName.TextChanged, AddressOf SetInfo
            AddHandler txtPhone.TextChanged, AddressOf SetInfo
            AddHandler txtEmail.TextChanged, AddressOf SetInfo
        End Sub

        Private Sub SetInfo()
            ResidentialClientDetails.ClientName = txtName.Text
            ResidentialClientDetails.Phone = txtPhone.Text
            ResidentialClientDetails.Email = txtEmail.Text
        End Sub

        Private Sub GetInfo()
            Try
                Dim client As Client = ClientController.GetClientByID(_clientID)
                If client IsNot Nothing Then
                    txtName.Text = client.Name
                    txtPhone.Text = client.Phone
                    txtEmail.Text = client.Email

                    ResidentialClientDetails.ClientName = client.Name
                    ResidentialClientDetails.Phone = client.Phone
                    ResidentialClientDetails.Email = client.Email
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Error loading personal info: {ex.Message}")
            End Try
        End Sub

        Private Sub UpdateCRMClient(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(txtName.Text) OrElse
               String.IsNullOrWhiteSpace(txtPhone.Text) OrElse
               String.IsNullOrWhiteSpace(txtEmail.Text) Then
                MessageBox.Show("Please fill in all personal information fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Try
                ' Get existing client from database
                Dim client As Client = ClientController.GetClientByID(_clientID)
                If client Is Nothing Then
                    MessageBox.Show("Client not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Update only personal information fields
                client.Name = txtName.Text
                client.Phone = txtPhone.Text
                client.Email = txtEmail.Text

                ' Save to existing ClientID
                Dim success As Boolean = ClientController.UpdateClient(client)

                If success Then
                    MessageBox.Show("Personal information updated successfully!")
                    ResidentialClientDetails.ClientName = client.Name
                    ResidentialClientDetails.Phone = client.Phone
                    ResidentialClientDetails.Email = client.Email
                Else
                    MessageBox.Show("Failed to update personal information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Error updating personal info: {ex.Message}")
                MessageBox.Show($"Error updating personal information: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub txtInput_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            ' Regex allows digits, symbols, and space
            Dim pattern As String = "^[0-9!@#$%^&*()_\-+=\.,:;?/ ]$"
            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, pattern) Then
                e.Handled = True
            End If
        End Sub
    End Class
End Namespace