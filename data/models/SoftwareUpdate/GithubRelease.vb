Imports System.Text.Json.Serialization

Namespace DPC.Data.Models
    Public Class GithubRelease
        <JsonPropertyName("tag_name")>
        Public Property tag_name As String

        <JsonPropertyName("prerelease")>
        Public Property prerelease As Boolean

        <JsonPropertyName("published_at")>
        Public Property published_at As DateTime

        <JsonPropertyName("assets")>
        Public Property assets() As GithubAsset()
    End Class

    Public Class GithubAsset
        <JsonPropertyName("browser_download_url")>
        Public Property browser_download_url As String
    End Class
End Namespace