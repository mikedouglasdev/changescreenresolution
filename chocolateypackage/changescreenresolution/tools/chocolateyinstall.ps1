$ErrorActionPreference = 'Stop'; 

$packageName= 'change-screen-resolution'
$toolName   = 'changescreenresolution'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$fileLocation = Join-Path $toolsDir 'changescreenresolution.exe'

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  fileType      = 'exe'
  url           = $url
  file         = $fileLocation
  softwareName  = 'change-screen-resolution*' 
  checksum      = '63AF2FA7F3A664B7FE7C7533BF73DDB2'
  checksumType  = 'md5' 
}

function Write-Config($key, $value)
  {
    # update config file
      $appConfig = "$installLocation\tools\$toolName.exe.config"
      $doc = (Get-Content $appConfig) -as [Xml]

      $obj = $doc.configuration.appSettings.add | Where-Object {$_.Key -eq $key}
      $obj.value = $value
      $doc.Save($appConfig)
  }

 $installLocation = $env:ChocolateyPackageFolder 

$pp = Get-PackageParameters

if ($null -ne $pp["RunAtStartup"] -and $pp["RunAtStartup"] -eq 'true') 
{
  if($null -ne $pp["resolution"])
  {
    $resolution = $pp["resolution"]
  }
  else {
    $resolution = "1920 1080"
  }

  Install-ChocolateyShortcut -ShortcutFilePath "C:\Users\All Users\Microsoft\Windows\Start Menu\Programs\Startup\csr.lnk" -Description "Change Screen Reso" -Arguments $resolution -TargetPath $installLocation\tools\$toolName.exe

  if($null -ne $pp["LoggingEnabled"] -and $pp["LoggingEnabled"] -eq 'true')
  {
    Write-Config 'LoggingEnabled' 'true'
  } 

  if($null -ne $pp["EmailEnabled"] -and $pp["EmailEnabled"] -eq 'true')
  {
    Write-Config 'EmailEnabled' 'true'
  }

  if($null -ne $pp["EmailFrom"] -and $pp["EmailFrom"].Length -gt 0)
  {
    Write-Config 'EmailFrom' $pp["EmailFrom"]
  }

  if($null -ne $pp["EmailTo"] -and $pp["EmailTo"].Length -gt 0)
  {
    Write-Config 'EmailTo' $pp["EmailTo"]
  }

  if($null -ne $pp["SendGridUser"] -and $pp["SendGridUser"].Length -gt 0)
  {
    Write-Config 'SendGridUser' $pp["SendGridUser"]
  }

  if($null -ne $pp["SendGridPwd"] -and $pp["SendGridPwd"].Length -gt 0)
  {
    Write-Config 'SendGridPwd' $pp["SendGridPwd"]
  }  
}