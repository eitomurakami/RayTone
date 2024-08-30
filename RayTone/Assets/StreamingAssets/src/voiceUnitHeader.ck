//-----------------------------------------------------------------------------
// RAYTONE VOICE UNIT OBLIGATORY HEADER
// Include globals
global float RAYTONE_BPM;
global Event RAYTONE_TICK;
global float valArr_chuck[];
global int trigArr_chuck[];
global int statusArr_chuck[];
global int shredId_chuck[];
global float outletArr_chuck[];
global float RAYTONE_LOCAL_GAIN;

// Read voice ID
Std.atoi(me.arg(0)) => int raytone_id;
// Write shred ID
me.id() => shredId_chuck[raytone_id];
// Read inlet index
Std.atoi(me.arg(1)) => int raytone_index;
// Read file directory
me.arg(2) => string RAYTONE_FILEPATH;
//-----------------------------------------------------------------------------

