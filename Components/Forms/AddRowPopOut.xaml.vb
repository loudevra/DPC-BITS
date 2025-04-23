Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Views.Stocks.ItemManager.NewProduct

Namespace DPC.Components.Forms
    Public Class AddRowPopout
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
            If ProductController.IsVariation = False Then
                If TxtRowNum IsNot Nothing Then
                    Dim rowCount As Integer
                    If Integer.TryParse(TxtRowNum.Text, rowCount) Then
                        For i As Integer = 1 To rowCount
                            ProductController.BtnAddRow_Click(Nothing, Nothing)
                        Next
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
                            ' Save current serial number values first
                            currentWindow.SaveSerialNumberValues()

                            ' Get the container that holds all serial number rows
                            Dim containerBorder As Border = TryCast(currentWindow.StackPanelSerialRow.Children(1), Border)
                            If containerBorder IsNot Nothing Then
                                Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
                                If scrollViewer IsNot Nothing Then
                                    Dim container = TryCast(scrollViewer.Content, StackPanel)

                                    If container IsNot Nothing Then
                                        ' Get current variation data to update later
                                        Dim currentData = ProductController.variationManager.GetCurrentVariationData()

                                        ' Add the requested number of rows
                                        For i As Integer = 1 To rowCount
                                            ' Use the current window's AddSerialNumberRow method
                                            currentWindow.AddSerialNumberRow(container, container.Children.Count + 1)
                                        Next

                                        ' Update the stock units textbox to match the new count
                                        currentWindow.TxtStockUnits.Text = container.Children.Count.ToString()

                                        ' Update the current variation data
                                        If currentData IsNot Nothing Then
                                            currentData.StockUnits = container.Children.Count

                                            ' Add empty entries to serial numbers list
                                            If currentData.SerialNumbers Is Nothing Then
                                                currentData.SerialNumbers = New List(Of String)()
                                            End If

                                            For i As Integer = 1 To rowCount
                                                currentData.SerialNumbers.Add("")
                                            Next
                                        End If

                                        ' Scroll to the bottom to show newly added rows
                                        scrollViewer.ScrollToEnd()

                                        ' Fix for 'Parent' not a member of 'DependencyObject'
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
