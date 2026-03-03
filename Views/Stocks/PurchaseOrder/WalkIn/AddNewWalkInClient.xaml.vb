Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Models

Namespace DPC.Views.Stocks.PurchaseOrder.WalkIn
    Partial Public Class AddNewWalkInClient
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            ' Check required fields
            If String.IsNullOrEmpty(txtClientName.Text) OrElse
               String.IsNullOrEmpty(txtPhoneNumber.Text) OrElse
               String.IsNullOrEmpty(txtClientEmail.Text) Then
                MessageBox.Show("Please fill in all required fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Dim completeAddress = $"{txtAddress.Text}, {txtCity.Text}, {txtRegion.Text}, {txtCountry.Text}, {txtPostalCode.Text}"
            ' Create the Client object (Make sure to use commas correctly!)
            Dim client As New Client With {
                .ClientGroupID = 2,
                .Name = txtClientName.Text,
                .Phone = txtPhoneNumber.Text,
                .Email = txtClientEmail.Text,
                .BillingAddress = completeAddress,
                .ShippingAddress = completeAddress,
                .CustomerGroup = "Residential",
                .ClientLanguage = "English",
                .TinId = txtTinId.Text,
                .ClientType = "Residential"
            }

            Dim success As Boolean = ClientController.CreateClient(client)

            If success Then
                MessageBox.Show("Client added successfully.")

                txtClientName.Text = ""
                txtPhoneNumber.Text = ""
                txtClientEmail.Text = ""
                txtAddress.Text = ""
                txtCity.Text = ""
                txtRegion.Text = ""
                txtCountry.Text = ""
                txtPostalCode.Text = ""
                txtTinId.Text = ""


            End If
        End Sub
    End Class
End Namespace