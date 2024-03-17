using System;
using System.Collections.Generic;

namespace TheWarriors
{
    public class TextureDictionary : RenderWareSection
    {
        public TextureDictionaryStructure textureDictionaryStruct;

        public List<TextureNative> textureNativeList;

        public Extension textureDictionaryExtension;

        public byte[] test;

        public TextureDictionary Read(ArchiveFileBinaryReader reader)
        {
            SectionID = RenderWareSectionID.TextureDictionary;
            iSectionSize = reader.ReadInt32();
            iRenderWareVersion = reader.ReadInt32();

            // NOTE: Dump as TXD for ElLoco
            long lELStartSectionPosition = reader.Position();

            test = new byte[iSectionSize - 12];
            test = reader.ReadBytes(iSectionSize - 12);

            reader.SeekBeginning(lELStartSectionPosition);

            // NOTE: TextureDictionaryStructure
            {
                if ((RenderWareSectionID)reader.ReadInt32() != RenderWareSectionID.Struct)
                {
                    throw new Exception("*** Error: Expected TextureDictionaryStructure at position " + (reader.Position() - 4));
                }

                textureDictionaryStruct = new TextureDictionaryStructure().Read(reader);
            }

            // NOTE: TextureNative
            {
                textureNativeList = new List<TextureNative>();

                for (Int32 iIterator = 0; iIterator < textureDictionaryStruct.iTextureCount; iIterator++)
                {
                    if ((RenderWareSectionID)reader.ReadInt32() != RenderWareSectionID.TextureNative)
                    {
                        throw new Exception("*** Error: Expected TextureNative at position " + (reader.Position() - 4));
                    }

                    textureNativeList.Add(new TextureNative().Read(reader));
                }
            }

            // NOTE: Extension
            {
                long lStartSectionPosition = reader.Position();

                if ((RenderWareSectionID)reader.ReadInt32() == RenderWareSectionID.Extension)
                {
                    textureDictionaryExtension = new Extension().Read(reader);
                }
                else
                {
                    reader.SeekBeginning(lStartSectionPosition);
                }
            }

            return this;
        }
    }
}
