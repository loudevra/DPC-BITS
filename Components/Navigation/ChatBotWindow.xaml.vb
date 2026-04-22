Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq
Imports System.Windows.Threading
Imports DPC.Data
' MongoDB Driver Imports
Imports MongoDB.Driver
Imports MongoDB.Bson

Namespace DPC.Components.Navigation.ChatBot

    Public Class ChatBotWindow

        ' ═══ IDENTITY & MONGO STATE ═══
        Public ReadOnly Property CurrentLoggedInUser As String
            Get
                Return DPC.Data.Helpers.GlobalVariables.CurrentUserName
            End Get
        End Property

        Private _db As IMongoDatabase
        Private _msgCollection As IMongoCollection(Of BsonDocument)
        Private _accCollection As IMongoCollection(Of BsonDocument)

        Private _knowledgeBase As JArray = Nothing
        Private _isTyping As Boolean = False
        Private _isLiveChat As Boolean = False
        Private _liveChatTimer As DispatcherTimer
        Private _lastMessageId As ObjectId = ObjectId.Empty

        Public Sub New()
            InitializeComponent()

            ' 1. Setup polling for Live Chat
            _liveChatTimer = New DispatcherTimer()
            _liveChatTimer.Interval = TimeSpan.FromSeconds(1.5)
            AddHandler _liveChatTimer.Tick, AddressOf SyncMessagesWithDatabase

            ' 2. Initialize MongoDB
            Try
                _db = SplashScreen.GetMongoDatabaseConnection()
                _msgCollection = _db.GetCollection(Of BsonDocument)("chat_messages")
                _accCollection = _db.GetCollection(Of BsonDocument)("accounts")

                SeedtestAccounts()
            Catch ex As Exception
                ' Silent fail if DB is offline
            End Try
        End Sub

        ' ═══ SEEDING TEST ACCOUNTS ═══
        Private Async Sub SeedtestAccounts()
            Try
                Dim count = Await _accCollection.CountDocumentsAsync(New BsonDocument())
                If count = 0 Then
                    Dim testUsers As New List(Of BsonDocument) From {
                        New BsonDocument From {{"username", "@SalesP1"}, {"password", "password123"}, {"role", "Sales"}},
                        New BsonDocument From {{"username", "admin"}, {"password", "admin123"}, {"role", "Administrator"}}
                    }
                    Await _accCollection.InsertManyAsync(testUsers)
                End If
            Catch
            End Try
        End Sub

        ' ═══ WINDOW EVENTS ═══
        Public Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
            LoadKnowledgeBase()
            Await AddAIBubbleAsync($"SYSTEM INITIALIZED." & vbNewLine & $"Welcome, {CurrentLoggedInUser}. Type 'Live Support' to chat.")
        End Sub

        ' Fixed: Added this back so TopNavBar can find it
        Public Sub PositionNearNavBar(ownerWindow As Window)
            If ownerWindow Is Nothing Then Return
            Me.Left = SystemParameters.WorkArea.Width - Me.Width - 20
            Me.Top = SystemParameters.WorkArea.Height - Me.Height - 60
        End Sub

        ' ═══ CHAT LOGIC ═══
        Public Async Sub SendMessage(sender As Object, e As RoutedEventArgs)
            Dim userText As String = UserInput.Text.Trim()
            If String.IsNullOrWhiteSpace(userText) OrElse _isTyping Then Return

            UserInput.Clear()

            If _isLiveChat Then
                StatusText.Text = "SENDING..."
                AddUserBubble(userText)
                Await SaveUserMessageToMongo(userText)
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

        ' ═══ MONGO OPERATIONS ═══
        Private Async Function SaveUserMessageToMongo(msg As String) As Task
            Try
                Dim doc As New BsonDocument From {
                    {"sender_name", CurrentLoggedInUser},
                    {"message", msg},
                    {"timestamp", DateTime.UtcNow},
                    {"is_from_admin", 0}
                }
                Await _msgCollection.InsertOneAsync(doc)
            Catch
                StatusText.Text = "OFFLINE"
            End Try
        End Function

        ' Fixed: Resolved the 'Gt' (Greater Than) error for ObjectId
        Private Async Sub SyncMessagesWithDatabase(sender As Object, e As EventArgs)
            _liveChatTimer.Stop()
            Try
                ' This specifically uses the MongoDB Filter Builder for ObjectIds
                Dim filter = Builders(Of BsonDocument).Filter.Gt(Of ObjectId)("_id", _lastMessageId)
                Dim sort = Builders(Of BsonDocument).Sort.Ascending("_id")

                Dim newMessages = Await _msgCollection.Find(filter).Sort(sort).ToListAsync()

                For Each doc In newMessages
                    Dim msgId = doc("_id").AsObjectId
                    Dim sName = doc("sender_name").AsString
                    Dim msgContent = doc("message").AsString

                    _lastMessageId = msgId

                    If Not sName.Equals(CurrentLoggedInUser, StringComparison.OrdinalIgnoreCase) Then
                        Me.Dispatcher.Invoke(Sub()
                                                 Dim ignore = AddAIBubbleAsync($"[{sName.ToUpper()}]: {msgContent}")
                                             End Sub)
                    End If
                Next
            Catch
            Finally
                _liveChatTimer.Start()
            End Try
        End Sub

        ' ═══ MODE SWITCH ═══
        Public Async Function SwitchToLiveChat() As Task
            _isLiveChat = True
            ChatTitle.Text = "COMMUNITY CHAT"
            StatusText.Text = "TRANSFERRING..."

            StatusText.Foreground = New SolidColorBrush(Color.FromRgb(105, 240, 174))
            ModeIndicator.Fill = New SolidColorBrush(Color.FromRgb(105, 240, 174))
            ToggleLiveBtn.Foreground = New SolidColorBrush(Color.FromRgb(105, 240, 174))

            Try
                Dim latest = Await _msgCollection.Find(New BsonDocument()).Sort(Builders(Of BsonDocument).Sort.Descending("_id")).Limit(1).FirstOrDefaultAsync()
                If latest IsNot Nothing Then
                    _lastMessageId = latest("_id").AsObjectId
                End If
            Catch
                _lastMessageId = ObjectId.Empty
            End Try

            _liveChatTimer.Start()
            StatusText.Text = "LIVE SUPPORT • ONLINE"
            Await AddAIBubbleAsync("Connected to Wireless Live Chat.")
        End Function

        ' ═══ KNOWLEDGE BASE HELPERS ═══
        ' Fixed: Re-added LoadKnowledgeBase
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

        ' Fixed: Re-added GetAnswer
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

        ' ═══ QUICK ASK BUTTONS (FIXED) ═══
        ' These methods solve the errors shown in image_772a41.png
        Public Sub QuickAsk_Quotation(sender As Object, e As RoutedEventArgs)
            UserInput.Text = "How to create a quotation?"
            SendMessage(Nothing, Nothing)
        End Sub

        Public Sub QuickAsk_Billing(sender As Object, e As RoutedEventArgs)
            UserInput.Text = "How to process billing?"
            SendMessage(Nothing, Nothing)
        End Sub

        Public Sub QuickAsk_Items(sender As Object, e As RoutedEventArgs)
            UserInput.Text = "How to add new items?"
            SendMessage(Nothing, Nothing)
        End Sub

        ' ═══ UI BUBBLE GENERATORS ═══
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
                Await Task.Delay(5)
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
            If MessageScroll IsNot Nothing Then MessageScroll.ScrollToEnd()
        End Sub

        ' ═══ UI EVENT HANDLERS ═══
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
    End Class
End Namespace