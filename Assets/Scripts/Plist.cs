using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class Plist
{
    public string plistName { get; private set; }
    public string textureName { get; private set; }
    public string path { get; private set; }
    public string sourcePath { get; private set; }
    public Dictionary<string, Frame> frames { get; private set; }
   
    //解析Plist(Xml)文件
    public static Plist Xml(string sourcePath)
    {
        FileInfo info = new FileInfo(sourcePath);
        if (info.Exists && info.Name.EndsWith(".plist") ) 
        {
            Plist ret = new Plist();
            ret.path = Path.GetDirectoryName(sourcePath).Replace("\\","/");
            ret.sourcePath = sourcePath.Replace("\\", "/");
            ret.plistName = info.Name;
            ret.textureName = info.Name.Replace(".plist", ".png");
            ret.frames = new Dictionary<string, Frame>();
            //解析Xml
            XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load(sourcePath);
            xmlDoc.LoadXml(File.ReadAllText(sourcePath));
            XmlNodeList node = xmlDoc.SelectSingleNode("plist").ChildNodes;
            foreach (XmlElement list in node)
            {
                var plist = list.ChildNodes;
                if (plist.Count % 2 == 0)
                {
                    for (int i = 0; i < plist.Count; i += 2)
                    {
                        if (plist[i].Name == "key" && plist[i + 1].Name == "dict"
                            && plist[i].InnerText == "frames" && plist[i + 1].ChildNodes.Count % 2 == 0)
                        {
                            var frameNodes = plist[i + 1].ChildNodes;
                            for (int j = 0; j < frameNodes.Count; j += 2)
                            {
                                if (frameNodes[j].Name == "key" && frameNodes[j + 1].Name == "dict"
                                    && frameNodes[j].InnerText.EndsWith(".png"))
                                {
                                    try
                                    {
                                        var name = frameNodes[j].InnerText;
                                        var frame = Frame.From(frameNodes[j + 1]);
                                        ret.frames.Add(name, frame);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError(sourcePath + "\n" + e.Message);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }
        return null;
    }

    public class Frame
    {
        public F1 frame;
        public F2 offset;
        public bool rotated = false;
        public F1 sourceColorRect;
        public F2 sourceSize;

        static int[] CastIntArray(string source)
        {
            string[] str = source.Replace("{", "").Replace("}", "").Split(',');
            int[] ret = new int[str.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = int.Parse(str[i]);
            return ret;
        }
        public static Frame From(XmlNode xe)
        {
            if (xe.ChildNodes.Count % 2 == 0)
            {
                //解析Xml节点
                var ret = new Frame();
                for (int i = 0; i < xe.ChildNodes.Count; i += 2)
                {
                    if (xe.ChildNodes[i].Name == "key")
                    {
                        switch (xe.ChildNodes[i].InnerText)
                        {
                            case "frame":
                                var frame = CastIntArray(xe.ChildNodes[i + 1].InnerText);
                                ret.frame.one.x = frame[0];
                                ret.frame.one.y = frame[1];
                                ret.frame.two.x = frame[2];
                                ret.frame.two.y = frame[3];
                                break;
                            case "offset":
                                var offset = CastIntArray(xe.ChildNodes[i + 1].InnerText);
                                ret.offset.x = offset[0];
                                ret.offset.y = offset[1];
                                break;
                            case "rotated":
                                ret.rotated = xe.ChildNodes[i + 1].Name.IndexOf("true") >= 0;
                                break;
                            case "sourceColorRect":
                                var sourceColorRect = CastIntArray(xe.ChildNodes[i + 1].InnerText);
                                ret.sourceColorRect.one.x = sourceColorRect[0];
                                ret.sourceColorRect.one.y = sourceColorRect[1];
                                ret.sourceColorRect.two.x = sourceColorRect[2];
                                ret.sourceColorRect.two.y = sourceColorRect[3];
                                break;
                            case "sourceSize":
                                var sourceSize = CastIntArray(xe.ChildNodes[i + 1].InnerText);
                                ret.sourceSize.x = sourceSize[0];
                                ret.sourceSize.y = sourceSize[1];
                                break;
                        }
                    }
                }
                return ret;
            }
            return null;
        }
    }
    public struct F1
    {
        public F2 one;
        public F2 two;
    }
    public struct F2
    {
        public int x;
        public int y;
    }
}
