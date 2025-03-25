Imports System
Imports System.Security.Cryptography
Imports Microsoft.AspNetCore.Cryptography.KeyDerivation

Namespace DPC.Data.Helpers
    Public Class PBKDF2Hasher
        Private Const Iterations As Integer = 10000
        Private Const SaltSize As Integer = 16
        Private Const HashSize As Integer = 32

        ' Generate a hashed password
        Public Shared Function HashPassword(password As String) As String
            Dim salt As Byte() = New Byte(SaltSize - 1) {}
            Using rng As New RNGCryptoServiceProvider()
                rng.GetBytes(salt)
            End Using

            Dim hash As Byte() = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, HashSize)

            Return Convert.ToBase64String(salt) & ":" & Convert.ToBase64String(hash)
        End Function

        ' Verify a password against the stored hash
        Public Shared Function VerifyPassword(password As String, hashedPassword As String) As Boolean
            Dim parts As String() = hashedPassword.Split(":"c)
            If parts.Length <> 2 Then Return False

            Dim salt As Byte() = Convert.FromBase64String(parts(0))
            Dim storedHash As String = parts(1)

            Dim computedHash As Byte() = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, HashSize)

            Return Convert.ToBase64String(computedHash) = storedHash
        End Function
    End Class
End Namespace
