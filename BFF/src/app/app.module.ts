import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavMenuComponent } from "./nav-menu/nav-menu.component";
import { NgApexchartsModule } from "ng-apexcharts";
import {HomeComponent} from "./home/home.component";
import {BoxListModule} from "./boxlist/boxlist.module";
import {NG_VALIDATORS, ReactiveFormsModule} from "@angular/forms";
import {positiveNumberValidator} from "./positiveNumberValidator";
import {OrdersModule} from "./orders/orders.module";
import {OrderRowComponent} from "./home/order-row/order-row.component";
import {OrderService} from "./services/order-service";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    OrderRowComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    NgApexchartsModule,
    BoxListModule,
    HttpClientModule,
    ReactiveFormsModule,
    OrdersModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
