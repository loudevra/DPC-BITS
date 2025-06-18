Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Reflection
Imports System.Windows.Controls.Primitives
Imports ClosedXML.Excel
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports Microsoft.Win32

Namespace DPC.Views.HRM.Departments
    Public Class DepartmentsView
        Inherits UserControl

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
        End Sub

        Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
            LoadDepartmentData()
        End Sub

        Private Sub LoadDepartmentData()
            If HRMController.LoadDepartment(dataGrid) Then
                HRMController.LoadDepartment(dataGrid)
            Else
                MessageBox.Show("Failed to loaded department. Seek for IT Help")
            End If
        End Sub

        Private Sub OnDepartmentSaved(sender As Object, e As EventArgs)
            LoadDepartmentData() ' Refresh the DataGrid
        End Sub

        Private Sub AddDepartmentPopup(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddDepartment form
            Dim AddDepartmentWindow As New DPC.Components.Forms.AddDepartment()

            ' Attach the event handler to refresh the DataGrid after saving
            AddHandler AddDepartmentWindow.DepartmentSaved, AddressOf OnDepartmentSaved

            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)

            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, AddDepartmentWindow, "windowcenter", True, -50, 0, parentWindow)
        End Sub

        Private Sub EditDepartmentPopup_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedRow As DataRowView = TryCast(dataGrid.SelectedItem, DataRowView)

            If selectedRow IsNot Nothing Then
                Dim departmentID As Integer = Convert.ToInt32(selectedRow("DepartmentID"))
                Dim departmentName As String = selectedRow("DepartmentName").ToString()

                ' Create the edit form and pass the data
                Dim editForm As New DPC.Components.Forms.EditDepartment()
                editForm.SetDepartment(departmentID, departmentName)

                ' Attach the event handler to refresh the DataGrid after editing
                AddHandler editForm.DepartmentSaved, AddressOf OnDepartmentSaved

                Dim parentWindow = Window.GetWindow(Me)
                PopupHelper.OpenPopupWithControl(sender, editForm, "windowcenter", True, -50, 0, parentWindow)
            Else
                MessageBox.Show("Please select a department first.")
            End If
        End Sub

        Private Sub DeleteDepartmentPopup_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedRow As DataRowView = TryCast(dataGrid.SelectedItem, DataRowView)

            If selectedRow IsNot Nothing Then
                Dim departmentID As Integer = Convert.ToInt32(selectedRow("DepartmentID"))
                Dim departmentName As String = selectedRow("DepartmentName").ToString()

                ' Create the edit form and pass the data
                Dim deleteForm As New DPC.Components.ConfirmationModals.HRMDeleteDepartment()
                deleteForm.SetDepartment(departmentID, departmentName)

                ' Attach the event handler to refresh the DataGrid after editing
                AddHandler deleteForm.DepartmentSaved, AddressOf OnDepartmentSaved

                Dim parentWindow = Window.GetWindow(Me)
                PopupHelper.OpenPopupWithControl(sender, deleteForm, "windowcenter", True, -50, 0, parentWindow)
            Else
                MessageBox.Show("Please select a department first.")
            End If
        End Sub
    End Class
End Namespace

