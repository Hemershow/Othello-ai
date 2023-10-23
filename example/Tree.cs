public class Node
{
    public Notakto State { get; set; }
    public float Avaliation { get; set; } = 0;
    public List<Node> Children { get; set; } = new();
    public bool Expanded { get; set; } = false;
    public bool YouPlays { get; set; } = true;

    public Node Play(int board, int position)
    {
        foreach (var child in Children)
        {
            var last = child.State.GetLast();
            if (last.board == board && last.position == position)
                return child;
        }
        return null;
    }

    public void Expand(int deep)
    {
        if (deep == 0)
            return;
        
        if (!this.Expanded)
        {
            var nexts = this.State.Next();
            foreach (var next in nexts)
                this.Children.Add(new Node()
                {
                    State = next,
                    YouPlays = !YouPlays
                });
            this.Expanded = true;
        }

        foreach (var child in this.Children)
            child.Expand(deep - 1);
    }

    public Node PlayBest()
    {
        return Children.MaxBy(n => 
            YouPlays ? n.Avaliation : -n.Avaliation
        );
    }

    public float MiniMax()
    {
        if (this.Children.Count == 0)
        {
            this.Avaliation = aval();
            return this.Avaliation;
        }

        if (YouPlays)
        {
            var value = float.NegativeInfinity;
            foreach (var child in Children)
            {
                var avaliation = child.MiniMax();
                if (avaliation > value)
                    value = avaliation;
            }
            this.Avaliation = value;
            return this.Avaliation;
        }
        else
        {
            var value = float.PositiveInfinity;
            foreach (var child in Children)
            {
                var avaliation = child.MiniMax();
                if (avaliation < value)
                    value = avaliation;
            }
            this.Avaliation = value;
            return this.Avaliation;
        }
    }

    private float aval()
    {
        if (State.GameEnded())
            return YouPlays ? float.PositiveInfinity : float.NegativeInfinity;
        
        return Random.Shared.NextSingle();
    }
}