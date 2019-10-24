namespace OurTextAdventure.Infrastructure

module ResultBindings =
    let (>>=) x f = Result.bind f x
    let switch processFunc input = Ok (processFunc input)
    let (>=>) switch1 switch2 = switch1 >> (Result.bind switch2)

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

    let anyStringOf validStrings =
        validStrings
        |> List.map expectString
        |> choice