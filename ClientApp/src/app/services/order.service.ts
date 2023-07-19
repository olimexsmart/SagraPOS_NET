import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MenuEntry } from '../interfaces/menu-entry';
import { BooleanResult } from '../interfaces/boolean-result';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  postPrintOrder(printerID: number, order: Map<MenuEntry, number>): Observable<any> {
    let plainOrder: OrderEntryDTO[] = []
    for (const [key, value] of order) {
      plainOrder.push({
        EntryID: key.id,
        Quantity: value
      })
    }
    // Sending also time because server is assumed to not have accurate time info
    return this.http.post<any>(this.baseUrl + `ConfirmOrder?printerID=${printerID}&now=${new Date().toISOString()}`, plainOrder)
  }
}

interface OrderEntryDTO {
  EntryID: number,
  Quantity: number
}
