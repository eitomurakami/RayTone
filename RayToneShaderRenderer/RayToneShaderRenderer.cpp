/*----------------------------------------------------------------------------
*  RayTone: A Node-based Audiovisual Sequencing Environment
*      https://www.raytone.app/
*
*  Copyright 2024 Eito Murakami and John Burnett
*
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
-----------------------------------------------------------------------------*/

//-----------------------------------------------------------------------------
// RayTone Shader Renderer
// Eito Murakami - December 2023. 
// 
// Compile with glad1 OpenGL 4.5 loader (headers + glad.c) added to
// Visual Studio or XCode.
//-----------------------------------------------------------------------------

#include <glad/glad.h>
#include <string>

#if defined(__CYGWIN32__)
#define UNITY_INTERFACE_API __stdcall
#define UNITY_INTERFACE_EXPORT __declspec(dllexport)
#elif defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64) || defined(WINAPI_FAMILY)
#define UNITY_INTERFACE_API __stdcall
#define UNITY_INTERFACE_EXPORT __declspec(dllexport)
#elif defined(__MACH__) || defined(__ANDROID__) || defined(__linux__)
#define UNITY_INTERFACE_API
#define UNITY_INTERFACE_EXPORT
#else
#define UNITY_INTERFACE_API
#define UNITY_INTERFACE_EXPORT
#endif

// 4K resolution
int SCREEN_WIDTH = 3840;
int SCREEN_HEIGHT = 2160;

// number of arguments
#define INLETS_NUM 8
#define TEXTURES_NUM 8

unsigned int program, vertexBuffer;
const GLfloat vertices[] = { -1.0f,-1.0f,0.0f,1.0f,-1.0f,0.0f,-1.0f,1.0f,0.0f,1.0f,-1.0f,0.0f,1.0f,1.0f,0.0f,-1.0f,1.0f,0.0f };
bool programReady = false;
bool shaderReady = false;
float iTime = 0;
float inlets[INLETS_NUM];
unsigned int textures[TEXTURES_NUM];
float textureResolutions[TEXTURES_NUM * 3];

// log callback
typedef void (UNITY_INTERFACE_API* FuncPtr)(const char* str);
FuncPtr errorLogCallback;

std::string vertexShaderText = \
"#version 410\n"
"in vec3 vPos;"
"void main()"
"{"
"gl_Position = vec4(vPos,1.0);"
"}";

std::string fragmentShaderText = \
"#version 410\n"
"uniform vec3 iResolution;"
"uniform float iTime;"
"uniform float inlets[8];"
"uniform sampler2D textures[8];"
"uniform vec3 textureResolutions[8];"
"out vec4 fragColor;"
"void main()"
"{"
"vec2 uv = gl_FragCoord.xy / iResolution.xy;"
"fragColor = vec4(uv.x, 0.0, uv.y * sin(iTime), 1.0);"
"}";

/// <summary>
/// Initialize program
/// </summary>
void Init()
{
    // Load GLAD
    if (gladLoadGL() == 0)
    {
        programReady = false;
        if (errorLogCallback)
        {
            errorLogCallback("Failed to initialize RayTone Shader Renderer.\n");
        }
        return;
    }

    // Init program
    glDeleteProgram(program);
    program = glCreateProgram();
    programReady = true;

    const GLubyte* version = glGetString(GL_VERSION);
    if (errorLogCallback)
    {
        errorLogCallback("RayTone Shader Renderer initialized.\n");
        errorLogCallback(reinterpret_cast<const char*>(version));
        errorLogCallback("\n");
    }
}

/// <summary>
/// Compile shaders
/// </summary>
/// <param name="vertexShaderText_arg"></param>
/// <param name="fragmentShaderText_arg"></param>
/// <returns></returns>
void CompileShaders()
{
    if (!programReady)
    {
        shaderReady = false;
        if (errorLogCallback)
        {
            errorLogCallback("RayTone Shader Renderer not initialized. Cannot compile shaders.\n");
        }
        return;
    }

    // Compile shaders
    int vertexShader = glCreateShader(GL_VERTEX_SHADER);
    int fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
    const char* vertexShaderText_temp = vertexShaderText.c_str();
    const char* fragmentShaderText_temp = fragmentShaderText.c_str();
    glShaderSource(vertexShader, 1, &vertexShaderText_temp, 0);
    glShaderSource(fragmentShader, 1, &fragmentShaderText_temp, 0);
    glCompileShader(vertexShader);
    glCompileShader(fragmentShader);

    // Check for fragment shader compilation error
    int isCompiled = 0;
    glGetShaderiv(fragmentShader, GL_COMPILE_STATUS, &isCompiled);
    std::string errorLog = "";
    if (isCompiled == 0)
    {
        GLint logLength = 0;
        glGetShaderiv(fragmentShader, GL_INFO_LOG_LENGTH, &logLength);
        errorLog.resize(logLength);
        glGetShaderInfoLog(fragmentShader, logLength, &logLength, &errorLog[0]);

        if (errorLogCallback)
        {
            errorLogCallback(errorLog.c_str());
        }

        shaderReady = false;
        return;
    } 

    // Link program
    glAttachShader(program, vertexShader);
    glAttachShader(program, fragmentShader);
    glLinkProgram(program);

    // Detach & delete shaders
    glDetachShader(program, vertexShader);
    glDetachShader(program, fragmentShader);
    glDeleteShader(vertexShader);
    glDeleteShader(fragmentShader);

    shaderReady = true;
    if (errorLogCallback)
    {
        errorLogCallback("Shader compilation successful.\n");
    }
}

