Imports System.Data
Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM.ClientGroup
    Public Class ClientGroups
        Inherits UserControl
        ' UI elements for direct access
        'Private popup As Popup
        Private recentlyClosed As Boolean = False
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            LoadDetails()
        End Sub

        Private Sub LoadDetails()
            dataGrid.ItemsSource = Nothing
            dataGrid.ItemsSource = ClientGroupController.GetClientGroup()
        End Sub

        Private Sub DeleteProduct(sender As Object, e As RoutedEventArgs)
            Dim deleteClient As New DPC.Components.ConfirmationModals.ConfirmClientGroupDeletion()
            Dim product As DataRowView = TryCast(dataGrid.SelectedItem, DataRowView)


            AddHandler deleteClient.Confirm, AddressOf ConfirmedDeletion
            Dim parentWindow As Window = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, deleteClient, "windowcenter", -100, 0, False, parentWindow)
        End Sub

        Private Sub ConfirmedDeletion()
            Dim _clientGroup As DPC.Data.Models.ClientGroup = TryCast(dataGrid.SelectedItem, DPC.Data.Models.ClientGroup)
            If ClientGroupController.DeleteClientGroup(_clientGroup) Then
                MessageBox.Show("Successfully delete client group")
            End If

            LoadDetails()
        End Sub


        Private Sub CRMAddNewClientGroup(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim CRMAddNewClientGroup As New DPC.Components.Forms.CRMAddNewClientGroup()
            AddHandler CRMAddNewClientGroup.FormClosed, AddressOf LoadDetails
            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)
            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, CRMAddNewClientGroup, "windowcenter", True, -50, 0, parentWindow)
        End Sub
    End Class
End Namespace
