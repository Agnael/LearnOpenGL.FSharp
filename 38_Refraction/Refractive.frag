#version 330 core
out vec4 FragColor;

in vec3 Normal;
in vec3 Position;

uniform vec3 uCameraPosition;
uniform samplerCube uAmbientCubemap;

void main()
{    
   // 1.52 = Refractive index of glass
   float refractiveRatio = 1.00 / 1.52;

   // Camera direction vector
   vec3 I = normalize(Position - uCameraPosition);

   // Reflected camera direction vector, used to sample from the ambient cube,
   // so that it looks like the rendered pixel is a reflection.
   vec3 R = refract(I, normalize(Normal), refractiveRatio);

   FragColor = vec4(texture(uAmbientCubemap, R).rgb, 1.0);
}