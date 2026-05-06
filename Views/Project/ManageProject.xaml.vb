Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports System.Linq
Imports DPC.DPC.Data.Helpers
Imports MySql.Data.MySqlClient
Imports System.Windows.Media

Namespace DPC.Views.Project
    Public Class ManageProject
        Inherits UserControl

        ' ── Shared slot: EditProject reads this on load ──────────
        Public Shared SelectedProject As DPC.Data.Model.Project = Nothing

        Private _projectID As String
        Private _allProjects As List(Of DPC.Data.Model.Project)
        Private _allProjectsTotal As List(Of DPC.Data.Model.Project)
        Private _filteredProjects As List(Of DPC.Data.Model.Project)
        Private _currentPage As Integer = 1
        Private _pageSize As Integer = 10

        Public Sub New()
            InitializeComponent()
            AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
        End Sub

        Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
            LoadData()
        End Sub

        ' =========================================================
        ' LOAD DATA
        ' =========================================================
        Public Sub LoadData()
            Try
                ' Active grid list (DPC GOV SALES is the default view)
                _allProjects = DPC.Data.Controllers.ProjectController.GetProjects()
                If _allProjects Is Nothing Then _allProjects = New List(Of DPC.Data.Model.Project)()

                ' Combined total for the status cards — always all three lists
                RefreshTotalForCards()

                _filteredProjects = _allProjects
                _currentPage = 1
                ApplyPagination()
                UpdateStatusCounts()
            Catch ex As Exception
                MessageBox.Show("Error retrieving project data: " & ex.Message,
                        "Data Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Rebuilds _allProjectsTotal from all three lists so the cards are always global
        Private Sub RefreshTotalForCards()
            Dim govSales = DPC.Data.Controllers.ProjectController.GetProjects()
            Dim awarded = DPC.Data.Controllers.ProjectController.GetAwardedProjects()
            Dim collection = DPC.Data.Controllers.ProjectController.GetCollectionData()

            _allProjectsTotal = New List(Of DPC.Data.Model.Project)()
            If govSales IsNot Nothing Then _allProjectsTotal.AddRange(govSales)
            If awarded IsNot Nothing Then _allProjectsTotal.AddRange(awarded)
            If collection IsNot Nothing Then _allProjectsTotal.AddRange(collection)
        End Sub

        ' =========================================================
        ' STATUS SUMMARY CARDS  (uses _allProjectsTotal — always global)
        ' =========================================================
        Private Sub UpdateStatusCounts()
            Dim source = If(_allProjectsTotal, New List(Of DPC.Data.Model.Project)())

            Dim awardedCount = source.Where(Function(p) String.Equals(p.Status, "AWARDED", StringComparison.OrdinalIgnoreCase)).Count()
            Dim ongoingCount = source.Where(Function(p) String.Equals(p.Status, "ON-GOING", StringComparison.OrdinalIgnoreCase)).Count()
            Dim doneCount = source.Where(Function(p) String.Equals(p.Status, "DONE", StringComparison.OrdinalIgnoreCase)).Count()
            Dim cancelledCount = source.Where(Function(p) String.Equals(p.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase)).Count()

            tbAwarded.Text = awardedCount.ToString()
            tbOnGoing.Text = ongoingCount.ToString()
            tbCompleted.Text = doneCount.ToString()
            tbCancelled.Text = cancelledCount.ToString()
        End Sub

        ' =========================================================
        ' PAGE SIZE DROP-DOWN  (list switcher)
        ' =========================================================
        Private Sub CmbPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If _allProjects Is Nothing Then Return

            Dim combo = TryCast(sender, ComboBox)
            If combo IsNot Nothing AndAlso combo.SelectedItem IsNot Nothing Then
                Dim selectedItem = TryCast(combo.SelectedItem, ComboBoxItem)
                If selectedItem IsNot Nothing Then
                    Select Case selectedItem.Content.ToString()
                        Case "DPC GOV SALES"
                            LoadDPCGovSalesData()
                        Case "AWARDED PROJECTS"
                            LoadAwardedProjectsData()
                        Case "COLLECTION"
                            LoadCollectionData()
                    End Select
                End If
            End If
        End Sub

        ' =========================================================
        ' PAGINATION
        ' =========================================================
        Private Sub ApplyPagination()
            If _filteredProjects Is Nothing Then Return

            Dim totalPages As Integer = Math.Max(1, Math.Ceiling(_filteredProjects.Count / _pageSize))
            If _currentPage > totalPages Then _currentPage = totalPages
            If _currentPage < 1 Then _currentPage = 1

            Dim paged = _filteredProjects.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList()

            If ProjectDataGrid Is Nothing Then Return

            ProjectDataGrid.ItemsSource = New ObservableCollection(Of DPC.Data.Model.Project)(paged)

            UpdatePageButtons(totalPages)
            UpdateStatusCounts()
        End Sub

        Private Sub UpdatePageButtons(totalPages As Integer)
            PageButtonsPanel.Items.Clear()

            For i As Integer = 1 To totalPages
                Dim pageNum = i
                Dim btn As New Button()
                btn.Content = pageNum.ToString()
                btn.FontSize = 14
                btn.Width = 30
                btn.Height = 30
                btn.FontFamily = New FontFamily("Lexend")
                btn.Margin = New Thickness(3, 0, 3, 0)
                btn.Tag = pageNum

                If pageNum = _currentPage Then
                    btn.Background = New SolidColorBrush(Color.FromRgb(85, 85, 85))
                    btn.Foreground = Brushes.White
                Else
                    btn.Background = Brushes.Transparent
                    btn.Foreground = New SolidColorBrush(Color.FromRgb(85, 85, 85))
                End If

                Dim factory As New FrameworkElementFactory(GetType(Border))
                factory.SetBinding(Border.BackgroundProperty,
                    New Binding("Background") With {
                        .RelativeSource = New RelativeSource(RelativeSourceMode.TemplatedParent)})
                factory.SetValue(Border.CornerRadiusProperty, New CornerRadius(15))
                Dim cp As New FrameworkElementFactory(GetType(ContentPresenter))
                cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center)
                cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center)
                factory.AppendChild(cp)
                btn.Template = New ControlTemplate(GetType(Button)) With {.VisualTree = factory}

                AddHandler btn.Click, AddressOf BtnPage_Click
                PageButtonsPanel.Items.Add(btn)
            Next
        End Sub

        Private Sub BtnPage_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn Is Nothing Then Return
            _currentPage = CInt(btn.Tag)
            ApplyPagination()
        End Sub

        Private Sub BtnPrev_Click(sender As Object, e As RoutedEventArgs)
            If _currentPage > 1 Then
                _currentPage -= 1
                ApplyPagination()
            End If
        End Sub

        Private Sub BtnNext_Click(sender As Object, e As RoutedEventArgs)
            Dim totalPages As Integer = Math.Max(1, Math.Ceiling(_filteredProjects.Count / _pageSize))
            If _currentPage < totalPages Then
                _currentPage += 1
                ApplyPagination()
            End If
        End Sub

        ' =========================================================
        ' LIST LOADERS
        ' Note: RefreshTotalForCards() is NOT called here so the
        ' card totals stay global even as the grid view changes.
        ' =========================================================
        Private Sub LoadDPCGovSalesData()
            Try
                _allProjects = DPC.Data.Controllers.ProjectController.GetProjects()
                If _allProjects Is Nothing Then _allProjects = New List(Of DPC.Data.Model.Project)()
                _filteredProjects = _allProjects
                _currentPage = 1
                txtSearch.Text = ""
                ApplyPagination()
            Catch ex As Exception
                MessageBox.Show("Error loading DPC GOV SALES: " & ex.Message)
            End Try
        End Sub

        Private Sub LoadAwardedProjectsData()
            Try
                _allProjects = DPC.Data.Controllers.ProjectController.GetAwardedProjects()
                If _allProjects Is Nothing Then _allProjects = New List(Of DPC.Data.Model.Project)()
                _filteredProjects = _allProjects
                _currentPage = 1
                txtSearch.Text = ""
                ApplyPagination()
            Catch ex As Exception
                MessageBox.Show("Error loading AWARDED PROJECTS: " & ex.Message)
            End Try
        End Sub

        Private Sub LoadCollectionData()
            Try
                _allProjects = DPC.Data.Controllers.ProjectController.GetCollectionData()
                If _allProjects Is Nothing Then _allProjects = New List(Of DPC.Data.Model.Project)()
                _filteredProjects = _allProjects
                _currentPage = 1
                txtSearch.Text = ""
                ApplyPagination()
            Catch ex As Exception
                MessageBox.Show("Error loading COLLECTION: " & ex.Message)
            End Try
        End Sub

        ' =========================================================
        ' SEARCH  (searches within the active list only)
        ' =========================================================
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _allProjects Is Nothing Then Return

            Dim keyword = txtSearch.Text.ToLower().Trim()

            If String.IsNullOrEmpty(keyword) Then
                _filteredProjects = _allProjects
            Else
                _filteredProjects = _allProjects.Where(Function(p)
                                                           Return (p.ProjectTitle?.ToLower().Contains(keyword)) OrElse
                                                                  (p.ReferenceNumber?.ToLower().Contains(keyword)) OrElse
                                                                  (p.Category?.ToLower().Contains(keyword)) OrElse
                                                                  (p.ProjectType?.ToLower().Contains(keyword)) OrElse
                                                                  (p.Status?.ToLower().Contains(keyword)) OrElse
                                                                  (p.Remarks?.ToLower().Contains(keyword)) OrElse
                                                                  (p.AssignSales?.ToLower().Contains(keyword)) OrElse
                                                                  (p.ProjectID.ToString().Contains(keyword))
                                                       End Function).ToList()
            End If

            _currentPage = 1
            ApplyPagination()
        End Sub

        ' =========================================================
        ' EDIT
        ' =========================================================
        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim projectID = btn.Tag?.ToString()
            Dim project = _allProjects?.FirstOrDefault(Function(p) p.ProjectID.ToString() = projectID)

            If project IsNot Nothing Then
                SelectedProject = project
                ViewLoader.DynamicView.NavigateToView("editproject", Me)
            End If
        End Sub

        ' =========================================================
        ' DELETE
        ' =========================================================
        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn Is Nothing Then Return
            _projectID = btn.Tag?.ToString()

            Dim confirmModal As New DPC.Components.ConfirmationModals.DeleteProductConfirmation()
            AddHandler confirmModal.Confirm, AddressOf DeleteProjectConfirmation_Closed

            Dim parentWindow As Window = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, confirmModal, "windowcenter", -100, 0, False, parentWindow)
        End Sub

        Private Sub DeleteProjectConfirmation_Closed()
            Dim query As String = "DELETE FROM project WHERE projectID = '" & _projectID & "'"
            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString
            Try
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
                ProjectDataGrid.ItemsSource = Nothing

                ' Reload everything so cards stay accurate after a delete
                LoadData()
            Catch ex As Exception
                MessageBox.Show("Error deleting project: " & ex.Message)
            End Try
        End Sub

        ' =========================================================
        ' EXCEL / CSV EXPORT
        ' =========================================================
        Private Sub BtnExportExcel_Click(sender As Object, e As RoutedEventArgs)
            If _filteredProjects Is Nothing OrElse _filteredProjects.Count = 0 Then
                MessageBox.Show("There are no projects to export.", "Empty Data",
                                MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            Dim sfd As New Microsoft.Win32.SaveFileDialog()
            sfd.Filter = "Excel CSV File (*.csv)|*.csv"
            sfd.FileName = "Project_Report_" & DateTime.Now.ToString("yyyyMMdd") & ".csv"

            If sfd.ShowDialog() = True Then
                Try
                    Dim sb As New System.Text.StringBuilder()

                    sb.AppendLine("Project ID,Date,Reference Number,Project Title,Category,Project Type," &
                                  "Contact Person,Contact Number,Email Address,Area of Delivery," &
                                  "Pre-Bid Date,Closing Date,ABC,Bid/RFQ Offer,Receive Date," &
                                  "Mode of Submission,Status,Remarks,Assign Sales,Note")

                    For Each p In _filteredProjects
                        Dim row As New List(Of String) From {
                            EscapeCsv(p.ProjectID.ToString()),
                            EscapeCsv(If(p.ProjectDate.HasValue, p.ProjectDate.Value.ToString("MMM dd, yyyy"), "")),
                            EscapeCsv(p.ReferenceNumber),
                            EscapeCsv(p.ProjectTitle),
                            EscapeCsv(p.Category),
                            EscapeCsv(p.ProjectType),
                            EscapeCsv(p.ContactPerson),
                            EscapeCsv(p.ContactNumber),
                            EscapeCsv(p.EmailAddress),
                            EscapeCsv(p.AreaOfDelivery),
                            EscapeCsv(If(p.PreBidDate.HasValue, p.PreBidDate.Value.ToString("MMM dd, yyyy"), "")),
                            EscapeCsv(If(p.ClosingDate.HasValue, p.ClosingDate.Value.ToString("MMM dd, yyyy"), "")),
                            EscapeCsv(p.ABC.ToString("N0")),
                            EscapeCsv(p.BidRFQOffer.ToString("N0")),
                            EscapeCsv(If(p.ReceiveDate.HasValue, p.ReceiveDate.Value.ToString("MMM dd, yyyy"), "")),
                            EscapeCsv(p.ModeOfSubmission),
                            EscapeCsv(p.Status),
                            EscapeCsv(p.Remarks),
                            EscapeCsv(p.AssignSales),
                            EscapeCsv(p.Note)
                        }
                        sb.AppendLine(String.Join(",", row))
                    Next

                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString())
                    MessageBox.Show("Exported successfully! You can now open this file in Excel.",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                Catch ex As Exception
                    MessageBox.Show("Error exporting data: " & ex.Message,
                                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        Private Function EscapeCsv(value As String) As String
            If String.IsNullOrWhiteSpace(value) Then Return """"""
            Return """" & value.Replace("""", """""") & """"
        End Function

    End Class
End Namespace
