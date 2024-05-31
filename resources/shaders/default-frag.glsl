#version 330 core

in vec3 normal;
in vec2 uv;

out vec4 color;

uniform sampler2D tex;

void main()
{
    vec3 value = texture(tex, uv).rgb;
    color = vec4(value, 1.0);
}