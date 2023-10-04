import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {firstValueFrom, Observable} from 'rxjs';
import {Box, BoxCreateDto, BoxUpdateDto} from "../interfaces/box-inteface";
import {Order} from "../interfaces/order-interface";

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  orders: Order[] = [];
  private apiUrl = 'http://localhost:5133/box';
  constructor(private http: HttpClient) {
    this.getlocal().then(r => console.log(this.orders));
  }

  async get() {
    const call = this.http.get<Order[]>(`${this.apiUrl}`);
    this.orders = await firstValueFrom<Order[]>(call);
  }

  async getlocal(){
    this.orders = [
      {
        id: 'b33faf20-114c-4f8d-bb56-47c1eeca77c1',
        status: 'Pending',
        createdAt: new Date(),
        updatedAt: new Date(),
        boxes: [
          {
            id: '1',
            weight: 10,
            colour: 'Red',
            material: 'Wood',
            dimensions: { width: 10, height: 10, length: 10},
            createdAt: new Date(),
            stock: 5,
            price: 50
          }
        ],
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
        status: 'Delivered',
        createdAt: new Date(),
        updatedAt: new Date(),
        boxes: [
          {
            id: '2',
            weight: 12,
            colour: 'Blue',
            material: 'Metal',
            dimensions: { width: 12, height: 12, length: 12},
            createdAt: new Date(),
            stock: 10,
            price: 75
          },
          {
            id: '3',
            weight: 15,
            colour: 'Green',
            material: 'Plastic',
            dimensions: { width: 15, height: 15, length: 15},
            createdAt: new Date(),
            stock: 8,
            price: 90
          }
        ],
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

  public create(boxCreateDto: BoxCreateDto) {
    return firstValueFrom(this.http.post<Order>(`${this.apiUrl}`, boxCreateDto));
  }

  public update(id: string, boxUpdateDto: BoxUpdateDto) {
    return firstValueFrom(this.http.put<Order>(`${this.apiUrl}/${id}`, boxUpdateDto));
  }

  public delete(id: string) : Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
