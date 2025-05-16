Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components
Imports System.Windows.Controls.Primitives
Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports System.Data
Imports System.ComponentModel
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Components.Dynamic ' Added import for DynamicDialogs

Namespace DPC.Views.Stocks.Suppliers.ManageBrands
    Public Class ManageBrands
        Inherits UserControl

        ' Properties for popup and data handling
        Private popup As Popup
        Private recentlyClosed As Boolean = False
        Private view As ICollectionView

        ' UI elements for direct access

        ' Properties for pagination
        Private _paginationHelper As PaginationHelper
        Private _searchFilterHelper As SearchFilterHelper

        Public Sub New(lblPageInfo As TextBlock)
            ' Constructor for testing or other uses that might provide a page info label
            InitializeComponent()
            InitializeControls()
        End Sub

        Public Sub New()
            InitializeComponent()
            InitializeControls()
        End Sub

        Private Sub InitializeControls()
            ' Find UI elements using their name
            dataGrid = TryCast(FindName("dataGrid"), DataGrid)
            txtSearch = TryCast(FindName("txtSearch"), TextBox)
            cboItemsPerPage = TryCast(FindName("cboItemsPerPage"), ComboBox)
            paginationPanel = TryCast(FindName("paginationPanel"), StackPanel)

            ' Verify that required controls are found
            If dataGrid Is Nothing Then
                ' Changed from MessageBox to DynamicDialogs
                DynamicDialogs.ShowError(Me, "DataGrid not found in the XAML.", "Initialization Error")
                Return
            End If

            If paginationPanel Is Nothing Then
                ' Changed from MessageBox to DynamicDialogs
                DynamicDialogs.ShowError(Me, "Pagination panel not found in the XAML.", "Initialization Error")
                Return
            End If

            ' Wire up event handlers
            If txtSearch IsNot Nothing Then
                AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
            End If

            If cboItemsPerPage IsNot Nothing Then
                AddHandler cboItemsPerPage.SelectionChanged, AddressOf CboItemsPerPage_SelectionChanged
            End If

            ' Initialize and load brands data
            LoadBrands()
        End Sub

        ' Event handler for TextChanged to update the filter
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _searchFilterHelper IsNot Nothing Then
                _searchFilterHelper.SearchText = txtSearch.Text
            End If
        End Sub

        ' Event Handler for Export Button Click - Updated to use ExcelExporter
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            If dataGrid Is Nothing Then Return

            Try
                Dim columnsToExclude As New List(Of String) From {"Settings"}
                ExcelExporter.ExportDataGridToExcel(dataGrid, columnsToExclude, "BrandsExport", "Brands")

                ' Added success message
                DynamicDialogs.ShowSuccess(Me, "Data exported successfully to Excel.", "Export Complete")
            Catch ex As Exception
                ' Added error handling
                DynamicDialogs.ShowError(Me, "Failed to export data: " & ex.Message, "Export Error")
            End Try
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
            ' Show success message
            DynamicDialogs.ShowSuccess(Me, "Brand added successfully!")
            LoadBrands()
        End Sub

        Private Sub LoadBrands()
            Try
                ' Check if DataGrid exists
                If dataGrid Is Nothing Then
                    ' Changed from MessageBox to DynamicDialogs
                    DynamicDialogs.ShowError(Me, "DataGrid control not found.", "Error")
                    Return
                End If

                ' Get all brands with error handling
                Dim allBrands As ObservableCollection(Of Object)
                Try
                    Dim brandList = BrandController.GetBrands()
                    If brandList Is Nothing Then
                        ' Changed from MessageBox to DynamicDialogs
                        DynamicDialogs.ShowWarning(Me, "Brand data returned null.", "Data Error")
                        allBrands = New ObservableCollection(Of Object)()
                    Else
                        allBrands = New ObservableCollection(Of Object)(brandList)
                    End If
                Catch ex As Exception
                    ' Changed from MessageBox to DynamicDialogs
                    DynamicDialogs.ShowError(Me, "Error retrieving brand data: " & ex.Message, "Data Error")
                    allBrands = New ObservableCollection(Of Object)()
                End Try

                ' Clear the pagination panel to avoid duplicate controls
                paginationPanel.Children.Clear()

                ' Initialize pagination helper with our DataGrid and pagination panel
                _paginationHelper = New PaginationHelper(dataGrid, paginationPanel)

                ' Set the items per page from the combo box if available
                If cboItemsPerPage IsNot Nothing Then
                    Dim selectedItem = TryCast(cboItemsPerPage.SelectedItem, ComboBoxItem)
                    If selectedItem IsNot Nothing Then
                        Dim itemsPerPageText As String = TryCast(selectedItem.Content, String)
                        Dim itemsPerPage As Integer
                        If Integer.TryParse(itemsPerPageText, itemsPerPage) Then
                            _paginationHelper.ItemsPerPage = itemsPerPage
                        End If
                    End If
                End If

                ' Set the all items to the helper
                _paginationHelper.AllItems = allBrands

                ' Initialize search filter helper with our pagination helper
                _searchFilterHelper = New SearchFilterHelper(_paginationHelper, "ID", "Name", "TotalSupplier")

            Catch ex As Exception
                ' Changed from MessageBox to DynamicDialogs with more details option
                Dim errorDialog = DynamicDialogs.ShowError(Me,
                    "Error in LoadBrands: " & ex.Message,
                    "Error",
                    "View Details")

                ' Add exception data for details
                errorDialog.DialogData = ex

                ' Handle "View Details" button click
                AddHandler errorDialog.PrimaryAction, Sub(s, args)
                                                          Dim exception = DirectCast(args.Data, Exception)
                                                          ' Show stack trace in another dialog
                                                          DynamicDialogs.ShowInformation(Me,
                                                              "Stack Trace: " & exception.StackTrace,
                                                              "Error Details")
                                                      End Sub
            End Try
        End Sub

        Private Sub CboItemsPerPage_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If _paginationHelper Is Nothing Then Return

            ' Get the selected value from the ComboBox
            Dim selectedComboBoxItem As ComboBoxItem = TryCast(cboItemsPerPage.SelectedItem, ComboBoxItem)
            If selectedComboBoxItem IsNot Nothing Then
                Dim itemsPerPageText As String = TryCast(selectedComboBoxItem.Content, String)
                Dim newItemsPerPage As Integer

                If Integer.TryParse(itemsPerPageText, newItemsPerPage) Then
                    ' Update the pagination helper's items per page
                    _paginationHelper.ItemsPerPage = newItemsPerPage
                End If
            End If
        End Sub
    End Class
End Namespace