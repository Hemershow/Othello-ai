public struct Data
{
    private const ulong u = 1;
    public ulong White { get; } = (u << 27) + (u << 36);
    public ulong Black { get; } = (u << 28) + (u << 35);

    public Data(ulong white, ulong black)
    {
        White = white;
        Black = black;
    }
}
public class Othello
{
    // private const ulong u = 1;
    protected readonly int[] surrounding = new int[8] { 1, -1, 8, -8, 9, -9, 7, -7 };
    Data data = new Data();
    int whiteCount = 0;
    int blackCount = 0;
    bool myTurn = true;
    bool white = false;
    public Othello(Data data, int white, int black)
    {
        this.data = data;
        whiteCount = white;
        blackCount = black;
    }
    public void Play(int position)
    {

    }
    public bool MoveTakesPiece(int currentPosition, int lineMovement)
    {
        var enemy = (data.White >>> (63 - currentPosition) & 1) == 1 ? data.Black : data.White;
        var updatedMovement = lineMovement;
        var positionIsEnemy = (enemy >>> (63 - updatedMovement) & 1) == 1;

        while (positionIsEnemy && IsInMap(63 - updatedMovement - currentPosition, currentPosition))
        {
            updatedMovement += lineMovement;
            positionIsEnemy = (enemy >>> (63 - updatedMovement) & 1) == 1;

            if (!positionIsEnemy)
                return true;
        }

        return false;
    }
    public bool IsInMap(int move, int oldPosition)
    {

    }
    public bool CanPlay(int position)
    {
        var pieces = white ? data.White : data.Black;

        if (
            !((pieces >>> (63 - position + 1) & 1) == 1 ||
            (pieces >>> (63 - position - 1) & 1) == 1 ||
            (pieces >>> (63 - position + 8) & 1) == 1 ||
            (pieces >>> (63 - position - 8) & 1) == 1 ||
            (pieces >>> (63 - position + 7) & 1) == 1 ||
            (pieces >>> (63 - position - 7) & 1) == 1 ||
            (pieces >>> (63 - position + 9) & 1) == 1 ||
            (pieces >>> (63 - position - 9) & 1) == 1)
        )
            return false;

        for (int i = 0; i < surrounding.Length; i++)
        {
            if (MoveTakesPiece(position, surrounding[i]))
                return true;
        }

        return false;
    }
    // public bool GameEnded()
    // {
    // }
    public Othello Clone()
    {
        Othello copy = new Othello(data, whiteCount, blackCount);
        return copy;
    }
    public IEnumerable<Othello> Next()
    {
        var clone = Clone();

        var state = data.White + data.Black;

        for (int i = 63; i >= 0; i--)
        {
            var freePosition = ((state >>> i) & 1) != 1;

            if (!freePosition)
                continue;

            if (!CanPlay(i))
                continue;

            clone.Play(i);
            yield return clone;
            clone = Clone();
        }
    }
}