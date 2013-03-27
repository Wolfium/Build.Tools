#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open Fake
open System

let private nunitRunners = "./NUnit.Runners"
let private nuget = "./nuget/nuget.exe"

let private ensureRunner (config : Map<string, string>) =
    if not (directoryExists <| config.get "core:tools" @@ nunitRunners) then
        let args =
            sprintf "install NUnit.Runners -ExcludeVersion -OutputDirectory \"%s\""
                (config.get "core:tools")
        let result =
            ExecProcess (fun info ->
                info.FileName <- config.get "core:tools" @@ nuget
                info.WorkingDirectory <- DirectoryName (config.get "core:tools")
                info.Arguments <- args) (TimeSpan.FromMinutes 5.)

        if result <> 0 then
            failwith "NUnit.Runners directory not found, and NuGet install failed."

let run (config : Map<string, string>) _ =
    ensureRunner config
    !! (sprintf @".\**\bin\%s\**\*.Tests.dll" (config.get "build:configuration"))
    |> NUnit 
        (fun defaults ->
            { defaults with 
                ToolPath = config.get "core:tools" @@ nunitRunners
             })