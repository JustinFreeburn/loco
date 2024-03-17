using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheWarriors
{
    public static class UnityTextureManager
    {
        private static Dictionary<String, Texture2D> textures = new Dictionary<String, Texture2D>();

        public static bool bGetTextureNameOnly = false;

        /// <summary>
        /// Dumps TXD from RenderWareSections for ElLoco
        /// </summary>
        public static void DumpTexturesFromRenderWareSections(RenderWareSection[] renderWareSections, UInt32 uiFileHash)
        {
            foreach (RenderWareSection renderWareSection in renderWareSections)
            {
                if (renderWareSection is TextureDictionary textureDictionary)
                {
                    string workpath = "C:\\Users\\justi\\Desktop\\";

                    using (BinaryWriter binWriter = new BinaryWriter(File.Open(workpath + String.Format("{0:X8}", uiFileHash) + ".txd", FileMode.Create)))
                    {
                        binWriter.Write(0x00000016);
                        binWriter.Write(textureDictionary.iSectionSize);
                        binWriter.Write(textureDictionary.iRenderWareVersion);
                        binWriter.Write(textureDictionary.test);
                    }
                }
            }
        }

        /// <summary>
        /// Stores Texture2D objects from a RenderWareStream (TextureDictionary) using the RenderWareTexture class
        /// </summary>
        public static void LoadTexturesFromRenderWareSections(RenderWareSection[] renderWareSections)
        {
            foreach (RenderWareSection renderWareSection in renderWareSections)
            {
                if (renderWareSection is TextureDictionary textureDictionary)
                {
                    foreach (TextureNative textureNative in textureDictionary.textureNativeList)
                    {
                        if (textures.ContainsKey(textureNative.textureNativeStructure.sTextureName) == false)
                        {
                            textures.Add(textureNative.textureNativeStructure.sTextureName, RenderWareTexture.LoadTextureFromRenderWareTextureNative(textureNative));
                        }
                        else
                        {
                            //Debug.Log("Warning: Duplicate texture found. Skipped loading \"" + textureNative.textureNativeStructure.sTextureName + "\".");
                        }
                    }
                }
            }
        }

        public static String GetTextureNameFromFileNameHash(UInt32 uiFileHash_)
        {
            bGetTextureNameOnly = true;

            RenderWareStream renderWareStream = new RenderWareStream(new RenderWareStreamFile(RockstarArchiveManager.GetWadArchiveFile(uiFileHash_)));

            bGetTextureNameOnly = false;

            foreach (RenderWareSection renderWareSection in renderWareStream.RenderWareStreamSections)
            {
                if (renderWareSection is TextureDictionary textureDictionary)
                {
                    if (textureDictionary.textureNativeList.Count > 1)
                    {
                        Debug.Log("Warning: More than 1 texture while using UnityTextureManager.GetTextureNameFromTXD(). Returning first texture name.");
                    }

                    return textureDictionary.textureNativeList[0].textureNativeStructure.sTextureName;
                }
            }

            return "";
        }

        public static Texture2D GetTextureFromDictionary(String textureName)
        {
            if (textures.ContainsKey(textureName) == true)
            {
                return textures[textureName];
            }

            // TODO: Return a default texture to identify failure...?

            return null;
        }

        public static void DisposeTextures()
        {
            foreach (Texture2D texture in textures.Values)
            {
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }
        }
    }
}