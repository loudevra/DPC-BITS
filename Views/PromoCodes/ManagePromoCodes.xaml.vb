Imports System.Data
Imports System.Windows.Controls
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.PromoCodes
    Partial Public Class ManagePromoCodes
        Inherits UserControl

        ' Stats counters for the dashboard
        Private activeCount As Integer = 0
        Private usedCount As Integer = 0
        Private expiredCount As Integer = 0
        Private totalCount As Integer = 0

        Public Sub New()
            InitializeComponent()
            LoadPromoCodes()
            LoadPromoCodeStats()
        End Sub

        Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
            ' Open the Add Promo Code window or navigate to that view

        End Sub

        Private Sub LoadPromoCodes()
            Try
                ' Use the PromoCodeController to fetch data
                Dim dt As DataTable = PromoCodeController.FetchPromoCodes()

                If dt IsNot Nothing Then
                    'dgPromoCodes.ItemsSource = dt.DefaultView
                End If
            Catch ex As Exception
                MessageBox.Show($"Error loading promo codes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub LoadPromoCodeStats()
            Try
                ' Get stats using the PromoCodeController
                Dim stats As Dictionary(Of String, Integer) = PromoCodeController.GetPromoCodeStats()

                ' Update UI elements with stats
                If stats IsNot Nothing Then
                    activeCount = stats("Active")
                    usedCount = stats("Used")
                    expiredCount = stats("Expired")
                    totalCount = stats("Total")

                    ' Update the TextBlocks in the UI
                    'txtActiveCount.Text = activeCount.ToString()
                    'txtUsedCount.Text = usedCount.ToString()
                    'txtExpiredCount.Text = expiredCount.ToString()
                    'txtTotalCount.Text = totalCount.ToString()
                End If
            Catch ex As Exception
                MessageBox.Show($"Error loading promo code statistics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub BtnView_Click(sender As Object, e As RoutedEventArgs)
            ' Get the selected promo code
            Dim row As DataRowView = TryCast(CType(sender, Button).DataContext, DataRowView)
            If row IsNot Nothing Then
                Dim id As Integer = Convert.ToInt32(row("ID"))
                ViewPromoCode(id)
            End If
        End Sub

        Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
            ' Get the selected promo code
            Dim row As DataRowView = TryCast(CType(sender, Button).DataContext, DataRowView)
            If row IsNot Nothing Then
                Dim id As Integer = Convert.ToInt32(row("ID"))
                EditPromoCode(id)
            End If
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            ' Get the selected promo code
            Dim row As DataRowView = TryCast(CType(sender, Button).DataContext, DataRowView)
            If row IsNot Nothing Then
                Dim id As Integer = Convert.ToInt32(row("ID"))
                Dim code As String = row("Code").ToString()

                ' Confirm deletion
                Dim result As MessageBoxResult = MessageBox.Show($"Are you sure you want to delete promo code '{code}'?",
                                                                "Confirm Deletion",
                                                                MessageBoxButton.YesNo,
                                                                MessageBoxImage.Question)

                If result = MessageBoxResult.Yes Then
                    ' Delete the promo code
                    If PromoCodeController.DeletePromoCode(id) Then
                        LoadPromoCodes()
                        LoadPromoCodeStats()
                        MessageBox.Show("Promo code deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    End If
                End If
            End If
        End Sub

        Private Sub ViewPromoCode(id As Integer)
            ' Get the promo code details

        End Sub

        Private Sub EditPromoCode(id As Integer)

        End Sub

        Private Sub TxtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Filter the DataGrid based on search text
            Dim searchText As String = txtSearch.Text.ToLower()

            'If dgPromoCodes.ItemsSource IsNot Nothing Then
            '    Dim view As DataView = TryCast(dgPromoCodes.ItemsSource, DataView)

            '    If view IsNot Nothing Then
            '        If String.IsNullOrWhiteSpace(searchText) Then
            '            view.RowFilter = ""
            '        Else
            '            ' Filter by Code (add more columns as needed)
            '            view.RowFilter = $"Code LIKE '%{searchText}%' OR Account LIKE '%{searchText}%' OR Note LIKE '%{searchText}%'"
            '        End If
            '    End If
            'End If
        End Sub

        Private Sub BtnExport_Click(sender As Object, e As RoutedEventArgs)
            ' Export functionality can be implemented here
            ' This could export to CSV or Excel
            MessageBox.Show("Export functionality will be implemented here.", "Export", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub CboPageSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Implementation for changing the number of items shown per page
            ' This would need pagination logic
        End Sub
    End Class
End Namespace