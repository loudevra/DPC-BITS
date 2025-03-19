Imports System.Windows.Controls
Imports System.Windows
Imports System.Windows.Input

Namespace DPC.Data.Converters
    Public Class PasswordMaskingBehavior
        Public Shared Function GetEnablePasswordMasking(element As DependencyObject) As Boolean
            Return element.GetValue(EnablePasswordMaskingProperty)
        End Function

        Public Shared Sub SetEnablePasswordMasking(element As DependencyObject, value As Boolean)
            element.SetValue(EnablePasswordMaskingProperty, value)
        End Sub

        Public Shared ReadOnly EnablePasswordMaskingProperty As DependencyProperty =
            DependencyProperty.RegisterAttached(
                "EnablePasswordMasking",
                GetType(Boolean),
                GetType(PasswordMaskingBehavior),
                New PropertyMetadata(False, AddressOf OnEnablePasswordMaskingChanged)
            )

        Private Shared Sub OnEnablePasswordMaskingChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim textBox As TextBox = TryCast(d, TextBox)

            If textBox IsNot Nothing AndAlso CBool(e.NewValue) Then
                AddHandler textBox.PreviewTextInput, AddressOf OnTextInput
                DataObject.AddPastingHandler(textBox, AddressOf OnPaste)
            Else
                RemoveHandler textBox.PreviewTextInput, AddressOf OnTextInput
                DataObject.RemovePastingHandler(textBox, AddressOf OnPaste)
            End If
        End Sub

        Private Shared Sub OnTextInput(sender As Object, e As TextCompositionEventArgs)
            Dim textBox As TextBox = CType(sender, TextBox)
            textBox.Text += e.Text
            textBox.CaretIndex = textBox.Text.Length
            e.Handled = True
        End Sub

        Private Shared Sub OnPaste(sender As Object, e As DataObjectPastingEventArgs)
            e.CancelCommand()
        End Sub
    End Class
End Namespace
