Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Reflection
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports Microsoft.Win32
Imports System.Windows.Controls


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

        ' Add this field at the top of the class
        Private _fullDataView As DataView = Nothing

        Private Sub LoadDepartmentData()
            If HRMController.LoadDepartment(dataGrid) Then
                ' Capture the loaded DataView for filtering/paging
                _fullDataView = TryCast(dataGrid.ItemsSource, DataView)
                ApplyFilter()
            Else
                MessageBox.Show("Failed to load department. Seek for IT Help")
            End If
        End Sub

        Private Sub ApplyFilter()
            If _fullDataView Is Nothing Then Exit Sub

            Dim searchText As String = If(txtSearch IsNot Nothing, txtSearch.Text.ToLower(), "")
            Dim pageSize As Integer = GetSelectedPageSize()

            ' Apply search filter on the full DataView
            Dim filterExpr As String = ""
            If searchText.Length > 0 Then
                filterExpr = $"DepartmentName LIKE '%{searchText}%' OR Convert(DepartmentID, 'System.String') LIKE '%{searchText}%'"
            End If
            _fullDataView.RowFilter = filterExpr

            ' Apply page size limit by taking only the first N rows
            Dim dt As DataTable = _fullDataView.Table.Clone()
            Dim count As Integer = 0
            For Each row As DataRowView In _fullDataView
                If count >= pageSize Then Exit For
                dt.ImportRow(row.Row)
                count += 1
            Next

            dataGrid.ItemsSource = dt.DefaultView
        End Sub

        Private Function GetSelectedPageSize() As Integer
            If cmbPageSize Is Nothing OrElse cmbPageSize.SelectedItem Is Nothing Then Return 10
            Dim selected As ComboBoxItem = TryCast(cmbPageSize.SelectedItem, ComboBoxItem)
            Dim size As Integer = 10
            Integer.TryParse(selected?.Content?.ToString(), size)
            Return size
        End Function

        Private Sub cmbPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ApplyFilter()
        End Sub

        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Reset to full DataView before filtering so search always works on all data
            If _fullDataView Is Nothing Then
                _fullDataView = TryCast(dataGrid.ItemsSource, DataView)
            End If
            ApplyFilter()
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
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            ' 1. Check if there is data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Try
                ' 2. Create a temporary DataGrid for export
                ' We use a temporary grid to avoid modifying the real one (hiding columns, etc.)
                Dim exportGrid As New DataGrid()
                exportGrid.AutoGenerateColumns = False

                ' 3. Manually define ONLY the text columns we want to export
                ' (We intentionally skip the Actions column here, preventing the crash)
                Dim colId As New DataGridTextColumn()
                colId.Header = "#"
                colId.Binding = New Binding("DepartmentID")
                exportGrid.Columns.Add(colId)

                Dim colName As New DataGridTextColumn()
                colName.Header = "Name"
                colName.Binding = New Binding("DepartmentName")
                exportGrid.Columns.Add(colName)

                ' 4. Convert the DataRowViews to simple Objects
                ' This creates a List of anonymous objects that the Exporter CAN read successfully.
                Dim exportList As New List(Of Object)

                For Each item In dataGrid.Items
                    Dim rowView As DataRowView = TryCast(item, DataRowView)
                    If rowView IsNot Nothing Then
                        exportList.Add(New With {
                    .DepartmentID = rowView("DepartmentID"),
                    .DepartmentName = rowView("DepartmentName")
                })
                    End If
                Next

                ' 5. Bind and Export
                exportGrid.ItemsSource = exportList
                ExcelExporter.ExportDataGridToExcel(exportGrid, "Departments", "Department List")

            Catch ex As Exception
                MessageBox.Show("Excel Export Failed: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

    End Class
End Namespace

