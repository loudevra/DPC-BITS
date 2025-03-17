Imports System.Windows
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Stocks.Suppliers.ManageSuppliers
    Public Class ManageSuppliers
        Inherits Window

        Public Sub New()
            InitializeComponent()
            LoadData()
        End Sub

        ' Load Data Using SupplierController
        Public Sub LoadData()
            dataGrid.ItemsSource = SupplierController.GetSuppliers()
        End Sub
    End Class
End Namespace
