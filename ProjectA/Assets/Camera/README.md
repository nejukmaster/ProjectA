Camera Controller
=================
The Camera Controller is a script that controls the movement of the camera during the game. The main functions of this script are as follows.
> Rotating around the character
>
> Track characters
>
> Camera Walk

### Rotating around the character
In Open World games, the camera has to move freely than the other games. Therefore, I made a camera that rotates around the character in a circle according to the movement of the mouse.

The basic idea is to add vertical and horizontal movements of the mouse to the spherical coordinates of the camera, respectively, and change them back into orthogonal coordinate systems. 
![Alt text](/ExplainImgs/SphericalCoordinateForCamera.png)
