import {Component, ElementRef, EventEmitter, Output, ViewChild} from '@angular/core';
import {BoxService} from "../../services/box-service";
import {BoxCreateDto} from "../../interfaces/box-inteface";
import {Dimensions} from "../../interfaces/dimension-interface";
import {FormControl, FormGroup, Validators} from "@angular/forms";
import {positiveNumberValidator} from "../../positiveNumberValidator";
import {min} from "rxjs";

@Component({
  selector: 'create-box',
  templateUrl: './createbox.component.html',
})
export class CreateboxComponent {
  box: BoxCreateDto;
  dimensions: Dimensions;
  boxForm = new FormGroup({
    weight: new FormControl(0, [Validators.required, positiveNumberValidator]),
    length: new FormControl(0, [Validators.required, positiveNumberValidator]),
    width: new FormControl(0, [Validators.required, positiveNumberValidator]),
    height: new FormControl(0, [Validators.required, positiveNumberValidator]),
    price: new FormControl(0, [Validators.required, positiveNumberValidator]),
    stock: new FormControl(0, [Validators.required]),
    colour: new FormControl(''),
    material: new FormControl(''),
  });

  constructor(public boxService: BoxService) {
    this.dimensions = {height: 0, width: 0, length: 0};
    this.box = {weight: 0, colour: "", material: "", dimensionsDto: this.dimensions, price: 0, stock: 0};
  }

  async onCreateBox(event: Event) {
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
      } catch (error) {
        console.error(error);
      }
    } else {
      console.error("Invalid form");
    }
  }
}
