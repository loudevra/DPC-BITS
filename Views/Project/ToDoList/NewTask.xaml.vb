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

Public Class NewTask
    Inherits UserControl

    Private startDateViewModel As New CalendarController.SingleCalendar()
    Private dueDateViewModel As New CalendarController.SingleCalendar()

    ' Store EmployeeID for saving (fits VARCHAR(20))
    Private AssignedEmployeeID As String = Nothing

    Public Sub New()
        InitializeComponent()
        SetupDatePickers()

        ' Updated to match your specific Task statuses and dashboard colors
        Dim statuses As New List(Of StatusItem) From {
            New StatusItem With {.Label = "Due", .Color = New SolidColorBrush(Color.FromRgb(199, 87, 87))},
            New StatusItem With {.Label = "Progress", .Color = New SolidColorBrush(Color.FromRgb(134, 188, 213))},
            New StatusItem With {.Label = "Done", .Color = New SolidColorBrush(Color.FromRgb(137, 172, 116))}
        }

        cmbStatus.ItemsSource = statuses
        cmbStatus.SelectedIndex = -1

        AddHandler Me.Loaded, AddressOf NewTask_Loaded
    End Sub

    Private Sub NewTask_Loaded(sender As Object, e As RoutedEventArgs)
        ' Display full name (whatever sidebar cache has)
        txtAssignTo.Text = CacheOnLoggedInName

        ' Resolve EmployeeID to save
        ResolveAssignedEmployeeID()
    End Sub

    Private Sub ResolveAssignedEmployeeID()
        Dim nm As String = CacheOnLoggedInName

        If Not String.IsNullOrWhiteSpace(nm) Then
            AssignedEmployeeID = nm
        Else
            AssignedEmployeeID = "Unassigned"
        End If
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
        startDateViewModel.SelectedDate = Nothing
        dueDateViewModel.SelectedDate = Nothing

        StartDatePicker.DataContext = startDateViewModel
        StartDateButton.DataContext = startDateViewModel

        DueDatePicker.DataContext = dueDateViewModel
        DueDateButton.DataContext = dueDateViewModel
    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            MessageBox.Show("Task Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        If String.IsNullOrWhiteSpace(AssignedEmployeeID) Then
            ResolveAssignedEmployeeID()
        End If

        Dim noteText As String = New TextRange(EditorBox.Document.ContentStart, EditorBox.Document.ContentEnd).Text.Trim()
        Dim selectedStatus = TryCast(cmbStatus.SelectedItem, StatusItem)

        ' =========================================================
        ' SAVE TASK TO IN-MEMORY LIST (No Database)
        ' =========================================================
        Dim newTask As New TaskModel()

        ' Simulate an Auto-Increment ID
        newTask.TaskID = GlobalTaskStore.TaskList.Count + 1

        newTask.Task = txtName.Text
        newTask.Status = If(selectedStatus IsNot Nothing, selectedStatus.Label, "Due")

        ' Format Start Date
        If StartDatePicker.SelectedDate.HasValue Then
            newTask.Start = StartDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy")
        Else
            newTask.Start = "-"
        End If

        ' Format Due Date
        If DueDatePicker.SelectedDate.HasValue Then
            newTask.DueDate = DueDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy")
        Else
            newTask.DueDate = "-"
        End If

        ' Add the newly created task to the global shared list directly
        GlobalTaskStore.TaskList.Add(newTask)
        ' =========================================================

        MessageBox.Show("Task added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
        ClearFields()
    End Sub

    Private Sub ClearFields()
        txtName.Clear()
        cmbStatus.SelectedIndex = -1

        startDateViewModel.SelectedDate = Nothing
        dueDateViewModel.SelectedDate = Nothing
        StartDatePicker.SelectedDate = Nothing
        DueDatePicker.SelectedDate = Nothing

        EditorBox.Document.Blocks.Clear()

        txtAssignTo.Text = CacheOnLoggedInName
        ResolveAssignedEmployeeID()

        txtName.Focus()
    End Sub

    Private Sub StartDateButton_Click(sender As Object, e As RoutedEventArgs) Handles StartDateButton.Click
        StartDatePicker.IsDropDownOpen = True
    End Sub

    Private Sub DueDateButton_Click(sender As Object, e As RoutedEventArgs) Handles DueDateButton.Click
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