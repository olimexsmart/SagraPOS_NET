import { MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Component, ViewChild } from '@angular/core';
import { InfoOrders } from '../interfaces/info-orders';
import { InfoService } from '../services/info.service';
import { ConfirmDialogModel, DialogPinComponent } from '../dialog-pin/dialog-pin.component';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from "@angular/material/form-field";


@Component({
  selector: 'app-info',
  templateUrl: './info.component.html',
  styleUrls: ['./info.component.css']
})
export class InfoComponent {
  displayedColumns: string[] = ['menuEntryName', 'quantitySold', 'totalSold', 'totalSoldPercentage', 'totalPercentage'];
  infoOrders: InfoOrders;
  tableDataSource;
  @ViewChild(MatSort) sort: MatSort = null!;

  constructor(
    private infoService: InfoService,
    private dialog: MatDialog
  ) {
    this.infoOrders = {
      infoOrderEntries: [],
      ordersTotal: 0,
      numOrders: 0
    }
    this.tableDataSource = new MatTableDataSource(this.infoOrders.infoOrderEntries)
  }

  ngOnInit(): void {
    this.refreshInfo()
  }

  clearInfo(): void {
    // TODO insert pin from a dialog
    // TODO Properly handle network errors
    const message = 'Tutti i dati andranno persi';
    const dialogData = new ConfirmDialogModel('Azzera info?', message);
    const dialogRef = this.dialog.open(DialogPinComponent, {
      maxWidth: '350px',
      data: dialogData,
    });
    dialogRef.afterClosed().subscribe((dialogResult) => {
      // TODO understand if instead of .value can be specified a strong type
      if(dialogResult.value === undefined) return
      this.infoService.resetInfoOrder(dialogResult.value).subscribe(
        {
          complete: this.refreshInfo.bind(this), // Love to know this hack by myself
          error: console.error // TODO snackbar with success/fail
        });
    });
  }

  private refreshInfo(): void {
    this.infoService.getInfoOrder().subscribe(infoOrders => {
      this.infoOrders = infoOrders
      this.tableDataSource = new MatTableDataSource(this.infoOrders.infoOrderEntries)
    })
  }
}
