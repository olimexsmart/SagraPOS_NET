import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Inventory } from '../interfaces/inventory';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

   getQuantities(): Observable<Inventory[]> {
    return this.http.get<Inventory[]>(this.baseUrl + `GetQuantities`)
  }
}

