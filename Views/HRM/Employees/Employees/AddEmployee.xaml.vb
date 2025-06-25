Imports System.Text.RegularExpressions
Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.HRM.Employees.Employees

    Public Class AddEmployee
        Inherits UserControl
        ' Event to notify parent when employee is added
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
            AddHandler txtSalary.TextChanged, Sub(s, e)
                                                  AllowDigit(s, e)
                                              End Sub
            AddHandler txtSalesCommission.TextChanged, Sub(s, e)
                                                           AllowDigit(s, e)
                                                       End Sub
        End Sub

        Private Sub AllowDigit(s As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(s, TextBox)
            If tb IsNot Nothing Then
                If Not tb.Text.All(AddressOf Char.IsDigit) Then
                    ' Remove non-digit characters (optional behavior)
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

            ' Salary & Sales Commission Parsing
            Dim salary As Decimal
            Dim salesCommission As Decimal

            If Not Decimal.TryParse(txtSalary.Text, salary) Then
                MessageBox.Show("Invalid salary amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            If String.IsNullOrWhiteSpace(txtSalesCommission.Text) Then
                salesCommission = 0
            Else
                salesCommission = txtSalesCommission.Text
            End If

            ' Create new Employee object
            Dim newEmployee As New Employee With {
                .Username = txtUsername.Text,
                .Email = txtEmail.Text,
                .Password = txtPassword.Text, ' Hash before storing in production
                .UserRoleID = CType(cmbUserRole.SelectedValue, Integer),
                .BusinessLocationID = CType(cmbBusinessLocation.SelectedValue, Integer),
                .Name = txtName.Text,
                .StreetAddress = txtStreetAddress.Text,
                .City = txtCity.Text,
                .Region = txtRegion.Text,
                .Country = txtCountry.Text,
                .PostalCode = txtPostalCode.Text,
                .Phone = txtPhone.Text,
                .Salary = salary,
                .SalesCommission = salesCommission,
                .Department = cmbDepartments.Text,
                .CreatedAt = DateTime.Now,
                .UpdatedAt = DateTime.Now
            }

            ' Insert into Database
            If EmployeeController.CreateEmployee(newEmployee) Then
                MessageBox.Show("Employee added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                ' Clear the form for next entry
                ClearForm()

                ViewLoader.DynamicView.NavigateToView("viewemployee", Me)
                ' Raise event to notify parent
                RaiseEvent EmployeeAdded(newEmployee)
            Else
                MessageBox.Show("Failed to add employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        Private Sub ClearForm()
            ' Clear all form fields
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

            ' Reset comboboxes to first item
            If cmbUserRole.Items.Count > 0 Then cmbUserRole.SelectedIndex = 0
            If cmbBusinessLocation.Items.Count > 0 Then cmbBusinessLocation.SelectedIndex = 0
            If cmbDepartments.Items.Count > 0 Then cmbDepartments.SelectedIndex = 0


            ' Hide validation messages
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

            ' Check each condition
            Dim lengthValid As Boolean = pwd.Length >= 6
            Dim maxLengthValid As Boolean = pwd.Length < 20
            Dim hasUpper As Boolean = pwd.Any(Function(c) Char.IsUpper(c))
            Dim hasLower As Boolean = pwd.Any(Function(c) Char.IsLower(c))
            Dim hasSpecial As Boolean = pwd.Any(Function(c) Not Char.IsLetterOrDigit(c))
            Dim hasNumber As Boolean = pwd.Any(Function(c) Char.IsDigit(c))

            ' Show indicators next to each rule
            ShowIfSatisfied(chkLength, lengthValid)
            ShowIfSatisfied(chkMaxLength, maxLengthValid)
            ShowIfSatisfied(chkUpper, hasUpper)
            ShowIfSatisfied(chkLower, hasLower)
            ShowIfSatisfied(chkSpecial, hasSpecial)
            ShowIfSatisfied(chkNumber, hasNumber)
        End Sub

        Private Sub TxtUsername_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim input As String = txtUsername.Text

            ' Check each condition
            Dim hasMinLength As Boolean = input.Length >= 6
            Dim hasMaxLength As Boolean = input.Length < 20
            Dim hasUppercase As Boolean = input.Any(Function(c) Char.IsUpper(c))
            Dim hasLowercase As Boolean = input.Any(Function(c) Char.IsLower(c))
            Dim hasSpecialChar As Boolean = input.Any(Function(c) Not Char.IsLetterOrDigit(c))
            Dim hasDigit As Boolean = input.Any(Function(c) Char.IsDigit(c))

            ' Combine all checks
            Dim isValid As Boolean = hasMinLength AndAlso hasMaxLength AndAlso
                             hasUppercase AndAlso hasLowercase AndAlso
                             hasSpecialChar AndAlso hasDigit

            ' Toggle both TextBlocks
            txtInvalidChars.Visibility = If(isValid, Visibility.Collapsed, Visibility.Visible)
            txtValidUsername.Visibility = If(isValid, Visibility.Visible, Visibility.Collapsed)
        End Sub

        Private Sub ShowIfSatisfied(textBlock As TextBlock, isMet As Boolean)
            textBlock.Visibility = If(isMet, Visibility.Visible, Visibility.Collapsed)
        End Sub

        ' Load User Roles into ComboBox
        Private Sub LoadUserRoles()
            Dim roles = EmployeeController.GetUserRoles()
            cmbUserRole.DisplayMemberPath = "Value"
            cmbUserRole.SelectedValuePath = "Key"
            cmbUserRole.ItemsSource = roles
        End Sub

        ' Load Business Locations into ComboBox
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
        ' Public method to reset the form
        Public Sub ResetForm()
            ClearForm()
        End Sub
    End Class
End Namespace