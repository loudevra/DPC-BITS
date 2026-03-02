Imports System.Data
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Imports OfficeOpenXml

Namespace DPC.Views.CRM.ClientGroup
    Public Class ClientGroups
        Inherits UserControl

        ' Holds the full unfiltered list
        Private _allClientGroups As IEnumerable(Of DPC.Data.Models.ClientGroup)

        ' Pagination state
        Private _currentPage As Integer = 1
        Private _pageSize As Integer = 10

        ' Holds the group pending deletion across the async confirmation callback
        Private _pendingDeleteGroup As DPC.Data.Models.ClientGroup

        Public Sub New()
            InitializeComponent()
            LoadDetails()
        End Sub

        Private Sub LoadDetails()
            _allClientGroups = ClientGroupController.GetClientGroup()
            _currentPage = 1
            ApplySearch(If(txtSearch IsNot Nothing AndAlso txtSearch.Text <> "Search", txtSearch.Text, ""))
        End Sub

        Private Sub RefreshGrid()
            If _allClientGroups Is Nothing Then Return

            ' Apply current search filter
            Dim keyword As String = If(txtSearch IsNot Nothing AndAlso txtSearch.Text <> "Search", txtSearch.Text.Trim(), "")

            Dim filtered As IEnumerable(Of DPC.Data.Models.ClientGroup)
            If String.IsNullOrWhiteSpace(keyword) Then
                filtered = _allClientGroups
            Else
                filtered = _allClientGroups.Where(
                    Function(g) g.GroupName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                                g.Description.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            End If

            Dim totalItems As Integer = filtered.Count()
            Dim totalPages As Integer = Math.Max(1, CInt(Math.Ceiling(totalItems / _pageSize)))

            ' Clamp page
            If _currentPage > totalPages Then _currentPage = totalPages
            If _currentPage < 1 Then _currentPage = 1

            ' Page the data
            Dim paged = filtered.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList()
            dataGrid.ItemsSource = Nothing
            dataGrid.ItemsSource = paged

            ' Update pagination labels
            Dim startEntry As Integer = If(totalItems = 0, 0, (_currentPage - 1) * _pageSize + 1)
            Dim endEntry As Integer = Math.Min(_currentPage * _pageSize, totalItems)
            txtPageInfo.Text = $"Showing {startEntry}–{endEntry} of {totalItems} entries"
            txtCurrentPage.Text = $"Page {_currentPage} of {totalPages}"

            ' Enable / disable nav buttons
            BtnPrevPage.IsEnabled = _currentPage > 1
            BtnNextPage.IsEnabled = _currentPage < totalPages
        End Sub

        Private Sub ApplySearch(keyword As String)
            _currentPage = 1
            RefreshGrid()
        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If txtSearch.Text = "Search" Then Return
            ApplySearch(txtSearch.Text.Trim())
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

        Private Sub CmbPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selected = TryCast(cmbPageSize.SelectedItem, ComboBoxItem)
            If selected Is Nothing Then Return

            Dim parsed As Integer
            If Integer.TryParse(selected.Content.ToString(), parsed) Then
                _pageSize = parsed
                _currentPage = 1
                RefreshGrid()
            End If
        End Sub

        Private Sub BtnPrevPage_Click(sender As Object, e As RoutedEventArgs)
            If _currentPage > 1 Then
                _currentPage -= 1
                RefreshGrid()
            End If
        End Sub

        Private Sub BtnNextPage_Click(sender As Object, e As RoutedEventArgs)
            _currentPage += 1
            RefreshGrid()
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
            form.EditGroup = clientGroup                         ' ← passes the record
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
                    For Each g In _allClientGroups
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