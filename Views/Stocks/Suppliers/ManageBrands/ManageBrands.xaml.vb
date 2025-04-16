Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components
Imports System.Windows.Controls.Primitives
Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports ClosedXML.Excel
Imports Microsoft.Win32
Imports System.Data
Imports System.IO
Imports System.Reflection
Imports System.ComponentModel
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Stocks.Suppliers.ManageBrands
    Public Class ManageBrands
        Inherits Window

        Private popup As Popup
        Private recentlyClosed As Boolean = False
        Private view As ICollectionView

        Public Sub New()
            InitializeComponent()
            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            ' Load DataGrid with items and create a CollectionViewSource for filtering
            view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource)
            If view IsNot Nothing Then
                view.Filter = AddressOf FilterDataGrid
            End If

            LoadBrands()
        End Sub

        ' Function to filter DataGrid based on search text
        Private Function FilterDataGrid(item As Object) As Boolean
            If String.IsNullOrWhiteSpace(txtSearch.Text) Then
                Return True ' Show all items if search is empty
            End If

            Dim searchText As String = txtSearch.Text.ToLower()

            Return False
        End Function

        ' Event Handler for Export Button Click
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            ' Check if DataGrid has data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Open SaveFileDialog
            Dim saveFileDialog As New SaveFileDialog() With {
     .Filter = "Excel Files (*.xlsx)|*.xlsx",
     .FileName = "DataGridExport.xlsx"
 }

            If saveFileDialog.ShowDialog() = True Then
                Try
                    ' Create Excel workbook
                    Using workbook As New XLWorkbook()
                        Dim dt As New DataTable()

                        ' Add DataGrid columns as table headers
                        For Each column As DataGridColumn In dataGrid.Columns
                            dt.Columns.Add(column.Header.ToString())
                        Next

                        ' Add rows from DataGrid items
                        For Each item In dataGrid.Items
                            Dim row As DataRow = dt.NewRow()
                            For i As Integer = 0 To dataGrid.Columns.Count - 1
                                Dim column As DataGridColumn = dataGrid.Columns(i)
                                Dim boundColumn = TryCast(column, DataGridBoundColumn)
                                If boundColumn IsNot Nothing AndAlso boundColumn.Binding IsNot Nothing Then
                                    Dim binding As Binding = TryCast(boundColumn.Binding, Binding)
                                    If binding IsNot Nothing AndAlso binding.Path IsNot Nothing Then
                                        Dim bindingPath As String = binding.Path.Path
                                        Dim prop As PropertyInfo = item.GetType().GetProperty(bindingPath)
                                        If prop IsNot Nothing Then
                                            row(i) = prop.GetValue(item, Nothing)?.ToString()
                                        End If
                                    End If
                                End If
                            Next
                            dt.Rows.Add(row)
                        Next

                        ' Add table to Excel sheet
                        Dim worksheet = workbook.Worksheets.Add(dt, "DataGridData")
                        worksheet.Columns().AdjustToContents()

                        ' Save Excel file
                        workbook.SaveAs(saveFileDialog.FileName)
                        MessageBox.Show("Export Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error exporting data: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        Private Sub OpenAddBrand(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            If recentlyClosed Then
                recentlyClosed = False
                Return
            End If

            If popup IsNot Nothing AndAlso popup.IsOpen Then
                popup.IsOpen = False
                recentlyClosed = True
                Return
            End If

            popup = New Popup With {
                .PlacementTarget = clickedButton,
                .Placement = PlacementMode.Bottom,
                .StaysOpen = False,
                .AllowsTransparency = True
            }

            Dim addBrandWindow As New DPC.Components.Forms.AddBrand()

            ' Handle the BrandAdded event
            AddHandler addBrandWindow.BrandAdded, AddressOf OnBrandAdded

            popup.Child = addBrandWindow

            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            popup.IsOpen = True
        End Sub

        ' Callback to reload the brand data
        Private Sub OnBrandAdded()
            LoadBrands()
        End Sub

        Private Sub LoadBrands()
            Dim brands = BrandController.GetBrands()
            dataGrid.ItemsSource = brands
        End Sub


        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim searchText As String = txtSearch.Text.Trim()
            SearchEmployee(searchText)
        End Sub

        Private Sub SearchEmployee(query As String)
            Dim conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
            Try
                conn.Open()
                Dim sql As String = "SELECT * FROM brand WHERE brandID LIKE @query OR BrandName LIKE @query"
                Dim cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@query", "%" & query & "%")

                Dim adapter As New MySqlDataAdapter(cmd)
                Dim dt As New DataTable()
                adapter.Fill(dt)

                ' TODO: bind dt to your result display control like a DataGrid or ListView
                dataGrid.ItemsSource = dt.DefaultView

            Catch ex As Exception
                MessageBox.Show("Search error: " & ex.Message)
            Finally
                conn.Close()
            End Try
        End Sub
    End Class
End Namespace
