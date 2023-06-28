import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ConfirmDialogModel } from '../dialog-pin/dialog-pin.component';

@Component({
  selector: 'app-dialog-update-inventory',
  templateUrl: './dialog-update-inventory.component.html',
  styleUrls: ['./dialog-update-inventory.component.css']
})
export class DialogUpdateInventoryComponent {
  form: FormGroup = new FormGroup({
    newQuantity: new FormControl(),
  });

  constructor(public dialogRef: MatDialogRef<DialogUpdateInventoryComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel) {
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(this.form.controls["newQuantity"]);
  }

  onDismiss(): void {
    // Close the dialog, return undefined
    this.dialogRef.close()
  }
}
