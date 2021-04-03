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
  * **Colors** ([Original article](https://learnopengl.com/Lighting/Colors))
    * [14_A_Lighting_Scene](#14_a_lighting_scene)
  * **Basic lighting** ([Original article](https://learnopengl.com/Lighting/Basic-Lighting))
    * [15_Phong_Ambient_Lighting](#15_phong_ambient_lighting)  
    * [16_Phong_Diffuse_Lighting](#16_phong_diffuse_lighting)  
    * [17_Phong_Specular_Lighting](#17_phong_specular_lighting)  
  * **Materials** ([Original article](https://learnopengl.com/Lighting/Materials))
    * [18_Setting_Materials](#18_setting_materials)  
    * [19_Light_Properties](#19_light_properties)  
    * [20_Different_Light_Colors](#20_different_light_colors)  
  * **Lighting maps** ([Original article](https://learnopengl.com/Lighting/Lighting-maps))
    * [21_Diffuse_Maps](#21_diffuse_maps)  
    * [22_Specular_Maps](#22_specular_maps)  
    * [23_Emission_Maps](#23_emission_maps)  
  * **Light casters** ([Original article](https://learnopengl.com/Lighting/Light-casters))
    * [24_Directional_Light](#24_directional_light)  
    * [25_Point_Light](#25_point_light)  
    * [26_Spotlight](#26_spotlight)  
  * **Multiple lights** ([Original article](https://learnopengl.com/Lighting/Multiple-lights))
    * Coming soon.
* **Model loading**
    * Coming soon.
* **Advanced OpenGL**
  * **Depth testing** ([Original article](https://learnopengl.com/Advanced-OpenGL/Depth-testing))
    * Coming soon.
  * **Stencil testing** ([Original article](https://learnopengl.com/Advanced-OpenGL/Stencil-testing))
    * Coming soon.
  * **Blending** ([Original article](https://learnopengl.com/Advanced-OpenGL/Blending))
    * Coming soon.
  * **Face culling** ([Original article](https://learnopengl.com/Advanced-OpenGL/Face-culling))
    * Coming soon.
  * **Framebuffers** ([Original article](https://learnopengl.com/Advanced-OpenGL/Framebuffers))
    * Coming soon.
  * **Cubemaps** ([Original article](https://learnopengl.com/Advanced-OpenGL/Cubemaps))
    * Coming soon.
  * **Advanced data** ([Original article](https://learnopengl.com/Advanced-OpenGL/Advanced-Data))
    * Coming soon.
  * **Advanced GLSL** ([Original article](https://learnopengl.com/Advanced-OpenGL/Advanced-GLSL))
    * Coming soon.
  * **Geometry shader** ([Original article](https://learnopengl.com/Advanced-OpenGL/Geometry-Shader))
    * Coming soon.
  * **Instancing** ([Original article](https://learnopengl.com/Advanced-OpenGL/Instancing))
    * Coming soon.
  * **Anti aliasing** ([Original article](https://learnopengl.com/Advanced-OpenGL/Anti-Aliasing))
    * Coming soon.
* **Advanced lighting**
  * **Advanced lighting** ([Original article](https://learnopengl.com/Advanced-Lighting/Advanced-Lighting))
    * Coming soon.
  * **Gamma correction** ([Original article](https://learnopengl.com/Advanced-Lighting/Gamma-Correction))
    * Coming soon.
  * **Shadow mapping** ([Original article](https://learnopengl.com/Advanced-Lighting/Shadows/Shadow-Mapping))
    * Coming soon.
  * **Point shadows** ([Original article](https://learnopengl.com/Advanced-Lighting/Shadows/Point-Shadows))
    * Coming soon.
  * **Normal mapping** ([Original article](https://learnopengl.com/Advanced-Lighting/Normal-Mapping))
    * Coming soon.
  * **Parallax mapping** ([Original article](https://learnopengl.com/Advanced-Lighting/Parallax-Mapping))
    * Coming soon.
  * **HDR** ([Original article](https://learnopengl.com/Advanced-Lighting/HDR))
    * Coming soon.
  * **Bloom** ([Original article](https://learnopengl.com/Advanced-Lighting/Bloom))
    * Coming soon.
  * **Deferred shading** ([Original article](https://learnopengl.com/Advanced-Lighting/Deferred-Shading))
    * Coming soon.
  * **SSAO** ([Original article](https://learnopengl.com/Advanced-Lighting/SSAO))
    * Coming soon.
* **PBR (physically based rendering)**
  * **Lighting** ([Original article](https://learnopengl.com/PBR/Lighting))
    * Coming soon.
  * **IBL - Image Based Lighting - Diffuse irradiance** ([Original article](https://learnopengl.com/PBR/IBL/Diffuse-irradiance))
    * Coming soon.
  * **IBL - Image Based Lighting - Specular IBL** ([Original article](https://learnopengl.com/PBR/IBL/Specular-IBL))
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
<img src="https://user-images.githubusercontent.com/32271030/112563263-c941db00-8db7-11eb-9f8b-ac6c6a418fbf.png" width="642" height="392">

## [Basic Lighting](https://learnopengl.com/Lighting/Basic-Lighting)  
#### 15_Phong_Ambient_Lighting
<img src="https://user-images.githubusercontent.com/32271030/112563349-ec6c8a80-8db7-11eb-9489-63de370d60b5.png" width="642" height="392">

#### 16_Phong_Diffuse_Lighting
<img src="https://user-images.githubusercontent.com/32271030/112570755-68b99a80-8dc5-11eb-803d-d38f2d08787d.png" width="642" height="392">

#### 17_Phong_Specular_Lighting
Note how the reflection of the light moves on each face of the cube when the camera moves.
<img src="https://user-images.githubusercontent.com/32271030/112575657-7c69fe80-8dcf-11eb-8938-4596695771eb.gif" width="642" height="392">

## [Materials](https://learnopengl.com/Lighting/Materials)  
#### 18_Setting_Materials
<img src="https://user-images.githubusercontent.com/32271030/112924274-34f0b480-90e6-11eb-8acf-414888800d95.png" width="642" height="392">  

#### 19_Light_Properties
<img src="https://user-images.githubusercontent.com/32271030/112923955-a11ee880-90e5-11eb-93d9-2df6f38fe222.png" width="642" height="392">  

#### 20_Different_Light_Colors
<img src="https://user-images.githubusercontent.com/32271030/112923956-a2e8ac00-90e5-11eb-9980-91338a512591.gif" width="642" height="392">

## [Lighting maps](https://learnopengl.com/Lighting/Lighting-maps)  
#### 21_Diffuse_Maps
<img src="https://user-images.githubusercontent.com/32271030/112937135-835d7d80-90fd-11eb-8a85-c649352fa5b2.gif" width="642" height="392">

#### 22_Specular_Maps
<img src="https://user-images.githubusercontent.com/32271030/112937162-8f493f80-90fd-11eb-82e8-ae5ebb93d61a.gif" width="642" height="392">

#### 23_Emission_Maps
<img src="https://user-images.githubusercontent.com/32271030/112937189-98d2a780-90fd-11eb-8ca4-30c5cb71cdeb.gif" width="642" height="392">

## [Light casters](https://learnopengl.com/Lighting/Light-casters)
#### 24_Directional_Light
<img src="https://user-images.githubusercontent.com/32271030/113493539-6d154000-94b6-11eb-8064-def97620a384.png" width="642" height="392">

#### 25_Point_Light
<img src="https://user-images.githubusercontent.com/32271030/113493545-743c4e00-94b6-11eb-83cf-43e66d4509a3.gif" width="642" height="392">

#### 26_Spotlight
<img src="https://user-images.githubusercontent.com/32271030/113493549-7bfbf280-94b6-11eb-8f73-f4f5e2f58467.png" width="642" height="392">

## [Multiple lights](https://learnopengl.com/Lighting/Multiple-lights)  
Coming soon.

# Model loading
Coming soon.

# Advanced OpenGL
## [Depth testing](https://learnopengl.com/Advanced-OpenGL/Depth-testing)  
Coming soon.

## [Stencil testing](https://learnopengl.com/Advanced-OpenGL/Stencil-testing)  
Coming soon.

## [Blending](https://learnopengl.com/Advanced-OpenGL/Blending)  
Coming soon.

## [Face culling](https://learnopengl.com/Advanced-OpenGL/Face-culling)  
Coming soon.

## [Framebuffers](https://learnopengl.com/Advanced-OpenGL/Framebuffers)  
Coming soon.

## [Cubemaps](https://learnopengl.com/Advanced-OpenGL/Cubemaps)  
Coming soon..

## [Advanced data](https://learnopengl.com/Advanced-OpenGL/Advanced-Data)  
Coming soon.

## [Advanced GLSL](https://learnopengl.com/Advanced-OpenGL/Advanced-GLSL)  
Coming soon.

## [Geometry shader](https://learnopengl.com/Advanced-OpenGL/Geometry-Shader)  
Coming soon.

## [Instancing](https://learnopengl.com/Advanced-OpenGL/Instancing)  
Coming soon.

## [Anti aliasing](https://learnopengl.com/Advanced-OpenGL/Anti-Aliasing)  
Coming soon.

# Advanced lighting
## [Advanced lighting](https://learnopengl.com/Advanced-Lighting/Advanced-Lighting)  
Coming soon.

## [Gamma Correction](https://learnopengl.com/Advanced-Lighting/Gamma-Correction)  
Coming soon.

## [Shadow mapping](https://learnopengl.com/Advanced-Lighting/Shadows/Shadow-Mapping)  
Coming soon.

## [Point shadows](https://learnopengl.com/Advanced-Lighting/Shadows/Point-Shadows)  
Coming soon.

## [Normal mapping](https://learnopengl.com/Advanced-Lighting/Normal-Mapping)  
Coming soon.

## [Parallax mapping](https://learnopengl.com/Advanced-Lighting/Parallax-Mapping)  
Coming soon.

## [HDR](https://learnopengl.com/Advanced-Lighting/HDR)  
Coming soon.

## [Bloom](https://learnopengl.com/Advanced-Lighting/Bloom)  
Coming soon.

## [Deferred Shading](https://learnopengl.com/Advanced-Lighting/Deferred-Shading)  
Coming soon.

## [SSAO](https://learnopengl.com/Advanced-Lighting/SSAO)  
Coming soon.

# PBR
## [Lighting](https://learnopengl.com/PBR/Lighting)  
Coming soon.

## [IBL - Image Based Lighting - Diffuse irradiance](https://learnopengl.com/PBR/IBL/Diffuse-irradiance)  
Coming soon.

## [IBL - Image Based Lighting - Specular IBL](https://learnopengl.com/PBR/IBL/Specular-IBL)  
Coming soon.

# In practice
Coming soon.
