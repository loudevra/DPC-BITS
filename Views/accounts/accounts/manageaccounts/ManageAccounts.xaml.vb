Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports System.Windows
Imports DPC.DPC.Data.Helpers
Imports System.IO
Imports Microsoft.Win32

Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Partial Public Class ManageAccounts
        Inherits UserControl

        ' Event to notify parent container when an account is added
        Public Event AccountAdded As EventHandler

        Public Sub New()
            InitializeComponent()
            ' Load Accounts data into DataGrid
            LoadAccounts()
        End Sub

        Private Sub LoadAccounts()
            Try
                Dim accounts = AccountController.GetAllAccounts()
                AccountsDataGrid.ItemsSource = accounts
            Catch ex As Exception
                MessageBox.Show("Error loading accounts: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub AddAccount_Click(sender As Object, e As RoutedEventArgs)
            Dim addAccountWindow As New AddAccount()

            Dim parentWindow As Window = Window.GetWindow(Me)
            ' Subscribe to the event to reload data after adding a category
            AddHandler addAccountWindow.AccountAdded, AddressOf OnAccountAdd
            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, addAccountWindow, "windowcenter", -50, 0, True, parentWindow)

        End Sub

        Private Sub OnAccountAdd(sender As Object, e As EventArgs)
            LoadAccounts() ' Reloads the accounts in the main view
            RaiseEvent AccountAdded(Me, New EventArgs()) ' Notify parent that account was added
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

        Private Sub NavigateToAddAccout(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("navaddaccount", Me)
        End Sub

        ' ---------------------------------------------------------------
        '  DATA GRID ACTIONS (Edit & Delete)
        ' ---------------------------------------------------------------

        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            ' Get the clicked button
            Dim btn As Button = CType(sender, Button)
            ' Retrieve the data context (the specific account bound to this row)
            Dim account = btn.DataContext

            ' TODO: Add your edit logic here, e.g., passing 'account' to an Edit Window
            MessageBox.Show("Edit clicked for account.", "Edit", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            ' Get the clicked button
            Dim btn As Button = CType(sender, Button)
            ' Retrieve the data context (the specific account bound to this row)
            Dim account = btn.DataContext

            ' TODO: Add your delete logic here, e.g., prompt for confirmation and remove from database
            Dim result = MessageBox.Show("Are you sure you want to delete this account?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
            If result = MessageBoxResult.Yes Then
                ' Execute delete operation
                ' LoadAccounts() ' Reload grid after deletion
            End If
        End Sub

        ' ---------------------------------------------------------------
        '  EXPORT TO EXCEL (CSV Format)
        ' ---------------------------------------------------------------
        Private Sub BtnExportExcel_Click(sender As Object, e As RoutedEventArgs)
            Try
                ' 1. Check if there is data in the grid
                If AccountsDataGrid.Items.Count = 0 Then
                    MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                ' 2. Open the Save File Dialog
                Dim saveFileDialog As New SaveFileDialog()
                saveFileDialog.Filter = "CSV (Excel Compatible) (*.csv)|*.csv"
                saveFileDialog.FileName = "Accounts_Export_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
                saveFileDialog.Title = "Export Accounts to Excel"

                ' 3. If the user clicks "Save"
                If saveFileDialog.ShowDialog() = True Then

                    ' 4. Create and write to the file
                    Using writer As New StreamWriter(saveFileDialog.FileName)
                        ' Write the Header Row matching your DataGrid columns
                        writer.WriteLine("ID,Name,Total Products,Stock Quantity,Worth (Sales/Stocks)")

                        ' 5. Loop through the current items in the DataGrid and write them
                        For Each obj In AccountsDataGrid.Items
                            If obj IsNot Nothing Then
                                ' Using Reflection to dynamically grab the properties by name
                                ' This safely handles the data regardless of your exact Model Class name!
                                Dim objType = obj.GetType()

                                Dim id = If(objType.GetProperty("ID")?.GetValue(obj, Nothing)?.ToString(), "")
                                Dim name = If(objType.GetProperty("Name")?.GetValue(obj, Nothing)?.ToString(), "").Replace("""", """""")
                                Dim totalProd = If(objType.GetProperty("TotalProd")?.GetValue(obj, Nothing)?.ToString(), "").Replace("""", """""")
                                Dim qty = If(objType.GetProperty("Qty")?.GetValue(obj, Nothing)?.ToString(), "").Replace("""", """""")
                                Dim worth = If(objType.GetProperty("Worth")?.GetValue(obj, Nothing)?.ToString(), "").Replace("""", """""")

                                writer.WriteLine($"""{id}"",""{name}"",""{totalProd}"",""{qty}"",""{worth}""")
                            End If
                        Next
                    End Using

                    MessageBox.Show("Accounts successfully exported!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                MessageBox.Show($"An error occurred while exporting: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class
End Namespace