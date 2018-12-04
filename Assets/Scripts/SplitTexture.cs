using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class SplitTexture
{
    static string path = null;
    static bool childDir;
    static string outPath = null;
    static bool clearOut;
    static bool useFileName;
    static bool backup;
    static bool rotated;
    static List<Plist> plists = new List<Plist>();

    public static string state { get; private set; }
    public static int number { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">源路径(文件夹或文件)</param>
    /// <param name="childDir">扫描子文件夹</param>
    /// <param name="outPath">输出文件夹(null默认为源文件同路径)</param>
    /// <param name="clearOut">清除同名文件夹</param>
    /// <param name="useFileName">使用文件名创建文件夹</param>
    /// <param name="backup">备份同名文件</param>
    /// <param name="rotated">自动旋转图片</param>
    /// <returns></returns>
    public static bool Do(string path, bool childDir, string outPath, bool clearOut, bool useFileName, bool backup, bool rotated)
    {
        if (File.Exists(path) || Directory.Exists(path))
        {
            number = 0;
            SplitTexture.outPath = outPath;
            SplitTexture.childDir = childDir;
            SplitTexture.path = path;
            SplitTexture.clearOut = outPath != null ? false : clearOut;
            SplitTexture.useFileName = useFileName;
            SplitTexture.backup = backup;
            SplitTexture.rotated = rotated;
            //异步执行方法(Unity的方法必须在主线程内调用)
            Loom.Instance();
            Loom.InvokeAsync(ThreadDelege);
            return true;
        }
        return false;
    }

    //线程逻辑
    static void ThreadDelege()
    {
        plists.Clear();
        allFiles.Clear();
        //单文件
        if (File.Exists(path))
        {
            if (path.EndsWith(".png"))
            {
                path = path.Replace(".png", ".plist");
            }
            var plist = Plist.Xml(path);
            if (plist != null)
            {
                plists.Add(plist);
            }
        }
        //遍历文件夹
        else if (Directory.Exists(path))
        {
            //递归遍历文件夹,找到所有plist文件
            GetAllFiles(new DirectoryInfo(path));
            var total = allFiles.Count;
            for (int i = 0; i < total; i++)
            {
                state = string.Format("解析({0}/{1}): ", i + 1, total) + allFiles[i];
                var plist = Plist.Xml(allFiles[i]);
                if (plist != null)
                {
                    plists.Add(plist);
                }
            }
        }
        //开始切割图片
        foreach (var plist in plists)
        {
            string _outPath = (outPath != null ? outPath : plist.path) + "/";
            if (useFileName)
            {
                _outPath += plist.textureName.Replace(".png", "") + "/";
            }
            Work(plist, _outPath);
        }
    }

    //遍历文件夹找出所有plist文件
    static List<string> allFiles = new List<string>();
    static void GetAllFiles(DirectoryInfo dir)
    {
        foreach (var fileInfo in dir.GetFiles())
        {
            if (fileInfo.Name.EndsWith(".plist"))
            {
                state = "扫描文件: " + fileInfo.FullName;
                allFiles.Add(fileInfo.FullName);
            }
        }
        foreach (var dirInfo in dir.GetDirectories())
        {
            if (childDir)
            {
                state = "扫描文件夹: " + dirInfo.FullName;
                GetAllFiles(dirInfo);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plist">配置文件</param>
    /// <param name="outPath">输出路径</param>
    /// <param name="clearOut">清空输出文件夹</param>
    /// <param name="backup">备份同名文件,否则删除</param>
    /// <param name="rotated">自动旋转图片</param>
    /// <param name="now">进度now</param>
    /// <param name="total">进度total</param>
    static void Work(Plist plist, string outPath)
    {
        string imgPath = plist.path + "/" + plist.textureName;
        //判断文件是否存在
        if (File.Exists(imgPath))
        {
            if (clearOut && Directory.Exists(outPath))
            {
                Directory.Delete(outPath, true);
            }
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            Loom.InvokeMain(() =>
            {
                //读取源文件Texture2D
                Texture2D source = GetTexrture2DFromPath(imgPath);
                Loom.InvokeAsync(() =>
                {
                    state = "准备: " + imgPath;
                    //创建单独Texture2D
                    foreach (var frameInfo in plist.frames)
                    {
                        try
                        {
                            var frame = frameInfo.Value;
                            var frameWidth = frame.frame.two.x;
                            var frameHeight = frame.frame.two.y;
                            //图片是否被旋转(如果旋转,则交换frameWidth与frameHeight的值)
                            if (frame.rotated)
                            {
                                var temp = frameWidth;
                                frameWidth = frameHeight;
                                frameHeight = temp;
                            }
                            Loom.InvokeMain(() =>
                            {
                                //计算图片Rect起点位置(plist默认左上,Unity默认左下)
                                var rectX = frame.frame.one.x;
                                var rectY = source.height - frame.frame.one.y - frameHeight;
                                //从source中取得数值
                                Texture2D t2d = new Texture2D(frameWidth, frameHeight, source.format, false);
                                t2d.SetPixels(source.GetPixels(rectX, rectY, frameWidth, frameHeight));
                                t2d.Apply();
                                var bytes = t2d.EncodeToPNG();
                                Loom.InvokeAsync(() =>
                                {
                                    //文件路径
                                    string outFile = outPath + frameInfo.Key;
                                    if (!Directory.Exists(Path.GetDirectoryName(outFile)))
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(outFile));
                                    }
                                    //是否已经存在文件
                                    if (File.Exists(outFile))
                                    {
                                        if (backup)
                                        {
                                            int i = 0;
                                            while (true)
                                            {
                                                var move = outPath + frameInfo.Key.Replace(".png", string.Format("_{0}.png", i));
                                                if (!File.Exists(move))
                                                {
                                                    File.Move(outFile, move);
                                                    break;
                                                }
                                                i++;
                                            }
                                        }
                                        else
                                        {
                                            File.Delete(outFile);
                                        }
                                    }
                                    state = "导出:" + outFile;
                                    number++;
                                    //写入数据
                                    File.WriteAllBytes(outFile, bytes);
                                    //旋转图片
                                    if (rotated && frame.rotated)
                                    {
                                        RotateImage(outFile);
                                    }
                                });
                            });
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                        }
                    }
                });

            });
        }
    }


    /**根据路径读取为Texture2D**/
    public static Texture2D GetTexrture2DFromPath(string imgPath)
    {
        //读取文件
        FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
        int byteLength = (int)fs.Length;
        byte[] imgBytes = new byte[byteLength];
        fs.Read(imgBytes, 0, byteLength);
        fs.Close();
        fs.Dispose();
        //转化为Texture2D
        var img = System.Drawing.Image.FromStream(new MemoryStream(imgBytes));
        Texture2D t2d = new Texture2D(img.Width, img.Height);
        img.Dispose();
        t2d.LoadImage(imgBytes);
        t2d.Apply();

        return t2d;
    }

    /*将图片旋转*/
    public static void RotateImage(string imgPath)
    {
        //读取文件
        FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
        int byteLength = (int)fs.Length;
        byte[] imgBytes = new byte[byteLength];
        fs.Read(imgBytes, 0, byteLength);
        fs.Close();
        fs.Dispose();
        //转化为Texture2D
        var img = System.Drawing.Image.FromStream(new MemoryStream(imgBytes));
        img.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);
        img.Save(imgPath);
        img.Dispose();
    }
}
