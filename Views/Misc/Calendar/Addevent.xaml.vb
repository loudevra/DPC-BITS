Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace DPC.Views.Misc.Calendar

    Public Class Addevent

        Public Event OnSaveCompleted()

        Private Sub BtnSave_Click(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(TxtEventTitle.Text) OrElse
               DpEventDate.SelectedDate Is Nothing OrElse
               CmbEventType.SelectedItem Is Nothing Then
                MessageBox.Show("Please fill out all fields before saving.",
                                "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedItem As ComboBoxItem = CType(CmbEventType.SelectedItem, ComboBoxItem)
            Dim category As String = selectedItem.Content.ToString()

            Dim newEvent As New AppEvent() With {
                .Title = TxtEventTitle.Text,
                .EventDate = DpEventDate.SelectedDate.Value,
                .Category = category,
                .EventColor = AppEvent.GetColorForCategory(category)
            }

            EventStore.AddNewEvent(newEvent)

            MessageBox.Show("Event added successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information)

            TxtEventTitle.Clear()
            DpEventDate.SelectedDate = Nothing
            CmbEventType.SelectedIndex = -1

            RaiseEvent OnSaveCompleted()
        End Sub

    End Class

End Namespace