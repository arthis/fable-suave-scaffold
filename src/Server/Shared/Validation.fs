namespace ServerCode.Domain

open System

// Model validation functions.  Write your validation functions once, for server and client!
module Validation =

    let verifyBookTitle title =
        if String.IsNullOrWhiteSpace title then Some "No title was entered" else
        None

    let verifyBookAuthors authors =
        if String.IsNullOrWhiteSpace authors then Some "No author was entered" else
        None

    let verifyBookLink link =
        if String.IsNullOrWhiteSpace link then Some "No link was entered" else
        None

    let verifyBook book =
        verifyBookTitle book.Title = None &&
        verifyBookAuthors book.Authors = None &&
        verifyBookLink book.Link = None

    let verifyWishList wishList =
        wishList.Books |> List.forall verifyBook


    
    let verifyTentacleName name =
        if String.IsNullOrWhiteSpace name then Some "No name was entered" else
        None

    let verifyTentacleFriendlyName friendlyName =
        if String.IsNullOrWhiteSpace friendlyName then Some "No friendlyName was entered" else
        None

    let verifyTentacle tentacle =
        verifyTentacleName tentacle.Name = None &&
        verifyTentacleFriendlyName tentacle.FriendlyName = None 

    let verifyTentacleList tentacleList =
        tentacleList.Tentacles |> List.forall verifyTentacle    