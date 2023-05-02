#!/bin/bash
echo ">>> Installing Microsoft SqlPackage..."
dotnet tool install -g microsoft.sqlpackage
echo ">>> Create SqlPkg application folder..."
[[ -d /Applications/SqlPkg ]] || sudo mkdir /Applications/SqlPkg
echo ">>> Copy SqlPkg files..."
sudo rsync -avPz --delete-before publish/ /Applications/SqlPkg
echo ">>> Add SqlPkg to system path..."
echo "/Applications/SqlPkg" >> SqlPkg.public
sudo cp SqlPkg.public /private/etc/paths.d/SqlPkg.public
rm -rf SqlPkg.public
echo ">>> SUCCESS!"
echo ">>> Restart your terminal to begin using SqlPkg"
