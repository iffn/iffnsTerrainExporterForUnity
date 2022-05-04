# iffnsTerrainExporterForUnity

Adds an editor tool to convert your Unity terrain into a mesh and either adds it to the scene or exports it as an OBJ file.
 
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
 
![image](https://user-images.githubusercontent.com/18383974/166719939-b091aded-0595-4c29-93ba-113cf76c7ff8.png)


Add this submodule with the following git command
```
git submodule add https://github.com/iffn/iffnsTerrainExporterForUnity.git Assets/iffnsStuff/iffnsTerrainExporterForUnity
```
