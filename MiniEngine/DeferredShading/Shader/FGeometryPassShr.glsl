#version 330 core
layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gAlbedoSpec;

in vec2 TexCoords;
in vec3 FragPos;
in vec3 Normal;

uniform sampler2D texture_diffuse1;
//uniform bool haveTexture;
//uniform float specular;

void main()
{    
    // store the fragment position vector in the first gbuffer texture
    gPosition = FragPos;
    // also store the per-fragment normals into the gbuffer
    gNormal = normalize(Normal);
    // and the diffuse per-fragment color
    //if (haveTexture)
    {
        gAlbedoSpec.rgb = texture(texture_diffuse1, TexCoords).rgb;
    }
    //else 
    {
       // gAlbedoSpec.rbg = vec3(0.5f, 0, 0);
    }

    gAlbedoSpec.a = 0;
}