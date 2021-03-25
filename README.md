# LearnOpenGL.FSharp
Exercises based on the https://learnopengl.com/ tutorials, using F# and SILK.NET as the OpenGL wrapper.
Each title title with a link is pointing to the original chapter from the learnopengl.com site on which the theory is explained and all of the math is being copied from.

# Table of contents
* **Getting started**
  * **Hello window** ([Original article](https://learnopengl.com/Getting-started/Hello-Window))
    * [01_Window](#01_window)
  * **Hello triangle** ([Original article](https://learnopengl.com/Getting-started/Hello-Triangle)) and **Shaders** ([Original article](https://learnopengl.com/Getting-started/Shaders))
    * [02_Triangle](#02_triangle)
    * [03_Quad](#03_quad)
  * **Textures** ([Original article](https://learnopengl.com/Getting-started/Textures))
    * [04_Quad_Texture](#04_quad_texture)
    * [05_Quad_Texture_Colored](#05_quad_texture_colored)
    * [06_Quad_Texture_Double](#06_quad_texture_double)
  * **Transformations** ([Original article](https://learnopengl.com/Getting-started/Transformations))
    * [07_Transformations](#07_transformations)
  * **Coordinate systems** ([Original article](https://learnopengl.com/Getting-started/Coordinate-Systems))
    * [08_Coordinate_Systems_Perspective](#08_coordinate_systems_perspective)
    * [09_Coordinate_Systems_Cube](#09_coordinate_systems_cube)
    * [10_Coordinate_Systems_Cube_With_Depth_Testing](#10_coordinate_systems_cube_with_depth_testing)
  * **Camera** ([Original article](https://learnopengl.com/Getting-started/Camera))
    * [12_Camera_Automove](#12_camera_automove)
    * [13_Camera_Walk_Around_With_Inputs](#13_camera_walk_around_with_inputs)   
* **Lighting**
    * [14_A_Lighting_Scene](#14_a_lighting_scene)   
  * In progress. 
* **Model loading**
  * Coming soon.
* **Advanced OpenGL**
  * Coming soon.
* **Advanced lighting**
  * Coming soon.
* **PBR (physically based rendering)**
  * Coming soon.
* **In practice**
  * Coming soon.


# Getting started

## [Hello window](https://learnopengl.com/Getting-started/Hello-Window)
#### 01_Window
<img src="https://user-images.githubusercontent.com/32271030/110713245-5abe2400-81e0-11eb-97a2-9599fe54ad2f.png" width="400" height="420">

## [Hello triangle](https://learnopengl.com/Getting-started/Hello-Triangle) + [Shaders](https://learnopengl.com/Getting-started/Shaders)
#### 02_Triangle
<img src="https://user-images.githubusercontent.com/32271030/110713867-6827de00-81e1-11eb-93d7-fed43910db31.gif" width="400" height="420">

#### 03_Quad
<img src="https://user-images.githubusercontent.com/32271030/110714970-2009bb00-81e3-11eb-8eb4-f499916d2f43.gif" width="400" height="420">

## [Textures](https://learnopengl.com/Getting-started/Textures) 
#### 04_Quad_Texture
<img src="https://user-images.githubusercontent.com/32271030/110714979-239d4200-81e3-11eb-91b3-451d2bbe0d37.png" width="400" height="420">

#### 05_Quad_Texture_Colored
<img src="https://user-images.githubusercontent.com/32271030/110714988-26983280-81e3-11eb-9acc-4cf871e88f12.png" width="400" height="420">

#### 06_Quad_Texture_Double
<img src="https://user-images.githubusercontent.com/32271030/110714993-29932300-81e3-11eb-9a0d-fe9bbadf77a7.png" width="400" height="420">

## [Transformations](https://learnopengl.com/Getting-started/Transformations) 
#### 07_Transformations
This one looks kind of weird because the images are not centered vertically, the intention is to be forced to realize that rotations in OpenGL are calculated taking the origin as the rotation center (and not the object's center) and if a rotation of an object is needed around itself, then said object must be translated to the origin, rotated and then translated back to it's initial position, which may be somewhat counterintuitive.

<img src="https://user-images.githubusercontent.com/32271030/110715001-2bf57d00-81e3-11eb-9dc7-98df6fd2cca2.gif" width="400" height="420">

## [Coordinate systems](https://learnopengl.com/Getting-started/Coordinate-Systems) 
#### 08_Coordinate_Systems_Perspective
<img src="https://user-images.githubusercontent.com/32271030/110715008-2ef06d80-81e3-11eb-8952-f2eaf02fb067.gif" width="400" height="420">

#### 09_Coordinate_Systems_Cube
<img src="https://user-images.githubusercontent.com/32271030/110715016-34e64e80-81e3-11eb-8251-8f75491dbb3c.gif" width="400" height="420">

#### 10_Coordinate_Systems_Cube_With_Depth_Testing
<img src="https://user-images.githubusercontent.com/32271030/110715030-39126c00-81e3-11eb-827b-db4022ad8837.gif" width="400" height="420">

#### 11_Coordinate_Systems_Many_Cubes
<img src="https://user-images.githubusercontent.com/32271030/110715170-7b3bad80-81e3-11eb-83aa-37c7205d0e14.gif" width="400" height="420">

## [Camera](https://learnopengl.com/Getting-started/Camera) 
#### 12_Camera_Automove
<img src="https://user-images.githubusercontent.com/32271030/110715048-40d21080-81e3-11eb-8f9e-3b52a85512b9.gif" width="400" height="420">

#### 13_Camera_Walk_Around_With_Inputs
**W** forward | **A** left | **S** back | **D** right | **SHIFT_LEFT** down | **SPACEBAR** up | **MOUSE_MOVE** for camera | **MOUSE_WHEEL** zoom in/out

<img src="https://user-images.githubusercontent.com/32271030/110715084-55aea400-81e3-11eb-8c91-41a2990751cc.gif" width="400" height="420">


# Lighting
## [Colors](https://learnopengl.com/Lighting/Colors) 
#### 14_A_Lighting_Scene
<img src="https://user-images.githubusercontent.com/32271030/112418419-2c743480-8d08-11eb-80fa-4e2908697324.gif" width="642" height="392">


In progress.

# Model loading
Coming soon.

# Advanced OpenGL
Coming soon.

# Advanced lighting
Coming soon.

# PBR
Coming soon.

# In practice
Coming soon.
