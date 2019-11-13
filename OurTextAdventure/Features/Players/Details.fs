module OurTextAdventure.Features.Players.Details

open OurTextAdventure.Models

type [<CLIMutable>] Query = { PlayerId: PlayerId }
type Model = { 
    Id: PlayerId
    Details: Details
    Inventory: Item seq
    Location: RoomId }
module Handler =  
    let Handle request = 
        // get player by id
        request.PlayerId

        