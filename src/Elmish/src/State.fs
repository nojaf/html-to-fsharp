module App.State

open Elmish
open Types
open System
open Fable.PowerPack
open Fable.PowerPack.Fetch
open System.Diagnostics
open System.Net
open System.Net.Http.Headers
open Fable.Helpers.React
open Fable.Import.Browser

let init _ =
  let model = {
    Input = String.Empty
    TransformFailed = None
    Output = String.Empty
    ShowCopiedMessage = false
  }
  model, Cmd.none

let rootUrl =
  #if DEBUG
    "http://localhost:5000/"
  #else
    "/"
  #endif

let transformRequest (input:string) =
  let url = sprintf "%stransform" rootUrl
  let model = {
    content = input
  }
  postRecord url model []
  |> Promise.bind (fun (response:Response) ->
    response.text()
  )

let transformCmd (input:string) =
  Cmd.ofPromise transformRequest input TransformSuccess TransformFailed

let copyToClipboard output dispatch =
    let codeElement = document.querySelector "code pre"
    let range = document.createRange()
    range.selectNode codeElement
    window.getSelection().addRange(range)

    try
        document.execCommand("copy") |> ignore
        window.getSelection().removeAllRanges()
        dispatch Copied
    with
    _ -> ()

let hideCopyMessage dispatch =
    window.setTimeout (fun () -> dispatch ResetCopy
    , 2000) |> ignore


let update msg model =
  match msg with
  | UpdateInput text ->
    { model with Input = text } , Cmd.none
  | Transform ->
    { model with TransformFailed = None }, transformCmd model.Input
  | TransformSuccess result ->
    { model with
        TransformFailed = Some false
        Output = result
      }, Cmd.none
  | TransformFailed ex ->
    { model with TransformFailed = Some true; Output = String.Empty}, Cmd.none
  | CopyToClipboard ->
    model, Cmd.ofSub (copyToClipboard model.Output)
  | Copied ->
    { model with ShowCopiedMessage = true }, Cmd.ofSub hideCopyMessage
  | ResetCopy ->
    { model with ShowCopiedMessage = false }, Cmd.none
