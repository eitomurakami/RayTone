//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS();
RAYTONE_DEFINE_INLETS("freq", "width");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
PulseOsc osc => RAYTONE_OUTPUT => dac;
0 => osc.freq;

// infinite loop
while (true)
{
    // Read inlets
    RAYTONE_INLET(0) => osc.freq;
    Math.max(Math.min(RAYTONE_INLET(1), 1), 0) => osc.width;

    // Silence osc if frequency is 0
    if (osc.freq() == 0)
    {
        0 => osc.gain;
    }
    else
    {
        RAYTONE_LOCAL_GAIN => osc.gain;
    }

    // Advance time
    1::ms => now;
}
