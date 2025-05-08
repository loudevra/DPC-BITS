Namespace DPC.Views.HRM.Departments
    Public Class DepartmentsView
        Inherits UserControl
        Private Sub AddDepartmentPopup(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim AddDepartmentWindow As New DPC.Components.Forms.AddDepartment()

            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)

            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, AddDepartmentWindow, "windowcenter", True, -50, 0, parentWindow)
        End Sub
    End Class
End Namespace

