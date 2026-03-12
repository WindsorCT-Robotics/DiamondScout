namespace ParagonRobotics.DiamondScout.Common

open FsToolkit.ErrorHandling
open ParagonRobotics.DiamondScout.Common.DomainEvents

/// Username normalized for purposes of comparison.
[<Struct>]
type NormalizedUserName = NormalizedUserName of string

type UserDirectory = private {
    RegisteredUsers: Set<UserId>
    UserIdsByName: Map<NormalizedUserName, UserId>
}
    
module UserDirectory =
    type Error =
        | UserAlreadyRegistered of UserId
        | UserNotFound of UserId
        | UserNameTaken of UserId
        
    type Command =
        | Register of userId: UserId * name: UserName
        | ChangeName of userId: UserId * name: UserName
        | Deactivate of userId: UserId
        
    type Event =
        | Registered of userId: UserId * name: NormalizedUserName
        | NameChanged of userId: UserId * name: NormalizedUserName
        | Deactivated of userId: UserId
        
    module Event =
        let evolve registry event =
            match event with
            | Registered (id, name) ->
                { registry with
                      RegisteredUsers = registry.RegisteredUsers |> Set.add id
                      UserIdsByName = registry.UserIdsByName |> Map.add name id }
            | NameChanged (id, name) ->
                { registry with
                      UserIdsByName = registry.UserIdsByName |> Map.add name id }
            | Deactivated id ->
                { registry with
                      RegisteredUsers = registry.RegisteredUsers |> Set.remove id
                      UserIdsByName = registry.UserIdsByName |> Map.filter (fun _ userId -> userId <> id) }

    module Validation =
        let userIdIsUnique registry id =
            match Set.contains id registry.RegisteredUsers with
            | true -> id |> UserAlreadyRegistered |> Validation.error
            | false -> Validation.ok id
            
        let userNameIsUnique registry name =
            match Map.tryFind name registry.UserIdsByName with
            | Some takenId -> takenId |> UserNameTaken |> Validation.error
            | None -> Validation.ok name
            
        let userIsRegistered registry id =
            match Set.contains id registry.RegisteredUsers with
            | true -> Validation.ok id
            | false -> id |> UserNotFound |> Validation.error
            
    module Command =
        let normalize (UserName name) =
            name.Trim().ToUpperInvariant() |> NormalizedUserName
            
        let decide command registry =
            match command with
            | Register (id, name) ->
                validation {
                    let! id = Validation.userIdIsUnique registry id
                    and! name = normalize >> Validation.userNameIsUnique registry <| name
                    
                    return Registered (id, name) |> List.singleton
                }
            | ChangeName (id, name) ->
                validation {
                    let! id = Validation.userIsRegistered registry id
                    and! name = normalize >> Validation.userNameIsUnique registry <| name
                    
                    return NameChanged (id, name) |> List.singleton
                }
            | Deactivate id ->
                validation {
                    let! id = Validation.userIsRegistered registry id
                    
                    return Deactivated id |> List.singleton
                }
    
    let initialState =
        { RegisteredUsers = Set [ UserId.Zero ]
          UserIdsByName = Map [ User.Zero.Name |> Command.normalize, UserId.Zero ] }
        
    let definition =
        AggregateDefinition.create initialState Event.evolve Command.decide