module HtmlToFSharp.Engine.Main

open FSharp.Data
open System
open Tags

let spaces = "  "
let doubleSpaces = sprintf "%s%s" spaces spaces
let newLine = Environment.NewLine

let private getIndentation level =
    match level with
    | zero when (zero = 0) -> String.Empty
    | _ ->
        [1..level]
        |> List.map (fun _ -> spaces)
        |> fun spaces -> String.Concat(spaces)

let rec processHtmlNode (indentLevel:int) (node:HtmlNode) =
    printfn "indent %i" indentLevel
    let ownIndentation = getIndentation indentLevel
    let parseChildren (nodes:HtmlNode list) =
        fun () ->
            nodes
            |> List.map (processHtmlNode ((+) indentLevel 1))
            |> fun htmls -> String.Join(newLine, htmls)
            |> fun childrenHtml ->
                 sprintf "[%s%s%s%s]" newLine childrenHtml newLine ownIndentation

    match node with
    | HtmlElement(name, attributes, elements) when (isKnownTagFunction name) ->
        parseHtmlTag attributes (parseChildren elements) name
    | HtmlElement(name, attributes, _) when (name = "base") ->
        parseHtmlTag attributes noChildren "``base``"
    | HtmlElement(name, attributes, _) when (isKnownVoidFunction name) ->
         parseHtmlTag attributes noChildren name
    | HtmlElement(name, attributes, elements) ->
        parseUnknownTag name attributes elements parseChildren
    | HtmlText content when (content = Environment.NewLine) -> 
        "br []"
    | HtmlText content ->
        sprintf "encodedText \"%s\"" content
    | HtmlComment content -> 
        sprintf "comment  \"%s\"" content
    | HtmlCData _ ->
        "CData is deprecated in HTML5"
    |> sprintf "%s%s" ownIndentation
    
let processRootHtmlNode = processHtmlNode 0
    
let parseHtmlString (html:string) = HtmlNode.Parse html