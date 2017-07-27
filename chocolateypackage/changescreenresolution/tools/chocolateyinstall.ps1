$ErrorActionPreference = 'Stop'; 

$packageName= 'changescreenresolution'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = '' # download url, HTTPS preferred
$url64      = '' # 64bit URL here (HTTPS preferred) or remove - if installer contains both (very rare), use $url
$fileLocation = Join-Path $toolsDir 'changescreenresolution.exe'
#$fileLocation = '\\SHARE_LOCATION\to\INSTALLER_FILE'

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  fileType      = 'exe'
  url           = $url
 # url64bit      = $url64
  file         = $fileLocation
  softwareName  = 'changescreenresolution*' #part or all of the Display Name as you see it in Programs and Features. It should be enough to be unique
  checksum      = '63AF2FA7F3A664B7FE7C7533BF73DDB2'
  checksumType  = 'md5' #default is md5, can also be sha1, sha256 or sha512
  #checksum64    = '347A26ACB7B348B9DA9A87CAD389EAA9C4E5DBFAE830D7D6153CCBF0D89C60DE'
  #checksumType64= 'sha256' #default is checksumType
}

function Write-Config($key, $value)
  {
    # update config file
      $appConfig = "$installLocation\tools\$packageName.exe.config"
      $doc = (Get-Content $appConfig) -as [Xml]

      $obj = $doc.configuration.appSettings.add | where {$_.Key -eq $key}
      $obj.value = $value
      $doc.Save($appConfig)
  }
# Install-ChocolateyPath 
# Install-ChocolateyPackage @packageArgs

 $packageName = $packageArgs.packageName
 $installLocation = $env:ChocolateyPackageFolder 
# # if (!$installLocation)  { Write-Warning "Can't find $packageName install location"; return }
# # Write-Host "$packageName installed to '$installLocation'"

# # Register-Application "$installLocation\$packageName.exe"
# # Write-Host "$packageName registered as $packageName"

$pp = Get-PackageParameters

if ($pp["RunAtStartup"] -ne $null -and $pp["RunAtStartup"] -eq 'true') 
{

  $resolution = "1920 1080"

  if($pp["resolution"] -ne $null)
  {
    $resource = $pp["resolution"]
  }

#  Install-ChocolateyShortcut -ShortcutFilePath "C:\Users\All Users\Microsoft\Windows\Start Menu\Programs\Startup\csr.lnk" -Description "Change Screen Reso" -Arguments $resolution -TargetPath $installLocation\$packageName.exe
  Install-ChocolateyShortcut -ShortcutFilePath "C:\Users\All Users\Microsoft\Windows\Start Menu\Programs\Startup\csr.lnk" -Description "Change Screen Reso" -Arguments $resolution -TargetPath $installLocation\tools\$packageName.exe

  if($pp["LoggingEnabled"] -ne $null -and $pp["LoggingEnabled"] -eq 'true')
  {
    Write-Config 'LoggingEnabled' 'true'
  } 

  if($pp["EmailEnabled"] -ne $null -and $pp["EmailEnabled"] -eq 'true')
  {
    Write-Config 'EmailEnabled' 'true'
  }

  if($pp["EmailFrom"] -ne $null -and $pp["EmailFrom"].Length -gt 0)
  {
    Write-Config 'EmailFrom' $pp["EmailFrom"]
  }

  if($pp["EmailTo"] -ne $null -and $pp["EmailTo"].Length -gt 0)
  {
    Write-Config 'EmailTo' $pp["EmailTo"]
  }

  if($pp["SendGridUser"] -ne $null -and $pp["SendGridUser"].Length -gt 0)
  {
    Write-Config 'SendGridUser' $pp["SendGridUser"]
  }

  if($pp["SendGridPwd"] -ne $null -and $pp["SendGridPwd"].Length -gt 0)
  {
    Write-Config 'SendGridPwd' $pp["SendGridPwd"]
  }


  
}