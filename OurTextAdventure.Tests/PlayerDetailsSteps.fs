module OurTextAdventure.Tests.PlayerDetailsSteps

open TickSpec
open Xunit
open OurTextAdventure.Models

let [<Given>] ``a player (.*) that can be described as (.*)`` (name:string) (description:string) = { 
    Id = PlayerId ""
    Details = { Name = name; Description = description}
    Inventory = []
    Location = RoomId "" 
}
          
let [<When>] ``I would describe the player`` (player:Player) = player |> ignore
          
let [<Then>] ``I would say (.*) is (.*)`` (name:string) (description:string) (player:Player) =
    Assert.Equal(name, player.Details.Name)
    Assert.Equal(description, player.Details.Description)