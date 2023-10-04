import {Component} from '@angular/core';
import {BoxService} from "../../services/box-service";

@Component({
  selector: 'app-box-list',
  templateUrl: './list.component.html',
})
export class ListComponent {

  constructor(public boxService: BoxService) {
  }
}
