Imports Microsoft.Web.WebView2.Wpf
Imports System.IO

Namespace DPC.Components.Navigation
    Public Class AIWebView
        Private WithEvents webView As New Microsoft.Web.WebView2.Wpf.WebView2()

        Private Async Sub AIWebView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            ' This is the most important line for stopping Designer errors
            If System.ComponentModel.DesignerProperties.GetIsInDesignMode(Me) Then Return

            Try
                If Not BrowserContainer.Children.Contains(webView) Then
                    BrowserContainer.Children.Add(webView)
                End If

                ' If this fails, it might be a runtime version issue
                Await webView.EnsureCoreWebView2Async(Nothing)
                webView.Source = New Uri("https://dream-pc-build-message.vercel.app/")

                ' Hide loading text once browser is ready
                LoadingText.Visibility = Visibility.Collapsed
            Catch ex As Exception
                Debug.WriteLine("WebView Error: " & ex.Message)
            End Try
        End Sub
    End Class
End Namespace