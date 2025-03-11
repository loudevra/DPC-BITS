Imports System.Windows
Imports System.Windows.Controls

Namespace DPC.Components.Forms
    Partial Public Class AddNewProductsForm
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub AddNewProduct(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Supplier Added Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub Cancel(sender As Object, e As RoutedEventArgs)
            ' Add your logic to handle canceling the operation here
        End Sub
    End Class
End Namespace
