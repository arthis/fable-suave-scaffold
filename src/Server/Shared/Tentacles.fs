namespace ServerCode.Domain

open System


type Tentacle =
    {
        Id : Guid;
        Name: string;
        FriendlyName : string;
    }
    static member empty = 
        { Id = Guid.NewGuid()
          Name = ""
          FriendlyName = "" }

type TentacleList =
    {
        MachineName : string
        Tentacles : Tentacle list
    }
    static member New machineName = 
        { MachineName = machineName
          Tentacles = [] }

type TentacleCommand =
    |  RemoveTentacle of Tentacle
    |  AddTentacle of Tentacle
    |  UpdateTentacleName of Guid*string
    |  UpdateTentacleFriendlyName of Guid*string

type TentacleEvent =
    |  TentacleRemoved of Tentacle
    |  TentacleAdded of Tentacle
    |  TentacleNameUpdated of Guid*string
    |  TentacleFriendlyNameUpdated of Guid*string    