
@email : springu3d@gmail.com

# S.Wireframe

SWireframe is a lighweight mesh generator tool. It can analysis unity mesh asset and generate profile wireframe mesh.

## Screenshot
 
![example1](SWireframe.png)
![example2](SWireframe1.png)

## Features

- not triangle or quad mesh
- adjustable tolerance parameters

    - ingore invalid tirangles(such as all three edge length is zero)
    - support to ignore triangle which all three edge length less than fixed length
    - support to ignore edge which less than fixed length
    - support to ignore isolated edge
    - support to set tolerance about vertex is contained by triangle
    - support to set tolerance about distance between vertex and triangle
- multi thread accelerate generation speed
- supports to generate wireframe in editor mode and play mode

## Hot to use it

### Script parameters:

Description about the primary script:

![script](wireframegenerator.png)

#### WireframeGenerator:

- **(bool) Generate Wireframe at Runtime** : Do you need to generate wireframe mesh in play mode ? If this toggle is checked, it would generate a new wireframe mesh.Wireframe mesh generated at runtime would been deleted when exit play mode.

WireframeAnalyzer Basic Setting:

- **(float) Minimum Angle In Degree / Maximum Angle In Degree** : The angle between two triangles greater than minimum angle and less than maximum angle could generate wireframe. **It's unit is degree rather than radian.**

- **(float) Vertical Angle Tolerance In Degree** : The angle between two triangles greater than (90° - VerticalAngleToleranceInDegree) would been identified as right angle(90°);**It's unit is degree rather than radian.**

- **(float) Tolerance Between Vertex and Triangle** : The distance between vertex and triangle less than ToleranceBetweenVertexAndTriangle would been identified as this vertex is contained by triangle.

- **(float) Tolerance Vertex In Triangle** : The distance between vertex and triangle's edge less than Tolerance would been identified as vertex is contained by triangle.

Ignore Setting:

- **(bool) Ignore Isolated Edge** : If the toggle is checked , generator would remove all the isolated line mesh.

- **(bool) Ingore Minimal Triangle/(float) Minimal Triangle Edge Length** : If all three edges length less than MinimalTriangleEdgeLength, generator would remove edges in this triangle.

- **(float) Ingore Edge Length Less Than** : If this toggle is checked, generator would remove all edge which length is less then IngoreEdgeLengthLessThan.

Multi Thread Setting:

- **(int) MAXIMUM_THREAD_PROCESSING_CAPACITY** : Generator would start multi task to analysis mesh , every task processing number less than MAXIMUM_THREAD_PROCESSING_CAPACITY.The default is 128.

> The more thread does not meaning the faster processing speed, you can try to modify this parameter according to your pc performance.

Debug Setting:

- **(bool) Enable Debug Output** : If this toggle is checked, wireframe generator would output the spent time;

#### WireframeData:

**Vec3** : In order to implement multi thread supported , WireframeData provides a Vec3 struct similar with UnityEngine.Vector3.

**MathHelper** : MathHelper provides api to convert object between Vec3 and Vector3.


### Method A:

- Add WireframeGenerator.cs to gameobject with meshfilter;
- Use default parameters or tune the parameters manually;
- Click the '**Renew Wireframe**' button;
- Then the wireframe would be generated and set as selected gameobject's child.
- If generated wireframe is not statisfy your need,modify basic parameters and ignore parameters, click the '**Renew Wireframe**' button again.
- Click the '**Delete Exist Wireframe**' button to clear generated wireframe mesh.


### Method B:

![operation](methodb.png)

- Right click the object in hierarchy window, select SWirframe->Generate Wireframe. This method would usd the default parameters to generate wireframe.


### Issue:

- If mesh triangle count is very large, the wireframe generator would spend much time.
- If mesh is very very very complex, the effect of wireframe maybe not satisfactory.

If you have trouble about SWireframe please call me via email : springu3d@gmail.com.
Finally,I am so sorry about my English.