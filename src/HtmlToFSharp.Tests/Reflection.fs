module Reflection

open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler.Interactive.Shell
open System
open System.IO
open System.Text
open Giraffe.GiraffeViewEngine
open Newtonsoft.Json

// Intialize output and input streams
let sbOut = new StringBuilder()
let sbErr = new StringBuilder()
let inStream = new StringReader("")
let outStream = new StringWriter(sbOut)
let errStream = new StringWriter(sbErr)

// Build command line arguments & start FSI session
let argv = [| "fsi.exe" |]
let allArgs = Array.append argv [|"--noninteractive";|]

let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
let fsiSession = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

fsiSession.EvalInteractionNonThrowing("#I __SOURCE_DIRECTORY__") |> ignore
fsiSession.EvalInteractionNonThrowing("#r \"../../../../../packages/Giraffe/lib/net461/Giraffe.dll\"") |> ignore
fsiSession.EvalInteractionNonThrowing("open Giraffe.GiraffeViewEngine") |> ignore

(*
Error Message:
 System.InvalidCastException : [A]Microsoft.FSharp.Collections.FSharpList`1[Giraffe.XmlViewEngine+XmlNode] cannot be cast to [B]Microsoft.FSharp.Collections.FSharpList`1[Giraffe.XmlViewEngine+XmlNode]. Type A originates from 'FSharp.Core, Version=4.4.1.0, Culture=neut
ral, PublicKeyToken=b03f5f7f11d50a3a' in the context 'Default' at location 'C:\Users\nojaf\AppData\Local\Temp\40e737f4-2a2e-4855-bd54-145e15f7f050\40e737f4-2a2e-4855-bd54-145e15f7f050\assembly\dl3\0b4337bc\00ee34fa_7d17d301\FSharp.Core.dll'. Type B originates from 'FS
harp.Core, Version=4.4.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' in the context 'Default' at location 'C:\Users\nojaf\AppData\Local\Temp\40e737f4-2a2e-4855-bd54-145e15f7f050\40e737f4-2a2e-4855-bd54-145e15f7f050\assembly\dl3\0b4337bc\00ee34fa_7d17d301\FSha
rp.Core.dll'.
*)

let createTempPath() =
    sprintf "%s.json" (Guid.NewGuid().ToString("N"))
    |> fun fileName -> Path.Combine(Path.GetTempPath(), fileName)

let evalExpressionAsXmlNodeList text =        
    match fsiSession.EvalExpression(text) with
    | Some value ->
        let jsonPath = createTempPath()
    
        value.ReflectionValue 
        |> JsonConvert.SerializeObject
        |> fun text -> System.IO.File.WriteAllText(jsonPath, text)
        
        let deleteFile () =
            if File.Exists jsonPath then
                File.Delete jsonPath
        
        try
            let nodes =
                File.ReadAllText(jsonPath)
                |> fun json -> JsonConvert.DeserializeObject<XmlNode list>(json)
                
            deleteFile()
            Some nodes
        with
        | _ ->
            deleteFile()
            None
    | _ -> None
    
