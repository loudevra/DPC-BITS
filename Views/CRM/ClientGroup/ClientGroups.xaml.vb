Imports System.Collections.ObjectModel
Imports System.Data
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports DPC.Data.Helpers
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports OfficeOpenXml

Namespace DPC.Views.CRM.ClientGroup
    Public Class ClientGroups
        Inherits UserControl

        ' Pagination + Search helpers (same as ManageSuppliers)
        Private _paginationHelper As PaginationHelper
        Private _searchFilterHelper As SearchFilterHelper

        ' Holds the group pending deletion
        Private _pendingDeleteGroup As DPC.Data.Models.ClientGroup

        Public Sub New()
            InitializeComponent()
            LoadDetails()
        End Sub

        Private Sub LoadDetails()
            Try
                ' Get all client groups from DB
                Dim allGroups = ClientGroupController.GetClientGroup()
                Dim allItems As New ObservableCollection(Of Object)(allGroups.Cast(Of Object)())

                ' Clear pagination panel to avoid duplicates on reload
                paginationPanel.Children.Clear()

                ' Initialize PaginationHelper (same as ManageSuppliers)
                _paginationHelper = New PaginationHelper(dataGrid, paginationPanel)

                ' Apply page size from ComboBox
                If cmbPageSize IsNot Nothing Then
                    Dim selected = TryCast(cmbPageSize.SelectedItem, ComboBoxItem)
                    If selected IsNot Nothing Then
                        Dim parsed As Integer
                        If Integer.TryParse(selected.Content.ToString(), parsed) Then
                            _paginationHelper.ItemsPerPage = parsed
                        End If
                    End If
                End If

                ' Set all items — this triggers the first render
                _paginationHelper.AllItems = allItems

                ' Initialize SearchFilterHelper with searchable columns
                _searchFilterHelper = New SearchFilterHelper(_paginationHelper,
                    "GroupName", "Description")

            Catch ex As Exception
                MessageBox.Show("Error loading client groups: " & ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub CmbPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If _paginationHelper Is Nothing Then Return

            Dim selected = TryCast(cmbPageSize.SelectedItem, ComboBoxItem)
            If selected Is Nothing Then Return

            Dim parsed As Integer
            If Integer.TryParse(selected.Content.ToString(), parsed) Then
                _paginationHelper.ItemsPerPage = parsed
            End If
        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If txtSearch.Text = "Search" Then Return
            If _searchFilterHelper IsNot Nothing Then
                _searchFilterHelper.SearchText = txtSearch.Text.Trim()
            End If
        End Sub

        Private Sub TxtSearch_GotFocus(sender As Object, e As RoutedEventArgs)
            If txtSearch.Text = "Search" Then
                txtSearch.Text = ""
                txtSearch.Foreground = System.Windows.Media.Brushes.Black
            End If
        End Sub

        Private Sub TxtSearch_LostFocus(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(txtSearch.Text) Then
                txtSearch.Text = "Search"
                txtSearch.Foreground = System.Windows.Media.Brushes.Gray
            End If
        End Sub

        Private Sub CRMAddNewClientGroup(sender As Object, e As RoutedEventArgs)
            Dim form As New DPC.Views.CRM.ClientGroup.AddNewClientGroup()
            AddHandler form.FormClosed, AddressOf LoadDetails
            PopupHelper.OpenPopupWithControl(sender, form, "windowcenter", True, -50, 0, Window.GetWindow(Me))
        End Sub

        Private Sub EditClientGroup(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim clientGroup = TryCast(btn.DataContext, DPC.Data.Models.ClientGroup)
            If clientGroup Is Nothing Then Return

            Dim form As New DPC.Views.CRM.ClientGroup.AddNewClientGroup()
            form.EditGroup = clientGroup
            AddHandler form.FormClosed, AddressOf LoadDetails
            PopupHelper.OpenPopupWithControl(sender, form, "windowcenter", True, -50, 0, Window.GetWindow(Me))
        End Sub

        Private Sub DeleteProduct(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim clientGroup = TryCast(btn.DataContext, DPC.Data.Models.ClientGroup)
            If clientGroup Is Nothing Then
                MessageBox.Show("Could not identify the selected group.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            _pendingDeleteGroup = clientGroup

            Dim modal As New DPC.Components.ConfirmationModals.ConfirmClientGroupDeletion()
            AddHandler modal.Confirm, AddressOf ConfirmedDeletion
            Dim parentWindow As Window = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, modal, "windowcenter", -100, 0, False, parentWindow)
        End Sub

        Private Sub ConfirmedDeletion()
            If _pendingDeleteGroup Is Nothing Then Return

            If ClientGroupController.DeleteClientGroup(_pendingDeleteGroup) Then
                MessageBox.Show("Client group deleted successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information)
            Else
                MessageBox.Show("Failed to delete client group.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error)
            End If

            _pendingDeleteGroup = Nothing
            LoadDetails()
        End Sub

        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            Try
                ExcelPackage.License.SetNonCommercialPersonal("DPC")

                Dim saveDialog As New Microsoft.Win32.SaveFileDialog()
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx"
                saveDialog.FileName = "ClientGroups_" & DateTime.Now.ToString("yyyyMMdd_HHmmss")

                If saveDialog.ShowDialog() <> True Then Return

                Dim allGroups = ClientGroupController.GetClientGroup()

                Using pkg As New ExcelPackage()
                    Dim ws As ExcelWorksheet = pkg.Workbook.Worksheets.Add("Client Groups")

                    ws.Cells(1, 1).Value = "#"
                    ws.Cells(1, 2).Value = "Group Name"
                    ws.Cells(1, 3).Value = "Description"
                    ws.Cells(1, 4).Value = "Total Clients"

                    Using headerRange = ws.Cells(1, 1, 1, 4)
                        headerRange.Style.Font.Bold = True
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
                        headerRange.Style.Fill.BackgroundColor.SetColor(
                            System.Drawing.Color.FromArgb(71, 71, 71))
                        headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White)
                    End Using

                    Dim row As Integer = 2
                    For Each g In allGroups
                        ws.Cells(row, 1).Value = g.ClientGroupID
                        ws.Cells(row, 2).Value = g.GroupName
                        ws.Cells(row, 3).Value = g.Description
                        ws.Cells(row, 4).Value = g.ClientCount
                        row += 1
                    Next

                    ws.Cells(ws.Dimension.Address).AutoFitColumns()
                    pkg.SaveAs(New FileInfo(saveDialog.FileName))
                End Using

                MessageBox.Show("Exported successfully!", "Excel Export",
                    MessageBoxButton.OK, MessageBoxImage.Information)

            Catch ex As Exception
                MessageBox.Show("Export failed: " & ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

    End Class
End Namespace