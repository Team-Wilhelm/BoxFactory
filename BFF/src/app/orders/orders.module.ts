import { NgModule } from '@angular/core';
import {OrdersComponent} from "./orders.component";
import {ListComponent} from "./list/list.component";
import {CommonModule} from "@angular/common";
import {CreateboxComponent} from "./(create)/createbox.component";
import {FormsModule} from "@angular/forms";

@NgModule({
  declarations: [
    OrdersComponent,
    ListComponent,
    CreateboxComponent
  ],
  imports: [
    CommonModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [OrdersComponent]
})
export class OrdersModule { }
