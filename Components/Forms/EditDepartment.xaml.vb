Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Controllers

Namespace DPC.Components.Forms
    Public Class EditDepartment

        ' Closing the popup
        Public Event close(sender As Object, e As RoutedEventArgs)
        Public DepartmentID As Integer

        ' Refresh the data after updating
        Public Event DepartmentSaved As EventHandler
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        Public Sub SetDepartment(departmentID As Integer, departmentName As String)
            ' You can assign these to fields or textboxes
            Me.DepartmentID = departmentID
            TxtDepartment.Text = departmentName
        End Sub

        Private Sub SaveDepartment_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim departmentName As String = TxtDepartment.Text()

                If HRMController.EditDepartments(DepartmentID, departmentName) Then
                    RaiseEvent DepartmentSaved(Me, EventArgs.Empty)
                    RaiseEvent close(Me, e)
                    PopupHelper.ClosePopup()
                    MessageBox.Show("Department update successfully.")
                Else
                    MessageBox.Show("Failed to update department.")
                End If
            Catch ex As Exception
                MessageBox.Show("Error database addDepartment.")
            End Try
        End Sub

        Private Sub ClosePopup_Click(sender As Object, e As RoutedEventArgs)
            ' Raise the close event
            RaiseEvent close(Me, e)
            PopupHelper.ClosePopup()
        End Sub
    End Class
End Namespace