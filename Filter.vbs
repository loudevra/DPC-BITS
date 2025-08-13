On Error Resume Next
Dim args
Set args = WScript.Arguments
If args.Count > 0 Then
    targetDir = args(0)
    targetDir = Replace(targetDir, "/target=", "")
    If Right(targetDir, 1) <> "\" Then targetDir = targetDir & "\"
    Set objFSO = CreateObject("Scripting.FileSystemObject")
    If objFSO.FileExists(targetDir & ".env") Then
        Set objFile = objFSO.GetFile(targetDir & ".env")
        If Not (objFile.Attributes And 2) Then
            objFile.Attributes = objFile.Attributes + 2 ' Add Hidden attribute
        End If
        objFSO.DeleteFile targetDir & ".env", True ' Delete the file
    End If
End If
If Err.Number <> 0 Then
    WScript.Echo "Error: " & Err.Description
End If