Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input

Public Class ManageStatementOfAccount
    ' Use the StatementModel to hold your Data
    Public Shared StatementList As New ObservableCollection(Of StatementModel)

    Public Sub New()
        InitializeComponent()

        ' Bind the DataGrid to your list. It will now start completely empty!
        dataGrid.ItemsSource = StatementList
    End Sub

    ' -------------------------------------------------------------------------
    ' EVENT HANDLERS
    ' -------------------------------------------------------------------------

    Private Sub BtnAddStatement_Click(sender As Object, e As RoutedEventArgs)
        ' Add your navigation logic here to go to the StatementOfAccountForm
        MessageBox.Show("Navigate to Add Statement Form...")
    End Sub

    Private Sub ExportToExcel(sender As Object, e As RoutedEventArgs)
        ' Excel export logic goes here
        MessageBox.Show("Exporting to Excel...")
    End Sub

    Private Sub FilterDateButton_Click(sender As Object, e As RoutedEventArgs)
        FilterDatePicker.IsDropDownOpen = True
    End Sub

    Private Sub FilterDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
        If FilterDatePicker.SelectedDate.HasValue Then
            FilterDateText.Text = FilterDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy")
            ClearDateButton.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub ClearDateButton_Click(sender As Object, e As RoutedEventArgs)
        FilterDatePicker.SelectedDate = Nothing
        FilterDateText.Text = "Select Date"
        ClearDateButton.Visibility = Visibility.Collapsed
    End Sub

    Private Sub SearchText_TextChanged(sender As Object, e As TextChangedEventArgs)
        ' Search/Filter logic for your DataGrid goes here
    End Sub

    Private Sub DataGrid_CellClick(sender As Object, e As MouseButtonEventArgs)
        ' Logic to show popup if a cell text is too long
    End Sub

    Private Sub OpenEditStatement(sender As Object, e As RoutedEventArgs)
        Dim btn = TryCast(sender, Button)
        If btn IsNot Nothing Then
            Dim record = TryCast(btn.DataContext, StatementModel)
            If record IsNot Nothing Then
                MessageBox.Show("Editing SOA: " & record.SOANo)
            End If
        End If
    End Sub
    ' -------------------------------------------------------------------------
    ' ACTION BUTTON EVENTS FOR CLIENTS
    ' -------------------------------------------------------------------------

    Private Sub OpenEditClient(sender As Object, e As RoutedEventArgs)
        Dim btn = TryCast(sender, Button)
        If btn IsNot Nothing Then
            ' Change "ClientModel" to whatever class name you use for your clients
            ' Dim record = TryCast(btn.DataContext, ClientModel)
            ' If record IsNot Nothing Then
            '     MessageBox.Show("Editing Client: " & record.ClientName)
            ' End If
            MessageBox.Show("Edit Client Clicked!")
        End If
    End Sub

    Private Sub DeleteClient(sender As Object, e As RoutedEventArgs)
        Dim result = MessageBox.Show("Are you sure you want to delete this client?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
        If result = MessageBoxResult.Yes Then
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                ' Change "ClientModel" to whatever class name you use for your clients
                ' Dim record = TryCast(btn.DataContext, ClientModel)
                ' If record IsNot Nothing Then
                '     YourClientObservableCollection.Remove(record)
                ' End If
                MessageBox.Show("Client Deleted!")
            End If
        End If
    End Sub

    Private Sub DeleteStatement(sender As Object, e As RoutedEventArgs)
        Dim result = MessageBox.Show("Are you sure you want to delete this statement?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
        If result = MessageBoxResult.Yes Then
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim record = TryCast(btn.DataContext, StatementModel)
                If record IsNot Nothing Then
                    StatementList.Remove(record)
                End If
            End If
        End If
    End Sub

End Class

' -------------------------------------------------------------------------
' DATA MODEL
' -------------------------------------------------------------------------
Public Class StatementModel
    Public Property SOANo As String
    Public Property ClientName As String
    Public Property StatementDate As String
    Public Property PONo As String
    Public Property ContractAmount As String
    Public Property NetAmountDue As String
End Class