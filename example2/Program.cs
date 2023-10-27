string file = args.Length < 1 ? "m1" : args[0];

int deep = 2;
var datas = new Data();

Othello initial = new Othello(datas, 2, 2, true);
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
    File.WriteAllText($"../front/{file}.txt", $"{turn} {tree.State.data.White} {tree.State.whiteCount} {tree.State.data.Black} {tree.State.blackCount}");
}


while (true)
{
    Thread.Sleep(1000);

    if (!File.Exists($"../front/[OUTPUT]{file}.txt"))
        continue;
    Thread.Sleep(250);

    var text = File.ReadAllText($"../front/[OUTPUT]{file}.txt");
    File.Delete($"../front/[OUTPUT]{file}.txt");

    var data = text.Split(" ");
    var white = ulong.Parse(data[1]);
    var black = ulong.Parse(data[3]);

    tree = tree.Play(white, black);
    tree.Expand(deep);

    tree.AlphaBeta(float.NegativeInfinity, float.PositiveInfinity);
    tree = tree.PlayBest();
    tree.Expand(deep);

    var turn = tree.YouPlays == true ? 1 : 0;

    File.WriteAllText($"../front/{file}.txt", $"{turn} {tree.State.data.White} {tree.State.whiteCount} {tree.State.data.Black} {tree.State.blackCount}");
}