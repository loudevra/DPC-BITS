Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers


Namespace DPC.Views.HRM.Employees.Holidays

    Public Class EmployeeHolidays
        Inherits UserControl

        Public Sub New()
            InitializeComponent()

        End Sub

        Private Sub AddHolidayControl(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim addHolidayControl As New DPC.Components.Forms.AddHoliday()
            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)
            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, addHolidayControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub
    End Class

End Namespace