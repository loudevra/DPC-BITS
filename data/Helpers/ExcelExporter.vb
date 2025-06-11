Imports System.Data
Imports System.IO
Imports System.Reflection
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports ClosedXML.Excel
Imports DPC.DPC.Data.Models
Imports MahApps.Metro.Controls
Imports Microsoft.Win32
Imports SkiaSharp

Namespace DPC.Data.Helpers
    Public Class ExcelExporter
        ''' <summary>
        ''' Exports DataGrid content to Excel file using a SaveFileDialog
        ''' </summary>
        Public Shared Function ExportDataGridToExcel(dataGrid As DataGrid, Optional defaultFileName As String = "Export", Optional worksheetName As String = "Data") As Boolean
            ' Check if DataGrid has data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return False
            End If

            ' Open SaveFileDialog
            Dim saveFileDialog As New SaveFileDialog() With {
                .Filter = "Excel Files (*.xlsx)|*.xlsx",
                .FileName = $"{defaultFileName}.xlsx",
                .OverwritePrompt = True ' Prompt user before overwriting
            }

            If saveFileDialog.ShowDialog() = True Then
                Return ExportDataGridToExcelFile(dataGrid, saveFileDialog.FileName, worksheetName)
            End If

            Return False
        End Function

        ''' <summary>
        ''' Exports DataGrid content to Excel file using a SaveFileDialog, with option to exclude specific columns
        ''' </summary>
        Public Shared Function ExportDataGridToExcel(dataGrid As DataGrid,
                                                   columnsToExclude As List(Of String),
                                                   Optional defaultFileName As String = "Export",
                                                   Optional worksheetName As String = "Data",
                                                    Optional model As Object = Nothing,
                                                         Optional modelProperty As String = Nothing) As Boolean
            ' Check if DataGrid has data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return False
            End If

            ' Open SaveFileDialog
            Dim saveFileDialog As New SaveFileDialog() With {
                .Filter = "Excel Files (*.xlsx)|*.xlsx",
                .FileName = $"{defaultFileName}.xlsx",
                .OverwritePrompt = True ' Prompt user before overwriting
            }

            If saveFileDialog.ShowDialog() = True Then
                Return ExportDataGridToExcelFile(dataGrid, saveFileDialog.FileName, worksheetName, columnsToExclude, model, modelProperty)
            End If

            Return False
        End Function

        ''' <summary>
        ''' Exports DataGrid content directly to a specified file path
        ''' </summary>
        Public Shared Function ExportDataGridToExcelFile(dataGrid As DataGrid,
                                                       filePath As String,
                                                       Optional worksheetName As String = "Data",
                                                       Optional columnsToExclude As List(Of String) = Nothing,
                                                       Optional model As Object = Nothing,
                                                         Optional modelProperty As String = Nothing) As Boolean
            Try
                ' Ensure no existing file is locked
                Try
                    If File.Exists(filePath) Then
                        File.Delete(filePath)
                    End If
                Catch ex As Exception
                    ' If we can't delete, use a different filename
                    Dim dir = Path.GetDirectoryName(filePath)
                    Dim fileName = Path.GetFileNameWithoutExtension(filePath)
                    Dim ext = Path.GetExtension(filePath)
                    filePath = Path.Combine(dir, $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{ext}")
                End Try

                ' Create a DataTable instead of directly using the workbook
                Dim dt As New DataTable()

                ' Add DataGrid columns as table headers (excluding specified columns)
                Dim includedColumns As New List(Of Integer)
                For i As Integer = 0 To dataGrid.Columns.Count - 1
                    Dim column As DataGridColumn = dataGrid.Columns(i)
                    Dim headerText As String = column.Header.ToString()

                    ' Only include columns that aren't in the exclusion list and are visible
                    If (columnsToExclude Is Nothing OrElse Not columnsToExclude.Contains(headerText)) AndAlso
                       column.Visibility = Visibility.Visible Then
                        dt.Columns.Add(headerText)
                        includedColumns.Add(i)
                    End If
                Next

                ' Add rows from DataGrid items
                For Each item In dataGrid.Items
                    Dim row As DataRow = dt.NewRow()
                    Dim dtColumnIndex As Integer = 0

                    For Each columnIndex In includedColumns
                        Dim column As DataGridColumn = dataGrid.Columns(columnIndex)

                        ' Handle bound columns
                        Dim boundColumn = TryCast(column, DataGridBoundColumn)

                        If boundColumn IsNot Nothing AndAlso boundColumn.Binding IsNot Nothing Then
                            Dim binding As Binding = TryCast(boundColumn.Binding, Binding)
                            If binding IsNot Nothing AndAlso binding.Path IsNot Nothing Then
                                Dim bindingPath As String = binding.Path.Path
                                Dim prop As PropertyInfo = item.GetType().GetProperty(bindingPath)

                                If prop IsNot Nothing Then
                                    row(dtColumnIndex) = prop.GetValue(item, Nothing)?.ToString()

                                End If
                            End If
                        ElseIf TypeOf column Is DataGridTemplateColumn Then
                            ' Skip template columns (like buttons)

                            Try
                                dataGrid.UpdateLayout()
                                dataGrid.ScrollIntoView(item)

                                Dim rowIndex As Integer = dataGrid.Items.IndexOf(item)
                                Dim dataGridRow = TryCast(dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex), DataGridRow)
                                If dataGridRow Is Nothing Then Throw New Exception("DataGridRow not found at row " & rowIndex)

                                dataGridRow.ApplyTemplate()

                                ' Step 1: Get the presenter
                                Dim presenter As DataGridCellsPresenter = Nothing
                                Dim queue As New Queue(Of DependencyObject)()
                                queue.Enqueue(dataGridRow)

                                While queue.Count > 0
                                    Dim current = queue.Dequeue()
                                    For i = 0 To VisualTreeHelper.GetChildrenCount(current) - 1
                                        Dim child = VisualTreeHelper.GetChild(current, i)
                                        If TypeOf child Is DataGridCellsPresenter Then
                                            presenter = CType(child, DataGridCellsPresenter)
                                            Exit While
                                        End If
                                        queue.Enqueue(child)
                                    Next
                                End While

                                If presenter Is Nothing Then Throw New Exception("Presenter not found at row " & rowIndex)

                                ' Step 2: Get the specific cell
                                Dim cell = TryCast(presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex), DataGridCell)
                                If cell Is Nothing Then Throw New Exception("Cell not found at row " & rowIndex)

                                cell.ApplyTemplate()

                                ' Step 3: Search for StackPanel inside the cell
                                Dim contentElement As DependencyObject = Nothing
                                If VisualTreeHelper.GetChildrenCount(cell) > 0 Then
                                    contentElement = VisualTreeHelper.GetChild(cell, 0)
                                End If
                                If contentElement Is Nothing Then Throw New Exception("Cell content visual is missing at row " & rowIndex)

                                Dim stackPanel As StackPanel = Nothing
                                Dim list As ListBox = Nothing
                                Dim visualQueue As New Queue(Of DependencyObject)()
                                visualQueue.Enqueue(contentElement)

                                While visualQueue.Count > 0
                                    Dim visual = visualQueue.Dequeue()
                                    For i = 0 To VisualTreeHelper.GetChildrenCount(visual) - 1
                                        Dim child = VisualTreeHelper.GetChild(visual, i)
                                        If TypeOf child Is StackPanel Then
                                            stackPanel = CType(child, StackPanel)
                                            Exit While
                                        End If
                                        If TypeOf child Is ListBox Then
                                            list = CType(child, ListBox)
                                            Exit While
                                        End If
                                        visualQueue.Enqueue(child)
                                    Next
                                End While

                                ' If column is stack panel
                                If stackPanel IsNot Nothing Then
                                    If stackPanel.Children.Count > 1 Then
                                        Dim textBlock = TryCast(stackPanel.Children(1), TextBlock)
                                        If textBlock IsNot Nothing Then
                                            row(dtColumnIndex) = textBlock.Text
                                        Else
                                            Throw New Exception("TextBlock not found in StackPanel at row " & rowIndex)
                                        End If
                                    Else
                                        Throw New Exception("Not enough children in StackPanel at row " & rowIndex)
                                    End If

                                End If

                                ' If column is listbox
                                If list IsNot Nothing Then
                                    Dim result As String = ""

                                    For Each listItem In list.Items
                                        model = listItem
                                        Dim modelType = model.GetType()
                                        Dim obj = modelType.GetProperty(modelProperty)


                                        result &= obj.GetValue(model, Nothing).ToString() & ", "
                                    Next

                                    ' Remove the trailing comma and space
                                    If result.EndsWith(", ") Then
                                        result = result.Substring(0, result.Length - 2)
                                    End If

                                    row(dtColumnIndex) = result
                                End If


                            Catch ex As Exception
                                MessageBox.Show("Error (TemplateColumn at dtColumnIndex " & dtColumnIndex & "): " & ex.Message)
                            End Try

                        End If

                        dtColumnIndex += 1
                    Next
                    dt.Rows.Add(row)
                Next

                ' Now create the workbook and add the data
                Using workbook As New XLWorkbook()
                    ' Add a worksheet with the data from the DataTable
                    Dim worksheet = workbook.Worksheets.Add(worksheetName)

                    ' Add headers manually
                    For i As Integer = 0 To dt.Columns.Count - 1
                        worksheet.Cell(1, i + 1).Value = dt.Columns(i).ColumnName
                    Next

                    ' Add data manually
                    For i As Integer = 0 To dt.Rows.Count - 1
                        For j As Integer = 0 To dt.Columns.Count - 1
                            worksheet.Cell(i + 2, j + 1).Value = dt.Rows(i)(j).ToString()
                        Next
                    Next

                    ' Only create a table if we have data
                    If dt.Rows.Count > 0 Then
                        ' Define the range for the table
                        Dim range = worksheet.Range(
                            worksheet.Cell(1, 1).Address,
                            worksheet.Cell(dt.Rows.Count + 1, dt.Columns.Count).Address)

                        ' Create the table with a unique name
                        Dim uniqueTableName As String = $"ExportTable_{Guid.NewGuid().ToString("N").Substring(0, 8)}"
                        Dim table = range.CreateTable(uniqueTableName)

                        ' Apply a table style
                        table.Theme = XLTableTheme.TableStyleMedium2
                    End If

                    ' Adjust column widths
                    worksheet.Columns().AdjustToContents()

                    ' Save the workbook to the specified path
                    workbook.SaveAs(filePath)
                End Using

                MessageBox.Show("Export Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Return True
            Catch ex As Exception
                MessageBox.Show("Error exporting data: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Exports a generic collection to Excel file using a SaveFileDialog
        ''' </summary>
        Public Shared Function ExportCollectionToExcel(Of T)(collection As IEnumerable(Of T),
                                                           Optional defaultFileName As String = "Export",
                                                           Optional worksheetName As String = "Data",
                                                           Optional propertiesToExclude As List(Of String) = Nothing) As Boolean
            If collection Is Nothing OrElse Not collection.Any() Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return False
            End If

            ' Open SaveFileDialog
            Dim saveFileDialog As New SaveFileDialog() With {
                .Filter = "Excel Files (*.xlsx)|*.xlsx",
                .FileName = $"{defaultFileName}.xlsx",
                .OverwritePrompt = True
            }

            If saveFileDialog.ShowDialog() = True Then
                Return ExportCollectionToExcelFile(collection, saveFileDialog.FileName, worksheetName, propertiesToExclude)
            End If

            Return False
        End Function

        ''' <summary>
        ''' Exports a generic collection directly to a specified file path
        ''' </summary>
        Public Shared Function ExportCollectionToExcelFile(Of T)(collection As IEnumerable(Of T),
                                                               filePath As String,
                                                               Optional worksheetName As String = "Data",
                                                               Optional propertiesToExclude As List(Of String) = Nothing) As Boolean
            Try
                ' Ensure no existing file is locked
                Try
                    If File.Exists(filePath) Then
                        File.Delete(filePath)
                    End If
                Catch ex As Exception
                    ' If we can't delete, use a different filename
                    Dim dir = Path.GetDirectoryName(filePath)
                    Dim fileName = Path.GetFileNameWithoutExtension(filePath)
                    Dim ext = Path.GetExtension(filePath)
                    filePath = Path.Combine(dir, $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{ext}")
                End Try

                ' Get properties of type T (excluding those in the exclusion list)
                Dim properties = GetTypeProperties(Of T)(propertiesToExclude)

                ' Create a DataTable
                Dim dt As New DataTable()

                ' Create columns in datatable
                For Each prop In properties
                    dt.Columns.Add(prop.Name)
                Next

                ' Add data rows
                For Each item In collection
                    Dim row As DataRow = dt.NewRow()
                    For i As Integer = 0 To properties.Count - 1
                        Dim value = properties(i).GetValue(item, Nothing)
                        row(i) = If(value IsNot Nothing, value.ToString(), "")
                    Next
                    dt.Rows.Add(row)
                Next

                ' Create the workbook and add the data
                Using workbook As New XLWorkbook()
                    ' Add a worksheet with a specific name
                    Dim worksheet = workbook.Worksheets.Add(worksheetName)

                    ' Add headers manually
                    For i As Integer = 0 To dt.Columns.Count - 1
                        worksheet.Cell(1, i + 1).Value = dt.Columns(i).ColumnName
                    Next

                    ' Add data manually
                    For i As Integer = 0 To dt.Rows.Count - 1
                        For j As Integer = 0 To dt.Columns.Count - 1
                            worksheet.Cell(i + 2, j + 1).Value = dt.Rows(i)(j).ToString()
                        Next
                    Next

                    ' Only create a table if we have data
                    If dt.Rows.Count > 0 Then
                        ' Define the range for the table
                        Dim range = worksheet.Range(
                            worksheet.Cell(1, 1).Address,
                            worksheet.Cell(dt.Rows.Count + 1, dt.Columns.Count).Address)

                        ' Create the table with a unique name
                        Dim uniqueTableName As String = $"ExportTable_{Guid.NewGuid().ToString("N").Substring(0, 8)}"
                        Dim table = range.CreateTable(uniqueTableName)

                        ' Apply a table style
                        table.Theme = XLTableTheme.TableStyleMedium2
                    End If

                    ' Adjust column widths
                    worksheet.Columns().AdjustToContents()

                    ' Save the workbook to the specified path
                    workbook.SaveAs(filePath)
                End Using

                MessageBox.Show("Export Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Return True
            Catch ex As Exception
                MessageBox.Show("Error exporting data: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Gets a list of PropertyInfo objects for a type, excluding specified properties
        ''' </summary>
        Private Shared Function GetTypeProperties(Of T)(Optional propertiesToExclude As List(Of String) = Nothing) As List(Of PropertyInfo)
            Dim type = GetType(T)
            Dim properties = type.GetProperties().ToList()

            ' Filter out properties to exclude
            If propertiesToExclude IsNot Nothing AndAlso propertiesToExclude.Count > 0 Then
                properties = properties.Where(Function(p) Not propertiesToExclude.Contains(p.Name)).ToList()
            End If

            Return properties
        End Function

        Public Shared Sub ExportExcel(dataGrid As DataGrid, columnNumber As Integer, fileName As String)

            Dim limit As Integer = (dataGrid.Columns.Count - columnNumber) + 1

            ' Check if DataGrid has data
            If dataGrid.Items.Count = 0 Then
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Open SaveFileDialog
            Dim saveFileDialog As New SaveFileDialog() With {
                .Filter = "Excel Files (*.xlsx)|*.xlsx",
                .FileName = fileName + ".xlsx"
            }

            If saveFileDialog.ShowDialog() = True Then
                Try
                    ' Create Excel workbook
                    Using workbook As New XLWorkbook()
                        Dim dt As New DataTable()

                        ' Add DataGrid columns as table headers
                        For i As Integer = 0 To dataGrid.Columns.Count - limit
                            Dim column As DataGridColumn = dataGrid.Columns(i)

                            dt.Columns.Add(column.Header.ToString())
                        Next

                        ' Add rows from DataGrid items
                        For Each item In dataGrid.Items
                            Dim row As DataRow = dt.NewRow()
                            Dim dataRowView = CType(item, DataRowView)

                            For i As Integer = 0 To dataGrid.Columns.Count - limit
                                Dim column As DataGridColumn = dataGrid.Columns(i)

                                row(column.Header.ToString()) = dataRowView(i).ToString()
                            Next

                            dt.Rows.Add(row)
                        Next

                        ' Add table to Excel sheet
                        Dim worksheet = workbook.Worksheets.Add(dt, fileName)
                        worksheet.Columns().AdjustToContents()

                        ' Save Excel file
                        workbook.SaveAs(saveFileDialog.FileName)
                        MessageBox.Show("Export Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error exporting data: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub
    End Class
End Namespace