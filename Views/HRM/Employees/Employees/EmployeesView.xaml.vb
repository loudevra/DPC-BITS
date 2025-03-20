Imports System.Collections.ObjectModel
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Model

Namespace DPC.Views.HRM.Employees.Employees
    Partial Public Class EmployeesView
        Inherits Window

        Private Employees As ObservableCollection(Of Employee)

        ' Declare Pagination Variables
        Private CurrentPage As Integer = 1
        Private TotalPages As Integer = 1

        Public Sub New()
            InitializeComponent()


            EmployeesDataGrid.ItemsSource = Employees

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Initialize Pagination
            TotalPages = 1
            CurrentPage = 1
            UpdatePagination()
        End Sub

        Private Sub UpdatePagination()
            TxtPageNumber.Text = $"Page {CurrentPage} of {TotalPages}"
            BtnFirstPage.IsEnabled = (CurrentPage > 1)
            BtnPrevPage.IsEnabled = (CurrentPage > 1)
            BtnNextPage.IsEnabled = (CurrentPage < TotalPages)
            BtnLastPage.IsEnabled = (CurrentPage < TotalPages)
        End Sub

        Private Sub BtnFirstPage_Click(sender As Object, e As RoutedEventArgs)
            CurrentPage = 1
            UpdatePagination()
        End Sub

        Private Sub BtnPrevPage_Click(sender As Object, e As RoutedEventArgs)
            If CurrentPage > 1 Then
                CurrentPage -= 1
                UpdatePagination()
            End If
        End Sub

        Private Sub BtnNextPage_Click(sender As Object, e As RoutedEventArgs)
            If CurrentPage < TotalPages Then
                CurrentPage += 1
                UpdatePagination()
            End If
        End Sub

        Private Sub BtnLastPage_Click(sender As Object, e As RoutedEventArgs)
            CurrentPage = TotalPages
            UpdatePagination()
        End Sub

        Private Sub AddEmployee(sender As Object, e As RoutedEventArgs)
            ' Open EmployeeAdd.xaml
            Dim addEmployeeWindow As New AddEmployee()
            addEmployeeWindow.Show()

            ' Close the current EmployeesView window
            Me.Close()
        End Sub

    End Class
End Namespace
