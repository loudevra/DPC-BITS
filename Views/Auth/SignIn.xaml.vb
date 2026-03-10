' SignIn.xaml.vb
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

        ' Fields for password handling
        Private passwordHidden As Boolean = True
        Private realPassword As String = ""
        Private Shared UserRoleID As Integer

        ' Add modal instance
        Private confirmationModal As LoginConfirmationModals

        Public Sub New()
            InitializeComponent()

            ' Add key event handler to both username and password fields
            AddHandler txtUsername.KeyDown, AddressOf TextBox_KeyDown
            AddHandler txtPassword.KeyDown, AddressOf TextBox_KeyDown

            ' Initialize the confirmation modal
            confirmationModal = New LoginConfirmationModals()

            ' Add the modal to the MainGrid
            MainGrid.Children.Add(confirmationModal)

            ' Add event handlers for the modal
            AddHandler confirmationModal.SuccessConfirmed, AddressOf OnLoginSuccess
            AddHandler confirmationModal.ErrorRetry, AddressOf OnLoginError
        End Sub


        ' Handle Sign-In Process
        Private Sub BtnSignIn_Click(sender As Object, e As RoutedEventArgs)
            PerformSignIn()
        End Sub

        ' Common method for sign-in functionality
        Private Sub PerformSignIn()
            Dim username As String = txtUsername.Text.Trim()
            Dim password As String = realPassword

            ' Check if fields are empty
            If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
                confirmationModal.ShowError("Please enter both username and password.")
                Return
            End If

            ' Authenticate user
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

                ' Store tokens for session
                SessionManager.SetSessionTokens(accessToken, refreshToken)

                ' Show success modal
                confirmationModal.ShowSuccess("Login Successful!")

                ' Note: Navigation will be handled in the OnLoginSuccess method
            Else
                ' Show error modal
                confirmationModal.ShowError("Invalid username or password. Please try again.")

                ' Clear password field after failed login
                realPassword = ""
                txtPassword.Text = ""
            End If
        End Sub

        ' Handle successful login
        Private Sub OnLoginSuccess()
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim query As String = "SELECT RoleName FROM userroles WHERE RoleID = " & UserRoleID
                    Dim cmd As New MySqlCommand(query, conn)
                    Dim reader = cmd.ExecuteReader()
                    While (reader.Read)
                        Dim Role As String = reader.GetString("RoleName")

                        ' Determine landing view based on role and permissions.
                        ' Default to dashboard if no preferred landing view is available.
                        Dim landingView As String = "dashboard"

                        ' Query permissions for this role to ensure the landing view is accessible
                        Try
                            Using permConn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                                permConn.Open()
                                Dim permQuery As String = "SELECT * FROM permissions WHERE Role = '" & Role & "'"
                                Dim permCmd As New MySqlCommand(permQuery, permConn)
                                Dim permReader = permCmd.ExecuteReader()
                                Dim SalesPerm As Boolean = False
                                Dim ProjectPerm As Boolean = False
                                Dim AccountsPerm As Boolean = False
                                If permReader.Read() Then
                                    SalesPerm = Convert.ToBoolean(permReader("Sales"))
                                    ProjectPerm = Convert.ToBoolean(permReader("Project"))
                                    AccountsPerm = Convert.ToBoolean(permReader("Accounts"))
                                End If

                                ' Role-specific preferred landing
                                Dim roleLower = Role.ToLower()
                                If roleLower.Contains("sales") Then
                                    If SalesPerm Then
                                        landingView = "walkinorder" ' Sales -> Walk-In New Order
                                    End If
                                ElseIf roleLower.Contains("manager") AndAlso roleLower.Contains("business") Then
                                    If ProjectPerm Then
                                        landingView = "manageproject" ' Business Manager -> Manage Project
                                    End If
                                ElseIf roleLower.Contains("owner") Then
                                    If AccountsPerm Then
                                        landingView = "manageaccounts" ' Business Owner -> Manage Accounts
                                    End If
                                ElseIf roleLower.Contains("admin") Then
                                    landingView = "dashboard" ' Admin -> Permissions
                                End If

                                ' Fallback: if preferred role mapping not usable, pick first available important module
                                If landingView = "dashboard" Then
                                    If SalesPerm Then
                                        landingView = "walkinorder"
                                    ElseIf ProjectPerm Then
                                        landingView = "manageproject"
                                    ElseIf AccountsPerm Then
                                        landingView = "manageaccounts"
                                    End If
                                End If
                            End Using
                        Catch exPerm As Exception
                            ' If permission check fails, continue with default dashboard
                            landingView = "dashboard"
                        End Try

                        ' Redirect to Base.xaml and load chosen landing view
                        Dim baseWindow As New Base(Role) With {
                            .CurrentView = ViewLoader.DynamicView.Load(landingView)
                        }

                        ' Show the Base window
                        baseWindow.Show()

                        ' Close the current window (SignIn window)
                        Dim currentWindow As Window = Window.GetWindow(Me)
                        currentWindow?.Close()
                    End While
                Catch ex As Exception
                    ' If anything goes wrong, fallback to opening Base with dashboard
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

        ' Handle retry after login error
        Private Sub OnLoginError()
            ' Focus on username field to retry
            txtUsername.Focus()
        End Sub


        ' Handle text input correctly while masking
        Private Sub TxtPassword_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            realPassword &= e.Text ' Append typed character to realPassword
            txtPassword.Text = New String("●"c, realPassword.Length) ' Mask the password
            txtPassword.CaretIndex = txtPassword.Text.Length ' Move cursor to end
            e.Handled = True ' Prevent actual text from appearing
        End Sub

        ' Handle backspace key and Enter key
        Private Sub TxtPassword_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Back AndAlso realPassword.Length > 0 Then
                realPassword = realPassword.Substring(0, realPassword.Length - 1)
                txtPassword.Text = New String("●"c, realPassword.Length)
                txtPassword.CaretIndex = txtPassword.Text.Length
                e.Handled = True
            End If
        End Sub

        ' New method to handle Enter key in both text fields
        Private Sub TextBox_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Enter Then
                e.Handled = True
                PerformSignIn()
            End If
        End Sub

        ' Handle TextChanged for any external modifications (e.g., clearing the field)
        Private Sub TxtPassword_TextChanged(sender As Object, e As TextChangedEventArgs)
            If String.IsNullOrEmpty(txtPassword.Text) Then
                realPassword = "" ' Reset real password if input is cleared
            End If
        End Sub


        Private Sub ForgotPassword_Click(sender As Object, e As MouseButtonEventArgs)
            ' Navigate to Forgot Password screen
            Dim forgotPasswordView As New ForgotPassword()
            Dim parentWindow As Window = Window.GetWindow(Me)

            ' If inside a window, switch to ForgotPassword view
            If TypeOf parentWindow Is MainWindow Then
                Dim mainWin As MainWindow = CType(parentWindow, MainWindow)
                mainWin.CurrentViewIndex = 1 ' Switch to Forgot Password View
            End If
        End Sub


        ' Toggle password visibility
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