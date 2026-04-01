Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports Microsoft.Win32
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Project
    Partial Public Class AddProject1
        Inherits UserControl

        Private startDateViewModel As New CalendarController.SingleCalendar()
        Private dueDateViewModel As New CalendarController.SingleCalendar()

        ' Store EmployeeID for saving (fits VARCHAR(20))
        Private AssignedEmployeeID As String = Nothing

        ' =========================================================
        ' 1. INTERNAL MEMORY (Keeps data alive across tabs)
        ' =========================================================
        Private Shared _savedProjectName As String = ""
        Private Shared _savedCustomer As String = ""
        Private Shared _savedBudget As String = ""
        Private Shared _savedStatusIndex As Integer = -1
        Private Shared _savedStartDate As DateTime? = Nothing
        Private Shared _savedDueDate As DateTime? = Nothing
        Private Shared _savedDueDateOnlyChecked As Boolean = True
        Private Shared _savedNote As String = ""

        Public Sub New()
            InitializeComponent()
            SetupDatePickers()

            Dim statuses As New List(Of StatusItem) From {
                New StatusItem With {.Label = "Waiting", .Color = New SolidColorBrush(Color.FromRgb(229, 209, 142))},
                New StatusItem With {.Label = "Processing", .Color = New SolidColorBrush(Color.FromRgb(134, 188, 213))},
                New StatusItem With {.Label = "Solved", .Color = New SolidColorBrush(Color.FromRgb(137, 172, 116))},
                New StatusItem With {.Label = "Cancelled", .Color = New SolidColorBrush(Color.FromRgb(230, 94, 94))}
            }

            cmbStatus.ItemsSource = statuses

            ' 2. RESTORE DATA FROM MEMORY
            txtName.Text = _savedProjectName
            txtCustomer.Text = _savedCustomer
            txtBudget.Text = _savedBudget
            cmbStatus.SelectedIndex = _savedStatusIndex

            StartDatePicker.SelectedDate = _savedStartDate
            startDateViewModel.SelectedDate = _savedStartDate

            DueDatePicker.SelectedDate = _savedDueDate
            dueDateViewModel.SelectedDate = _savedDueDate

            RadBtnDueDateOnly.IsChecked = _savedDueDateOnlyChecked
            RadBtnStartDueDate.IsChecked = Not _savedDueDateOnlyChecked

            If Not String.IsNullOrWhiteSpace(_savedNote) Then
                EditorBox.Document.Blocks.Clear()
                EditorBox.Document.Blocks.Add(New Paragraph(New Run(_savedNote)))
            End If

            ' 3. AUTO-SAVE HANDLERS (Added AFTER restoring to prevent overwriting)
            AddHandler txtName.TextChanged, AddressOf SaveTextToMemory
            AddHandler txtCustomer.TextChanged, AddressOf SaveTextToMemory
            AddHandler txtBudget.TextChanged, AddressOf SaveTextToMemory
            AddHandler cmbStatus.SelectionChanged, AddressOf SaveComboToMemory
            AddHandler StartDatePicker.SelectedDateChanged, AddressOf SaveDatesToMemory
            AddHandler DueDatePicker.SelectedDateChanged, AddressOf SaveDatesToMemory
            AddHandler RadBtnDueDateOnly.Checked, AddressOf SaveRadioToMemory
            AddHandler RadBtnStartDueDate.Checked, AddressOf SaveRadioToMemory
            AddHandler EditorBox.TextChanged, AddressOf SaveEditorToMemory

            AddHandler Me.Loaded, AddressOf AddProject1_Loaded
        End Sub

        ' =========================================================
        ' MEMORY MANAGEMENT (Auto-saving as you type)
        ' =========================================================
        Private Sub SaveTextToMemory(sender As Object, e As TextChangedEventArgs)
            _savedProjectName = txtName.Text
            _savedCustomer = txtCustomer.Text
            _savedBudget = txtBudget.Text
        End Sub

        Private Sub SaveComboToMemory(sender As Object, e As SelectionChangedEventArgs)
            _savedStatusIndex = cmbStatus.SelectedIndex
        End Sub

        Private Sub SaveDatesToMemory(sender As Object, e As SelectionChangedEventArgs)
            _savedStartDate = StartDatePicker.SelectedDate
            _savedDueDate = DueDatePicker.SelectedDate
        End Sub

        Private Sub SaveRadioToMemory(sender As Object, e As RoutedEventArgs)
            _savedDueDateOnlyChecked = RadBtnDueDateOnly.IsChecked.GetValueOrDefault(True)
        End Sub

        Private Sub SaveEditorToMemory(sender As Object, e As TextChangedEventArgs)
            _savedNote = New TextRange(EditorBox.Document.ContentStart, EditorBox.Document.ContentEnd).Text.Trim()
        End Sub

        ' =========================================================
        ' EXISTING LOGIC BELOW
        ' =========================================================

        Private Sub AddProject1_Loaded(sender As Object, e As RoutedEventArgs)
            ' Display full name (whatever sidebar cache has)
            txtAssignTo.Text = CacheOnLoggedInName
            ' Resolve EmployeeID to save
            ResolveAssignedEmployeeID()
        End Sub

        ' Prefer Email; fallback to Name.
        Private Sub ResolveAssignedEmployeeID()
            AssignedEmployeeID = Nothing

            Dim email As String = CacheOnLoggedInEmail
            Dim nm As String = CacheOnLoggedInName

            If String.IsNullOrWhiteSpace(email) AndAlso String.IsNullOrWhiteSpace(nm) Then Return

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    If Not String.IsNullOrWhiteSpace(email) Then
                        Dim q1 As String = "SELECT EmployeeID FROM employee WHERE Email = @e LIMIT 1"
                        Using cmd As New MySqlCommand(q1, conn)
                            cmd.Parameters.AddWithValue("@e", email.Trim())
                            Dim result = cmd.ExecuteScalar()
                            If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                                AssignedEmployeeID = result.ToString()
                                Return
                            End If
                        End Using
                    End If

                    If Not String.IsNullOrWhiteSpace(nm) Then
                        Dim q2 As String = "SELECT EmployeeID FROM employee WHERE Name = @n LIMIT 1"
                        Using cmd As New MySqlCommand(q2, conn)
                            cmd.Parameters.AddWithValue("@n", nm.Trim())
                            Dim result = cmd.ExecuteScalar()
                            If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                                AssignedEmployeeID = result.ToString()
                                Return
                            End If
                        End Using
                    End If
                End Using
            Catch ex As Exception
                ' optional: MessageBox.Show(ex.Message)
            End Try
        End Sub

        Private Sub TxtToUpper_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtName.TextChanged
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            Dim originalSelectionStart = tb.SelectionStart
            Dim originalSelectionLength = tb.SelectionLength
            Dim originalText = tb.Text

            Dim upperText = originalText.ToUpperInvariant()

            If Not String.Equals(originalText, upperText, StringComparison.Ordinal) Then
                RemoveHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
                tb.Text = upperText
                tb.SelectionStart = Math.Min(originalSelectionStart, tb.Text.Length)
                tb.SelectionLength = originalSelectionLength
                AddHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
            End If
        End Sub

        Public Sub SetupDatePickers()
            StartDatePicker.DataContext = startDateViewModel
            StartDateButton.DataContext = startDateViewModel

            DueDatePicker.DataContext = dueDateViewModel
            DueDateButton.DataContext = dueDateViewModel
        End Sub

        Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(txtName.Text) Then
                MessageBox.Show("Project Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            If String.IsNullOrWhiteSpace(AssignedEmployeeID) Then
                ResolveAssignedEmployeeID()
            End If

            If String.IsNullOrWhiteSpace(AssignedEmployeeID) Then
                MessageBox.Show("Cannot determine EmployeeID for: " & txtAssignTo.Text, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim noteText As String = New TextRange(EditorBox.Document.ContentStart, EditorBox.Document.ContentEnd).Text.Trim()
            Dim selectedStatus = TryCast(cmbStatus.SelectedItem, StatusItem)

            Dim rawBudget As Long
            Long.TryParse(txtBudget.Text.Replace(",", ""), rawBudget)

            Dim proj As New DPC.Data.Model.Project With {
                .ProjectName = txtName.Text,
                .Status = If(selectedStatus IsNot Nothing, selectedStatus.Label, Nothing),
                .Customer = txtCustomer.Text,
                .Budget = rawBudget,
                .StartDate = StartDatePicker.SelectedDate,
                .DueDate = DueDatePicker.SelectedDate,
                .CalculationMode = If(RadBtnDueDateOnly.IsChecked, "Due Date Only", "Start to Due Date"),
                .LinkToCalendar = False,
                .AssignedTo = AssignedEmployeeID,
                .Note = noteText
            }

            Dim success As Boolean = DPC.Data.Controllers.ProjectController.CreateProject(proj)

            If success Then
                MessageBox.Show("Project added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                ClearFields()
            End If
        End Sub

        Private Sub ClearFields()
            ' Clear UI
            txtName.Clear()
            txtCustomer.Clear()
            txtBudget.Clear()
            cmbStatus.SelectedIndex = -1

            startDateViewModel.SelectedDate = Nothing
            dueDateViewModel.SelectedDate = Nothing
            StartDatePicker.SelectedDate = Nothing
            DueDatePicker.SelectedDate = Nothing

            EditorBox.Document.Blocks.Clear()
            RadBtnDueDateOnly.IsChecked = True

            txtAssignTo.Text = CacheOnLoggedInName
            ResolveAssignedEmployeeID()

            ' Clear Local Shared Memory
            _savedProjectName = ""
            _savedCustomer = ""
            _savedBudget = ""
            _savedStatusIndex = -1
            _savedStartDate = Nothing
            _savedDueDate = Nothing
            _savedDueDateOnlyChecked = True
            _savedNote = ""

            txtName.Focus()
        End Sub

        Private Sub txtBudget_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            RemoveHandler tb.TextChanged, AddressOf txtBudget_TextChanged

            Dim rawText As String = tb.Text.Replace(",", "").Trim()
            Dim number As Long

            If rawText = "" Then
                tb.Text = ""
            ElseIf Long.TryParse(rawText, number) Then
                Dim formatted As String = number.ToString("N0")
                Dim caretOffset As Integer = tb.Text.Length - tb.CaretIndex
                tb.Text = formatted
                Dim newCaret As Integer = Math.Max(0, formatted.Length - caretOffset)
                tb.CaretIndex = newCaret
            Else
                If tb.Text.Length > 0 Then
                    tb.Text = tb.Text.Remove(tb.Text.Length - 1)
                    tb.CaretIndex = tb.Text.Length
                End If
            End If

            AddHandler tb.TextChanged, AddressOf txtBudget_TextChanged
        End Sub

        ' =========================================================
        ' DATE PICKER CLICK HANDLERS (With Past-Date Restrictions)
        ' =========================================================
        Private Sub StartDateButton_Click(sender As Object, e As RoutedEventArgs) Handles StartDateButton.Click
            Dim minDate As DateTime = DateTime.Today
            If StartDatePicker.SelectedDate.HasValue AndAlso StartDatePicker.SelectedDate.Value < DateTime.Today Then
                minDate = StartDatePicker.SelectedDate.Value
            End If

            StartDatePicker.DisplayDateStart = minDate
            StartDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub DueDateButton_Click(sender As Object, e As RoutedEventArgs) Handles DueDateButton.Click
            Dim minDate As DateTime = DateTime.Today
            If DueDatePicker.SelectedDate.HasValue AndAlso DueDatePicker.SelectedDate.Value < DateTime.Today Then
                minDate = DueDatePicker.SelectedDate.Value
            End If

            DueDatePicker.DisplayDateStart = minDate
            DueDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub StartDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles StartDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    Dim be = BindingOperations.GetBindingExpression(StartDateButton, Button.DataContextProperty)
                    If be IsNot Nothing Then be.UpdateTarget()
                End If
            End If
        End Sub

        Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DueDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    Dim be = BindingOperations.GetBindingExpression(DueDateButton, Button.DataContextProperty)
                    If be IsNot Nothing Then be.UpdateTarget()
                End If
            End If
        End Sub

        ' (Rich Text Editor Formatting Buttons stay the exact same as you had them)
        Private Sub Format_Bold_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.ToggleBold.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Format_Italic_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.ToggleItalic.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Format_Underline_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.ToggleUnderline.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Format_Strike_Click(sender As Object, e As RoutedEventArgs)
            Dim selection = EditorBox.Selection
            If Not selection.IsEmpty Then
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough)
            End If
        End Sub

        Private Sub Format_Subscript_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(Typography.VariantsProperty, FontVariants.Subscript)
        End Sub

        Private Sub Format_Superscript_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(Typography.VariantsProperty, FontVariants.Superscript)
        End Sub

        Private Sub Format_TextColor_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.RoyalBlue)
        End Sub

        Private Sub Format_Highlight_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow)
        End Sub

        Private Sub Format_Clear_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ClearAllProperties()
        End Sub

        Private Sub FontSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If EditorBox Is Nothing OrElse cmbFontSize.SelectedItem Is Nothing Then Return
            Dim item As ComboBoxItem = TryCast(cmbFontSize.SelectedItem, ComboBoxItem)
            If item IsNot Nothing Then
                Dim size As Double
                If Double.TryParse(item.Content.ToString(), size) Then
                    EditorBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, size)
                End If
            End If
        End Sub

        Private Sub Format_AlignLeft_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.AlignLeft.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Format_AlignCenter_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.AlignCenter.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Format_AlignRight_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.AlignRight.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Format_AlignJustify_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.AlignJustify.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Format_List_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.ToggleBullets.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Format_IndentInc_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.IncreaseIndentation.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Format_IndentDec_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.DecreaseIndentation.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Insert_Quote_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.IncreaseIndentation.Execute(Nothing, EditorBox)
            EditingCommands.ToggleItalic.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Insert_Link_Click(sender As Object, e As RoutedEventArgs)
            Dim url As String = Microsoft.VisualBasic.Interaction.InputBox("Enter the URL:", "Insert Link", "http://")
            If String.IsNullOrWhiteSpace(url) Then Return
            If Not url.StartsWith("http") Then url = "http://" & url

            Dim link As New Hyperlink(New Run(url))
            link.NavigateUri = New Uri(url)
            AddHandler link.RequestNavigate, AddressOf Hyperlink_RequestNavigate

            If Not EditorBox.Selection.IsEmpty Then
                Dim selectedText As String = EditorBox.Selection.Text
                EditorBox.Selection.Text = ""
                Dim newRun As New Run(selectedText)
                Dim newLink As New Hyperlink(newRun)
                newLink.NavigateUri = New Uri(url)
                AddHandler newLink.RequestNavigate, AddressOf Hyperlink_RequestNavigate
                InsertLinkAtCaret(newLink)
            Else
                InsertLinkAtCaret(link)
            End If
        End Sub

        Private Sub InsertLinkAtCaret(link As Hyperlink)
            If EditorBox.CaretPosition.Paragraph IsNot Nothing Then
                EditorBox.CaretPosition.Paragraph.Inlines.Add(link)
            Else
                Dim para As New Paragraph(link)
                EditorBox.Document.Blocks.Add(para)
            End If
        End Sub

        Private Sub Hyperlink_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
            System.Diagnostics.Process.Start(New System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) With {.UseShellExecute = True})
            e.Handled = True
        End Sub

        Private Sub Insert_Image_Click(sender As Object, e As RoutedEventArgs)
            Dim openFileDialog As New OpenFileDialog()
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            If openFileDialog.ShowDialog() = True Then
                Try
                    Dim bitmap As New BitmapImage()
                    bitmap.BeginInit()
                    bitmap.UriSource = New Uri(openFileDialog.FileName)
                    bitmap.CacheOption = BitmapCacheOption.OnLoad
                    bitmap.EndInit()

                    Dim img As New Image()
                    img.Source = bitmap
                    img.Width = 300
                    img.Stretch = Stretch.Uniform

                    Dim container As New InlineUIContainer(img, EditorBox.CaretPosition)
                Catch ex As Exception
                    MessageBox.Show("Unable to insert this image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        Private Sub Insert_Code_Click(sender As Object, e As RoutedEventArgs)
            Dim range As New TextRange(EditorBox.Selection.Start, EditorBox.Selection.End)
            range.ApplyPropertyValue(TextElement.FontFamilyProperty, New FontFamily("Consolas"))
            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.LightGray)
            range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black)
        End Sub

        Private isFullscreen As Boolean = False
        Private Sub Toggle_Fullscreen_Click(sender As Object, e As RoutedEventArgs)
            isFullscreen = Not isFullscreen
            If isFullscreen Then
                EditorBox.Height = 600
                EditorBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible
            Else
                EditorBox.Height = 250
            End If
        End Sub

        Private Sub cmbStatus_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbStatus.SelectionChanged
        End Sub

    End Class

    Public Class StatusItem
        Public Property Label As String
        Public Property Color As SolidColorBrush
    End Class
End Namespace