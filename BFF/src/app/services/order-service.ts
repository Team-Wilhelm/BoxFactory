import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {firstValueFrom, Observable} from 'rxjs';
import {BoxCreateDto, BoxUpdateDto} from "../interfaces/box-inteface";
import {Order, OrderCreateDto, ShippingStatus, ShippingStatusDto} from "../interfaces/order-interface";

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  orders: Order[] = [];
  private apiUrl = 'http://localhost:5133/Order';
  constructor(private http: HttpClient) {
    this.get().then(r => console.log(this.orders));
  }

  async get() {
    const call = this.http.get<Order[]>(`${this.apiUrl}`);
    this.orders = await firstValueFrom<Order[]>(call);
  }

  public getbyId(id: string) {
    return firstValueFrom(this.http.get<Order>(`${this.apiUrl}/${id}`));
  }

  public create(orderCreateDto: OrderCreateDto) {
    return firstValueFrom(this.http.post<Order>(`${this.apiUrl}`, orderCreateDto));
  }

  public update(id: string, boxUpdateDto: BoxUpdateDto) {
    return firstValueFrom(this.http.put<Order>(`${this.apiUrl}/${id}`, boxUpdateDto));
  }

  public delete(id: string) {
    return firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
  }

  public updateStatus(id: string, status: ShippingStatusDto) {
    return firstValueFrom(this.http.patch<Order>(`${this.apiUrl}/${id}`, status));
  }
}
