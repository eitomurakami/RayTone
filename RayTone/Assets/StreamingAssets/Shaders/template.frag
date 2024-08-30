#version 410

//-----------------------------------------------------------------------------
//RAYTONE HEADER
uniform vec3 iResolution;
uniform float iTime;
uniform float inlets[8];
uniform sampler2D textures[8];
uniform vec3 textureResolutions[8];

#define RAYTONE_RESOLUTION iResolution
#define RAYTONE_RATIO vec2(RAYTONE_RESOLUTION.x / RAYTONE_RESOLUTION.y, 1.0)
#define RAYTONE_TIME iTime
#define RAYTONE_INLET(i) inlets[i]
#define RAYTONE_TEXTURE(i) textures[i]
#define RAYTONE_TEXTURE_RESOLUTION(i) textureResolutions[i]
//-----------------------------------------------------------------------------

out vec4 fragColor;
void main()	
{
	// Define UV coordinates based on canvas resolution
	vec2 uv = gl_FragCoord.xy / RAYTONE_RESOLUTION.xy;

	// Read inlet 0 to create yellow columns
	float r = sin(uv.x * RAYTONE_INLET(0) * 100);
	float g = sin(uv.x * RAYTONE_INLET(0) * 100);
	float b = uv.y * (sin(RAYTONE_TIME) * 0.5 + 0.5); // flashing blue

	// Define texture coordinates based on its resolution
	vec2 tex1_uv = gl_FragCoord.xy / RAYTONE_TEXTURE_RESOLUTION(0).xy;

	// Sample texture
	vec4 tex1 = texture(RAYTONE_TEXTURE(0), tex1_uv);

	// Output is RGB + texture
	fragColor = vec4(r, g, b, 1.0) + tex1;
}