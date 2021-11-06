module Gui

#nowarn "9"
#nowarn "51"
open GalanteMath
open Galante
open Silk.NET.Input
open BaseWindowSlice
open BaseMouseSlice
open System.Drawing
open BaseCameraSlice
open System.Numerics
open Galante.OpenGL
open Silk.NET.Windowing
open BaselineState
open System
open BaseAssetSlice
open BaseGlSlice
open Gl
open Silk.NET.OpenGL
open Microsoft.FSharp
open Microsoft.FSharp.NativeInterop
open BaseGuiSlice
open Silk.NET.OpenGL.Extensions.ImGui
open ImGuiNET

let specialColor = v4 0.9f 0.7f 0.2f 1.0f
let fontScale = 1.f

let margin = 5.0f
let margin2 = margin * 2.0f
let padding = 10.0f
let padding2 = 10.0f * 2.0f
let paddingV2 = v2 padding padding

let resolutionToString (res: Size) = $"{res.Width}x{res.Height}" 

let v3toString (v3: Vector3) =
   sprintf
      "<%.2fx %.2fy %.2fz>" 
      (Math.Round(float v3.X, 2))
      (Math.Round(float v3.Y, 2)) 
      (Math.Round(float v3.Z, 2))

let degToString (Degrees value) = value.ToString()

let controlToString control =
   match control with
   | KeyboardKey keyboardKey -> keyboardKey.ToString()
   | MouseKey mouseKey -> mouseKey.ToString()
   | MouseMove -> "MOUSE MOVE"
   | MouseWheelMove -> "MOUSE WHEEL"

let controlCombinationsToString combinations =
   combinations
   |> List.map (fun combination ->
      match combination with
      | Single control ->  $"[{controlToString control}]"
      | Multiple controls -> 
         controls
         |> List.map controlToString
         |> String.concat " + "
         |> fun combinationStr -> $"[{combinationStr}]"
   )
   |> String.concat " "

let textLineHeight = ImGui.GetTextLineHeightWithSpacing ()

let defaultWindowFlags =
   ImGuiWindowFlags.NoMove |||
   ImGuiWindowFlags.NoResize |||
   ImGuiWindowFlags.NoTitleBar

// Defines global styles
let guiConfigureStyles () =
   ImGui.PushStyleVar (ImGuiStyleVar.WindowRounding, 0.0f)
   ImGui.PushStyleVar (ImGuiStyleVar.WindowBorderSize, 0.0f)
   ImGui.PushStyleColor (ImGuiCol.WindowBg, v4 0.0f 0.0f 0.0f 0.6f)

// Defines window sizes *******************************************************
// Window Size - Locked State
let msgLockedCamera = "Locked camera"
let msgUnlockedCamera = "Unlocked camera"
let msgLockedStateInstruction = "F9/F10 (un)locks"

let windowSize_LockedState =
   let indicatorSize = 
      Vector2.Max (
         ImGui.CalcTextSize msgLockedCamera, 
         ImGui.CalcTextSize msgUnlockedCamera)

   let instructionsSize: Vector2 = ImGui.CalcTextSize msgLockedStateInstruction

   let sizeX: single = 
      MathF.Max(indicatorSize.X, instructionsSize.X) + padding2
   let sizeY: single = indicatorSize.Y + instructionsSize.Y + padding2

   v2 sizeX sizeY

// Window Size - Camera Position
let msgCameraPosition = "Camera position"
let msgNormalizedVector = v3toString <| v3 -1.0f -1.0f -1.0f

let msgCameraDirection = "Camera direction"

let windowSize_CameraPosition =
   let titleSize = ImGui.CalcTextSize msgCameraPosition
   let valueSize = ImGui.CalcTextSize msgNormalizedVector

   let sizeX = MathF.Max(titleSize.X, valueSize.X) + padding2
   let sizeY = titleSize.Y + valueSize.Y + padding2

   v2 sizeX sizeY

// Window Size - Camera Direcation
// Window Size - Instructions to show more

