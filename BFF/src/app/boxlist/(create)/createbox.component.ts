import {Component, ElementRef, EventEmitter, Output, ViewChild} from '@angular/core';
import {BoxService} from "../../services/box-service";
import {BoxCreateDto} from "../../interfaces/box-inteface";
import {Dimensions} from "../../interfaces/dimension-interface";
import {FormControl, FormGroup, Validators} from "@angular/forms";
import {positiveNumberValidator} from "../../positiveNumberValidator";

@Component({
  selector: 'create-box',
  templateUrl: './createbox.component.html',
})
export class CreateboxComponent {
  @ViewChild('create_box') createBox!: ElementRef<HTMLDialogElement>;

  box: BoxCreateDto;
  dimensions: Dimensions;
  boxForm = new FormGroup({
    weight: new FormControl('', [Validators.required, positiveNumberValidator]),
    length: new FormControl('', [Validators.required, positiveNumberValidator]),
    width: new FormControl('', [Validators.required, positiveNumberValidator]),
    height: new FormControl('', [Validators.required, positiveNumberValidator]),
    price: new FormControl('', [Validators.required, positiveNumberValidator]),
    stock: new FormControl('', [Validators.required, positiveNumberValidator]),
    colour: new FormControl(''),
    material: new FormControl(''),
  });

  constructor(public boxService: BoxService) {
    this.dimensions = {height: 0, width: 0, length: 0};
    this.box = {weight: 0, colour: "", material: "", dimensionsDto: this.dimensions, price: 0, stock: 0};
  }

  async onCreateBox() {
    console.log(this.boxForm.valid);
    if (this.boxForm.valid) {
      try {
        const createdBox = await this.boxService.create({
          weight: Number.parseInt(this.boxForm.controls.weight.value!),
          colour: this.boxForm.controls.colour.value || "",
          material: this.boxForm.controls.material.value || "",
          dimensionsDto: {
            height: Number.parseInt(this.boxForm.controls.height.value!),
            width: Number.parseInt(this.boxForm.controls.width.value!),
            length: Number.parseInt(this.boxForm.controls.length.value!)
          },
          price: Number.parseInt(this.boxForm.controls.price.value!),
          stock: Number.parseInt(this.boxForm.controls.stock.value!)
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
