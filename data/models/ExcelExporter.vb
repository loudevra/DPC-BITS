Imports System.Collections.ObjectModel
Imports System.IO
Imports DPC.DPC.Data.Models
Imports OfficeOpenXml
Imports OfficeOpenXml.Style

Namespace DPC.Data.Helpers
    Public Class ExcelExporter
        ' Export documents collection to Excel file
        Public Sub ExportDocuments(documents As ObservableCollection(Of Document), filePath As String)
            ' Set license context for EPPlus
            ExcelPackage.License.SetNonCommercialOrganization("DREAM PC BUILDS")

            Using package As New ExcelPackage()
                ' Create worksheet
                Dim worksheet = package.Workbook.Worksheets.Add("Documents")

                ' Set headers
                worksheet.Cells(1, 1).Value = "ID"
                worksheet.Cells(1, 2).Value = "Title"
                worksheet.Cells(1, 3).Value = "File Name"
                worksheet.Cells(1, 4).Value = "File Type"
                worksheet.Cells(1, 5).Value = "File Size"
                worksheet.Cells(1, 6).Value = "Upload Date"

                ' Format header row
                Using headerRange = worksheet.Cells(1, 1, 1, 6)
                    headerRange.Style.Font.Bold = True
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid

                    ' Set background color to Light Gray
                    headerRange.Style.Fill.BackgroundColor.SetColor(255, 211, 211, 211)

                    ' Set font color to Black
                    headerRange.Style.Font.Color.SetColor(255, 0, 0, 0)
                End Using

                ' Add data
                Dim row = 2
                For Each doc In documents
                    worksheet.Cells(row, 1).Value = doc.DocumentID
                    worksheet.Cells(row, 2).Value = doc.Title
                    worksheet.Cells(row, 3).Value = doc.FileName
                    worksheet.Cells(row, 4).Value = doc.FileType
                    worksheet.Cells(row, 5).Value = Base64Utility.GetReadableFileSize(doc.FileSize)
                    worksheet.Cells(row, 6).Value = doc.UploadDate
                    worksheet.Cells(row, 6).Style.Numberformat.Format = "dd-mm-yyyy"
                    row += 1
                Next

                ' Auto fit columns
                worksheet.Cells.AutoFitColumns()

                ' Save the file
                package.SaveAs(New FileInfo(filePath))
            End Using
        End Sub
    End Class
End Namespace
