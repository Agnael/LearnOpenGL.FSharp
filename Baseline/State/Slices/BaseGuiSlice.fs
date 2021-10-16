module BaseGuiSlice
   open System.Drawing
   open Silk.NET.Input
   open Silk.NET.OpenGL.Extensions.ImGui
        
   type GuiAction =
      | ControllerInitialized of ImGuiController

   type GuiState =
      { Controller: ImGuiController option
      ;}
      static member Default =
         { Controller = None
         ;}

   let reduce a s =
      match a with
      | ControllerInitialized controller -> 
         if controller = null then 
            invalidOp "A null ImGuiController is not a valid initialized value"

         { s with Controller = Some controller }