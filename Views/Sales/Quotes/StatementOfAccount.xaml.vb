Public Class StatementOfAccount

    ' Default constructor
    Public Sub New()
        InitializeComponent()
    End Sub

    ' Overloaded constructor that receives the generated data
    Public Sub New(data As StatementModel)
        InitializeComponent()

        ' Set the DataContext. This automatically maps the data to the XAML Bindings!
        Me.DataContext = data
    End Sub

End Class