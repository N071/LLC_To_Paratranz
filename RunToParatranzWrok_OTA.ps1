Remove-Item Debug/Localize/ -Recurse
Remove-Item Debug/Localize_OTA/ -Recurse
Start-Process -FilePath "Debug/LLC_To_Paratranz.exe" -ArgumentList "ToParatranzWrok" -Wait

$commitMessage = $(Get-Date)
git add Debug/Localize/
git commit -m $commitMessage
$changedFiles=$(git diff --name-only HEAD HEAD^ -- Debug/Localize/)

New-Item -Path "Debug" -Name "Localize_OTA" -ItemType "directory" -Force
$changedFilesList = $changedFiles -split " "
foreach ($file in $changedFilesList) {
    if (Test-Path -Path $file) {
        $destination = "Debug/Localize_OTA/$file"
        $destination = $destination.Replace("Debug/Localize/", "")
        $destinationDirectory = Split-Path -Path $destination -Parent
        if (!(Test-Path -Path $destinationDirectory)) {
            New-Item -ItemType Directory -Force -Path $destinationDirectory
        }
        Copy-Item -Path $file -Destination $destination -Force -Recurse
    }
}