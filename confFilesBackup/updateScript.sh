#!/bin/bash

systemctl stop SagraPOS

cp bin/Release/net7.0/linux-arm64/publish/SagraPOS.sqlite3 SagraPOS.sqlite3
cp bin/Release/net7.0/linux-arm64/publish/logo.jpg logo.jpg
rm -rf bin/

unzip SagraPOS-arm64.zip

chmod a+x bin/Release/net7.0/linux-arm64/publish/SagraPOS
mv SagraPOS.sqlite3 bin/Release/net7.0/linux-arm64/publish/SagraPOS.sqlite3
mv logo.jpg bin/Release/net7.0/linux-arm64/publish/logo.jpg

systemctl start SagraPOS