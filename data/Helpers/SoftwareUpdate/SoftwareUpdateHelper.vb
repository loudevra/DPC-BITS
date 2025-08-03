Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Windows.Forms
Imports Newtonsoft.Json.Linq
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports DPC.DPC.Data.Models
Imports System.Version

Namespace DPC.Data.Helpers.SoftwareUpdate
    Public Class SoftwareUpdateHelper
        ' Variable to hold the GitHub token and repository details
        Private Shared ReadOnly GitHubToken As String = EnvLoader.GetEnv("GIT_TOKEN")
        Private Shared ReadOnly OrgName As String = "loudevra"
        Private Shared ReadOnly RepoName As String = "DPC-BITS"
        Private Shared ReadOnly GitHubApiUrl As String = $"https://api.github.com/repos/{OrgName}/{RepoName}/releases/latest"

        ' Class to represent the GitHub release structure
        Public Shared Async Function GetLatestPrivatePreReleaseVersion() As Task(Of String)
            Using client As New HttpClient()
                client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", GitHubToken)
                client.DefaultRequestHeaders.UserAgent.ParseAdd("MyAppUpdater")

                Try
                    Dim response = Await client.GetAsync(GitHubApiUrl)
                    ' Ensure the response is successful
                    response.EnsureSuccessStatusCode()

                    Dim json = Await response.Content.ReadAsStringAsync()
                    Dim release = JsonSerializer.Deserialize(Of GithubRelease)(json)

                    If release IsNot Nothing Then
                        Dim normalizedTag = release.tag_name.Replace("v", "").Trim() ' Remove 'v' and extra spaces
                        MessageBox.Show($"Debug: Latest tag from GitHub: {normalizedTag}") ' Debug output
                        Return normalizedTag
                    End If
                Catch ex As Exception
                    System.Windows.Forms.MessageBox.Show("Error checking for updates: " & ex.Message)
                End Try
                Return Nothing
            End Using
        End Function

        ' Check for updates against the latest version on GitHub
        Public Shared Async Function CheckForUpdate() As Task
            Try
                ' Fetch the latest version from GitHub
                Dim latestVersionStr = Await SoftwareUpdateHelper.GetLatestPrivatePreReleaseVersion()
                ' Takes the Current version from the application info
                Dim currentVersionStr = My.Application.Info.Version.ToString()
                MessageBox.Show($"Debug: Current Version: {currentVersionStr}, Latest Version: {latestVersionStr}") ' Debug output

                If latestVersionStr IsNot Nothing Then
                    Dim latestVersion = New Version(latestVersionStr)
                    Dim currentVersion = New Version(currentVersionStr)

                    MessageBox.Show($"Debug: Comparing - Current: {currentVersion}, Latest: {latestVersion}") ' Debug output

                    If latestVersion > currentVersion Then
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
                    ElseIf latestVersion < currentVersion Then
                        MessageBox.Show($"You are running a version newer than the latest release ({currentVersion}).")
                    Else
                        MessageBox.Show($"You are running the latest version ({currentVersion}).")
                    End If
                Else
                    MessageBox.Show("No latest version information available.")
                End If
            Catch ex As Exception
                MessageBox.Show("Error checking for updates: " & ex.Message)
            End Try
        End Function
    End Class
End Namespace