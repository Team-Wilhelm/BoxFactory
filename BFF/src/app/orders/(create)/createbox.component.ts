import {Component} from '@angular/core';
import {BoxService} from "../../services/box-service";
import {BoxCreateDto} from "../../interfaces/box-inteface";
import {Dimensions} from "../../interfaces/dimension-interface";

@Component({
  selector: 'create-box',
  templateUrl: './createbox.component.html',
})
export class CreateboxComponent {
  box: BoxCreateDto;
  dimensions: Dimensions;

  constructor(public boxService: BoxService) {
    this.dimensions = {height: 0, width: 0, length: 0};
    this.box = {weight: 0, colour: "", material: "", dimensions: this.dimensions, price: 0, stock: 0};
  }

  async onCreateBox() {
    try {
      const createdbox = await this.boxService.create(this.box);
      this.boxService.boxes.push(createdbox);
      console.log(createdbox);
    } catch (error) {
      console.error(error);
    }
  }
}
