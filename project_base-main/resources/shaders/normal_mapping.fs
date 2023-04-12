#version 330 core
out vec4 FragColor;

struct DirLight {
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct PointLight {
    vec3 position;

    vec3 specular;
    vec3 diffuse;
    vec3 ambient;

    float constant;
    float linear;
    float quadratic;
};

in VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentLightPos1;
    vec3 TangentViewPos1;
    vec3 TangentFragPos1;
    vec3 TangentLightPos2;
    vec3 TangentViewPos2;
    vec3 TangentFragPos2;
} fs_in;

uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
uniform DirLight dirLight;
uniform PointLight pointLight1;
uniform PointLight pointLight2;
//uniform vec3 lightPos;
//uniform vec3 viewPos;

vec3 CalcPointLight1(PointLight light, vec3 normal, vec3 viewDir)
{
    // diffuse
    vec3 lightDir = normalize(fs_in.TangentLightPos1 - fs_in.TangentFragPos1);
    float diff = max(dot(lightDir, normal), 0.0);

    // specular shading
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);



    // attenuation
    float distance = length(light.position - fs_in.TangentFragPos1);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    // blending
    vec3 color = texture(diffuseMap, fs_in.TexCoords).rgb;

    // combine results
    vec3 ambient = light.ambient * color;
    vec3 diffuse = light.diffuse * diff * color;
    vec3 specular = light.specular * spec * color;
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec3 CalcPointLight2(PointLight light, vec3 normal, vec3 viewDir)
{
    // diffuse
    vec3 lightDir = normalize(fs_in.TangentLightPos2 - fs_in.TangentFragPos2);
    float diff = max(dot(lightDir, normal), 0.0);

    // specular shading
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);



    // attenuation
    float distance = length(light.position - fs_in.TangentFragPos2);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    // blending
    vec3 color = texture(diffuseMap, fs_in.TexCoords).rgb;

    // combine results
    vec3 ambient = light.ambient * color;
    vec3 diffuse = light.diffuse * diff * color;
    vec3 specular = light.specular * spec * color;
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.direction);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);


    // get diffuse color
    vec3 color = texture(diffuseMap, fs_in.TexCoords).rgb;
        // ambient
    vec3 ambient = light.ambient * color;
    vec3 diffuse = light.diffuse * diff * color;
    vec3 specular = light.specular * spec * color;


    return (ambient + diffuse + specular);
}

void main()
{           
     // obtain normal from normal map in range [0,1]
    vec3 normal = texture(normalMap, fs_in.TexCoords).rgb;
    // transform normal vector to range [-1,1]
    normal = normalize(normal * 2.0 - 1.0);  // this normal is in tangent space


   vec3 viewDir1 = normalize(fs_in.TangentViewPos1 - fs_in.TangentFragPos1);
   vec3 viewDir2 = normalize(fs_in.TangentViewPos2 - fs_in.TangentFragPos2);

   vec3 result = CalcDirLight(dirLight, normal, viewDir1) +
                    CalcPointLight1(pointLight1, normal, viewDir1) +
                    CalcPointLight2(pointLight2, normal, viewDir2);

   FragColor = vec4(result, 1.0);
}