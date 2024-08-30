//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS();
RAYTONE_DEFINE_OUTLET(true);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------
    
// DSP chain
RAYTONE_INPUT(0) => Gain gain => blackhole;

// infinite loop
while (true)
{
    // Outlet is the audio sample from 1ms ago
    gain.last() => RAYTONE_OUTLET;
    1::ms => now;
}
