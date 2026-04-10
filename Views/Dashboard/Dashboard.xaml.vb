Imports DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Dashboard
    Partial Public Class Dashboard
        Inherits UserControl

        Private _selectedYear As Integer = DateTime.Now.Year
        Private _earliestYear As Integer = DateTime.Now.Year

        Public Sub New()
            InitializeComponent()
            AddHandler Loaded, AddressOf Dashboard_Loaded
        End Sub

        Private Sub Dashboard_Loaded(sender As Object, e As RoutedEventArgs)
            _earliestYear = BillingController.GetEarliestSalesYear()
            _selectedYear = DateTime.Now.Year
            TxtSelectedYear.Text = _selectedYear.ToString()
            AddHandler TxtSelectedYear.KeyDown, AddressOf TxtSelectedYear_KeyDown
            LoadSalesData()
        End Sub

        Private Sub LoadSalesData()
            Try
                Dim todaySales As Decimal = BillingController.GetTodayTotalSales()
                Dim monthSales As Decimal = BillingController.GetThisMonthTotalSales()
                Dim yearlySales As Decimal = BillingController.GetSalesByYear(_selectedYear)

                TxtTodaySales.Text = "+" & todaySales.ToString("N2")
                TxtMonthSales.Text = "+" & monthSales.ToString("N2")
                TxtYearlySales.Text = "+" & yearlySales.ToString("N2")

                ' Disable ▶ if already at current year
                BtnNextYear.IsEnabled = (_selectedYear < DateTime.Now.Year)
                ' Disable ◀ if at or before earliest record (with 5yr buffer)
                BtnPrevYear.IsEnabled = (_selectedYear > _earliestYear - 5)
            Catch ex As Exception
                Debug.WriteLine("LoadSalesData error: " & ex.Message)
            End Try
        End Sub

        Private Sub BtnPrevYear_Click(sender As Object, e As RoutedEventArgs)
            _selectedYear -= 1
            TxtSelectedYear.Text = _selectedYear.ToString()
            LoadSalesData()
        End Sub

        Private Sub BtnNextYear_Click(sender As Object, e As RoutedEventArgs)
            _selectedYear += 1
            TxtSelectedYear.Text = _selectedYear.ToString()
            LoadSalesData()
        End Sub

        Private Sub TxtSelectedYear_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Return Then
                Dim parsed As Integer
                If Integer.TryParse(TxtSelectedYear.Text, parsed) AndAlso parsed > 1900 AndAlso parsed <= DateTime.Now.Year + 10 Then
                    _selectedYear = parsed
                    LoadSalesData()
                Else
                    ' Reset to last valid year if bad input
                    TxtSelectedYear.Text = _selectedYear.ToString()
                End If
            End If
        End Sub

    End Class
End Namespace