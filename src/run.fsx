#r @"C:\Users\nojaf\Projects\FSharp.Data\src\bin\Debug\FSharp.Data.dll"
#r @"..\packages\FSharp.Compiler.Service\lib\net45\FSharp.Compiler.Service.dll"
#r @"..\packages\Giraffe\lib\net461\Giraffe.dll"

open FSharp.Data
open System
open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler.Interactive.Shell
open Giraffe.XmlViewEngine


open System.IO
open System.Text

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

let evalExpression text =
  match fsiSession.EvalExpression(text) with
  | Some value -> printfn "%A" value.ReflectionValue
  | None -> printfn "Got no result!"

let evalExpressionAsXmlNodeList text =
    match fsiSession.EvalExpression(text) with
    | Some value when (value.ReflectionValue :? XmlNode list) ->
        value.ReflectionValue :?> XmlNode list |> Some
    | _ -> None

// fsiSession.EvalInteractionNonThrowing("sprintf \"%s\" __SOURCE_DIRECTORY__")
fsiSession.EvalInteractionNonThrowing("#I __SOURCE_DIRECTORY__")
fsiSession.EvalInteractionNonThrowing("#r \"./packages/Giraffe/lib/net461/Giraffe.dll\"")
fsiSession.EvalInteractionNonThrowing("open Giraffe.XmlViewEngine")


evalExpression "h1 [] []"

let input = "<h1 title=\"foobar\" required>Test <strong>one</strong> two</h1><br><ng-app></ng-app>"

let html = HtmlNode.Parse input

