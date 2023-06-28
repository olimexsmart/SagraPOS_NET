import { Component } from '@angular/core';
import { ConfirmDialogModel, DialogPinComponent } from '../dialog-pin/dialog-pin.component';
import { MatDialog } from '@angular/material/dialog';
import { DialogUpdateInventoryComponent } from '../dialog-update-inventory/dialog-update-inventory.component';

@Component({
  selector: 'app-inventory-mode',
  templateUrl: './inventory-mode.component.html',
  styleUrls: ['./inventory-mode.component.css']
})
export class InventoryModeComponent {
  modeActive: boolean = false

  constructor(private dialog: MatDialog) { }

  public isActive(): boolean {
    return this.modeActive
  }

  public updateInventory(entryID: number): void {
    console.log(entryID)
    const dialogData = new ConfirmDialogModel('Attivare?', 'bomber');
    const dialogRef = this.dialog.open(DialogUpdateInventoryComponent, {
      maxWidth: '350px',
      data: dialogData,
    });
    dialogRef.afterClosed().subscribe((dialogResult) => {
      if (dialogResult.value === undefined) return
      console.log(dialogResult.value)
    });
  }

  toggleMode() {
    if (this.modeActive) {
      this.modeActive = false
    }
    else {
      const message = 'Modalità Inventario';
      const dialogData = new ConfirmDialogModel('Attivare?', message);
      const dialogRef = this.dialog.open(DialogPinComponent, {
        maxWidth: '350px',
        data: dialogData,
      });
      dialogRef.afterClosed().subscribe((dialogResult) => {
        if (dialogResult.value === undefined) return
        this.modeActive = true
      });
    }
  }
}
