Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports DPC.DPC.Data.Helpers.ViewLoader

Namespace DPC.Views.Misc.OverTime

    ' 1. Data Model
    Public Class OvertimeRequestModel
        Public Property OvertimeID As String
        Public Property EmployeeName As String
        Public Property JobTitle As String
        Public Property Department As String
        Public Property TotalHours As String
        Public Property RequestDate As String
        Public Property Status As String

        ' --- ADD THESE MISSING FIELDS ---
        Public Property EmployeeID As String
        Public Property Supervisor As String
        Public Property StartTime As String
        Public Property EndTime As String
        Public Property Reason As String
        Public Property Remarks As String
        Public Property RequestedBy As String
    End Class

    Public Class ManageTimeoutRequests
        Inherits UserControl

        ' 2. Shared Master List
        ' It MUST have the word "Shared" right here!
        Public Shared GlobalOvertimeList As New ObservableCollection(Of OvertimeRequestModel)()

        Public Sub New()
            InitializeComponent()

            ' Add one default row so the grid isn't totally empty
            If GlobalOvertimeList.Count = 0 Then
                GlobalOvertimeList.Add(New OvertimeRequestModel With {
                    .OvertimeID = "OT-001",
                    .EmployeeName = "John Doe",
                    .JobTitle = "Developer",
                    .Department = "IT",
                    .TotalHours = "4",
                    .RequestDate = "Oct 25, 2025",
                    .Status = "Pending"
                })
            End If

            ' 3. Bind the DataGrid to the Master List
            dataGrid.ItemsSource = GlobalOvertimeList
        End Sub

        ' ==========================================
        ' NAVIGATION & ACTIONS
        ' ==========================================
        Private Sub NavigateToNewRequest(sender As Object, e As RoutedEventArgs)
            DynamicView.NavigateToView("overtimerequestform", Me)
        End Sub

        Private Sub NavigateToPrintPreview(sender As Object, e As RoutedEventArgs)
            ' Placeholder for print preview navigation
        End Sub
        ' Inside ManageTimeoutRequests.xaml.vb
        ' Place this inside your ManageOvertimeRequests.xaml.vb file
        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            ' 1. Get the clicked row's data
            Dim selectedRecord As OvertimeRequestModel = CType(CType(sender, Button).DataContext, OvertimeRequestModel)

            If selectedRecord IsNot Nothing Then
                ' 2. Pass it to the shared Edit variable
                DPC.Views.Misc.OverTime.EditOverTime.TargetEditRecord = selectedRecord

                ' 3. Navigate to the Edit view
                DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("editovertime", Me)
            End If
        End Sub

        ' ==========================================
        ' DELETE BUTTON LOGIC
        ' ==========================================
        Private Sub DeleteRequest_Click(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim itemToDelete As OvertimeRequestModel = TryCast(btn.DataContext, OvertimeRequestModel)
            If itemToDelete IsNot Nothing Then
                ' Ask for confirmation
                Dim result = MessageBox.Show($"Are you sure you want to delete request {itemToDelete.OvertimeID}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)

                If result = MessageBoxResult.Yes Then
                    ' Delete it from the list (Instantly updates the UI)
                    GlobalOvertimeList.Remove(itemToDelete)
                End If
            End If
        End Sub
        ' ==========================================
        ' SEARCH FILTER LOGIC
        ' ==========================================
        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' 1. Grab the text from the search box and make it lowercase
            Dim query As String = txtSearch.Text.ToLower()

            ' 2. If the search box is empty, show the entire master list
            If String.IsNullOrWhiteSpace(query) Then
                dataGrid.ItemsSource = GlobalOvertimeList
            Else
                ' 3. Filter the list to find matching Names, IDs, Departments, or Statuses
                Dim filteredList = GlobalOvertimeList.Where(Function(item) _
                    (item.EmployeeName IsNot Nothing AndAlso item.EmployeeName.ToLower().Contains(query)) OrElse
                    (item.OvertimeID IsNot Nothing AndAlso item.OvertimeID.ToLower().Contains(query)) OrElse
                    (item.Department IsNot Nothing AndAlso item.Department.ToLower().Contains(query)) OrElse
                    (item.Status IsNot Nothing AndAlso item.Status.ToLower().Contains(query))
                ).ToList()

                ' 4. Instantly update the DataGrid with the filtered results!
                dataGrid.ItemsSource = filteredList
            End If
        End Sub
        ' Change the name from BtnEdit_Click to NavigateToEdit
        Private Sub NavigateToEdit(sender As Object, e As RoutedEventArgs)
            ' 1. Get the clicked row's data
            Dim selectedRecord As OvertimeRequestModel = CType(CType(sender, Button).DataContext, OvertimeRequestModel)

            If selectedRecord IsNot Nothing Then
                ' 2. Pass it to the shared Edit variable
                DPC.Views.Misc.OverTime.EditOverTime.TargetEditRecord = selectedRecord

                ' 3. Navigate to the Edit view
                DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("editovertime", Me)
            End If
        End Sub

        Private Sub dataGrid_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles dataGrid.SelectionChanged

        End Sub
    End Class
End Namespace