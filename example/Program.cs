string file = args.Length < 1 ? "m1" : args[0];
int boards = args.Length < 2 ? 2 : int.Parse(args[1]);

int deep = 6;

Notakto initial = new Notakto(boards);
Node tree = new Node
{
    State = initial,
    YouPlays = file == "m1"
};

tree.Expand(deep);

if (tree.YouPlays)
{
    tree.MiniMax();
    tree = tree.PlayBest();
    tree.Expand(deep);
    var last = tree.State.GetLast();
    
    File.WriteAllText($"{file}.txt", $"{last.board} {last.position}");
}

while (true)
{
    Thread.Sleep(1000);

    if (!File.Exists($"{file} last.txt"))
        continue;
    
    var text = File.ReadAllText($"{file} last.txt");
    File.Delete($"{file} last.txt");

    var data = text.Split(" ");
    var board = int.Parse(data[0]);
    var position = int.Parse(data[1]);

    tree = tree.Play(board, position);
    tree.Expand(deep);

    tree.MiniMax();
    tree = tree.PlayBest();
    tree.Expand(deep);

    var last = tree.State.GetLast();
    File.WriteAllText($"{file}.txt", $"{last.board} {last.position}");
}