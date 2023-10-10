import {Component} from '@angular/core';
import {BoxService} from "../../services/box-service";
import {BoxUpdateDto} from "../../interfaces/box-inteface";

@Component({
  selector: 'app-box-list',
  templateUrl: './list.component.html',
})
export class ListComponent {

  constructor(public boxService: BoxService) {
  }

  async deleteBox(boxId: string) {
    try {
      this.boxService.delete(boxId);
      this.boxService.boxes = this.boxService.boxes.filter(b => b.id != boxId);
    } catch (error) {
      console.error(error);
    }
  }

  async updateBox(boxId: string, boxUpdateDto: BoxUpdateDto) {
    try {
      this.boxService.update(boxId, boxUpdateDto);
    } catch (error) {
      console.error(error);
    }
  }
}
