open System
open OurTextAdventure.Models
open OurTextAdventure.GameLogic
open OurTextAdventure.Infrastructure.CommandParsing

[<EntryPoint>]
let main argv =
    let key: Item = { Details = { Name = "key"; Description = "A rusty key."} }
    let allRooms = [
        {
            Id = RoomId "center"
            Details = { Name = "A central room"; Description = "You are standing in a central room with exits in all directions." }; 
            Items = []; 
            Exits = {
                North = PassableExit({ Name = "A northern room"; Description = ""}, RoomId "north")
                South = PassableExit({ Name = "A southern room"; Description = "" }, RoomId "south")
                East = LockedExit({ Name = "An eastern room"; Description = "You see a locked door to the east." }, key, PassableExit({ Name = "An eastern room"; Description = "" }, RoomId "east"))
                West = PassableExit({ Name = "A western room"; Description = "" }, RoomId "west")
            }
        }
        {
            Id = RoomId "north"
            Details = { Name = "A northern room"; Description = "" }; 
            Items = []; 
            Exits = {
                North = NoExit (Some { Name = ""; Description = "" })
                East = PassableExit({ Name = "A room"; Description = ""}, RoomId "northeast")
                South = PassableExit({ Name = "A room"; Description = ""}, RoomId "center")
                West = PassableExit({ Name = "A room"; Description = ""}, RoomId "northwest")
            }
        }
        {
            Id = RoomId "northeast"
            Details = { Name = "A north eastern room"; Description = "" }; 
            Items = []; 
            Exits = {
                North = NoExit (Some { Name = ""; Description = "" })
                East = NoExit None
                South = PassableExit({ Name = "A room"; Description = ""}, RoomId "east")
                West = PassableExit({ Name = "A room"; Description = ""}, RoomId "north")
            }
        }
        {
            Id = RoomId "east"
            Details = { Name = "A co"; Description = "" }; 
            Items = []; 
            Exits = {
                North = PassableExit({ Name = "A room"; Description = ""}, RoomId "northeast")
                East = NoExit None
                South = PassableExit({ Name = "A room"; Description = ""}, RoomId "southeast") 
                West = PassableExit({ Name = "A room"; Description = ""}, RoomId "center")
            }
        }
        {
            Id = RoomId "southeast"
            Details = { Name = "A south eastern room"; Description = "" }; 
            Items = []; 
            Exits = {
                North = PassableExit({ Name = "A room"; Description = ""}, RoomId "east")
                East = NoExit None
                South = NoExit None 
                West = PassableExit({ Name = "A room"; Description = ""}, RoomId "south")
            }
        }
        {
            Id = RoomId "south"
            Details = { Name = "A southern room"; Description = "" }; 
            Items = []; 
            Exits = {
                North = PassableExit({ Name = "A room"; Description = ""}, RoomId "center")
                East = PassableExit({ Name = "A room"; Description = ""}, RoomId "southeast")
                South = NoExit None 
                West = PassableExit({ Name = "A room"; Description = ""}, RoomId "southwest")
            }
        }
        {
            Id = RoomId "southwest"
            Details = { Name = "A south western room"; Description = "" }; 
            Items = []; 
            Exits = {
                North = PassableExit({ Name = "A room"; Description = ""}, RoomId "west")
                East = PassableExit({ Name = "A room"; Description = ""}, RoomId "south")
                South = NoExit None 
                West = NoExit None
            }
        }
        {
            Id = RoomId "west"
            Details = { Name = "A western room"; Description = "" }; 
            Items = []; 
            Exits = {
                North = PassableExit({ Name = "A room"; Description = ""}, RoomId "northwest")
                East = PassableExit({ Name = "A room"; Description = ""}, RoomId "center")
                South = PassableExit({ Name = "A room"; Description = ""}, RoomId "southwest")
                West = NoExit None
            }
        }
        {
            Id = RoomId "northwest"
            Details = { Name = "A north western room"; Description = "" }; 
            Items = []; 
            Exits = {
                North = NoExit (Some { Name = ""; Description = "" })
                East = PassableExit({ Name = "A room"; Description = ""}, RoomId "center")
                South = PassableExit({ Name = "A room"; Description = ""}, RoomId "west")
                West = NoExit None
            }
        }
    ] 

    let player = { 
        Details = { Name = "Action Hank"; Description = "A man wielding the most rugged beard known to man."}
        Inventory = []
        Location = RoomId "center" }

    let gameWorld = { Rooms = allRooms |> Seq.map(fun room -> (room.Id, room)) |> Map.ofSeq; Player = player }

    let gameEngine = GameEngine(gameWorld)
        
    // wander aimlessly every 5 seconds
    let random = System.Random()
    let intervalInMilliSeconds = 5000
    let handler = OurTextAdventure.Features.Players.Wander.Handler(gameEngine)
    handler.Handle random intervalInMilliSeconds |> ignore
    
    let inputToAction line =        
        match line with
        | "quit" -> gameEngine.Stop(); Environment.Exit(0)
        | "reset" -> gameEngine.Reset gameWorld
        | "move north" -> gameEngine.ApplyUpdate(move north)
        | "move east" -> gameEngine.ApplyUpdate(move east)
        | "move south" -> gameEngine.ApplyUpdate(move south)
        | "move west" -> gameEngine.ApplyUpdate(move west)
        | "who am i" -> printfn "%s" (describeDetails gameWorld.Player.Details)
        | _ -> 
            line 
            |> stringToCharList
            |> runParser (["quit"; "reset"; "move north"; "move east"; "move south"; "move west"; "who am i"] |> anyStringOf)
            |> printfn "%A"
            
    describeCurrentRoom gameWorld 
    |> fun x -> 
        match x with 
        | Ok msg -> printfn "%s" msg
        | Error error -> printfn "%s" error
         
    fun _ -> Console.ReadLine()
    |> Seq.initInfinite
    |> Seq.iter inputToAction

    0 // return an integer exit code
