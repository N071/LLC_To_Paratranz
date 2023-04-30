param(
    [string]$ParatranzAPI
)
$url = "https://paratranz.cn/api/projects/6860/artifacts/download"
$headers = @{
    "Authorization" = "$ParatranzAPI"
    "accept" = "*/*"
}

$response = Invoke-WebRequest -Uri $url -Headers $headers -Method Get
[IO.File]::WriteAllBytes("test.zip", $response.Content)

# Unzip the file to the current directory
Expand-Archive -Path "test.zip" -DestinationPath "."

# Run the RunDeleteParatranzWrok.bat script
& ".\RunDeleteParatranzWrok.bat"

# Copy the utf8\Localize folder to the Debug folder
Copy-Item -Path ".\utf8\Localize" -Destination ".\Debug" -Recurse

# Run the RunToGitHubWrok.bat script
& ".\RunToGitHubWrok.bat"

# Change directory to the LocalizeLimbusCompany repository
Set-Location "D:\a\LLC_To_Paratranz\LLC_To_Paratranz\LLC_Test"

# Add all changes to the repository
git add .

$commitMessage = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
git commit -m $commitMessage
git push https://github.com/LocalizeLimbusCompany/LLC_Test.git