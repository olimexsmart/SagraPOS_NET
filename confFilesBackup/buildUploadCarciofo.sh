#!/bin/bash

dotnet publish --runtime linux-x64 --self-contained --configuration Release

#ssh olli@192.168.1.6 "sudo systemctl stop SagraPOS"
scp -r bin/Release/net7.0/linux-x64/publish olli@192.168.1.6:/home/olli/SagraPOS/
#ssh olli@192.168.1.6 "sudo systemctl start SagraPOS"