Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports System.Data
Imports DPC.Data.Helpers

Public Class SOAController

    ' -------------------------------------------------------------------------
    ' GENERATE SOA NUMBER via stored procedure
    ' -------------------------------------------------------------------------
    Public Shared Function GenerateSOANumber() As String
        Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
            conn.Open()
            Using cmd As New MySqlCommand("CALL sp_generate_soa_no(@out)", conn)
                cmd.Parameters.Add("@out", MySqlDbType.VarChar, 50).Direction = ParameterDirection.Output
                cmd.ExecuteNonQuery()
                Return cmd.Parameters("@out").Value.ToString()
            End Using
        End Using
    End Function

    ' -------------------------------------------------------------------------
    ' LOAD ALL (header only, for the DataGrid list)
    ' -------------------------------------------------------------------------
    Public Shared Function LoadAll() As ObservableCollection(Of StatementModel)
        Dim result As New ObservableCollection(Of StatementModel)
        Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
            conn.Open()
            Dim sql = "
                SELECT soa_id, soa_no, client_id, client_name,
                       project_title, statement_date, po_no, si_no, dr_no, bs_no,
                       po_date, delivery_period_days, required_delivery_date,
                       actual_completion_date, contract_amount, ld_rate_pct_per_day,
                       days_delayed, subtotal_vat_inclusive, payments_total,
                       outstanding_balance, liquidated_damages, grand_total, computed_ld
                FROM v_soa_summary
                ORDER BY statement_date DESC"

            Using cmd As New MySqlCommand(sql, conn)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        result.Add(MapHeader(reader))
                    End While
                End Using
            End Using
        End Using
        Return result
    End Function

    ' -------------------------------------------------------------------------
    ' LOAD FULL (header + line items + payments, for the edit form)
    ' -------------------------------------------------------------------------
    Public Shared Function LoadFull(soaId As Integer) As StatementModel
        Dim model As StatementModel = Nothing

        Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
            conn.Open()

            ' 1. Load header from view
            Dim headerSql = "
                SELECT soa_id, soa_no, client_id, client_name,
                       project_title, statement_date, po_no, si_no, dr_no, bs_no,
                       po_date, delivery_period_days, required_delivery_date,
                       actual_completion_date, contract_amount, ld_rate_pct_per_day,
                       days_delayed, subtotal_vat_inclusive, payments_total,
                       outstanding_balance, liquidated_damages, grand_total, computed_ld
                FROM v_soa_summary
                WHERE soa_id = @id"

            Using cmd As New MySqlCommand(headerSql, conn)
                cmd.Parameters.AddWithValue("@id", soaId)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        model = MapHeader(reader)
                    End If
                End Using
            End Using

            If model Is Nothing Then Return Nothing

            ' 2. Load line items
            Dim linesSql = "
                SELECT item_date, description, qty, amount, payment, balance
                FROM soa_line_items
                WHERE soa_id = @id
                ORDER BY sort_order"

            Using cmd As New MySqlCommand(linesSql, conn)
                cmd.Parameters.AddWithValue("@id", soaId)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        model.LineItems.Add(New LineItemModel With {
                        .DateStr = If(IsDBNull(reader("item_date")), "", CDate(reader("item_date")).ToString("MM/dd/yyyy")),
                        .Description = NullToString(reader("description")),
                        .Qty = NullToString(reader("qty")),
                        .Amount = NullToString(reader("amount")),
                        .Payment = NullToString(reader("payment")),
                        .Balance = NullToString(reader("balance"))
                    })
                    End While
                End Using
            End Using

            ' 3. Load payment details
            Dim paysSql = "
                SELECT payment_date, reference, amount_paid
                FROM soa_payment_details
                WHERE soa_id = @id
                ORDER BY sort_order"

            Using cmd As New MySqlCommand(paysSql, conn)
                cmd.Parameters.AddWithValue("@id", soaId)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        model.PaymentItems.Add(New PaymentItemModel With {
                        .DateStr = If(IsDBNull(reader("payment_date")), "", CDate(reader("payment_date")).ToString("MM/dd/yyyy")),
                        .Reference = NullToString(reader("reference")),
                        .AmountPaid = NullToString(reader("amount_paid"))
                    })
                    End While
                End Using
            End Using
        End Using

        Return model
    End Function

    ' -------------------------------------------------------------------------
    ' INSERT (returns new soa_id)
    ' -------------------------------------------------------------------------
    Public Shared Function InsertSOA(m As StatementModel) As Integer
        Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
            conn.Open()
            Using tx = conn.BeginTransaction()
                Try
                    Dim soaId As Integer = InsertHeader(conn, tx, m)
                    InsertLineItems(conn, tx, soaId, m.LineItems)
                    InsertPayments(conn, tx, soaId, m.PaymentItems)
                    tx.Commit()
                    Return soaId
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Function

    ' -------------------------------------------------------------------------
    ' UPDATE (replaces children on every save)
    ' -------------------------------------------------------------------------
    Public Shared Function UpdateSOA(m As StatementModel) As Boolean
        Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
            conn.Open()
            Using tx = conn.BeginTransaction()
                Try
                    UpdateHeader(conn, tx, m)

                    Using cmd As New MySqlCommand(
                    "DELETE FROM soa_line_items WHERE soa_id=@id", conn, tx)
                        cmd.Parameters.AddWithValue("@id", m.SoaId)
                        cmd.ExecuteNonQuery()
                    End Using

                    Using cmd As New MySqlCommand(
                    "DELETE FROM soa_payment_details WHERE soa_id=@id", conn, tx)
                        cmd.Parameters.AddWithValue("@id", m.SoaId)
                        cmd.ExecuteNonQuery()
                    End Using

                    InsertLineItems(conn, tx, m.SoaId, m.LineItems)
                    InsertPayments(conn, tx, m.SoaId, m.PaymentItems)

                    tx.Commit()
                    Return True
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Function

    ' -------------------------------------------------------------------------
    ' DELETE
    ' -------------------------------------------------------------------------
    Public Shared Function DeleteSOA(soaId As Integer) As Boolean
        Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
            conn.Open()
            ' Children cascade-delete via FK, so only need to delete the header
            Using cmd As New MySqlCommand(
            "DELETE FROM statements_of_account WHERE soa_id=@id", conn)
                cmd.Parameters.AddWithValue("@id", soaId)
                cmd.ExecuteNonQuery()
                Return True
            End Using
        End Using
    End Function

    ' =========================================================================
    ' PRIVATE HELPERS
    ' =========================================================================

    Private Shared Function InsertHeader(conn As MySqlConnection,
                                         tx As MySqlTransaction,
                                         m As StatementModel) As Integer
        Dim sql = "
            INSERT INTO statements_of_account
              (soa_no, client_id, po_no, si_no, dr_no, bs_no,
               project_title, statement_date, po_date,
               delivery_period_days, required_delivery_date, actual_completion_date,
               contract_amount, ld_rate_pct_per_day, days_delayed,
               subtotal_vat_inclusive, total_payment_made, outstanding_balance,
               liquidated_damages, grand_total, status)
            VALUES
              (@soa_no, @client_id, @po_no, @si_no, @dr_no, @bs_no,
               @project_title, @statement_date, @po_date,
               @delivery_period_days, @required_delivery_date, @actual_completion_date,
               @contract_amount, @ld_rate, @days_delayed,
               @subtotal, @total_payment, @outstanding,
               @ld_amount, @grand_total, 'generated')"

        Using cmd As New MySqlCommand(sql, conn, tx)
            MapHeaderParams(cmd, m)
            cmd.ExecuteNonQuery()
            Return CInt(cmd.LastInsertedId)
        End Using
    End Function

    Private Shared Sub UpdateHeader(conn As MySqlConnection,
                                    tx As MySqlTransaction,
                                    m As StatementModel)
        Dim sql = "
            UPDATE statements_of_account SET
              client_id                = @client_id,
              po_no                    = @po_no,
              si_no                    = @si_no,
              dr_no                    = @dr_no,
              bs_no                    = @bs_no,
              project_title            = @project_title,
              statement_date           = @statement_date,
              po_date                  = @po_date,
              delivery_period_days     = @delivery_period_days,
              required_delivery_date   = @required_delivery_date,
              actual_completion_date   = @actual_completion_date,
              contract_amount          = @contract_amount,
              ld_rate_pct_per_day      = @ld_rate,
              days_delayed             = @days_delayed,
              subtotal_vat_inclusive   = @subtotal,
              total_payment_made       = @total_payment,
              outstanding_balance      = @outstanding,
              liquidated_damages       = @ld_amount,
              grand_total              = @grand_total
            WHERE soa_id = @soa_id"

        Using cmd As New MySqlCommand(sql, conn, tx)
            MapHeaderParams(cmd, m)
            cmd.Parameters.AddWithValue("@soa_id", m.SoaId)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Shared Sub MapHeaderParams(cmd As MySqlCommand, m As StatementModel)
        cmd.Parameters.AddWithValue("@soa_no", m.SOANo)
        cmd.Parameters.AddWithValue("@client_id", m.ClientId)
        cmd.Parameters.AddWithValue("@po_no", NullIfEmpty(m.PONo))
        cmd.Parameters.AddWithValue("@si_no", NullIfEmpty(m.SINo))
        cmd.Parameters.AddWithValue("@dr_no", NullIfEmpty(m.DRNo))
        cmd.Parameters.AddWithValue("@bs_no", NullIfEmpty(m.BSNo))
        cmd.Parameters.AddWithValue("@project_title", m.ProjectTitle)
        cmd.Parameters.AddWithValue("@statement_date", ParseDateOrNull(m.StatementDate))
        cmd.Parameters.AddWithValue("@po_date", ParseDateOrNull(m.PODate))
        cmd.Parameters.AddWithValue("@delivery_period_days", ParseIntOrNull(m.DeliveryPeriod))
        cmd.Parameters.AddWithValue("@required_delivery_date", ParseDateOrNull(m.RequiredDate))
        cmd.Parameters.AddWithValue("@actual_completion_date", ParseDateOrNull(m.CompletionDate))
        cmd.Parameters.AddWithValue("@contract_amount", ParseDecimalOrNull(m.ContractAmount))
        cmd.Parameters.AddWithValue("@ld_rate", ParseDecimalOrNull(m.LDRate))
        cmd.Parameters.AddWithValue("@days_delayed", ParseIntOrNull(m.LDDaysDelayed))
        cmd.Parameters.AddWithValue("@subtotal", ParseDecimalOrNull(m.Subtotal))
        cmd.Parameters.AddWithValue("@total_payment", ParseDecimalOrNull(m.TotalPayment))
        cmd.Parameters.AddWithValue("@outstanding", ParseDecimalOrNull(m.OutstandingBalance))
        cmd.Parameters.AddWithValue("@ld_amount", ParseDecimalOrNull(m.LiquidatedDamages))
        cmd.Parameters.AddWithValue("@grand_total", ParseDecimalOrNull(m.NetAmountDue))
    End Sub

    Private Shared Sub InsertLineItems(conn As MySqlConnection,
                                       tx As MySqlTransaction,
                                       soaId As Integer,
                                       items As IList(Of LineItemModel))
        If items Is Nothing OrElse items.Count = 0 Then Return
        Dim sql = "
            INSERT INTO soa_line_items
              (soa_id, sort_order, item_date, description, qty, amount, payment, balance)
            VALUES
              (@soa_id, @sort, @dt, @desc, @qty, @amt, @pay, @bal)"

        For i = 0 To items.Count - 1
            Using cmd As New MySqlCommand(sql, conn, tx)
                Dim it = items(i)
                cmd.Parameters.AddWithValue("@soa_id", soaId)
                cmd.Parameters.AddWithValue("@sort", i)
                cmd.Parameters.AddWithValue("@dt", ParseDateOrNull(it.DateStr))
                cmd.Parameters.AddWithValue("@desc", NullIfEmpty(it.Description))
                cmd.Parameters.AddWithValue("@qty", ParseDecimalOrNull(it.Qty))
                cmd.Parameters.AddWithValue("@amt", ParseDecimalOrNull(it.Amount))
                cmd.Parameters.AddWithValue("@pay", ParseDecimalOrNull(it.Payment))
                cmd.Parameters.AddWithValue("@bal", ParseDecimalOrNull(it.Balance))
                cmd.ExecuteNonQuery()
            End Using
        Next
    End Sub

    Private Shared Sub InsertPayments(conn As MySqlConnection,
                                      tx As MySqlTransaction,
                                      soaId As Integer,
                                      items As IList(Of PaymentItemModel))
        If items Is Nothing OrElse items.Count = 0 Then Return
        Dim sql = "
            INSERT INTO soa_payment_details
              (soa_id, sort_order, payment_date, reference, amount_paid)
            VALUES
              (@soa_id, @sort, @dt, @ref, @amt)"

        For i = 0 To items.Count - 1
            Using cmd As New MySqlCommand(sql, conn, tx)
                Dim it = items(i)
                cmd.Parameters.AddWithValue("@soa_id", soaId)
                cmd.Parameters.AddWithValue("@sort", i)
                cmd.Parameters.AddWithValue("@dt", ParseDateOrNull(it.DateStr))
                cmd.Parameters.AddWithValue("@ref", NullIfEmpty(it.Reference))
                cmd.Parameters.AddWithValue("@amt", ParseDecimalOrNull(it.AmountPaid))
                cmd.ExecuteNonQuery()
            End Using
        Next
    End Sub

    Private Shared Function MapHeader(reader As MySqlDataReader) As StatementModel
        Dim ldPerDay As Decimal = 0
        Dim contractAmt As Decimal = 0
        Dim ldRate As Decimal = 0
        If Not IsDBNull(reader("contract_amount")) Then contractAmt = CDec(reader("contract_amount"))
        If Not IsDBNull(reader("ld_rate_pct_per_day")) Then ldRate = CDec(reader("ld_rate_pct_per_day"))
        ldPerDay = contractAmt * (ldRate / 100)

        Return New StatementModel With {
            .SoaId = CInt(reader("soa_id")),
            .SOANo = NullToString(reader("soa_no")),
            .ClientId = NullToString(reader("client_id")),
            .ClientName = NullToString(reader("client_name")),
            .ProjectTitle = NullToString(reader("project_title")),
            .StatementDate = If(IsDBNull(reader("statement_date")), "", CDate(reader("statement_date")).ToString("MMMM dd, yyyy")),
            .PONo = NullToString(reader("po_no")),
            .SINo = NullToString(reader("si_no")),
            .DRNo = NullToString(reader("dr_no")),
            .BSNo = NullToString(reader("bs_no")),
            .PODate = If(IsDBNull(reader("po_date")), "", CDate(reader("po_date")).ToString("MMMM dd, yyyy")),
            .DeliveryPeriod = NullToString(reader("delivery_period_days")),
            .RequiredDate = If(IsDBNull(reader("required_delivery_date")), "", CDate(reader("required_delivery_date")).ToString("MMMM dd, yyyy")),
            .CompletionDate = If(IsDBNull(reader("actual_completion_date")), "", CDate(reader("actual_completion_date")).ToString("MMMM dd, yyyy")),
            .ContractAmount = NullToString(reader("contract_amount")),
            .LDRate = NullToString(reader("ld_rate_pct_per_day")),
            .LDDaysDelayed = NullToString(reader("days_delayed")),
            .LDPerDay = ldPerDay.ToString("N2"),
            .Subtotal = NullToString(reader("subtotal_vat_inclusive")),
            .TotalPayment = NullToString(reader("payments_total")),
            .OutstandingBalance = NullToString(reader("outstanding_balance")),
            .LiquidatedDamages = NullToString(reader("liquidated_damages")),
            .NetAmountDue = NullToString(reader("grand_total"))
        }
    End Function

    ' ── tiny parse helpers ───────────────────────────────────────────────────

    Private Shared Function NullIfEmpty(s As String) As Object
        Return If(String.IsNullOrWhiteSpace(s), DBNull.Value, CObj(s))
    End Function

    Private Shared Function NullToString(val As Object) As String
        Return If(IsDBNull(val), "", val.ToString())
    End Function

    Private Shared Function ParseDateOrNull(s As String) As Object
        Dim d As Date
        Return If(Date.TryParse(s, d), CObj(d), DBNull.Value)
    End Function

    Private Shared Function ParseDecimalOrNull(s As String) As Object
        Dim v As Decimal
        Return If(Decimal.TryParse(s?.Replace(",", ""), v), CObj(v), DBNull.Value)
    End Function

    Private Shared Function ParseIntOrNull(s As String) As Object
        Dim v As Integer
        Return If(Integer.TryParse(s, v), CObj(v), DBNull.Value)
    End Function

End Class