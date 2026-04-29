Imports System.Text.RegularExpressions
Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.HRM.Employees.Employees

    Public Class AddEmployee
        Inherits UserControl
        Public Event EmployeeAdded(employee As Employee)

        Public Sub New()
            InitializeComponent()
            LoadUserRoles()
            LoadBusinessLocations()
            LoadDepartments()

            AddHandler txtCity.TextChanged, AddressOf CityCheck

            AddHandler txtPhone.TextChanged, Sub(s, e)
                                                 AllowDigit(s, e)
                                             End Sub

            ' Removed salary and sales commission digit-only restriction

            ' Wire up uppercase enforcement
            AddHandler txtName.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtStreetAddress.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtCity.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtRegion.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtCountry.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtPostalCode.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

        Private Sub TxtToUpper_TextChanged(sender As Object, e As TextChangedEventArgs)
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

        Private Sub AllowDigit(s As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(s, TextBox)
            If tb IsNot Nothing Then
                If Not tb.Text.All(AddressOf Char.IsDigit) Then
                    tb.Text = New String(tb.Text.Where(AddressOf Char.IsDigit).ToArray())
                    tb.CaretIndex = tb.Text.Length
                End If
            End If
        End Sub

        Private Sub CityCheck()
            If IsCityInCalabarzon(txtCity.Text) Then
                txtRegion.Text = "CALABARZON"
                txtCountry.Text = "Philippines"
            Else
                txtRegion.Text = Nothing
                txtCountry.Text = Nothing
            End If
        End Sub

        Private Function IsCityInCalabarzon(locationName As String) As Boolean
            Dim calabarzonLocations As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
                "Bacoor", "Imus", "Dasmariñas", "General Trias", "Cavite City", "Tagaytay", "Trece Martires", "Carmona",
                "Alfonso", "Amadeo", "Gen. Mariano Alvarez", "General Emilio Aguinaldo", "Indang", "Kawit",
                "Magallanes", "Maragondon", "Mendez", "Naic", "Noveleta", "Rosario", "Silang", "Tanza", "Ternate",
                "Biñan", "Cabuyao", "Calamba", "San Pablo", "San Pedro", "Santa Rosa",
                "Alaminos", "Bay", "Calauan", "Cavinti", "Famy", "Kalayaan", "Liliw", "Los Baños", "Luisiana",
                "Lumban", "Mabitac", "Magdalena", "Majayjay", "Nagcarlan", "Paete", "Pagsanjan", "Pakil",
                "Pangil", "Pila", "Rizal", "San Antonio", "Santa Cruz", "Santa Maria", "Siniloan", "Victoria",
                "Batangas City", "Lipa", "Tanauan", "Sto. Tomas", "Calaca",
                "Agoncillo", "Alitagtag", "Balayan", "Balete", "Bauan", "Calatagan", "Cuenca", "Ibaan",
                "Laurel", "Lemery", "Lian", "Lobo", "Mabini", "Malvar", "Mataasnakahoy", "Nasugbu",
                "Padre Garcia", "Rosario", "San Jose", "San Juan", "San Luis", "San Nicolas", "San Pascual",
                "Santa Teresita", "Taal", "Talisay", "Taysan", "Tingloy", "Tuy",
                "Antipolo",
                "Angono", "Baras", "Binangonan", "Cainta", "Cardona", "Jala-Jala", "Morong", "Pililla",
                "Rodriguez", "San Mateo", "Tanay", "Taytay", "Teresa",
                "Lucena", "Tayabas",
                "Agdangan", "Alabat", "Atimonan", "Buenavista", "Burdeos", "Calauag", "Candelaria",
                "Catanauan", "Dolores", "General Luna", "General Nakar", "Guinayangan", "Gumaca",
                "Infanta", "Jomalig", "Lopez", "Lucban", "Macalelon", "Mauban", "Mulanay", "Padre Burgos",
                "Pagbilao", "Panukulan", "Patnanungan", "Perez", "Pitogo", "Plaridel", "Polillo", "Quezon",
                "Real", "Sampaloc", "San Andres", "San Antonio", "San Francisco", "San Narciso",
                "Sariaya", "Tagkawayan", "Tiaong", "Unisan"
            }
            Return calabarzonLocations.Contains(locationName.Trim())
        End Function

        Private Sub BtnAddEmployee_Click(sender As Object, e As RoutedEventArgs)
            ' Validate required fields
            If String.IsNullOrWhiteSpace(txtUsername.Text) OrElse
               String.IsNullOrWhiteSpace(txtEmail.Text) OrElse
               String.IsNullOrWhiteSpace(txtPassword.Text) OrElse
               String.IsNullOrWhiteSpace(txtName.Text) OrElse
               String.IsNullOrWhiteSpace(txtPhone.Text) Then
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Email validation
            Dim emailPattern As String = "^[^@\s]+@[^@\s]+\.[^@\s]+$"
            If Not Regex.IsMatch(txtEmail.Text, emailPattern) Then
                MessageBox.Show("Invalid email format.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Allow empty or "N/A" for Salary
            Dim salary As Decimal? = Nothing
            Dim salaryText As String = txtSalary.Text.Trim()
            If Not String.IsNullOrEmpty(salaryText) AndAlso salaryText.ToUpper() <> "N/A" Then
                Dim parsedSalary As Decimal
                If Not Decimal.TryParse(salaryText, parsedSalary) Then
                    MessageBox.Show("Invalid salary amount. Enter a number or leave it blank / type N/A.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Exit Sub
                End If
                salary = parsedSalary
            End If

            ' Allow empty or "N/A" for Sales Commission
            Dim salesCommission As Decimal? = Nothing
            Dim commissionText As String = txtSalesCommission.Text.Trim()
            If Not String.IsNullOrEmpty(commissionText) AndAlso commissionText.ToUpper() <> "N/A" Then
                Dim parsedCommission As Decimal
                If Not Decimal.TryParse(commissionText, parsedCommission) Then
                    MessageBox.Show("Invalid sales commission. Enter a number or leave it blank / type N/A.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Exit Sub
                End If
                salesCommission = parsedCommission
            End If

            ' Create new Employee object
            Dim newEmployee As New Employee With {
                .Username = txtUsername.Text,
                .Email = txtEmail.Text,
                .Password = txtPassword.Text,
                .UserRoleID = CType(cmbUserRole.SelectedValue, Integer),
                .BusinessLocationID = CType(cmbBusinessLocation.SelectedValue, Integer),
                .Name = txtName.Text,
                .StreetAddress = txtStreetAddress.Text,
                .City = txtCity.Text,
                .Region = txtRegion.Text,
                .Country = txtCountry.Text,
                .PostalCode = txtPostalCode.Text,
                .Phone = txtPhone.Text,
                .Salary = If(salary.HasValue, salary.Value, 0),
                .SalesCommission = If(salesCommission.HasValue, salesCommission.Value, 0),
                .Department = cmbDepartments.Text,
                .CreatedAt = DateTime.Now,
                .UpdatedAt = DateTime.Now
            }

            ' Insert into Database
            If EmployeeController.CreateEmployee(newEmployee) Then
                MessageBox.Show("Employee added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                ClearForm()
                ViewLoader.DynamicView.NavigateToView("viewemployee", Me)
                RaiseEvent EmployeeAdded(newEmployee)
            Else
                MessageBox.Show("Failed to add employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        Private Sub ClearForm()
            txtUsername.Clear()
            txtEmail.Clear()
            txtPassword.Clear()
            txtName.Clear()
            txtStreetAddress.Clear()
            txtCity.Clear()
            txtRegion.Clear()
            txtCountry.Clear()
            txtPostalCode.Clear()
            txtPhone.Clear()
            txtSalary.Clear()
            txtSalesCommission.Clear()

            If cmbUserRole.Items.Count > 0 Then cmbUserRole.SelectedIndex = 0
            If cmbBusinessLocation.Items.Count > 0 Then cmbBusinessLocation.SelectedIndex = 0
            If cmbDepartments.Items.Count > 0 Then cmbDepartments.SelectedIndex = 0

            txtInvalidChars.Visibility = Visibility.Collapsed
            txtValidUsername.Visibility = Visibility.Collapsed
            chkLength.Visibility = Visibility.Collapsed
            chkMaxLength.Visibility = Visibility.Collapsed
            chkUpper.Visibility = Visibility.Collapsed
            chkLower.Visibility = Visibility.Collapsed
            chkSpecial.Visibility = Visibility.Collapsed
            chkNumber.Visibility = Visibility.Collapsed
        End Sub

        Private Sub TxtPassword_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim pwd As String = txtPassword.Text
            ShowIfSatisfied(chkLength, pwd.Length >= 6)
            ShowIfSatisfied(chkMaxLength, pwd.Length < 20)
            ShowIfSatisfied(chkUpper, pwd.Any(Function(c) Char.IsUpper(c)))
            ShowIfSatisfied(chkLower, pwd.Any(Function(c) Char.IsLower(c)))
            ShowIfSatisfied(chkSpecial, pwd.Any(Function(c) Not Char.IsLetterOrDigit(c)))
            ShowIfSatisfied(chkNumber, pwd.Any(Function(c) Char.IsDigit(c)))
        End Sub

        Private Sub TxtUsername_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim input As String = txtUsername.Text
            Dim isValid As Boolean = input.Length >= 6 AndAlso input.Length < 20 AndAlso
                                     input.Any(Function(c) Char.IsUpper(c)) AndAlso
                                     input.Any(Function(c) Char.IsLower(c)) AndAlso
                                     input.Any(Function(c) Not Char.IsLetterOrDigit(c)) AndAlso
                                     input.Any(Function(c) Char.IsDigit(c))
            txtInvalidChars.Visibility = If(isValid, Visibility.Collapsed, Visibility.Visible)
            txtValidUsername.Visibility = If(isValid, Visibility.Visible, Visibility.Collapsed)
        End Sub

        Private Sub ShowIfSatisfied(textBlock As TextBlock, isMet As Boolean)
            textBlock.Visibility = If(isMet, Visibility.Visible, Visibility.Collapsed)
        End Sub

        Private Sub LoadUserRoles()
            Dim roles = EmployeeController.GetUserRoles()
            cmbUserRole.DisplayMemberPath = "Value"
            cmbUserRole.SelectedValuePath = "Key"
            cmbUserRole.ItemsSource = roles
        End Sub

        Private Sub LoadBusinessLocations()
            Dim locations = EmployeeController.GetBusinessLocations()
            cmbBusinessLocation.DisplayMemberPath = "Value"
            cmbBusinessLocation.SelectedValuePath = "Key"
            cmbBusinessLocation.ItemsSource = locations
        End Sub

        Private Sub LoadDepartments()
            Dim departments = EmployeeController.GetDepartments()
            cmbDepartments.ItemsSource = departments
        End Sub

        Public Sub ResetForm()
            ClearForm()
        End Sub
    End Class
End Namespace