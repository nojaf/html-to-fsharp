#I "./packages"
#r "FAKE/tools/FakeLib.dll"

open Fake

Target "Build" (fun _ ->
    DotNetCli.Build id
)

Run "Build" 