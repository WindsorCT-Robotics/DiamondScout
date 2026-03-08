open Microsoft.Extensions.Configuration
open Saturn
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Serilog
open Serilog.AspNetCore
open Serilog.Events
open Serilog.Extensions.Logging
open Serilog.Extensions.Hosting
open Serilog.Sinks.SystemConsole
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

type LoggerConfiguration =
    { MinimumLevel: LogEventLevel
      Path: string }

let logger cfg =
    LoggerConfiguration()
    |> _.MinimumLevel.Is(cfg.MinimumLevel)
    |> _.WriteTo.ColoredConsole()
    |> _.WriteTo.File(cfg.Path)
    |> _.CreateBootstrapLogger()

let app =
    application {
        use_developer_exceptions

        logging (fun l ->
            let cfg =
                Config.getConfiguration(l.Services).GetRequiredSection("Logger").Get<LoggerConfiguration>()

            l.AddSerilog(logger cfg, dispose = true) |> ignore)

        url "http://0.0.0.0:8085"
        memory_cache
        use_gzip
    }

run app
