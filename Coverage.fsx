#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System
open Fake.OpenCoverHelper

let run (config : Map<string, string>) _ =
    let testDlls = !! (sprintf @".\**\bin\%s\**\*.Tests.dll" (config.get "build:configuration"))
    if Seq.length testDlls > 0 then
        ensureOpenCover config
        testDlls
        |> OpenCover 
            (fun defaults ->
                { defaults with 
                    ToolPath = config.get "core:tools" @@ openCoverRunners
                 })

