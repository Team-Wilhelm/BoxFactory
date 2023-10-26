import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {firstValueFrom} from 'rxjs';
import {Order, OrderCreateDto, ShippingStatusDto} from "../interfaces/order-interface";

@Injectable({
  providedIn: 'root'
})
export class StatsService {
  private apiUrl = environment.baseURL + '/stats';

  constructor(private http: HttpClient) {

  }

  public async getOrdersCountByMonth(): Promise<Map<number, number>> {
    const call = this.http.get<{ [key: number]: number }>(`${this.apiUrl}`);
    return new Map(Object.entries(await firstValueFrom(call)).map(([key, value]) => [parseInt(key), value]));
  }
}
