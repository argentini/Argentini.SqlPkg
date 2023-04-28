source publish-linux-arm64.sh
source publish-linux-x64.sh
source publish-macos-arm64.sh
source publish-macos-x64.sh
source publish-windows-arm64.sh
source publish-windows-x64.sh

cp -r Argentini.SqlPkg/bin/Release/net7.0/linux-arm64/publish Installers/Linux-Arm64/
cp -r Argentini.SqlPkg/bin/Release/net7.0/linux-x64/publish Installers/Linux-x64/
cp -r Argentini.SqlPkg/bin/Release/net7.0/osx-arm64/publish Installers/macOS-Arm64/
cp -r Argentini.SqlPkg/bin/Release/net7.0/osx-x64/publish Installers/macOS-x64/
cp -r Argentini.SqlPkg/bin/Release/net7.0/win-arm64/publish Installers/Windows-Arm64/
cp -r Argentini.SqlPkg/bin/Release/net7.0/win-x64/publish Installers/Windows-x64/

cp blank.dacpac Installers/Linux-Arm64/publish/
cp blank.dacpac Installers/Linux-x64/publish/
cp blank.dacpac Installers/macOS-Arm64/publish/
cp blank.dacpac Installers/macOS-x64/publish/
cp blank.dacpac Installers/Windows-Arm64/publish/
cp blank.dacpac Installers/Windows-x64/publish/
