public class Node
{
    public Othello State { get; set; }
    public float Avaliation { get; set; } = 0;
    public List<Node> Children { get; set; } = new();
    public bool Expanded { get; set; } = false;
    public bool YouPlays { get; set; } = true;
    public Node Play(ulong white, ulong black)
    {
        foreach (var child in Children)
        {
            if (
                child.State.data.White == white &&
                child.State.data.Black == black
            )
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
                    YouPlays = !YouPlays,
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

    public float AlphaBeta(float alpha, float beta)
    {
        if (Children.Count == 0)
        {
            Avaliation = aval();
            return Avaliation;
        }

        if (YouPlays)
        {
            var value = float.NegativeInfinity;
            foreach (var child in Children)
            {
                var avaliation = child.AlphaBeta(alpha, beta);
                if (avaliation > value)
                    value = avaliation;
                alpha = alpha > value ? alpha : value;
            }
            Avaliation = value;
            return Avaliation;
        }
        else
        {
            var value = float.PositiveInfinity;
            foreach (var child in Children)
            {
                var avaliation = child.AlphaBeta(alpha, beta);
                if (avaliation < value)
                    value = avaliation;
                beta = beta < value ? beta : value;
            }
            Avaliation = value;
            return Avaliation;
        }
    }

    private float aval()
    {
        if (State.GameEnded())
            return YouPlays ? float.PositiveInfinity : float.NegativeInfinity;
        
        return Random.Shared.NextSingle();
    }
}