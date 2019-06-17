$CertFile = $args[0]
$Password = $args[1]

# Find VSIX
$VsixPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("$PSScriptRoot\..\src\SSDTLifecycleExtension\bin\Release\SSDTLifecycleExtension.vsix")

# Find VSIX Sign Tool
$VsixSignTool = $env:userprofile + '\.nuget\packages\microsoft.vssdk.vsixsigntool\16.1.28916.169\tools\vssdk\vsixsigntool.exe'

# Sign VSIX with certificate
Write-Host "Signing $VsixPath with $CertFile ..."
& $VsixSignTool sign /f $CertFile /p $Password $VsixPath