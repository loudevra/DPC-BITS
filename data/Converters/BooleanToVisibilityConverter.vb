Imports System.Windows.Controls
Imports System.Windows
Imports System.Windows.Input
Imports System.Globalization
Imports System.Windows.Data

Namespace DPC.Data.Converters
    Public Class BooleanToVisibilityConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim boolValue As Boolean = CBool(value)
            Return If(boolValue, Visibility.Visible, Visibility.Collapsed)
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Dim visibility As Visibility = DirectCast(value, Visibility)
            Return visibility = Visibility.Visible
        End Function
    End Class
End Namespace

