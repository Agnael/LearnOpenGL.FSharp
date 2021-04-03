#version 330 core
struct Material {
	sampler2D diffuseMap;
	sampler2D specularMap;
	float shininess;
};

struct Light {
	vec3 position;

	// Attenuation components
	float constantComponent;
	float linearComponent;
	float quadraticComponent;

	vec3 ambientColor;
	vec3 diffuseColor;
	vec3 specularColor;
};

out vec4 FragColor;
  
in vec3 Normal;
in vec3 CurrentFragPos;
in vec2 TextureCoords;

uniform vec3 uViewerPos;
uniform Material uMaterial;
uniform Light uLight;

float getAttenuation(float d, float Kc, float Kl, float Kq)
{	
	return 1.0f / (Kc + Kl * d + Kq * (d*d));
};

void main()
{
	// Read maps 
	vec3 diffuseMapColor = vec3(texture(uMaterial.diffuseMap, TextureCoords));
	vec3 specularMapColor = vec3(texture(uMaterial.specularMap, TextureCoords));

	// ATTENUATION
	float distanceToLightSrc = length(uLight.position - CurrentFragPos);
	float attenuation = 
		getAttenuation(
			distanceToLightSrc, 
			uLight.constantComponent, 
			uLight.linearComponent, 
			uLight.quadraticComponent);

	// AMBIENT
    vec3 ambient = uLight.ambientColor * diffuseMapColor * attenuation;

	// DIFFUSE ---------------------------------------------------------------
	vec3 normal = normalize(Normal);
	vec3 lightDir = normalize(uLight.position - CurrentFragPos);	
	float diffuseImpact = dot(normal, lightDir);
	float diff = max(diffuseImpact, 0.0);
	vec3 diffuse = uLight.diffuseColor * diff * diffuseMapColor * attenuation;
	
	// SPECULAR --------------------------------------------------------------
	vec3 viewDir = normalize(uViewerPos - CurrentFragPos);
	vec3 reflectionDir = reflect(-lightDir, normal);	
	float spec = pow(max(dot(viewDir, reflectionDir), 0.0), uMaterial.shininess);
	vec3 specular = uLight.specularColor * spec * specularMapColor * attenuation;

	vec3 result = ambient + diffuse + specular;
	FragColor = vec4(result, 1.0);
}  