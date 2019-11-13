module OurTextAdventure.Models

type Details = { Name: string; Description: string; }

type Item = { Details: Details }

type RoomId = RoomId of string

type Exit =
    | PassableExit of Details * destination: RoomId
    | LockedExit of Details * key: Item * next: Exit
    | NoExit of Details option

and Exits = { North: Exit; East: Exit; South: Exit; West: Exit; }

and Room = { Id: RoomId; Details: Details; Items: Item list; Exits: Exits }

type PlayerId = PlayerId of string
type Player = { Id: PlayerId; Details: Details; Location: RoomId; Inventory: Item list }

type World = { Rooms: Map<RoomId, Room>; Player: Player }
