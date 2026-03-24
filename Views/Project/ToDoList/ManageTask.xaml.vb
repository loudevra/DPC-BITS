Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Linq
Imports System.IO
Imports Microsoft.Win32
Imports DPC.DPC.Data.Helpers '<-- Added missing import here!

Public Class ManageTask
    Public Sub New()
        InitializeComponent()

        ' Bind the DataGrid directly to our global in-memory list
        TasksDataGrid.ItemsSource = GlobalTaskStore.TaskList

        ' Recalculate the stats whenever you navigate to this page
        AddHandler Me.Loaded, AddressOf ManageTask_Loaded
    End Sub

    Private Sub ManageTask_Loaded(sender As Object, e As RoutedEventArgs)
        UpdateStats()
    End Sub

    Public Sub UpdateStats()
        Dim dueCount As Integer = 0
        Dim progCount As Integer = 0
        Dim doneCount As Integer = 0

        ' Loop through the shared list to count statuses
        For Each t In GlobalTaskStore.TaskList
            If t.Status = "Due" Then dueCount += 1
            If t.Status = "Progress" Then progCount += 1
            If t.Status = "Done" Then doneCount += 1
        Next

        ' Update the statistics cards at the top of the UI
        tbTotal.Text = GlobalTaskStore.TaskList.Count.ToString()
        tbDue.Text = dueCount.ToString()
        tbProgress.Text = progCount.ToString()
        tbDone.Text = doneCount.ToString()
    End Sub

    ' =========================================================
    ' 1. SEARCH FUNCTIONALITY
    ' =========================================================
    Private Sub txtSearch_TextChanged(sender As Object, e As Controls.TextChangedEventArgs)
        Dim searchText As String = txtSearch.Text.ToLower().Trim()

        ' Hide or show the "Search" placeholder text
        If txtSearchPlaceholder IsNot Nothing Then
            txtSearchPlaceholder.Visibility = If(searchText.Length > 0, Visibility.Hidden, Visibility.Visible)
        End If

        If String.IsNullOrWhiteSpace(searchText) Then
            ' Reset to full list when search is empty
            TasksDataGrid.ItemsSource = GlobalTaskStore.TaskList
        Else
            ' Filter the list based on Task Name, ID, or Status
            Dim filteredList = GlobalTaskStore.TaskList.Where(Function(t) _
                t.TaskID.ToString().Contains(searchText) OrElse
                (t.Task IsNot Nothing AndAlso t.Task.ToLower().Contains(searchText)) OrElse
                (t.Status IsNot Nothing AndAlso t.Status.ToLower().Contains(searchText))
            ).ToList()

            TasksDataGrid.ItemsSource = filteredList
        End If
    End Sub

    ' =========================================================
    ' 2. EXCEL (CSV) EXPORT FUNCTIONALITY
    ' =========================================================
    Private Sub BtnExportExcel_Click(sender As Object, e As RoutedEventArgs)
        Dim itemsToExport = TryCast(TasksDataGrid.ItemsSource, IEnumerable(Of TaskModel))

        If itemsToExport Is Nothing OrElse itemsToExport.Count() = 0 Then
            MessageBox.Show("No data available to export.", "Export Data", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Dim saveFileDialog As New SaveFileDialog()
        saveFileDialog.Filter = "CSV Excel File (*.csv)|*.csv"
        saveFileDialog.FileName = "TasksExport_" & DateTime.Now.ToString("yyyyMMdd") & ".csv"
        saveFileDialog.Title = "Export Tasks to Excel"

        If saveFileDialog.ShowDialog() = True Then
            Try
                Using writer As New StreamWriter(saveFileDialog.FileName)
                    writer.WriteLine("Task ID,Task Name,Start Date,Due Date,Status")

                    For Each item In itemsToExport
                        Dim id As String = item.TaskID.ToString()
                        Dim tName As String = """" & If(item.Task, "").Replace("""", """""") & """"
                        Dim sDate As String = """" & If(item.Start, "") & """"
                        Dim dDate As String = """" & If(item.DueDate, "") & """"
                        Dim status As String = """" & If(item.Status, "") & """"

                        writer.WriteLine($"{id},{tName},{sDate},{dDate},{status}")
                    Next
                End Using

                MessageBox.Show("Data successfully exported to Excel!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information)
            Catch ex As Exception
                MessageBox.Show("Error exporting data: " & ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If
    End Sub

    ' =========================================================
    ' 3. EDIT AND DELETE FUNCTIONALITY
    ' =========================================================
    Private Sub BtnDeleteTask_Click(sender As Object, e As RoutedEventArgs)
        Dim btn As Button = TryCast(sender, Button)
        If btn IsNot Nothing Then
            Dim selectedTask As TaskModel = TryCast(btn.DataContext, TaskModel)

            If selectedTask IsNot Nothing Then
                Dim result = MessageBox.Show($"Are you sure you want to delete task '{selectedTask.Task}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)

                If result = MessageBoxResult.Yes Then
                    GlobalTaskStore.TaskList.Remove(selectedTask)
                    UpdateStats()
                    txtSearch_TextChanged(Nothing, Nothing)
                End If
            End If
        End If
    End Sub

    Private Sub BtnEditTask_Click(sender As Object, e As RoutedEventArgs)
        Dim btn As Button = TryCast(sender, Button)
        If btn IsNot Nothing Then
            Dim selectedTask As TaskModel = TryCast(btn.DataContext, TaskModel)

            If selectedTask IsNot Nothing Then
                ' Store this task in memory so the Edit Task view can read it
                GlobalTaskStore.TaskToEdit = selectedTask

                ' Navigates using the shortened, correct path
                ViewLoader.DynamicView.NavigateToView("edittask", Me)
            End If
        End If
    End Sub

End Class

' =========================================================
' IN-MEMORY DATA STORE (Accessible globally to all pages)
' =========================================================
Public Class GlobalTaskStore
    Public Shared TaskList As New ObservableCollection(Of TaskModel)

    ' Temporarily stores the task that the user wants to edit
    Public Shared TaskToEdit As TaskModel = Nothing
End Class

Public Class TaskModel
    Public Property TaskID As Integer
    Public Property Task As String
    Public Property DueDate As String
    Public Property Start As String
    Public Property Status As String
End Class