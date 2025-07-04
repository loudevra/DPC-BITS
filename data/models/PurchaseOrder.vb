Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.DPC.Data.Model
    ' Main purchase order data model
    Public Class PurchaseOrderDataModel
        Public Property OrderID As String
        Public Property OrderNumber As String
        Public Property SupplierID As String
        Public Property WarehouseID As String
        Public Property OrderDate As Date
        Public Property OrderDueDate As Date
        Public Property OrderStatus As String
        Public Property SubTotal As Decimal
        Public Property TaxTotal As Decimal
        Public Property DiscountTotal As Decimal
        Public Property GrandTotal As Decimal
        Public Property Notes As String

        ' Collection of line items for this purchase order
        Public Property LineItems As New ObservableCollection(Of PurchaseOrderLineItemModel)

        ' Reference to supplier data
        Public Property Supplier As SupplierDataModel

        Public Sub New()
            OrderDate = Date.Today
            OrderDueDate = Date.Today.AddDays(1)
            OrderStatus = "Draft"
            LineItems = New ObservableCollection(Of PurchaseOrderLineItemModel)
        End Sub

        ' Calculate order totals based on current line items
        Public Sub CalculateTotals()
            SubTotal = 0
            TaxTotal = 0
            DiscountTotal = 0

            For Each item In LineItems
                SubTotal += item.SubTotal
                TaxTotal += item.TaxAmount
                DiscountTotal += item.Discount
            Next

            GrandTotal = SubTotal + TaxTotal - DiscountTotal

            ' Ensure total is not negative
            If GrandTotal < 0 Then
                GrandTotal = 0
            End If
        End Sub

        ' Add a new line item to the order
        Public Function AddLineItem() As PurchaseOrderLineItemModel
            Dim newItem As New PurchaseOrderLineItemModel()
            LineItems.Add(newItem)
            Return newItem
        End Function

        ' Remove a line item from the order
        Public Sub RemoveLineItem(item As PurchaseOrderLineItemModel)
            If LineItems.Contains(item) Then
                LineItems.Remove(item)
                CalculateTotals()
            End If
        End Sub

        ' Check if a product is already in the order to prevent duplicates
        Public Function IsProductAlreadyAdded(productID As String, currentItemIndex As Integer) As Boolean
            For i As Integer = 0 To LineItems.Count - 1
                If i = currentItemIndex Then Continue For

                Dim item As PurchaseOrderLineItemModel = LineItems(i)
                If item.ProductID = productID Then
                    Return True
                End If
            Next

            Return False
        End Function
    End Class

    ' Line item data model for purchase order
    Public Class PurchaseOrderLineItemModel
        Public Property LineID As String
        Public Property OrderID As String
        Public Property ProductID As String
        Public Property ProductName As String
        Public Property Quantity As Integer = 1
        Public Property UnitPrice As Decimal
        Public Property TaxRate As Decimal
        Public Property TaxAmount As Decimal
        Public Property Discount As Decimal
        Public Property SubTotal As Decimal
        Public Property LineTotal As Decimal
        Public Property Notes As String

        Public Sub New()
            ' Default values
            Quantity = 1
            UnitPrice = 0
            TaxRate = 0
            TaxAmount = 0
            Discount = 0
            SubTotal = 0
            LineTotal = 0
        End Sub

        ' Calculate line totals based on quantity, price, tax and discount
        Public Sub CalculateLineTotals()
            SubTotal = Quantity * UnitPrice
            TaxAmount = SubTotal * (TaxRate / 100)
            LineTotal = SubTotal + TaxAmount - Discount

            ' Ensure line total is not negative
            If LineTotal < 0 Then
                LineTotal = 0
            End If
        End Sub

        ' Set product details from a ProductDataModel
        Public Sub SetProductDetails(product As ProductDataModel)
            ProductID = product.ProductID
            ProductName = product.ProductName
            UnitPrice = product.BuyingPrice
            TaxRate = product.DefaultTax
            CalculateLineTotals()
        End Sub
    End Class

    Public Class PurchaseOrderModel
        Public Property InvoiceNumber As String
        Public Property OrderData As String
        Public Property DueDate As String
        Public Property Tax As String
        Public Property Discount As String
        Public Property SupplierID As String
        Public Property SupplierName As String
        Public Property WarehouseID As String
        Public Property WarehouseName As String
        Public Property OrderItems As String
        Public Property OrderNote As String
        Public Property TotalTax As Decimal
        Public Property TotalDiscount As Decimal
        Public Property TotalPrice As Decimal

    End Class
End Namespace