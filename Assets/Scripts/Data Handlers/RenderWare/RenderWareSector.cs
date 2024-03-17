using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TheWarriors
{
    public class RenderWareSector
    {
        /// 1 x TextureDictionary.
        /// ? x Atomic.

        private RenderWareSectorFile renderWareSectorFile;

        /// <summary>
        /// 2+ sections minimum. This class renders Atomic sections. It contains no texture data (has a dummy TextureDictionary), but contains mesh data in the Atomic Geometry Extension.
        /// </summary>
        public RenderWareSector(UInt32 uiFileHash_)
        {
            renderWareSectorFile = new RenderWareSectorFile(RockstarArchiveManager.GetWadArchiveFile(uiFileHash_));
        }

        public GameObject CreateSectorObject(Dictionary<UInt32, Vector3> sectorModelPositionList, bool bTransparent)
        {
            if (renderWareSectorFile == null)
            {
                return null;
            }

            GameObject sectorObject = new GameObject("sector_" + String.Format("{0:X8}", renderWareSectorFile.uiFileHash));

            foreach (KeyValuePair<UInt32, RenderWareSection> keyValuePair in renderWareSectorFile.renderWareStreamAtomicSections)
            {
                if (keyValuePair.Value is Atomic atomic)
                {
                    if (atomic.geometry != null)
                    {
                        MaterialList materialList = atomic.geometry.materialList;

                        GameObject atomicObject = new GameObject("atomic_" + keyValuePair.Key);

                        foreach (RenderWareSection atomicGeometryRenderWareSection in atomic.geometry.geometryExtension.extensionSectionList)
                        {
                            if (atomicGeometryRenderWareSection is NativeDataPlg nativeDataPlg)
                            {
                                for (Int32 iIterator = 0; iIterator < nativeDataPlg.nativeDataPlgStructure.materialSplits.Count; iIterator++)
                                {
                                    GameObject atomicMeshObject = new GameObject("atomic_" + keyValuePair.Key + "_mesh_" + iIterator);

                                    MeshFilter meshFilter = atomicMeshObject.AddComponent<MeshFilter>();
                                    MeshRenderer meshRenderer = atomicMeshObject.AddComponent<MeshRenderer>();

                                    NativeDataPlgStructure.MaterialSplit scaledMaterialSplit = nativeDataPlg.nativeDataPlgStructure.materialSplits[iIterator];

                                    // NOTE: Scale the vertex, UV and normal data.
                                    bool bAtomicScaleFound = false;

                                    foreach (RenderWareSection extension in atomic.atomicExtension.extensionSectionList)
                                    {
                                        if (extension is AtomicScale atomicScale)
                                        {
                                            RenderWareModel.ScaleMaterialSplit(ref scaledMaterialSplit, atomicScale.fVertexScale, atomicScale.fUVScale, atomicScale.fUnknownScale);

                                            bAtomicScaleFound = true;

                                            break;
                                        }
                                    }

                                    if (bAtomicScaleFound == false)
                                    {
                                        RenderWareModel.ScaleMaterialSplit(ref scaledMaterialSplit, -1, -1, -1);

                                        Debug.Log("Warning: Didn't find AtomicScale for sector!");
                                    }

                                    meshFilter.mesh = RenderWareModel.UnityModelFromMaterialSplit(scaledMaterialSplit);

                                    if (materialList.materialList[scaledMaterialSplit.iMaterialIndex].texture != null)
                                    {
                                        Texture2D texture = UnityTextureManager.GetTextureFromDictionary(materialList.materialList[scaledMaterialSplit.iMaterialIndex].texture.sDiffuseTextureName);

                                        if (texture == null)
                                        {
                                            Debug.Log("Warning: Failed to find texture \"" + materialList.materialList[scaledMaterialSplit.iMaterialIndex].texture.sDiffuseTextureName + "\"");
                                        }
                                        else
                                        {
                                            meshRenderer.material.mainTexture = texture;
                                            meshRenderer.material.mainTexture.name = materialList.materialList[scaledMaterialSplit.iMaterialIndex].texture.sDiffuseTextureName;
                                        }
                                    }

                                    if (scaledMaterialSplit.bHasUVData == true)
                                    {
                                        meshRenderer.material.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                                    }
                                    else
                                    {
                                        meshRenderer.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                                    }

                                    if (scaledMaterialSplit.bHasRGBAData == true)
                                    {
                                        Color[] colors = new Color[scaledMaterialSplit.RGBA.Length];

                                        for (Int32 iNestedIterator = 0; iNestedIterator < scaledMaterialSplit.RGBA.Length; iNestedIterator++)
                                        {
                                            colors[iNestedIterator].r = scaledMaterialSplit.RGBA[iNestedIterator][0];
                                            colors[iNestedIterator].g = scaledMaterialSplit.RGBA[iNestedIterator][1];
                                            colors[iNestedIterator].b = scaledMaterialSplit.RGBA[iNestedIterator][2];
                                            colors[iNestedIterator].a = scaledMaterialSplit.RGBA[iNestedIterator][3];
                                        }

                                        meshFilter.mesh.colors = colors;
                                    }

                                    //meshFilter.mesh.RecalculateNormals();

                                    atomicMeshObject.transform.SetParent(atomicObject.transform);
                                }
                            }
                        }

                        if (sectorModelPositionList.ContainsKey(keyValuePair.Key) == true)
                        {
                            atomicObject.transform.position = sectorModelPositionList[keyValuePair.Key];
                        }
                        
                        atomicObject.transform.parent = sectorObject.transform;
                    }
                }
            }

            // **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** 
            // NOTE: Export as OBJ
            Int32 iSubMeshCount = 0;

            foreach (Transform atomicObject in sectorObject.transform)
            {
                float parentX = atomicObject.transform.position.x;
                float parentY = atomicObject.transform.position.y;
                float parentZ = atomicObject.transform.position.z;

                //Debug.Log("parentX=" + parentX + ",parentY=" + parentY + ",parentZ=" + parentZ);

                foreach (Transform child in atomicObject.transform)
                {
                    string workpath = "C:\\Users\\justi\\Desktop\\objDump\\spookarama\\test\\";
                    string path = workpath + child.gameObject.name + "_" + iSubMeshCount + ".obj";
                    string mtlpath = mtlpath = workpath + child.gameObject.name + "_" + iSubMeshCount + ".mtl";

                    Mesh mesh = child.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    MeshRenderer meshRenderer = child.gameObject.GetComponent<MeshRenderer>();

                    StringBuilder sb = new StringBuilder();
                    StringBuilder sbmtl = new StringBuilder();

                    foreach (Vector3 v in mesh.vertices)
                    {
                        sb.Append(string.Format("v {0} {1} {2}\n", parentX + v.x, parentY + v.y, parentZ + v.z));
                    }

                    foreach (Vector2 v in mesh.uv)
                    {
                        sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
                    }

                    foreach (Vector3 v in mesh.normals)
                    {
                        sb.Append(string.Format("vn {0} {1} {2}\n", parentX + v.x, parentY + v.y, parentZ + v.z));
                    }

                    for (int material = 0; material < mesh.subMeshCount; material++)
                    {
                        sb.Append(string.Format("\ng {0}\n", child.gameObject.name + "_" + iSubMeshCount + ".obj"));
                        sb.Append(string.Format("mtllib {0}.mtl\n", child.gameObject.name + "_" + iSubMeshCount));
                        sb.Append(string.Format("usemtl Diffuse{0}\n\n", material));

                        int[] triangles = mesh.GetTriangles(material);

                        for (int i = 0; i < triangles.Length; i += 3)
                        {
                            sb.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
                        }

                        // NOTE: Create the material file
                        sbmtl.Append(string.Format("newmtl Diffuse{0}\n", material));

                        if (meshRenderer != null && meshRenderer.sharedMaterial.mainTexture != null)
                        {
                            string textureName = meshRenderer.sharedMaterial.mainTexture.name;

                            sbmtl.Append(string.Format("map_Kd {0}.png\n", textureName));

                            Texture2D texture = UnityTextureManager.GetTextureFromDictionary(textureName);

                            if (texture != null)
                            {
                                byte[] bytes = texture.EncodeToPNG();
                                File.WriteAllBytes(workpath + textureName + ".png", bytes);
                            }

                        }
                    }

                    StreamWriter writer = new StreamWriter(path);
                    writer.Write(sb.ToString());
                    writer.Close();

                    StreamWriter mtlwriter = new StreamWriter(mtlpath);
                    mtlwriter.Write(sbmtl.ToString());
                    mtlwriter.Close();

                    iSubMeshCount++;
                }
            }
            // **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** **** 

            sectorObject.transform.localScale = new Vector3(-1f, 1f, 1f);

            return sectorObject;
        }
    }
}