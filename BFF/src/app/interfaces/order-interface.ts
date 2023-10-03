import {Box} from "./box-inteface";
import {Customer} from "./customer-interface";

export interface Order {
  id: string; // Guids are represented as strings in Typescript
  status?: string; // string? is represented as Optional String in TypeScript
  createdAt: Date; // DateTime in C# = Date in TypeScript
  updatedAt?: Date; // Nullable DateTime translates to an optional Date in TypeScript
  boxes?: Box[]; // Assuming Box is another interface we defined in TypeScript. List<T> in C# = T[] in TypeScript
  customer?: Customer; // Assuming Customer is another interface we defined in TypeScript
}
