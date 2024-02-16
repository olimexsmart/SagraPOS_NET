# PROJECT ARCHIVED
This first version works well, but it's not easily deployable. It requires to be familiar with the terminal and some basic network configuration. 
A project with the same name con be found on my profile. The backend was rewritten in Node and it's packaged as an Electron app.

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
- [ ] Error in printing 'é' (e.g. caffé)
- [x] Orders are not printed in category order
- [ ] Print connection info at startup (with QR codes) (and from settings)
- [x] Add shutdown button and light
- [x] Manage two or more printers
- [x] Info page
- [x] Print info 
- [x] Reset info
- [x] DB table with info
- [x] Insert in DB real data
- [x] Setting table in DB (put there logo.jpg)
- [ ] Swap DB API
- [ ] Dialog for cash computations
- [x] Fix total new line overflow
- [ ] Better spacing in tablet view (kinda too crammed)
- [ ] Setting view (and then edit) page
- [ ] Product view (and then edit) page
- [x] Date and time should be coming from client
- [x] All assets (font, icons, ecc) loaded from LAN
- [ ] Document here build and deploy procedure (detailed)
- [x] Move printing to a separate class
- [ ] Setting an inventory value to null does not hide badge (caused by CSS)
- [ ] Remember order selection when changing screen
- [ ] Confirm OK/NOK snackbar when performing operations (like order)
- [x] Badge on receipt icon conting elements in order
- [x] Order euro sign and "x" on item quantities
- [ ] Complete log of events table (with its screen accessible from settings)
- [ ] Info table max width on large screens
- [ ] Periodic refresh with requestAnimationFrame
- [ ] Toggle to see all items in a unique list
- [ ] Settings to order with different logic items list (decided by the user)
- [ ] Log entries should carry prices along, because prices could change in time
- [ ] Custom order also for categories
- [ ] Cut categories separate by item categories
- [ ] Log also on which device the order was printed
- [ ] Optional recap print in settings
- [ ] Better recap layout
- [ ] Abstract PrintHelper and solve P3 printer layout differences

