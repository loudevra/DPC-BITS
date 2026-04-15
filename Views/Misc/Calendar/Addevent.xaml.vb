Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Public Class Addevent
    ' Signal to tell the main window to swap back to the calendar
    Public Event OnSaveCompleted()

    Private Sub BtnSave_Click(sender As Object, e As RoutedEventArgs)

        ' Validation
        If String.IsNullOrWhiteSpace(TxtEventTitle.Text) OrElse DpEventDate.SelectedDate Is Nothing OrElse CmbEventType.SelectedItem Is Nothing Then
            MessageBox.Show("Please fill out all fields before saving.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        ' Parse the pastel color from the ComboBox Tag
        Dim selectedItem As ComboBoxItem = CType(CmbEventType.SelectedItem, ComboBoxItem)
        Dim colorHex As String = selectedItem.Tag.ToString()
        Dim brushConverter As New BrushConverter()
        Dim eventColor As SolidColorBrush = CType(brushConverter.ConvertFrom(colorHex), SolidColorBrush)

        ' Create the AppEvent object
        Dim newEvent As New AppEvent() With {
            .Title = TxtEventTitle.Text,
            .EventDate = DpEventDate.SelectedDate.Value,
            .EventColor = eventColor
        }

        ' Save it
        EventStore.AddNewEvent(newEvent)
        MessageBox.Show("Event added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

        ' Clear fields
        TxtEventTitle.Clear()
        DpEventDate.SelectedDate = Nothing
        CmbEventType.SelectedIndex = -1

        ' Return to calendar view
        RaiseEvent OnSaveCompleted()

    End Sub
End Class