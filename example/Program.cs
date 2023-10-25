string file = args.Length < 1 ? "m1" : args[0];

int deep = 2;
var datas = new Data();
const ulong u = 1;
System.Console.WriteLine((u << 27) + (u << 36));

Othello initial = new Othello(datas, 2, 2, file != "m1");
Node tree = new Node
{
    State = initial,
    YouPlays = file == "m1"
};

tree.Expand(deep);
if (tree.YouPlays)
{
    tree.AlphaBeta(float.NegativeInfinity, float.PositiveInfinity);
    tree = tree.PlayBest();
    tree.Expand(deep);
    
    var turn = tree.YouPlays == true ? 1 : 0;
    File.WriteAllText($"{file}.txt", $"{turn} {tree.State.data.White} {tree.State.data.Black} {tree.State.whiteCount} {tree.State.blackCount}");
}

while (true)
{
    Thread.Sleep(1000);

    if (!File.Exists($"{file} last.txt"))
        continue;
    
    var text = File.ReadAllText($"{file} last.txt");
    File.Delete($"{file} last.txt");

    var data = text.Split(" ");
    var white = ulong.Parse(data[1]);
    var black = ulong.Parse(data[2]);

    tree = tree.Play(white, black);
    tree.Expand(deep);

    tree.AlphaBeta(float.NegativeInfinity, float.PositiveInfinity);
    tree = tree.PlayBest();
    tree.Expand(deep);
    
    var turn = tree.YouPlays == true ? 1 : 0;

    File.WriteAllText($"{file}.txt", $"{turn} {tree.State.data.White} {tree.State.data.Black} {tree.State.whiteCount} {tree.State.blackCount}");
}