Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Controllers
    Public Class ProductController

        Public Shared Sub GetProductCategory(comboBox As ComboBox)
            Dim query As String = "
    SELECT TRIM(CONCAT(
        IF(category = 'fdas', 
           UPPER(category), 
           IF(CHAR_LENGTH(SUBSTRING_INDEX(category, ' ', 1)) <= 3,
              UPPER(SUBSTRING_INDEX(category, ' ', 1)),
              CONCAT(UPPER(LEFT(SUBSTRING_INDEX(category, ' ', 1), 1)),
                     LOWER(SUBSTRING(SUBSTRING_INDEX(category, ' ', 1), 2)))
           )
        ),
        IF(LOCATE(' ', category) > 0, CONCAT(' ',
            IF(category = 'fdas', 
               UPPER(category),
               IF(CHAR_LENGTH(SUBSTRING_INDEX(category, ' ', -1)) <= 3,
                  UPPER(SUBSTRING_INDEX(category, ' ', -1)),
                  CONCAT(UPPER(LEFT(SUBSTRING_INDEX(category, ' ', -1), 1)),
                         LOWER(SUBSTRING(SUBSTRING_INDEX(category, ' ', -1), 2)))
               )
            )
        ), '')
    )) AS category
    FROM productcategory
    ORDER BY category ASC;
    "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()
                            While reader.Read()
                                Dim categoryName As String = reader("category").ToString()
                                comboBox.Items.Add(New ComboBoxItem With {.Content = categoryName})
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub

        Public Shared Sub GetProductSubcategory(categoryName As String, comboBox As ComboBox)
            Dim query As String = "SELECT subcategory FROM productcategory WHERE LOWER(category) = LOWER(@categoryName)"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()


                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@categoryName", categoryName)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()

                            If reader.Read() Then
                                Dim subcategoryData As String = reader("subcategory").ToString()

                                If String.IsNullOrWhiteSpace(subcategoryData) Then
                                    comboBox.Items.Add(New ComboBoxItem With {.Content = "No Subcategories Available"})
                                Else
                                    Dim subcategories As String() = subcategoryData.Split(","c).
                                                                Select(Function(s) StrConv(s.Trim(), VbStrConv.ProperCase)).
                                                                ToArray()

                                    For Each subcategory As String In subcategories
                                        comboBox.Items.Add(New ComboBoxItem With {.Content = subcategory})
                                    Next
                                End If
                            Else
                                comboBox.Items.Add(New ComboBoxItem With {.Content = "No Subcategories Found"})
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub

    End Class

End Namespace
