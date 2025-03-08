Namespace DPC.Components.Forms
    Public Class AddNewClientGrpForm
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BtnAddGroup_Click(sender As Object, e As RoutedEventArgs)
            Dim groupName As String = txtGroupName.Text.Trim()
            Dim description As String = txtDescription.Text.Trim()

            If String.IsNullOrWhiteSpace(groupName) Then
                MessageBox.Show("Group Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' TODO: Implement database insertion logic here
            MessageBox.Show($"New Client Group Added: {groupName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub
    End Class
End Namespace
