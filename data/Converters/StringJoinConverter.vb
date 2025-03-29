Imports System
Imports System.Globalization
Imports System.Windows.Data

Namespace DPC.Data.Converters
    Public Class StringJoinConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object _
            Implements IValueConverter.Convert
            Dim brandNames = TryCast(value, List(Of String))
            Return If(brandNames IsNot Nothing, String.Join(", ", brandNames), String.Empty)
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object _
            Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
