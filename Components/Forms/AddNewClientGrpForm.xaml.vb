Imports System.Windows
Imports System.Windows.Controls

Namespace DPC.Components.Forms
    Public Class AddNewClientGrpForm
        Inherits UserControl

        ' Public event to notify when modal is closed
        Public Event ModalClosed As EventHandler

        Public Sub New()
            InitializeComponent()
        End Sub

        ' Close the modal
        Public Sub CloseModal()
            Me.Visibility = Visibility.Collapsed
            RaiseEvent ModalClosed(Me, EventArgs.Empty) ' Notify parent if needed
        End Sub

        ' Button click event to add a new group
        Private Sub btnAddGroup_Click(sender As Object, e As RoutedEventArgs)
            Dim groupName As String = txtGroupName.Text.Trim()
            Dim description As String = txtDescription.Text.Trim()

            If String.IsNullOrWhiteSpace(groupName) Then
                MessageBox.Show("Group Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' TODO: Implement database insertion logic here
            MessageBox.Show($"New Client Group Added: {groupName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            ' Close modal after adding
            CloseModal()
        End Sub

        ' Close button click event
        Private Sub CloseModal_Click(sender As Object, e As RoutedEventArgs)
            CloseModal()
        End Sub

    End Class
End Namespace
