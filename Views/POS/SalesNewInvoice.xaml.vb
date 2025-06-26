Namespace DPC.Views.POS
    Public Class SalesNewInvoice
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub CardControl(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim CardControl As New DPC.Views.POS.CreditPayment()
            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)
            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, CardControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub
        Private Sub DraftControl(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim DraftControl As New DPC.Components.Forms.AddAttendance()
            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)
            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, DraftControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub
        Private Sub PaymentControl(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim PaymentControl As New DPC.Views.POS.InvoicePayment()
            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)
            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, PaymentControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub
    End Class
End Namespace