using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using Pamella;

App.Open(new NotaktoView
{
    Model1 = args.Length > 0 ? args[0] : "m1.txt",
    Model2 = args.Length > 1 ? args[1] : "m2.txt"
});

public class Notakto
{
    public int TableCount { get; private set; }
    public List<bool[]> Tables { get; set; } = new();

    public Notakto(int tableCount)
    {
        this.TableCount = tableCount;
        for (int i = 0; i < tableCount; i++)
            this.Tables.Add(new bool[9]);
    }
}

public class NotaktoView : View
{
    public string Model1 { get; set; }
    public string Model2 { get; set; }

    const float margin = 25;
    Notakto notakto = null;
    bool model1Plays = true;

    protected override void OnStart(IGraphics g)
    {
        start(1);

        g.SubscribeKeyDownEvent(key =>
        {
            if (key == Input.Escape)
                App.Close();
            
            if (key == Input.D1)
            {
                start(1);
                Invalidate();
            }
            
            if (key == Input.D2)
            {
                start(2);
                Invalidate();
            }
            
            if (key == Input.D3)
            {
                start(3);
                Invalidate();
            }
            
            if (key == Input.D4)
            {
                start(4);
                Invalidate();
            }
        });
    }

    protected override void OnFrame(IGraphics g)
    {
        try
        {
            play(
                model1Plays ? Model1 : Model2,
                model1Plays ? Model2 : Model1
            );
        }
        catch { }
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.Black);
        if (notakto is null)
            return;
        
        g.DrawText(new RectangleF(0, 0, 100, 100), 
            model1Plays ? Brushes.Blue : Brushes.Green,
            model1Plays ? Model1 : Model2
        );
        
        var layoutData = calcLines(
            g.Width, g.Height, notakto.TableCount
        );
        var lines = layoutData.lines;
        var size = layoutData.size;
        
        int index = 0;
        float ySurplus = g.Height - (size + margin) * lines - margin;
        float y = ySurplus / 2;
        for (int i = 0; i < lines; i++)
        {
            int lineSize = notakto.TableCount / lines;
            float xSurplus = g.Width - (size + margin) * lineSize - margin;
            float x = xSurplus / 2;
            for (int j = 0; j < lineSize; j++)
            {
                draw(g, x, y, size, notakto.Tables[index]);
                index++;
                x += margin + size;
            }
            y += margin + size;
        }
    }

    private void draw(IGraphics g, float x, float y, float size, bool[] board)
    {
        bool ended = false;
        int[] count = new int[6];
        for (int i = 0; i < 9; i++)
        {
            if(board[i])
                count[i % 3]++;
                
            if(board[i])
                count[3 + i / 3]++;
        }
        foreach (var c in count)
            if (c == 3)
                ended = true;
            
        if (board[0] && board[4] && board[8])
            ended = true;

        if (board[2] && board[4] && board[6])
            ended = true;
        
        var color = ended ? Brushes.Red : Brushes.White;
        var subSize = (size - 10) / 3;
        g.FillRectangle(
            x + subSize, y + 5,
            5, size - 10,
            color
        );
        g.FillRectangle(
            x + 2 * subSize + 5, y + 5,
            5, size - 10,
            color
        );
        g.FillRectangle(
            x + 5, y + subSize,
            size - 10, 5,
            color
        );
        g.FillRectangle(
            x + 5, y + 2 * subSize + 5,
            size - 10, 5,
            color
        );

        float len = 15;
        float disloc = subSize + 5;
        for (int i = 0; i < 3; i++)
        {
            var dx = i * disloc;
            for (int j = 0; j < 3; j++)
            {
                var hasX = board[i + 3 * j];
                if (!hasX)
                    continue;
                
                var dy = j * disloc;
                
                g.FillPolygon(
                    new PointF[] {
                        new PointF(dx + x + 6, dy + y + 14),
                        new PointF(dx + x + 14, dy + y + 6),
                        new PointF(dx + x + subSize / 2, dy + y + (subSize - len) / 2),
                        
                        new PointF(dx + x + subSize - 14, dy + y + 6),
                        new PointF(dx + x + subSize - 6, dy + y + 14),
                        new PointF(dx + x + (subSize + len) / 2, dy + y + subSize / 2),

                        new PointF(dx + x + subSize - 6, dy + y + subSize - 14),
                        new PointF(dx + x + subSize - 14, dy + y + subSize - 6),
                        new PointF(dx + x + subSize / 2, dy + y + (subSize + len) / 2),

                        new PointF(dx + x + 14, dy + y + subSize - 6),
                        new PointF(dx + x + 6, dy + y + subSize - 14),
                        new PointF(dx + x + (subSize - len) / 2, dy + y + subSize / 2),
                    }, color
                );
            }
        }
    }

    private (int lines, float size) calcLines(float wid, float hei, int board)
    {
        var bestSize = float.MinValue;
        for (int lineCount = 1; lineCount < board; lineCount++)
        {
            int lineSize = board / lineCount;

            var boardWid = (wid - margin * (lineSize + 1)) / lineSize;
            var boardHei = (hei - margin * (lineCount + 1)) / lineCount;
            var boardSize = 
                boardWid < boardHei ? 
                boardWid : boardHei;
            
            if (boardSize > bestSize)
                bestSize = boardSize;
            else return (lineCount - 1, bestSize);
        }
        return (board - 1, bestSize);
    }

    private bool play(string file, string other)
    {
        if (!File.Exists(file))
            return false;
        
        var content = File.ReadAllText(file);
        var data = content
            .Split(' ')
            .Select(int.Parse)
            .ToArray();
        
        notakto.Tables[data[0]][data[1]] = true;
        File.Delete(file);
        File.WriteAllText(other.Replace(".txt", " last.txt"), content);
        model1Plays = !model1Plays;
        Invalidate();
        return true;
    }

    private void start(int level)
    {
        notakto = level switch
        {
            1 => new Notakto(2),
            2 => new Notakto(6),
            3 => new Notakto(24),
            4 => new Notakto(72),
            _ => null
        };
    }
}