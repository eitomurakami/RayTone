//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS();
RAYTONE_DEFINE_INLETS();
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
adc => RAYTONE_OUTPUT => dac;

// infinite loop
while (true)
{
    1::second => now;
}
