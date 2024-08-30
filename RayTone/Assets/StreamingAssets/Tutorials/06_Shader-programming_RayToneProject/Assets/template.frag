#version 410

//-----------------------------------------------------------------------------
//RAYTONE HEADER
uniform vec3 iResolution;
uniform float iTime;
uniform float inlets[8];
uniform sampler2D textures[8];
uniform vec3 textureResolutions[8];

#define RAYTONE_RESOLUTION iResolution
#define RAYTONE_TIME iTime
#define RAYTONE_INLET(i) inlets[i]
#define RAYTONE_TEXTURE(i) textures[i]
#define RAYTONE_TEXTURE_RESOLUTION(i) textureResolutions[i]
//-----------------------------------------------------------------------------

out vec4 fragColor;
void main()	
{
	// Define uv coordinates based on canvas resolution
	vec2 uv = gl_FragCoord.xy / RAYTONE_RESOLUTION.xy;

	// Create columns
	float columns = step(0.5, sin(uv.x * RAYTONE_INLET(0)));

	// Define texture coordinates based on its resolution
	vec2 tex1_uv = gl_FragCoord.xy / RAYTONE_TEXTURE_RESOLUTION(0).xy;

	// Apply offset to tex1_uv 
	tex1_uv.x += sin(tex1_uv.y * RAYTONE_INLET(1)) * 0.5;

	// Sample texture
	vec4 tex1 = texture(RAYTONE_TEXTURE(0), tex1_uv);
	tex1 *= 0.5;

	// Output
	fragColor = tex1 * columns;
}