module BaseGuiSlice
   open System.Drawing
   open Silk.NET.Input
   open Silk.NET.OpenGL.Extensions.ImGui
   open System.Numerics
   
   type StateIndicator<'TState> = { 
      Name: string
      GetValue: 'TState -> string
   }

   type StateIndicatorSection<'TState> = {
      Title: string
      Indicators: StateIndicator<'TState> list
   }

   type Control =
      | KeyboardKey of Key
      | MouseKey of MouseButton
      | MouseMove
      | MouseWheelMove

   type ControlCombination =
      | Single of Control
      | Multiple of Control list
      
   type ControlInstruction = {
      Controls: ControlCombination list
      Explanation: string
   }

   type GuiText<'TState> =
      | RegularText of string
      | ColoredText of string * Vector4
      | RegularTextGenerated of ('TState -> string)
      | ColoredTextGenerated of ('TState -> string * Vector4)
      
   type GuiAction<'TMainState> =
      | ControllerInitialized of ImGuiController
      | AddStateIndicatorSection of StateIndicatorSection<'TMainState>
      | AddControlInstruction of ControlInstruction
      | AddAlwaysVisibleControlInstruction of ControlInstruction
      | AddTextLine of GuiText<'TMainState>
      | ShowFullInfo
      | HideFullInfo

   type GuiState<'TMainState> =
      { Controller: ImGuiController option
      ; StateIndicatorSections: StateIndicatorSection<'TMainState> list
      ; TextLines: GuiText<'TMainState> list
      ; GeneralControlInstructions: ControlInstruction list
      ; AlwaysVisibleControlInstructions: ControlInstruction list
      ; ShowFullInfo: bool
      ; ShowFullInfoControl: ControlInstruction
      ;}
      static member Default: GuiState<'TMainState> =
         { Controller = None
         ; StateIndicatorSections = []
         ; GeneralControlInstructions = []
         ; TextLines = []
         ; AlwaysVisibleControlInstructions = []
         ; ShowFullInfo = false
         ; ShowFullInfoControl = 
            {
               Controls = [Single (KeyboardKey Key.F1)]; 
               Explanation = "" 
            }
         ;}

   let reduce<'MainState> (a: GuiAction<'MainState>) s =
      match a with
      | ControllerInitialized controller -> 
         if controller = null then 
            invalidOp "A null ImGuiController is not a valid initialized value"

         { s with Controller = Some controller }
      | AddStateIndicatorSection newItem -> 
         { s with StateIndicatorSections = newItem::s.StateIndicatorSections }
      | AddControlInstruction newItem -> 
         { s with 
               GeneralControlInstructions = 
                  newItem::s.GeneralControlInstructions }
      | AddAlwaysVisibleControlInstruction  newItem ->
         { s with 
               AlwaysVisibleControlInstructions = 
                  newItem::s.AlwaysVisibleControlInstructions }
      | AddTextLine newItem -> 
         { s with TextLines = newItem::s.TextLines }
      | ShowFullInfo -> { s with ShowFullInfo = true }
      | HideFullInfo -> { s with ShowFullInfo = false }