Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports Newtonsoft.Json
Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MongoDB.Driver.GridFS
Imports System.IO
Imports System.Diagnostics
Imports System.Threading.Tasks

Public Class FileView

    Private _currentClientID As String
    Private _currentClientName As String
    Private _clientQuotes As List(Of QuotesModel)

    ' MongoDB variables
    Private _gridFS As GridFSBucket
    Private _mongoDatabase As IMongoDatabase
    Private _fsFilesCollection As IMongoCollection(Of BsonDocument)

    Public Sub New()
        InitializeComponent()
        _clientQuotes = New List(Of QuotesModel)()

        Try
            _mongoDatabase = DPC.SplashScreen.GetMongoDatabaseConnection()
            _gridFS = DPC.SplashScreen.GetGridFSConnection()
            _fsFilesCollection = _mongoDatabase.GetCollection(Of BsonDocument)("fs.files")
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("MongoDB Init Error: " & ex.Message)
        End Try
    End Sub

    Public Sub LoadClientData(client As Object)
        If client IsNot Nothing Then
            _currentClientID = client.ClientID.ToString()
            _currentClientName = client.Name

            txtFullName.Text = client.Name
            txtPhone.Text = client.Phone
            txtEmail.Text = client.Email

            Dim billingAdd As String = If(client.BillingAddress IsNot Nothing, client.BillingAddress.ToString(), "")
            Dim billingParts As String() = billingAdd.Split(New String() {", "}, System.StringSplitOptions.None)

            txtAddress.Text = If(billingParts.Length > 0, billingParts(0), "")
            txtCity.Text = If(billingParts.Length > 1, billingParts(1), "")
            txtRegion.Text = If(billingParts.Length > 2, billingParts(2), "")
            txtZipCode.Text = If(billingParts.Length > 4, billingParts(4), "")

            LoadClientFilesAsync()
        End If
    End Sub

    Private Async Sub LoadClientFilesAsync()
        Try
            Dim ceDocuments As New List(Of FileItemModel)
            Dim bsDocuments As New List(Of FileItemModel)
            Dim drDocuments As New List(Of FileItemModel)
            Dim soaDocuments As New List(Of FileItemModel)

            _clientQuotes.Clear()
            Dim clientQuoteNumbers As New List(Of String)()

            ' 1. Fetch ALL physical files from the database
            Dim sortDef = Builders(Of BsonDocument).Sort.Descending("uploadDate")
            Dim rawGridFSFiles = Await _fsFilesCollection.Find(New BsonDocument()).Sort(sortDef).ToListAsync()

            ' STRICT PDF FILTER: ONLY display physical PDF files.
            Dim allGridFSFiles = rawGridFSFiles.Where(Function(f) f("filename").AsString.ToLower().EndsWith(".pdf")).ToList()

            ' ==========================================
            ' 1. LOAD COST ESTIMATES & GET QUOTE NUMBERS
            ' ==========================================
            Dim allQuotes = QuotesController.GetQuotes(5000, "All")
            If allQuotes IsNot Nothing Then
                For Each quote In allQuotes
                    Dim isMatch As Boolean = (quote.ClientID IsNot Nothing AndAlso quote.ClientID.ToString() = _currentClientID) OrElse
                                             (quote.ClientName IsNot Nothing AndAlso quote.ClientName = _currentClientName)

                    If isMatch Then
                        _clientQuotes.Add(quote)

                        ' Save the exact quote number (e.g. "WICE-03262026-0001")
                        Dim qNum As String = quote.QuoteNumber.ToUpper()
                        clientQuoteNumbers.Add(qNum)

                        ' Also save the base quote number without the prefix (e.g. "03262026-0001")
                        If qNum.Contains("-") Then
                            Dim baseId As String = qNum.Substring(qNum.IndexOf("-") + 1)
                            clientQuoteNumbers.Add(baseId)
                        End If

                        ' Scan GridFS for uploaded PDFs containing this Quote Number
                        Dim physicalFile = allGridFSFiles.FirstOrDefault(Function(f) f("filename").AsString.ToUpper().Contains(qNum))

                        If physicalFile IsNot Nothing Then
                            Dim uploadDate As DateTime = physicalFile("uploadDate").ToUniversalTime().ToLocalTime()
                            ceDocuments.Add(New FileItemModel With {
                                .FileType = "CE",
                                .FileName = physicalFile("filename").AsString,
                                .DocumentID = quote.QuoteNumber,
                                .GridFSId = physicalFile("_id").ToString(),
                                .DateCreated = uploadDate.ToString("MMM dd, yyyy"),
                                .RawDate = uploadDate
                            })
                        End If
                    End If
                Next
            End If

            ' ==========================================
            ' 2. STRICT MATCH: BILLING (BS), DELIVERY (DR), SOA
            ' ==========================================
            ' Clean the client name (e.g. "WEST PARC CONDOMINIUM")
            Dim cleanClientName As String = _currentClientName.Replace(" ", "").ToUpper()
            Dim exactClientName As String = _currentClientName.ToUpper()

            For Each physicalFile In allGridFSFiles
                Dim fileName As String = physicalFile("filename").AsString
                Dim fileNameUpper As String = fileName.ToUpper()

                ' Skip CE files so they don't duplicate
                If fileNameUpper.StartsWith("GPCE") OrElse fileNameUpper.StartsWith("WICE") OrElse fileNameUpper.StartsWith("BCCE") OrElse fileNameUpper.StartsWith("HHCE") Then
                    Continue For
                End If

                Dim belongsToClient As Boolean = False

                ' RULE 1: EXACT NAME MATCH (e.g. BL-WEST PARC CONDOMINIUM.pdf)
                If fileNameUpper.Contains(exactClientName) OrElse fileNameUpper.Replace(" ", "").Contains(cleanClientName) Then
                    belongsToClient = True
                End If

                ' RULE 2: EXACT QUOTE NUMBER MATCH (e.g. BL-03262026-0001.pdf)
                If Not belongsToClient Then
                    For Each qNum In clientQuoteNumbers
                        If fileNameUpper.Contains(qNum) Then
                            belongsToClient = True
                            Exit For
                        End If
                    Next
                End If

                ' If it perfectly matches, put it in the correct folder
                If belongsToClient Then
                    Dim uploadDate As DateTime = physicalFile("uploadDate").ToUniversalTime().ToLocalTime()
                    Dim fileModel As New FileItemModel With {
                        .FileName = fileName,
                        .DocumentID = fileName,
                        .GridFSId = physicalFile("_id").ToString(),
                        .DateCreated = uploadDate.ToString("MMM dd, yyyy"),
                        .RawDate = uploadDate
                    }

                    If fileNameUpper.StartsWith("BL") OrElse fileNameUpper.StartsWith("BS") Then
                        fileModel.FileType = "BS"
                        bsDocuments.Add(fileModel)
                    ElseIf fileNameUpper.StartsWith("DR") Then
                        fileModel.FileType = "DR"
                        drDocuments.Add(fileModel)
                    ElseIf fileNameUpper.StartsWith("SOA") Then
                        fileModel.FileType = "SOA"
                        soaDocuments.Add(fileModel)
                    End If
                End If
            Next

            ' ==========================================
            ' APPLY BINDINGS & SORT ALPHABETICALLY BY FILE NAME (A-Z)
            ' ==========================================
            If CEFilesList IsNot Nothing Then CEFilesList.ItemsSource = ceDocuments.OrderBy(Function(x) x.FileName).ToList()
            If BSFilesList IsNot Nothing Then BSFilesList.ItemsSource = bsDocuments.OrderBy(Function(x) x.FileName).ToList()
            If DRFilesList IsNot Nothing Then DRFilesList.ItemsSource = drDocuments.OrderBy(Function(x) x.FileName).ToList()
            If SOAFilesList IsNot Nothing Then SOAFilesList.ItemsSource = soaDocuments.OrderBy(Function(x) x.FileName).ToList()

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Failed to load client files: " & ex.Message)
        End Try
    End Sub

    Private Async Sub OpenFile_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim btn As Button = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim selectedDoc = TryCast(btn.DataContext, FileItemModel)
            If selectedDoc Is Nothing Then Return

            If Not String.IsNullOrEmpty(selectedDoc.GridFSId) Then
                Await PreviewGridFSFile(selectedDoc.GridFSId, selectedDoc.FileName)
            End If

        Catch ex As Exception
            MessageBox.Show("Error opening file: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Async Function PreviewGridFSFile(fileId As String, fileName As String) As Task
        Try
            Me.Cursor = Cursors.Wait
            Dim tempFolder As String = Path.GetTempPath()
            Dim uniqueFileName As String = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.ToString("HHmmss")}{Path.GetExtension(fileName)}"
            Dim tempFilePath As String = Path.Combine(tempFolder, uniqueFileName)

            Await Task.Run(Async Function()
                               Using fileStream As New FileStream(tempFilePath, FileMode.Create, FileAccess.Write)
                                   Await _gridFS.DownloadToStreamAsync(New ObjectId(fileId), fileStream)
                               End Using
                           End Function)

            Dim pInfo As New ProcessStartInfo(tempFilePath) With {
                .UseShellExecute = True
            }
            Process.Start(pInfo)

        Catch ex As Exception
            MessageBox.Show($"Error opening physical file: {ex.Message}", "Preview Error", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            Me.Cursor = Cursors.Arrow
        End Try
    End Function

    Private Sub LoadQuoteIntoCache(quote As QuotesModel)
        ' Left empty to prevent errors if previously referenced
    End Sub

    Private Sub LoadBillingIntoCache(invoiceNumber As String)
        ' Left empty to prevent errors if previously referenced
    End Sub

    Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
    End Sub

End Class

Public Class FileItemModel
    Public Property FileType As String
    Public Property FileName As String
    Public Property DateCreated As String
    Public Property RawDate As DateTime
    Public Property DocumentID As String
    Public Property GridFSId As String
End Class