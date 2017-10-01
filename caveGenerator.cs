using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Runtime.InteropServices;

class Program {
    static void Main(string[] args) {

        int mapWidth, mapHeight, fillPercent;

        if (args.Length < 3) {
            Console.WriteLine("Usage: *.exe <width> <height> <fill%>");
            return;
        } else {
            mapWidth = Int32.Parse(args[0]);
            mapHeight = Int32.Parse(args[1]);
            fillPercent = Int32.Parse(args[2]);
        }

        CaveGenerator caveGenerator = new CaveGenerator();
        Renderer renderer = new Renderer();
        ImageRenderer imageRenderer = new ImageRenderer();

        int[] playerPos = {0, 0};

        byte[,] mapContent = new byte[mapWidth, mapHeight];
        bool[,] map = new bool[mapWidth, mapHeight];

        caveGenerator.GenerateCave(ref map, ref mapContent, 2, fillPercent);
        // caveGenerator.SpawnPlayer(ref map, ref mapContent, ref playerPos);

        // renderer.DrawMap(ref map, ref mapContent);
        Console.WriteLine("Rendering image...");
        imageRenderer.DrawMap(ref mapContent);

        Console.WriteLine("Done.");
        return;
    }
}

class CaveGenerator {

    Random rnd = new Random();
    int mapHeight, mapWidth;

    public void GenerateCave(ref bool[,] _map, ref byte[,] _mapContent, int _iterations = 2, int _fill = 50) {

        mapWidth = _map.GetLength(0);
        mapHeight = _map.GetLength(1);

        Console.WriteLine("Filling map with random blocks...");

        // Fill map with random blocks
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                if (x == 0 || x == mapWidth-1 || y == 0 || y == mapHeight-1) {
                    _map[x, y] = true;
                    _mapContent[x, y] = 1;
                } else {
                    _map[x, y] = rnd.Next(1, 100) <= _fill;
                }
            }
        }

        Console.WriteLine("Generating caves...");

        // Iterate trough array and perform cellular automata filtering for given times
        for (int i = 0; i < _iterations; i++) {
            for (int y = 1; y < mapHeight-1; y++) {
                for (int x = 1; x < mapWidth-1; x++) {
                    // If block is filled
                    if (_map[x, y]) {
                        if (getNeighborStates(ref _map, x, y) >= 4) {
                            _map[x, y] = true;
                            _mapContent[x, y] = 1;
                        } else {
                            _map[x, y] = false;
                        }
                    }
                    // If block is empty
                    else {
                        if (getNeighborStates(ref _map, x, y) >= 5) {
                            _map[x, y] = true;
                            _mapContent[x, y] = 1;
                        }
                        else {
                            _map[x, y] = false;
                        }
                    }
                }
            }
        }

        Console.WriteLine("Generating tin ore...");
        GenerateOres(ref _map, ref _mapContent, 2, 20);

        Console.WriteLine("Generating copper ore...");
        GenerateOres(ref _map, ref _mapContent, 3, 20);

        Console.WriteLine("Generating gold ore...");
        GenerateOres(ref _map, ref _mapContent, 4, 5);

        Console.WriteLine("Generating diamonds...");
        GenerateOres(ref _map, ref _mapContent, 5, 1);

        Console.WriteLine("Generating bedrock...");
        GenerateBedRock(ref _map, ref _mapContent);
    }

    void GenerateBedRock(ref bool[,] _map, ref byte[,] _mapContent) {
        int width = _map.GetLength(0);
        int height = _map.GetLength(1);

        for (int y = height-1; y > height - 5; y--) {
            for (int x = 0; x < width; x++) {
                if (y < height - 2) {
                    if (rnd.Next(0, 2) == 1) {
                        _map[x, y] = false;
                        _mapContent[x, y] = 6;
                    }
                } else {
                    _map[x, y] = false;
                    _mapContent[x, y] = 6;
                }
            }
        }
    }

    void GenerateOres(ref bool[,] _map, ref byte[,] _mapContent, byte _blockId, int _fill = 0) {
        for (int y = 1; y < mapHeight-1; y++) {
            for (int x = 1; x < mapWidth-1; x++) {
                if (_map[x, y] && rnd.Next(1, 1000) <= _fill) {
                    _mapContent[x, y] = _blockId;

                    if (rnd.Next(1, 100) <= 50) {
                        _mapContent[x-1, y-1] = _blockId;
                        _map[x-1, y-1] = true;
                    }
                    if (rnd.Next(1, 100) <= 50) {
                        _mapContent[x-1, y] = _blockId;
                        _map[x-1, y] = true;
                    }
                    if (rnd.Next(1, 100) <= 50) {
                        _mapContent[x-1, y+1] = _blockId;
                        _map[x-1, y+1] = true;
                    }

                    if (rnd.Next(1, 100) <= 50) {
                        _mapContent[x, y-1] = _blockId;
                        _map[x, y-1] = true;
                    }
                    if (rnd.Next(1, 100) <= 50) {
                        _mapContent[x, y+1] = _blockId;
                        _map[x, y+1] = true;
                    }

                    if (rnd.Next(1, 100) <= 50) {
                        _mapContent[x+1, y-1] = _blockId;
                        _map[x+1, y-1] = true;
                    }
                    if (rnd.Next(1, 100) <= 50) {
                        _mapContent[x+1, y] = _blockId;
                        _map[x+1, y] = true;
                    }
                    if (rnd.Next(1, 100) <= 50) {
                        _mapContent[x+1, y+1] = _blockId;
                        _map[x+1, y+1] = true;
                    }
                }
            }
        }
    }

    public void SpawnPlayer(ref bool[,] _map, ref byte[,] _mapContent, ref int[] _playerPos) {
        int x, y;
        while(true) {
            x = rnd.Next(1, mapWidth-1);
            y = rnd.Next(1, mapHeight-1);
            if (!_map[x, y]) {
                _playerPos = new int[] {x, y};
                _mapContent[x, y] = 3;
                return;
            }
        }
    }

    // Get the amount of surrounding blocks
    int getNeighborStates(ref bool[,] _map, int x, int y) {
        int neighbors = 0;

        if (_map[x-1, y-1]) neighbors++;
        if (_map[x-1, y]) neighbors++;
        if (_map[x-1, y+1]) neighbors++;
        
        if (_map[x, y-1]) neighbors++;
        if (_map[x, y+1]) neighbors++;

        if (_map[x+1, y-1]) neighbors++;
        if (_map[x+1, y]) neighbors++;
        if (_map[x+1, y+1]) neighbors++;

        // Console.Write(neighbors);
        return neighbors;
    }
}

