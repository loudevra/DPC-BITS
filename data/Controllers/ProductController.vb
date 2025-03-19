Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Controllers
    Public Class ProductController

        Public Shared Sub GetProductCategory(comboBox As ComboBox)
            Dim query As String = "SELECT TRIM(CONCAT(
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

            Using cmd As New MySqlCommand(query, SplashScreen.DBConnection)
                Try
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        comboBox.Items.Clear()
                        While reader.Read()
                            Dim categoryName As String = reader("category").ToString()
                            comboBox.Items.Add(New ComboBoxItem With {.Content = categoryName})
                        End While
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub
    End Class

End Namespace