let knownTagFunctions = 
    [
        "a";
        "abbr";
        "address";
        "article";
        "aside";
        "audio";
        "b";
        "base";
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

let isKnownTagFunction tag =
    List.exists (fun t -> t = tag) knownTagFunctions 
    
let knownVoidFunctions =
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
    
let isKnownVoidFunction tag =
    List.exists (fun t -> t = tag) knownVoidFunctions

let parseAttributes (attributes:HtmlAttribute list) =
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
    
let parseHtmlTag (attributes:HtmlAttribute list) (elements:(unit -> string)) (tagName:string) =
    let attrs = parseAttributes attributes
    elements()
    |> sprintf "%s %s %s" tagName attrs

let noChildren() = String.Empty 

let parseUnknownTag name attributes elements (parseChildren:HtmlNode list -> unit -> string) =
    sprintf "tag \"%s\"" name
    |> parseHtmlTag attributes (parseChildren elements)

let spaces = "  "
let doubleSpaces = sprintf "%s%s" spaces spaces
let newLine = Environment.NewLine

let getIndentation level =
    match level with
    | zero when (zero = 0) -> String.Empty
    | _ ->
        [1..level]
        |> List.map (fun _ -> spaces)
        |> fun spaces -> String.Concat(spaces)

let rec processHtmlNode (indentLevel:int) (node:HtmlNode) =
    let ownIndentation = getIndentation indentLevel
    let parseChildren (nodes:HtmlNode list) =
        fun () ->
            nodes
            |> List.map (processHtmlNode ((+) indentLevel 1))
            |> fun htmls -> String.Join("\n", htmls)
            |> fun childrenHtml ->
                 sprintf "[\n%s\n%s]" childrenHtml ownIndentation

    match node with
    | HtmlElement(name, attributes, elements) when (name = "base") ->
        parseHtmlTag attributes (parseChildren elements) "``base``"
    | HtmlElement(name, attributes, elements) when (isKnownTagFunction name) ->
        parseHtmlTag attributes (parseChildren elements) name
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


let addGiraffeReferences (parsedHtml:string list) =
    let htmlFns = 
        parsedHtml
        |> fun res -> String.Join("\n", res)
        |> fun res -> res.Split([|"\n"|], StringSplitOptions.None) 
        |> Seq.map (sprintf "%s%s" doubleSpaces)
        |> fun res -> String.Join("\n", res)
    
    printfn "%s" htmlFns

    let reference = "#r @\"C:\\Users\\SIDFLOV\\Projects\\Scripts\\packages\\Giraffe\\lib\\net461\\Giraffe.dll\""   
    let openStatement = "open Giraffe.XmlViewEngine"
    let tempFn = "let myHtml ="
    
    String.Concat(
        reference, newLine, 
        openStatement, newLine, 
        newLine, 
        tempFn, newLine, 
        spaces, "[", newLine, 
        htmlFns, newLine, 
        spaces, "]"
    )

// "<h1 title=\"foo\" data-barry required>Test <strong>one</strong> <button><i class=\"fa fa-times\"></i> two</button></h1><br><ng-app></ng-app><h3><em>meh</em><span>2</span></h3>"
// "<h3><em>meh</em><span>2</span></h3>"
// "<ng-app><strong>title</strong></ng-app>"
// "<!-- ma boy -->"
"""
<html lang="en">
  <head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <meta name="description" content="">
    <meta name="author" content="">
    <link rel="icon" href="../../../../favicon.ico">

    <title>Starter Template for Bootstrap</title>

    <link href="../../../../dist/css/bootstrap.min.css" rel="stylesheet">

    <link href="starter-template.css" rel="stylesheet">
  </head>

  <body>

    <nav class="navbar navbar-expand-md navbar-dark bg-dark fixed-top">
      <a class="navbar-brand" href="#">Navbar</a>
      <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarsExampleDefault" aria-controls="navbarsExampleDefault" aria-expanded="false" aria-label="Toggle navigation">
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" id="navbarsExampleDefault">
        <ul class="navbar-nav mr-auto">
          <li class="nav-item active">
            <a class="nav-link" href="#">Home <span class="sr-only">(current)</span></a>
          </li>
          <li class="nav-item">
            <a class="nav-link" href="#">Link</a>
          </li>
          <li class="nav-item">
            <a class="nav-link disabled" href="#">Disabled</a>
          </li>
          <li class="nav-item dropdown">
            <a class="nav-link dropdown-toggle" href="http://example.com" id="dropdown01" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">Dropdown</a>
            <div class="dropdown-menu" aria-labelledby="dropdown01">
              <a class="dropdown-item" href="#">Action</a>
              <a class="dropdown-item" href="#">Another action</a>
              <a class="dropdown-item" href="#">Something else here</a>
            </div>
          </li>
        </ul>
        <form class="form-inline my-2 my-lg-0">
          <input class="form-control mr-sm-2" type="text" placeholder="Search" aria-label="Search">
          <button class="btn btn-outline-success my-2 my-sm-0" type="submit">Search</button>
        </form>
      </div>
    </nav>

    <main role="main" class="container">

      <div class="starter-template">
        <h1>Bootstrap starter template</h1>
        <p class="lead">Use this document as a way to quickly start any new project.<br> All you get is this text and a mostly barebones HTML document.</p>
      </div>

    </main>
    <script src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
    <script>window.jQuery || document.write('<script src="../../../../assets/js/vendor/jquery.min.js"><\/script>')</script>
    <script src="../../../../assets/js/vendor/popper.min.js"></script>
    <script src="../../../../dist/js/bootstrap.min.js"></script>
  </body>
</html>
"""
|> HtmlNode.Parse
|> List.map (processHtmlNode 0)
|> fun ps -> String.Join("\n", ps)
|> fun nodes -> sprintf "[%s%s%s]" newLine nodes newLine
|> evalExpressionAsXmlNodeList
|> fun result ->
    match result with
    | Some nodes ->
        renderHtmlNodes nodes
        |> printfn "%s"
    | None ->
        printfn "couldn't parse nodes"
//|> fun ps -> String.Join("\n", ps)
// |> addGiraffeReferences
// |> fun result -> IO.File.WriteAllText(@"C:\Temp\html-result.fsx", result)

(*
    perfect unit test:
    -----------------
    html:string -> Parse -> XmlNode list -> XmlViewEngine -> result:string
    AssertEqual html result
*)

// https://fsharp.github.io/FSharp.Compiler.Service/interactive.html