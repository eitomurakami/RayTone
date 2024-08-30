// predefined ChucK global variables

global float RAYTONE_LOCAL_GAIN;
global float valArr_chuck[10000];
global int trigArr_chuck[10000];
global int statusArr_chuck[10000];
global int shredId_chuck[1000];
global float outletArr_chuck[1000];
global Gain raytone_gain_null;

0.1::second => now;
<<< "ChucK version: " + Machine.version(), "">>>;