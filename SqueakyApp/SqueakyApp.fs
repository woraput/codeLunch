// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace SqueakyApp

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms

module App = 
  type Msg =
      | Plus
      | Minus
      | Multiply 
      | IncreaseA
      | IncreaseB
      | Clear
  
  /// The model from which the view is generated
  type Model = 
      { InputA : int
        InputB : int  
        Result : int}
  
  /// Returns the initial state
  let init() = { InputA = 0 ; InputB = 0; Result =0  }
      
  /// The funtion to update the view
  let update (msg:Msg) (model:Model) = 
     match msg with
        | Plus -> {model with Result = model.InputA + model.InputB }
        | Minus ->  { model with Result = (if model.InputA > model.InputB then model.InputA - model.InputB else  0  ) }
        | Multiply -> {model with Result = model.InputA * model.InputB }
        | IncreaseA -> {model with InputA =  model.InputA + 1 }
        | IncreaseB -> {model with InputB =  model.InputB + 1 }
        | Clear -> init() 

  /// The view function giving updated content for the page
  let view (model: Model) dispatch =
      View.ContentPage(
        title = "เครื่องคิดเลข",
        content = View.StackLayout( padding= Thickness 20.0, verticalOptions = LayoutOptions.Center,
            children = [
                 View.Label(automationId="InputALabel", text=sprintf "%d" model.InputA,horizontalOptions=LayoutOptions.CenterAndExpand)
                 View.Button( text="เพิ่มA", command= (fun () -> dispatch IncreaseA))
                 View.Label(automationId="InputBLabel", text=sprintf "%d" model.InputB ,horizontalOptions = LayoutOptions.CenterAndExpand)
                 View.Button(automationId="IncrementBButton", text="เพิ่มB", command= (fun () -> dispatch IncreaseB)) 
                 View.Button( text="+", command= (fun () -> dispatch Plus))
                 View.Button( text="-", command= (fun () -> dispatch Minus))
                 View.Button( text="*", command= (fun () -> dispatch Multiply))
                 View.Label(automationId="ResultLabel", text=sprintf "%d" model.Result,horizontalOptions=LayoutOptions.CenterAndExpand)
                 View.Button(automationId="ResetButton", text="reset", command= (fun () -> dispatch Clear)) 
        ]))
  
  type App () as app = 
      inherit Application ()
  
      let runner = 
          Program.mkSimple init update view
          |> Program.withConsoleTrace
          |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


