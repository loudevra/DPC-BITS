Imports System.Windows.Controls
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports System.Data
Imports DPC.DPC.Views.Warehouse
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf
Imports System.Diagnostics
Imports System.Windows
Imports System.Windows.Media

Namespace DPC.Views.Stocks.Warehouses
    Public Class Warehouses
        Inherits UserControl

        ' Properties for pagination
        Private _paginationHelper As PaginationHelper
        Private _searchFilterHelper As SearchFilterHelper

        ' UI elements for direct access
        Private popup As Popup
        Private recentlyClosed As Boolean = False

        Public Sub New()
            InitializeComponent()
            AddHandler Me.Loaded, AddressOf Warehouses_Loaded
            InitializeControls()
        End Sub

        Private Sub Warehouses_Loaded(sender As Object, e As RoutedEventArgs)
            ' 1) Direct host window (fast)
            Dim hostWindow As Window = Window.GetWindow(Me)
            Debug.WriteLine($"[Warehouses] Window.GetWindow(Me) => {(If(hostWindow IsNot Nothing, hostWindow.GetType().FullName, "null"))}")

            ' 2) Walk up visual ancestors to find DialogHost or Window
            Dim dlgHost = FindAncestor(Of MaterialDesignThemes.Wpf.DialogHost)(Me)
            If dlgHost IsNot Nothing Then
                Debug.WriteLine($"[Warehouses] Found DialogHost ancestor. Identifier: {dlgHost.Identifier}")
            Else
                Debug.WriteLine("[Warehouses] No DialogHost ancestor found in visual tree.")
            End If

            ' 3) Enumerate Application windows and check containment
            For Each w As Window In Application.Current.Windows
                If IsDescendant(w, Me) Then
                    Debug.WriteLine($"[Warehouses] Contained in window: {w.GetType().FullName}; Title='{w.Title}'")
                End If
            Next
        End Sub

        Private Function IsDescendant(parent As DependencyObject, child As DependencyObject) As Boolean
            If parent Is child Then Return True
            Dim count = VisualTreeHelper.GetChildrenCount(parent)
            For i As Integer = 0 To count - 1
                Dim c = VisualTreeHelper.GetChild(parent, i)
                If c Is child Then Return True
                If IsDescendant(c, child) Then Return True
            Next
            Return False
        End Function

        Private Function FindAncestor(Of T As DependencyObject)(start As DependencyObject) As T
            Dim current = start
            While current IsNot Nothing
                current = VisualTreeHelper.GetParent(current)
                If current Is Nothing Then Exit While
                Dim asT = TryCast(current, T)
                If asT IsNot Nothing Then Return asT
            End While
            Return Nothing
        End Function

        Private Function FindDescendant(Of T As DependencyObject)(start As DependencyObject) As T
            If start Is Nothing Then Return Nothing
            Dim count = VisualTreeHelper.GetChildrenCount(start)
            For i As Integer = 0 To count - 1
                Dim child = VisualTreeHelper.GetChild(start, i)
                Dim asT = TryCast(child, T)
                If asT IsNot Nothing Then Return asT
                Dim nested = FindDescendant(Of T)(child)
                If nested IsNot Nothing Then Return nested
            Next
            Return Nothing
        End Function

        Public Sub InitializeControls()
            ' Find UI elements using their name
            dataGrid = TryCast(FindName("dataGrid"), DataGrid)
            txtSearch = TryCast(FindName("txtSearch"), TextBox)
            cboPageSize = TryCast(FindName("cboPageSize"), ComboBox)
            paginationPanel = TryCast(FindName("paginationPanel"), StackPanel)

            ' Verify that required controls are found
            If dataGrid Is Nothing Then
                MessageBox.Show("DataGrid not found in the XAML.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            If paginationPanel Is Nothing Then
                MessageBox.Show("Pagination panel not found in the XAML.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' Wire up event handlers
            If txtSearch IsNot Nothing Then
                AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
            End If

            If cboPageSize IsNot Nothing Then
                AddHandler cboPageSize.SelectionChanged, AddressOf CboPageSize_SelectionChanged
            End If

            ' Set up button click event
            If btnAddNew IsNot Nothing Then
                AddHandler btnAddNew.Click, AddressOf BtnAddNew_Click
            End If

            ' Initialize and load warehouses data
            LoadData()
        End Sub

        ' Event handler for TextChanged to update the filter
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _searchFilterHelper IsNot Nothing Then
                _searchFilterHelper.SearchText = txtSearch.Text
            End If
        End Sub

        ' Load Data Using WarehouseController
        Public Sub LoadData()
            Try
                ' Check if DataGrid exists
                If dataGrid Is Nothing Then
                    MessageBox.Show("DataGrid control not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Get all warehouses with error handling
                Dim allWarehouses As ObservableCollection(Of Object)
                Try
                    Dim warehouseList = WarehouseController.GetWarehouses()
                    If warehouseList Is Nothing Then
                        MessageBox.Show("Warehouse data returned null.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                        allWarehouses = New ObservableCollection(Of Object)()
                    Else
                        allWarehouses = New ObservableCollection(Of Object)(warehouseList)
                    End If
                Catch ex As Exception
                    MessageBox.Show("Error retrieving warehouse data: " & ex.Message, "Data Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    allWarehouses = New ObservableCollection(Of Object)()
                End Try

                ' Clear the pagination panel to avoid duplicate controls
                paginationPanel.Children.Clear()

                ' Initialize pagination helper with our DataGrid and pagination panel
                _paginationHelper = New PaginationHelper(dataGrid, paginationPanel)

                ' Set the items per page from the combo box if available
                If cboPageSize IsNot Nothing Then
                    Dim selectedItem = TryCast(cboPageSize.SelectedItem, ComboBoxItem)
                    If selectedItem IsNot Nothing Then
                        Dim itemsPerPageText As String = TryCast(selectedItem.Content, String)
                        Dim itemsPerPage As Integer
                        If Integer.TryParse(itemsPerPageText, itemsPerPage) Then
                            _paginationHelper.ItemsPerPage = itemsPerPage
                        End If
                    End If
                End If

                ' Set the all items to the helper
                _paginationHelper.AllItems = allWarehouses

                ' Initialize search filter helper with our pagination helper
                _searchFilterHelper = New SearchFilterHelper(_paginationHelper,
                    "ID", "Name", "TotalProducts", "StockQuantity", "Worth")

            Catch ex As Exception
                MessageBox.Show("Error in LoadData: " & ex.Message & vbCrLf & "Stack Trace: " & ex.StackTrace,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub CboPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If _paginationHelper Is Nothing Then Return

            ' Get the selected value from the ComboBox
            Dim selectedComboBoxItem As ComboBoxItem = TryCast(cboPageSize.SelectedItem, ComboBoxItem)
            If selectedComboBoxItem IsNot Nothing Then
                Dim itemsPerPageText As String = TryCast(selectedComboBoxItem.Content, String)
                Dim newItemsPerPage As Integer

                If Integer.TryParse(itemsPerPageText, newItemsPerPage) Then
                    ' Update the pagination helper's items per page
                    _paginationHelper.ItemsPerPage = newItemsPerPage
                End If
            End If
        End Sub

        ' Event Handler for Export Button Click - Using the ExcelExporter helper
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            If dataGrid Is Nothing Then Return

            ' Create a list of column headers to exclude
            Dim columnsToExclude As New List(Of String) From {"Settings"}
            ' Use the ExcelExporter helper with column exclusions
            ExcelExporter.ExportDataGridToExcel(dataGrid, columnsToExclude, "Warehouses", "Warehouses List")
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            ' Find the parent window to use as owner
            Dim parentWindow As Window = Window.GetWindow(Me)

            Dim addWarehousePopup As New AddWarehouse()

            ' Set owner if we found a parent window
            If parentWindow IsNot Nothing Then
                addWarehousePopup.Owner = parentWindow
            End If

            addWarehousePopup.ShowDialog() ' Show as modal popup

            ' Check if reload flag is set and refresh the data
            If WarehouseController.Reload Then
                LoadData()
                WarehouseController.Reload = False ' Reset the flag after reloading
            End If
        End Sub

        ' Example: OpenEditWarehouse handler — adapted to your model
        Private Async Sub OpenEditWarehouse(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, FrameworkElement)
            If btn Is Nothing Then Return

            Dim dataObj = btn.DataContext
            If dataObj Is Nothing Then Return

            Dim id As Integer
            Dim name As String = String.Empty
            Dim totalProducts As Integer? = Nothing
            Dim stockQuantity As Integer? = Nothing
            Dim worth As Decimal? = Nothing

            Dim row = TryCast(dataObj, DPC.Data.Model.Warehouses)
            If row IsNot Nothing Then
                id = row.ID
                name = If(row.Name, String.Empty)
                totalProducts = row.TotalProducts
                stockQuantity = row.StockQuantity
                worth = row.Worth
            Else
                Try
                    id = Convert.ToInt32(CallByName(dataObj, "ID", CallType.Get))
                    name = Convert.ToString(CallByName(dataObj, "Name", CallType.Get))

                    Dim tpObj = Nothing
                    Try
                        tpObj = CallByName(dataObj, "TotalProducts", CallType.Get)
                    Catch ex As Exception
                        tpObj = Nothing
                    End Try
                    If tpObj IsNot Nothing Then totalProducts = Convert.ToInt32(tpObj)

                    Dim sqObj = Nothing
                    Try
                        sqObj = CallByName(dataObj, "StockQuantity", CallType.Get)
                    Catch ex As Exception
                        sqObj = Nothing
                    End Try
                    If sqObj IsNot Nothing Then stockQuantity = Convert.ToInt32(sqObj)

                    Dim worthObj = Nothing
                    Try
                        worthObj = CallByName(dataObj, "Worth", CallType.Get)
                    Catch ex As Exception
                        worthObj = Nothing
                    End Try
                    If worthObj IsNot Nothing Then worth = Convert.ToDecimal(worthObj)

                Catch ex As Exception
                    MessageBox.Show("Unable to read selected row: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End Try
            End If

            ' Create the dialog instance (could be a UserControl or a Window depending on EditWarehouse.xaml)
            Dim dlgInstance As New DPC.Components.Forms.EditWarehouse()
            dlgInstance.Warehouses = Me
            dlgInstance.LoadWarehouseData(id, name, totalProducts, stockQuantity, worth)

            ' If the dialog class was converted to a Window, show it as a Window (cannot be hosted inside a DialogHost)
            Dim dlgAsWindow = TryCast(dlgInstance, Window)
            If dlgAsWindow IsNot Nothing Then
                Dim ownerWin As Window = Window.GetWindow(Me)
                If ownerWin IsNot Nothing Then
                    dlgAsWindow.Owner = ownerWin
                    dlgAsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner
                Else
                    dlgAsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen
                End If
                dlgAsWindow.SizeToContent = SizeToContent.WidthAndHeight
                dlgAsWindow.ShowDialog()
                InitializeControls()
                Return
            End If

            ' Otherwise it's a UserControl (or other visual) - try to show inside a DialogHost
            Dim dlgHost = FindAncestor(Of MaterialDesignThemes.Wpf.DialogHost)(Me)
            If dlgHost IsNot Nothing AndAlso Not String.IsNullOrEmpty(dlgHost.Identifier) Then
                Await DialogHost.Show(dlgInstance, dlgHost.Identifier)
                InitializeControls()
                Return
            End If

            ' Search other application windows for a DialogHost
            Dim foundIdentifier As String = Nothing
            For Each w As Window In Application.Current.Windows
                Dim root As DependencyObject = TryCast(w.Content, DependencyObject)
                If root IsNot Nothing Then
                    Dim hostInWindow = FindDescendant(Of MaterialDesignThemes.Wpf.DialogHost)(root)
                    If hostInWindow IsNot Nothing AndAlso Not String.IsNullOrEmpty(hostInWindow.Identifier) Then
                        foundIdentifier = hostInWindow.Identifier
                        Exit For
                    End If
                End If
            Next

            If Not String.IsNullOrEmpty(foundIdentifier) Then
                Await DialogHost.Show(dlgInstance, foundIdentifier)
                InitializeControls()
                Return
            End If

            ' Fallback: no DialogHost available — show dialog content in a temporary Window (dlgInstance is a visual (UserControl) here)
            Dim fallbackOwner As Window = Window.GetWindow(Me)
            Dim fallback As New Window() With {
                .Title = "Edit Warehouse",
                .Content = dlgInstance,
                .SizeToContent = SizeToContent.WidthAndHeight,
                .WindowStartupLocation = If(fallbackOwner IsNot Nothing, WindowStartupLocation.CenterOwner, WindowStartupLocation.CenterScreen),
                .Owner = fallbackOwner
            }
            fallback.ShowDialog()
            InitializeControls()
        End Sub

        Private Sub DeleteWarehouse(sender As Object, e As RoutedEventArgs)
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

            Dim deleteWarehouseWindow As New DPC.Components.ConfirmationModals.ConfirmWarehouseDeletion()

            ' Converts selected row items into brand model
            Dim warehouse As DPC.Data.Model.Warehouses = TryCast(dataGrid.SelectedItem, DPC.Data.Model.Warehouses)

            deleteWarehouseWindow.warehouseID = warehouse.ID
            deleteWarehouseWindow.Warehouse = Me

            popup.Child = deleteWarehouseWindow

            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            popup.IsOpen = True
        End Sub

        Private Sub dataGrid_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)

        End Sub
    End Class
End Namespace