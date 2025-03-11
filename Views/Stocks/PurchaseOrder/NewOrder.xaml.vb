Namespace DPC.Views.Stocks.PurchaseOrder
    Public Class NewOrder
        Private rowCount As Integer = 1
        Private MyDynamicGrid As Grid

        Public Sub New()
            InitializeComponent()
            orderDate.SelectedDate = Date.Today
            orderDueDate.SelectedDate = Date.Today.AddDays(1)
            MyDynamicGrid = CType(TableGridPanel.Children(0), Grid)
        End Sub

        ' ➜ Add a New Row
        Private Sub AddNewRow()
            rowCount += 1
            Dim row1 As New RowDefinition() With {.Height = GridLength.Auto}
            Dim row2 As New RowDefinition() With {.Height = GridLength.Auto}

            MyDynamicGrid.RowDefinitions.Add(row1)
            MyDynamicGrid.RowDefinitions.Add(row2)

            Dim newRowStart As Integer = MyDynamicGrid.RowDefinitions.Count - 2

            ' Naming the row
            Dim rowName As String = $"Row_{rowCount}"

            ' Create UI Elements (Matches XAML Layout) and assign names
            CreateTextBox(newRowStart, 0, $"txtItem_{rowCount}")
            CreateTextBox(newRowStart, 1, $"txtQuantity_{rowCount}")
            CreateTextBox(newRowStart, 2, $"txtRate_{rowCount}")
            CreateTextBox(newRowStart, 3, $"txtTaxPercent_{rowCount}")
            CreateTextBlock(newRowStart, 4, "0", $"txtTax_{rowCount}") ' Tax
            CreateTextBox(newRowStart, 5, $"txtDiscount_{rowCount}")
            CreateTextBlock(newRowStart, 6, "0", $"txtAmount_{rowCount}") ' Amount
            CreateDeleteButton(newRowStart, 7, $"btnDelete_{rowCount}")

            ' Full-Width TextBox for Notes
            Dim fullWidthTextBox As New TextBox() With {
        .Width = Double.NaN, ' Auto width
        .Height = 60,
        .FontFamily = New FontFamily("Lexend"),
        .HorizontalContentAlignment = HorizontalAlignment.Left,
        .VerticalContentAlignment = VerticalAlignment.Top,
        .Name = $"txtNotes_{rowCount}" ' Assign unique name
    }
            Grid.SetRow(fullWidthTextBox, newRowStart + 1)
            Grid.SetColumn(fullWidthTextBox, 0)
            Grid.SetColumnSpan(fullWidthTextBox, 8)
            MyDynamicGrid.Children.Add(fullWidthTextBox)
        End Sub


        ' Updated CreateTextBox with Name
        Private Sub CreateTextBox(row As Integer, column As Integer, name As String)
            Dim txt As New TextBox() With {
        .Width = 150,
        .Height = 30,
        .Name = name ' Assign unique name
    }
            Grid.SetRow(txt, row)
            Grid.SetColumn(txt, column)
            MyDynamicGrid.Children.Add(txt)
        End Sub

        ' Updated CreateTextBlock with Name
        Private Sub CreateTextBlock(row As Integer, column As Integer, text As String, name As String)
            Dim txtBlock As New TextBlock() With {
        .Text = text,
        .VerticalAlignment = VerticalAlignment.Center,
        .Name = name ' Assign unique name
    }
            Grid.SetRow(txtBlock, row)
            Grid.SetColumn(txtBlock, column)
            MyDynamicGrid.Children.Add(txtBlock)
        End Sub

        ' Updated CreateDeleteButton with Name
        Private Sub CreateDeleteButton(row As Integer, column As Integer, name As String)
            Dim btn As New Button() With {
        .Width = 120,
        .Height = 30,
        .Name = name ' Assign unique name
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

            Dim text As New TextBlock() With {
        .Text = "Delete",
        .Foreground = Brushes.Black,
        .VerticalAlignment = VerticalAlignment.Center
    }

            stack.Children.Add(icon)
            stack.Children.Add(text)
            btn.Content = stack

            AddHandler btn.Click, Sub(sender As Object, e As RoutedEventArgs)
                                      RemoveRow(row)
                                  End Sub

            Grid.SetRow(btn, row)
            Grid.SetColumn(btn, column)
            MyDynamicGrid.Children.Add(btn)
        End Sub


        ' Remove Row Functionality
        Private Sub RemoveRow(row As Integer)
            Dim toRemove As New List(Of UIElement)

            ' Find elements belonging to the row
            For Each child As UIElement In MyDynamicGrid.Children
                If Grid.GetRow(child) = row Or Grid.GetRow(child) = row + 1 Then
                    toRemove.Add(child)
                End If
            Next

            ' Remove elements
            For Each element As UIElement In toRemove
                MyDynamicGrid.Children.Remove(element)
            Next
        End Sub


        Private Sub btnAddRow_Click(sender As Object, e As RoutedEventArgs) Handles btnAddRow.Click
            AddNewRow()
        End Sub

        'Typing Events for the data table


        'Calculations for the data table
        Private Function CalculateTax(quantity As Integer, price As Single, taxPercent As Single, isInclusive As Boolean) As Single
            Dim taxAmount As Single

            If isInclusive Then
                ' Extract tax from the total price
                taxAmount = (price * taxPercent) / (100 + taxPercent)
            Else
                ' Apply tax on top of base price
                taxAmount = price * (taxPercent / 100)
            End If

            Return taxAmount
        End Function

    End Class
End Namespace
