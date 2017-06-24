module Client.TentacleList


open Fable.Core
open Fable.Import
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props
open ServerCode.Domain
open Style
open Messages
open System
open Fable.Core.JsInterop
open Fable.PowerPack
open Fable.PowerPack.Fetch.Fetch_types


type Model = 
    { 
        TentacleList : TentacleList
        Token : string
        NewTentacle : Tentacle
        NewTentacleId : Guid // unique key to reset the vdom-elements, see https://github.com/fable-compiler/fable-suave-scaffold/issues/107#issuecomment-301312224
        NameErrorText : string option
        FriendlyNameErrorText : string option
        ErrorMsg : string 
    }

/// Get the wish list from the server, used to populate the model
let getTentacleList token =
    promise {        
        let url = "api/tentaclelist/"
        let props = 
            [ Fetch.requestHeaders [
                HttpRequestHeaders.Authorization ("Bearer " + token) ]]

        return! Fable.PowerPack.Fetch.fetchAs<TentacleList> url props
    }

let loadTentacleListCmd token = 
    Cmd.ofPromise getTentacleList token FetchedTentacleList FetchError

let postTentacleList (token,tentacleList) =
    promise {        
        let url = "api/tentaclelist/"
        let body = toJson tentacleList
        let props = 
            [ RequestProperties.Method HttpMethod.POST
              Fetch.requestHeaders [
                HttpRequestHeaders.Authorization ("Bearer " + token)
                HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body !^body ]

        return! Fable.PowerPack.Fetch.fetchAs<TentacleList> url props
    }

let postAddTentacle (token,tentacle) =
    promise {        
        let url = "/api/tentacles/add"
        let body = toJson tentacle
        let props = 
            [ RequestProperties.Method HttpMethod.POST
              Fetch.requestHeaders [
                HttpRequestHeaders.Authorization ("Bearer " + token)
                HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body !^body ]

        return! Fable.PowerPack.Fetch.fetchAs<string> url props
    }    

let postRemoveTentacle (token,tentacle) =
    promise {        
        let url = "/api/tentacles/remove"
        let body = toJson tentacle
        let props = 
            [ RequestProperties.Method HttpMethod.POST
              Fetch.requestHeaders [
                HttpRequestHeaders.Authorization ("Bearer " + token)
                HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body !^body ]

        return! Fable.PowerPack.Fetch.fetchAs<string> url props
    }    

let postTentacleListCmd (token,tentacleList) = 
    Cmd.ofPromise postTentacleList (token,tentacleList) FetchedTentacleList FetchError

let postAddTentacleCmd    (token,tentacle) = 
    Cmd.ofPromise postAddTentacle (token,tentacle)  TentacleAdded FetchError

let postRemoveTentacleCmd    (token,tentacle) = 
    Cmd.ofPromise postRemoveTentacle (token,tentacle)  TentacleRemoved FetchError

let init (user:UserData) = 
    { TentacleList = TentacleList.New user.UserName
      Token = user.Token
      NewTentacle = Tentacle.empty
      NewTentacleId = Guid.NewGuid()
      NameErrorText = None
      FriendlyNameErrorText = None
      ErrorMsg = "" }, loadTentacleListCmd user.Token


let update (msg:TentacleListMsg) model : Model*Cmd<TentacleListMsg> = 
    match msg with
    | TentacleListMsg.LoadForUser user ->
        model, []
    | FetchedTentacleList tentacleList ->
        let tentacleList = { tentacleList with Tentacles = tentacleList.Tentacles |> List.sortBy (fun b -> b.Name) }
        { model with TentacleList = tentacleList }, Cmd.none
    | NameChanged name -> 
        { model with NewTentacle = { model.NewTentacle with Name = name }; NameErrorText = Validation.verifyTentacleName name }, Cmd.none
    | FriendlyNameChanged friendlyName -> 
        { model with NewTentacle = { model.NewTentacle with FriendlyName = friendlyName }; FriendlyNameErrorText = Validation.verifyTentacleFriendlyName friendlyName }, Cmd.none    
    | RemoveTentacle tentacle -> 
        let tentacleList = { model.TentacleList with Tentacles = model.TentacleList.Tentacles |> List.filter ((<>) tentacle) }
        { model with TentacleList = tentacleList}, postRemoveTentacleCmd(model.Token,tentacle)
    | TentacleRemoved tentacle ->
        model, Cmd.none    
    | AddTentacle ->
        if Validation.verifyTentacle model.NewTentacle then
            let tentacleList = { model.TentacleList with Tentacles = (model.NewTentacle :: model.TentacleList.Tentacles) |> List.sortBy (fun b -> b.Name) }
            { model with TentacleList = tentacleList; NewTentacle = Tentacle.empty; NewTentacleId = Guid.NewGuid() }, postAddTentacleCmd(model.Token,model.NewTentacle)
        else
            { model with  
                NameErrorText = Validation.verifyTentacleName model.NewTentacle.Name
                FriendlyNameErrorText = Validation.verifyTentacleFriendlyName model.NewTentacle.FriendlyName
            }, Cmd.none
    | TentacleAdded tentacle ->
        model, Cmd.none
    | FetchError _ -> 
        model, Cmd.none      

let newTentacleForm (model:Model) dispatch =
    let buttonActive = if String.IsNullOrEmpty model.NewTentacle.Name ||
                          String.IsNullOrEmpty model.NewTentacle.FriendlyName 
                        then "btn-disabled"
                        else "btn-primary"
    

    div [] [
        h4 [] [str "New Book"]
        div [ClassName "container"] [
            div [ClassName "row"] [
                div [ClassName "col-md-8"] [
                    div [ClassName ("form-group has-feedback")] [
                        yield div [ClassName "input-group"] [
                             yield span [ClassName "input-group-addon"] [span [ClassName "glyphicon glyphicon-pencil"] [] ]
                             yield input [
                                     Key ("Name_" + model.NewTentacleId.ToString())
                                     HTMLAttr.Type "text"
                                     Name "Name"
                                     DefaultValue (U2.Case1 model.NewTentacle.Name)
                                     ClassName "form-control"
                                     Placeholder "Please insert tentacle name"
                                     Required true
                                     OnChange (fun (ev:React.FormEvent) -> dispatch (TentacleListMsg (TentacleListMsg.NameChanged !!ev.target?value))) ]
                             match model.NameErrorText with
                             | Some e -> yield span [ClassName "glyphicon glyphicon-remove form-control-feedback"] []
                             | _ -> ()
                        ]
                        match model.NameErrorText with
                        | Some e -> yield p [ClassName "text-danger"][str e]
                        | _ -> ()
                    ]
                    div [ClassName ("form-group has-feedback") ] [
                         yield div [ClassName "input-group"][
                             yield span [ClassName "input-group-addon"] [span [ClassName "glyphicon glyphicon-user"] [] ]
                             yield input [ 
                                     Key ("FriendlyName_" + model.NewTentacleId.ToString())
                                     HTMLAttr.Type "text"
                                     Name "FriendlyName"
                                     DefaultValue (U2.Case1 model.NewTentacle.FriendlyName)
                                     ClassName "form-control"
                                     Placeholder "Please insert friendly name"
                                     Required true
                                     OnChange (fun (ev:React.FormEvent) -> dispatch (TentacleListMsg (TentacleListMsg.FriendlyNameChanged !!ev.target?value)))]
                             match model.FriendlyNameErrorText with
                             | Some e -> yield span [ClassName "glyphicon glyphicon-remove form-control-feedback"] []
                             | _ -> ()
                         ]
                         match model.FriendlyNameErrorText with
                         | Some e -> yield p [ClassName "text-danger"][str e]
                         | _ -> ()
                    ]
                    button [ ClassName ("btn " + buttonActive); OnClick (fun _ -> dispatch (TentacleListMsg TentacleListMsg.AddTentacle))] [
                        i [ClassName "glyphicon glyphicon-plus"; Style [PaddingRight 5]] []
                        str "Add"
                    ]  
                ]                    
            ]        
        ]
    ]
let view (model:Model) (dispatch: AppMsg -> unit) = 
    div [] [
        h4 [] [str "tentaclelist" ]
        table [ClassName "table table-striped table-hover"] [
            thead [] [
                    tr [] [
                        th [] [str "Name"]
                        th [] [str "FriendlyName"]
                        th [] [str "action"]
                ]
            ]                
            tbody[] [
                for tentacle in model.TentacleList.Tentacles do
                    yield 
                      tr [] [
                        td [] [ str tentacle.Name ]
                        td [] [ str tentacle.FriendlyName ]
                        td [] [ buttonLink "" (fun _ -> dispatch (TentacleListMsg (RemoveTentacle tentacle))) [ str "Remove" ] ]
                        ]
            ]
        ]
        newTentacleForm (model) dispatch
    ]        