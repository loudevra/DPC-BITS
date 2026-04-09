Imports System.Windows
Imports System.Windows.Controls
Imports DPC.DPC.Data.Models

Public Class FileView

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
    End Sub

    ' This method receives the selected client from the CRM view
    Public Sub LoadClientData(client As Client)
        If client IsNot Nothing Then
            ' 1. Populate standard profile text boxes
            txtFullName.Text = client.Name
            txtPhone.Text = client.Phone
            txtEmail.Text = client.Email

            ' 2. Handle the Address string (splitting it by comma, just like in CRMClients)
            Dim billingParts As String() = If(String.IsNullOrEmpty(client.BillingAddress),
                                              New String() {},
                                              client.BillingAddress.Split(New String() {", "}, StringSplitOptions.None))

            ' Safely assign parts of the address based on how many parts exist
            txtAddress.Text = If(billingParts.Length > 0, billingParts(0), "")
            txtCity.Text = If(billingParts.Length > 1, billingParts(1), "")
            txtRegion.Text = If(billingParts.Length > 2, billingParts(2), "")
            ' Note: Index 3 is usually Country in your logic, so Zip Code is Index 4
            txtZipCode.Text = If(billingParts.Length > 4, billingParts(4), "")
        End If
    End Sub

    Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        ' Search logic
    End Sub

End Class