import {Component, OnInit, ViewChild} from '@angular/core';

import {
  ChartComponent,
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexTitleSubtitle
} from "ng-apexcharts";
import {OrderService} from "../services/order-service";
import {Order} from "../interfaces/order-interface";


export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  yaxis?: ApexXAxis;
  title: ApexTitleSubtitle;
};

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  @ViewChild("chart") chart!: ChartComponent;
  public chartOptions: Partial<ChartOptions>;
  latestOrders: Order[] = [];
  ordersCount: number = 0;
  boxesSold: number = 0;
  totalRevenue: number = 0;
  data: number[] = [];

  constructor(public orderService: OrderService) {
    this.loadOrders();
    this.loadStatistics();
    this.fetchDataForChart().then(data => this.data = data);

    // TODO: Chart takes forever to load, fix this
    this.chartOptions = {
      series: [
        {
          name: "Orders",
          data: this.data
        }
      ],
      chart: {
        height: 350,
        type: "bar"
      },
      title: {
        text: "Order overview"
      },
      xaxis: {
        categories: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
      },
      yaxis: {
        min: 0,
        max: 10
      }
    };
  }

  async loadOrders() {
    try {
      this.latestOrders = await this.orderService.getLatest();
      console.log(this.latestOrders);
    } catch (error) {
      console.error('Error loading orders:', error);
    }
  }

  async loadStatistics() {
    try {
      this.ordersCount = await this.orderService.getOrdersCount();
      this.boxesSold = await this.orderService.getBoxesSold();
      this.totalRevenue = await this.orderService.getTotalRevenue();
    } catch (error) {
      console.error('Error loading orders:', error);
    }
  }

  async fetchDataForChart() {
    await this.orderService.get();
    const data = [] as number[];
    for (let i = 0; i < 12; i++) {
      data[i] = this.orderService.orders.filter(o => o.createdAt.getMonth() == i).length as number;
    }
    console.log(data);
    this.chartOptions.series![0] = {
      name: "Orders",
      data: data
    }
    return data;
  }
}
