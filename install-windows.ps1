$TARGET_DIR = "cs2\game\csgo"
$GAMEINFO_FILE = "$TARGET_DIR\gameinfo.gi"

Write-Output "Install SteamCMD"
winget install --id Valve.SteamCMD --exact --accept-source-agreements --disable-interactivity --accept-source-agreements --force

Write-Output "Create csgo folder"
New-Item -ItemType Directory -Force -Path $TARGET_DIR

Write-Output "Install CS2"
Start-Process -NoNewWindow -FilePath "$env:LOCALAPPDATA\Microsoft\WinGet\Links\steamcmd" -ArgumentList "+force_install_dir $PSScriptRoot\cs2\ +login Anonymous +app_update 730"
# Start-Process -NoNewWindow -FilePath "$env:LOCALAPPDATA\Microsoft\WinGet\Links\steamcmd" -ArgumentList "+force_install_dir $PSScriptRoot\cs2\ +login Anonymous +app_update  730 validate +exit"

Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"
Write-Output "Installed CS2"

Write-Output "Load MetaModVersion"
$latestMM = Invoke-RestMethod "https://mms.alliedmods.net/mmsdrop/2.0/mmsource-latest-windows"

Write-Output "Download MetaMod Version $latestMM"
Invoke-WebRequest "https://mms.alliedmods.net/mmsdrop/2.0/$latestMM" -OutFile "$TARGET_DIR\latestMM.zip"

Write-Output "Extract MetaMod Version $latestMM"
Expand-Archive "$TARGET_DIR\latestMM.zip" -DestinationPath $TARGET_DIR -Force

# Write-Output "Fix GameInfo"
# if (Test-Path $GAMEINFO_FILE) {
#     $NEW_ENTRY = "			Game	csgo/addons/metamod"
#     $SEL = Select-String -Path $GAMEINFO_FILE -Pattern $NEW_ENTRY
#     if ($SEL -ne $null) {
#         Write-Output "The entry '$NEW_ENTRY' already exists in $GAMEINFO_FILE. No changes were made."
#     }
#     else {
#         (Get-Content $GAMEINFO_FILE) |
#         Foreach-Object {
#             $_ # send the current line to output
#             if ($_ -match "Game_LowViolence") {
#                 #Add Lines after the selected pattern
#                 $NEW_ENTRY
#             }
#         } | Set-Content $GAMEINFO_FILE

#         Write-Output "The file $GAMEINFO_FILE has been modified successfully. '$NEW_ENTRY' has been added."
#     }
# }

Write-Output "Install CounterStrikeSharp"

# TODO Install CSS
# wget -q -O $(currentDir)/counterstrikesharp.zip $(shell curl -s -L -H "Accept: application/vnd.github+json" https://api.github.com/repos/roflmuffin/CounterStrikeSharp/releases/tags/$(shell dotnet list PugSharp/PugSharp.csproj package --format json | jq -r '.projects[].frameworks[].topLevelPackages[] | select(.id == "CounterStrikeSharp.API") | .resolvedVersion' | sed 's|1.0.|v|g') | jq -r '.assets[] | select(.browser_download_url | test("with-runtime")) | .browser_download_url')


Write-Output "Successfully prepared CS. To start: .\cs2\game\bin\win64\cs2.exe -dedicated +map de_dust2"