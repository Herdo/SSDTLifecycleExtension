$Password = $args[0]

# Find VSIX
$VsixPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("$PSScriptRoot\..\src\SSDTLifecycleExtension\bin\Release\SSDTLifecycleExtension.vsix")

# Find VSIX Sign Tool
$VsixSignTool = $env:userprofile + '\.nuget\packages\microsoft.vssdk.vsixsigntool\16.1.28916.169\tools\vssdk\vsixsigntool.exe'

# Sign VSIX with certificate
& $VsixSignTool sign /f $env:DOWNLOADSECUREFILE_SECUREFILEPATH /p $Password $VsixPath