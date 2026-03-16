' ManageProject.xaml.vb (UPDATED: restore Cancelled count to tbTotal)
Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports System.Linq
Imports DPC.DPC.Data.Helpers
Imports MySql.Data.MySqlClient
Imports System.Windows.Media

Namespace DPC.Views.Project
    Public Class ManageProject
        Inherits UserControl

        Private _projectID As String
        Private _allProjects As List(Of DPC.Data.Model.Project)
        Private _filteredProjects As List(Of DPC.Data.Model.Project)
        Private _currentPage As Integer = 1
        Private _pageSize As Integer = 10

        Public Sub New()
            InitializeComponent()
            AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
            LoadData()
        End Sub

        Public Sub LoadData()
            Try
                _allProjects = DPC.Data.Controllers.ProjectController.GetProjects()
                If _allProjects Is Nothing Then
                    _allProjects = New List(Of DPC.Data.Model.Project)()
                End If
                _filteredProjects = _allProjects
                _currentPage = 1
                ApplyPagination()
                UpdateStatusCounts()
            Catch ex As Exception
                MessageBox.Show("Error retrieving project data: " & ex.Message, "Data Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub UpdateStatusCounts()
            If _filteredProjects Is Nothing Then
                tbWaiting.Text = "0"
                tbProcessing.Text = "0"
                tbSolved.Text = "0"
                tbTotal.Text = "0"
                Return
            End If

            Dim waiting = _filteredProjects.Where(Function(p) String.Equals(p.Status, "Waiting", StringComparison.OrdinalIgnoreCase)).Count()
            Dim processing = _filteredProjects.Where(Function(p) String.Equals(p.Status, "Processing", StringComparison.OrdinalIgnoreCase)).Count()
            Dim solved = _filteredProjects.Where(Function(p) String.Equals(p.Status, "Solved", StringComparison.OrdinalIgnoreCase)).Count()
            Dim cancelled = _filteredProjects.Where(Function(p) String.Equals(p.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)).Count()

            tbWaiting.Text = waiting.ToString()
            tbProcessing.Text = processing.ToString()
            tbSolved.Text = solved.ToString()

            ' Keep your original behavior: tbTotal shows Cancelled count
            tbTotal.Text = cancelled.ToString()
        End Sub

        Private Sub ApplyPagination()
            If _filteredProjects Is Nothing Then Return

            Dim totalPages As Integer = Math.Max(1, Math.Ceiling(_filteredProjects.Count / _pageSize))
            If _currentPage > totalPages Then _currentPage = totalPages
            If _currentPage < 1 Then _currentPage = 1

            Dim paged = _filteredProjects.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList()
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
                factory.SetBinding(Border.BackgroundProperty, New Binding("Background") With {.RelativeSource = New RelativeSource(RelativeSourceMode.TemplatedParent)})
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

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _allProjects Is Nothing Then Return

            Dim keyword = txtSearch.Text.ToLower().Trim()

            If String.IsNullOrEmpty(keyword) Then
                _filteredProjects = _allProjects
            Else
                _filteredProjects = _allProjects.Where(Function(p)
                                                           Return (p.ProjectName?.ToLower().Contains(keyword)) OrElse
                                                                  (p.Status?.ToLower().Contains(keyword)) OrElse
                                                                  (p.Customer?.ToLower().Contains(keyword)) OrElse
                                                                  (p.AssignedToName?.ToLower().Contains(keyword)) OrElse
                                                                  (p.ProjectID.ToString().Contains(keyword))
                                                       End Function).ToList()
            End If

            _currentPage = 1
            ApplyPagination()
            UpdateStatusCounts()
        End Sub

        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim projectID = btn.Tag?.ToString()
            Dim project = _allProjects?.FirstOrDefault(Function(p) p.ProjectID.ToString() = projectID)

            If project IsNot Nothing Then
                CacheProjectID = project.ProjectID.ToString()
                CacheProjectName = project.ProjectName
                CacheProjectStatus = project.Status
                CacheProjectCustomer = project.Customer
                CacheProjectBudget = project.Budget.ToString()
                CacheProjectStartDate = project.StartDate
                CacheProjectDueDate = project.DueDate
                CacheProjectAssignedTo = project.AssignedTo
                ViewLoader.DynamicView.NavigateToView("editproject", Me)
            End If
        End Sub

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
                LoadData()
                UpdateStatusCounts()
            Catch ex As Exception
                MessageBox.Show("Error deleting project: " & ex.Message)
            End Try
        End Sub
        ' --- EXCEL EXPORT FUNCTIONALITY ---

        Private Sub BtnExportExcel_Click(sender As Object, e As RoutedEventArgs)
            ' 1. Check if there is data to export
            If _filteredProjects Is Nothing OrElse _filteredProjects.Count = 0 Then
                MessageBox.Show("There are no projects to export.", "Empty Data", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            ' 2. Open a Save File Dialog so the user can choose where to save the file
            Dim sfd As New Microsoft.Win32.SaveFileDialog()
            sfd.Filter = "Excel CSV File (*.csv)|*.csv"
            sfd.FileName = "Project_Report_" & DateTime.Now.ToString("yyyyMMdd") & ".csv"

            If sfd.ShowDialog() = True Then
                Try
                    ' 3. Build the Excel-compatible text using StringBuilder
                    Dim sb As New System.Text.StringBuilder()

                    ' Add the Header Row
                    sb.AppendLine("Project ID,Project Name,Status,Customer,Budget,Start Date,Due Date,Assigned To")

                    ' Add the Data Rows
                    For Each p In _filteredProjects
                        Dim row As New List(Of String) From {
                            EscapeCsv(p.ProjectID.ToString()),
                            EscapeCsv(p.ProjectName),
                            EscapeCsv(p.Status),
                            EscapeCsv(p.Customer),
                            EscapeCsv(p.Budget.ToString()),
                            EscapeCsv(If(p.StartDate IsNot Nothing, p.StartDate.ToString(), "")),
                            EscapeCsv(If(p.DueDate IsNot Nothing, p.DueDate.ToString(), "")),
                            EscapeCsv(p.AssignedToName)
                        }
                        ' Join columns with commas
                        sb.AppendLine(String.Join(",", row))
                    Next

                    ' 4. Save the file
                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString())
                    MessageBox.Show("Exported successfully! You can now open this file in Excel.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                Catch ex As Exception
                    MessageBox.Show("Error exporting data: " & ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        ' Helper function to handle commas or quotes inside your data (e.g., if a Project Name has a comma in it)
        Private Function EscapeCsv(value As String) As String
            If String.IsNullOrWhiteSpace(value) Then Return """"""
            ' Wrap value in quotes and double-up any existing quotes for Excel compatibility
            Return """" & value.Replace("""", """""") & """"
        End Function

    End Class
End Namespace