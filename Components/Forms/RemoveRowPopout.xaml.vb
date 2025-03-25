Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Views.Stocks.ItemManager.NewProduct

Namespace DPC.Components.Forms
    Public Class RemoveRowPopout
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            If Not IsNumeric(e.Text) OrElse Not Integer.TryParse(e.Text, New Integer()) Then
                e.Handled = True ' Block non-integer input
            End If
        End Sub

        Private Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            If e.DataObject.GetDataPresent(GetType(String)) Then
                Dim pastedText As String = CStr(e.DataObject.GetData(GetType(String)))
                If Not Integer.TryParse(pastedText, New Integer()) Then
                    e.CancelCommand() ' Cancel if pasted data is not an integer
                End If
            Else
                e.CancelCommand()
            End If
        End Sub

        Private Sub Add_Click(sender As Object, e As RoutedEventArgs)
            If TxtRowNum IsNot Nothing Then
                Dim currentValue As Integer
                If Integer.TryParse(TxtRowNum.Text, currentValue) Then
                    currentValue += 1
                    TxtRowNum.Text = currentValue.ToString()
                Else
                    TxtRowNum.Text = "1"
                End If
            Else
                MessageBox.Show("TxtRowNum is not found or not initialized.")
            End If
        End Sub

        Private Sub Subtract_Click(sender As Object, e As RoutedEventArgs)
            If TxtRowNum IsNot Nothing Then
                Dim currentValue As Integer
                If Integer.TryParse(TxtRowNum.Text, currentValue) Then
                    currentValue = Math.Max(currentValue - 1, 0)
                    TxtRowNum.Text = currentValue.ToString()
                Else
                    TxtRowNum.Text = "0"
                End If
            Else
                MessageBox.Show("TxtRowNum is not found or not initialized.")
            End If
        End Sub

        Private Sub GenerateRows(sender As Object, e As RoutedEventArgs)
            If TxtRowNum IsNot Nothing Then
                Dim rowCount As Integer
                If Integer.TryParse(TxtRowNum.Text, rowCount) Then
                    Dim productController As New ProductController()
                    For i As Integer = 1 To rowCount
                        ProductController.RemoveLatestRow()
                    Next

                    ' Update TxtStockunits
                    If ProductController.MainContainer IsNot Nothing Then
                        ProductController.TxtStockUnits.Text = ProductController.MainContainer.Children.Count.ToString()
                    End If
                Else
                    MessageBox.Show("Invalid number in TxtRowNum.")
                End If
            Else
                MessageBox.Show("TxtRowNum is not found or not initialized.")
            End If
        End Sub


    End Class
End Namespace
