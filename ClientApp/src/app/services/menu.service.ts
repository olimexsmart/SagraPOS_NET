import { Inject, Injectable } from '@angular/core';
import { MenuCategories } from '../interfaces/menu-categories';
import { MenuEntry } from '../interfaces/menu-entry';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MenuService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  getCategories(): Observable<MenuCategories[]> {
    return this.http.get<MenuCategories[]>(this.baseUrl + `GetCategories`)
  }

  getMenuEntries(): Observable<MenuEntry[]> {
    return this.http.get<MenuEntry[]>(this.baseUrl + `GetEntries`)
  }
}
