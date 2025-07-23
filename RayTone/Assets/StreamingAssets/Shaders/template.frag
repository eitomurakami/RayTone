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
#define PI 3.1415926535
//-----------------------------------------------------------------------------


//--------------------UV MOD-------------------- 
vec2 mirror(vec2 uv, vec2 orien);
vec2 pixelate(vec2 uv, vec2 resolution, vec2 cellSize);
vec2 rotate(vec2 uv, vec2 center, float degrees);
vec2 swirl(vec2 uv, vec2 resolution, vec2 center, float radius, float angle);

//--------------------COLOR MOD--------------------
vec4 contrast(vec4 color, float intensity);
vec4 invert(vec4 color);
vec4 quantize(vec4 color, float depth);

//--------------------TEXTURES--------------------
vec4 blurTexture(sampler2D tex, vec2 uv, vec2 resolution, float size);
vec4 bloomTexture(sampler2D tex, vec2 uv, vec2 resolution, float size, float threshold, float intensity);

//--------------------SYNTHESIS--------------------
float ellipse(vec2 uv, vec2 center, vec2 size, float blur);
float fbm(vec2 uv, int octaves);
float noise(vec2 uv);
float random(vec2 uv);
float rect(vec2 uv, vec2 center, vec2 size, float blur, float blurSize);
float grids(vec2 uv, vec2 nums, vec2 widths);
vec3 randomColor(float seed);




//--------------------UV MOD-------------------- 
// Apply a mirror effect
vec2 mirror(vec2 uv, vec2 orien)
{
	return -1 * sign(orien) * (abs(orien * 2 * uv - orien) - abs(orien));
}

// Pixelate UV coordinates with the specified number of pixels per cell
vec2 pixelate(vec2 uv, vec2 resolution, vec2 cellSize)
{
    return floor(uv * resolution / cellSize) / resolution * cellSize;
}

// Rotate UV coordinates by degrees
vec2 rotate(vec2 uv, vec2 center, float degrees)
{
    float angle = radians(degrees);  // degrees to radians

    uv -= center;
    uv *= RAYTONE_RATIO;
    uv *= mat2(cos(angle), -sin(angle), sin(angle), cos(angle));
    uv /= RAYTONE_RATIO;
    uv += center;

    return uv;
}

// Apply a swirl effect with the specified position, radius, and angle
// https://www.shadertoy.com/view/Xscyzn
vec2 swirl(vec2 uv, vec2 resolution, vec2 center, float radius, float angle)
{
	uv -= center;
	float _len = length(uv * vec2(resolution.x / resolution.y, 1.0));
	float _angle = atan(uv.y, uv.x) + angle * smoothstep(radius, 0.0, _len);

	return length(uv) * vec2(cos(_angle), sin(_angle)) + center;
}


//--------------------COLOR MOD--------------------
// Control color contrast using POW function
vec4 contrast(vec4 color, float intensity)
{
    return pow(color, vec4(intensity));
}

// Invert color
vec4 invert(vec4 color)
{
    return vec4(vec3(1.0) - color.xyz, color.a);
}

// Quantize color
vec4 quantize(vec4 color, float depth)
{
    return round(color * depth) / depth;
}

//--------------------TEXTURES--------------------
// Sample a texture and apply simple 8-directional blur
// https://www.shadertoy.com/view/Ms2Xz3
vec4 blurTexture(sampler2D tex, vec2 uv, vec2 resolution, float size)
{
	vec2 texel = size / resolution;
    
    vec4 blur = texture(tex, uv, size);
    blur += texture(tex, uv + vec2(texel.x,0.0), size);    	
    blur += texture(tex, uv + vec2(-texel.x,0.0), size);    	
    blur += texture(tex, uv + vec2(0.0,texel.y), size);    	
    blur += texture(tex, uv + vec2(0.0,-texel.y), size);    	
    blur += texture(tex, uv + vec2(texel.x,texel.y), size);    	
    blur += texture(tex, uv + vec2(-texel.x,texel.y), size);    	
    blur += texture(tex, uv + vec2(texel.x,-texel.y), size);    	
    blur += texture(tex, uv + vec2(-texel.x,-texel.y), size);    
    blur /= 9.0;
    return blur;
}

