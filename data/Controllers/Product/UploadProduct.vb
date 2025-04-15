Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers

Namespace DPC.Data.Controllers
    Public Class UploadProduct
        Private Shared ReadOnly Property MainContainer As StackPanel
            Get
                Return ProductController.MainContainer
            End Get
        End Property
        Private Shared ReadOnly Property TxtStockUnits As TextBox
            Get
                Return ProductController.TxtStockUnits
            End Get
        End Property
        Private Shared ReadOnly Property SerialNumbers As List(Of TextBox)
            Get
                Return ProductController.SerialNumbers
            End Get
        End Property

        'function to import serialnumber
        Public Shared Sub ImportSerialNumbers()
            Dim openFileDialog As New Microsoft.Win32.OpenFileDialog With {
                .Filter = "CSV Files (*.csv)|*.csv"
            }

            If openFileDialog.ShowDialog() = True Then
                Try
                    Dim filePath As String = openFileDialog.FileName
                    Dim lines As List(Of String) = IO.File.ReadAllLines(filePath).ToList()

                    ' Ensure there are serial numbers available
                    If lines.Count <= 1 Then
                        MessageBox.Show("No serial numbers found in the file.")
                        Return
                    End If

                    ' Remove the first line (title "Serial Number") and read the rest
                    lines.RemoveAt(0)

                    ' Clear existing rows if any
                    MainContainer.Children.Clear()
                    SerialNumbers.Clear()

                    ' Process serial numbers
                    Dim serialNumbersData As New List(Of String)
                    For Each line In lines
                        Dim serialNumber As String = line.Trim()
                        If Not String.IsNullOrWhiteSpace(serialNumber) Then
                            serialNumbersData.Add(serialNumber)
                        End If
                    Next

                    If serialNumbersData.Any() Then
                        ' Update stock units once
                        TxtStockUnits.Text = serialNumbersData.Count.ToString()

                        ' Dynamically add rows for each serial number
                        For Each serialNumber In serialNumbersData
                            ProductController.BtnAddRow_Click(Nothing, Nothing, True)
                            If SerialNumbers.Count > 0 Then
                                SerialNumbers.Last().Text = serialNumber
                            End If
                        Next

                        'MessageBox.Show($"Successfully imported {serialNumbersData.Count} serial numbers.")
                    Else
                        MessageBox.Show("No valid serial numbers found.")
                    End If

                Catch ex As Exception
                    MessageBox.Show($"Error importing serial numbers: {ex.Message}")
                End Try
            End If
        End Sub
    End Class
End Namespace
