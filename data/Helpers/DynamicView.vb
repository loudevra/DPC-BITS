Imports DPC.DPC.Views

Namespace DPC.Data.Helpers
    Public Class DynamicView
        Public Shared Function Load(viewName As String) As UserControl
            Select Case viewName.ToLower()
                Case "dashboard"
                    Return New Dashboard.Dashboard() ' This is now a UserControl

                Case Else
                    ' Return a placeholder UserControl with error text
                    Dim errorContent As New TextBlock With {
                        .Text = $"View not found: {viewName}",
                        .FontSize = 20,
                        .HorizontalAlignment = HorizontalAlignment.Center,
                        .VerticalAlignment = VerticalAlignment.Center
                    }

                    Return New UserControl With {.Content = errorContent}
            End Select
        End Function
    End Class
End Namespace
