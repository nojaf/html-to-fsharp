module HtmlToFSharp.Engine.Tags

open FSharp.Data
open System
open Attributes
open HtmlToFSharp.Engine

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
 
let isKnownTagFunction tag =
    List.exists (fun t -> t = tag) knownTagFunctions 
    
let isKnownVoidFunction tag =
    List.exists (fun t -> t = tag) knownVoidFunctions
    
let parseHtmlTag (attributes:HtmlAttribute list) (elements:(unit -> string)) (tagName:string) =
    let attrs = parseAttributes attributes
    elements()
    |> sprintf "%s %s %s" tagName attrs
    
let noChildren() = String.Empty 

let parseUnknownTag name attributes elements (parseChildren:HtmlNode list -> unit -> string) =
    sprintf "tag \"%s\"" name
    |> parseHtmlTag attributes (parseChildren elements)