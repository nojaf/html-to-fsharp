module Tests

open System
open Xunit
open HtmlToFSharp.Engine
open Giraffe.XmlViewEngine
open Reflection

let stripNewLines (html:string) =
    html.Split([|newLine|], StringSplitOptions.RemoveEmptyEntries)
    |> Seq.map (fun line -> line.Trim())
    |> fun res -> String.Concat(res)
    
let assertEqual expected actual =
    Assert.Equal(expected, actual)

let parseToGiraffe (html:string) =
    let expected =
        stripNewLines html
    
    let sourceCode =
        expected
        |> parseHtmlString
        |> List.map processRootHtmlNode
        |> fun nodes -> String.Join(newLine, nodes)
        |> fun nodes -> sprintf "[%s%s%s]" newLine nodes newLine
        
    sourceCode
    |> evalExpressionAsXmlNodeList
    
let parseToGiraffeAndBack (html:string) =
    let expected =
        stripNewLines html

    let sourceCode =
        expected
        |> parseHtmlString
        |> List.map processRootHtmlNode
        |> fun nodes -> String.Join(newLine, nodes)
        |> fun nodes -> sprintf "[%s%s%s]" newLine nodes newLine
        
    sourceCode
    |> evalExpressionAsXmlNodeList
    |> fun result ->
        match result with
        | Some giraffeNodes ->
            renderHtmlNodes giraffeNodes
            |> assertEqual expected
        | None ->
            let text =
                sprintf "Unable to process %s%s%s with reflection" newLine sourceCode newLine
            Assert.True(false, text)

[<Fact>]
let ``single known node`` () =
    "<h1>Hello world</h1>"
    |> parseToGiraffeAndBack
    
[<Fact>]
let ``known element with attribute`` () =
    "<div class=\"container\">content</div>"
    |> parseToGiraffeAndBack
    
[<Fact>]
let ``unary tag`` () =
    "<input type=\"email\">"
    |> parseToGiraffeAndBack
    
// [<Fact>]
let ``base tag`` () =
    "<base target=\"_blank\">"
    |> parseToGiraffeAndBack
    
[<Fact>]
let ``unknown tag`` () =
    "<my-app></my-app>"
    |> parseToGiraffeAndBack
    
[<Fact>]
let ``br tag`` () =
    "<br>"
    |> parseToGiraffeAndBack
    
    
(*
    Giraffe adds in two additional spaces
    See https://github.com/dustinmoris/Giraffe/blob/master/src/Giraffe/XmlViewEngine.fs
    let comment (content : string) = rawText (sprintf "<!-- %s -->" content)
*)
[<Fact>]
let ``comment`` () =
    "<!--meh-->"
    |> parseToGiraffe
    |> fun result ->
        match result with
        | Some giraffeNodes ->
            renderHtmlNodes giraffeNodes
            |> assertEqual "<!-- meh -->"
        | None ->
            let text =
                sprintf "Unable to process %s%s%s with reflection" newLine "<!--meh-->" newLine
            Assert.True(false, text)
            
[<Fact>]
let ``element with children`` () =
    """
    <h1>
        Some <strong>strong</strong> title
    </h1>
    """
    |> parseToGiraffeAndBack
    
[<Fact>]
let ``Bulma starter kit`` () =
    """
    <html>
      <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <title>Hello Bulma!</title>
        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css">
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bulma/0.6.1/css/bulma.min.css">
      </head>
      <body>
      <section class="section">
        <div class="container">
          <h1 class="title">
            Hello World
          </h1>
          <p class="subtitle">
            My first website with <strong>Bulma</strong>!
          </p>
        </div>
      </section>
      </body>
    </html>
    """        
    |> parseToGiraffeAndBack
    
[<Fact>]
let ``Bootstrap starter kit`` () =
    """
    <html lang="en">
      <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
        <meta name="description" content="my description">
        <meta name="author" content="me">
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
        <script src="../../../../assets/js/vendor/popper.min.js"></script>
        <script src="../../../../dist/js/bootstrap.min.js"></script>
      </body>
    </html>
    """
    |> parseToGiraffeAndBack