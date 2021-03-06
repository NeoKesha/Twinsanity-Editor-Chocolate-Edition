﻿using Newtonsoft.Json;
using Assimp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using TT_Lab.AssetData.Graphics.SubModels;
using Twinsanity.TwinsanityInterchange.Implementations.PS2.Items.Graphics;
using Twinsanity.TwinsanityInterchange.Common;

namespace TT_Lab.AssetData.Graphics
{
    public class ModelData : AbstractAssetData
    {
        public ModelData()
        {
            Vertexes = new List<List<Vertex>>();
            Faces = new List<List<IndexedFace>>();
        }

        public ModelData(PS2AnyModel model) : this()
        {
            twinRef = model;
        }
        [JsonProperty(Required = Required.Always)]
        public List<List<Vertex>> Vertexes { get; set; }
        [JsonProperty(Required = Required.Always)]
        public List<List<IndexedFace>> Faces { get; set; }
        protected override void Dispose(Boolean disposing)
        {
            Vertexes.ForEach(vl => vl.Clear());
            Vertexes.Clear();
            Faces.ForEach(fl => fl.Clear());
            Faces.Clear();
        }

        public override void Save(string dataPath, JsonSerializerSettings? settings = null)
        {
            try
            {
                Scene scene = new Scene
                {
                    RootNode = new Node("Root")
                };

                for (var i = 0; i < Vertexes.Count; ++i)
                {
                    var submodel = Vertexes[i];
                    var faces = Faces[i];
                    Mesh mesh = new Mesh(PrimitiveType.Triangle);
                    foreach (var ver in submodel)
                    {
                        mesh.Vertices.Add(new Vector3D(ver.Position.X, ver.Position.Y, ver.Position.Z));
                        mesh.TextureCoordinateChannels[0].Add(new Vector3D(ver.UV.X, ver.UV.Y, 1.0f));
                        mesh.VertexColorChannels[0].Add(new Color4D(ver.Color.X, ver.Color.Y, ver.Color.Z, ver.Color.W));
                    }
                    foreach (var face in faces)
                    {
                        mesh.Faces.Add(new Face(new int[] { face.Indexes[0], face.Indexes[1], face.Indexes[2] }));
                    }
                    mesh.MaterialIndex = 0;
                    scene.Meshes.Add(mesh);
                    scene.RootNode.MeshIndices.Add(i);
                }

                Material mat = new Material
                {
                    Name = "Default"
                };
                scene.Materials.Add(mat);

                AssimpContext context = new AssimpContext();
                context.ExportFile(scene, dataPath, "collada");
                context.Dispose();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error exporting for Model {twinRef.GetID()}: {ex.Message} Stack: \n{ex.StackTrace}");
            }
        }

        public override void Load(String dataPath, JsonSerializerSettings? settings = null)
        {
            Vertexes.Clear();
            Faces.Clear();
            AssimpContext context = new AssimpContext();
            var scene = context.ImportFile(dataPath);
            foreach (var mesh in scene.Meshes)
            {
                var submodel = new List<Vertex>();
                for (var i = 0; i < mesh.VertexCount; ++i)
                {
                    submodel.Add(new Vertex(
                        new Vector4(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, 0.0f),
                        new Vector4(mesh.VertexColorChannels[0][i].R, mesh.VertexColorChannels[0][i].G, mesh.VertexColorChannels[0][i].B, mesh.VertexColorChannels[0][i].A),
                        new Vector4(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y, 1.0f, 0.0f)
                        ));
                }

                var faces = new List<IndexedFace>();
                for (var i = 0; i < mesh.FaceCount; ++i)
                {
                    faces.Add(new IndexedFace(mesh.Faces[i].Indices.ToArray()));
                }
                Vertexes.Add(submodel);
                Faces.Add(faces);
            }
            context.Dispose();
        }

        public override void Import()
        {
            PS2AnyModel model = (PS2AnyModel)twinRef;
            Vertexes = new List<List<Vertex>>();
            Faces = new List<List<IndexedFace>>();
            var refIndex = 0;
            var offset = 0;
            foreach (var e in model.SubModels)
            {
                var vertList = new List<Vertex>();
                var faceList = new List<IndexedFace>();
                refIndex = 0;
                e.CalculateData();
                for (var j = 0; j < e.Vertexes.Count; ++j)
                {
                    if (j < e.Vertexes.Count - 2)
                    {
                        if (e.Connection[j + 2])
                        {
                            if ((/*offset +*/ j) % 2 == 0)
                            {
                                faceList.Add(new IndexedFace(new int[] { refIndex, refIndex + 1, refIndex + 2 }));
                            }
                            else
                            {
                                faceList.Add(new IndexedFace(new int[] { refIndex + 1, refIndex, refIndex + 2 }));
                            }
                        }
                        ++refIndex;
                    }
                    var ver = new Vertex(e.Vertexes[j], new Vector4(1.0f, 1.0f, 1.0f, 1.0f), e.UVW[j], e.EmitColor[j])
                    {
                        Normal = new Vector4(e.Normals[j].X, e.Normals[j].Y, e.Normals[j].Z, e.Normals[j].W)
                    };
                    vertList.Add(ver);
                }
                Vertexes.Add(vertList);
                Faces.Add(faceList);
                //offset += e.Vertexes.Count;
                refIndex += 2;
            }
        }
    }
}
