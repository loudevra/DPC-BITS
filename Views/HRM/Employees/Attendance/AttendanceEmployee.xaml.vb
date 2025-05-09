
Namespace DPC.Views.HRM.Employees.Attendance
    ''' <summary>
    ''' Interaction logic for ProductCategories.xaml
    ''' </summary>
    Public Class AttendanceEmployee
        Inherits UserControl
        ' UI elements for direct access
        'Private popup As Popup
        Private recentlyClosed As Boolean = False

        Public Sub New()
            InitializeComponent()
        End Sub
        Private Sub AddAttendanceControl(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim addAttendanceControl As New DPC.Components.Forms.AddAttendance()
            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)
            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, addAttendanceControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub
    End Class
End Namespace