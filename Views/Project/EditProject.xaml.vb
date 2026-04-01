Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports MySql.Data.MySqlClient

Public Class EditProject
    Inherits UserControl

    Public Sub New()
        InitializeComponent()

        ' Initialize project statuses using the explicit WPF Media Color namespace to avoid ambiguity
        Dim statuses As New List(Of StatusItem) From {
            New StatusItem With {.Label = "Waiting", .Color = New SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 209, 142))},
            New StatusItem With {.Label = "Processing", .Color = New SolidColorBrush(System.Windows.Media.Color.FromRgb(134, 188, 213))},
            New StatusItem With {.Label = "Solved", .Color = New SolidColorBrush(System.Windows.Media.Color.FromRgb(137, 172, 116))},
            New StatusItem With {.Label = "Cancelled", .Color = New SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 94, 94))}
        }
        cmbStatus.ItemsSource = statuses

        AddHandler Me.Loaded, AddressOf EditProject_Loaded
    End Sub

    Private Sub EditProject_Loaded(sender As Object, e As RoutedEventArgs)
        Try
            ' 1. Load the Name directly using the imported Helpers variable
            txtName.Text = CacheProjectName

            ' 2. Load the Status Dropdown
            For Each item As StatusItem In cmbStatus.Items
                If item.Label = CacheProjectStatus Then
                    cmbStatus.SelectedItem = item
                    Exit For
                End If
            Next

            ' 3. Safely Load Start Date
            Dim sDate As DateTime
            If DateTime.TryParse(CacheProjectStartDate, sDate) Then
                StartDatePicker.SelectedDate = sDate
            End If

            ' 4. Safely Load Due Date
            Dim dDate As DateTime
            If DateTime.TryParse(CacheProjectDueDate, dDate) Then
                DueDatePicker.SelectedDate = dDate
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading project details: " & ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub BtnSaveChanges_Click(sender As Object, e As RoutedEventArgs)
        ' 1. Validation
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            MessageBox.Show("Project Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        ' 2. Database Update 
        Try
            ' Ensure GetDatabaseConnection() is Public Shared in your SplashScreen class
            Dim connStr As String = DPC.SplashScreen.GetDatabaseConnection().ConnectionString
            Using conn As New MySqlConnection(connStr)
                conn.Open()

                ' Include dates in the update query
                Dim query As String = "UPDATE project SET ProjectName=@name, Status=@status, StartDate=@start, DueDate=@due WHERE projectID=@id"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@name", txtName.Text)

                    Dim selectedStatus = TryCast(cmbStatus.SelectedItem, StatusItem)
                    cmd.Parameters.AddWithValue("@status", If(selectedStatus IsNot Nothing, selectedStatus.Label, "Waiting"))

                    cmd.Parameters.AddWithValue("@start", If(StartDatePicker.SelectedDate, DBNull.Value))
                    cmd.Parameters.AddWithValue("@due", If(DueDatePicker.SelectedDate, DBNull.Value))

                    cmd.Parameters.AddWithValue("@id", CacheProjectID)

                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Project updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            ' 3. Navigate back to the Manage Tasks view
            ViewLoader.DynamicView.NavigateToView("manageproject", Me)

        Catch ex As Exception
            MessageBox.Show("Update failed: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    ' Date and Text Handlers with Past-Date Restriction
    Private Sub StartDateButton_Click(sender As Object, e As RoutedEventArgs)
        Dim minDate As DateTime = DateTime.Today
        ' If the project already has an older date saved, use that as the minimum
        If StartDatePicker.SelectedDate.HasValue AndAlso StartDatePicker.SelectedDate.Value < DateTime.Today Then
            minDate = StartDatePicker.SelectedDate.Value
        End If

        StartDatePicker.DisplayDateStart = minDate
        StartDatePicker.IsDropDownOpen = True
    End Sub

    Private Sub DueDateButton_Click(sender As Object, e As RoutedEventArgs)
        Dim minDate As DateTime = DateTime.Today
        ' If the project already has an older date saved, use that as the minimum
        If DueDatePicker.SelectedDate.HasValue AndAlso DueDatePicker.SelectedDate.Value < DateTime.Today Then
            minDate = DueDatePicker.SelectedDate.Value
        End If

        DueDatePicker.DisplayDateStart = minDate
        DueDatePicker.IsDropDownOpen = True
    End Sub

    ' Fixed Event Signature for TextChanged
    Private Sub TxtToUpper_TextChanged(sender As Object, e As System.Windows.Controls.TextChangedEventArgs)
        Dim tb = TryCast(sender, TextBox)
        If tb IsNot Nothing Then
            tb.Text = tb.Text.ToUpper()
            tb.SelectionStart = tb.Text.Length
        End If
    End Sub

    Private Sub StartDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
    End Sub

    Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
    End Sub
End Class