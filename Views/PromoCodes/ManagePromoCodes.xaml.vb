Imports System.Data
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers ' Make sure this is the correct namespace for ViewLoader

Namespace DPC.Views.PromoCodes
    Partial Public Class ManagePromoCodes
        Inherits UserControl

        ' Stats counters for the dashboard
        Private activeCount As Integer = 0
        Private usedCount As Integer = 0
        Private expiredCount As Integer = 0
        Private totalCount As Integer = 0

        Public Sub New()
            InitializeComponent()
            LoadPromoCodes()
            LoadPromoCodeStats()
        End Sub

        Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
            ' Open the Add Promo Code window or navigate to that view

        End Sub

        Private Sub LoadPromoCodes()
            Try
                ' Use the PromoCodeController to fetch data
                Dim dt As DataTable = PromoCodeController.FetchPromoCodes()

                If dt IsNot Nothing Then
                    'dgPromoCodes.ItemsSource = dt.DefaultView
                End If
            Catch ex As Exception
                MessageBox.Show($"Error loading promo codes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub LoadPromoCodeStats()
            Try
                ' Get stats using the PromoCodeController
                Dim stats As Dictionary(Of String, Integer) = PromoCodeController.GetPromoCodeStats()

                ' Update UI elements with stats
                If stats IsNot Nothing Then
                    activeCount = stats("Active")
                    usedCount = stats("Used")
                    expiredCount = stats("Expired")
                    totalCount = stats("Total")

                    ' Update the TextBlocks in the UI
                    'txtActiveCount.Text = activeCount.ToString()
                    'txtUsedCount.Text = usedCount.ToString()
                    'txtExpiredCount.Text = expiredCount.ToString()
                    'txtTotalCount.Text = totalCount.ToString()
                End If
            Catch ex As Exception
                MessageBox.Show($"Error loading promo code statistics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub BtnView_Click(sender As Object, e As RoutedEventArgs)
            ' Get the selected promo code
            Dim row As DataRowView = TryCast(CType(sender, Button).DataContext, DataRowView)
            If row IsNot Nothing Then
                Dim id As Integer = Convert.ToInt32(row("ID"))
                ViewPromoCode(id)
            End If
        End Sub

        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            ' Get the selected promo code
            Dim row As DataRowView = TryCast(CType(sender, Button).DataContext, DataRowView)
            If row IsNot Nothing Then
                Dim id As Integer = Convert.ToInt32(row("ID"))
                EditPromoCode(id)
            End If
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            ' Get the selected promo code
            Dim row As DataRowView = TryCast(CType(sender, Button).DataContext, DataRowView)
            If row IsNot Nothing Then
                Dim id As Integer = Convert.ToInt32(row("ID"))
                Dim code As String = row("Code").ToString()

                ' Confirm deletion
                Dim result As MessageBoxResult = MessageBox.Show($"Are you sure you want to delete promo code '{code}'?",
                                                                "Confirm Deletion",
                                                                MessageBoxButton.YesNo,
                                                                MessageBoxImage.Question)

                If result = MessageBoxResult.Yes Then
                    ' Delete the promo code
                    If PromoCodeController.DeletePromoCode(id) Then
                        LoadPromoCodes()
                        LoadPromoCodeStats()
                        MessageBox.Show("Promo code deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    End If
                End If
            End If
        End Sub

        Private Sub ViewPromoCode(id As Integer)
            ' Get the promo code details

        End Sub

        Private Sub EditPromoCode(id As Integer)

        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Filter the DataGrid based on search text
            Dim searchText As String = txtSearch.Text.ToLower()

            'If dgPromoCodes.ItemsSource IsNot Nothing Then
            '    Dim view As DataView = TryCast(dgPromoCodes.ItemsSource, DataView)

            '    If view IsNot Nothing Then
            '        If String.IsNullOrWhiteSpace(searchText) Then
            '            view.RowFilter = ""
            '        Else
            '            ' Filter by Code (add more columns as needed)
            '            view.RowFilter = $"Code LIKE '%{searchText}%' OR Account LIKE '%{searchText}%' OR Note LIKE '%{searchText}%'"
            '        End If
            '    End If
            'End If
        End Sub

        Private Sub BtnExport_Click(sender As Object, e As RoutedEventArgs)
            ' Export functionality can be implemented here
            ' This could export to CSV or Excel
            MessageBox.Show("Export functionality will be implemented here.", "Export", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub CboPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Implementation for changing the number of items shown per page
            ' This would need pagination logic
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

        Private Sub NavigateToAddPromoCode(sender As Object, e As RoutedEventArgs)
            ' This assumes ViewLoader.DynamicView.NavigateToView is the correct navigation method.
            ViewLoader.DynamicView.NavigateToView("addpromocode", Me)
        End Sub
    End Class
End Namespace