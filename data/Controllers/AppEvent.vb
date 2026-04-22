Imports System.Windows.Media

Public Class AppEvent
    Public Property EventID As Integer
    Public Property Title As String
    Public Property EventDate As DateTime
    Public Property Category As String
    Public Property EventColor As SolidColorBrush

    Public Shared Function GetColorForCategory(category As String) As SolidColorBrush
        Dim brushConverter As New BrushConverter()
        Select Case category.ToLower()
            Case "meeting"
                Return CType(brushConverter.ConvertFrom("#DDD6FE"), SolidColorBrush)
            Case "deadline"
                Return CType(brushConverter.ConvertFrom("#FECACA"), SolidColorBrush)
            Case "salary payout"
                Return CType(brushConverter.ConvertFrom("#BBF7D0"), SolidColorBrush)
            Case "holiday"
                Return CType(brushConverter.ConvertFrom("#BFDBFE"), SolidColorBrush)
            Case Else
                Return CType(brushConverter.ConvertFrom("#D3D3D3"), SolidColorBrush)
        End Select
    End Function
End Class