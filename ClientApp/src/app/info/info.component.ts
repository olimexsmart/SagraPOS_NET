import { MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Component, ViewChild } from '@angular/core';
import { InfoOrders } from '../interfaces/info-orders';
import { InfoService } from '../services/info.service';

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
    private infoService: InfoService
  ) {
    this.infoOrders = {
      infoOrderEntries: [],
      ordersTotal: 0,
      numOrders: 0
    }
    this.tableDataSource = new MatTableDataSource(this.infoOrders.infoOrderEntries)
  }

  ngOnInit(): void {
    this.infoService.getInfoOrder().subscribe(infoOrders => {
      this.infoOrders = infoOrders
      this.tableDataSource = new MatTableDataSource(this.infoOrders.infoOrderEntries)
    })
  }
}
