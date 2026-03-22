namespace ParagonRobotics.DiamondScout.Common

open System
open System.Runtime.CompilerServices

module ResultExtensions =
    [<Extension>]
    let Match (result: Result<'ok, 'err>) (actionOk: Action<'ok>) (actionErr: Action<'err>) =
        match result with
        | Ok ok -> actionOk.Invoke(ok)
        | Error err -> actionErr.Invoke(err)
