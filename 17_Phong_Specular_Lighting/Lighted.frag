#version 330 core
out vec4 FragColor;
  
uniform vec3 uObjectColor;
uniform vec3 uLightColor;
uniform vec3 uLightSourcePos;
uniform vec3 uViewerPos;

in vec3 Normal;
in vec3 CurrentFragPos;

void main()
{
    float ambientStrength = 0.1;
	float specularStrength = 0.5;

	// AMBIENT
    vec3 ambient = ambientStrength * uLightColor;

	// DIFFUSE ---------------------------------------------------------------
	vec3 normal = normalize(Normal);
	vec3 lightDir = normalize(uLightSourcePos - CurrentFragPos);
	
	// Calculate the diffuse impact of the light on the current fragment by 
	// taking the dot product between the norm and lightDir vectors.
	float dotProduct = dot(normal, lightDir);
	
	/*************************************************************************
		The  resulting value is then multiplied with the light's color to 
	 get the diffuse component, resulting in a darker diffuse component the 
	 greater the angle between both vectors.
		If the angle between both vectors is greater than 90 degrees then 
	 the result of the dot product will actually become negative and we end 
	 up with a negative diffuse component. For that reason we use the max 
	 function that returns the highest of both its parameters to make sure 
	 the diffuse component (and thus the colors) never become negative. 
		Lighting for negative colors is not really defined so it's best to 
	 stay away from that, unless you're one of those eccentric artists.
    *************************************************************************/
	float diff = max(dotProduct, 0.0);
	vec3 diffuse = diff * uLightColor;
	
	// SPECULAR --------------------------------------------------------------
	/*************************************************************************
		Calculate the view direction vector and the corresponding reflect 
	 vector along the normal axis.
		Note that we negate the lightDir vector. The reflect function expects 
	 the first vector to point from the light source towards the fragment's 
	 position, but the lightDir vector is currently pointing the other way 
	 around: from the fragment towards the light source (this depends on the 
	 order of subtraction earlier on when we calculated the lightDir vector). 
	 To make sure we get the correct reflect vector we reverse its direction 
	 by negating the lightDir vector first.
    *************************************************************************/
	vec3 viewDir = normalize(uViewerPos - CurrentFragPos);
	vec3 reflectionDir = reflect(-lightDir, normal);
	
	/*************************************************************************
		What's left to do is to actually calculate the specular component, with 
	 the following formula.
		We first calculate the dot product between the view direction and the 
	 reflect direction (and make sure it's not negative) and then raise it to 
	 the power of 32. This 32 value is the shininess value of the highlight. 
		The higher the shininess value of an object, the more it properly 
	 reflects the light instead of scattering it all around and thus the 
	 smaller the highlight becomes (With a higher shininess value, like 256, 
	 the reflection is smaller and looks laser-like).
		We don't want the specular component to be too distracting so we keep 
	 the exponent at 32. 
    *************************************************************************/
	int shininess = 32;
	float spec = pow(max(dot(viewDir, reflectionDir), 0.0), shininess);
	vec3 specular = specularStrength * spec * uLightColor;

	/*************************************************************************
	 Now that we have both an ambient and a diffuse component we add both 
	 colors to each other and then multiply the result with the color of 
	 the object to get the resulting fragment's output color:
    *************************************************************************/
	vec3 result = (ambient + diffuse + specular) * uObjectColor;
	FragColor = vec4(result, 1.0);
}  