Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class AddCategory
        Public Event BrandAdded()

        Public Sub New()
            InitializeComponent()

            CreateCategoryPanel()
        End Sub

        Private Sub CreateCategoryPanel()
            ' Create CategoryPanel
            Dim categoryPanel As New StackPanel() With {
        .Name = "CategoryPanel"
    }

            ' Create Horizontal StackPanel for Category Label
            Dim categoryLabelPanel As New StackPanel() With {
        .Orientation = Orientation.Horizontal,
        .Margin = New Thickness(0, 0, 0, 5)
    }
            Dim categoryLabel As New TextBlock() With {
        .Text = "Category Name:",
        .FontSize = 14,
        .FontWeight = FontWeights.SemiBold,
        .Margin = New Thickness(0, 0, 5, 0)
    }
            categoryLabelPanel.Children.Add(categoryLabel)

            ' Create Category Border & TextBox
            Dim categoryBorder As New Border() With {
        .Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style),
        .Margin = New Thickness(0, 0, 0, 15)
    }

            Dim txtName As New TextBox() With {
        .Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style),
        .Name = "TxtName"
    }

            categoryBorder.Child = txtName

            ' Add elements to CategoryPanel
            categoryPanel.Children.Add(categoryLabelPanel)
            categoryPanel.Children.Add(categoryBorder)

            ' Create DescriptionPanel
            Dim descriptionPanel As New StackPanel() With {
        .Name = "DescriptionPanel"
    }

            ' Create Horizontal StackPanel for Description Label
            Dim descriptionLabelPanel As New StackPanel() With {
        .Orientation = Orientation.Horizontal,
        .Margin = New Thickness(0, 0, 0, 5)
    }
            Dim descriptionLabel As New TextBlock() With {
        .Text = "Description:",
        .FontSize = 14,
        .FontWeight = FontWeights.SemiBold,
        .Margin = New Thickness(0, 0, 5, 0)
    }
            descriptionLabelPanel.Children.Add(descriptionLabel)

            ' Create Description Border & TextBox
            Dim descriptionBorder As New Border() With {
        .Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style),
        .Margin = New Thickness(0, 0, 0, 15)
    }

            Dim txtDescription As New TextBox() With {
        .Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style),
        .Name = "TxtDescription"
    }

            descriptionBorder.Child = txtDescription

            ' Add elements to DescriptionPanel
            descriptionPanel.Children.Add(descriptionLabelPanel)
            descriptionPanel.Children.Add(descriptionBorder)

            ' Add both panels to MainContent
            MainContent.Children.Add(categoryPanel)
            MainContent.Children.Add(descriptionPanel)
        End Sub


        Private Sub RemoveCategoryPanel()
            ' Ensure there are at least two panels to remove
            If MainContent.Children.Count >= 2 Then
                ' Remove the last two added panels (Category and Description)
                MainContent.Children.RemoveAt(MainContent.Children.Count - 1) ' Description Panel
                MainContent.Children.RemoveAt(MainContent.Children.Count - 1) ' Category Panel
            End If
        End Sub


        Private Sub decreaseBtn(sender As Object, e As RoutedEventArgs)
            ' Get the current value of the CategoryNumber TextBlock
            Dim currentValue As Integer

            ' Ensure the current value is a valid integer
            If Integer.TryParse(CategoryNumber.Text, currentValue) Then
                ' Prevent removing the last remaining category
                If currentValue > 1 Then
                    ' Call the function to decrease the value
                    AdjustCategoryNumber(-1)

                    ' Remove the last added category panel
                    RemoveCategoryPanel()
                End If
            End If
        End Sub


        Private Sub increaseBtn(sender As Object, e As RoutedEventArgs)
            ' Call the function to increase the value
            AdjustCategoryNumber(1)

            ' Add the newly created CategoryPanel into MainContent
            CreateCategoryPanel()
        End Sub



        Private Sub AdjustCategoryNumber(change As Integer)
            ' Get the current value of the CategoryNumber TextBlock
            Dim currentValue As Integer

            ' Ensure the current value is a valid integer
            If Integer.TryParse(CategoryNumber.Text, currentValue) Then
                ' Adjust the value by the specified change (increase or decrease)
                currentValue += change
                ' Update the TextBlock with the new value
                CategoryNumber.Text = currentValue.ToString()
            Else
                ' If the value is not a valid integer, set it to 0 or a default value
                CategoryNumber.Text = "0"
            End If
        End Sub


        Public Shared Sub InsertBrand(brandName As String)
            If String.IsNullOrWhiteSpace(brandName) Then
                MessageBox.Show("Brand name cannot be empty.")
                Return
            End If

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Check for duplicate brand
                    Dim checkQuery As String = "SELECT COUNT(*) FROM Brand WHERE BrandName = @BrandName"
                    Using checkCmd As New MySqlCommand(checkQuery, conn)
                        checkCmd.Parameters.AddWithValue("@BrandName", brandName)

                        ' Safely check for NULL and convert to Int32
                        Dim result As Object = checkCmd.ExecuteScalar()
                        Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)

                        If count > 0 Then
                            MessageBox.Show("Brand already exists.")
                            Return
                        End If
                    End Using

                    ' Insert brand without BrandID
                    Dim query As String = "INSERT INTO Brand (BrandName) VALUES (@BrandName)"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@BrandName", brandName)
                        cmd.ExecuteNonQuery()
                    End Using

                    MessageBox.Show("Brand added successfully!")
                End Using
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
            End Try
        End Sub
    End Class
End Namespace
