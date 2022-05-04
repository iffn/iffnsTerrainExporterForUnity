# if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace iffnsStuff.iffnsUnityTools.Exporters
{
    public class iffnsTerrainExporter : EditorWindow
    {
        Terrain linkedTerrain;
        
        string FileNameWithoutEnding;
        int Skips;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int vertexCount = -1;
        int triangleCount = -1;

        [MenuItem("Tools/iffnsStuff/TerrainToMeshConverter")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(iffnsTerrainExporter));
        }

        void OnGUI()
        {
            if (Skips < 0) Skips = 0;

            GUILayout.Label(text: "Simple Terrain exporter by iffn", style: EditorStyles.boldLabel);


            linkedTerrain = EditorGUILayout.ObjectField(
               obj: linkedTerrain,
               objType: typeof(Terrain),
               true) as Terrain;

            Skips = EditorGUILayout.IntField(label: "Skips", value: Skips);
            EditorGUILayout.HelpBox(message: "Skipping allows to reduce the ammount of verticies of the mesh by only using every nth line. Some points may be lost at the edge.", type: MessageType.None);

            if(linkedTerrain != null)
            {
                TerrainData terrainData = linkedTerrain.terrainData;

                int heightMapSize = terrainData.heightmapResolution;

                int lostEdgePoints = (heightMapSize - 1) % (Skips + 1);

                GUILayout.Label("Original terrain size = " + heightMapSize);
                GUILayout.Label("Lost point width at edge = " + lostEdgePoints);

                if (GUILayout.Button("Update info"))
                {
                    GenerateData();
                    UpdateCount();
                }

                string infoText = "Click Update info to update data" + System.Environment.NewLine;
                infoText += "Verticies = " + ConvertIntToString(vertexCount) + System.Environment.NewLine;
                infoText += "Triangles = " + ConvertIntToString(triangleCount) + System.Environment.NewLine;

                EditorGUILayout.HelpBox(infoText, MessageType.Info);

                if (GUILayout.Button("Add mesh to scene"))
                {
                    GenerateData();
                    AddMeshToScene();
                    UpdateCount();
                }

                if (GUILayout.Button("Export as mesh"))
                {
                    GenerateData();
                    ExportAsMesh();
                    UpdateCount();
                }

                EditorGUILayout.HelpBox(message: "Exported meshes are added to the Streaming asset folder", type: MessageType.None);
            }
        }

        void UpdateCount()
        {
            vertexCount = vertices.Count;
            triangleCount = triangles.Count;
        }

        void GenerateData()
        {
            //Prepare info
            TerrainData terrainData;
            terrainData = linkedTerrain.terrainData;
            int heightMapSize = terrainData.heightmapResolution;

            Vector2 gridSize = new Vector2(terrainData.size.x, terrainData.size.z) * (1f / (heightMapSize - 1));

            vertices = new List<Vector3>();
            uvs = new List<Vector2>();
            triangles = new List<int>();

            if(Skips < 0) Skips = 0;

            //Generate mesh data
            for (int x = 0; x < heightMapSize; x += Skips + 1)
            {
                for (int y = 0; y < heightMapSize; y += Skips + 1)
                {
                    float height = terrainData.GetHeight(y, x);

                    Vector3 vector = new Vector3(y * gridSize.y, height, x * gridSize.x);

                    vertices.Add(vector);

                    uvs.Add(new Vector2(vector.x, vector.z));
                }
            }

            int outputVerticesCount = (int)Mathf.Sqrt(vertices.Count);

            for (int x = 0; x < outputVerticesCount - 1; x++)
            {
                for (int y = 0; y < outputVerticesCount - 1; y++)
                {
                    int A = outputVerticesCount * x + y;
                    int B = outputVerticesCount * x + y + 1;
                    int C = outputVerticesCount * (x + 1) + y + 1;
                    int D = outputVerticesCount * (x + 1) + y;

                    triangles.Add(A);
                    triangles.Add(C);
                    triangles.Add(B);
                    triangles.Add(A);
                    triangles.Add(D);
                    triangles.Add(C);
                }
            }
        }

        void ExportAsMesh()
        {
            //Export
            string meshName = "MeshOf" + linkedTerrain.name;

            List<string> exportText = GetObjLines(meshName: meshName, vertices: vertices, uvs: uvs, triangles: triangles, upDirection: UpDirection.Y);

            string outputText = string.Join(separator: System.Environment.NewLine, values: exportText); //Very fast, takes about 2ms for 13k lines

            string completeFileLocation = Path.Combine(Application.streamingAssetsPath, meshName + ".obj");

            int addition = 1;

            while (true)
            {
                if (!File.Exists(completeFileLocation)) break;

                completeFileLocation = Path.Combine(Application.streamingAssetsPath, meshName + addition + ".obj");

                addition++;
            }

            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            File.WriteAllText(completeFileLocation, outputText);
        }

        void AddMeshToScene()
        {
            GameObject terrainMesh = new GameObject();

            terrainMesh.name = "Mesh of " + linkedTerrain.name;

            terrainMesh.transform.position = linkedTerrain.transform.position;
            terrainMesh.transform.rotation = linkedTerrain.transform.rotation;

            MeshRenderer renderer = terrainMesh.AddComponent<MeshRenderer>();
            

            MeshFilter MeshFilter = terrainMesh.AddComponent<MeshFilter>();

            Mesh mesh;
            if (MeshFilter.sharedMesh == null)
            {
                mesh = new Mesh();
                MeshFilter.sharedMesh = mesh;
            }
            else
            {
                mesh = MeshFilter.sharedMesh;
                Debug.Log("Mesh existed");
            }

            if (vertices.Count > 65535)
            {
                //Avoid vertex limit
                //https://answers.unity.com/questions/471639/mesh-with-more-than-65000-vertices.html
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            renderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");

            //Apply to display mesh
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }


        public enum UpDirection
        {
            Y,
            Z
        }

        List<string> GetObjLines(string meshName, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, UpDirection upDirection)
        {
            List<string> returnList = new List<string>();

            returnList.Add("o " + meshName);

            //Vertices
            switch (upDirection)
            {
                case UpDirection.Y:
                    foreach (Vector3 vertex in vertices)
                    {
                        returnList.Add("v " + -vertex.x + " " + vertex.y + " " + vertex.z);
                    }
                    break;
                case UpDirection.Z:
                    foreach (Vector3 vertex in vertices)
                    {
                        returnList.Add("v " + -vertex.x + " " + vertex.z + " " + vertex.y);
                    }
                    break;
                default:
                    break;
            }

            //UVs
            foreach (Vector2 uv in uvs)
            {
                returnList.Add("vt " + uv.x + " " + uv.y + " ");
            }

            //Faces
            if (triangles.Count != 0)
            {
                for (int i = 0; i < triangles.Count - 1; i += 3)
                {
                    //First index in OBJ is 1 for some reason
                    int t1 = triangles[i] + 1;
                    int t2 = triangles[i + 1] + 1;
                    int t3 = triangles[i + 2] + 1;

                    returnList.Add("f " + t1 + "/" + t1 + " " + t3 + "/" + t3 + " " + t2 + "/" + t2);
                }
            }

            return returnList;
        }

        string ConvertIntToString(int value)
        {
            if (value < 10000)
            {
                return "" + value;
            }

            string returnValue = string.Format("{0:n0}", value);

            returnValue = returnValue.Replace(",", "'");

            return returnValue;
        }
    }
}
#endif