import { InventoryService } from '../services/inventory.service';
import { Inventory } from './../interfaces/inventory';
import { Component, ChangeDetectorRef, ViewChild, Inject } from '@angular/core';
import { MediaMatcher } from '@angular/cdk/layout';

import { MenuEntry } from '../interfaces/menu-entry';
import { MenuCategories } from '../interfaces/menu-categories';
import { MatDrawer } from '@angular/material/sidenav';
import { MenuService } from '../services/menu.service';
import { InitEmptyPrinter, Printer } from '../interfaces/printer';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainComponent {
  mobileQuery: MediaQueryList;
  private mobileQueryListener: () => void;
  title = 'SagraPOS';
  categories: MenuCategories[] = []
  menuEntries: MenuEntry[] = []
   badgeCount :  Inventory[] = []; //TODO dictionary o right type
  //badgeCount :number; //dictionary o right type

  constructor(
    @Inject('BASE_URL') public baseUrl: string,
    private menuService: MenuService,
    public inventoryService:InventoryService,
    changeDetectorRef: ChangeDetectorRef,
    media: MediaMatcher) {
      // this.badgeCount = 1;
    this.mobileQuery = media.matchMedia('(max-width: 600px)');
    this.mobileQueryListener = () => changeDetectorRef.detectChanges(); // Is this variable necessary?
    this.mobileQuery.addListener(this.mobileQueryListener); // TODO fix this deprecation
  }

  @ViewChild('sidenav') sidenav!: MatDrawer;

  ngOnInit(): void {
    this.menuService.getCategories().subscribe(categories => this.categories = categories)
    this.menuService.getMenuEntries().subscribe(menuEntries => this.menuEntries = menuEntries)
    this.inventoryService.getQuantities().subscribe(badgeCount =>
      {
        this.badgeCount = badgeCount
        console.log(this.badgeCount[1])
      })
    setInterval(() => {
      this.inventoryService.getQuantities().subscribe(badgeCount =>
        {
          this.badgeCount = badgeCount
          console.log(this.badgeCount[3])
        })
      }, 5000);
  }

  ngAfterViewInit() {
    setTimeout(() => {
      // Avoid opening if on mobile
      if (!this.mobileQuery.matches)
        this.sidenav.open()
    })
  }
}
