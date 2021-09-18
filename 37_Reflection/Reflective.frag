#version 330 core
out vec4 FragColor;

in vec3 Normal;
in vec3 Position;

uniform vec3 uCameraPosition;
uniform samplerCube uAmbientCubemap;

void main()
{    
   // Camera direction vector
   vec3 I = normalize(Position - uCameraPosition);

   // Reflected camera direction vector, used to sample from the ambient cube,
   // so that it looks like the rendered pixel is a reflection.
   vec3 R = reflect(I, normalize(Normal));

   FragColor = vec4(texture(uAmbientCubemap, R).rgb, 1.0);
}