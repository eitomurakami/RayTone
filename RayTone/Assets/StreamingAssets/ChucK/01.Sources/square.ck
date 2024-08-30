//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS();
RAYTONE_DEFINE_INLETS("freq");
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
SqrOsc osc => RAYTONE_OUTPUT => dac;
0 => osc.freq;

// infinite loop
while (true)
{
    // Read inlets
    RAYTONE_INLET(0) => osc.freq;

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
