Namespace DPC.Data.Helpers
    Public Class FindParentGrid
        ' Helper function to find the parent grid
        Public Shared Function FindParentGrid(element As DependencyObject) As Grid
            While element IsNot Nothing AndAlso Not (TypeOf element Is Grid)
                element = VisualTreeHelper.GetParent(element)
            End While
            Return TryCast(element, Grid)
        End Function
    End Class
End Namespace



