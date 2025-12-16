Imports ClosedXML.Excel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMEditResidentialClientOtherSettings
        Private _clientID As String

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Sub New(clientID As String)
            InitializeComponent()
            _clientID = clientID

            LoadCustomerGroups()
            GetInfo()

            AddHandler cmbCustomerGroup.SelectionChanged, AddressOf SetInfo
            AddHandler cmbLanguage.SelectionChanged, AddressOf SetInfo
        End Sub

        Private Sub LoadCustomerGroups()
            Dim customerGroups = ClientGroupController.GetCustomerGroup()
            cmbCustomerGroup.DisplayMemberPath = "Value"
            cmbCustomerGroup.SelectedValuePath = "Key"
            cmbCustomerGroup.ItemsSource = customerGroups
        End Sub

        Private Sub SetInfo()
            Try
                Dim selectedGroup = CType(cmbCustomerGroup.SelectedItem, KeyValuePair(Of Integer, String))
                Dim selectedItem As ComboBoxItem = TryCast(cmbLanguage.SelectedItem, ComboBoxItem)

                ResidentialClientDetails.ClientGroupID = CInt(cmbCustomerGroup.SelectedValue)

                If String.IsNullOrWhiteSpace(selectedGroup.Value) Then
                    ResidentialClientDetails.CustomerGroup = ""
                Else
                    ResidentialClientDetails.CustomerGroup = selectedGroup.Value.ToString()
                End If

                ResidentialClientDetails.CustomerLanguage = If(selectedItem?.Content IsNot Nothing, selectedItem.Content.ToString(), cmbLanguage.Text)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Error in SetInfo: {ex.Message}")
            End Try
        End Sub

        Private Sub GetInfo()
            Try
                Dim client As Client = ClientController.GetClientByID(_clientID)
                If client IsNot Nothing Then
                    cmbCustomerGroup.SelectedValue = client.ClientGroupID
                    cmbLanguage.Text = client.ClientLanguage

                    ResidentialClientDetails.ClientGroupID = client.ClientGroupID
                    ResidentialClientDetails.CustomerGroup = client.CustomerGroup
                    ResidentialClientDetails.CustomerLanguage = client.ClientLanguage
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Error loading other settings: {ex.Message}")
            End Try
        End Sub

        Private Sub UpdateCRMClient(sender As Object, e As RoutedEventArgs)
            Try
                ' Get existing client from database
                Dim client As Client = ClientController.GetClientByID(_clientID)
                If client Is Nothing Then
                    MessageBox.Show("Client not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Update other settings fields
                Dim selectedGroup = CType(cmbCustomerGroup.SelectedItem, KeyValuePair(Of Integer, String))
                Dim selectedItem As ComboBoxItem = TryCast(cmbLanguage.SelectedItem, ComboBoxItem)

                client.ClientGroupID = CInt(cmbCustomerGroup.SelectedValue)
                client.CustomerGroup = If(selectedGroup.Value Is Nothing, "", selectedGroup.Value.ToString())
                client.ClientLanguage = If(selectedItem?.Content IsNot Nothing, selectedItem.Content.ToString(), "English")

                ' Save to existing ClientID
                Dim success As Boolean = ClientController.UpdateClient(client)

                If success Then
                    MessageBox.Show("Settings updated successfully!")
                    ResidentialClientDetails.ClientGroupID = client.ClientGroupID
                    ResidentialClientDetails.CustomerGroup = client.CustomerGroup
                    ResidentialClientDetails.CustomerLanguage = client.ClientLanguage
                Else
                    MessageBox.Show("Failed to update settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Error updating settings: {ex.Message}")
                MessageBox.Show($"Error updating settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class
End Namespace