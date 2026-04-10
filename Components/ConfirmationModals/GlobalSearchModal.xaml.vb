Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports DPC.DPC.Data.Controllers

Namespace DPC.DPC.Components.ConfirmationModals
    Public Class GlobalSearchModal
        Public Event NavigateToView(viewName As String, itemId As String)

        Private SearchResults As Collections.ObjectModel.ObservableCollection(Of GlobalSearchController.SearchResultItem)

        Public Sub New(searchQuery As String)
            InitializeComponent()

            ' Perform search
            SearchQueryText.Text = $"Searching for: ""{searchQuery}"""
            SearchResults = GlobalSearchController.GlobalSearch(searchQuery)

            ' Bind results
            SearchResultsList.ItemsSource = SearchResults

            ' Update UI based on results
            If SearchResults.Count > 0 Then
                NoResultsPanel.Visibility = Visibility.Collapsed
                ResultCountText.Text = $"{SearchResults.Count} result(s) found"
            Else
                NoResultsPanel.Visibility = Visibility.Visible
                ResultCountText.Text = "0 results found"
            End If
        End Sub

        Private Sub CloseModal(sender As Object, e As RoutedEventArgs)
            Me.DialogResult = False
            Me.Close()
        End Sub

        Private Sub Result_MouseEnter(sender As Object, e As MouseEventArgs)
            Dim border As Border = TryCast(sender, Border)
            If border IsNot Nothing Then
                border.Background = New SolidColorBrush(Color.FromRgb(245, 245, 245))
            End If
        End Sub

        Private Sub Result_MouseLeave(sender As Object, e As MouseEventArgs)
            Dim border As Border = TryCast(sender, Border)
            If border IsNot Nothing Then
                border.Background = Brushes.White
            End If
        End Sub

        Private Sub Result_Click(sender As Object, e As MouseButtonEventArgs)
            Dim border As Border = TryCast(sender, Border)
            If border IsNot Nothing Then
                Dim item As GlobalSearchController.SearchResultItem = TryCast(border.DataContext, GlobalSearchController.SearchResultItem)
                If item IsNot Nothing Then
                    ' Raise event to navigate
                    RaiseEvent NavigateToView(item.NavigationTarget, item.ID)
                    Me.Close()
                End If
            End If
        End Sub
    End Class
End Namespace