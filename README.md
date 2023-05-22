# SagraPOS

Open Source receipt printing for simple events

# Build for RaspberryPi 3 or 4
The default IP is 192.168.1.5, change it in the script.
Enter three times the password, could be avoided setting an SSH keys.
The stops the service, uploads all the build files and restarts the service.
```
./confFilesBackup/buildUploadRPi.sh 
```

## To-Be-Done
- [x] Add shutdown button and light
- [ ] Manage two or more printers
- [x] Info page
- [ ] Print info 
- [ ] Reset info
- [x] DB table with info
- [ ] Print connection info at startup (with QR codes) (and from settings)
- [x] Insert in DB real data
- [ ] Setting table in DB (put there logo.jpg)
- [ ] Swap DB API
- [ ] Dialog for cash computations
- [ ] Fix total new line overflow
- [ ] Better spacing in tablet view (kinda too crammed)
- [ ] Setting view (and then edit) page
- [ ] Product view (and then edit) page
- [ ] Date and time should be coming from client
- [x] All assets (font, icons, ecc) loaded from LAN
- [ ] Document here build and deploy procedure (detailed)
- [ ] Move printing to a separate class

