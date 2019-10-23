namespace OurTextAdventure

module Common =
    let (>>=) x f = Result.bind f x
    let switch processFunc input = Ok (processFunc input)
    let (>=>) switch1 switch2 = switch1 >> (Result.bind switch2)

module Domain =
    type Details = { Name: string; Description: string; }

    type Item = { Details: Details }

    type RoomId = | RoomId of string

    type Exit =
        | PassableExit of Details * destination: RoomId
        | LockedExit of Details * key: Item * next: Exit
        | NoExit of Details option

    and Exits = { North: Exit; East: Exit; South: Exit; West: Exit; }
    
    and Room = {
        Id: RoomId
        Details: Details
        Items: Item list
        Exits: Exits }

    type Player = { Details: Details; Location: RoomId; Inventory: Item list }

    type World = { Rooms: Map<RoomId, Room>; Player: Player }

module InitialWorld =
    open Domain
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
            Details = { Name = ""; Description = "" }; 
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

    let gameWorld = {
        Rooms = allRooms |> Seq.map(fun room -> (room.Id, room)) |> Map.ofSeq
        Player = player }
    
module GameLogic =
    open Common
    open Domain
    open InitialWorld

    let getRoom world roomId =
        match world.Rooms.TryFind roomId with
        | Some room -> Ok room
        | None -> Error "Room not found!"

    let describeDetails details =
        sprintf "\n\n%s\n\n%s\n\n" details.Name details.Description

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

    
    let gameEngine = GameEngine(gameWorld)

    let rand = System.Random()
    let playerController =
        MailboxProcessor.Start(fun inbox ->
            let rec innerLoop state =
                async {
                    try
                        let! eventMsg = inbox.Receive(2000)
                        if eventMsg = "stop" then return ()
                    with 
                    | :? System.TimeoutException ->
                        ["north", north;"east", east;"south", south;"west", west;]
                        |> List.item (rand.Next 4)
                        |> fun (dir, dirFunc) -> printfn "Wandering %s..." dir; dirFunc
                        |> move
                        |> gameEngine.ApplyUpdate

                        do! innerLoop state
                }
            innerLoop 0)
    
module CommandParsing =
    type Parser<'a> = Parser of (char list -> Result<'a * char list, string>)
    let runParser parser inputChars =
        let (Parser parserFunc) = parser
        parserFunc inputChars

    let expectChar expectedChar =
        let innerParser inputChars =
            match inputChars with
            | c :: remainingChars -> 
                if c = expectedChar 
                then Ok (c, remainingChars) 
                else Error (sprintf "We expected %c but got %c" expectedChar c)
            | [] -> Error (sprintf "We expected %c, but we've reached the end of input" expectedChar)
        Parser innerParser

    let stringToCharList = List.ofSeq

    let orParse parser1 parser2 = 
        let innerParser inputChars =
            match runParser parser1 inputChars with
            | Ok result -> Ok result
            | Error _ -> runParser parser2 inputChars
        Parser innerParser

    let (<|>) = orParse
    
    let choice parserList = 
        List.reduce orParse parserList

    let anyCharOf validChars =
        validChars
        |> List.map expectChar
        |> choice
    
    let andParse parser1 parser2 = 
        let innerParser inputChars =
            match runParser parser1 inputChars with
            | Error msg -> Error msg 
            | Ok (c1, remaining1) -> 
                match runParser parser2 remaining1 with 
                    | Error msg -> Error msg
                    | Ok (c2, remaining2) ->
                        Ok ((c1, c2), remaining2)
        Parser innerParser

    let (.>>.) = andParse          
    let mapParser mapFunc parser =
        let innerParser inputChars =
            match runParser parser inputChars with
            | Error msg -> Error msg
            | Ok (result, remaining) -> Ok (mapFunc result, remaining)
        Parser innerParser

    let applyParser funcAsParser paramAsParser =
        (funcAsParser .>>. paramAsParser)
        |> mapParser (fun (f, x) -> f x)

    let (<*>) = applyParser
    let returnAsParser result =
        let innerParser inputChars =
            Ok (result, inputChars)
        Parser innerParser

    let liftToParser2 funcToLift paramAsParser1 paramAsParser2 =
        returnAsParser funcToLift <*> paramAsParser1 <*> paramAsParser2        
    let rec sequenceParsers parsers =
        let cons head rest = head :: rest
        let consAsParser = liftToParser2 cons
        match parsers with
        | [] -> returnAsParser [] 
        | parser::remainingParsers -> 
            consAsParser parser (sequenceParsers remainingParsers)
    let charListAsString = List.toArray >> string
    let expectString expectedString =
        expectedString
        |> stringToCharList
        |> List.map expectChar
        |> sequenceParsers
        |> mapParser charListAsString

    stringToCharList "take"
    |> runParser (expectString "lake" <|> expectString "take")
    |> printfn "%A"

