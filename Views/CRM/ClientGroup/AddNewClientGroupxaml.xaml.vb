' AddNewClientGroupxaml.xaml.vb
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Namespace DPC.Views.CRM.ClientGroup
    Public Class AddNewClientGroup
        Inherits System.Windows.Controls.UserControl
        Public Event FormClosed()
        Private _editGroup As DPC.Data.Models.ClientGroup = Nothing
        Public Property EditGroup As DPC.Data.Models.ClientGroup
            Get
                Return _editGroup
            End Get
            Set(value As DPC.Data.Models.ClientGroup)
                _editGroup = value
                PopulateForEdit()
            End Set
        End Property
        Public Sub New()
            InitializeComponent()
        End Sub
        Private Sub PopulateForEdit()
            If _editGroup Is Nothing Then Return
            txtGroupName.Text = _editGroup.GroupName
            txtDescription.Text = _editGroup.Description
            txtHeader.Text = "Edit Client Group"
            txtBtnLabel.Text = "Save Changes"
        End Sub
        Private Sub BtnAddGroup_Click(sender As Object, e As System.Windows.RoutedEventArgs)
            Dim name As String = txtGroupName.Text.Trim()
            Dim desc As String = txtDescription.Text.Trim()
            If String.IsNullOrWhiteSpace(name) Then
                System.Windows.MessageBox.Show("Group name is required.", "Validation",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning)
                Return
            End If
            Dim success As Boolean
            If _editGroup IsNot Nothing Then
                _editGroup.GroupName = name
                _editGroup.Description = desc
                success = ClientGroupController.UpdateClientGroup(_editGroup)
                If success Then
                    System.Windows.MessageBox.Show("Client group updated successfully.", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information)
                Else
                    System.Windows.MessageBox.Show("Failed to update client group.", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error)
                End If
            Else
                Dim newGroup As New DPC.Data.Models.ClientGroup With {
                    .GroupName = name,
                    .Description = desc
                }
                success = ClientGroupController.CreateClientGroup(newGroup)   ' ← correct name
                If success Then
                    System.Windows.MessageBox.Show("Client group added successfully.", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information)
                Else
                    System.Windows.MessageBox.Show("Failed to add client group.", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error)
                End If
            End If
            If success Then
                RaiseEvent FormClosed()
                Dim popup = FindParentPopup(Me)
                If popup IsNot Nothing Then popup.IsOpen = False
            End If
        End Sub
        Private Function FindParentPopup(element As System.Windows.DependencyObject) As System.Windows.Controls.Primitives.Popup
            Dim parent = System.Windows.Media.VisualTreeHelper.GetParent(element)
            While parent IsNot Nothing
                Dim p = TryCast(parent, System.Windows.Controls.Primitives.Popup)
                If p IsNot Nothing Then Return p
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent)
            End While
            Return Nothing
        End Function
    End Class
End Namespace