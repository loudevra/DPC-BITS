Imports System.Globalization
Imports System
Imports System.Windows.Data

Namespace DPC.Data.Converters.ValueConverter
    Public Class HalfWidthConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim fullWidth As Double = CType(value, Double)
            Return fullWidth / 2 ' Return half of the navbar width
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return DependencyProperty.UnsetValue
        End Function
    End Class
End Namespace
