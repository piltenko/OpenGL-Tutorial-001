#version 330 core

#define NUMBER_OF_POINT_LIGHTS 4

struct Material
{
	sampler2D diffuse;
	sampler2D specular;
	float shininess;
};

struct DirLight
{
	vec3 direction;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct PointLight
{
	vec3 position;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float constant;
	float linear;
	float quadratic;
};

struct SpotLight
{
	vec3 direction;
	vec3 position;
	float cutOff;
	float outerCutOff;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float constant;
	float linear;
	float quadratic;
};

out vec4 color;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;

uniform vec3 viewPos;
uniform Material material;
uniform DirLight dirLight;
uniform SpotLight spotLight;
uniform PointLight pointLights[NUMBER_OF_POINT_LIGHTS];

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);








void main()
{
	vec3 norm = normalize(Normal);
	vec3 viewDir = normalize(viewPos - FragPos);

	vec3 result = CalcDirLight(dirLight, norm, viewDir);

	for(int i = 0; i < NUMBER_OF_POINT_LIGHTS; i++)
	{
		result += CalcPointLight(pointLights[i], norm, FragPos, viewDir);
	}

	result += CalcSpotLight(spotLight, norm, FragPos, viewDir);

	color = vec4(result, 1.0f);
}







vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
	// Getting light direction
	vec3 lightDir = normalize(-light.direction);
	// Diffuse light calculation
	float diff = max(dot(normal, lightDir), 0.0f);
	// Specular light calculation
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0f), material.shininess);
	// Adding directional light contribution
	vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
	vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));
	vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));

	return(ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
	// Getting light direction
	vec3 lightDir = normalize(light.position - fragPos);
	// Diffuse light calculation
	float diff = max(dot(normal, lightDir), 0.0f);
	// Specular light calculation
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0f), material.shininess);
	// Calculating attenuation
	float distance = length(light.position - fragPos);
	float attenuation = 1.0f / (light.constant + light.linear * distance + light.quadratic * pow(distance, 2));
	// Applying point light contribution to the result
	vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
	vec3 diffuse = light.diffuse * attenuation * diff * vec3(texture(material.diffuse, TexCoords));
	vec3 specular = light.specular * attenuation * spec * vec3(texture(material.specular, TexCoords));

	//return(ambient + diffuse + specular);
	return(vec3(0, 0, 0));
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
	// Getting light direction
	vec3 lightDir = normalize(light.position - fragPos);
	// Diffuse light calculation
	float diff = max(dot(normal, lightDir), 0.0f);
	// Specular light calculation
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0f), material.shininess);
	// Calculating attenuation
	float distance = length(light.position - fragPos);
	float attenuation = 1.0f / (light.constant + light.linear * distance + light.quadratic * pow(distance, 2));
	// Calculating spotlight properties
	float theta = dot(lightDir, normalize(-light.direction));
	float epsilon = light.cutOff - light.outerCutOff;
	float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
	// Applying  spotlight contribution to the result
	vec3 ambient = light.ambient * intensity * attenuation * vec3(texture(material.diffuse, TexCoords));
	vec3 diffuse = light.diffuse * intensity * attenuation * diff * vec3(texture(material.diffuse, TexCoords));
	vec3 specular = light.specular * intensity * attenuation * spec * vec3(texture(material.specular, TexCoords));

	return(ambient + diffuse + specular);
}