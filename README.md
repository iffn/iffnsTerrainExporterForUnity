# iffnsTerrainExporterForUnity

Adds an editor tool to convert your Unity terrain into a mesh to either add it to the scene or to export it as an OBJ file.
 
The tool can be found in the Menu bar in: Tools / iffnsStuff / TerrainToMeshConverter 
 
Supports
- Add as mesh to scene
- Export as .obj
- UV map generation
- Skipping points to reduce mesh size

Does not support
- Texutre maps
- Holes
- Automatic addition of the lost edge points which occur depending on the skip invervall
 
![image](https://user-images.githubusercontent.com/18383974/166724968-e2187588-aa1c-4bed-aa52-0d01111badd4.png)


Add this submodule with the following git command
```
git submodule add https://github.com/iffn/iffnsTerrainExporterForUnity.git Assets/iffnsStuff/iffnsTerrainExporterForUnity
```