class Renderer {

    // Print out array
    public void DrawMap(ref bool[,] _map, ref byte[,] _mapContent) {
        Console.Clear();

        int width = _map.GetLength(0), height = _map.GetLength(1);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                switch(_mapContent[x, y]) {
                    case 3:
                        Console.Write("◄ ");
                        break;
                    case 2:
                        Console.Write("░░");
                        break;
                    case 1:
                        Console.Write("██");
                        break;
                    default:
                        Console.Write("  ");
                        break;
                }
            }
            Console.WriteLine();
        }
    }
}

class ImageRenderer {
    public void DrawMap(ref byte[,] _mapContent) {
        int width = _mapContent.GetLength(0);
        int height = _mapContent.GetLength(1);
        int channels = 3;

        byte[] imageData = new byte[width*height*channels];

        int i = 0;
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                switch(_mapContent[x, y]) {
                    case 1: // Stone
                        imageData[i] = 100;
                        imageData[i+1] = 100;
                        imageData[i+2] = 100;
                        break;
                    case 2: // Tin
                        imageData[i] = 190;
                        imageData[i+1] = 190;
                        imageData[i+2] = 190;
                        break;
                    case 3: // Copper
                        imageData[i] = 57;
                        imageData[i+1] = 111;
                        imageData[i+2] = 150;
                        break;
                    case 4: // Gold
                        imageData[i] = 87;
                        imageData[i+1] = 231;
                        imageData[i+2] = 242;
                        break;
                    case 5: // Diamond
                        imageData[i] = 255;
                        imageData[i+1] = 219;
                        imageData[i+2] = 91;
                        break;
                    case 6: // Bedrock
                        imageData[i] = 30;
                        imageData[i+1] = 20;
                        imageData[i+2] = 20;
                        break;
                    default: // Blank
                        imageData[i] = 50;
                        imageData[i+1] = 50;
                        imageData[i+2] = 50;
                        break;
                }
                i += 3;
            }
        }

        Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        BitmapData bmData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        IntPtr pNative = bmData.Scan0;
        Marshal.Copy(imageData, 0, pNative, width*height*channels);
        bitmap.UnlockBits(bmData);

        bitmap.Save("map.png", ImageFormat.Png);
    }
}