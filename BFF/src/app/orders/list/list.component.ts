import {Component} from '@angular/core';
import {OrderService} from "../../services/order-service";

@Component({
  selector: 'app-box-list',
  templateUrl: './list.component.html',
})
export class ListComponent {

  constructor(public orderService: OrderService) {
  }
}
