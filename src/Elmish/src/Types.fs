module App.Types

type Msg =
  | UpdateInput of text:string
  | Transform
  | TransformFailed of exn
  | TransformSuccess of result:string
  | CopyToClipboard
  | Copied
  | ResetCopy

type Model = {
    Input: string
    TransformFailed: bool option
    Output: string
    ShowCopiedMessage: bool
  }

type TransformRequest= {
  content:string
}
