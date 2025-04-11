Imports System.Data
Imports System.IO
Imports System.Windows.Controls
Imports System.Reflection
Imports System.Windows
Imports Microsoft.Win32
Imports ClosedXML.Excel

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
                                                   Optional worksheetName As String = "Data") As Boolean
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
                Return ExportDataGridToExcelFile(dataGrid, saveFileDialog.FileName, worksheetName, columnsToExclude)
            End If

            Return False
        End Function

        ''' <summary>
        ''' Exports DataGrid content directly to a specified file path
        ''' </summary>
        Public Shared Function ExportDataGridToExcelFile(dataGrid As DataGrid,
                                                       filePath As String,
                                                       Optional worksheetName As String = "Data",
                                                       Optional columnsToExclude As List(Of String) = Nothing) As Boolean
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
                            row(dtColumnIndex) = ""
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
    End Class
End Namespace