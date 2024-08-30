// RayTone internal clock by ChucK

global Event RAYTONE_TICK_CONTROL;
global Event RAYTONE_TICK;
0 => global float RAYTONE_BPM;
125 => global float clock;
50 => global float clock_delay;

// Minimum clock duration is 10 ms
if (clock < 10)
{
    10 => clock;
}

// infinite loop
while (true)
{
    RAYTONE_TICK_CONTROL.broadcast();
    clock_delay::ms => now;
    RAYTONE_TICK.broadcast();
    Math.max(clock - clock_delay, 1)::ms => now;
}