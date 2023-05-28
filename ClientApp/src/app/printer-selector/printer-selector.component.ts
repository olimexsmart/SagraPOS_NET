import { Component, EventEmitter, Inject, Input, Output } from '@angular/core';
import { SettingsService } from '../services/settings.service';
import { Printer } from '../interfaces/printer';

@Component({
  selector: 'app-printer-selector',
  templateUrl: './printer-selector.component.html',
  styleUrls: ['./printer-selector.component.css']
})
export class PrinterSelectorComponent {
  printers: Printer[] = []

  @Input()  selectedPrinter: Printer = null!
  @Output() selectedPrinterChange = new EventEmitter<Printer>();

  constructor(
    @Inject('BASE_URL') public baseUrl: string,
    private settingService: SettingsService
  ) { }

  ngOnInit(): void {
    this.settingService.getPrinters().subscribe(printers => 
      {
        this.printers = printers
        this.selectedPrinter = printers[0]
      })
  }

  changeSelectedPrinter(p: Printer): void {
    this.selectedPrinter = p
    this.selectedPrinterChange.emit(p)
  }
}
