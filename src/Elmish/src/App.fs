module App.View

open Elmish
open Fable.Core.JsInterop
open State
open Types
open Fulma.Elements
open Fulma.Layouts

importAll "../sass/main.sass"

open Fable.Import.React
open Fable.Helpers.React
open Props
open Form
open System

let inputColumn model dispatch =
    let updateInput (ev:FormEvent) =
        !!ev.currentTarget?value
        |> UpdateInput
        |> dispatch

    Column.column [Column.Width.isHalf] [
        Heading.h2 [] [ str "Input" ]
        Field.field_div [ ] [
            Label.label [ ] [ str "Enter HTML code" ]
            Control.control_div [] [
                Textarea.textarea [
                    Textarea.defaultValue model.Input
                    Textarea.props [OnChange updateInput]
                ] [ ]
            ]
        ]
    ]

let settingsColumn (model:Model) (dispatch:Dispatch<Msg>) =
    let enabled =
        if String.IsNullOrWhiteSpace(model.Input) then
            Button.isDisabled
        else
            Button.props []

    let onGenerateClick _ = dispatch Msg.Transform

    let copyButtonClassName =
        if String.IsNullOrEmpty(model.Output) then
            "is-hidden"
        else
            ""
    let onCopyClick _ = dispatch CopyToClipboard

    Column.column [Column.customClass "has-text-centered"; Column.props [Id "settings"]] [
        Button.button_btn [Button.isPrimary; enabled; Button.onClick onGenerateClick] [
            str "Generate"
        ]
        Button.button_btn [Button.customClass copyButtonClassName; Button.onClick onCopyClick] [
            str "Copy to clipboard"
        ]
    ]

let outputColumn model dispatch =
    let className =
        if String.IsNullOrEmpty(model.Output) then
            "is-hidden"
        else
            ""
        |> ClassName

    let copiedLabel =
        if model.ShowCopiedMessage then
             label [Id "copied"] [str "Copied!"]
        else
            label [ClassName "has-text-white"] [str "white"]


    Column.column [Column.Width.isHalf] [
        Heading.h2 [] [ str "Output" ]
        code [className] [
            pre [] [
                str model.Output
            ]
        ]
        copiedLabel
    ]

let root (model:Model) (dispatch:Dispatch<Msg>) =
    Container.container [] [
        Content.content [] [
            Columns.columns [] [
                inputColumn model dispatch
                settingsColumn model dispatch
                outputColumn model dispatch
            ]
        ]
    ]

open Elmish.React
open Elmish.Debug
open Elmish.HMR

// App
Program.mkProgram init update root
#if DEBUG
|> Program.withDebugger
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
|> Program.run
