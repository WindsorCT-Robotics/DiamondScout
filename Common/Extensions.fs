namespace ParagonRobotics.DiamondScout.Common

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open FSharp.Collections

type ResultExtensions() =
    [<Extension>]
    static member inline Match (result: Result<'ok, 'err>) (actionOk: Action<'ok>) (actionErr: Action<'err>) =
        match result with
        | Ok ok -> actionOk.Invoke(ok)
        | Error err -> actionErr.Invoke(err)

type ListExtensions() =
    [<Extension>]
    static member inline ToReadOnlyList<'T> (fSharpList: 'T list) = fSharpList :> IReadOnlyList<'T>
    [<Extension>]
    static member inline FromReadOnlyList<'T> (readOnlyList: 'T IReadOnlyList) =
        match readOnlyList with
        | :? list<'T> as list -> list
        | readOnlyList -> readOnlyList |> Seq.toList