Imports System.Windows
Imports DPC.DPC.Components.Navigation

Namespace DPC.Views.HRM.Employees.Employees
    Public Class EmployeeAdd
        Inherits Window

        Public Sub New()
            InitializeComponent()
            LoadSidebar()
        End Sub

        Private Sub LoadSidebar()
            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar
        End Sub

        Private Sub CancelAddEmployee(sender As Object, e As RoutedEventArgs)
            ' Navigate back to EmployeesView and close current window
            Dim employeesView As New EmployeesView()
            employeesView.Show()
            Me.Close()
        End Sub

        Private Sub SaveEmployee(sender As Object, e As RoutedEventArgs)
            ' Implement save logic here (e.g., validate and save to database)
            MessageBox.Show("Employee details saved successfully!")

            ' Navigate back to EmployeesView
            Dim employeesView As New EmployeesView()
            employeesView.Show()
            Me.Close()
        End Sub

    End Class
End Namespace
