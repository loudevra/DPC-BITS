Imports System.Windows
Imports System.Windows.Input

Namespace DPC
    Partial Public Class MainWindow
        Public Sub New()
            InitializeComponent()
        End Sub

        ' Sign-In button click event

        Private Sub BtnSignIn_Click(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Welcome: " & txtEmail.Text)
            ' Redirect to Dashboard.xaml
            Dim dashboard As New Views.Dashboard.Dashboard()
            dashboard.Show()
            Me.Close()
        End Sub
        ' Close the application when the close button is clicked
        Private Sub CloseApp_Click(sender As Object, e As RoutedEventArgs)
            Me.Close()
        End Sub

    End Class
End Namespace
