import {Component, ElementRef, EventEmitter, Output, ViewChild} from '@angular/core';
import {BoxService} from "../../services/box-service";
import {BoxUpdateDto} from "../../interfaces/box-inteface";
import {DimensionsDto} from "../../interfaces/dimension-interface";
import {FormControl, FormGroup, Validators} from "@angular/forms";
import {positiveNumberValidator} from "../../positiveNumberValidator";
import {min} from "rxjs";

@Component({
  selector: 'update-box',
  templateUrl: './updatebox.component.html',
})
export class UpdateboxComponent {
  box: BoxUpdateDto;
  dimensions: DimensionsDto;
  boxForm = new FormGroup({
    weight: new FormControl(0, [Validators.required, positiveNumberValidator]),
    length: new FormControl(0, [Validators.required, positiveNumberValidator]),
    width: new FormControl(0, [Validators.required, positiveNumberValidator]),
    height: new FormControl(0, [Validators.required, positiveNumberValidator]),
    price: new FormControl(0, [Validators.required, positiveNumberValidator]),
    stock: new FormControl(0, [Validators.required, Validators.min(0)]),
    colour: new FormControl(''),
    material: new FormControl(''),
  });

  constructor(public boxService: BoxService) {
    this.dimensions = {height: 0, width: 0, length: 0};
    this.box = {weight: 0, colour: "", material: "", dimensionsDto: this.dimensions, price: 0, stock: 0};
  }

  async onUpdateBox(event: Event) {
    console.log(this.boxForm.valid);
    if (this.boxForm.valid) {
      try {
        const createdBox = await this.boxService.create({
          weight: this.boxForm.controls.weight.value!,
          colour: this.boxForm.controls.colour.value || "",
          material: this.boxForm.controls.material.value || "",
          dimensionsDto: {
            height: this.boxForm.controls.height.value!,
            width: this.boxForm.controls.width.value!,
            length: this.boxForm.controls.length.value!
          },
          price: this.boxForm.controls.price.value!,
          stock: this.boxForm.controls.stock.value!
        });

        this.boxService.boxes.push(createdBox);
        this.boxForm.reset();
      } catch (error) {
        console.error(error);
      }
    } else {
      console.error("Invalid form");
    }
  }
}
