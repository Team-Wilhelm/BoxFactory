import {Component} from "@angular/core";
import {OrderService} from "../../services/order-service";

@Component({
  selector: 'order-row',
  templateUrl: './order-row.component.html',
})
export class OrderRowComponent {
  constructor(public orderService: OrderService) {
  }
}
