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
                        ' UPDATED QUERY: Select ID as well so we can store it in GlobalVariables
                        Dim query As String = "SELECT EmployeeID, UserRoleID FROM employee WHERE Username = @user"
                        Dim cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@user", username)

                        Dim reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            UserRoleID = reader.GetInt32("UserRoleID")

                            ' ═══ CRITICAL FIX ═══
                            ' Store the logged-in user data globally so the ChatBot can see it
                            DPC.Data.Helpers.GlobalVariables.CurrentUserName = username
                            DPC.Data.Helpers.GlobalVariables.CacheOnEmployeeID = reader.GetInt32("EmployeeID")
                            ' ════════════════════
                        End If
                        reader.Close()
                    Catch ex As Exception
                        ' Log error if needed
                    End Try
                End Using

                SessionManager.SetSessionTokens(accessToken, refreshToken)
                confirmationModal.ShowSuccess("Login Successful!")

                ' Note: After this, your code usually navigates to the Dashboard/MainWindow
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
                    Dim query As String = "SELECT RoleName FROM userroles WHERE RoleID = @roleId"
                    Dim cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@roleId", UserRoleID)

                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim Role As String = reader.GetString("RoleName")

                            ' 1. LOAD PERMISSIONS FIRST!
                            PermissionCache.LoadForRole(Role)

                            ' 2. DETERMINE LANDING VIEW
                            Dim landingView As String = ""
                            Dim roleLower = Role.ToLower()

                            ' Specific Routing based on your roles
                            If roleLower.Contains("sales") AndAlso PermissionCache.Can("Sales") Then
                                landingView = "walkinorder"

                            ElseIf (roleLower.Contains("tech") OrElse roleLower.Contains("project")) AndAlso PermissionCache.Can("Project") Then
                                landingView = "manageproject"

                                ' Universal Fallback: Route them to whatever they CAN see
                            ElseIf PermissionCache.Can("Dashboard") Then
                                landingView = "dashboard"
                            ElseIf PermissionCache.Can("Sales") Then
                                landingView = "walkinorder"
                            ElseIf PermissionCache.Can("Project") Then
                                landingView = "manageproject"
                            ElseIf PermissionCache.Can("Stocks") Then
                                landingView = "stocks" ' (Change to your actual stocks view name if different)
                            ElseIf PermissionCache.Can("CRM") Then
                                landingView = "crm" ' (Change to your actual crm view name if different)
                            Else
                                ' Absolute fallback
                                landingView = "dashboard"
                            End If

                            ' 3. LOAD THE WINDOW
                            Dim baseWindow As New Base(Role) With {
                                .CurrentView = ViewLoader.DynamicView.Load(landingView)
                            }
                            baseWindow.Show()

                            Dim currentWindow As Window = Window.GetWindow(Me)
                            currentWindow?.Close()
                        End If
                    End Using
                Catch ex As Exception
                    ' Fallback in case of database error
                    Try
                        PermissionCache.LoadForRole("Administrator") ' Safe default
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

