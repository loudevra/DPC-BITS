Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers


Namespace DPC.Views.HRM.Employees.Salaries

    Public Class EmployeeSalaries
        Inherits UserControl

        Public Sub New()
            InitializeComponent()

        End Sub

        Private Sub AddSalaries(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("addnewsalaries", Me)
        End Sub
    End Class

End Namespace