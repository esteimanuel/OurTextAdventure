module OurTextAdventure.Features.Players.Wander

open OurTextAdventure.GameLogic
open System

type Handler(gameEngine: GameEngine) =  
    member _.Handle (rand: Random) (intervalInMilliSeconds: int) =
        MailboxProcessor.Start(fun inbox ->
            let rec innerLoop state =
                async {
                    try
                        let! eventMsg = inbox.Receive(intervalInMilliSeconds)
                        if eventMsg = "stop" then return ()
                    with 
                    | :? System.TimeoutException ->
                        ["north", north; "east", east; "south", south; "west", west;]
                        |> List.item (rand.Next 4)
                        |> fun (dir, dirFunc) -> printf "Wandering %s..." dir; dirFunc
                        |> move
                        |> gameEngine.ApplyUpdate

                        do! innerLoop state
                }
            innerLoop 0)