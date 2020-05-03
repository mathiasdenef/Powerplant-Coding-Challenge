import { Component, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ProductionPlanService } from '../services/production-plan.service';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-production-plan-component',
  templateUrl: './production-plan.component.html'
})
export class ProductionPlanComponent {

  editorOptionsRequest: JsonEditorOptions = new JsonEditorOptions();
  editorOptionsResponse: JsonEditorOptions = new JsonEditorOptions();
  requestData: any;
  responseData: any;
  responseSignalR: any;

  errorMessage: string;
  subscription: Subscription;

  @ViewChild("jsonEditorRequest", { static: true }) editorRequest: JsonEditorComponent;
  @ViewChild("jsonEditorResponse", { static: true }) editorResponse: JsonEditorComponent;
  @ViewChild("jsonEditorSignalR", { static: true }) editorSignalR: JsonEditorComponent;

  constructor(private productionPlanService: ProductionPlanService) {
  }

  ngOnInit() {
    this.initTestObject();
    this.editorOptionsRequest.mode = 'code'; // set all allowed modes
    this.editorOptionsRequest.mainMenuBar = false;
    this.editorOptionsRequest.statusBar = false;

    this.editorOptionsResponse.mode = 'view'; // set all allowed modes
    this.editorOptionsResponse.mainMenuBar = false;
    this.editorOptionsResponse.statusBar = false;
    this.editorOptionsResponse.navigationBar = false;
    //this.options.mode = 'code'; //set only one mode

    this.subscription = this.productionPlanService.productionPlanReceived.asObservable().subscribe(
      productionPlan => {
        this.responseSignalR = productionPlan;
        // Workaround, expandAll will be called after all change detection is triggered
        setTimeout(() => this.editorSignalR.expandAll(), 0);
      }
    )
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  initTestObject() {
    this.requestData = {
      "load": 480,
      "fuels":
      {
        "gas(euro/MWh)": 13.4,
        "kerosine(euro/MWh)": 50.8,
        "co2(euro/ton)": 20,
        "wind(%)": 60
      },
      "powerplants": [
        {
          "name": "gasfiredbig1",
          "type": "gasfired",
          "efficiency": 0.53,
          "pmin": 100,
          "pmax": 460
        },
        {
          "name": "gasfiredbig2",
          "type": "gasfired",
          "efficiency": 0.53,
          "pmin": 100,
          "pmax": 460
        },
        {
          "name": "gasfiredsomewhatsmaller",
          "type": "gasfired",
          "efficiency": 0.37,
          "pmin": 40,
          "pmax": 210
        },
        {
          "name": "tj1",
          "type": "turbojet",
          "efficiency": 0.3,
          "pmin": 0,
          "pmax": 16
        },
        {
          "name": "windpark1",
          "type": "windturbine",
          "efficiency": 1,
          "pmin": 0,
          "pmax": 150
        },
        {
          "name": "windpark2",
          "type": "windturbine",
          "efficiency": 1,
          "pmin": 0,
          "pmax": 36
        }
      ]
    };
  }

  onClickButton() {
    console.log("requestData", this.editorRequest.get());

    this.productionPlanService.getProductionPlan(this.editorRequest.get()).subscribe(
      response => {
        this.errorMessage = null;
        this.responseData = response;
        // Workaround, expandAll will be called after all change detection is triggered
        setTimeout(() => this.editorResponse.expandAll(), 0);
      },
      error => {
        this.errorMessage = error.error ? error.error : error.message;
      }
    );
  }


  onClickLoad(scenarioNumber: number) {
    switch (scenarioNumber) {
      case 1:
        this.requestData = {
          "load": 480,
          "fuels":
          {
            "gas(euro/MWh)": 13.4,
            "kerosine(euro/MWh)": 50.8,
            "co2(euro/ton)": 20,
            "wind(%)": 60
          },
          "powerplants": [
            {
              "name": "gasfiredbig1",
              "type": "gasfired",
              "efficiency": 0.53,
              "pmin": 100,
              "pmax": 460
            },
            {
              "name": "gasfiredbig2",
              "type": "gasfired",
              "efficiency": 0.53,
              "pmin": 100,
              "pmax": 460
            },
            {
              "name": "gasfiredsomewhatsmaller",
              "type": "gasfired",
              "efficiency": 0.37,
              "pmin": 40,
              "pmax": 210
            },
            {
              "name": "tj1",
              "type": "turbojet",
              "efficiency": 0.3,
              "pmin": 0,
              "pmax": 16
            },
            {
              "name": "windpark1",
              "type": "windturbine",
              "efficiency": 1,
              "pmin": 0,
              "pmax": 150
            },
            {
              "name": "windpark2",
              "type": "windturbine",
              "efficiency": 1,
              "pmin": 0,
              "pmax": 36
            }
          ]
        }
        break;
      case 2:
        this.requestData = {
          "load": 480,
          "fuels":
          {
            "gas(euro/MWh)": 13.4,
            "kerosine(euro/MWh)": 50.8,
            "co2(euro/ton)": 20,
            "wind(%)": 0
          },
          "powerplants": [
            {
              "name": "gasfiredbig1",
              "type": "gasfired",
              "efficiency": 0.53,
              "pmin": 100,
              "pmax": 460
            },
            {
              "name": "gasfiredbig2",
              "type": "gasfired",
              "efficiency": 0.53,
              "pmin": 100,
              "pmax": 460
            },
            {
              "name": "gasfiredsomewhatsmaller",
              "type": "gasfired",
              "efficiency": 0.37,
              "pmin": 40,
              "pmax": 210
            },
            {
              "name": "tj1",
              "type": "turbojet",
              "efficiency": 0.3,
              "pmin": 0,
              "pmax": 16
            },
            {
              "name": "windpark1",
              "type": "windturbine",
              "efficiency": 1,
              "pmin": 0,
              "pmax": 150
            },
            {
              "name": "windpark2",
              "type": "windturbine",
              "efficiency": 1,
              "pmin": 0,
              "pmax": 36
            }
          ]
        }

        break;
      case 3:
        this.requestData = {
          "load": 910,
          "fuels":
          {
            "gas(euro/MWh)": 13.4,
            "kerosine(euro/MWh)": 50.8,
            "co2(euro/ton)": 20,
            "wind(%)": 60
          },
          "powerplants": [
            {
              "name": "gasfiredbig1",
              "type": "gasfired",
              "efficiency": 0.53,
              "pmin": 100,
              "pmax": 460
            },
            {
              "name": "gasfiredbig2",
              "type": "gasfired",
              "efficiency": 0.53,
              "pmin": 100,
              "pmax": 460
            },
            {
              "name": "gasfiredsomewhatsmaller",
              "type": "gasfired",
              "efficiency": 0.37,
              "pmin": 40,
              "pmax": 210
            },
            {
              "name": "tj1",
              "type": "turbojet",
              "efficiency": 0.3,
              "pmin": 0,
              "pmax": 16
            },
            {
              "name": "windpark1",
              "type": "windturbine",
              "efficiency": 1,
              "pmin": 0,
              "pmax": 150
            },
            {
              "name": "windpark2",
              "type": "windturbine",
              "efficiency": 1,
              "pmin": 0,
              "pmax": 36
            }
          ]
        }
        break;
    }
  }
}
