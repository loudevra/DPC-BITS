Imports System.Windows.Controls.Primitives
Imports System.Windows
Imports DPC.DPC.Data.Helpers

Namespace DPC.Components.UI
    Public Class PopUpMenuDataReports
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
                popup.VerticalOffset = -button.ActualHeight * 6.301
            Else
                ' Sidebar is expanded - position menu immediately to the right
                popup.HorizontalOffset = sidebarWidth - button.Margin.Left
                popup.VerticalOffset = -button.ActualHeight * 6.301
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

        ''' <summary>
        ''' Navigate to Account Statements
        ''' </summary>
        Private Sub NavigateToAccountStatements(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("accountstatements", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Customer Account Statements
        ''' </summary>
        Private Sub NavigateToCustomerAccountStatements(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("customeraccountstatements", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Supplier Account Statements
        ''' </summary>
        Private Sub NavigateToSupplierAccountStatements(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("supplieraccountstatements", Me)
        End Sub

        ''' <summary>
        ''' Navigate to TAX Statements
        ''' </summary>
        Private Sub NavigateToTaxStatements(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("taxstatements", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Product Sales Reports
        ''' </summary>
        Private Sub NavigateToProductSalesReports(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("productsalesreports", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Product Categories Report
        ''' </summary>
        Private Sub NavigateToProductCategoriesReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("productcategoriesreport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Trending Products Report
        ''' </summary>
        Private Sub NavigateToTrendingProductsReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("trendingproductsreport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Profit Report
        ''' </summary>
        Private Sub NavigateToProfitReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("profitreport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Top Customers Report
        ''' </summary>
        Private Sub NavigateToTopCustomersReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("topcustomersreport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Income vs Expenses Report
        ''' </summary>
        Private Sub NavigateToIncomeVsExpensesReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("incomevsexpensesreport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Income Report
        ''' </summary>
        Private Sub NavigateToIncomeReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("incomereport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Expenses Report
        ''' </summary>
        Private Sub NavigateToExpensesReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("expensesreport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Statistics Report
        ''' </summary>
        Private Sub NavigateToStatisticsReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("statisticsreport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Calculate Income Report
        ''' </summary>
        Private Sub NavigateToCalculateIncomeReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("calculateincome", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Calculate Expenses Report
        ''' </summary>
        Private Sub NavigateToCalculateExpensesReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("calculateexpenses", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Sales Report
        ''' </summary>
        Private Sub NavigateToSalesReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("salesreport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Products Report
        ''' </summary>
        Private Sub NavigateToProductsReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("productsreport", Me)
        End Sub

        ''' <summary>
        ''' Navigate to Employee Commission Report
        ''' </summary>
        Private Sub NavigateToEmployeeCommissionReport(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("employeecommission", Me)
        End Sub
        Private Sub NavigateToBusinessregister(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("businnessregisters", Me)
        End Sub

        Private Sub NavigateToGenerateStatement(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("generatestatement", Me)
        End Sub

        Private Sub NavigateToFiles(sender As Object, e As RoutedEventArgs)
            ' Open Payroll view
            ViewLoader.DynamicView.NavigateToView("hrmeditfiles", Me)
        End Sub






        ''' <summary>
        ''' This method is used in the XAML to handle navigation for all buttons
        ''' You would need to modify the XAML to call each specific method instead of one general method
        ''' </summary>
        Private Sub NavigateToNewInvoice(sender As Object, e As RoutedEventArgs)
            ' This is the existing general handler - you should replace calls to this
            ' with the specific navigation methods above
            MessageBox.Show("This is a placeholder. Replace with specific navigation.", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub
    End Class
End Namespace