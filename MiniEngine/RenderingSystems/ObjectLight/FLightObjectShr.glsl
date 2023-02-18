#version 330 core

// output color
out vec4 FragColor;

// color of lightobject
uniform vec3 lightColor;

void main()
{
	FragColor = vec4(lightColor, 1);
}