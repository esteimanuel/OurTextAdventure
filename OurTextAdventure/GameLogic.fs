module OurTextAdventure.GameLogic

open Infrastructure.ResultBindings
open Models

let getRoom world roomId =
    match world.Rooms.TryFind roomId with
    | Some room -> Ok room
    | None -> Error "Room not found!"

let describeDetails details =
    sprintf "%s\n\n%s\n\n" details.Name details.Description

let extractDetailsFromRoom (room: Room) =
    room.Details

let getCurrentRoom world =
    world.Player.Location
    |> getRoom world

let describeCurrentRoom world =
    getCurrentRoom world
    >>= switch extractDetailsFromRoom
    >>= switch describeDetails

let north { North = northExit } = northExit
let east exits = exits.East
let south { South = southExit } = southExit
let west { West = westExit } = westExit

let getExit direction exits =
    match (direction exits) with
    | PassableExit (_, roomId) -> Ok roomId
    | LockedExit _ -> Error "There is a locked door in that direction."
    | NoExit _ -> Error "There is no room in that direction."

let setCurrentRoom world room =
    { world with 
        Player = { world.Player with Location = room.Id }}

let displayResult result =
    match result with
    | Ok s -> printfn "%s" s
    | Error f -> printfn "%s" f

let move direction world = 
    world
    |> getCurrentRoom
    >>= switch (fun room-> room.Exits)
    >>= getExit direction
    >>= getRoom world
    >>= switch (setCurrentRoom world)

type GameEvent = 
    | UpdateState of (World -> Result<World, string>)
    | ResetState of World
    | EndGameLoop

type GameEngine(initialState: World) =
    let applyUpdate updateFunc worldState =
        match updateFunc worldState with
        | Ok newState -> 
            describeCurrentRoom newState |> displayResult
            newState
        | Error message -> 
            printfn "%s" message
            worldState
    
    let gameLoop =
        MailboxProcessor.Start(fun inbox -> 
            let rec innerLoop worldState =
                async {
                    let! eventMsg = inbox.Receive()
                    match eventMsg with
                    | UpdateState updateFunc -> return! innerLoop (applyUpdate updateFunc worldState)
                    | ResetState world -> return! innerLoop world
                    | EndGameLoop -> return ()
                }
            innerLoop initialState)

    member _.ApplyUpdate(updateFunc) =
        gameLoop.Post(UpdateState updateFunc)

    member _.Stop() =
        gameLoop.Post(EndGameLoop)

    member _.Reset(world) =
        gameLoop.Post(ResetState world)

