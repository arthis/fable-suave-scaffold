/// Wish list API web parts and data access functions.
module ServerCode.TentacleList

open System.IO
open Suave
open Suave.Logging
open System.Net
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open System
open Suave.ServerErrors
open ServerCode.Domain
open Suave.Logging
open Suave.Logging.Message

let logger = Log.create "FableSample"


let tentaclesActor = MailboxProcessor.Start(fun inbox-> 

    // the message processing function
    let rec messageLoop() = async{
        
        // read a message
        let! msg = inbox.Receive()

        match msg with
        | RemoveTentacle t -> ()
        | AddTentacle t -> ()
        | UpdateTentacleName (i,n) -> ()
        | UpdateTentacleFriendlyName (i, nf) -> ()
        
        // process a message
        // printfn "message is: %s" msg

        // loop to top
        return! messageLoop()  
        }

    // start the loop 
    messageLoop() 
    )


/// The default initial data 
let defaultTentacleList machineName : TentacleList =
    {
        MachineName = machineName
        Tentacles = 
            [{
              Id= Guid.NewGuid()
              Name = "PumaWeb PreProd"
              FriendlyName = "PumaWebPreProd"}
             {
              Id= Guid.NewGuid()
              Name = "Brunswick PreProd"
              FriendlyName = "BrunswickPreProd"}]
    }

//  file name used to store the data for a specific user
let jsonFileName = "./temp/db/tentacles.json" 

/// Query the database
let getWishListFromDB userName =
    let fi = FileInfo(jsonFileName)
    if not fi.Exists then
        defaultTentacleList userName
    else
        File.ReadAllText(fi.FullName)
        |> FableJson.ofJson<TentacleList>

/// Save to the database
let saveTentacleListToDB (tentacleList:TentacleList) =
    try
        let fi = FileInfo(jsonFileName)
        if not fi.Directory.Exists then
            fi.Directory.Create()
        File.WriteAllText(fi.FullName, FableJson.toJson tentacleList)
    with exn ->
        logger.error (eventX "Save failed with exception" >> addExn exn)

/// Handle the GET on /api/tentaclelist
let getTentacleList (ctx: HttpContext) =
    Auth.useToken ctx (fun token -> async {
        try
            let tentacleList = getWishListFromDB token.UserName
            return! Successful.OK (FableJson.toJson tentacleList) ctx
        with exn ->
            logger.error (eventX "SERVICE_UNAVAILABLE" >> addExn exn)
            return! SERVICE_UNAVAILABLE "Database not available" ctx
    })

/// Handle the POST on /api/tentaclelist
let postTentacleList (ctx: HttpContext) =
    Auth.useToken ctx (fun token -> async {
        try
            let tentacleList = 
                ctx.request.rawForm
                |> System.Text.Encoding.UTF8.GetString
                |> FableJson.ofJson<Domain.TentacleList>
            
            if Validation.verifyTentacleList tentacleList then
                saveTentacleListToDB tentacleList
                return! Successful.OK (FableJson.toJson tentacleList) ctx
            else
                return! BAD_REQUEST "WishList is not valid" ctx
        with exn ->
            logger.error (eventX "Database not available" >> addExn exn)
            return! SERVICE_UNAVAILABLE "Database not available" ctx
    })    

/// Handle the POST on /api/tentacles/remove
let postRemoveTentacle (ctx:HttpContext) = 
    Auth.useToken ctx (fun token -> async {
        
        ctx.request.rawForm
        |> System.Text.Encoding.UTF8.GetString
        |> FableJson.ofJson<Tentacle>
        |> RemoveTentacle
        |> tentaclesActor.Post

        return! Successful.OK "OK" ctx
    })    


/// Handle the POST on /api/tentacles/add
let postAddTentacle (ctx:HttpContext) = 
    Auth.useToken ctx (fun token -> async {
        
        ctx.request.rawForm
        |> System.Text.Encoding.UTF8.GetString
        |> FableJson.ofJson<Tentacle>
        |> AddTentacle
        |> tentaclesActor.Post

        return! Successful.OK "OK" ctx
    })    

/// Handle the POST on /api/tentacles/updateTentacleName
let postUpdateTentacleName (ctx:HttpContext) = 
    Auth.useToken ctx (fun token -> async {
        
        ctx.request.rawForm
        |> System.Text.Encoding.UTF8.GetString
        |> FableJson.ofJson<Guid*string>
        |> UpdateTentacleName
        |> tentaclesActor.Post

        return! Successful.OK "OK" ctx
    })        

/// Handle the POST on /api/tentacles/updateTentacleFriendlyName
let postUpdateTentacleFriendlyName (ctx:HttpContext) = 
    Auth.useToken ctx (fun token -> async {
        
        ctx.request.rawForm
        |> System.Text.Encoding.UTF8.GetString
        |> FableJson.ofJson<Guid*string>
        |> UpdateTentacleFriendlyName
        |> tentaclesActor.Post

        return! Successful.OK "OK" ctx
    })            

