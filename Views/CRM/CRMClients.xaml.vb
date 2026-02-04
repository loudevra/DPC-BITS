Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Models
Imports DPC.Data.Helpers.ViewLoader

Namespace DPC.Views.CRM


    Public Class CRMClients

        Private _typingTimer As DispatcherTimer
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            LoadClients()

            _typingTimer = New DispatcherTimer With {
               .Interval = TimeSpan.FromMilliseconds(250)
           }

            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick
        End Sub

        Private Sub LoadClients()
            If String.IsNullOrWhiteSpace(SearchTxt.Text) Then
                dataGrid.ItemsSource = Nothing
                dataGrid.ItemsSource = ClientController.GetAllClients()
            Else
                dataGrid.ItemsSource = Nothing
                dataGrid.ItemsSource = ClientController.SearchClients(SearchTxt.Text)
            End If

        End Sub

        Private Sub DataGrid_CellClick(sender As Object, e As MouseButtonEventArgs)
            Dim depObj As DependencyObject = TryCast(e.OriginalSource, DependencyObject)

            Dim cell = TryCast(depObj, TextBlock)

            If TypeOf cell Is TextBlock Then
                ' Show popup near the clicked cell
                PopupText.Text = cell.Text
                CellValuePopup.PlacementTarget = sender
                CellValuePopup.IsOpen = True
            End If

        End Sub

        Private Sub SearchText_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Reset the timer
            _typingTimer.Stop()

            ' Start the timer
            _typingTimer.Start()
        End Sub

        Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
            ' Stop the timer
            _typingTimer.Stop()

            LoadClients()
        End Sub

        Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
            ' Check if DataGrid has data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Use the ExcelExporter helper with column exclusions
            ExcelExporter.ExportDataGridToExcel(dataGrid, "ClientsExport", "Clients List")

        End Sub

        Friend Sub ShowPopup(parent As UIElement, sender As Object)
            ' Ensure sender is a Button
            Dim button As Button = TryCast(sender, Button)
            If button Is Nothing Then
                Return
            End If

            ' Get the window containing the button
            Dim window As Window = Window.GetWindow(button)
            If window Is Nothing Then
                Return
            End If

            ' Get sidebar width - determine if sidebar is expanded or collapsed
            Dim sidebarWidth As Double = 0

            ' Get parent sidebar if available
            Dim parentControl = TryCast(button.Parent, FrameworkElement)
            While parentControl IsNot Nothing
                If TypeOf parentControl Is StackPanel AndAlso parentControl.Name = "SidebarMenu" Then
                    ' Found the sidebar menu container, get its parent (likely the sidebar)
                    Dim sidebarContainer = TryCast(parentControl.Parent, FrameworkElement)
                    If sidebarContainer IsNot Nothing Then
                        sidebarWidth = sidebarContainer.ActualWidth
                        Exit While
                    End If
                ElseIf TypeOf parentControl.Parent Is DPC.Components.Navigation.Sidebar Then
                    ' Direct parent is sidebar
                    sidebarWidth = CType(parentControl.Parent, FrameworkElement).ActualWidth
                    Exit While
                End If
                parentControl = TryCast(parentControl.Parent, FrameworkElement)
            End While

            ' If we couldn't find sidebar, use a default value
            If sidebarWidth = 0 Then
                ' Default to expanded sidebar width
                sidebarWidth = 260
            End If

            ' Create the popup with proper positioning
            Dim popup As New Popup With {
                .Child = Me,
                .StaysOpen = False,
                .Placement = PlacementMode.Relative,
                .PlacementTarget = button,
                .IsOpen = True,
                .AllowsTransparency = True
            }

            ' Calculate optimal position based on sidebar width
            If sidebarWidth <= 80 Then
                ' Sidebar is collapsed - position menu farther right
                popup.HorizontalOffset = 60
                popup.VerticalOffset = -button.ActualHeight * 3 ' Align with button
            Else
                ' Sidebar is expanded - position menu immediately to the right
                popup.HorizontalOffset = sidebarWidth - button.Margin.Left
                popup.VerticalOffset = -button.ActualHeight * 3 ' Align with button
            End If

            ' Store references to event handlers so we can remove them later
            Dim locationChangedHandler As EventHandler = Nothing
            Dim sizeChangedHandler As SizeChangedEventHandler = Nothing

            ' Define event handlers
            locationChangedHandler = Sub(s, e)
                                         If popup.IsOpen Then
                                             ' Recalculate position when window moves
                                             popup.HorizontalOffset = popup.HorizontalOffset
                                             popup.VerticalOffset = popup.VerticalOffset
                                         End If
                                     End Sub

            sizeChangedHandler = Sub(s, e)
                                     If popup.IsOpen Then
                                         ' Recalculate position when window resizes
                                         popup.HorizontalOffset = popup.HorizontalOffset
                                         popup.VerticalOffset = popup.VerticalOffset
                                     End If
                                 End Sub

            ' Add event handlers
            AddHandler window.LocationChanged, locationChangedHandler
            AddHandler window.SizeChanged, sizeChangedHandler

            ' Handle popup closed to cleanup event handlers
            AddHandler popup.Closed, Sub(s, e)
                                         RemoveHandler window.LocationChanged, locationChangedHandler
                                         RemoveHandler window.SizeChanged, sizeChangedHandler
                                     End Sub
        End Sub

        Private Sub NavigateToSelectClients(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("selectclients", Me)
        End Sub



        Private Sub DeleteCRMclient(sender As Object, e As RoutedEventArgs)
            Try
                ' Get the button that was clicked
                Dim button As Button = TryCast(sender, Button)
                If button Is Nothing Then Return

                ' Get the data context (the client object) from the button
                Dim client As Client = TryCast(button.DataContext, Client)
                If client Is Nothing Then Return

                ' Confirm deletion with user
                Dim result As MessageBoxResult = MessageBox.Show(
            $"Are you sure you want to delete client '{client.Name}'?",
            "Confirm Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question)

                If result <> MessageBoxResult.Yes Then
                    Return
                End If

                ' Call delete method from ClientController
                Dim success As Boolean = ClientController.DeleteClient(client.ClientID.ToString())

                If success Then
                    MessageBox.Show("Client deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    LoadClients() ' Refresh the client list
                Else
                    MessageBox.Show("Failed to delete client.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If

            Catch ex As Exception
                MessageBox.Show($"Error deleting client: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub OpenEditCRMclient(sender As Object, e As RoutedEventArgs)
            Try
                ' Get the button that was clicked
                Dim button As Button = TryCast(sender, Button)
                If button Is Nothing Then Return

                ' Get the data context (the client object) from the button
                Dim client As Client = TryCast(button.DataContext, Client)
                If client Is Nothing Then Return

                System.Diagnostics.Debug.WriteLine($"DEBUG: Clicked edit for ClientID = {client.ClientID}")
                System.Diagnostics.Debug.WriteLine($"DEBUG: ClientID Type = {client.ClientID.GetType()}")

                ' Get the full client details from database
                Dim clientIDString As String = client.ClientID.ToString()
                System.Diagnostics.Debug.WriteLine($"DEBUG: Calling GetClientByID with: {clientIDString}")

                Dim fullClient As Client = ClientController.GetClientByID(clientIDString)

                If fullClient Is Nothing Then
                    System.Diagnostics.Debug.WriteLine($"DEBUG: GetClientByID returned NULL for ID: {clientIDString}")
                    MessageBox.Show($"Failed to load client details. ClientID: {clientIDString}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                Else
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Got client: {fullClient.Name}")
                End If

                ' Determine client type and navigate accordingly
                If client.ClientType = "Residential" Then
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Creating edit form for Residential client with ID: {clientIDString}")

                    ' Create the edit form
                    Dim editForm As New CRMEditResidentialClient()

                    ' Set the ClientID before navigation
                    editForm.SetClientID(clientIDString)

                    System.Diagnostics.Debug.WriteLine($"DEBUG: Navigating to editresidentialclient")

                    ' Navigate to the residential client edit form
                    ViewLoader.DynamicView.NavigateToView("editresidentialclient", editForm)

                ElseIf client.ClientType = "Corporate" Then
                    MessageBox.Show("Corporate client editing is not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"DEBUG: Exception in OpenEditCRMclient: {ex.Message}")
                System.Diagnostics.Debug.WriteLine($"DEBUG: StackTrace: {ex.StackTrace}")
                MessageBox.Show($"Error opening client for editing: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub PopulateResidentialClientDetails(client As Client)
            ' Parse the billing address (assuming format: "Address, City, Region, Country, ZipCode")
            Dim billingParts As String() = If(String.IsNullOrEmpty(client.BillingAddress),
                                      New String() {},
                                      client.BillingAddress.Split(New String() {", "}, StringSplitOptions.None))
            Dim shippingParts As String() = If(String.IsNullOrEmpty(client.ShippingAddress),
                                       New String() {},
                                       client.ShippingAddress.Split(New String() {", "}, StringSplitOptions.None))

            ' Populate the ResidentialClientDetails module
            ResidentialClientDetails.ClientName = client.Name
            ResidentialClientDetails.Phone = client.Phone
            ResidentialClientDetails.Email = client.Email

            ' Billing Address
            ResidentialClientDetails.BillAddress = If(billingParts.Length > 0, billingParts(0), "")
            ResidentialClientDetails.BillCity = If(billingParts.Length > 1, billingParts(1), "")
            ResidentialClientDetails.BillRegion = If(billingParts.Length > 2, billingParts(2), "")
            ResidentialClientDetails.BillCountry = If(billingParts.Length > 3, billingParts(3), "")
            ResidentialClientDetails.BillZipCode = If(billingParts.Length > 4, billingParts(4), "")

            ' Shipping Address
            ResidentialClientDetails.Address = If(shippingParts.Length > 0, shippingParts(0), "")
            ResidentialClientDetails.City = If(shippingParts.Length > 1, shippingParts(1), "")
            ResidentialClientDetails.Region = If(shippingParts.Length > 2, shippingParts(2), "")
            ResidentialClientDetails.Country = If(shippingParts.Length > 3, shippingParts(3), "")
            ResidentialClientDetails.ZipCode = If(shippingParts.Length > 4, shippingParts(4), "")

            ' Other details
            ResidentialClientDetails.ClientGroupID = client.ClientGroupID
            ResidentialClientDetails.CustomerGroup = client.CustomerGroup
            ResidentialClientDetails.CustomerLanguage = client.ClientLanguage
            ResidentialClientDetails.SameAsBilling = (client.BillingAddress = client.ShippingAddress)
        End Sub

        Private Sub PopulateCorporateClientDetails(client As Client)
            ' Parse the billing address (assuming format: "Address, City, Region, Country, ZipCode")
            Dim billingParts As String() = If(String.IsNullOrEmpty(client.BillingAddress),
                                      New String() {},
                                      client.BillingAddress.Split(New String() {", "}, StringSplitOptions.None))
            Dim shippingParts As String() = If(String.IsNullOrEmpty(client.ShippingAddress),
                                       New String() {},
                                       client.ShippingAddress.Split(New String() {", "}, StringSplitOptions.None))

            ' TODO: Create a CorporateClientDetails module similar to ResidentialClientDetails
            ' For now, this is a placeholder. You'll need to implement this based on your corporate client structure

            ' Example structure (adjust based on your actual corporate client properties):
            ' CorporateClientDetails.Company = client.Company
            ' CorporateClientDetails.Representative = client.Representative
            ' ... etc
        End Sub
    End Class
End Namespace
