Namespace DPC.Views.Stocks.PurchaseOrder
    Public Class NewOrder
        Private rowCount As Integer = 0
        Private MyDynamicGrid As Grid

        Public Sub New()
            InitializeComponent()

            orderDate.SelectedDate = Date.Today
            orderDueDate.SelectedDate = Date.Today.AddDays(1)
            MyDynamicGrid = CType(TableGridPanel.Children(0), Grid)
            AddNewRow()
        End Sub

        ' ➜ Add a New Row
        Private Sub AddNewRow()
            rowCount += 1
            Dim row1 As New RowDefinition() With {.Height = GridLength.Auto}
            Dim row2 As New RowDefinition() With {.Height = GridLength.Auto}

            MyDynamicGrid.RowDefinitions.Add(row1)
            MyDynamicGrid.RowDefinitions.Add(row2)

            Dim newRowStart As Integer = MyDynamicGrid.RowDefinitions.Count - 2

            ' Create UI Elements (Matches XAML Layout)
            CreateTextBox(newRowStart, 0) ' Item Name
            CreateTextBox(newRowStart, 1) ' Quantity
            CreateTextBox(newRowStart, 2) ' Rate
            CreateTextBox(newRowStart, 3) ' Tax %
            CreateTextBox(newRowStart, 4) ' Tax (Editable)
            CreateTextBox(newRowStart, 5) ' Discount
            CreateTextBox(newRowStart, 6) ' Amount (Editable)
            CreateDeleteButton(newRowStart, 7)

            ' Full-Width TextBox for Notes
            Dim fullWidthTextBox As New TextBox() With {
        .Width = Double.NaN, ' Auto width
        .Height = 60,
        .FontFamily = New FontFamily("Lexend"),
        .HorizontalContentAlignment = HorizontalAlignment.Left,
        .VerticalContentAlignment = VerticalAlignment.Top
    }
            Grid.SetRow(fullWidthTextBox, newRowStart + 1)
            Grid.SetColumn(fullWidthTextBox, 0)
            Grid.SetColumnSpan(fullWidthTextBox, 8)
            MyDynamicGrid.Children.Add(fullWidthTextBox)
        End Sub

        ' ➜ Create TextBox
        Private Sub CreateTextBox(row As Integer, column As Integer)
            Dim txtName As String = $"txt_{row}_{column}"

            ' Check if the name already exists and unregister it
            Dim existingElement As Object = Me.FindName(txtName)
            If existingElement IsNot Nothing Then
                Me.UnregisterName(txtName)
            End If

            ' Create TextBox
            Dim txt As New TextBox() With {
        .Name = txtName,
        .Width = Double.NaN,
        .Height = 30,
        .FontFamily = New FontFamily("Lexend"),
        .HorizontalContentAlignment = If(column = 0, HorizontalAlignment.Left, HorizontalAlignment.Center),
        .VerticalContentAlignment = VerticalAlignment.Center
    }

            ' Attach numeric validation and event handlers
            If column = 1 Or column = 2 Or column = 3 Or column = 4 Or column = 5 Or column = 6 Then
                AddHandler txt.PreviewTextInput, AddressOf ValidateNumericInput
                AddHandler txt.TextChanged, Sub(sender As Object, e As TextChangedEventArgs)
                                                Dim parentRow As Integer = Grid.GetRow(CType(sender, TextBox))
                                                UpdateTaxAndAmount(parentRow)
                                            End Sub
            End If

            ' Set Grid position
            Grid.SetRow(txt, row)
            Grid.SetColumn(txt, column)

            ' Register the TextBox name
            Me.RegisterName(txtName, txt)

            ' Add to Grid
            MyDynamicGrid.Children.Add(txt)
        End Sub

        'Check if entered text is a number
        Private Sub ValidateNumericInput(sender As Object, e As TextCompositionEventArgs)
            Dim allowedPattern As String = "^[0-9.]+$" ' Allow only digits and decimal point
            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, allowedPattern) Then
                e.Handled = True ' Reject input if it doesn't match
            End If
        End Sub

        ' ➜ Create TextBlock
        Private Sub CreateTextBlock(row As Integer, column As Integer, text As String)
            Dim txtBlock As New TextBlock() With {
                .Text = text,
                .FontFamily = New FontFamily("Lexend"),
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center
            }
            Grid.SetRow(txtBlock, row)
            Grid.SetColumn(txtBlock, column)
            MyDynamicGrid.Children.Add(txtBlock)
        End Sub

        ' ➜ Create Delete Button
        Private Sub CreateDeleteButton(row As Integer, column As Integer)
            Dim btn As New Button() With {
        .Width = Double.NaN,
        .Height = 30,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .VerticalAlignment = VerticalAlignment.Center
    }

            Dim stack As New StackPanel() With {
        .Orientation = Orientation.Horizontal,
        .HorizontalAlignment = HorizontalAlignment.Center
    }

            Dim icon As New MaterialDesignThemes.Wpf.PackIcon() With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistRemove,
        .Foreground = Brushes.Black,
        .Width = 24,
        .Height = 24,
        .Margin = New Thickness(0, 0, 5, 0)
    }

            Dim deleteText As New TextBlock() With {
        .Text = "Delete",
        .FontFamily = New FontFamily("Lexend"),
        .Foreground = Brushes.Black,
        .VerticalAlignment = VerticalAlignment.Center
    }

            stack.Children.Add(icon)
            stack.Children.Add(deleteText)
            btn.Content = stack

            ' Attach delete functionality
            AddHandler btn.Click, Sub(sender As Object, e As RoutedEventArgs)
                                      Dim parentRow As Integer = Grid.GetRow(CType(sender, Button))
                                      RemoveRow(parentRow)
                                  End Sub

            Grid.SetRow(btn, row)
            Grid.SetColumn(btn, column)
            MyDynamicGrid.Children.Add(btn)
        End Sub

        ' Remove Row Functionality
        Private Sub RemoveRow(row As Integer)
            ' Find all elements in the specified row and the corresponding note row
            Dim elementsToRemove As New List(Of UIElement)

            For Each element As UIElement In MyDynamicGrid.Children
                If Grid.GetRow(element) = row Or Grid.GetRow(element) = row + 1 Then
                    elementsToRemove.Add(element)
                End If
            Next

            ' Unregister names and remove elements from the grid
            For Each element As UIElement In elementsToRemove
                If TypeOf element Is TextBox Then
                    Dim txtBox As TextBox = CType(element, TextBox)
                    If Not String.IsNullOrEmpty(txtBox.Name) AndAlso Me.FindName(txtBox.Name) IsNot Nothing Then
                        Try
                            Me.UnregisterName(txtBox.Name)
                        Catch ex As ArgumentException
                            ' Ignore error if the name is already unregistered
                        End Try
                    End If
                End If
                MyDynamicGrid.Children.Remove(element)
            Next

            ' Remove the corresponding RowDefinitions (ensure valid index before removing)
            If row < MyDynamicGrid.RowDefinitions.Count Then
                MyDynamicGrid.RowDefinitions.RemoveAt(row)
                If row < MyDynamicGrid.RowDefinitions.Count Then
                    MyDynamicGrid.RowDefinitions.RemoveAt(row) ' Remove note row as well
                End If
            End If

            ' Adjust remaining rows (shift everything above the deleted row)
            Dim elementsToShift As New List(Of UIElement)
            For Each element As UIElement In MyDynamicGrid.Children
                Dim currentRow As Integer = Grid.GetRow(element)
                If currentRow > row Then
                    elementsToShift.Add(element)
                End If
            Next

            For Each element As UIElement In elementsToShift
                Grid.SetRow(element, Grid.GetRow(element) - 2)
            Next

            ' Reduce row count
            rowCount -= 1
        End Sub


        Private Sub btnAddRow_Click(sender As Object, e As RoutedEventArgs) Handles btnAddRow.Click
            AddNewRow()
        End Sub

        'Calculations for the data table

        Private Sub UpdateTaxAndAmount(row As Integer)
            ' Get references to the required textboxes
            Dim quantityTxt As TextBox = TryCast(Me.FindName($"txt_{row}_1"), TextBox)
            Dim rateTxt As TextBox = TryCast(Me.FindName($"txt_{row}_2"), TextBox)
            Dim taxPercentTxt As TextBox = TryCast(Me.FindName($"txt_{row}_3"), TextBox)
            Dim taxTxt As TextBox = TryCast(Me.FindName($"txt_{row}_4"), TextBox) ' Now a TextBox
            Dim discountTxt As TextBox = TryCast(Me.FindName($"txt_{row}_5"), TextBox)
            Dim amountTxt As TextBox = TryCast(Me.FindName($"txt_{row}_6"), TextBox) ' Now a TextBox

            ' Ensure all fields have valid numeric input
            Dim quantity As Integer = If(quantityTxt IsNot Nothing AndAlso Integer.TryParse(quantityTxt.Text, quantity), quantity, 0)
            Dim rate As Single = If(rateTxt IsNot Nothing AndAlso Single.TryParse(rateTxt.Text, rate), rate, 0)
            Dim taxPercent As Single = If(taxPercentTxt IsNot Nothing AndAlso Single.TryParse(taxPercentTxt.Text, taxPercent), taxPercent, 0)
            Dim discount As Single = If(discountTxt IsNot Nothing AndAlso Single.TryParse(discountTxt.Text, discount), discount, 0)

            ' Calculate subtotal (Quantity × Rate)
            Dim subtotal As Single = quantity * rate

            ' Compute tax (Tax % of subtotal)
            Dim taxAmount As Single = subtotal * (taxPercent / 100)

            ' Update the Tax field dynamically
            If taxTxt IsNot Nothing Then
                taxTxt.Text = taxAmount.ToString("0.00")
            End If

            ' Calculate total amount (Subtotal + Tax - Discount)
            Dim totalAmount As Single = subtotal + taxAmount - discount

            ' Ensure the total amount is not negative
            If totalAmount < 0 Then totalAmount = 0

            ' Update the Amount field dynamically
            If amountTxt IsNot Nothing Then
                amountTxt.Text = totalAmount.ToString("0.00")
            End If
        End Sub

        Private Sub TextBoxValueChanged(sender As Object, e As TextChangedEventArgs)
            Dim txt As TextBox = CType(sender, TextBox)

            ' Extract row number from textbox name (e.g., "txt_2_1" → row 2)
            Dim parts As String() = txt.Name.Split("_"c)
            If parts.Length < 3 Then Exit Sub

            Dim row As Integer
            If Integer.TryParse(parts(1), row) Then
                UpdateTaxAndAmount(row) ' Call function to update tax and amount
            End If
        End Sub
    End Class
End Namespace
