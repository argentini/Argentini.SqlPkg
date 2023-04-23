echo ""
echo "Making SqlPkg executable and modifying your system \$PATH..."
sudo chmod +x /usr/bin/sqlpkg/sqlpkg
sed -i -z "s/export PATH=\$PATH:\/usr\/bin\/sqlpkg\n//" ~/.bashrc
echo "export PATH=\$PATH:/usr/bin/sqlpkg" >> ~/.bashrc
export PATH=$PATH:/usr/bin/sqlpkg
echo "DONE!"
echo ""