// Sample a texture and apply bloom using blurTexture with screen blend mode
// https://www.shadertoy.com/view/Ms2Xz3
vec4 bloomTexture(sampler2D tex, vec2 uv, vec2 resolution, float size, float threshold, float intensity)
{
    vec4 color = texture(tex, uv);

	vec4 blur = blurTexture(tex, uv, resolution, size);
    blur = clamp(blur - threshold, 0.0, 1.0) / (1.0 - threshold);

    // screen blend mode
    return 1.0 - (1.0 - color) * (1.0 - blur * intensity);
}


//--------------------SYNTHESIS--------------------
// Generate an ellipse with the specified position and size
// Note: size is relative to resolution; divide size by RAYTONE_RATIO to generate a circle
float ellipse(vec2 uv, vec2 center, vec2 size, float blur)
{
    size *= 0.5;  // diameter to radius
    float dist = 1.0 - clamp(distance(uv / size, center / size), 0.0, 1.0);
    
    // lerp solid and blurred
    return mix(ceil(dist), dist, blur);
}

// Generate FBM
// https://thebookofshaders.com/13/
float fbm(vec2 uv, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;

    vec2 shift = vec2(100.0);

    // Rotate to reduce axial bias
    mat2 rot = mat2(cos(0.5), sin(0.5), -sin(0.5), cos(0.5));

    // Loop of octaves
    for (int i = 0; i < octaves; ++i) 
    {
        value += amplitude * noise(uv);
        uv = rot * uv * 2.0 + shift;
        amplitude *= 0.5;
    }

    return value;
}

// Generate 2D value noise
// https://thebookofshaders.com/11/
float noise(vec2 uv)
{
    vec2 i = floor(uv);
    vec2 f = fract(uv);

    // Four corners in 2D of a tile
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));

    // Smooth Interpolation
    vec2 u = smoothstep(0.0, 1.0, f);

    // Mix 4 coorners percentages
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

// Generate a pseudo-random number
// https://thebookofshaders.com/10/
float random(vec2 uv)
{
    return fract(sin(dot(uv.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

// Generate a rectangle with the specified position and size
// Note: size is relative to resolution; divide size by RAYTONE_RATIO to generate a square
// https://www.shadertoy.com/view/7lBXWm
float rect(vec2 uv, vec2 center, vec2 size, float blur, float blurSize)
{
    size *= 0.5;  // half length
    float dist = length(max(abs(uv - center), size) - size);  // inside area is negative
    float solid = step(0.0, -dist);
    float blurred = smoothstep(-blurSize, blurSize, -dist);

    // lerp solid and blurred
    return mix(solid, blurred, blur);
}

// Generate grids with the specified number of rows and columns
float grids(vec2 uv, vec2 nums, vec2 widths)
{
    float col = step(sin(uv.x * nums.x * 2 * PI - PI * 0.5), clamp(1.0 - widths.x, 0.0, 1.0));
    float row = step(sin(uv.y * nums.y * 2 * PI - PI * 0.5), clamp(1.0 - widths.y, 0.0, 1.0));

    return 1.0 - col * row;
}

// utility function for generating a pseudo-random color (vec3)
vec3 randomColor(float seed)
{
    return vec3(random(vec2(seed)), random(vec2(seed + 0.11)), random(vec2(seed + 0.23)));
}





out vec4 fragColor;
void main()	
{
    // Define UV coordinates based on canvas resolution
	vec2 uv = gl_FragCoord.xy / RAYTONE_RESOLUTION.xy;

	// Read inlet 0 to create yellow columns
	float r = sin(uv.x * RAYTONE_INLET(0) * 100);
	float g = sin(uv.x * RAYTONE_INLET(0) * 100);
	float b = uv.y * (sin(RAYTONE_TIME) * 0.5 + 0.5);  // flashing blue

	// Define texture coordinates based on its resolution
	vec2 tex1_uv = gl_FragCoord.xy / RAYTONE_TEXTURE_RESOLUTION(0).xy;

	// Sample texture
	vec4 tex1 = texture(RAYTONE_TEXTURE(0), tex1_uv);

	// Output is RGB + texture
	fragColor = vec4(r, g, b, 1.0) + tex1;
}