Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
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

        Public Sub New()
            InitializeComponent()
            ' Set the Panel to be invisible by default
            Me.Visibility = Visibility.Collapsed
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

        ' Add keyboard support - close on Escape key
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Key = Key.Escape Then
                HideDialog()
                e.Handled = True
            End If
        End Sub

        ' Helper methods for common dialog scenarios

        ' Show standard success dialog
        Public Shared Function ShowSuccess(parent As Panel, message As String, Optional buttonText As String = "Got It!", Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As New DynamicDialogs()
            parent.Children.Add(dialog)
            dialog.ShowDialog(DialogType.Success, message, primaryButtonText:=buttonText, data:=data)
            Return dialog
        End Function

        ' Show standard error dialog
        Public Shared Function ShowError(parent As Panel, message As String, Optional buttonText As String = "Try Again", Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As New DynamicDialogs()
            parent.Children.Add(dialog)
            dialog.ShowDialog(DialogType.[Error], message, primaryButtonText:=buttonText, data:=data)
            Return dialog
        End Function

        ' Show conditional dialog based on a condition
        Public Shared Function ShowConditional(parent As Panel, condition As Boolean,
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
        Public Shared Function ShowConfirmation(parent As Panel, message As String,
                                               Optional title As String = "Confirm",
                                               Optional yesButtonText As String = "Yes",
                                               Optional noButtonText As String = "No",
                                               Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As New DynamicDialogs()
            parent.Children.Add(dialog)
            dialog.ShowDialog(DialogType.Question, message, title, yesButtonText, noButtonText, data)
            Return dialog
        End Function

        ' Show information dialog
        Public Shared Function ShowInformation(parent As Panel, message As String,
                                             Optional title As String = "Information",
                                             Optional buttonText As String = "OK",
                                             Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As New DynamicDialogs()
            parent.Children.Add(dialog)
            dialog.ShowDialog(DialogType.Information, message, title, buttonText, data:=data)
            Return dialog
        End Function

        ' Show warning dialog
        Public Shared Function ShowWarning(parent As Panel, message As String,
                                         Optional title As String = "Warning",
                                         Optional buttonText As String = "OK",
                                         Optional data As Object = Nothing) As DynamicDialogs
            Dim dialog As New DynamicDialogs()
            parent.Children.Add(dialog)
            dialog.ShowDialog(DialogType.Warning, message, title, buttonText, data:=data)
            Return dialog
        End Function
    End Class
End Namespace