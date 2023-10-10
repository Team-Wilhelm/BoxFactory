import { NgModule } from '@angular/core';
import {BoxListComponent} from "./boxlist.component";
import {ListComponent} from "./list/list.component";
import {CommonModule} from "@angular/common";
import {CreateboxComponent} from "./(create)/createbox.component";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";

@NgModule({
  declarations: [
    BoxListComponent,
    ListComponent,
    CreateboxComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [BoxListComponent]
})
export class BoxListModule { }
