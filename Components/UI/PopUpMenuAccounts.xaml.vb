Imports System.Windows.Controls.Primitives
Imports System.Windows
Imports DPC.DPC.Data.Helpers

Namespace DPC.Components.UI
    Public Class PopUpMenuAccounts
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
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

        Private Sub NavigateToManageAccounts(sender As Object, e As RoutedEventArgs)
            Dim ManageAcctWindow As New Views.Accounts.Accounts.ManageAccounts.ManageAccounts()
            ManageAcctWindow.Show()

            ' Close the current window where this UserControl is being used
            Dim currentWindow As Window = Window.GetWindow(Me)
            currentWindow?.Close()
        End Sub

        Private Sub NavigateToBalanceSheets(sender As Object, e As RoutedEventArgs)
            ' Implementation for navigating to Balance Sheets
            ViewLoader.DynamicView.NavigateToView("balancesheets", Me)
        End Sub

        Private Sub NavigateToAccountStatement(sender As Object, e As RoutedEventArgs)
            ' Implementation for navigating to Account Statement
            ViewLoader.DynamicView.NavigateToView("accountstatement", Me)
        End Sub

        Private Sub NavigateToViewTransactions(sender As Object, e As RoutedEventArgs)
            ' Implementation for navigating to View Transactions
            ViewLoader.DynamicView.NavigateToView("viewtransactions", Me)
        End Sub

        Private Sub NavigateToNewTransaction(sender As Object, e As RoutedEventArgs)

        End Sub

        Private Sub NavigateToNewTransfer(sender As Object, e As RoutedEventArgs)

        End Sub

        Private Sub NavigateToClientsTransaction(sender As Object, e As RoutedEventArgs)
            ' Implementation for navigating to Clients Transaction
            ViewLoader.DynamicView.NavigateToView("clientstransaction", Me)
        End Sub

        Private Sub NavigateToIncome(sender As Object, e As RoutedEventArgs)
            ' Implementation for navigating to Income
            ViewLoader.DynamicView.NavigateToView("income", Me)
        End Sub

        Private Sub NavigateToExpense(sender As Object, e As RoutedEventArgs)
            ' Implementation for navigating to Expense
            ViewLoader.DynamicView.NavigateToView("expense", Me)
        End Sub
    End Class
End Namespace