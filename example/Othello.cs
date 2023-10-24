using System.Numerics;

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
    private const ulong u = 1;
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
        // muda atual
        foreach(var s in surrounding){
            var moveChange = MoveAndChange(s, position, white);

            data = new Data(moveChange.Item1, moveChange.Item2);
            whiteCount = moveChange.Item3;
            whiteCount = moveChange.Item4;
        }
    }
    public (ulong, ulong, int, int) MoveAndChange(int direction, int currentPosition, bool white)
    {
        var (enemy, friend) = (data.White >>> (63 - currentPosition) & 1) == 1 ? (data.Black, data.White) : (data.White, data.Black);
        var updatedPosition = currentPosition + direction;
        var positionIsEnemy = (enemy >>> (63 - updatedPosition) & 1) == 1;
        var positionIsFriend = (friend >>> (63 - updatedPosition) & 1) == 1;
        ulong newEnemy = 0;
        ulong newMe = 0;
        int enemyCount = 0;
        int myCount = 0;

        while (positionIsEnemy && IsInArea(direction, updatedPosition, currentPosition))
        {
            newEnemy += u << updatedPosition;
            newMe += u << updatedPosition;

            if (!positionIsEnemy && positionIsFriend)
                var newData = new Data(
                    this.white ? data.White + newMe : data.White - newEnemy,
                    this.white ? data.Black - newEnemy : data.Black - newMe
                    )
                // inimigo = data.black - newEnemy
                // eu = data.white + newMe
                return 

            updatedPosition += direction;
            positionIsEnemy = (enemy >>> (63 - updatedPosition) & 1) == 1;
            positionIsFriend = (friend >>> (63 - updatedPosition) & 1) == 1;
        }

        return (data.White, data.Black, 0, 0);
    }
    public bool MoveTakesPiece(int currentPosition, int lineMovement)
    {
        var enemy = (data.White >>> (63 - currentPosition) & 1) == 1 ? data.Black : data.White;
        var updatedMovement = lineMovement + currentPosition;
        var positionIsEnemy = (enemy >>> (63 - updatedMovement) & 1) == 1;

        while (positionIsEnemy && IsInArea(lineMovement, updatedMovement, currentPosition))
        {
            updatedMovement += lineMovement;
            positionIsEnemy = (enemy >>> (63 - updatedMovement) & 1) == 1;

            if (!positionIsEnemy && positionIsFriend)
                return true;
        }

        return false;
    }
    public bool IsInArea(int movement, int currentPosition, int originalPosition)
    {
        var line = originalPosition / 8;
        var column = originalPosition % 8;
        var right = movement == -7 || movement == 9;

        if (currentPosition > 63 || currentPosition < 0)
            return false;

        switch (Math.Abs(movement))
        {
            case 1:
                return currentPosition / 8 == line;
                
            case 8:
                return currentPosition % 8 == column;

            case 9:
                if (right)
                    return currentPosition % 8 > column && currentPosition % 9 == originalPosition % 9;
                return currentPosition % 8 < column && currentPosition % 9 == originalPosition % 9;

            case 7:
                if (right)
                    return currentPosition % 8 > column && currentPosition % 7 == originalPosition % 7;
                return currentPosition % 8 < column && currentPosition % 7 == originalPosition % 7;

            default:
                return false;
        }
    }
    public bool CanPlay(int position)
    {
        var pieces = white ? data.White : data.Black;

        var hasAdjacent = false;

        for (int i = 0; i < surrounding.Length; i++)
        {
            var newPosition = position + surrounding[i];

            if(newPosition < 0 && newPosition > 63)
                continue;

            if (
                position % 8 == 7 && (surrounding[i] == 1 || surrounding[i] == -7 || surrounding[i] == 9) ||
                position % 8 == 0 && (surrounding[i] == -1 || surrounding[i] == 7 || surrounding[i] == -9)
            )
                continue;

            if (((pieces >>> newPosition) & 1) == 1){
                hasAdjacent = true;
            }
        }

        if(!hasAdjacent) return false;

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

        var state = data.White | data.Black;

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