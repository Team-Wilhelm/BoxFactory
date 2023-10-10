import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {firstValueFrom, Observable} from 'rxjs';
import {Box, BoxCreateDto, BoxUpdateDto} from "../interfaces/box-inteface";

@Injectable({
  providedIn: 'root'
})
export class BoxService {
  boxes: Box[] = [];
  private apiUrl = 'http://localhost:5133/box';
  constructor(private http: HttpClient) {
    this.getlocal().then(r => console.log(this.boxes));
    //this.get().then(r => console.log(this.boxes));
  }

  async get() {
    const call = this.http.get<Box[]>(`${this.apiUrl}`);
    this.boxes = await firstValueFrom<Box[]>(call);
  }

  async getlocal(){
    this.boxes = [
      {
        id: "3e0c4000-28e1-46c4-b2e6-1c4bad9128b1",
        weight: 5.3,
        colour: "blue",
        material: "wood",
        dimensions: { width: 2, length: 3, height: 1 },
        createdAt: new Date(2022, 2, 1),
        stock: 20,
        price: 49.99
      },
      {
        id: "1d678c2e-990f-4129-959e-8cc1ecf8691d",
        weight: 7.2,
        colour: "red",
        material: "plastic",
        createdAt: new Date(2022, 2, 5),
        stock: 15,
        price: 29.99
      },
      {
        id: "15afb791-f4fd-4b5c-beaf-5367fcbdfb55",
        weight: 3.7,
        colour: "green",
        dimensions: { width: 1, length: 2, height: 1 },
        createdAt: new Date(2022, 3, 1),
        stock: 30,
        price: 19.99
      }
    ];
  }

  public getbyId(id: string): Observable<Box> {
    return this.http.get<Box>(`${this.apiUrl}/${id}`);
  }

  public create(boxCreateDto: BoxCreateDto) {
    return firstValueFrom(this.http.post<Box>(`${this.apiUrl}`, boxCreateDto));
  }

  public update(id: string, boxUpdateDto: BoxUpdateDto) {
    return firstValueFrom(this.http.put<Box>(`${this.apiUrl}/${id}`, boxUpdateDto));
  }

  public delete(id: string) {
    return firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
  }
}
