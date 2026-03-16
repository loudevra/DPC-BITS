' CRMClients.xaml.vb
Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports DPC.Data.Helpers.ViewLoader

Namespace DPC.Views.CRM

    Public Class CRMClients

        ' Pagination & Search helpers (same pattern as ManageSuppliers)
        Private _paginationHelper As PaginationHelper
        Private _searchFilterHelper As SearchFilterHelper

        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()

            ' Wire up events and load data
            AddHandler Me.Loaded, AddressOf OnViewLoaded
        End Sub

        Private Sub OnViewLoaded(sender As Object, e As RoutedEventArgs)
            LoadClients()
        End Sub

        ' ---------------------------------------------------------------
        '  LOAD DATA  (mirrors ManageSuppliers.LoadData exactly)
        ' ---------------------------------------------------------------
        Private Sub LoadClients()
            Try
                ' Fetch all clients
                Dim allClients As ObservableCollection(Of Object)
                Try
                    Dim clientList = ClientController.GetAllClients()
                    If clientList Is Nothing Then
                        allClients = New ObservableCollection(Of Object)()
                    Else
                        allClients = New ObservableCollection(Of Object)(clientList)
                    End If
                Catch ex As Exception
                    MessageBox.Show("Error retrieving client data: " & ex.Message, "Data Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    allClients = New ObservableCollection(Of Object)()
                End Try

                ' Clear pagination panel to avoid duplicate controls
                paginationPanel.Children.Clear()

                ' Initialize PaginationHelper with DataGrid and pagination panel
                _paginationHelper = New PaginationHelper(dataGrid, paginationPanel)

                ' Apply page size from ComboBox
                If cboPageSize IsNot Nothing Then
                    Dim selectedItem = TryCast(cboPageSize.SelectedItem, ComboBoxItem)
                    If selectedItem IsNot Nothing Then
                        Dim pageText As String = TryCast(selectedItem.Content, String)
                        Dim pageSize As Integer
                        If Integer.TryParse(pageText, pageSize) Then
                            _paginationHelper.ItemsPerPage = pageSize
                        End If
                    End If
                End If

                ' Hand all items to the pagination helper
                _paginationHelper.AllItems = allClients

                ' Initialize SearchFilterHelper — list every searchable property
                _searchFilterHelper = New SearchFilterHelper(_paginationHelper,
                    "ClientID", "Name", "ClientType", "BillingAddress", "Email", "Phone")

            Catch ex As Exception
                MessageBox.Show("Error in LoadClients: " & ex.Message & vbCrLf & ex.StackTrace,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' ---------------------------------------------------------------
        '  SEARCH  — just push text into the helper; it handles the rest
        ' ---------------------------------------------------------------
        Private Sub SearchText_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _searchFilterHelper IsNot Nothing Then
                _searchFilterHelper.SearchText = SearchTxt.Text
            End If
        End Sub

        ' ---------------------------------------------------------------
        '  PAGE SIZE COMBO
        ' ---------------------------------------------------------------
        Private Sub CboPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If _paginationHelper Is Nothing Then Return

            Dim selected As ComboBoxItem = TryCast(cboPageSize.SelectedItem, ComboBoxItem)
            If selected IsNot Nothing Then
                Dim pageText As String = TryCast(selected.Content, String)
                Dim newSize As Integer
                If Integer.TryParse(pageText, newSize) Then
                    _paginationHelper.ItemsPerPage = newSize
                End If
            End If
        End Sub

        ' ---------------------------------------------------------------
        '  CELL CLICK POPUP
        ' ---------------------------------------------------------------
        Private Sub DataGrid_CellClick(sender As Object, e As MouseButtonEventArgs)
            Dim depObj As DependencyObject = TryCast(e.OriginalSource, DependencyObject)
            Dim cell = TryCast(depObj, TextBlock)

            If TypeOf cell Is TextBlock Then
                PopupText.Text = cell.Text
                CellValuePopup.PlacementTarget = sender
                CellValuePopup.IsOpen = True
            End If
        End Sub

        ' ---------------------------------------------------------------
        '  EXPORT
        ' ---------------------------------------------------------------
        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If
            ExcelExporter.ExportDataGridToExcel(dataGrid, "ClientsExport", "Clients List")
        End Sub

        ' ---------------------------------------------------------------
        '  NAVIGATION
        ' ---------------------------------------------------------------
        Private Sub NavigateToSelectClients(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("selectclients", Me)
        End Sub

        ' ---------------------------------------------------------------
        '  DELETE CLIENT
        ' ---------------------------------------------------------------
        Private Sub DeleteCRMclient(sender As Object, e As RoutedEventArgs)
            Try
                Dim button As Button = TryCast(sender, Button)
                If button Is Nothing Then Return

                Dim client As Client = TryCast(button.DataContext, Client)
                If client Is Nothing Then Return

                Dim result As MessageBoxResult = MessageBox.Show(
                    $"Are you sure you want to delete client '{client.Name}'?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question)

                If result <> MessageBoxResult.Yes Then Return

                Dim success As Boolean = ClientController.DeleteClient(client.ClientID.ToString())

                If success Then
                    MessageBox.Show("Client deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    LoadClients()
                Else
                    MessageBox.Show("Failed to delete client.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If

            Catch ex As Exception
                MessageBox.Show($"Error deleting client: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' ---------------------------------------------------------------
        '  EDIT CLIENT
        ' ---------------------------------------------------------------
        Private Sub OpenEditCRMclient(sender As Object, e As RoutedEventArgs)
            Try
                Dim button As Button = TryCast(sender, Button)
                If button Is Nothing Then Return

                Dim clientPreview As Client = TryCast(button.DataContext, Client)
                If clientPreview Is Nothing Then Return

                Dim clientIDString As String = clientPreview.ClientID.ToString()
                Dim fullClient As Client = ClientController.GetClientByID(clientIDString)

                If fullClient Is Nothing Then
                    MessageBox.Show($"Failed to load client details. ClientID: {clientIDString}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                PopulateResidentialClientDetails(fullClient)

                Try
                    Dim mainWindow As DPC.Base = Nothing
                    For Each w As Window In Application.Current.Windows
                        If TypeOf w Is DPC.Base Then
                            mainWindow = DirectCast(w, DPC.Base)
                            Exit For
                        End If
                    Next

                    If mainWindow IsNot Nothing Then
                        Dim editView As New CRM.CRMEditResidentialClient()
                        editView.SetClientID(clientIDString)
                        mainWindow.CurrentView = editView

                        Dim current As DependencyObject = TryCast(sender, DependencyObject)
                        While current IsNot Nothing
                            Dim parentPopup = TryCast(current, Popup)
                            If parentPopup IsNot Nothing Then
                                parentPopup.IsOpen = False
                                Exit While
                            End If
                            Dim fe = TryCast(current, FrameworkElement)
                            If fe IsNot Nothing AndAlso fe.Parent IsNot Nothing Then
                                current = fe.Parent
                            ElseIf VisualTreeHelper.GetParent(current) IsNot Nothing Then
                                current = VisualTreeHelper.GetParent(current)
                            Else
                                current = Nothing
                            End If
                        End While
                    Else
                        ViewLoader.DynamicView.NavigateToView("editresidentialclient", Me)
                    End If
                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine($"Error navigating to edit view: {ex.Message}")
                    ViewLoader.DynamicView.NavigateToView("editresidentialclient", Me)
                End Try
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"DEBUG: Exception in OpenEditCRMclient: {ex.Message}")
                MessageBox.Show($"Error opening client for editing: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' ---------------------------------------------------------------
        '  POPULATE HELPERS
        ' ---------------------------------------------------------------
        Private Sub PopulateResidentialClientDetails(client As Client)
            Dim billingParts As String() = If(String.IsNullOrEmpty(client.BillingAddress),
                                              New String() {},
                                              client.BillingAddress.Split(New String() {", "}, StringSplitOptions.None))
            Dim shippingParts As String() = If(String.IsNullOrEmpty(client.ShippingAddress),
                                               New String() {},
                                               client.ShippingAddress.Split(New String() {", "}, StringSplitOptions.None))

            ResidentialClientDetails.ClientName = client.Name
            ResidentialClientDetails.Phone = client.Phone
            ResidentialClientDetails.Email = client.Email

            ResidentialClientDetails.BillAddress = If(billingParts.Length > 0, billingParts(0), "")
            ResidentialClientDetails.BillCity = If(billingParts.Length > 1, billingParts(1), "")
            ResidentialClientDetails.BillRegion = If(billingParts.Length > 2, billingParts(2), "")
            ResidentialClientDetails.BillCountry = If(billingParts.Length > 3, billingParts(3), "")
            ResidentialClientDetails.BillZipCode = If(billingParts.Length > 4, billingParts(4), "")

            ResidentialClientDetails.Address = If(shippingParts.Length > 0, shippingParts(0), "")
            ResidentialClientDetails.City = If(shippingParts.Length > 1, shippingParts(1), "")
            ResidentialClientDetails.Region = If(shippingParts.Length > 2, shippingParts(2), "")
            ResidentialClientDetails.Country = If(shippingParts.Length > 3, shippingParts(3), "")
            ResidentialClientDetails.ZipCode = If(shippingParts.Length > 4, shippingParts(4), "")

            ResidentialClientDetails.ClientGroupID = client.ClientGroupID
            ResidentialClientDetails.CustomerGroup = client.CustomerGroup
            ResidentialClientDetails.CustomerLanguage = client.ClientLanguage
            ResidentialClientDetails.SameAsBilling = (client.BillingAddress = client.ShippingAddress)
        End Sub

        Private Sub PopulateCorporateClientDetails(client As Client)
            ' TODO: implement corporate client details population
        End Sub

        ' ---------------------------------------------------------------
        '  SHOW POPUP  (sidebar-aware positioning — unchanged)
        ' ---------------------------------------------------------------
        Friend Sub ShowPopup(parent As UIElement, sender As Object)
            Dim button As Button = TryCast(sender, Button)
            If button Is Nothing Then Return

            Dim window As Window = Window.GetWindow(button)
            If window Is Nothing Then Return

            Dim sidebarWidth As Double = 0
            Dim parentControl = TryCast(button.Parent, FrameworkElement)
            While parentControl IsNot Nothing
                If TypeOf parentControl Is StackPanel AndAlso parentControl.Name = "SidebarMenu" Then
                    Dim sidebarContainer = TryCast(parentControl.Parent, FrameworkElement)
                    If sidebarContainer IsNot Nothing Then
                        sidebarWidth = sidebarContainer.ActualWidth
                        Exit While
                    End If
                ElseIf TypeOf parentControl.Parent Is DPC.Components.Navigation.Sidebar Then
                    sidebarWidth = CType(parentControl.Parent, FrameworkElement).ActualWidth
                    Exit While
                End If
                parentControl = TryCast(parentControl.Parent, FrameworkElement)
            End While

            If sidebarWidth = 0 Then sidebarWidth = 260

            Dim popup As New Popup With {
                .Child = Me,
                .StaysOpen = False,
                .Placement = PlacementMode.Relative,
                .PlacementTarget = button,
                .IsOpen = True,
                .AllowsTransparency = True
            }

            If sidebarWidth <= 80 Then
                popup.HorizontalOffset = 60
                popup.VerticalOffset = -button.ActualHeight * 3
            Else
                popup.HorizontalOffset = sidebarWidth - button.Margin.Left
                popup.VerticalOffset = -button.ActualHeight * 3
            End If

            Dim locationChangedHandler As EventHandler = Nothing
            Dim sizeChangedHandler As SizeChangedEventHandler = Nothing

            locationChangedHandler = Sub(s, ev)
                                         If popup.IsOpen Then
                                             popup.HorizontalOffset = popup.HorizontalOffset
                                             popup.VerticalOffset = popup.VerticalOffset
                                         End If
                                     End Sub

            sizeChangedHandler = Sub(s, ev)
                                     If popup.IsOpen Then
                                         popup.HorizontalOffset = popup.HorizontalOffset
                                         popup.VerticalOffset = popup.VerticalOffset
                                     End If
                                 End Sub

            AddHandler window.LocationChanged, locationChangedHandler
            AddHandler window.SizeChanged, sizeChangedHandler

            AddHandler popup.Closed, Sub(s, ev)
                                         RemoveHandler window.LocationChanged, locationChangedHandler
                                         RemoveHandler window.SizeChanged, sizeChangedHandler
                                     End Sub
        End Sub

    End Class
End Namespace
