Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports DPC.DPC.Data.Helpers
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Project
    Public Class ManageProject
        Inherits UserControl

        Private _projectID As String
        Private _allProjects As List(Of DPC.Data.Model.Project)

        Public Sub New()
            InitializeComponent()
            AddHandler txtSearch.TextChanged, AddressOf TxtSearch_TextChanged
            LoadData()
        End Sub

        ' ── Load Data ─────────────────────────────────────────────────
        Public Sub LoadData()
            Try
                _allProjects = DPC.Data.Controllers.ProjectController.GetProjects()
                If _allProjects Is Nothing Then
                    _allProjects = New List(Of DPC.Data.Model.Project)()
                End If
                ProjectDataGrid.ItemsSource = New ObservableCollection(Of DPC.Data.Model.Project)(_allProjects)
            Catch ex As Exception
                MessageBox.Show("Error retrieving project data: " & ex.Message, "Data Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' ── Search ────────────────────────────────────────────────────
        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _allProjects Is Nothing Then Return

            Dim keyword = txtSearch.Text.ToLower().Trim()

            If String.IsNullOrEmpty(keyword) Then
                ProjectDataGrid.ItemsSource = New ObservableCollection(Of DPC.Data.Model.Project)(_allProjects)
                Return
            End If

            Dim filtered = _allProjects.Where(Function(p)
                                                  Return (p.ProjectName?.ToLower().Contains(keyword)) OrElse
                                                  (p.Status?.ToLower().Contains(keyword)) OrElse
                                                  (p.Customer?.ToLower().Contains(keyword)) OrElse
                                                  (p.AssignedToName?.ToLower().Contains(keyword)) OrElse
                                                  (p.ProjectID.ToString().Contains(keyword))
                                              End Function).ToList()

            ProjectDataGrid.ItemsSource = New ObservableCollection(Of DPC.Data.Model.Project)(filtered)
        End Sub

        ' ── Edit Button ───────────────────────────────────────────────
        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim projectID = btn.Tag?.ToString()
            Dim project = _allProjects?.FirstOrDefault(Function(p) p.ProjectID.ToString() = projectID)

            If project IsNot Nothing Then
                ' Populate cache for EditProject to read on load
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

        ' ── Delete Button ─────────────────────────────────────────────
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
            Catch ex As Exception
                MessageBox.Show("Error deleting project: " & ex.Message)
            End Try
        End Sub

    End Class
End Namespace