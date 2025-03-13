Imports System
Imports System.Globalization
Imports System.Windows.Data

Namespace DPC.Converters
    Public Class RoleToBooleanConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If value IsNot Nothing AndAlso parameter IsNot Nothing Then
                Return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase)
            End If
            Return False
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            If value IsNot Nothing AndAlso CBool(value) Then
                Return parameter.ToString()
            End If
            Return Binding.DoNothing
        End Function
    End Class
End Namespace
