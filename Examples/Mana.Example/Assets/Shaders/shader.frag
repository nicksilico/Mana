#version 330 core

struct Material {
    float shininess;
};

uniform Material material;

out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;
uniform sampler2D texture_ambient_roughness_metalness;

uniform vec3 lightPos;
uniform vec3 cameraPos;
uniform vec3 lightColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

//#define VISUALIZE_NORMALS
//#define VISUALIZE_SPECULAR
//#define VISUALIZE_DIFFUSE
//#define VISUALIZE_DIFFUSE_ALPHA

void main()
{
    {
        #ifdef VISUALIZE_SPECULAR
        vec4 s = texture(texture_specular1, TexCoord);
        FragColor = vec4(s.r, s.g, s.b, 1.0);
        return;
        #endif
    }

    {
        #ifdef VISUALIZE_DIFFUSE
        FragColor = texture(texture_diffuse1, TexCoord);
        return;
        #endif
    }

    {
        #ifdef VISUALIZE_DIFFUSE_ALPHA
        vec4 s = texture(texture_diffuse1, TexCoord);
        float r = s.r;
        FragColor = vec4(r, r, r, 1.0);
        return;
        #endif
    }

    {
        #ifdef VISUALIZE_NORMALS
        FragColor = vec4((Normal.xyz + vec3(1.0, 1.0, 1.0)) / 2.0 , 1.0);
        return;
        #endif
    }

    vec4 unlitFragmentColor = texture(texture_diffuse1, TexCoord);

    // ----- AMBIENT -----
    vec3 ambient = lightColor * 0.3;

    // ----- DIFFUSE -----
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    // diff = lighting amount based on dot product
    float diff = max(dot(norm, lightDir), 0.0);
    // diffuse = light's influence on object
    vec3 diffuse = vec3(diff * unlitFragmentColor) * lightColor;

    // ----- SPECULAR -----
    vec3 cameraDir = normalize(cameraPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(cameraDir, reflectDir), 0.0), 2);
    vec3 specular = vec3(texture(texture_specular1, TexCoord) * spec) * lightColor;

    // ----- FINAL -----
    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}
