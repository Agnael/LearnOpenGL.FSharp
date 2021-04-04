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
	vec3 ambientColor;
	vec3 diffuseColor;
	vec3 specularColor;
	float constantComponent;
	float linearComponent;
	float quadraticComponent;
};

struct SpotLight {
	vec3 position;
	vec3 direction;
	float innerCutOffAngleCos;
	float outerCutOffAngleCos;

	vec3 ambientColor;
	vec3 diffuseColor;
	vec3 specularColor;

	float constantComponent;
	float linearComponent;
	float quadraticComponent;
};

out vec4 FragColor;
  
in vec3 Normal;
in vec3 CurrentFragPos;
in vec2 TextureCoords;

uniform vec3 uViewerPos;
uniform Material uMaterial;

#define NR_POINT_LIGHTS 4 
uniform PointLight uPointLights[NR_POINT_LIGHTS];
uniform DirectionalLight uDirectionalLight;
uniform SpotLight uSpotLight;

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

vec3 getSpotlightColor(
	SpotLight light, 
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

	// Spotlight intensity
	float theta = dot(lightDir, normalize(-light.direction));
	float epsilon = light.innerCutOffAngleCos - light.outerCutOffAngleCos;
	float intensity = 
		clamp((theta - light.outerCutOffAngleCos) / epsilon, 0.0, 1.0);
	
	// Attenuation
	float distance = length(light.position - fragPos);
	float attenuation = 
		getAttenuation(
			distance, 
			light.constantComponent,
			light.linearComponent, 
			light.quadraticComponent);

	// Results
	vec3 ambient = light.ambientColor * diffMapColor;
	vec3 diffuse = light.diffuseColor * diffMapColor * attenuation * intensity;
	vec3 specular = 
		light.specularColor * specMapColor * attenuation * intensity;

	return (ambient + diffuse + specular);
};

void main()
{
	vec3 normal = normalize(Normal);
	vec3 viewDir = normalize(uViewerPos - CurrentFragPos);

	vec3 diffMapColor = vec3(texture(uMaterial.diffuseMap, TextureCoords));
	vec3 specMapColor = vec3(texture(uMaterial.specularMap, TextureCoords));

	vec3 result = 
		getDirectionalLightColor(
			uDirectionalLight, 
			normal, 
			viewDir, 
			diffMapColor, 
			specMapColor);

	for (int i = 0; i < NR_POINT_LIGHTS; i++)
	{
		result += 
			getPointLightColor(
				uPointLights[i], 
				normal, 
				CurrentFragPos, 
				viewDir, 
				diffMapColor, 
				specMapColor);
	}

	result += 
		getSpotlightColor(
			uSpotLight,
			normal,
			CurrentFragPos,
			viewDir,
			diffMapColor,
			specMapColor);

	FragColor = vec4(result, 1.0);
}  