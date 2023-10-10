import {Component} from '@angular/core';
import {OrderService} from "../../services/order-service";

@Component({
  selector: 'app-order-list',
  templateUrl: './list.component.html',
})
export class ListComponent {

  constructor(public orderService: OrderService) {
  }

  protected readonly Object = Object;
}