/// <summary>
/// Update frame
/// </summary>d
void Render()
{
    if (!programReady || !shaderReady)
    {
        return;
    }
    glUseProgram(program);

    // vertex buffer
    glGenBuffers(1, &vertexBuffer);
    glBindBuffer(GL_ARRAY_BUFFER, vertexBuffer);
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

    // Set vertices positions
    int vpos_location = glGetAttribLocation(program, "vPos");
    glEnableVertexAttribArray(vpos_location);
    glVertexAttribPointer(vpos_location, 3, GL_FLOAT, GL_FALSE, 0, (void*)0);

    // Set uniforms
    int iResolutionLoc = glGetUniformLocation(program, "iResolution");
    if (iResolutionLoc != -1) glUniform3f(iResolutionLoc, SCREEN_WIDTH, SCREEN_HEIGHT, 1);

    int iTimeLoc = glGetUniformLocation(program, "iTime");
    if (iTimeLoc != -1) glUniform1f(iTimeLoc, iTime);

    int inletsLoc = glGetUniformLocation(program, "inlets");
    if (inletsLoc != -1) glUniform1fv(inletsLoc, INLETS_NUM, inlets);

    int texturesLoc = glGetUniformLocation(program, "textures");
    if (texturesLoc != -1)
    {
        for (int i = 0; i < TEXTURES_NUM; i++)
        {
            glActiveTexture(GL_TEXTURE0 + i);
            glBindTexture(GL_TEXTURE_2D, textures[i]);
            glUniform1i(texturesLoc + i, i);
        }
    }

    int textureResolutionLoc = glGetUniformLocation(program, "textureResolutions");
    if (textureResolutionLoc != -1) glUniform3fv(textureResolutionLoc, TEXTURES_NUM, textureResolutions);

    // Render!
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    glEnable(GL_BLEND);
    glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glViewport(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
    glDrawArrays(GL_TRIANGLES, 0, 6);

    // Delete vertex buffer
    glDeleteBuffers(1, &vertexBuffer);
}

typedef void(UNITY_INTERFACE_API* UnityRenderingEvent)(int eventId);

/// <summary>
/// Execute render event
/// </summary>
/// <param name="eventID"></param>
/// <returns></returns>
static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
    switch (eventID)
    {
    case 0:
        Init();
        break;
    case 1:
        CompileShaders();
        break;
    case 2:
        Render();
        break;
    }
}

/// <summary>
/// Register error log callback
/// </summary>
/// <param name="callback"></param>
/// <returns></returns>
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API RegisterErrorLogCallback(FuncPtr callback)
{
    if (callback)
    {
        errorLogCallback = callback;
    }
}

/// <summary>
/// OnRenderEvent callback
/// </summary>
/// <returns></returns>
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Execute()
{
    return OnRenderEvent;
}

/// <summary>
/// Set fragment shader text
/// </summary>
/// <param name="fragmentShaderText_arg"></param>
/// <returns></returns>
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetFragmentShaderText(const char* fragmentShaderText_arg)
{
    fragmentShaderText = fragmentShaderText_arg;
}

/// <summary>
/// Set time
/// </summary>
/// <param name="time_arg"></param>
/// <returns></returns>
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTime(float time_arg)
{
    iTime = time_arg;
}

/// <summary>
/// Set inlet value
/// </summary>
/// <param name="inletIndex"></param>
/// <param name="val"></param>
/// <returns></returns>
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetInletVal(int inletIndex, float val)
{
    inlets[inletIndex] = val;
}

/// <summary>
/// Set texture pointer
/// </summary>
/// <param name="textureIndex"></param>
/// <param name="texturePtr"></param>
/// <returns></returns>
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTexturePointer(int textureIndex, void* texturePtr, float width, float height)
{
    textures[textureIndex] = (unsigned int)(size_t)texturePtr;
    textureResolutions[textureIndex * 3 + 0] = width;
    textureResolutions[textureIndex * 3 + 1] = height;
    textureResolutions[textureIndex * 3 + 2] = 0;
}

/// <summary>
/// Set rendering resolution
/// </summary>
/// <param name="width"></param>
/// <param name="height"></param>
/// <returns></returns>
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetResolution(int width, int height)
{
    SCREEN_WIDTH = width;
    SCREEN_HEIGHT = height;
}
