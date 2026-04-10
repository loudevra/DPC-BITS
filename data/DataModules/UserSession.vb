Module UserSession
    ''' <summary>
    ''' Currently logged-in user's Employee ID
    ''' </summary>
    Public CacheOnEmployeeID As String = ""

    ''' <summary>
    ''' Currently logged-in user's full name
    ''' </summary>
    Public CacheOnLoggedInName As String = ""

    ''' <summary>
    ''' Currently logged-in user's email
    ''' </summary>
    Public CacheOnLoggedInEmail As String = ""

    ''' <summary>
    ''' Currently logged-in user's role
    ''' </summary>
    Public CacheOnUserRole As String = ""

    ''' <summary>
    ''' Check if current user is an administrator
    ''' </summary>
    Public ReadOnly Property IsAdmin As Boolean
        Get
            Return CacheOnUserRole = "Administrator" OrElse CacheOnUserRole = "Business Owner"
        End Get
    End Property

    ''' <summary>
    ''' Clears all user session data on logout
    ''' </summary>
    Public Sub ClearSession()
        CacheOnEmployeeID = ""
        CacheOnLoggedInName = ""
        CacheOnLoggedInEmail = ""
        CacheOnUserRole = ""
    End Sub
End Module