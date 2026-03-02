Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Controllers
Imports System.Windows.Documents
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports Microsoft.Win32 ' Required for OpenFileDialog
Imports System.Windows.Navigation

Namespace DPC.Views.Project
    Partial Public Class AddProject1
        Inherits UserControl

        ' ViewModels for the custom date pickers
        Private startDateViewModel As New CalendarController.SingleCalendar()
        Private dueDateViewModel As New CalendarController.SingleCalendar()

        Public Sub New()
            InitializeComponent()
            SetupDatePickers() ' Initialize the date picker contexts

            ' Populate Status ComboBox with colored indicators
            Dim statuses As New List(Of StatusItem) From {
                New StatusItem With {.Label = "Pending", .Color = New SolidColorBrush(Color.FromRgb(255, 193, 7))},   ' Amber
                New StatusItem With {.Label = "In Progress", .Color = New SolidColorBrush(Color.FromRgb(33, 150, 243))},  ' Blue
                New StatusItem With {.Label = "Completed", .Color = New SolidColorBrush(Color.FromRgb(76, 175, 80))},   ' Green
                New StatusItem With {.Label = "Cancelled", .Color = New SolidColorBrush(Color.FromRgb(244, 67, 54))}    ' Red
            }
            cmbStatus.ItemsSource = statuses
            cmbStatus.SelectedIndex = -1
        End Sub

        ' =========================================================
        ' SECTION 1: TEXT HANDLING (Uppercase Project Name)
        ' =========================================================

        ' Ensure project name is Uppercase while preserving caret position
        Private Sub TxtToUpper_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtName.TextChanged
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            Dim originalSelectionStart = tb.SelectionStart
            Dim originalSelectionLength = tb.SelectionLength
            Dim originalText = tb.Text

            Dim upperText = originalText.ToUpperInvariant()

            ' Only update if there is a change to avoid infinite loops
            If Not String.Equals(originalText, upperText, StringComparison.Ordinal) Then
                RemoveHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
                tb.Text = upperText
                tb.SelectionStart = Math.Min(originalSelectionStart, tb.Text.Length)
                tb.SelectionLength = originalSelectionLength
                AddHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
            End If
        End Sub

        ' =========================================================
        ' SECTION 2: DATE PICKER LOGIC
        ' =========================================================

        ' Setup bindings between DatePickers, Buttons, and ViewModels
        Public Sub SetupDatePickers()
            startDateViewModel.SelectedDate = Nothing
            dueDateViewModel.SelectedDate = Nothing

            ' Bind the DataContexts for the hidden pickers and visible buttons
            StartDatePicker.DataContext = startDateViewModel
            StartDateButton.DataContext = startDateViewModel

            DueDatePicker.DataContext = dueDateViewModel
            DueDateButton.DataContext = dueDateViewModel
        End Sub

        Private Sub txtBudget_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            ' Detach handler to prevent infinite loop
            RemoveHandler tb.TextChanged, AddressOf txtBudget_TextChanged

            ' Strip everything except digits
            Dim rawText As String = tb.Text.Replace(",", "").Trim()

            ' Handle empty or non-numeric input gracefully
            Dim number As Long
            If rawText = "" Then
                tb.Text = ""
            ElseIf Long.TryParse(rawText, number) Then
                ' Format with commas
                Dim formatted As String = number.ToString("N0")
                Dim caretOffset As Integer = tb.Text.Length - tb.CaretIndex

                tb.Text = formatted

                ' Restore caret position intelligently
                Dim newCaret As Integer = Math.Max(0, formatted.Length - caretOffset)
                tb.CaretIndex = newCaret
            Else
                ' Non-numeric character typed — revert to last valid value
                tb.Text = tb.Text.Remove(tb.Text.Length - 1)
                tb.CaretIndex = tb.Text.Length
            End If

            ' Re-attach handler
            AddHandler tb.TextChanged, AddressOf txtBudget_TextChanged
        End Sub

        ' Open the hidden DatePicker dropdown when the custom button is clicked
        Private Sub StartDateButton_Click(sender As Object, e As RoutedEventArgs) Handles StartDateButton.Click
            StartDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub DueDateButton_Click(sender As Object, e As RoutedEventArgs) Handles DueDateButton.Click
            DueDatePicker.IsDropDownOpen = True
        End Sub

        ' Sync the ViewModel when the Start Date changes
        Private Sub StartDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles StartDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    ' Force the button binding to update its text
                    Dim be = BindingOperations.GetBindingExpression(StartDateButton, Button.DataContextProperty)
                    If be IsNot Nothing Then be.UpdateTarget()
                End If
            End If
        End Sub

        ' Sync the ViewModel when the Due Date changes
        Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DueDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    ' Force the button binding to update its text
                    Dim be = BindingOperations.GetBindingExpression(DueDateButton, Button.DataContextProperty)
                    If be IsNot Nothing Then be.UpdateTarget()
                End If
            End If
        End Sub

        ' =========================================================
        ' SECTION 3: SUBMIT LOGIC
        ' =========================================================

        Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
            ' Collect field values
            Dim projectName = txtName.Text
            Dim customer = txtCustomer.Text

            ' Parse budget (strip commas before saving)
            Dim rawBudget As Long
            Long.TryParse(txtBudget.Text.Replace(",", ""), rawBudget)

            Dim selectedStatus = TryCast(cmbStatus.SelectedItem, StatusItem)
            Dim statusLabel = selectedStatus?.Label

            ' ... Save to database ...
        End Sub

        Private Sub cmbStatus_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbStatus.SelectionChanged

        End Sub
        ' =========================================================
        ' SECTION 4: RICH TEXT EDITOR LOGIC
        ' =========================================================

        ' --- Text Formatting ---
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
            ' Toggle Strikethrough by checking current decorations
            Dim selection = EditorBox.Selection
            If selection.IsEmpty Then Return

            Dim currentDecor = selection.GetPropertyValue(Inline.TextDecorationsProperty)
            If currentDecor Is DependencyProperty.UnsetValue OrElse currentDecor Is Nothing Then
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough)
            Else
                ' If strictly implementing toggle logic is complex, generally re-applying standard clears it
                ' For simplicity, we apply Strikethrough directly
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough)
            End If
        End Sub

        Private Sub Format_Subscript_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(Typography.VariantsProperty, FontVariants.Subscript)
        End Sub

        Private Sub Format_Superscript_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(Typography.VariantsProperty, FontVariants.Superscript)
        End Sub

        ' --- Styling (Color/Highlight) ---
        Private Sub Format_TextColor_Click(sender As Object, e As RoutedEventArgs)
            ' Logic: Opens a Color Dialog in a real app. 
            ' Demo: Toggles to a preset color (e.g., Blue)
            EditorBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.RoyalBlue)
        End Sub

        Private Sub Format_Highlight_Click(sender As Object, e As RoutedEventArgs)
            ' Demo: Toggles to Yellow highlight
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

        ' --- Alignment & Lists ---
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

        ' --- Inserts ---
        Private Sub Insert_Link_Click(sender As Object, e As RoutedEventArgs)
            Dim url As String = Microsoft.VisualBasic.Interaction.InputBox("Enter the URL:", "Insert Link", "http://")

            If String.IsNullOrWhiteSpace(url) Then Return

            ' Ensure the URL is valid
            If Not url.StartsWith("http") Then url = "http://" & url

            ' Create the hyperlink object
            Dim link As New Hyperlink(New Run(url))
            link.NavigateUri = New Uri(url)

            ' IMPORTANT: Handle the click event to open the browser
            AddHandler link.RequestNavigate, AddressOf Hyperlink_RequestNavigate

            ' Insert the link at the current caret position
            ' We wrap it in a Span to insert it safely as an inline element
            Dim span As New Span(link)

            ' Check if we are currently selecting text to replace, or just inserting
            If Not EditorBox.Selection.IsEmpty Then
                ' If text is selected, replace it with the link (using the selected text as the link text)
                Dim selectedText As String = EditorBox.Selection.Text
                link.Inlines.Clear()
                link.Inlines.Add(New Run(selectedText))

                ' Replace selection
                Dim range As New TextRange(EditorBox.Selection.Start, EditorBox.Selection.End)
                range.Text = "" ' Clear existing text


                EditorBox.CaretPosition.InsertTextInRun("") ' Split current run if needed
                Dim textPointer As TextPointer = EditorBox.CaretPosition
                Dim newRun As New Run(selectedText)
                Dim newLink As New Hyperlink(newRun)
                newLink.NavigateUri = New Uri(url)
                AddHandler newLink.RequestNavigate, AddressOf Hyperlink_RequestNavigate

                ' We use a slightly different approach for replacement:
                ' 1. Delete selection
                EditorBox.Selection.Text = ""
                ' 2. Insert new link at cursor
                InsertLinkAtCaret(newLink)
            Else
                ' Just insert at cursor
                InsertLinkAtCaret(link)
            End If
        End Sub

        ' Helper to insert the link object
        Private Sub InsertLinkAtCaret(link As Hyperlink)
            If EditorBox.CaretPosition.Paragraph IsNot Nothing Then
                ' Simple insertion if we are inside a paragraph
                EditorBox.CaretPosition.Paragraph.Inlines.Add(link)
            Else
                ' Fallback: Insert a new paragraph with the link
                Dim para As New Paragraph(link)
                EditorBox.Document.Blocks.Add(para)
            End If
        End Sub

        ' This event actually opens the browser
        Private Sub Hyperlink_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
            System.Diagnostics.Process.Start(New System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) With {.UseShellExecute = True})
            e.Handled = True
        End Sub

        Private Sub Insert_Image_Click(sender As Object, e As RoutedEventArgs)
            Dim openFileDialog As New OpenFileDialog()
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            openFileDialog.Title = "Select an Image"

            If openFileDialog.ShowDialog() = True Then
                Try
                    ' Create the bitmap from the selected file
                    Dim bitmap As New BitmapImage()
                    bitmap.BeginInit()
                    bitmap.UriSource = New Uri(openFileDialog.FileName)
                    bitmap.CacheOption = BitmapCacheOption.OnLoad ' Important to release file lock
                    bitmap.EndInit()

                    ' Create the Image element
                    Dim img As New Image()
                    img.Source = bitmap
                    img.Width = 300 ' Default width, can be adjusted
                    img.Stretch = Stretch.Uniform

                    ' FIX: Pass the CaretPosition to the constructor to insert at cursor
                    Dim container As New InlineUIContainer(img, EditorBox.CaretPosition)
                Catch ex As Exception
                    MessageBox.Show("Unable to insert this image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        Private Sub Insert_Quote_Click(sender As Object, e As RoutedEventArgs)
            ' Apply italics and indentation to simulate a blockquote
            EditingCommands.IncreaseIndentation.Execute(Nothing, EditorBox)
            EditingCommands.ToggleItalic.Execute(Nothing, EditorBox)
        End Sub

        Private Sub Insert_Code_Click(sender As Object, e As RoutedEventArgs)
            ' Format selection as Code (Consolas, Grey Background)
            Dim range As New TextRange(EditorBox.Selection.Start, EditorBox.Selection.End)
            range.ApplyPropertyValue(TextElement.FontFamilyProperty, New FontFamily("Consolas"))
            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.LightGray)
            range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black)
        End Sub

        ' --- Misc ---
        Private isFullscreen As Boolean = False
        Private Sub Toggle_Fullscreen_Click(sender As Object, e As RoutedEventArgs)
            isFullscreen = Not isFullscreen
            If isFullscreen Then
                ' Simple fullscreen simulation: Expand height greatly
                EditorBox.Height = 600
                EditorBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible
            Else
                EditorBox.Height = 250
            End If
        End Sub
    End Class

    Public Class StatusItem
        Public Property Label As String
        Public Property Color As SolidColorBrush
    End Class

End Namespace