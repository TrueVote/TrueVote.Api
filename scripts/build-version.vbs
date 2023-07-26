Dim objShell, exe
Set objShell = CreateObject("WScript.Shell")
WScript.Echo "bash -c ""../scripts/build-version.sh"""
set exe = objShell.Exec("bash -c ""../scripts/build-version.sh""")
WScript.echo exe.StdOut.ReadAll
