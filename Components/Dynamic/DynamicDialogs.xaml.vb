Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.ComponentModel
Imports MaterialDesignThemes.Wpf

Namespace DPC.Components.Dynamic
    Public Class DynamicDialogs
        Inherits UserControl

        ' Event handlers for dialog actions
        Public Event PrimaryAction(sender As Object, e As DialogEventArgs)
        Public Event SecondaryAction(sender As Object, e As DialogEventArgs)
        Public Event DialogClosed(sender As Object, e As DialogEventArgs)

        ' Custom data that can be passed through the dialog
        Private _dialogData As Object
        Public Property DialogData As Object
            Get
                Return _dialogData
            End Get
            Set(value As Object)
                _dialogData = value
            End Set
        End Property

        ' Enum for dialog types
        Public Enum DialogType
            Success
            [Error] ' Using brackets to escape the reserved keyword
            Warning
            Information
            Question
            Custom
        End Enum

        ' Custom event args to pass with events
        Public Class DialogEventArgs
            Inherits EventArgs

            Public Property Result As Boolean
            Public Property Data As Object

            Public Sub New(result As Boolean, data As Object)
                Me.Result = result
                Me.Data = data
            End Sub
        End Class

        ' Colors for different dialog types
        Private ReadOnly SuccessColor As New SolidColorBrush(Color.FromRgb(109, 192, 103))   ' #6DC067
        Private ReadOnly ErrorColor As New SolidColorBrush(Color.FromRgb(224, 93, 93))       ' #E05D5D
        Private ReadOnly WarningColor As New SolidColorBrush(Color.FromRgb(240, 173, 78))    ' #F0AD4E
        Private ReadOnly InfoColor As New SolidColorBrush(Color.FromRgb(91, 192, 222))       ' #5BC0DE
        Private ReadOnly QuestionColor As New SolidColorBrush(Color.FromRgb(70, 130, 180))   ' #4682B4

        ' Icons for different dialog types
        Private ReadOnly SuccessIcon As PackIconKind = PackIconKind.Check
        Private ReadOnly ErrorIcon As PackIconKind = PackIconKind.Close
        Private ReadOnly WarningIcon As PackIconKind = PackIconKind.Alert
        Private ReadOnly InfoIcon As PackIconKind = PackIconKind.Information
        Private ReadOnly QuestionIcon As PackIconKind = PackIconKind.HelpCircleOutline

        ' Static property to access the active instance
        Private Shared _activeInstance As DynamicDialogs = Nothing
        Public Shared Property ActiveInstance As DynamicDialogs
            Get
                Return _activeInstance
            End Get
            Private Set(value As DynamicDialogs)
                _activeInstance = value
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
            ' Set the control to be invisible by default
            Me.Visibility = Visibility.Collapsed

            ' Add handler for keyboard input
            AddHandler Loaded, AddressOf Dialog_Loaded
        End Sub

        Private Sub Dialog_Loaded(sender As Object, e As RoutedEventArgs)
            ' Ensure we can receive keyboard focus
            Me.Focusable = True

            ' Add event handler for all key presses at application level
            Dim parentWindow = Window.GetWindow(Me)
            If parentWindow IsNot Nothing Then
                AddHandler parentWindow.KeyDown, AddressOf Window_KeyDown
            End If
        End Sub

        Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
            ' Only process Escape key if dialog is visible
            If Me.Visibility = Visibility.Visible AndAlso e.Key = Key.Escape Then
                HideDialog()
                e.Handled = True
            End If
        End Sub

        Protected Overrides Sub OnVisualParentChanged(oldParent As DependencyObject)
            MyBase.OnVisualParentChanged(oldParent)

            ' If we're removing from the visual tree, unsubscribe from events
            If oldParent IsNot Nothing Then
                Dim parentWindow = Window.GetWindow(oldParent)
                If parentWindow IsNot Nothing Then
                    RemoveHandler parentWindow.KeyDown, AddressOf Window_KeyDown
                End If
            End If
        End Sub

        ' Show a dialog with specified type and message
        Public Function ShowDialog(dialogType As DialogType, message As String,
                                  Optional title As String = Nothing,
                                  Optional primaryButtonText As String = Nothing,
                                  Optional secondaryButtonText As String = Nothing,
                                  Optional data As Object = Nothing) As DynamicDialogs

            ' Set dialog data
            DialogData = data

            ' Set default title and button text based on dialog type if not provided
            Select Case dialogType
                Case DialogType.Success
                    SetDialogAppearance(SuccessColor, SuccessIcon)
                    If String.IsNullOrEmpty(title) Then title = "Success!"
                    If String.IsNullOrEmpty(primaryButtonText) Then primaryButtonText = "Got It!"

                Case DialogType.[Error]
                    SetDialogAppearance(ErrorColor, ErrorIcon)
                    If String.IsNullOrEmpty(title) Then title = "Whoops!"
                    If String.IsNullOrEmpty(primaryButtonText) Then primaryButtonText = "Try Again"

                Case DialogType.Warning
                    SetDialogAppearance(WarningColor, WarningIcon)
                    If String.IsNullOrEmpty(title) Then title = "Warning!"
                    If String.IsNullOrEmpty(primaryButtonText) Then primaryButtonText = "OK"

                Case DialogType.Information
                    SetDialogAppearance(InfoColor, InfoIcon)
                    If String.IsNullOrEmpty(title) Then title = "Information"
                    If String.IsNullOrEmpty(primaryButtonText) Then primaryButtonText = "OK"

                Case DialogType.Question
                    SetDialogAppearance(QuestionColor, QuestionIcon)
                    If String.IsNullOrEmpty(title) Then title = "Confirm"
                    If String.IsNullOrEmpty(primaryButtonText) Then primaryButtonText = "Yes"
                    If String.IsNullOrEmpty(secondaryButtonText) Then secondaryButtonText = "No"
                    ' For questions, always show the secondary button by default
                    SecondaryButton.Visibility = Visibility.Visible

                Case DialogType.Custom
                    ' For custom, caller must specify all parameters
            End Select

            ' Update dialog content
            DialogTitle.Text = title
            DialogMessage.Text = message
            PrimaryButton.Content = primaryButtonText

            ' Configure secondary button if text is provided
            If Not String.IsNullOrEmpty(secondaryButtonText) Then
                SecondaryButton.Content = secondaryButtonText
                SecondaryButton.Visibility = Visibility.Visible
            Else
                SecondaryButton.Visibility = Visibility.Collapsed
            End If

            ' Show the dialog
            Me.Visibility = Visibility.Visible
            DialogModalOverlay.Visibility = Visibility.Visible

            ' Add focus to primary button for keyboard navigation
            PrimaryButton.Focus()

            ' Set as active instance
            ActiveInstance = Me

            Return Me
        End Function

        ' Set dialog color theme and icon
        Private Sub SetDialogAppearance(color As SolidColorBrush, icon As PackIconKind)
            DialogHeader.Background = color
            DialogIcon.Kind = icon
            DialogIcon.Foreground = color
        End Sub

        ' Set custom dialog appearance (for DialogType.Custom)
        Public Sub SetCustomAppearance(headerColor As Color, iconKind As PackIconKind)
            Dim customColor As New SolidColorBrush(headerColor)
            SetDialogAppearance(customColor, iconKind)
        End Sub

        ' Hide dialog with animation
        Public Sub HideDialog()
            ' Clear active instance if this is the active one
            If ActiveInstance Is Me Then
                ActiveInstance = Nothing
            End If

            ' Create fade out animation
            Dim fadeOut As New DoubleAnimation With {
                .From = 1,
                .To = 0,
                .Duration = New Duration(TimeSpan.FromSeconds(0.2))
            }

            ' Add completed event handler to update visibility after animation
            AddHandler fadeOut.Completed, Sub(s, e)
                                              DialogModalOverlay.Visibility = Visibility.Collapsed
                                              Me.Visibility = Visibility.Collapsed
                                              RaiseEvent DialogClosed(Me, New DialogEventArgs(False, DialogData))
                                          End Sub

            ' Apply animation
            DialogModalOverlay.BeginAnimation(Grid.OpacityProperty, fadeOut)
        End Sub

        ' Event handlers for buttons
        Private Sub PrimaryButton_Click(sender As Object, e As RoutedEventArgs)
            HideDialog()
            RaiseEvent PrimaryAction(Me, New DialogEventArgs(True, DialogData))
        End Sub

        Private Sub SecondaryButton_Click(sender As Object, e As RoutedEventArgs)
            HideDialog()
            RaiseEvent SecondaryAction(Me, New DialogEventArgs(False, DialogData))
        End Sub

        Private Sub BtnCloseDialog_Click(sender As Object, e As RoutedEventArgs)
            HideDialog()
        End Sub

        ' Helper methods for showing dialogs using different parent container types

        ' Type-checking helper for parent containers
        Private Shared Function GetAppropriateContainer(parent As Object) As DependencyObject
            If parent Is Nothing Then
                ' If no parent specified, try to find the application's main window
                If Application.Current IsNot Nothing AndAlso Application.Current.MainWindow IsNot Nothing Then
                    Return Application.Current.MainWindow
                Else
                    Throw New ArgumentException("No valid parent container provided and no main window found.")
                End If
            End If

            ' Validate parent type
            Dim container As DependencyObject

            If TypeOf parent Is Window Then
                ' For windows, get the content
                container = DirectCast(parent, Window)
            ElseIf TypeOf parent Is UserControl Then
                ' For UserControls
                container = DirectCast(parent, UserControl)
            ElseIf TypeOf parent Is Panel Then
                ' For Panels
                container = DirectCast(parent, Panel)
            ElseIf TypeOf parent Is ContentControl Then
                ' For ContentControls
                container = DirectCast(parent, ContentControl)
            ElseIf TypeOf parent Is FrameworkElement Then
                ' For any other FrameworkElement
                container = DirectCast(parent, FrameworkElement)
            Else
                Throw New ArgumentException("Unsupported parent container type: " & parent.GetType().Name)
            End If

            Return container
        End Function

        ' Find a suitable parent container for the dialog
        Private Shared Function FindPanelContainer(parent As DependencyObject) As Panel
            ' Get the visual tree helper
            Dim targetPanel As Panel = Nothing

            ' First try to find a Grid or Panel in the parent
            If TypeOf parent Is Panel Then
                targetPanel = DirectCast(parent, Panel)
            ElseIf TypeOf parent Is ContentControl Then
                Dim contentControl = DirectCast(parent, ContentControl)
                If TypeOf contentControl.Content Is Panel Then
                    targetPanel = DirectCast(contentControl.Content, Panel)
                End If
            ElseIf TypeOf parent Is Window Then
                Dim window = DirectCast(parent, Window)
                If TypeOf window.Content Is Panel Then
                    targetPanel = DirectCast(window.Content, Panel)
                End If
            End If

            ' If no suitable panel was found, look for a Grid
            If targetPanel Is Nothing Then
                targetPanel = FindVisualChild(Of Grid)(parent)
            End If

            ' If still no suitable panel, create an AdornerLayer
            If targetPanel Is Nothing Then
                ' Look for any panel
                targetPanel = FindVisualChild(Of Panel)(parent)
            End If

            ' If still no panel found, throw error
            If targetPanel Is Nothing Then
                Throw New InvalidOperationException("Could not find or create a suitable container for the dialog.")
            End If

            Return targetPanel
        End Function

        ' Helper to find a child element of a specific type
        Private Shared Function FindVisualChild(Of T As DependencyObject)(parent As DependencyObject) As T
            Dim childCount As Integer = VisualTreeHelper.GetChildrenCount(parent)
            For i As Integer = 0 To childCount - 1
                Dim child As DependencyObject = VisualTreeHelper.GetChild(parent, i)
                If TypeOf child Is T Then
                    Return DirectCast(child, T)
                Else
                    Dim result As T = FindVisualChild(Of T)(child)
                    If result IsNot Nothing Then
                        Return result
                    End If
                End If
            Next
            Return Nothing
        End Function

        ' Create a dialog instance and add it to the specified parent
        Private Shared Function CreateDialogInstance(parent As Object) As DynamicDialogs
            ' Get the appropriate container
            Dim container = GetAppropriateContainer(parent)

            ' Find a suitable panel to host the dialog
            Dim targetPanel = FindPanelContainer(container)

            ' Create the dialog instance
            Dim dialog As New DynamicDialogs()

            ' Add to the target panel
            targetPanel.Children.Add(dialog)

            ' Ensure the dialog is on top
            Panel.SetZIndex(dialog, 1000)

            Return dialog
        End Function

        ' Show standard success dialog
        Public Shared Function ShowSuccess(parent As Object, message As String, Optional buttonText As String = "Got It!", Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As DynamicDialogs = CreateDialogInstance(parent)
            dialog.ShowDialog(DialogType.Success, message, primaryButtonText:=buttonText, data:=data)
            Return dialog
        End Function

        ' Show standard error dialog
        Public Shared Function ShowError(parent As Object, message As String, Optional buttonText As String = "Try Again", Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As DynamicDialogs = CreateDialogInstance(parent)
            dialog.ShowDialog(DialogType.[Error], message, primaryButtonText:=buttonText, data:=data)
            Return dialog
        End Function

        ' Show conditional dialog based on a condition
        Public Shared Function ShowConditional(parent As Object, condition As Boolean,
                                              successMessage As String, errorMessage As String,
                                              Optional successButtonText As String = "Got It!",
                                              Optional errorButtonText As String = "Try Again",
                                              Optional data As Object = Nothing) As DynamicDialogs
            If condition Then
                Return ShowSuccess(parent, successMessage, successButtonText, data)
            Else
                Return ShowError(parent, errorMessage, errorButtonText, data)
            End If
        End Function

        ' Show confirmation dialog (Yes/No)
        Public Shared Function ShowConfirmation(parent As Object, message As String,
                                               Optional title As String = "Confirm",
                                               Optional yesButtonText As String = "Yes",
                                               Optional noButtonText As String = "No",
                                               Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As DynamicDialogs = CreateDialogInstance(parent)
            dialog.ShowDialog(DialogType.Question, message, title, yesButtonText, noButtonText, data)
            Return dialog
        End Function

        ' Show information dialog
        Public Shared Function ShowInformation(parent As Object, message As String,
                                             Optional title As String = "Information",
                                             Optional buttonText As String = "OK",
                                             Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As DynamicDialogs = CreateDialogInstance(parent)
            dialog.ShowDialog(DialogType.Information, message, title, buttonText, data:=data)
            Return dialog
        End Function

        ' Show warning dialog
        Public Shared Function ShowWarning(parent As Object, message As String,
                                         Optional title As String = "Warning",
                                         Optional buttonText As String = "OK",
                                         Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As DynamicDialogs = CreateDialogInstance(parent)
            dialog.ShowDialog(DialogType.Warning, message, title, buttonText, data:=data)
            Return dialog
        End Function
    End Class
End Namespace