Imports System.Windows
Imports System.Windows.Controls

Namespace DPC.Components.Forms
    Partial Public Class AddNewProductsNextForm
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub AddNewProduct(sender As Object, e As RoutedEventArgs)

            MessageBox.Show("Add New Product button clicked")
        End Sub

        Private Sub Cancel(sender As Object, e As RoutedEventArgs)

            MessageBox.Show("Cancel button clicked")
        End Sub
    End Class
End Namespace
