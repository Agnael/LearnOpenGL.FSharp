# LearnOpenGL.FSharp
Exercises based on the https://learnopengl.com/ tutorials, using F# and SILK.NET as the OpenGL wrapper.
Each title that has a link is pointing to the original chapter from the learnopengl.com site, in which the theory is explained and all of the math is being copied from.

This repo can be useful if you are following the excelent tutorials on your own and get stuck on some of them (pointer manipulation is a bitch), but please note that this code is not supposed to set any guideline about how you should structure your own projects. It´s just the result of me trying to learn OpenGL, F#, FRP and functional programming in general, so expect the code to be suboptimal at best. 

Also note that the 'Galante' library i wrote here is just a set of helpers i needed along the way and will need a complete refactor once i have a better understanding about OpenGl, but that´s not likely to happen within this repo.

From exercise 47 and onward, DearImGui is used to display an informative overlay, but this seems to be working only in my PC for some reason. I don't know what's going on (might be driver related) but I'll try to get it to work properly.

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
    * [27_Multiple_Lights](#27_multiple_lights)  
* **Model loading**
  * **Model loading** ([Original article](https://learnopengl.com/Model-Loading/Model))
    * [28_Model_Loading](#28_model_loading)
* **Advanced OpenGL**
  * **Depth testing** ([Original article](https://learnopengl.com/Advanced-OpenGL/Depth-testing))
    * [29_Visualizing_The_Depth_Buffer](#29_visualizing_the_depth_buffer)
  * **Stencil testing** ([Original article](https://learnopengl.com/Advanced-OpenGL/Stencil-testing))
    * [30_Stencil_Testing](#30_stencil_testing)
  * **Blending** ([Original article](https://learnopengl.com/Advanced-OpenGL/Blending))
    * [31_Discarding_Fragments](#31_discarding_fragments)
    * [32_Blending](#32_blending)
  * **Face culling** ([Original article](https://learnopengl.com/Advanced-OpenGL/Face-culling))
    * [33_Face_Culling](#33_face_culling)
  * **Framebuffers** ([Original article](https://learnopengl.com/Advanced-OpenGL/Framebuffers))
    * [34_Rendering_To_A_Texture](#34_rendering_to_a_texture)
    * [35_PostProcessing_Kernel_Effects](#35_postprocessing_kernel_effects)
  * **Cubemaps** ([Original article](https://learnopengl.com/Advanced-OpenGL/Cubemaps))
    * [36_Skybox](#36_skybox)
    * [37_Reflection](#37_reflection)
    * [38_Refraction](#38_refraction)
  * **Advanced data** ([Original article](https://learnopengl.com/Advanced-OpenGL/Advanced-Data))
    * No visual representation.
  * **Advanced GLSL** ([Original article](https://learnopengl.com/Advanced-OpenGL/Advanced-GLSL))
    * [39_Vertex_Shader_Variables](#39_vertex_shader_variables)
    * [40_Fragment_Shader_Variables](#40_fragment_shader_variables)
    * [41_Uniform_Buffer_Objects](#41_uniform_buffer_objects)
  * **Geometry shader** ([Original article](https://learnopengl.com/Advanced-OpenGL/Geometry-Shader))
    * [42_Building_Houses](#42_building_houses)
    * [43_Exploding_Objects](#43_exploding_objects)
    * [44_Visualizing_Normal_Vectors](#44_visualizing_normal_vectors)
  * **Instancing** ([Original article](https://learnopengl.com/Advanced-OpenGL/Instancing))
    * [45_Instancing](#45_instancing)
  * **Anti aliasing** ([Original article](https://learnopengl.com/Advanced-OpenGL/Anti-Aliasing))
    * [46_Antialiasing](#46_antialiasing)
* **Advanced lighting**
  * **Advanced lighting** ([Original article](https://learnopengl.com/Advanced-Lighting/Advanced-Lighting))
    * [47_Blinn_Phong](#47_blinn_phong)
  * **Gamma correction** ([Original article](https://learnopengl.com/Advanced-Lighting/Gamma-Correction))
    * [48_Gamma_Correction](#48_gamma_correction)
  * **Shadow mapping** ([Original article](https://learnopengl.com/Advanced-Lighting/Shadows/Shadow-Mapping))
    * [49_Shadow_Mapping](#49_shadow_mapping)
  * **Point shadows** ([Original article](https://learnopengl.com/Advanced-Lighting/Shadows/Point-Shadows))
    * [50_Point_Shadows](#50_point_shadows)
  * **Normal mapping** ([Original article](https://learnopengl.com/Advanced-Lighting/Normal-Mapping))
    * [51_Normal_Mapping](#51_normal_mapping)
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
Note that although this light source looks similar as in the previous examples, this time it's not reaching the objects that are too far away from it, giving a more realisting feel to the source.

<img src="https://user-images.githubusercontent.com/32271030/113493545-743c4e00-94b6-11eb-83cf-43e66d4509a3.gif" width="642" height="392">

#### 26_Spotlight
<img src="https://user-images.githubusercontent.com/32271030/113493549-7bfbf280-94b6-11eb-8f73-f4f5e2f58467.png" width="642" height="392">

## [Multiple lights](https://learnopengl.com/Lighting/Multiple-lights)  
#### 27_Multiple_Lights
<img src="https://user-images.githubusercontent.com/32271030/113494742-b1f2a400-94c1-11eb-8ed3-cf4b73fa2031.gif" width="642" height="392">

# Model loading
## [Model loading](https://learnopengl.com/Model-Loading/Model) 
#### 28_Model_Loading
<img src="https://user-images.githubusercontent.com/32271030/114671951-01637c00-9cdb-11eb-98b1-a6fc02eb570c.gif" width="642" height="392">

<img src="https://user-images.githubusercontent.com/32271030/114672225-48ea0800-9cdb-11eb-9ca9-c06e75bc8d97.gif" width="642" height="392">

# Advanced OpenGL
## [Depth testing](https://learnopengl.com/Advanced-OpenGL/Depth-testing)  
#### 29_Visualizing_The_Depth_Buffer
<img src="https://user-images.githubusercontent.com/32271030/114982996-78794b80-9e66-11eb-9f79-9645d7e76c1b.png" width="642" height="392">

## [Stencil testing](https://learnopengl.com/Advanced-OpenGL/Stencil-testing)  
#### 30_Stencil_Testing
<img src="https://user-images.githubusercontent.com/32271030/115124511-09504400-9f99-11eb-91b5-a2e87411e718.png" width="642" height="392">

## [Blending](https://learnopengl.com/Advanced-OpenGL/Blending)  
#### 31_Discarding_Fragments
<img src="https://user-images.githubusercontent.com/32271030/133192360-5b4748f2-d88f-49ff-9d72-48a8cfb09ea1.png" width="642" height="392">

#### 32_Blending
<img src="https://user-images.githubusercontent.com/32271030/133192366-7a9a0f81-7c97-4572-a37b-3ffd301ad853.png" width="642" height="392">


## [Face culling](https://learnopengl.com/Advanced-OpenGL/Face-culling)  
#### 33_Face_Culling
Only half of the faces are rendered. For demonstration purposes, the front faces were removed this time.
<img src="https://user-images.githubusercontent.com/32271030/133198964-249688d1-5417-4766-89c9-d47b0350886c.png" width="642" height="392">

## [Framebuffers](https://learnopengl.com/Advanced-OpenGL/Framebuffers)  
#### 34_Rendering_To_A_Texture
<img src="https://user-images.githubusercontent.com/32271030/133543205-6cf18a10-52eb-4789-8b90-c9af7ca01ee0.gif" width="642" height="392">

#### 35_PostProcessing_Kernel_Effects
<img src="https://user-images.githubusercontent.com/32271030/133543181-108f8599-c501-47c4-8763-46423961cce7.png" width="642" height="392">

## [Cubemaps](https://learnopengl.com/Advanced-OpenGL/Cubemaps)  
#### 36_Skybox
<img src="https://user-images.githubusercontent.com/32271030/133737513-e4237c93-943c-4bb7-97ae-c84f6dac0ac5.gif" width="642" height="392">

#### 37_Reflection
<img src="https://user-images.githubusercontent.com/32271030/133899732-354a458e-3488-4549-8aa6-5a18ed9978ac.gif" width="642" height="392">

#### 38_Refraction
<img src="https://user-images.githubusercontent.com/32271030/133935594-4d76d620-3dd7-4c5f-8e19-3173741636f3.gif" width="642" height="392">

## [Advanced data](https://learnopengl.com/Advanced-OpenGL/Advanced-Data)  
No visual representation.

## [Advanced GLSL](https://learnopengl.com/Advanced-OpenGL/Advanced-GLSL)
#### 39_Vertex_Shader_Variables
<img src="https://user-images.githubusercontent.com/32271030/134447875-6204b59d-a4fd-4591-bb29-a762dab07e2f.png" width="642" height="392">

#### 40_Fragment_Shader_Variables
<img src="https://user-images.githubusercontent.com/32271030/134447878-a78bbaa2-94d6-4372-89a9-23045ad2fde0.gif" width="642" height="392">

#### 41_Uniform_Buffer_Objects
<img src="https://user-images.githubusercontent.com/32271030/134447882-45a10056-2377-4617-9011-2a4fb4d3fc25.png" width="642" height="392">

## [Geometry shader](https://learnopengl.com/Advanced-OpenGL/Geometry-Shader)  
#### 42_Building_Houses
<img src="https://user-images.githubusercontent.com/32271030/135960115-588c856d-db0a-4a4e-9d25-8fc4af3da3dd.png" width="642" height="392">

#### 43_Exploding_Objects
<img src="https://user-images.githubusercontent.com/32271030/135960122-63fc5554-4e3d-4d28-86e0-dce64c180214.gif" width="642" height="392">

#### 44_Visualizing_Normal_Vectors
<img src="https://user-images.githubusercontent.com/32271030/135960123-5f98cbe0-be38-45bc-881a-cc530090af36.png" width="642" height="392">

## [Instancing](https://learnopengl.com/Advanced-OpenGL/Instancing)  
#### 45_Instancing
Rendering 100.000 asteroids went from 4 FPS to 80+ FPS when started using the instanced model matrix array.

<img src="https://user-images.githubusercontent.com/32271030/137234727-423c1aed-ba45-43e0-ac96-89ef24cbca6e.png" width="642" height="392">

## [Anti aliasing](https://learnopengl.com/Advanced-OpenGL/Anti-Aliasing)  
#### 46_Antialiasing
<img src="https://user-images.githubusercontent.com/32271030/137436133-e6278f4d-6425-49bb-8c65-f18ce6aa34cc.gif" width="642" height="392">

# Advanced lighting
## [Advanced lighting](https://learnopengl.com/Advanced-Lighting/Advanced-Lighting)  
#### 47_Blinn_Phong
<img src="https://user-images.githubusercontent.com/32271030/141052650-d8f504ae-84d2-48fd-9a0b-7fe80c8fcb02.gif" width="642" height="392">

## [Gamma Correction](https://learnopengl.com/Advanced-Lighting/Gamma-Correction)  
#### 48_Gamma_Correction
<img src="https://user-images.githubusercontent.com/32271030/141051923-dbcdffcc-0362-449c-aba1-9dd986b26a27.gif" width="642" height="392">

## [Shadow mapping](https://learnopengl.com/Advanced-Lighting/Shadows/Shadow-Mapping)  
#### 49_Shadow_Mapping
<img src="https://user-images.githubusercontent.com/32271030/145318854-d95446fb-5419-4a0b-a9ca-4e57ddfc4482.gif" width="642" height="392">

## [Point shadows](https://learnopengl.com/Advanced-Lighting/Shadows/Point-Shadows)  
#### 50_Point_Shadows
![50_Point_Shadows_MJYPqhwRcr](https://github.com/Agnael/LearnOpenGL.FSharp/assets/32271030/cc55a6fc-1afd-451d-bb2c-09efac8508a4)

## [Normal mapping](https://learnopengl.com/Advanced-Lighting/Normal-Mapping)  
#### 51_Normal_Mapping
![ezgif-4-4ea843e4d3](https://github.com/Agnael/LearnOpenGL.FSharp/assets/32271030/54f94dca-6f61-412c-ac8a-961744062515)
![ezgif-5-987bcc8c2a](https://github.com/Agnael/LearnOpenGL.FSharp/assets/32271030/8a81781d-0261-4990-9648-747ad9a307af)

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
