Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMEditResidentialClient
        Private _clientID As String
        Private _isEditMode As Boolean = False

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub ResidentialTabs_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If Not IsLoaded Then Return
            ResidentialMainContent.Children.Clear()
            Dim selectedTab = CType(sender, TabControl).SelectedIndex
            Select Case selectedTab
                Case 0
                    ResidentialMainContent.Children.Add(New CRMEditResidentialClientPersonalInfo(_clientID))
                Case 1
                    ResidentialMainContent.Children.Add(New CRMEditResidentialClientBillingAddress(_clientID))
                Case 2
                    ResidentialMainContent.Children.Add(New CRMEditResidentialClientShippingAddress(_clientID))
                Case 3
                    ResidentialMainContent.Children.Add(New CRMEditResidentialClientOtherSettings(_clientID))
            End Select
        End Sub

        Private Sub CRMEditResidentialClient_Loaded(sender As Object, e As RoutedEventArgs)
            If _isEditMode Then
                ' Load the client data into ResidentialClientDetails
                Dim client = ClientController.GetClientByID(_clientID)
                If client IsNot Nothing Then
                    LoadClientDataIntoDetails(client)
                End If
            End If
            ' Load first tab
            ResidentialMainContent.Children.Add(New CRMEditResidentialClientPersonalInfo(_clientID))
        End Sub

        Private Sub LoadClientDataIntoDetails(client As Client)
            ' Parse billing address
            Dim billingParts As String() = If(String.IsNullOrEmpty(client.BillingAddress),
                                      New String() {},
                                      client.BillingAddress.Split(New String() {", "}, StringSplitOptions.None))

            ' Parse shipping address
            Dim shippingParts As String() = If(String.IsNullOrEmpty(client.ShippingAddress),
                                       New String() {},
                                       client.ShippingAddress.Split(New String() {", "}, StringSplitOptions.None))

            ' Populate ResidentialClientDetails module
            ResidentialClientDetails.ClientName = client.Name
            ResidentialClientDetails.Phone = client.Phone
            ResidentialClientDetails.Email = client.Email

            ' Billing Address
            ResidentialClientDetails.BillAddress = If(billingParts.Length > 0, billingParts(0), "")
            ResidentialClientDetails.BillCity = If(billingParts.Length > 1, billingParts(1), "")
            ResidentialClientDetails.BillRegion = If(billingParts.Length > 2, billingParts(2), "")
            ResidentialClientDetails.BillCountry = If(billingParts.Length > 3, billingParts(3), "")
            ResidentialClientDetails.BillZipCode = If(billingParts.Length > 4, billingParts(4), "")

            ' Shipping Address
            ResidentialClientDetails.Address = If(shippingParts.Length > 0, shippingParts(0), "")
            ResidentialClientDetails.City = If(shippingParts.Length > 1, shippingParts(1), "")
            ResidentialClientDetails.Region = If(shippingParts.Length > 2, shippingParts(2), "")
            ResidentialClientDetails.Country = If(shippingParts.Length > 3, shippingParts(3), "")
            ResidentialClientDetails.ZipCode = If(shippingParts.Length > 4, shippingParts(4), "")

            ' Other details
            ResidentialClientDetails.ClientGroupID = client.ClientGroupID
            ResidentialClientDetails.CustomerGroup = client.CustomerGroup
            ResidentialClientDetails.CustomerLanguage = client.ClientLanguage
            ResidentialClientDetails.SameAsBilling = (client.BillingAddress = client.ShippingAddress)
        End Sub

        Public Sub SetClientID(clientID As String)
            _clientID = clientID
            _isEditMode = True
        End Sub

        Public Function IsEditMode() As Boolean
            Return _isEditMode
        End Function

        Public Function GetClientID() As String
            Return _clientID
        End Function
    End Class
End Namespace