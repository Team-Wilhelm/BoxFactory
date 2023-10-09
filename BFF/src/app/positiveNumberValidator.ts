import {AbstractControl, FormControl, ValidationErrors, ValidatorFn} from "@angular/forms";

export function positiveNumberValidator() : ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const positive = control.value > 0;
    console.log('Control value:', control.value); // Add this line
    console.log('Is positive?', positive); // And this line
    return positive ? null : {positiveNumber: {value: control.value}}
  };
}
