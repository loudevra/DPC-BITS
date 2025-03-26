Namespace DPC.Components.Forms
    Public Class AddVariation

        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BtnEditVariationName(sender As Object, e As RoutedEventArgs)
            EditVariationFunction(TxtVariationName, True)
        End Sub

        Private Sub BtnClearVariationName(sender As Object, e As RoutedEventArgs)
            TxtVariationName.Focus()
            TxtVariationName.Clear()

            ' Do not toggle the read-only state when clearing
            EditVariationFunction(TxtVariationName, False)
        End Sub

        Private Sub EditVariationFunction(VariationName As TextBox, shouldToggle As Boolean)
            If shouldToggle Then
                ' Toggle the IsReadOnly property
                VariationName.IsReadOnly = Not VariationName.IsReadOnly
            End If

            ' Change the button content to reflect the state
            If VariationName.IsReadOnly Then
                BtnIconVariationName.Kind = MaterialDesignThemes.Wpf.PackIconKind.PencilOffOutline
            Else
                BtnIconVariationName.Kind = MaterialDesignThemes.Wpf.PackIconKind.RenameOutline
                VariationName.Focus()
                VariationName.SelectAll()
            End If
        End Sub


    End Class
End Namespace
