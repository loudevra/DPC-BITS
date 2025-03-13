Namespace DPC.Views.ItemManager.AddNewProducts
    Public Class AddNewProducts
        Inherits Window

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub btnAddRow_Click(sender As Object, e As RoutedEventArgs)
            ' Open the second form
            Dim secondForm As New AddNewProductSecondForm()
            secondForm.Show()
        End Sub
    End Class
End Namespace
