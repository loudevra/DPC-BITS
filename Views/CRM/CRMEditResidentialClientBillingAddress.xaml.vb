Imports ClosedXML.Excel
Imports System.Windows
Imports System.Windows.Controls
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Namespace DPC.Views.CRM

    Partial Public Class CRMEditResidentialClientBillingAddress
        Private _clientID As String = ""

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Sub New(clientID As String)
            Me.New()
            _clientID = clientID
            ' Wire up button click manually
            AddHandler btnAddUpdate.Click, AddressOf UpdateCRMClient
            ' Load data after control is loaded
            AddHandler Me.Loaded, AddressOf UserControl_Loaded
        End Sub

        Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
            LoadData()
        End Sub

        Private Sub LoadData()
            Try
                Dim client As Client = ClientController.GetClientByID(_clientID)
                If client IsNot Nothing Then
                    Dim billingParts As String() = If(String.IsNullOrEmpty(client.BillingAddress),
                                                  New String() {},
                                                  client.BillingAddress.Split(New String() {", "}, StringSplitOptions.None))
                    txtAddress.Text = If(billingParts.Length > 0, billingParts(0), "")
                    txtCity.Text = If(billingParts.Length > 1, billingParts(1), "")
                    txtRegion.Text = If(billingParts.Length > 2, billingParts(2), "")
                    txtCountry.Text = If(billingParts.Length > 3, billingParts(3), "")
                    txtZipCode.Text = If(billingParts.Length > 4, billingParts(4), "")
                End If
            Catch ex As Exception
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub UpdateCRMClient(sender As Object, e As RoutedEventArgs)
            If ValidateInput() Then
                SaveBillingAddress()
            End If
        End Sub

        Private Function ValidateInput() As Boolean
            If String.IsNullOrWhiteSpace(txtAddress.Text) OrElse
               String.IsNullOrWhiteSpace(txtCity.Text) OrElse
               String.IsNullOrWhiteSpace(txtRegion.Text) OrElse
               String.IsNullOrWhiteSpace(txtCountry.Text) OrElse
               String.IsNullOrWhiteSpace(txtZipCode.Text) Then
                MessageBox.Show("Please fill in all address fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return False
            End If
            Return True
        End Function

        Private Sub SaveBillingAddress()
            Try
                Dim client As Client = ClientController.GetClientByID(_clientID)
                If client Is Nothing Then
                    MessageBox.Show("Client not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If
                Dim billingAddress As String = $"{txtAddress.Text}, {txtCity.Text}, {txtRegion.Text}, {txtCountry.Text}, {txtZipCode.Text}"
                client.BillingAddress = billingAddress
                Dim success As Boolean = ClientController.UpdateClient(client)
                If success Then
                    MessageBox.Show("Billing address updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If
            Catch ex As Exception
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class


End Namespace

