//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS();
RAYTONE_DEFINE_OUTLET(true);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------
    
// DSP chain
RAYTONE_INPUT(0) => FFT fft => blackhole;

// Initialize FFT
4096 => fft.size;
Windowing.hann(fft.size()) => fft.window;
fft.size()::samp => dur HOP;
UAnaBlob blob;
second / samp => float srate;

// infinite loop
while (true)
{
    // Advance time
    HOP => now;

    // Compute FFT
    fft.upchuck() @=> blob;
    
    // Find peak FFT magnitude
    0 => int index;
    0 => float max;
    for(int i; i < blob.fvals().size(); i++)
    {
        // Compare and find peak
        if(blob.fvals()[i] > max)
        {
            blob.fvals()[i] => max;
            i => index;
        }
    }

    // Estimate peak frequency
    (index $ float) / fft.size() * srate => RAYTONE_OUTLET;
}
