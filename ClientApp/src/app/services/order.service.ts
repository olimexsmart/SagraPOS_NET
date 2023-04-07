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
    let plainOrder: any = {
      total: total,
      items: []
    }
    for(const [key, value] of order) {
      let plain : any = key
      plain.quantity = value
      plainOrder.items.push(plain)
    }
    console.log(plainOrder)
    return this.http.post<boolean>(this.baseUrl + 'ConfirmOrder', plainOrder)
  }
}
