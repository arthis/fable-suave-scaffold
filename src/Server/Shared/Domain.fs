/// Domain model shared between client and server.
namespace ServerCode.Domain
   
open System

// Json web token type.
type JWT = string

// Login credentials.
type Login = 
    { UserName : string
      Password : string }



/// The data for each book in /api/wishlist
type Book = 
    { Title: string
      Authors: string
      Link: string }

    static member empty = 
        { Title = ""
          Authors = ""
          Link = "" }

/// The logical representation of the data for /api/wishlist
type WishList = 
    { UserName : string
      Books : Book list }

    // Create a new WishList.  This is supported in client code too,
    // thanks to the magic of https://www.nuget.org/packages/Fable.JsonConverter
    static member New userName = 
        { UserName = userName
          Books = [] }

type Artifact =
    {
        filePath : string;
        url : PrintfFormat<(int -> string),unit,string,string,int>;
        login : string;
        password : string;
    }

type Site =
    {
        name : string;
        url : string;
        port : int;
        physicalPath : string;
    }

type TentacleStepScript = 
    {
        name : string;
        script : string;
    }

type DBName = | DBName of string

type SqlConnection = 
    {
        serverName :string;
        login:string;
        password:string;
    }

type SqlFile = 
    {
        folder : string
        files : string []
    }

type InstallDBScript = 
    {
        sqlConnection : SqlConnection
        dbName: DBName;
        dataPath : string;
        logPath: string;
        sqlFiles : SqlFile list
    }

type UpdateDBScript = 
    {
        sqlConnection : SqlConnection
        script : string;
    }

type DetachDBScript = 
    {
        sqlConnection : SqlConnection
        dbName : DBName;
    }

type AttachDBScript = 
    {
        sqlConnection : SqlConnection
        dbName : DBName;
        dataFilePath : string;
        logFilePath: string;
    }

type UnzipStep = 
    {
        archive :string;
        target :string;
    }
type MoveStep = 
    {
        sourceFolder : string;
        targetFolder : string;
    }

type DeploymentStep =
    | Script of TentacleStepScript
    | CreateNewSite of Site
    | Move of MoveStep
    | Unzip of UnzipStep
    | HttpDownlad of Artifact
    | SFtpDownlad of Artifact
    | InstallDB of InstallDBScript
    | UpdateDB of UpdateDBScript
    | DetachDB of DetachDBScript
    | AttachDB of AttachDBScript
    | StartService of string
    | StopService of string
    | SiteMaintenanceOn of string
    | SiteMaintenanceOff of string          


