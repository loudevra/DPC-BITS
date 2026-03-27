Imports DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Dashboard
    Partial Public Class Dashboard
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            AddHandler Loaded, AddressOf Dashboard_Loaded
        End Sub

        Private Sub Dashboard_Loaded(sender As Object, e As RoutedEventArgs)
            LoadSalesData()
        End Sub

        Private Sub LoadSalesData()
            Try
                Dim todaySales As Decimal = BillingController.GetTodayTotalSales()
                Dim monthSales As Decimal = BillingController.GetThisMonthTotalSales()

                TxtTodaySales.Text = "+" & todaySales.ToString("N2")
                TxtMonthSales.Text = "+" & monthSales.ToString("N2")
            Catch ex As Exception
                Debug.WriteLine("Error loading sales data: " & ex.Message)
            End Try
        End Sub


    End Class
End Namespace