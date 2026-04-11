Imports System.Windows
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.Sales.Quotes
    Public Class BillingStatement
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BillingStatement_Loaded(sender As Object, e As RoutedEventArgs)
            Try
                ' Assign directly without a "With" block
                txtClientName.Text = CostEstimateDetails.CEClientName
                txtClientAddress.Text = CostEstimateDetails.CEAddress
                txtClientCityRegion.Text = CostEstimateDetails.CECity & If(Not String.IsNullOrEmpty(CostEstimateDetails.CERegion), ", " & CostEstimateDetails.CERegion, "")
                txtClientPhone.Text = CostEstimateDetails.CEPhone
                txtClientEmail.Text = CostEstimateDetails.CEEmail
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Error loading billing info: " & ex.Message)
            End Try
        End Sub
    End Class
End Namespace