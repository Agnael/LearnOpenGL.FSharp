#version 330 core
struct Material {
	sampler2D diffuseMap;
	sampler2D specularMap;
	float shininess;
};

in VS_OUT {
   vec2 TexCoords;
} fs_in;

uniform mat4 uModel;
uniform Material uMaterial;

out vec4 FragColor;

void main()
{
	vec3 diffMapColor = vec3(1.0, 1.0, 1.0);

	vec2 diffuseMapSize = textureSize(uMaterial.diffuseMap, 0);
	if (diffuseMapSize.x > 1.0 && diffuseMapSize.y > 1.0)
		diffMapColor = texture(uMaterial.diffuseMap, fs_in.TexCoords).rgb;
	
	FragColor = vec4(diffMapColor, 1.0);
}