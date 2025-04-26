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
            If ProductController.IsVariation = False Then
                If TxtRowNum IsNot Nothing AndAlso ProductController.MainContainer IsNot Nothing Then
                    Dim currentValue As Integer
                    Dim latestRowCount As Integer = ProductController.MainContainer.Children.Count
                    ' Ensure TxtRowNum does not exceed latest row count - 1
                    If Integer.TryParse(TxtRowNum.Text, currentValue) Then
                        If currentValue < latestRowCount - 1 Then
                            currentValue += 1
                            TxtRowNum.Text = currentValue.ToString()
                        End If
                    Else
                        TxtRowNum.Text = "1"
                    End If
                Else
                    MessageBox.Show("TxtRowNum or MainContainer is not initialized.")
                End If
            ElseIf ProductController.IsVariation = True Then
                ' Get the current ProductVariationDetails instance
                Dim currentWindow = Application.Current.Windows.OfType(Of ProductVariationDetails)().FirstOrDefault()

                If currentWindow IsNot Nothing Then
                    ' Get the container that holds all serial number rows
                    Dim containerBorder As Border = TryCast(currentWindow.StackPanelSerialRow.Children(1), Border)
                    If containerBorder IsNot Nothing Then
                        Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
                        If scrollViewer IsNot Nothing Then
                            Dim container = TryCast(scrollViewer.Content, StackPanel)

                            If container IsNot Nothing Then
                                Dim currentValue As Integer
                                Dim latestRowCount As Integer = container.Children.Count

                                ' Ensure TxtRowNum does not exceed latest row count - 1
                                If Integer.TryParse(TxtRowNum.Text, currentValue) Then
                                    If currentValue < latestRowCount - 1 Then
                                        currentValue += 1
                                        TxtRowNum.Text = currentValue.ToString()
                                    End If
                                Else
                                    TxtRowNum.Text = "1"
                                End If
                            End If
                        End If
                    End If
                Else
                    MessageBox.Show("ProductVariationDetails window not found.")
                End If
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
            If ProductController.IsVariation = False Then
                If TxtRowNum IsNot Nothing Then
                    Dim rowCount As Integer
                    If Integer.TryParse(TxtRowNum.Text, rowCount) Then
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
            ElseIf ProductController.IsVariation = True Then
                If TxtRowNum IsNot Nothing Then
                    Dim rowCount As Integer
                    If Integer.TryParse(TxtRowNum.Text, rowCount) Then
                        ' Find the current ProductVariationDetails instance
                        Dim currentWindow = Application.Current.Windows.OfType(Of ProductVariationDetails)().FirstOrDefault()

                        If currentWindow IsNot Nothing Then
                            ' Get the container that holds all serial number rows
                            Dim containerBorder As Border = TryCast(currentWindow.StackPanelSerialRow.Children(1), Border)
                            If containerBorder IsNot Nothing Then
                                Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
                                If scrollViewer IsNot Nothing Then
                                    Dim container = TryCast(scrollViewer.Content, StackPanel)

                                    If container IsNot Nothing Then
                                        ' Get current variation data to update later
                                        Dim currentData = ProductController.variationManager.GetCurrentVariationData()

                                        ' Don't allow removing all rows - keep at least one
                                        rowCount = Math.Min(rowCount, container.Children.Count - 1)

                                        ' Save the current serial number values
                                        currentWindow.SaveSerialNumberValues()

                                        ' Remove the requested number of rows from bottom up
                                        For i As Integer = 1 To rowCount
                                            If container.Children.Count > 1 Then
                                                ' Remove from the bottom (last child)
                                                Dim lastRowIndex = container.Children.Count - 1

                                                ' Remove the corresponding serial number from data model
                                                If currentData IsNot Nothing AndAlso
                                                   currentData.SerialNumbers IsNot Nothing AndAlso
                                                   lastRowIndex >= 0 AndAlso
                                                   lastRowIndex < currentData.SerialNumbers.Count Then
                                                    currentData.SerialNumbers.RemoveAt(lastRowIndex)
                                                End If

                                                ' Remove the row from UI
                                                container.Children.RemoveAt(lastRowIndex)
                                            End If
                                        Next

                                        ' Update the stock units textbox to match the new count
                                        currentWindow.TxtStockUnits.Text = container.Children.Count.ToString()

                                        ' Update the current variation data
                                        If currentData IsNot Nothing Then
                                            currentData.StockUnits = container.Children.Count
                                        End If

                                        ' Close the popup properly
                                        Dim popup = TryCast(Me.Parent, Popup)
                                        If popup IsNot Nothing Then
                                            popup.IsOpen = False
                                        Else
                                            ' Try to find parent popup differently
                                            Dim parent = VisualTreeHelper.GetParent(Me)
                                            While parent IsNot Nothing AndAlso Not TypeOf parent Is Popup
                                                parent = VisualTreeHelper.GetParent(parent)
                                            End While

                                            If TypeOf parent Is Popup Then
                                                DirectCast(parent, Popup).IsOpen = False
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Else
                            MessageBox.Show("ProductVariationDetails window not found.")
                        End If
                    Else
                        MessageBox.Show("Invalid number in TxtRowNum.")
                    End If
                Else
                    MessageBox.Show("TxtRowNum is not found or not initialized.")
                End If
            End If
        End Sub
    End Class
End Namespace