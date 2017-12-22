module HtmlToFSharp.Engine.Attributes

open FSharp.Data
open System

let knownAttributes = 
    [
      "abbr", "_abbr"                 
      "accept", "_accept"               
      "accept-charset", "_acceptCharset"        
      "accesskey", "_accesskey"            
      "action", "_action"               
      "alt", "_alt"                  
      "autocomplete", "_autocomplete"         
      "border", "_border"               
      "challenge", "_challenge"            
      "charset", "_charset"              
      "cite", "_cite"                 
      "class", "_class"                
      "cols", "_cols"                 
      "colspan", "_colspan"              
      "content", "_content"              
      "contenteditable", "_contenteditable"      
      "coords", "_coords"               
      "crossorigin", "_crossorigin"          
      "data", "_data"                 
      "datetime", "_datetime"             
      "dir", "_dir"                  
      "dirname", "_dirname"              
      "download", "_download"             
      "enctype", "_enctype"              
      "for", "_for"                  
      "form", "_form"                 
      "formaction", "_formaction"           
      "formenctype", "_formenctype"          
      "formmethod", "_formmethod"           
      "formtarget", "_formtarget"           
      "headers", "_headers"              
      "height", "_height"               
      "high", "_high"                 
      "href", "_href"                 
      "hreflang", "_hreflang"             
      "http-equiv", "_httpEquiv"            
      "id", "_id"                   
      "keytype", "_keytype"              
      "kind", "_kind"                 
      "label", "_label"                
      "lang", "_lang"                 
      "list", "_list"                 
      "low", "_low"                  
      "manifest", "_manifest"             
      "max", "_max"                  
      "maxlength", "_maxlength"            
      "media", "_media"                
      "mediagroup", "_mediagroup"           
      "method", "_method"               
      "min", "_min"                  
      "minlength", "_minlength"            
      "name", "_name"                 
      "optimum", "_optimum"              
      "pattern", "_pattern"              
      "placeholder", "_placeholder"          
      "poster", "_poster"               
      "preload", "_preload"              
      "rel", "_rel"                  
      "rows", "_rows"                 
      "rowspan", "_rowspan"              
      "sandbox", "_sandbox"              
      "spellcheck", "_spellcheck"           
      "scope", "_scope"                
      "shape", "_shape"                
      "size", "_size"                 
      "sizes", "_sizes"                
      "span", "_span"                 
      "src", "_src"                  
      "srcdoc", "_srcdoc"               
      "srclang", "_srclang"              
      "start", "_start"                
      "step", "_step"                 
      "style", "_style"                
      "tabindex", "_tabindex"             
      "target", "_target"               
      "title", "_title"                
      "translate", "_translate"            
      "type", "_type"                 
      "usemap", "_usemap"               
      "value", "_value"                
      "width", "_width"
      "wrap", "_wrap"
    ] |> Map.ofList

let knownFlagAttributes = 
    [
        "async", "_async"                
        "autofocus", "_autofocus"            
        "autoplay", "_autoplay"             
        "checked", "_checked"              
        "controls", "_controls"             
        "default", "_default"              
        "defer", "_defer"                
        "disabled", "_disabled"             
        "formnovalidate", "_formnovalidate"       
        "hidden", "_hidden"               
        "ismap", "_ismap"                
        "loop", "_loop"                 
        "multiple", "_multiple"             
        "muted", "_muted"                
        "novalidate", "_novalidate"           
        "readonly", "_readonly"             
        "required", "_required"             
        "reversed", "_reversed"             
        "selected", "_selected"             
        "typemustmatch", "_typemustmatch"        
    ] |> Map.ofList
    
let isKnowAttribute (name:string) =
    knownAttributes |> Map.containsKey name    
    
let isKnowFlagAttribute (name:string) =
    knownFlagAttributes |> Map.containsKey name

let parseAttributes (attributes:HtmlAttribute list) =
    match attributes with
    | [] -> "[]"
    | _ ->
        attributes
        |> List.map (fun (HtmlAttribute(name, value)) ->
            match (name,String.IsNullOrWhiteSpace(value)) with
            | (attr', false) when (isKnowAttribute attr') ->
                sprintf "%s \"%s\"" (knownAttributes.[name]) value
            | (_, false) ->
                sprintf "attr \"%s\" \"%s\"" name value
            | (flag', true) when(isKnowFlagAttribute flag') ->
                sprintf "%s" (knownFlagAttributes.[name])
            | (_, true) ->
                sprintf "flag \"%s\"" name
        )
        |> fun attrs -> String.Join("; ", attrs)
        |> sprintf "[ %s ]"