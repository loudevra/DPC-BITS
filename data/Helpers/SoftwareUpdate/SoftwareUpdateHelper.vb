Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Windows.Forms
Imports Newtonsoft.Json.Linq
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports DPC.DPC.Data.Models

Namespace DPC.Data.Helpers.SoftwareUpdate
    Public Class SoftwareUpdateHelper
        Private Shared ReadOnly GitHubToken As String = EnvLoader.GetEnv("GIT_TOKEN")
        Private Shared ReadOnly OrgName As String = "loudevra"
        Private Shared ReadOnly RepoName As String = "DPC-BITS"
        Private Shared ReadOnly GitHubApiUrl As String = $"https://api.github.com/repos/{OrgName}/{RepoName}/releases/latest"

        Public Shared Async Function GetLatestPrivatePreReleaseVersion() As Task(Of String)
            Using client As New HttpClient()
                client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", GitHubToken)
                client.DefaultRequestHeaders.UserAgent.ParseAdd("MyAppUpdater")

                Try
                    Dim response = Await client.GetAsync(GitHubApiUrl)
                    response.EnsureSuccessStatusCode()

                    Dim json = Await response.Content.ReadAsStringAsync()
                    Dim release = JsonSerializer.Deserialize(Of GithubRelease)(json)

                    If release IsNot Nothing AndAlso release.prerelease Then
                        Return release.tag_name
                    End If
                Catch ex As Exception
                    System.Windows.Forms.MessageBox.Show("Error checking for updates: " & ex.Message)
                End Try
                Return Nothing
            End Using
        End Function

        Public Shared Async Function CheckForUpdate() As Task
            Try
                Dim latestVersion = Await SoftwareUpdateHelper.GetLatestPrivatePreReleaseVersion()
                Dim currentVersion = My.Application.Info.Version.ToString()

                If latestVersion IsNot Nothing AndAlso latestVersion <> currentVersion Then
                    Using client As New HttpClient()
                        client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", GitHubToken)
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("MyAppUpdater")

                        Dim response = Await client.GetAsync(GitHubApiUrl)
                        response.EnsureSuccessStatusCode()

                        Dim json = Await response.Content.ReadAsStringAsync()
                        Dim release = JsonSerializer.Deserialize(Of GithubRelease)(json)

                        If release IsNot Nothing AndAlso release.assets IsNot Nothing AndAlso release.assets.Length > 0 Then
                            Dim downloadUrl = release.assets(0).browser_download_url

                            Dim result = System.Windows.Forms.MessageBox.Show(
                                $"A new version ({latestVersion}) is available. Do you want to download it now?",
                                "Software Update", MessageBoxButtons.YesNo, MessageBoxIcon.Information)

                            If result = MessageBoxResult.Yes Then
                                Process.Start(New ProcessStartInfo(downloadUrl) With {
                                    .UseShellExecute = True
                                })
                                Application.Current.Shutdown()
                            End If
                        End If
                    End Using
                End If
            Catch ex As Exception
                ' Optional: log or ignore
            End Try
        End Function
    End Class
End Namespace