#version 330 core

out vec4 FragColor;

in vec2 TexCoord;
in vec4 Color;

uniform int u_depth;
uniform sampler2DArray texture0;

void main()
{
    FragColor = texture(texture0, vec3(TexCoord, u_depth));
}