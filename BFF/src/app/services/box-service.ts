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
    this.get();
  }

  async get(searchTerm?: string) {
    const call = this.http.get<Box[]>(`${this.apiUrl}?boxesPerPage=1000&searchTerm=${searchTerm ?? ''}`);
    this.boxes = await firstValueFrom<Box[]>(call);
  }

  public getbyId(id: string) {
    return firstValueFrom(this.http.get<Box>(`${this.apiUrl}/${id}`));
  }

  public create(boxCreateDto: BoxCreateDto) {
    return firstValueFrom(this.http.post<Box>(`${this.apiUrl}`, boxCreateDto));
  }

  public update(id: string, boxUpdateDto: BoxUpdateDto) {
    return firstValueFrom(this.http.put<Box>(`${this.apiUrl}/${id}`, boxUpdateDto));
  }

  public delete(id: string) : Observable<any> {
    return this.http.delete(`${this.apiUrl}/box/${id}`);
  }

  public getColours(): string[] {
    return ['red', 'blue', 'green', 'yellow', 'black', 'white', 'brown', 'grey', 'orange', 'purple', 'pink', 'gold', 'silver', 'bronze', 'copper'];
  }

  public getMaterials(): string[] {
    return ['cardboard', 'plastic', 'wood', 'metal'];
  }
}
