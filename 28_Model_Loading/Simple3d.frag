#version 330 core
struct Material {
	sampler2D diffuseMap;
	sampler2D specularMap;
	float shininess;
};

struct DirectionalLight {
	vec3 direction;

	vec3 ambientColor;
	vec3 diffuseColor;
	vec3 specularColor;
};

struct PointLight {
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

in vec3 CurrentFragPos;
in vec3 Normal;
in vec2 TexCoords;

uniform mat4 uModel;
uniform vec3 uViewerPos;
uniform Material uMaterial;
uniform PointLight uPointLight;
uniform DirectionalLight uDirectionalLight;

float getAttenuation(float d, float Kc, float Kl, float Kq)
{	
	return 1.0f / (Kc + Kl * d + Kq * (d*d));
};

vec3 getDirectionalLightColor(
	DirectionalLight light, 
	vec3 normal, 
	vec3 viewDir, 
	vec3 diffMapColor, 
	vec3 specMapColor)
{
	vec3 lightDir = normalize(-light.direction);

	// Diffuse shading
	float diff = max(dot(normal, lightDir), 0.0);

	// Specular shading
	vec3 reflectedDirection = reflect(-lightDir, normal);
	float spec = 
		pow(max(dot(viewDir, reflectedDirection), 0.0), uMaterial.shininess);

	// Results
	vec3 ambient = light.ambientColor * diffMapColor;
	vec3 diffuse = light.diffuseColor * diff * diffMapColor;
	vec3 specular = light.specularColor * spec * specMapColor;

	return (ambient + diffuse + specular);
};

vec3 getPointLightColor(
	PointLight light, 
	vec3 normal, 
	vec3 fragPos, 
	vec3 viewDir, 
	vec3 diffMapColor, 
	vec3 specMapColor)
{
	vec3 lightDir = normalize(light.position - fragPos);
	
	// Diffuse shading
	float diff = max(dot(normal, lightDir), 0.0);

	// Specular shading
	vec3 reflectedDirection = reflect(-lightDir, normal);
	float spec = 
		pow(max(dot(viewDir, reflectedDirection), 0.0), uMaterial.shininess);

	// Attenuation
	float distance = length(light.position - fragPos);
	float attenuation = 
		getAttenuation(
			distance, 
			light.constantComponent,
			light.linearComponent, 
			light.quadraticComponent);

	// Results
	vec3 ambient = light.ambientColor * diffMapColor * attenuation;
	vec3 diffuse = light.diffuseColor * diff * diffMapColor * attenuation;
	vec3 specular = light.specularColor * spec * specMapColor * attenuation;

	return (ambient + diffuse + specular);
};

void main()
{
//	FragColor = texture(uMaterial.diffuseMap, TexCoords);
//	return;

	vec3 viewDir = normalize(uViewerPos - CurrentFragPos);

	vec3 diffMapColor = vec3(1.0, 1.0, 1.0);
	vec3 specMapColor = vec3(1.0, 1.0, 1.0);
	vec3 normalMapColor = vec3(1.0, 1.0, 1.0);

	vec2 diffuseMapSize = textureSize(uMaterial.diffuseMap, 0);
	if (diffuseMapSize.x > 1.0 && diffuseMapSize.y > 1.0)
		diffMapColor = texture(uMaterial.diffuseMap, TexCoords).rgb;
	
	vec2 specularMapSize = textureSize(uMaterial.specularMap, 0);
	if (specularMapSize.x > 1.0 && specularMapSize.y > 1.0)
		specMapColor = texture(uMaterial.specularMap, TexCoords).rgb;
	
//	vec3 normal = normalize(normalMapColor * 2.0 - 1.0);
	vec3 normal = Normal;

	vec3 result = 
		getDirectionalLightColor(
			uDirectionalLight, 
			normal, 
			viewDir, 
			diffMapColor, 
			specMapColor);
	
	result += 
		getPointLightColor(
			uPointLight, 
			normal, 
			CurrentFragPos, 
			viewDir, 
			diffMapColor, 
			specMapColor);

	FragColor = vec4(result, 1.0);
}