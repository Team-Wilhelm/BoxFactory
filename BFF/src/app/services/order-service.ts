import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {firstValueFrom, Observable} from 'rxjs';
import {BoxCreateDto, BoxUpdateDto} from "../interfaces/box-inteface";
import {Order, OrderCreateDto, ShippingStatus} from "../interfaces/order-interface";

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

  async getlocal(){
    this.orders = [
      {
        id: 'b33faf20-114c-4f8d-bb56-47c1eeca77c1',
        shippingStatus: ShippingStatus.Received,
        createdAt: new Date(),
        updatedAt: new Date(),
        boxes: {
          "1": 10,
        },
        customer: {
          firstName: 'John',
          lastName: 'Doe',
          email: 'john.doe@example.com',
          address: { houseNumber: 23,houseNumberAddition:'', streetName: 'Main St', city: 'Hometown', country: 'CA', postalCode: '12345' },
          phoneNumber: '123-456-7890'
        }
      },
      {
        id: 'a12bc34d-e56f-78g9-0hi1-234j567k890l',
        shippingStatus: ShippingStatus.Shipped,
        createdAt: new Date(),
        updatedAt: new Date(),
        boxes: {
          "2": 5,
          "3": 2,
        },
        customer: {
          firstName: 'Jane',
          lastName: 'Smith',
          email: 'jane.smith@example.com',
          address: { houseNumber: 45,houseNumberAddition:'', streetName: 'Market St', city: 'Newtown', country: 'US', postalCode: '67890' },
          phoneNumber: '098-765-4321'
        }
      }
    ];
  }

  public getbyId(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${id}`);
  }

  public create(orderCreateDto: OrderCreateDto) {
    return firstValueFrom(this.http.post<Order>(`${this.apiUrl}`, orderCreateDto));
  }

  public update(id: string, boxUpdateDto: BoxUpdateDto) {
    return firstValueFrom(this.http.put<Order>(`${this.apiUrl}/${id}`, boxUpdateDto));
  }

  public delete(id: string) : Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  public async getLatest() : Promise<Order[]> {
    const call = this.http.get<Order[]>(`${this.apiUrl}/Latest`);
    return await firstValueFrom(call);
  }
}
