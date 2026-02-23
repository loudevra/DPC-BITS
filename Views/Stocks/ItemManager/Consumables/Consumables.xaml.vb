Imports System.Collections.ObjectModel
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers.Stocks
Imports DPC.DPC.Views.ItemManager.Consumables
Imports DPC.DPC.Views.Warehouse
Imports DPC.Views.ItemManager.Consumables

Namespace DPC.Views.Stocks.ItemManager.Consumables
    Public Class Consumables
        ' Timer for search debounce to prevent excessive database queries
        Private _typingTimer As DispatcherTimer
        Private _isInitialized As Boolean = False

        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()

            ' Set up the search timer with a 250ms delay
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(250)
            }
            AddHandler _typingTimer.Tick, AddressOf OnTypingTimerTick

            ' Load data after the control is fully loaded
            AddHandler Me.Loaded, AddressOf UserControl_Loaded
        End Sub

        Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
            If Not _isInitialized Then
                LoadConsumables()
                _isInitialized = True
            End If
        End Sub

        ' Loads the standard list of consumables
        Private Sub LoadConsumables()
            Try
                Dim limit As Integer = 10
                If cboPageSize.SelectedItem IsNot Nothing Then
                    Dim selectedItem = TryCast(cboPageSize.SelectedItem, ComboBoxItem)
                    If selectedItem IsNot Nothing Then
                        limit = Convert.ToInt32(selectedItem.Content)
                    End If
                End If

                ' Controller should return ObservableCollection(Of ConsumableModels)
                dataGrid.ItemsSource = PullOutFormController.GetConsumables(limit)
            Catch ex As Exception
                MessageBox.Show($"Error loading consumables: {ex.Message}")
            End Try
        End Sub

        ' Handles the "Add New" button click
        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            Dim parentWindow As Window = Window.GetWindow(Me)
            Dim addConsumables As New AddConsumables()

            If parentWindow IsNot Nothing Then
                addConsumables.Owner = parentWindow
            End If

            addConsumables.ShowDialog()

            ' Refresh the grid after adding a new item
            dataGrid.ItemsSource = Nothing
            LoadConsumables()
        End Sub

        ' --- SEARCH LOGIC ---

        ' Triggers every time the search text changes
        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            _typingTimer.Stop()
            _typingTimer.Start()
        End Sub

        ' Executes the search after the user pauses typing
        Private Sub OnTypingTimerTick(sender As Object, e As EventArgs)
            _typingTimer.Stop()
            PerformSearch()
        End Sub

        Private Sub PerformSearch()
            Try
                Dim searchTextValue As String = txtSearch.Text.Trim()

                ' Get current limit from ComboBox
                Dim limit As Integer = 10
                If cboPageSize.SelectedItem IsNot Nothing Then
                    limit = Convert.ToInt32(CType(cboPageSize.SelectedItem, ComboBoxItem).Content)
                End If

                Dim results As ObservableCollection(Of ConsumableModels)

                If String.IsNullOrWhiteSpace(searchTextValue) Then
                    results = PullOutFormController.GetConsumables(limit)
                Else
                    ' Calls the search logic in the controller
                    results = PullOutFormController.SearchConsumables(searchTextValue, limit)
                End If

                dataGrid.ItemsSource = results
            Catch ex As Exception
                MessageBox.Show($"Error searching: {ex.Message}")
            End Try
        End Sub

        ' --- ROW ACTIONS (EDIT & DELETE) ---

        ' Logic for editing a specific row item
        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                ' Retrieve the specific row data from the button's Tag
                Dim consumableModel = TryCast(btn.Tag, ConsumableModels)

                If consumableModel IsNot Nothing Then
                    Dim parentWindow As Window = Window.GetWindow(Me)
                    Dim editForm As New EditConsumables(consumableModel)

                    If parentWindow IsNot Nothing Then
                        editForm.Owner = parentWindow
                    End If

                    ' Refresh the grid if the edit was saved successfully
                    If editForm.ShowDialog() = True Then
                        dataGrid.ItemsSource = Nothing
                        LoadConsumables()
                    End If
                End If
            End If
        End Sub

        ' Logic for deleting a specific row item
        Private Async Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim consumableModel = TryCast(btn.Tag, ConsumableModels)

                If consumableModel IsNot Nothing Then
                    ' Confirmation dialog before deletion
                    Dim result = MessageBox.Show($"Are you sure you want to delete {consumableModel.ProductName}?",
                                                 "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)

                    If result = MessageBoxResult.Yes Then
                        ' Call async delete via the controller
                        If Await PullOutFormController.DeleteConsumableAsync(consumableModel.ProductID) Then
                            dataGrid.ItemsSource = Nothing
                            LoadConsumables()
                            MessageBox.Show("Consumable deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                        End If
                    End If
                End If
            End If
        End Sub

        ' Handles pagination limit changes
        Private Sub cboPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboPageSize.SelectionChanged
            If _isInitialized Then
                LoadConsumables()
            End If
        End Sub

    End Class

    ' Data model representing each consumable row
    Public Class ConsumableModels
        Public Property ProductID As String
        Public Property ProductName As String
        Public Property WarehouseName As String
        Public Property Stock As Integer
    End Class
End Namespace