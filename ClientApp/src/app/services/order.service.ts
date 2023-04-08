import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MenuEntry } from '../interfaces/menu-entry';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  postPrintOrder(order : Map<MenuEntry, number>, total :  number) : Observable<boolean> {
    let plainOrder: OrderEntryDTO[] = []
    for(const [key, value] of order) {
      plainOrder.push({
        EntryID: key.id,
        Quantity: value
      })
    }
    console.log(plainOrder)
    return this.http.post<boolean>(this.baseUrl + 'ConfirmOrder', plainOrder)
  }
}

interface OrderEntryDTO
{
    EntryID: number,
    Quantity: number
}
