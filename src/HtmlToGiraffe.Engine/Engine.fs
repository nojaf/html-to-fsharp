module HtmlToGiraffe.Engine

open FSharp.Data
open System

let private knownTagFunctions = 
    [
        "a";
        "abbr";
        "address";
        "article";
        "aside";
        "audio";
        "b";
        "bdi";
        "bdo";
        "body";
        "button";
        "canvas";
        "caption";
        "cite";
        "code";
        "colgroup";
        "data";
        "datalist";
        "dd";
        "del";
        "details";
        "dfn";
        "dialog";
        "div";
        "dl";
        "dt";
        "em";
        "fieldset";
        "figcaption";
        "figure";
        "footer";
        "form";
        "h1";
        "h2";
        "h3";
        "h4";
        "h5";
        "h6";
        "head";
        "header";
        "hgroup";
        "html";
        "i";
        "ins";
        "kbd";
        "label";
        "legend";
        "li";
        "main";
        "map";
        "mark";
        "menu";
        "meter";
        "nav";
        "noscript";
        "object";
        "ol";
        "optgroup";
        "option";
        "output";
        "p";
        "pre";
        "progress";
        "q";
        "rp";
        "rt";
        "rtc";
        "ruby";
        "s";
        "samp";
        "script";
        "section";
        "select";
        "small";
        "span";
        "strong";
        "style";
        "sub";
        "summary";
        "sup";
        "table";
        "tbody";
        "td";
        "textarea";
        "tfoot";
        "th";
        "thead";
        "time";
        "title";
        "tr";
        "u";
        "ul";
        "var";
        "video";
    ]

let private isKnownTagFunction tag =
    List.exists (fun t -> t = tag) knownTagFunctions 
    
let private knownVoidFunctions =
    [
        "area" ;
        "base" ;
        "br" ;
        "col" ;
        "embed" ;
        "hr" ;
        "img" ;
        "input" ;
        "link" ;
        "menuitem" ;
        "meta" ;
        "param" ;
        "source" ;
        "track" ;
        "wbr" ;
    ]
    
let private isKnownVoidFunction tag =
    List.exists (fun t -> t = tag) knownVoidFunctions

let private parseAttributes (attributes:HtmlAttribute list) =
    match attributes with
    | [] -> "[]"
    | _ ->
        attributes
        |> List.map (fun (HtmlAttribute(name, value)) ->
            match (String.IsNullOrWhiteSpace(value)) with
            | false ->
                sprintf "attr \"%s\" \"%s\"" name value
            | true ->
                sprintf "flag \"%s\"" name
        )
        |> fun attrs -> String.Join("; ", attrs)
        |> sprintf "[ %s ]"
    
let private parseHtmlTag (attributes:HtmlAttribute list) (elements:(unit -> string)) (tagName:string) =
    let attrs = parseAttributes attributes
    elements()
    |> sprintf "%s %s %s" tagName attrs
    
let private noChildren() = String.Empty 

let private parseUnknownTag name attributes elements (parseChildren:HtmlNode list -> unit -> string) =
    sprintf "tag \"%s\"" name
    |> parseHtmlTag attributes (parseChildren elements)

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
            let childrenIndentation =  
                (+) indentLevel 1 |> getIndentation

            nodes
            |> List.map (processHtmlNode ((+) indentLevel 1))
            |> fun htmls -> String.Join(newLine, htmls)
            |> fun childrenHtml ->
                 sprintf "[%s%s%s%s]" newLine childrenHtml newLine ownIndentation

    match node with
    | HtmlElement(name, attributes, elements) when (isKnownTagFunction name) ->
        parseHtmlTag attributes (parseChildren elements) name
    | HtmlElement(name, attributes, elements) when (name = "base") ->
        parseHtmlTag attributes noChildren "``base``"
    | HtmlElement(name, attributes, elements) when (isKnownVoidFunction name) ->
         parseHtmlTag attributes noChildren name
    | HtmlElement(name, attributes, elements) ->
        parseUnknownTag name attributes elements parseChildren
    | HtmlText content when (content = Environment.NewLine) -> 
        "br []"
    | HtmlText content ->
        sprintf "encodedText \"%s\"" content
    | HtmlComment content -> 
        sprintf "comment  \"%s\"" content
    | HtmlCData content ->
        "CData is deprecated in HTML5"
    |> sprintf "%s%s" ownIndentation
    
let processRootHtmlNode = processHtmlNode 0
    
let parseHtmlString (html:string) = HtmlNode.Parse html