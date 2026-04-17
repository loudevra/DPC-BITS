Public Class StatementOfAccount

    ' Default constructor
    Public Sub New()
        InitializeComponent()
    End Sub

    ' Overloaded constructor that receives the generated data
    Public Sub New(data As StatementModel)
        InitializeComponent()

        ' Inject the data into the XAML TextBlocks
        If docDate IsNot Nothing Then docDate.Text = data.StatementDate
        If docClientName IsNot Nothing Then docClientName.Text = "BILL TO: " & data.ClientName.ToUpper()
        If docPONo IsNot Nothing Then docPONo.Text = "PO NO.: " & data.PONo

        ' Format currencies in the summary
        If docContractAmt IsNot Nothing Then docContractAmt.Text = data.ContractAmount

        ' You can easily inject the other fields we named into the layout here as well!
        ' If docOutstandingBalance IsNot Nothing Then docOutstandingBalance.Text = data.ContractAmount

        If docNetAmtDue IsNot Nothing Then docNetAmtDue.Text = data.NetAmountDue
    End Sub

End Class