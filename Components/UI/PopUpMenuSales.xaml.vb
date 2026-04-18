Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model

Namespace DPC.Components.UI
    Public Class PopUpMenuSales
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

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

            If sidebarWidth = 0 Then
                sidebarWidth = 260
            End If

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

            locationChangedHandler = Sub(s, e)
                                         If popup.IsOpen Then
                                             popup.HorizontalOffset = popup.HorizontalOffset
                                             popup.VerticalOffset = popup.VerticalOffset
                                         End If
                                     End Sub

            sizeChangedHandler = Sub(s, e)
                                     If popup.IsOpen Then
                                         popup.HorizontalOffset = popup.HorizontalOffset
                                         popup.VerticalOffset = popup.VerticalOffset
                                     End If
                                 End Sub

            AddHandler window.LocationChanged, locationChangedHandler
            AddHandler window.SizeChanged, sizeChangedHandler

            AddHandler popup.Closed, Sub(s, e)
                                         RemoveHandler window.LocationChanged, locationChangedHandler
                                         RemoveHandler window.SizeChanged, sizeChangedHandler
                                     End Sub
        End Sub

        ' Navigation Actions
        Private Sub NavigateToNewInvoice(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("salesnewinvoice", Me)
        End Sub

        Private Sub NavigateToNewInvoiceV2(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("salesnewposinvoicemobile", Me)
        End Sub

        Private Sub NavigateToManageInvoices(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("manageposinvoices", Me)
        End Sub

        Private Sub NavigateToNewQuote(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToCachedView("salesnewquote", Me)
        End Sub

        Private Sub NavigateToManageQuote(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("salesquote", Me)
        End Sub

        Private Sub NavigateToManageWalkInClients(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("managewalkin", Me)
        End Sub

        Private Sub NavigateToNewDeliveryReceipt(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToCachedView("newdelivery", Me)
        End Sub

        Private Sub NavigateToManageDeliveryReceipt(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("managedeliveryreceipt", Me)
        End Sub

        Private Sub NavigateToNewQuoteGovernment(sender As Object, e As RoutedEventArgs)
            CostEstimateDetails.CEGovCETitle = "Cost Estimate"
            CostEstimateDetails.CEGovCEButton = "Generate Cost Estimate"
            If Application.Current.Properties.Contains("QuoteCache") Then
                Application.Current.Properties.Remove("QuoteCache")
            End If
            ViewLoader.DynamicView.NavigateToView("salesquotegovernment", Me)
        End Sub

        Private Sub NavigateToManageQuoteGovernment(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("salesquotegovernmentmanage", Me)
        End Sub

        Private Sub NavigateToNewSubscription(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("newsubscriptions", Me)
        End Sub

        Private Sub NavigateToSubscriptions(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("subscriptions", Me)
        End Sub

        Private Sub NavigateToCreditNotes(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("creditnote", Me)
        End Sub

        Private Sub NavigateToNewPOSInvoice(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("salesnewposinvoice", Me)
        End Sub

        Private Sub NavigateToWalkIn(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToCachedView("walkinorder", Me)
        End Sub

        Private Sub NavigateToStatementOfAccount(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("statementofaccount", Me)
        End Sub

        Private Sub NavigateToManageStatementOfAccount(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("managestatementofaccount", Me)
        End Sub
    End Class
End Namespace