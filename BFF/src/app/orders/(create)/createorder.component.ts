import {Component} from '@angular/core';
import {BoxService} from "../../services/box-service";
import {Dimensions, DimensionsDto} from "../../interfaces/dimension-interface";
import {OrderCreateDto} from "../../interfaces/order-interface";
import {Box} from "../../interfaces/box-inteface";
import {CreateCustomerDto} from "../../interfaces/customer-interface";
import {CreateAddressDto} from "../../interfaces/address-interface";
import {OrderService} from "../../services/order-service";
import {FormControl, Validators} from "@angular/forms";

@Component({
  selector: 'create-box',
  templateUrl: './createorder.component.html',
})
export class CreateorderComponent {
  order: OrderCreateDto;
  customer: CreateCustomerDto;
  address: CreateAddressDto;
  boxes: Box[];
  addedBoxes: Record<string, number> = {};
  activeTab: string;
  amountFormControl = new FormControl(1, [Validators.required, Validators.min(1)]);

  constructor(public boxService: BoxService, public orderService: OrderService) {
    this.boxes = boxService.boxes;
    this.address = {
      streetName: "",
      city: "",
      country: "",
      postalCode: "",
      houseNumber: 0,
      houseNumberAddition: "",
    }
    this.customer = {
      firstName: "",
      lastName: "",
      email: "",
      phoneNumber: "",
      address: this.address,
    }
    this.order = {
      boxes: {},
      customer: this.customer,
    }
    this.activeTab = "box-tab";
  }

  changeTab(tabId: string): void {
    this.activeTab = tabId;
  }

  addBox(boxId: string, boxAmount: string, event: Event): void {
    event.stopPropagation();
    this.addedBoxes[boxId] = Number(boxAmount);
    this.order.boxes = this.addedBoxes;
    boxId = "";
    boxAmount = "";
  }

  removeBox(boxId: string, event: Event): void {
    event.stopPropagation();
    delete this.addedBoxes[boxId]; // removes the property from the object
  }

  async onCreateOrder() {
    await this.orderService.create(this.order);
  }

  protected readonly parseInt = parseInt;
}
