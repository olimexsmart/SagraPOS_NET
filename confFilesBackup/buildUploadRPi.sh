#!/bin/bash

dotnet publish --runtime linux-arm64 --self-contained --configuration Release

ssh olli@192.168.1.5 "sudo systemctl stop SagraPOS"
scp -r bin/Release/net7.0/linux-arm64/publish olli@192.168.1.5:/home/olli/SagraPOS/
ssh olli@192.168.1.5 "sudo systemctl start SagraPOS"