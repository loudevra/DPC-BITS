Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports Newtonsoft.Json

Public Class FileView

    Private _currentClientID As String
    Private _currentClientName As String
    Private _clientQuotes As List(Of QuotesModel)

    Public Sub New()
        InitializeComponent()
        _clientQuotes = New List(Of QuotesModel)()
    End Sub

    ' Receives client data from CRM view
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

            LoadClientFiles()
        End If
    End Sub

    Private Sub LoadClientFiles()
        Try
            Dim ceDocuments As New List(Of FileItemModel)
            Dim bsDocuments As New List(Of FileItemModel)
            _clientQuotes.Clear()

            ' ==========================================
            ' 1. LOAD COST ESTIMATES (CE FOLDER)
            ' ==========================================
            Dim allQuotes = QuotesController.GetQuotes(5000, "All")
            If allQuotes IsNot Nothing Then
                For Each quote In allQuotes
                    Dim isMatch As Boolean = False
                    If quote.ClientID IsNot Nothing AndAlso quote.ClientID.ToString() = _currentClientID Then
                        isMatch = True
                    ElseIf quote.ClientName IsNot Nothing AndAlso quote.ClientName = _currentClientName Then
                        isMatch = True
                    End If

                    If isMatch Then
                        _clientQuotes.Add(quote)
                        ceDocuments.Add(New FileItemModel With {
                            .FileType = "CE",
                            .FileName = quote.QuoteNumber,
                            .DocumentID = quote.QuoteNumber,
                            .DateCreated = quote.QuoteDate
                        })
                    End If
                Next
            End If
            CEFilesList.ItemsSource = ceDocuments

            ' ==========================================
            ' 2. LOAD BILLING STATEMENTS (BS FOLDER)
            ' ==========================================
            ' Because the raw SQL was breaking your app, we are using the Dummy UI files here. 
            ' Once you have a "BillingController", you can load them exactly like the QuotesController above!
            bsDocuments.Add(New FileItemModel With {
                .FileType = "BS",
                .FileName = "BS-04102026-0001",
                .DocumentID = "BS-04102026-0001",
                .DateCreated = "Apr 10, 2026"
            })
            bsDocuments.Add(New FileItemModel With {
                .FileType = "BS",
                .FileName = "BS-11152025-0002",
                .DocumentID = "BS-11152025-0002",
                .DateCreated = "Nov 15, 2025"
            })
            BSFilesList.ItemsSource = bsDocuments

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Failed to load client files: " & ex.Message)
        End Try
    End Sub

    Private Sub OpenFile_Click(sender As Object, e As RoutedEventArgs)
        Try
            Dim btn As Button = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim selectedDoc = TryCast(btn.DataContext, FileItemModel)
            If selectedDoc Is Nothing Then Return

            If selectedDoc.FileType = "CE" Then
                Dim targetQuote = _clientQuotes.FirstOrDefault(Function(q) q.QuoteNumber = selectedDoc.DocumentID)
                If targetQuote IsNot Nothing Then
                    LoadQuoteIntoCache(targetQuote)
                End If
                ViewLoader.DynamicView.NavigateToView("costestimate", Me)

            ElseIf selectedDoc.FileType = "BS" Then
                ' Triggers the Billing Statement Cache
                LoadBillingIntoCache(selectedDoc.DocumentID)
                ViewLoader.DynamicView.NavigateToView("billingestimate", Me)
            End If

        Catch ex As Exception
            MessageBox.Show("Error opening file: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    ' -------------------------------------------------------------
    ' CACHE HELPERS (Pushes data to the next screen before it opens)
    ' -------------------------------------------------------------

    Private Sub LoadQuoteIntoCache(quote As QuotesModel)
        Try
            CostEstimateDetails.CEQuoteNumberCache = quote.QuoteNumber
            CostEstimateDetails.CEQuoteDateCache = quote.QuoteDate
            CostEstimateDetails.CEValidUntilDate = quote.Validity
            CostEstimateDetails.CETotalBaseAmount = quote.TotalPrice
            CostEstimateDetails.CETotalAmountCache = quote.TotalPrice

            CostEstimateDetails.CEClientName = txtFullName.Text
            CostEstimateDetails.CEAddress = txtAddress.Text
            CostEstimateDetails.CECity = txtCity.Text
            CostEstimateDetails.CERegion = txtRegion.Text
            CostEstimateDetails.CEPhone = txtPhone.Text
            CostEstimateDetails.CEEmail = txtEmail.Text

            Dim jsonString As String = If(quote.OrderItems IsNot Nothing, quote.OrderItems.ToString(), "[]")
            Dim rawData = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(jsonString)
            CostEstimateDetails.CEQuoteItemsCache = rawData
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error loading quote cache: " & ex.Message)
        End Try
    End Sub

    Private Sub LoadBillingIntoCache(invoiceNumber As String)
        Try
            ' 1. Pass the Perspective Client info to ensure the Billing Statement header stays filled!
            CostEstimateDetails.CEClientName = txtFullName.Text
            CostEstimateDetails.CEAddress = txtAddress.Text
            CostEstimateDetails.CECity = txtCity.Text
            CostEstimateDetails.CERegion = txtRegion.Text
            CostEstimateDetails.CEPhone = txtPhone.Text
            CostEstimateDetails.CEEmail = txtEmail.Text

            ' 2. Pass the clicked Invoice Number so the next screen knows which one to load
            CostEstimateDetails.CEQuoteNumberCache = invoiceNumber
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error loading billing cache: " & ex.Message)
        End Try
    End Sub

    Private Sub TextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
    End Sub

End Class

Public Class FileItemModel
    Public Property FileType As String
    Public Property FileName As String
    Public Property DateCreated As String
    Public Property DocumentID As String
End Class