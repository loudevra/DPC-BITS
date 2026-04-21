Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq
Imports MySql.Data.MySqlClient
Imports System.Windows.Threading
Imports DPC.Data

Namespace DPC.Components.Navigation.ChatBot

    Public Class ChatBotWindow

        ' ═══ IDENTITY & STATE ═══
        Public Property CurrentLoggedInUser As String = System.Environment.MachineName

        Private _knowledgeBase As JArray = Nothing
        Private _isTyping As Boolean = False
        Private _isLiveChat As Boolean = False
        Private _liveChatTimer As DispatcherTimer
        Private _lastMessageId As Integer = 0

        Public Sub New()
            InitializeComponent()

            ' Setup polling for Live Chat
            _liveChatTimer = New DispatcherTimer()
            _liveChatTimer.Interval = TimeSpan.FromSeconds(2)
            AddHandler _liveChatTimer.Tick, AddressOf SyncMessagesWithDatabase
        End Sub

        ' 1. LOADED EVENT
        Public Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
            LoadKnowledgeBase()
            Await AddAIBubbleAsync($"SYSTEM INITIALIZED." & vbNewLine & $"Welcome, {CurrentLoggedInUser}. Type 'Live Support' to chat.")
        End Sub

        ' 2. POSITIONING
        Public Sub PositionNearNavBar(ownerWindow As Window)
            If ownerWindow Is Nothing Then Return
            Me.Left = SystemParameters.WorkArea.Width - Me.Width - 20
            Me.Top = SystemParameters.WorkArea.Height - Me.Height - 60
        End Sub

        ' 3. SEND MESSAGE
        Public Async Sub SendMessage(sender As Object, e As RoutedEventArgs)
            Dim userText As String = UserInput.Text.Trim()
            If String.IsNullOrWhiteSpace(userText) OrElse _isTyping Then Return

            UserInput.Clear()

            If _isLiveChat Then
                StatusText.Text = "SENDING..."

                ' FIX: Add your own bubble immediately for that instant Messenger feel
                AddUserBubble(userText)

                ' Save to DB in background
                Await SaveUserMessageToDb(userText)

                StatusText.Text = "LIVE SUPPORT • ONLINE"
            Else
                AddUserBubble(userText)
                _isTyping = True
                StatusText.Text = "THINKING..."
                Await Task.Delay(600)

                If userText.ToLower().Contains("live") OrElse userText.ToLower().Contains("agent") Then
                    Await SwitchToLiveChat()
                Else
                    Dim reply As String = GetAnswer(userText)
                    Await AddAIBubbleAsync(reply)
                End If

                If Not _isLiveChat Then StatusText.Text = "SYSTEM ACTIVE"
                _isTyping = False
            End If
            UserInput.Focus()
        End Sub

        ' 4. SWITCH TO LIVE SUPPORT
        Public Async Function SwitchToLiveChat() As Task
            _isLiveChat = True
            ChatTitle.Text = "COMMUNITY CHAT"
            StatusText.Text = "TRANSFERRING..."
            StatusText.Foreground = New SolidColorBrush(Color.FromRgb(105, 240, 174))
            ModeIndicator.Fill = New SolidColorBrush(Color.FromRgb(105, 240, 174))
            ToggleLiveBtn.Foreground = New SolidColorBrush(Color.FromRgb(105, 240, 174))

            Await AddAIBubbleAsync("Entering Live Mode. Loading recent messages...")

            ' FIX: Reset ID and get current Max ID so we only poll for NEW messages
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim cmd As New MySqlCommand("SELECT MAX(id) FROM chat_messages", conn)
                    Dim result = cmd.ExecuteScalar()
                    _lastMessageId = If(IsDBNull(result), 0, Convert.ToInt32(result))
                End Using
            Catch
                _lastMessageId = 0
            End Try

            _liveChatTimer.Start()
            StatusText.Text = "LIVE SUPPORT • ONLINE"
        End Function

        ' 5. DATABASE OPERATIONS
        Private Async Function SaveUserMessageToDb(msg As String) As Task
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    Await conn.OpenAsync()
                    ' Using parameters to handle special characters and security
                    Dim query As String = "INSERT INTO chat_messages (sender_name, message, is_from_admin) VALUES (@sender, @msg, 0)"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@sender", CurrentLoggedInUser)
                        cmd.Parameters.AddWithValue("@msg", msg)
                        Await cmd.ExecuteNonQueryAsync()
                    End Using
                End Using
            Catch ex As Exception
                StatusText.Text = "OFFLINE"
            End Try
        End Function

        Private Async Sub SyncMessagesWithDatabase(sender As Object, e As EventArgs)
            _liveChatTimer.Stop() ' Pause to prevent overlapping calls
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    Await conn.OpenAsync()

                    ' REGULAR POLLING: Load only new messages
                    Dim query As String = "SELECT id, sender_name, message FROM chat_messages WHERE id > @lastId ORDER BY id ASC"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@lastId", _lastMessageId)
                        Using reader = Await cmd.ExecuteReaderAsync()
                            While Await reader.ReadAsync()
                                Dim msgId As Integer = reader.GetInt32("id")
                                Dim sName As String = reader.GetString("sender_name")
                                Dim msgContent As String = reader.GetString("message")

                                ' Update the tracker
                                _lastMessageId = msgId

                                ' FIX: Only add a bubble if it came from the OTHER device
                                If Not sName.Equals(CurrentLoggedInUser, StringComparison.OrdinalIgnoreCase) Then
                                    Me.Dispatcher.Invoke(Sub()
                                                             DisplayIncomingMessage(msgId, sName, msgContent)
                                                         End Sub)
                                End If
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Handle connection errors
            Finally
                _liveChatTimer.Start()
            End Try
        End Sub

        Private Sub DisplayIncomingMessage(id As Integer, senderName As String, msgText As String)
            ' This is now primarily called for messages from other users
            Dim ignore = AddAIBubbleAsync("[" & senderName.ToUpper() & "]: " & msgText)
        End Sub

        ' 6. KNOWLEDGE BASE HELPERS
        Private Sub LoadKnowledgeBase()
            Try
                Dim filePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "knowledge-data", "knowledge.json")
                If File.Exists(filePath) Then
                    Dim jsonText As String = File.ReadAllText(filePath)
                    _knowledgeBase = CType(JObject.Parse(jsonText)("entries"), JArray)
                End If
            Catch
            End Try
        End Sub

        Private Function GetAnswer(userQuestion As String) As String
            Try
                If _knowledgeBase Is Nothing Then Return "Knowledge base not loaded."
                Dim lowerQuestion As String = userQuestion.ToLower()
                For Each entry As JToken In _knowledgeBase
                    Dim keywords As JArray = CType(entry("keywords"), JArray)
                    For Each k In keywords
                        If lowerQuestion.Contains(k.ToString().ToLower()) Then Return entry("answer").ToString()
                    Next
                Next
            Catch
            End Try
            Return "Inquiry not recognized. Type 'Live Support' to speak with a human."
        End Function

        ' 7. UI HELPERS
        Private Async Function AddAIBubbleAsync(message As String) As Task
            Dim bubble As New Border() With {.Style = CType(Resources("AIBubble"), Style)}
            Dim txt As New TextBlock() With {
                .Style = CType(Resources("HighlightText"), Style),
                .FontSize = 13, .TextWrapping = TextWrapping.Wrap
            }
            bubble.Child = txt
            MessageContainer.Children.Add(bubble)

            Dim currentText As String = ""
            For Each letter As Char In message
                currentText &= letter
                txt.Text = currentText
                ScrollToBottom()
                Await Task.Delay(10)
            Next
        End Function

        Private Sub AddUserBubble(message As String)
            Dim bubble As New Border() With {.Style = CType(Resources("UserBubble"), Style)}
            Dim txt As New TextBlock() With {
                .Text = message, .FontFamily = New FontFamily("Lexend"), .FontWeight = FontWeights.Bold,
                .Foreground = Brushes.White, .FontSize = 13, .TextWrapping = TextWrapping.Wrap
            }
            bubble.Child = txt
            MessageContainer.Children.Add(bubble)
            ScrollToBottom()
        End Sub

        Private Sub ScrollToBottom()
            If MessageScroll IsNot Nothing Then
                MessageScroll.ScrollToEnd()
            End If
        End Sub

        ' 8. EVENT HANDLERS
        Public Sub UserInput_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Enter Then
                e.Handled = True
                SendMessage(sender, Nothing)
            End If
        End Sub

        Public Sub CloseChat(sender As Object, e As RoutedEventArgs)
            _liveChatTimer.Stop()
            Me.Hide()
        End Sub

        Public Async Sub ToggleSupportMode(sender As Object, e As RoutedEventArgs)
            If Not _isLiveChat Then
                Await SwitchToLiveChat()
            Else
                _isLiveChat = False
                _liveChatTimer.Stop()
                ChatTitle.Text = "POS GUIDE AI"
                StatusText.Text = "SYSTEM ACTIVE"
                StatusText.Foreground = New SolidColorBrush(Colors.Gray)
                ModeIndicator.Fill = Brushes.White
                ToggleLiveBtn.Foreground = Brushes.White
                Await AddAIBubbleAsync("Environment Switched: AI Assistant is now active.")
            End If
        End Sub

        ' 9. QUICK SUGGESTIONS
        Public Sub QuickAsk_Quotation(sender As Object, e As RoutedEventArgs)
            UserInput.Text = "How do I create a quotation?"
            SendMessage(sender, e)
        End Sub

        Public Sub QuickAsk_Billing(sender As Object, e As RoutedEventArgs)
            UserInput.Text = "How do I make a billing statement?"
            SendMessage(sender, e)
        End Sub

        Public Sub QuickAsk_Items(sender As Object, e As RoutedEventArgs)
            UserInput.Text = "How do I add items or products?"
            SendMessage(sender, e)
        End Sub

    End Class
End Namespace