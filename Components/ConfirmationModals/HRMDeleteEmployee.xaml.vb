Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Controllers

Namespace DPC.Components.ConfirmationModals
    Public Class HRMDeleteEmployee
        Public Event DeletedEmployee As EventHandler
        Private ReadOnly _employeeToDelete As Employee

        Public Sub New(emp As Employee)
            ' This call is required by the designer.
            InitializeComponent()
            ' Add any initialization after the InitializeComponent() call.
            _employeeToDelete = emp
        End Sub

        Private Sub DeleteEmployee()
            If _employeeToDelete Is Nothing Then
                MessageBox.Show("No employee selected for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim success As Boolean = EmployeeController.DeleteEmployee(_employeeToDelete.EmployeeID)

            If success Then
                MessageBox.Show("Employee deleted successfully.")
                RaiseEvent DeletedEmployee(Me, EventArgs.Empty)
            Else
                MessageBox.Show("Failed to delete employee. Please try again.")
            End If
        End Sub

        Private Sub Cancel_Click(sender As Object, e As RoutedEventArgs)
            Me.Visibility = Visibility.Collapsed ' Hide Modal
        End Sub

        Private Sub Confirm_Click(sender As Object, e As RoutedEventArgs)
            DeleteEmployee()
        End Sub
    End Class
End Namespace