# GPU Dual Contouring

this is an implementation of the Dual Contouring surface exctraction algorithm running on the GPU. As this project is planned to be used as a real-time deformable terrain it also includes CSG Boolean operations and Chunking.

![Chunking](../assets/chunking.jpg)

## Requirements

- GPU supporting shader level >4.5
- Unity >2019.1.0f2 (only tested on this version so far)

## Realtime updating
![Demo of updating in realtime](../assets/realtime.gif?raw=true)


## Boolean operations

WIP. Union, Intersection, Subtraction are all working most of the time.

![Union](../assets/union.png)
![Intersect](../assets/intersect.png)
![Subtract](../assets/subtract.png)
