//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS();
RAYTONE_DEFINE_INLETS();
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
Noise noise => RAYTONE_OUTPUT => dac;
RAYTONE_LOCAL_GAIN => noise.gain;

// infinite loop
while (true)
{
    1000::ms => now;
}
