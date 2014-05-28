// include Fake lib
#r @"../FAKE/FakeLib.dll"

open Fake
open Fake.ReleaseNotesHelper
open Fake.AssemblyInfoFile

let releasesFileData = ReadFile "RELEASE_NOTES.md"

let release = parseReleaseNotes releasesFileData

tracefn "Starting (%A - #%s)" buildServer buildVersion
tracefn "Nuget    version: %s" release.NugetVersion
tracefn "Latest version release notes:"
tracefn "%A" release

let packFile = !! (@"./src/**/*.nuspec") |> Seq.exactlyOne

// Directories
let outDir              = "./output/"
let dropDir             = outDir @@ "/artifacts/"
let packingDir          = outDir @@ "/packing"    

let nugetToolPath = "./NuGet/NuGet.exe"

// Targets
Target "Clean" (fun _ -> 
    CleanDirs [outDir; dropDir; packingDir]
)

Target "Package" (fun _ -> 

    tracefn "pack file: %A" packFile
    
    let packInfo = (ReadFileAsString >> getNuspecProperties) packFile

    CopyRecursive "src" (packingDir) true
        |> Log "CreatePackage-Output: "

    CopyRecursive "Fake" (packingDir @@ "Fake") true
        |> Log "CreatePackage-Output: "

    CopyRecursive "NuGet" (packingDir) true
        |> Log "CreatePackage-Output: "

    trace ""
    trace "Release Notes:"
    trace (release.Notes |> toLines)
    trace ""

    NuGet (fun p -> 
            {p with   
                Version = release.NugetVersion
                ReleaseNotes = release.Notes |> toLines
                WorkingDir = packingDir
                ToolPath = nugetToolPath
                OutputPath = dropDir
                })
        packFile
)

Target "Publish" (fun _ -> 
    let nugetRegex = getRegEx @"([0-9]+.)+[0-9]+(-[a-zA-Z]+\d*)?(.[0-9]+)?"

    !! (dropDir @@ "/**/*.nupkg")
        |> Seq.iter (fun nupckgFile -> 
            let filename = nupckgFile |> filename
            tracefn "Publishing: %A"  (filename)
            let filenameNoExt = fileNameWithoutExt filename
            let nugetCalc = nugetRegex.Match filenameNoExt
            if not nugetCalc.Success
            then failwithf "Unable to parse valid NuGet version from package file name (%s)." filename

            let packageVersion = nugetCalc.Value
            let packageId = filenameNoExt.Remove (filenameNoExt.Length - packageVersion.Length - 1)

            
            tracefn "PackID : %A - PackVersion: %A" packageVersion packageId

            NuGetPublish (fun p -> 
                {p with   
                    Project = packageId
                    Version = packageVersion               
                    OutputPath = dropDir
                    AccessKey = getBuildParamOrDefault "nuget_api_key" ""
                    PublishUrl = getBuildParamOrDefault "nuget_server_url" ""
                    PublishTrials = 0
                    ToolPath = nugetToolPath
                    })
            
            )
)

// Build order
"Clean"
//  ==> "UpdatePackage"
  ==> "Package"

// start build
RunTargetOrDefault "Package"
