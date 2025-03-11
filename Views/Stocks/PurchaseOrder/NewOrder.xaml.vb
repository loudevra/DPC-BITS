Namespace DPC.Views.Stocks.PurchaseOrder
    Public Class NewOrder
        Private rowCount As Integer = 1 ' Tracks the number of data row sets
        Private MyDynamicGrid As Grid ' Reference to your Grid

        Public Sub New()
            InitializeComponent()

            orderDate.SelectedDate = Date.Today
            orderDueDate.SelectedDate = Date.Today.AddDays(1)
            ' Find the Grid inside StackPanel (Replace "MyStackPanel" with the actual name)
            MyDynamicGrid = CType(TableGridPanel.Children(0), Grid)
        End Sub

        ' Method to Add a New Set of Rows Dynamically
        Private Sub AddNewRow()
            ' Increment row count for tracking
            rowCount += 1

            ' Create Row 1 & Row 2 dynamically
            Dim row1 As New RowDefinition() With {.Height = GridLength.Auto}
            Dim row2 As New RowDefinition() With {.Height = GridLength.Auto}

            ' Add Rows to Grid
            MyDynamicGrid.RowDefinitions.Add(row1)
            MyDynamicGrid.RowDefinitions.Add(row2)

            ' Determine the new starting row index
            Dim newRowStart As Integer = MyDynamicGrid.RowDefinitions.Count - 2

            ' Create Controls for Row 1
            CreateTextBox(newRowStart, 0, 150) ' Item Name
            CreateTextBox(newRowStart, 1, 150) ' Quantity
            CreateTextBox(newRowStart, 2, 150) ' Rate
            CreateTextBox(newRowStart, 3, 150) ' Tax(%)
            CreateTextBlock(newRowStart, 4, "0") ' Tax
            CreateTextBox(newRowStart, 5, 150) ' Discount
            CreateTextBlock(newRowStart, 6, "0") ' Amount
            CreateDeleteButton(newRowStart, 7) ' Delete Button

            ' Create TextBox for Row 2 (Spanning 8 Columns)
            Dim fullWidthTextBox As New TextBox() With {
                .Height = 30
            }
            Grid.SetRow(fullWidthTextBox, newRowStart + 1)
            Grid.SetColumn(fullWidthTextBox, 0)
            Grid.SetColumnSpan(fullWidthTextBox, 8)
            MyDynamicGrid.Children.Add(fullWidthTextBox)
        End Sub

        ' Helper Method: Create a TextBox
        Private Sub CreateTextBox(row As Integer, column As Integer, width As Double)
            Dim txt As New TextBox() With {
                .Width = width,
                .Height = 30
            }
            Grid.SetRow(txt, row)
            Grid.SetColumn(txt, column)
            MyDynamicGrid.Children.Add(txt)
        End Sub

        ' Helper Method: Create a TextBlock
        Private Sub CreateTextBlock(row As Integer, column As Integer, text As String)
            Dim txtBlock As New TextBlock() With {
                .Text = text,
                .VerticalAlignment = VerticalAlignment.Center
            }
            Grid.SetRow(txtBlock, row)
            Grid.SetColumn(txtBlock, column)
            MyDynamicGrid.Children.Add(txtBlock)
        End Sub

        ' Helper Method: Create a Delete Button
        Private Sub CreateDeleteButton(row As Integer, column As Integer)
            ' Create the delete button
            Dim btn As New Button() With {
        .Width = 120,
        .Height = 30
    }

            ' Create the StackPanel to hold icon + text
            Dim stack As New StackPanel() With {
        .Orientation = Orientation.Horizontal,
        .HorizontalAlignment = HorizontalAlignment.Center
    }

            ' Create the MaterialDesign PackIcon
            Dim icon As New MaterialDesignThemes.Wpf.PackIcon() With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistRemove,
        .Foreground = Brushes.Black,
        .Width = 24,
        .Height = 24,
        .Margin = New Thickness(0, 0, 5, 0) ' Space between icon and text
    }

            ' Create the TextBlock for button text
            Dim text As New TextBlock() With {
        .Text = "Delete",
        .Foreground = Brushes.Black,
        .VerticalAlignment = VerticalAlignment.Center
    }

            ' Add icon and text to StackPanel
            stack.Children.Add(icon)
            stack.Children.Add(text)

            ' Set StackPanel as button content
            btn.Content = stack

            ' Attach event to delete row
            AddHandler btn.Click, Sub(sender As Object, e As RoutedEventArgs)
                                      RemoveRow(row)
                                  End Sub

            ' Position button in Grid
            Grid.SetRow(btn, row)
            Grid.SetColumn(btn, column)

            ' Add button to Grid
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
    End Class

End Namespace