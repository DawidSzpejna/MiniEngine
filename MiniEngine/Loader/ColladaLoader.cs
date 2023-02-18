using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OpenTK.Mathematics;
using System.Xml.Serialization;
using MiniEngine.Graphics;

namespace MiniEngine.Loader
{
    public class Mesh
    {
        public Mesh(float[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
            Model = Matrix4.Identity;
        }

        public Mesh(float[] vertices, int[] indices, Matrix4 model)
        {
            Vertices = vertices;
            Indices = indices;
            Model = model;
        }

        public void AddTexture(string path) 
            => MeshTextures = new Texture(path);

        public float[] Vertices { get; set; }
        public int[] Indices { get; set; }
        public Matrix4 Model { get; set; }
        public Texture MeshTextures { get; set; }
    }

    public static class ColladaLoad
    {
        #region Properties
        private static readonly System.Globalization.CultureInfo USculture = new System.Globalization.CultureInfo("en-US");
        #endregion


        #region Loader Functions
        public static List<Mesh> Load(string name, string directory)
        {
            string path = directory + '/' + name;
            Collada141.COLLADA collada = DeserializeObject(path);
            return ParseObjectToMeshes(collada, directory);
        }
        #endregion


        #region Helpful Functions
        private static Collada141.COLLADA DeserializeObject(string path)
        {
            Collada141.COLLADA tmp;

            XmlSerializer serializer = new XmlSerializer(typeof(Collada141.COLLADA));
            try
            {
                using (Stream reader = new FileStream(path, FileMode.Open))
                {
                    tmp = (Collada141.COLLADA)serializer.Deserialize(reader);
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Loader cannot find file: {path}");
                Console.WriteLine(e.Message);
                return null;
            }

            return tmp;
        }

        private static List<Mesh> ParseObjectToMeshes(Collada141.COLLADA collada, string directory)
        {
            Dictionary<string, Mesh> meshesDictionary = new Dictionary<string, Mesh>();
            Dictionary<string, string> texturesBuffer = null;

            foreach (var elem in collada.Items)
            {
                var typeOfElement = elem.GetType();

                // recognising "library_*" in collada
                if (typeOfElement.Equals(typeof(Collada141.library_effects)))
                {
                    // deserialization to dictionary of {image_id, effect_id}
                    var effectElements = elem as Collada141.library_effects;
                    texturesBuffer = ParseEffects(effectElements?.effect);
                }
                else if (typeOfElement.Equals(typeof(Collada141.library_images)))
                {
                    // exchange {image_id, effect_id} -(to)-> {effect_id, image_path}
                    var imageElements = elem as Collada141.library_images;
                    ParseImages(texturesBuffer, imageElements?.image);
                }
                else if (typeOfElement.Equals(typeof(Collada141.library_materials)))
                {
                    // exchange {effect_id, image_path} -(to)-> {material_id, image_path}
                    var materialElements = elem as Collada141.library_materials;
                    ParseMaterials(texturesBuffer, materialElements?.material);

                }
                else if (typeOfElement.Equals(typeof(Collada141.library_geometries)))
                {
                    var geometryElements = elem as Collada141.library_geometries;

                    foreach (Collada141.geometry geometry in geometryElements.geometry)
                    {
                        // we can parse only meshes in this parser
                        Collada141.mesh meshElement = geometry.Item as Collada141.mesh;
                        if (meshElement != null)
                        {
                            var mesh = ParseMeshes(meshElement, geometry.id);
                            meshesDictionary.Add($"#{geometry.id}", mesh);
                        }
                    }
                }
                else if (typeOfElement.Equals(typeof(Collada141.library_visual_scenes)))
                {
                    var visual_scenes = ((Collada141.library_visual_scenes)elem).visual_scene;
                    foreach (var scene in visual_scenes)
                    {
                        ParseNodes(meshesDictionary, texturesBuffer, scene, directory);
                    }
                }
            }
            return meshesDictionary.Values.ToList();
        }

        private static Dictionary<string, string> ParseEffects(Collada141.effect[] effects)
        {
            if (effects == null) return null;
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var eff in effects)
            {
                if (eff?.Items?.Length != 0)
                {
                    Collada141.effectFx_profile_abstractProfile_COMMON elem = eff.Items[0];
                    if (elem?.Items?.Length != 0)
                    {
                        Collada141.common_newparam_type newparam_t = elem.Items?[0] as Collada141.common_newparam_type;
                        if (newparam_t != null)
                        {
                            Collada141.fx_surface_common surf = newparam_t?.Item as Collada141.fx_surface_common;
                            if (surf != null)
                            {
                                Collada141.fx_surface_init_from_common init_from = surf.init_from[0];
                                result.Add(init_from.Value, eff.id);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static Dictionary<string, string> ParseImages(Dictionary<string, string> texBuff, Collada141.image[] images)
        {
            if (images == null || texBuff == null) return null;

            foreach (var img in images)
            {
                if (texBuff.ContainsKey(img.id))
                {
                    var init_from = img.Item as string;
                    if (init_from != null)
                    {
                        string effect_id = texBuff[img.id];
                        texBuff.Remove(img.id);

                        texBuff.Add('#' + effect_id, init_from);
                    }
                }
            }
            return texBuff;
        }

        private static Dictionary<string, string> ParseMaterials(Dictionary<string, string> texBuff, Collada141.material[] materials)
        {
            if (texBuff == null || materials == null) return null;

            foreach (var mtr in materials)
            {
                string url = mtr?.instance_effect?.url;
                if (mtr.instance_effect != null && texBuff.ContainsKey(url))
                {
                    string image_path = texBuff[url];

                    texBuff.Remove(url);
                    texBuff.Add('#' + mtr.id, image_path);
                }
            }

            return texBuff;
        }

        private static Mesh ParseMeshes(Collada141.mesh mesh, string id)
        {
            float[] vers = null;
            float[] norms = null;
            float[] texCoords = null;

            Regex regPositions = new Regex($"{id}-positions");
            Regex regNormals = new Regex($"{id}-normals");
            Regex regTexCoords = new Regex($"{id}-map-0");


            // parsing arrays of vertex's position and normal
            foreach (var elem in mesh.source)
            {
                if (regPositions.IsMatch(elem.id))
                {
                    var array = elem.Item as Collada141.float_array;
                    ulong count = array.count;

                    vers = new float[count];
                    for (ulong i = 0; i < count; i++)
                    {
                        vers[i] = (float)array.Values[i];
                    }
                }
                else if (regNormals.IsMatch(elem.id))
                {
                    var array = elem.Item as Collada141.float_array;
                    ulong count = array.count;

                    norms = new float[count];
                    for (ulong i = 0; i < count; i++)
                    {
                        norms[i] = (float)array.Values[i];
                    }
                }
                //////// ---------------------------
                else if (regTexCoords.IsMatch(elem.id))
                {
                    var array = elem.Item as Collada141.float_array;
                    ulong count = array.count;

                    texCoords = new float[count];
                    for (ulong i = 0; i < count; i++)
                    {
                        texCoords[i] = (float)array.Values[i];
                    }
                }
                /// -------------------------------
            }

            List<float> vertices = new List<float>();
            List<int> indices = new List<int>();

            // creating array with vertex's position and normal
            foreach (var elem in mesh.Items)
            {
                Collada141.triangles triangles = elem as Collada141.triangles;
                if (triangles != null)
                {
                    ulong elements = (ulong)triangles.input.Length;
                    string faces = triangles.p;
                    string[] info = faces.Split(' ');
                    int counter = 0;

                    for (ulong i = 0; i < triangles.count * 3; i++)
                    {
                        int verIdx = int.Parse(info[i * elements + 0]);
                        vertices.Add(vers[verIdx * 3 + 0]);
                        vertices.Add(vers[verIdx * 3 + 1]);
                        vertices.Add(vers[verIdx * 3 + 2]);

                        int normIdx = int.Parse(info[i * elements + 1]);
                        vertices.Add(norms[normIdx * 3 + 0]);
                        vertices.Add(norms[normIdx * 3 + 1]);
                        vertices.Add(norms[normIdx * 3 + 2]);

                        int texCoordIdx = int.Parse(info[i * elements + 2]);
                        vertices.Add(texCoords[texCoordIdx * 2 + 0]);
                        vertices.Add(texCoords[texCoordIdx * 2 + 1]);
                        indices.Add(counter++);
                    }

                    break;
                }
            }

            return new Mesh(vertices.ToArray(), indices.ToArray());
        }

        private static void ParseNodes(
            Dictionary<string, Mesh> meshesDictionary,
            Dictionary<string, string> texBuff,
            Collada141.visual_scene scene,
            string directory
            )
        {
            foreach (var nd in scene.node)
            {
                string key = nd.instance_geometry?[0].url;
                if (key == null || !meshesDictionary.ContainsKey(key)) continue;

                Mesh mesh = meshesDictionary[key];

                // Parse the model matrix
                Matrix4 model = new Matrix4();
                foreach (var itm in nd.Items)
                {
                    Collada141.matrix mtx = itm as Collada141.matrix;

                    // we can parse only matrices
                    if (mtx != null)
                    {
                        string[] tmp = mtx._Text_.Split(" ");
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            model[i % 4, i / 4] = float.Parse(tmp[i], USculture);
                        }
                    }
                }
                mesh.Model *= model;

                // Parse the material of mesh
                if (texBuff != null && nd?.instance_geometry?.Length != 0)
                {
                    var inst_geo = nd.instance_geometry[0];
                    if (inst_geo.bind_material != null)
                    {
                        var tech_common = inst_geo.bind_material.technique_common;
                        if (tech_common != null && tech_common.Length != 0)
                        {
                            var instance_material = tech_common[0];
                            string material_id = instance_material.target;
                            if (texBuff.ContainsKey(material_id))
                            {
                                mesh.AddTexture(directory + '/' + texBuff[material_id]);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
