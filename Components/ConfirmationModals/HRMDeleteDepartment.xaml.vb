Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient

Namespace DPC.Components.ConfirmationModals
    Public Class HRMDeleteDepartment
        ' Storing the id
        Public DepartmentID As Integer
        Dim BeforeDepartmentName As String
        ' Refreshing the Data
        Public Event DepartmentSaved As EventHandler

        ' When closing the popup modal
        Public Event close(sender As Object, e As RoutedEventArgs)
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        Public Sub SetDepartment(departmentID As Integer, departmentName As String)
            ' You can assign these to fields or textboxes
            Me.DepartmentID = departmentID
            BeforeDepartmentName = departmentName
        End Sub

        Private Sub Confirm_Click(sender As Object, e As RoutedEventArgs)
            If HRMController.DeleteDepartment(DepartmentID) Then
                HRMController.ActionLogs(CacheOnLoggedInName, "Delete", BeforeDepartmentName, Nothing)

                RaiseEvent DepartmentSaved(Me, EventArgs.Empty)
                RaiseEvent close(Me, e)
                PopupHelper.ClosePopup()
                MessageBox.Show("Successfully delete the department")
            Else
                MessageBox.Show("Failed to delete the department")
            End If
        End Sub

        Private Sub Cancel_Click(sender As Object, e As RoutedEventArgs)
            RaiseEvent DepartmentSaved(Me, EventArgs.Empty)
            RaiseEvent close(Me, e)
            PopupHelper.ClosePopup()
        End Sub
    End Class
End Namespace