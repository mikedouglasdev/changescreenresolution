# Delete shortcut if exists

$shortcut = "C:\Users\All Users\Microsoft\Windows\Start Menu\Programs\Startup\csr.lnk"
write-host "Removing shortcut (if exists)"
if(test-path $shortcut)
{
    remove-item $shortcut
}