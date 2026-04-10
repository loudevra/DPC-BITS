Imports System.Windows
Imports DPC.DPC.Components
Imports DPC.DPC.Components.ConfirmationModals
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports Microsoft.VisualBasic.ApplicationServices
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Auth
    Public Class SignIn
        Inherits UserControl

        Private passwordHidden As Boolean = True
        Private realPassword As String = ""
        Private Shared UserRoleID As Integer
        Private isProcessing As Boolean = False
        Private confirmationModal As LoginConfirmationModals

        Public Sub New()
            InitializeComponent()
            AddHandler txtUsername.KeyDown, AddressOf TextBox_KeyDown
            AddHandler txtPassword.KeyDown, AddressOf TextBox_KeyDown
            confirmationModal = New LoginConfirmationModals()
            MainGrid.Children.Add(confirmationModal)
            AddHandler confirmationModal.SuccessConfirmed, AddressOf OnLoginSuccess
            AddHandler confirmationModal.ErrorRetry, AddressOf OnLoginError
        End Sub

        Private Sub BtnSignIn_Click(sender As Object, e As RoutedEventArgs)
            PerformSignIn()
        End Sub

        Private Sub PerformSignIn()
            If isProcessing Then Return
            isProcessing = True

            Dim username As String = txtUsername.Text.Trim()
            Dim password As String = realPassword

            If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
                confirmationModal.ShowError("Please enter both username and password.")
                isProcessing = False
                Return
            End If

            Dim authResult As (String, String) = AuthController.SignIn(username, password)
            Dim accessToken As String = authResult.Item1
            Dim refreshToken As String = authResult.Item2

            If Not String.IsNullOrEmpty(accessToken) AndAlso Not String.IsNullOrEmpty(refreshToken) Then
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    Try
                        conn.Open()
                        Dim query As String = "SELECT UserRoleID FROM employee WHERE Username = '" & username & "'"
                        Dim cmd As New MySqlCommand(query, conn)
                        Dim reader = cmd.ExecuteReader()
                        While (reader.Read)
                            UserRoleID = reader.GetInt32("UserRoleID")
                        End While
                    Catch ex As Exception
                    End Try
                End Using

                SessionManager.SetSessionTokens(accessToken, refreshToken)
                confirmationModal.ShowSuccess("Login Successful!")
            Else
                confirmationModal.ShowError("Invalid username or password." & vbCrLf & "Please try again.")
                realPassword = ""
                txtPassword.Text = ""
                isProcessing = False
            End If
        End Sub

        Private Sub OnLoginSuccess()
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim query As String = "SELECT RoleName FROM userroles WHERE RoleID = " & UserRoleID
                    Dim cmd As New MySqlCommand(query, conn)
                    Dim reader = cmd.ExecuteReader()
                    While (reader.Read)
                        Dim Role As String = reader.GetString("RoleName")
                        Dim landingView As String = "dashboard"

                        Try
                            Using permConn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                                permConn.Open()
                                Dim permQuery As String = "SELECT * FROM permissions WHERE Role = '" & Role & "'"
                                Dim permCmd As New MySqlCommand(permQuery, permConn)
                                Dim permReader = permCmd.ExecuteReader()
                                Dim SalesPerm As Boolean = False

                                If permReader.Read() Then
                                    SalesPerm = Convert.ToBoolean(permReader("Sales"))
                                    ProjectPerm = Convert.ToBoolean(permReader("Project"))
                                    AccountsPerm = Convert.ToBoolean(permReader("Accounts"))
                                End If

                                Dim roleLower = Role.ToLower()
                                If roleLower.Contains("sales") Then
                                    If SalesPerm Then landingView = "walkinorder"
                                ElseIf roleLower.Contains("manager") AndAlso roleLower.Contains("business") Then
                                    If ProjectPerm Then landingView = "manageproject"
                                ElseIf roleLower.Contains("admin") Then
                                    landingView = "dashboard"
                                End If

                                If landingView = "dashboard" Then
                                    If SalesPerm Then
                                        landingView = "walkinorder"
                                    ElseIf ProjectPerm Then
                                        landingView = "manageproject"
                                    End If
                                End If
                            End Using
                        Catch exPerm As Exception
                            landingView = "dashboard"
                        End Try

                        PermissionCache.LoadForRole(Role)
                        Dim baseWindow As New Base(Role) With {
                            .CurrentView = ViewLoader.DynamicView.Load(landingView)
                        }
                        baseWindow.Show()

                        Dim currentWindow As Window = Window.GetWindow(Me)
                        currentWindow?.Close()
                    End While
                Catch ex As Exception
                    Try
                        Dim baseWindow As New Base("") With {
            .CurrentView = ViewLoader.DynamicView.Load("dashboard")
        }
                        baseWindow.Show()
                        Dim currentWindow As Window = Window.GetWindow(Me)
                        currentWindow?.Close()
                    Catch
                    End Try
                End Try
            End Using
        End Sub

        Private Sub OnLoginError()
            txtUsername.Focus()
        End Sub

        Private Sub TxtPassword_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            realPassword &= e.Text
            txtPassword.Text = New String("●"c, realPassword.Length)
            txtPassword.CaretIndex = txtPassword.Text.Length
            e.Handled = True
        End Sub

        Private Sub TxtPassword_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Back AndAlso realPassword.Length > 0 Then
                realPassword = realPassword.Substring(0, realPassword.Length - 1)
                txtPassword.Text = New String("●"c, realPassword.Length)
                txtPassword.CaretIndex = txtPassword.Text.Length
                e.Handled = True
            End If
        End Sub

        Private Sub TextBox_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Enter Then
                e.Handled = True
                PerformSignIn()
            End If
        End Sub

        Private Sub TxtPassword_TextChanged(sender As Object, e As TextChangedEventArgs)
            If String.IsNullOrEmpty(txtPassword.Text) Then
                realPassword = ""
            End If
        End Sub

        Private Sub ForgotPassword_Click(sender As Object, e As MouseButtonEventArgs)
            Dim forgotPasswordView As New ForgotPassword()
            Dim parentWindow As Window = Window.GetWindow(Me)

            If TypeOf parentWindow Is MainWindow Then
                Dim mainWin As MainWindow = CType(parentWindow, MainWindow)
                mainWin.CurrentViewIndex = 1
            End If
        End Sub

        Private Sub BtnTogglePassword_Click(sender As Object, e As RoutedEventArgs)
            passwordHidden = Not passwordHidden

            If passwordHidden Then
                txtPassword.Text = New String("●"c, realPassword.Length)
                iconPasswordToggle.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOffOutline
            Else
                txtPassword.Text = realPassword
                iconPasswordToggle.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOutline
            End If

            txtPassword.CaretIndex = txtPassword.Text.Length
        End Sub

    End Class
End Namespace

