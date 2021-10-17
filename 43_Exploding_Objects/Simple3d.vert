﻿#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

out VS_OUT {
   vec2 TexCoords;
} vs_out;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;


in VS_OUT {
   vec3 TexCoords;
} gs_in[];

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
    vs_out.TexCoords = aTexCoords;
}