// Defines windows, using calculated sizes of the whole layout ****************
// Window - Locked State
let guiRenderLockedState state =
   let isLockedStateWidgetCreated = 
      ImGui.Begin ("LockedState", defaultWindowFlags)

   ImGui.SetWindowSize windowSize_LockedState

   let position: Vector2 = v2 margin margin
   ImGui.SetWindowPos position 

   if state.Camera.IsLocked then
      ImGui.TextColored (v4 1.0f 0.5f 0.5f 1.0f, msgLockedCamera)
   else
      ImGui.TextColored (v4 0.2f 1.0f 0.2f 1.0f, msgUnlockedCamera)

   ImGui.Text msgLockedStateInstruction

   ImGui.End()
   state

let guiRenderCameraPosition state =
   let isCameraPositionWidgetCreated = 
      ImGui.Begin ("CameraPosition", defaultWindowFlags)

   ImGui.SetWindowSize windowSize_CameraPosition

   let position: Vector2 = v2 (margin2 + windowSize_LockedState.X) margin
   ImGui.SetWindowPos position 

   ImGui.TextColored (specialColor, msgCameraPosition)
   ImGui.Text <| v3toString state.Camera.Position
      
   ImGui.End()
   state

let guiRenderCameraDirection state =
   let isCameraDirectionWidgetCreated = 
      ImGui.Begin ("CameraDirection", defaultWindowFlags)

   // Reuses the camera position vector display window´s size
   ImGui.SetWindowSize windowSize_CameraPosition

   let previousWindowsOffsetX =
      windowSize_LockedState.X + windowSize_CameraPosition.X

   let position: Vector2 = v2 (margin * 3.0f + previousWindowsOffsetX) margin
   ImGui.SetWindowPos position 

   ImGui.TextColored (specialColor, msgCameraDirection)
   ImGui.Text <| v3toString state.Camera.TargetDirection
      
   ImGui.End()
   state

// Uses the minimum resolution used for this exercises. 
// Yes. Hardcoded. Don't look at me like that, the whole repo is a mess.
let minimumScreenWidth = 640.0f

let getWindowSize_InstructionsToShowMore =
   let previousWindowsOffsetX =
      windowSize_LockedState.X + 
      windowSize_CameraPosition.X * 2.0f +
      margin
      
   minimumScreenWidth - previousWindowsOffsetX

let getWindowPosition_InstructionsToShowMore windowWidth state =
   let currentResolutionWidth = single state.Window.Resolution.Width
   v2 (currentResolutionWidth - windowWidth - margin) margin

let guiRenderInstructionsToShowMore state =
   let isShowMoreInfoWidgetCreated = 
      ImGui.Begin ("ShowMoreInfo", defaultWindowFlags)

   let availableWidth = getWindowSize_InstructionsToShowMore - padding2

   // Uses the height of one of the other windows
   v2 availableWidth windowSize_LockedState.Y
   |> ImGui.SetWindowSize

   getWindowPosition_InstructionsToShowMore availableWidth state
   |> ImGui.SetWindowPos 

   ImGui.TextWrapped "F1 to show more info"
      
   ImGui.End()
   state

let guiRenderStickyInfo state =
   let isStickyInfoWidgetCreated = 
      ImGui.Begin ("StickyInfo", defaultWindowFlags)

   v2 margin (windowSize_LockedState.Y + margin2)
   |> ImGui.SetWindowPos 
   
   state.Gui.AlwaysVisibleControlInstructions
   |> List.rev
   |> List.iter (fun instruction ->
      ImGui.SetWindowSize <| v2 0.0f 0.0f

      let fullInfoSetterControlStr = 
         instruction.Controls
         |> controlCombinationsToString
      
      ImGui.TextColored (specialColor, fullInfoSetterControlStr)
      ImGui.TextWrapped instruction.Explanation

      ImGui.Separator()
   )
      
   ImGui.End()
   state

let guiRenderExtendedInfo state =
   let isStickyInfoWidgetCreated = 
      ImGui.Begin ("ExtendedInfo", defaultWindowFlags)

   let width = 300.0f
   let resolutionWidth = single state.Window.Resolution.Width

   ImGui.SetWindowSize <| v2 width 0.0f

   v2 (resolutionWidth - width - margin) (windowSize_LockedState.Y + margin2)
   |> ImGui.SetWindowPos

   state.Gui.GeneralControlInstructions
   |> List.rev
   |> List.iter (fun instruction ->
      instruction.Controls
      |> controlCombinationsToString
      |> fun str -> ImGui.TextColored (specialColor, str)

      ImGui.TextWrapped instruction.Explanation
   )

   ImGui.End()
   state