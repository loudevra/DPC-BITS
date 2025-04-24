Imports DPC.DPC.Views
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace DPC.Data.Helpers.ViewLoader
    ''' <summary>
    ''' Responsible for loading view components by name
    ''' </summary>
    Public Class ViewLoader
        ''' <summary>
        ''' Loads the requested view by name
        ''' </summary>
        ''' <param name="viewName">Name of the view to load</param>
        ''' <returns>UserControl representing the requested view</returns>
        Public Shared Function Load(viewName As String) As UserControl
            Try
                Select Case viewName.ToLower()
                    Case "dashboard"
                        Return New Dashboard.Dashboard() ' This is a UserControl
                    Case "stockstransfer"
                        Return New Stocks.StocksTransfer.StocksTransfer() ' This is a UserControl
                    Case "newsuppliers"
                        Return New Stocks.Supplier.NewSuppliers.NewSuppliers() ' This is now a UserControl
                    Case "managesuppliers"
                        Return New Stocks.Suppliers.ManageSuppliers.ManageSuppliers() ' This is now a UserControl
                    Case "managebrands"
                        Return New Stocks.Suppliers.ManageBrands.ManageBrands() ' This is now a UserControl
                    Case "warehouses"
                        Return New Stocks.Warehouses.Warehouses() ' Added the Warehouses UserControl
                    Case "productcategories"
                        Return New Stocks.ProductCategories.ProductCategories() ' Added the ProductCategories UserControl
                    Case "promocodes"
                        Return New PromoCodes.ManagePromoCodes() ' Added the ManagePromoCodes UserControl
                    Case "manageproducts"
                        Return New Stocks.ItemManager.ProductManager.ManageProducts() ' Added the ManageProducts UserControl
                    Case "newproducts"
                        Return New Stocks.ItemManager.NewProduct.AddNewProducts() ' Added the NewProducts UserControl
                    Case Else
                        ' Return a placeholder UserControl with error text
                        Dim errorContent As New TextBlock With {
                            .Text = $"View not found: {viewName}",
                            .FontSize = 20,
                            .HorizontalAlignment = HorizontalAlignment.Center,
                            .VerticalAlignment = VerticalAlignment.Center
                        }
                        Return New UserControl With {.Content = errorContent}
                End Select
            Catch ex As Exception
                MessageBox.Show($"Error loading view '{viewName}': {ex.Message}")
                ' Return an error UserControl in case of exception
                Dim errorContent As New TextBlock With {
                    .Text = $"Error loading view: {viewName}",
                    .FontSize = 20,
                    .Foreground = New SolidColorBrush(Colors.Red),
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center
                }
                Return New UserControl With {.Content = errorContent}
            End Try
        End Function

        ''' <summary>
        ''' Helper function to get the name of a view
        ''' </summary>
        Public Shared Function GetViewName(view As Object) As String
            If view Is Nothing Then Return String.Empty

            Dim typeName As String = view.GetType().Name.ToLower()

            If typeName = "dashboard" Then
                Return "dashboard"
            ElseIf typeName = "stockstransfer" Then
                Return "stocks.stocktransfer"
            ElseIf typeName = "newsuppliers" Then
                Return "newsuppliers"
            ElseIf typeName = "managesuppliers" Then
                Return "managesuppliers"
            ElseIf typeName = "managebrands" Then
                Return "managebrands"
            ElseIf typeName = "warehouses" Then
                Return "warehouses"
            ElseIf typeName = "productcategories" Then
                Return "productcategories" ' Added the ProductCategories view type
            ElseIf typeName = "promocodes" Then
                Return "promocodes" ' Added the ManagePromoCodes view type
            ElseIf typeName = "manageproducts" Then
                Return "manageproducts" ' Added the ManageProducts view type
            ElseIf typeName = "newproducts" Then
                Return "newproducts" ' Added the ManageProducts view type
            Else
                Return typeName
            End If
        End Function
    End Class
End Namespace