Namespace DPC.Data.Controllers
    Public Class SessionManager
        Private Shared _accessToken As String
        Private Shared _refreshToken As String

        ' Store tokens
        Public Shared Sub SetSessionTokens(accessToken As String, refreshToken As String)
            _accessToken = accessToken
            _refreshToken = refreshToken
        End Sub

        ' Retrieve tokens
        Public Shared Function GetAccessToken() As String
            Return _accessToken
        End Function

        Public Shared Function GetRefreshToken() As String
            Return _refreshToken
        End Function
    End Class
End Namespace
