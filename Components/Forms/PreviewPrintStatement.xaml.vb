Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Components.Forms
    Public Class PreviewPrintStatement

        Private itemDataSource As New ObservableCollection(Of OrderItems)

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            If StatementDetails.signature = False Then
                SignatureGrid.Child = Nothing
            End If

            InvoiceNumber.Text = StatementDetails.InvoiceNumberCache
            InvoiceDate.Text = StatementDetails.InvoiceDateCache
            DueDate.Text = StatementDetails.DueDateCache
            Tax.Text = StatementDetails.TaxCache
            TotalCost.Text = StatementDetails.TotalCostCache

            For Each item In StatementDetails.OrderItemsCache

                itemDataSource.Add(New OrderItems With {
                    .Quantity = item("Quantity"),
                    .Description = item("ItemName"),
                    .UnitPrice = item("Rate"),
                    .LinePrice = item("Price")
                })
            Next

            dataGrid.ItemsSource = itemDataSource

        End Sub

    End Class

End Namespace


