Imports ClosedXML.Excel
Imports System.Windows
Imports System.Windows.Controls
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM

    Partial Public Class CRMEditResidentialClientShippingAddress
        Private _clientID As String = ""

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Sub New(clientID As String)
            Me.New()
            _clientID = clientID
            ' Wire up button click manually
            AddHandler btnAddUpdate.Click, AddressOf UpdateCRMClient
            AddHandler billingCheckBox.Checked, AddressOf GetInfoBillAddress
            AddHandler billingCheckBox.Unchecked, Sub()
                                                      txtAddress.Text = ""
                                                      txtCity.Text = ""
                                                      txtRegion.Text = ""
                                                      txtCountry.Text = ""
                                                      txtZipCode.Text = ""
                                                      txtAddress.IsEnabled = True
                                                      txtCity.IsEnabled = True
                                                      txtRegion.IsEnabled = True
                                                      txtCountry.IsEnabled = True
                                                      txtZipCode.IsEnabled = True
                                                  End Sub
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
                    Dim shippingParts As String() = If(String.IsNullOrEmpty(client.ShippingAddress),
                                                   New String() {},
                                                   client.ShippingAddress.Split(New String() {", "}, StringSplitOptions.None))
                    txtAddress.Text = If(shippingParts.Length > 0, shippingParts(0), "")
                    txtCity.Text = If(shippingParts.Length > 1, shippingParts(1), "")
                    txtRegion.Text = If(shippingParts.Length > 2, shippingParts(2), "")
                    txtCountry.Text = If(shippingParts.Length > 3, shippingParts(3), "")
                    txtZipCode.Text = If(shippingParts.Length > 4, shippingParts(4), "")
                    billingCheckBox.IsChecked = (client.BillingAddress = client.ShippingAddress)
                    If billingCheckBox.IsChecked = True Then
                        GetInfoBillAddress()
                    End If
                End If
            Catch ex As Exception
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub GetInfoBillAddress()
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
                    txtAddress.IsEnabled = False
                    txtCity.IsEnabled = False
                    txtRegion.IsEnabled = False
                    txtCountry.IsEnabled = False
                    txtZipCode.IsEnabled = False
                End If
            Catch ex As Exception
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub UpdateCRMClient(sender As Object, e As RoutedEventArgs)
            SaveShippingAddress()
        End Sub

        Private Sub SaveShippingAddress()
            Try
                Dim client As Client = ClientController.GetClientByID(_clientID)
                If client Is Nothing Then
                    MessageBox.Show("Client not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If
                If billingCheckBox.IsChecked = True Then
                    client.ShippingAddress = client.BillingAddress
                Else
                    If Not (String.IsNullOrWhiteSpace(txtAddress.Text) AndAlso
                            String.IsNullOrWhiteSpace(txtCity.Text) AndAlso
                            String.IsNullOrWhiteSpace(txtRegion.Text) AndAlso
                            String.IsNullOrWhiteSpace(txtCountry.Text) AndAlso
                            String.IsNullOrWhiteSpace(txtZipCode.Text)) Then
                        Dim shippingAddress As String = $"{txtAddress.Text}, {txtCity.Text}, {txtRegion.Text}, {txtCountry.Text}, {txtZipCode.Text}"
                        client.ShippingAddress = shippingAddress
                    Else
                        client.ShippingAddress = ""
                    End If
                End If
                Dim success As Boolean = ClientController.UpdateClient(client)
                If success Then
                    MessageBox.Show("Shipping address updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If
            Catch ex As Exception
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class

End Namespace
